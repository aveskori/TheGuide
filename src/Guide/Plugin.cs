using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

using SlugBase.DataTypes;
using Guide.WorldChanges;
using Guide.Creatures;
using Guide.Objects;
using Guide.Guide;
using static GuideStatusClass;
using Guide.Medium;



namespace GuideSlugBase
{
    [BepInDependency("slime-cubed.slugbase")]
    
    [BepInPlugin(MOD_ID, "Guide", "0.4.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "aveskori.guide";
        

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            
            /*
            //Fisobs Content
            // Critobs
            Content.Register(new VanLizCritob());
            VanHooks.Hooks();
            Content.Register(new ChrLizCritob());
            CherryHooks.Hooks();
            //Content.Register(new molemousecritob());
            
            
            // Fisobs
            //Content.Register(new CloversFisobs());
            Content.Register(new HazerSacFisobs());
            HazerSac.Hooks();
            Content.Register(new LSpearFisobs());
            Content.Register(new SCloverFisobs());
            Content.Register(new CentiShellFisobs());
            */
            // Guide Hooks (I should REALLY move some of these to their own class :sob:)
            On.Player.Update += Player_Update;
            On.Creature.Grasp.ctor += Grasp_ctor;
            PebblesConversationOverride.Hooks();
            On.JellyFish.Collide += JellyFish_Collide;  //Guide immunity to jellyfish stuns
            On.JellyFish.Update += JellyFish_Update; //AND JELLYFISH TICKLES...
            On.Centipede.Shock += Centipede_Shock; // if slippery, immune to centishocks
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut; //HUD HINTS
            //On.RegionGate.customKarmaGateRequirements += GuideGateFix;
            //GuideCrafts.Hooks();
            On.Player.GrabUpdate += BubbleFruitPop; //slippery ability causes bubblefruit to pop
            On.Player.LungUpdate += Player_LungUpdate; //Infinite capacity lungs, no more panic swim

            //Medium Hooks (these are going to be in separate classes)
            MediumAbilities.Hooks();


            // Custom Hooks -- Scavenger AI
            ScavBehaviorTweaks.Hooks();
            

            //-- Stops the game from lagging when devtools is enabled and there's scavs in the world
            IL.DenFinder.TryAssigningDen += DenFinder_TryAssigningDen;
        }

        private void Player_LungUpdate(On.Player.orig_LungUpdate orig, Player self)
        {
            if(self.IsGuide(out var guide) || self.IsMedium(out var medium))
            {
               //basically makes infinite air and stops player from panicking while swimming  
                if(self.airInLungs < 1f)
                {
                    self.airInLungs = 1f;
                }
            }

        }

        private bool RotundWorld; //are we rotund??
        private bool _postModsInit;
        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            if (_postModsInit) return;

            try
            {
                _postModsInit = true;
                if (ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus"))
                {
                    RotundWorld = true;
                    Logger.LogInfo("We gettin ROTUND (also HIII person reading these logs!!!!)");
                }
                else
                {
                    RotundWorld = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void BubbleFruitPop(On.Player.orig_GrabUpdate orig, Player self, bool eu) //certified nut sweller
        {
            orig(self, eu);
            
            if (self.GetCat().IsGuide && self.GetCat().slippery
                && ScavBehaviorTweaks.FindNearbyGuide(self.room) != null)  //Player is guide, guide is slippery, guide is not null
            {
                for(int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed is WaterNut) //grasps arent null, grasp is waternut
                    {

                        (self.grasps[i].grabbed as WaterNut).swellCounter--;
                        if ((self.grasps[i].grabbed as WaterNut).swellCounter < 1)
                        {
                            (self.grasps[i].grabbed as WaterNut).Swell();
                        }
                        return;
                    }
                    return;
                }
            }
            return;
        }
        //(Pocky-Raisin) Try looking into slugBase savedata, it's intended to be used like that
        //HHH DOESNT WORK YET
        //(Visiting FP with a LanternSpear ticks SpearKey to true, Guide gains access to LC)
        /*private void GuideGateFix(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
        {
            orig(self);
            GuideStatusClass.GuideStatus guide = null;
            if (ModManager.MSC && self.room.abstractRoom.name == "GATE_UW_LC" && guide.SpearKey)
            {
                int num;
                if (int.TryParse(self.karmaRequirements[0].value, out num))
                {
                    self.karmaRequirements[0] = RegionGate.GateRequirement.OneKarma;
                }
                int num2;
                if (int.TryParse(self.karmaRequirements[1].value, out num2))
                {
                    self.karmaRequirements[1] = RegionGate.GateRequirement.OneKarma;
                }
            }
            
        }*/

        private static bool IsInit;
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            try
            {
                if (IsInit) return;
                IsInit = true;
             
                On.PlayerGraphics.ctor += PlayerGraphics_ctor;
                On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
                On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
                On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
                On.PlayerGraphics.Update += PlayerGraphics_Update;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (self.IsGuide(out var guide))
            {
                self.gills?.Update();
                guide.topGills?.Update();
            }
        }

        private void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (!self.IsGuide(out var guide)) return;

            const int length = 4;
            const float wideness = 1.5f;
            const float roundness = 0.9f;
            var pup = self.player.playerState.isPup;
            
            self.tail = new TailSegment[length];
            for (var i = 0; i < length; i++)
            {
                var segRad = Mathf.Lerp(6f, 1f, Mathf.Pow((i + 1f) / length, wideness)) * (1f + Mathf.Sin(i / (float)length * (float)Math.PI) * roundness);
                self.tail[i] = new TailSegment(self, segRad, (i == 0 ? 4 : 7) * (pup ? 0.5f : 1f), i > 0 ? self.tail[i - 1] : null, 0.85f, 1f, i == 0 ? 1f : 0.5f, true);
            }

            

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);
            self.bodyParts = bp.ToArray();
        }

        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var isGuide = false;
            GuideStatusClass.GuideStatus guide = null;
            
            try
            {
                isGuide = self.IsGuide(out guide);
                if (isGuide)
                {
                    guide.SpritesReady = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            orig(self, sLeaser, rCam);
            if (!isGuide) return;
            
            //Add to array ~~~~~~~~~~~
            guide.BodySpotsSprite = sLeaser.sprites.Length;
            guide.HipsSpotsSprite = sLeaser.sprites.Length + 1;
            guide.LegsSpotsSprite = sLeaser.sprites.Length + 2;
            guide.FaceBlushSprite = sLeaser.sprites.Length + 3;

            guide.TasselSpriteA[0] = sLeaser.sprites.Length + 4;
            guide.TasselSpriteA[1] = sLeaser.sprites.Length + 5;
            guide.TasselSpriteA[2] = sLeaser.sprites.Length + 6;
            guide.TasselSpriteA[3] = sLeaser.sprites.Length + 7;
            guide.TasselSpriteA[4] = sLeaser.sprites.Length + 8;

            guide.TasselSpriteB[0] = sLeaser.sprites.Length + 9;
            guide.TasselSpriteB[1] = sLeaser.sprites.Length + 10;
            guide.TasselSpriteB[2] = sLeaser.sprites.Length + 11;
            guide.TasselSpriteB[3] = sLeaser.sprites.Length + 12;
            guide.TasselSpriteB[4] = sLeaser.sprites.Length + 13;

            guide.TailSpots[0] = sLeaser.sprites.Length + 14;
            guide.TailSpots[1] = sLeaser.sprites.Length + 15;
            guide.TailSpots[2] = sLeaser.sprites.Length + 16;
            guide.TailSpots[3] = sLeaser.sprites.Length + 17;
            guide.TailSpots[4] = sLeaser.sprites.Length + 18;

            guide.topGills = new UpperHavenGills(self, sLeaser.sprites.Length + 19);//fixed gills
            self.gills = new LowerHavenGills(self, guide.topGills.startSprite + guide.topGills.numberOfSprites);

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 19 + self.gills.numberOfSprites + guide.topGills.numberOfSprites); //Adds body spots to sprite array (5), add five more for the danglefruit sprite (5), add five more for inner tassel sprite (5), tail spot sprites (5)

            //~~~~~~~~~~~~~~~~~~~
            //Assign sprites ~~~~~~~~~~
            sLeaser.sprites[guide.BodySpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.HipsSpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.LegsSpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.FaceBlushSprite] = new FSprite("pixel");

            guide.topGills.InitiateSprites(sLeaser, rCam);
            self.gills.InitiateSprites(sLeaser, rCam);

            for(int i = 0; i < 5; i++)
            {
                sLeaser.sprites[guide.TasselSpriteA[i]] = new FSprite("DangleFruit0A"); //adds 0, 1, 2, 3, 4
                sLeaser.sprites[guide.TasselSpriteB[i]] = new FSprite("DangleFruit0B");
                sLeaser.sprites[guide.TailSpots[i]] = new FSprite("tinyStar");
            }           
            
            guide.SetupColors();

            guide.SpritesReady = true;
            self.AddToContainer(sLeaser, rCam, null);
        }

        private void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (!self.IsGuide(out var guide) || !guide.SpritesReady) return;

            newContatiner ??= rCam.ReturnFContainer("Midground");

            newContatiner.AddChild(sLeaser.sprites[guide.BodySpotsSprite]);
            sLeaser.sprites[guide.BodySpotsSprite].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
            
            newContatiner.AddChild(sLeaser.sprites[guide.HipsSpotsSprite]);
            sLeaser.sprites[guide.HipsSpotsSprite].MoveInFrontOfOtherNode(sLeaser.sprites[1]);
            
            newContatiner.AddChild(sLeaser.sprites[guide.LegsSpotsSprite]);
            sLeaser.sprites[guide.LegsSpotsSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            
            newContatiner.AddChild(sLeaser.sprites[guide.FaceBlushSprite]);
            sLeaser.sprites[guide.FaceBlushSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);

            for (int j = guide.topGills.startSprite; j < self.gills.startSprite + self.gills.numberOfSprites; j++)
            {
                newContatiner.AddChild(sLeaser.sprites[j]);
                sLeaser.sprites[j].MoveBehindOtherNode(sLeaser.sprites[9]);
            }

            for(int i = 0; i < 5; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[guide.TasselSpriteA[i]]);
                sLeaser.sprites[guide.TasselSpriteA[i]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);

                newContatiner.AddChild(sLeaser.sprites[guide.TasselSpriteB[i]]);
                sLeaser.sprites[guide.TasselSpriteB[i]].MoveInFrontOfOtherNode(sLeaser.sprites[3]);

                newContatiner.AddChild(sLeaser.sprites[guide.TailSpots[i]]);
                sLeaser.sprites[guide.TailSpots[i]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            }           

        }

        private const string SpritePrefix = "GuideSprites_";//What's this?
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            

            if (!self.IsGuide(out var guide)) return;

            sLeaser.sprites[1].scaleX += 0.15f;
            for(int j = 0; j < 5; j++)
            {
                sLeaser.sprites[guide.TasselSpriteA[j]].scale = 0.5f;
                sLeaser.sprites[guide.TasselSpriteB[j]].scale = 0.5f;
            }
            
            //set color
            sLeaser.sprites[guide.BodySpotsSprite].Follow(sLeaser.sprites[0]);
            sLeaser.sprites[guide.BodySpotsSprite].color = guide.SpotsColor;
            
            sLeaser.sprites[guide.HipsSpotsSprite].Follow(sLeaser.sprites[1]);
            sLeaser.sprites[guide.HipsSpotsSprite].color = guide.SpotsColor;
            
            sLeaser.sprites[guide.LegsSpotsSprite].Follow(sLeaser.sprites[4]);
            sLeaser.sprites[guide.LegsSpotsSprite].color = guide.SpotsColor;

            sLeaser.sprites[guide.FaceBlushSprite].Follow(sLeaser.sprites[9]);
            sLeaser.sprites[guide.FaceBlushSprite].color = guide.SpotsColor;

            self.gills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            guide.topGills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            self.gills?.SetGillColors(guide.BodyColor, guide.GillsColor);
            guide.topGills?.SetGillColors(guide.BodyColor, guide.GillsColor);

            self.gills?.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            guide.topGills?.ApplyPalette(sLeaser, rCam, rCam.currentPalette);



            for (int i = 0; i < 5; i++)
            {
                sLeaser.sprites[guide.TasselSpriteA[i]].color = guide.TasselAColor;
                sLeaser.sprites[guide.TasselSpriteB[i]].color = guide.TasselBColor;
                sLeaser.sprites[guide.TailSpots[i]].color = guide.SpotsColor;
            }
          

            //top row positions
            for(var i = 0; i < 3; i++)
            {
                var offset = Custom.PerpendicularVector(Custom.DirVec(self.tail[i].pos, self.tail[i+1].pos)) * (self.tail[i].rad * 0.8f);
                offset.y = Mathf.Abs(offset.y);
                

                sLeaser.sprites[guide.TasselSpriteA[i]].SetPosition((self.tail[i].pos + self.tail[i+1].pos) / 2 - camPos + offset);

                sLeaser.sprites[guide.TasselSpriteB[i]].SetPosition((self.tail[i].pos + self.tail[i+1].pos)/2 - camPos + offset);

               
                
            }
            

            //bottom row
            sLeaser.sprites[guide.TasselSpriteA[3]].SetPosition(self.tail[1].pos - camPos);
            sLeaser.sprites[guide.TasselSpriteA[4]].SetPosition(self.tail[2].pos - camPos);

            sLeaser.sprites[guide.TasselSpriteB[3]].SetPosition(self.tail[1].pos - camPos);
            sLeaser.sprites[guide.TasselSpriteB[4]].SetPosition(self.tail[2].pos - camPos);                    
            
            for(int l = 0; l < 4; l++)
            {
                sLeaser.sprites[guide.TailSpots[l]].SetPosition(Vector2.Lerp(sLeaser.sprites[guide.TasselSpriteA[l]].GetPosition(), sLeaser.sprites[guide.TasselSpriteA[l + 1]].GetPosition(), 0.5f));
            }                               
            

            //spots pos
            sLeaser.sprites[guide.BodySpotsSprite].element = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_BodyA");
            if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + "Spots_" + sLeaser.sprites[4].element.name, out var element))
            {
                sLeaser.sprites[guide.LegsSpotsSprite].element = element;
            }
            if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + "Blush_" + sLeaser.sprites[9].element.name, out element))
            {
                sLeaser.sprites[guide.FaceBlushSprite].element = element;
            }
            
            var hipsElement = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_HipsA");
            if (self.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                if (self.player.bodyChunks[1].vel.x < -3f)
                {
                    hipsElement = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_LeftHipsA");
                }
                else if (self.player.bodyChunks[1].vel.x > 3f)
                {
                    hipsElement = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_RightHipsA");
                }
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Crawl || self.player.bodyMode == Player.BodyModeIndex.CorridorClimb || self.player.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut)
            {
                var headPos = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                var legsPos = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                var bodyAngle = Custom.AimFromOneVectorToAnother(legsPos, headPos);

                if (bodyAngle < -30 && bodyAngle > -150)
                {
                    hipsElement = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_LeftHipsA");
                }
                else if (bodyAngle > 30 && bodyAngle < 150)
                {
                    hipsElement = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_RightHipsA");
                }
            }

            sLeaser.sprites[guide.HipsSpotsSprite].element = hipsElement;
            
            sLeaser.sprites[guide.LegsSpotsSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            
            foreach (var sprite in sLeaser.sprites)
            {
                if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + sprite.element.name, out var newElement))
                {
                    sprite.element = newElement;
                }
            }
        }


        private void DenFinder_TryAssigningDen(ILContext il)
        {
            //dont write to log if devtools enabled
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt<RainWorld>("get_ShowLogs"));

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((DenFinder self) => self.creature.creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Scavenger);
            cursor.Emit(OpCodes.And);
        }

        private void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
        {
            //check if player guide AND slippery, else run orig
            if(shockObj is Player && (shockObj as Player).slugcatStats.name.value == "Guide" && (shockObj as Player).GetCat().slippery)
            {
                self.room.PlaySound(SoundID.Centipede_Shock, 1f, 1.5f, 0.5f);
                self.LoseAllGrasps();
                self.room.PlaySound(SoundID.Slugcat_Bite_Centipede, 1f, 2f, 0.75f);
                return;
            }
            else
            {
                orig(self, shockObj);
            }
        }

        private void JellyFish_Collide(On.JellyFish.orig_Collide orig, JellyFish self, PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            if((otherObject as Player).slugcatStats?.name?.value == "Guide")
            {
                self.room.PlaySound(SoundID.Slugcat_Bite_Centipede, self.bodyChunks[0], false, 1.5f, 1.5f);
                return;
            }
            else
            {
                orig(self, otherObject, myChunk, otherChunk);
            }
        }

        private void JellyFish_Update(On.JellyFish.orig_Update orig, JellyFish self, bool eu)
        {
            orig(self, eu);
            //DON'T GRAB ONTO GUIDE
            for (int i = 0; i < self.tentacles.Length; i++)
            {
                if (self.latchOnToBodyChunks[i] != null && self.latchOnToBodyChunks[i].owner is Player player && player.slugcatStats?.name?.value == "Guide")
                {
                    self.latchOnToBodyChunks[i] = null; //LET GO
                }
            }
        }

        private void Grasp_ctor(On.Creature.Grasp.orig_ctor orig, Creature.Grasp self, Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
        {
            //check if player guide and slippery true, release grasp
            orig(self, grabber, grabbed, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
            if (grabbed is Player player && (grabbed as Player).slugcatStats.name.value == "Guide")
            {

                if (grabbed.grabbedBy.Count > 0 && player.GetCat().slippery == true)
                {
                    self.grabber.room.PlaySound(SoundID.Water_Nut_Swell, grabbed.bodyChunks[1], false, 1f, 2f, false);
                    grabber.ReleaseGrasp(graspUsed);
                    grabbed.AllGraspsLetGoOfThisObject(true);
                    player.GetCat().slipperyTime = 40 * 15;
                    player.GetCat().slippery = true;
                }

            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            bool sFlag = false;
            if (self.GetCat().IsGuide)
            {
                //when underwater, slippery = true, countdown starts
                if (self.animation == Player.AnimationIndex.DeepSwim)
                {
                    if (!sFlag)
                    {
                        self.room.PlaySound(SoundID.Red_Lizard_Spit_Hit_Player, 0f, 0.50f, 1f);
                        sFlag = true;
                    }
                    
                    if(self.FoodInStomach < self.slugcatStats.foodToHibernate) //if food in stomach is less than food to hibernate (Update = 40 tps, SlipperyTime = update rate * seconds
                    {
                        self.GetCat().slipperyTime = 40 * 30;
                    }
                    else
                    {
                        self.GetCat().slipperyTime = 40 * 60;
                    }
                    self.GetCat().slippery = true;
                    self.slugcatStats.runspeedFac = 1.5f;
                    self.slugcatStats.corridorClimbSpeedFac = 1.6f;
                    self.slugcatStats.poleClimbSpeedFac = 1.6f;
                    self.waterFriction = 0.99f;
                    self.buoyancy = 0.9f;
                    
                }
                //slippery countdown
                if (self.GetCat().slipperyTime > 0)
                {
                    self.GetCat().slipperyTime--;
                    if (!RotundWorld) // check rotund world
                    {
                        Vector2 pos = self.bodyChunks[1].pos + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), 9f + Mathf.Lerp(-2f, 2f, UnityEngine.Random.value));
                        self.room.AddObject(new WaterDrip(pos, new Vector2(0, 1), false));
                    }                   
                }
                else
                {
                    self.GetCat().slippery = false;
                    sFlag = false;
                    self.slugcatStats.runspeedFac = 1f;
                    self.slugcatStats.corridorClimbSpeedFac = 1f;
                    self.slugcatStats.poleClimbSpeedFac = 1f;
                }

            }


            if (self.GetCat().harvestCounter < 40 && self.input[0].pckp && self.input[0].y > 0)
            {
                self.GetCat().harvestCounter++;
            }
            else if (self.GetCat().harvestCounter > 0)
            {
                self.GetCat().harvestCounter--;
            }
            orig(self, eu);
        }

        //extra hunts for other guide features?
        bool shownWaterHint = false;
        bool shownJellyHint = false;
        bool shownCentiHint = false;

        //HUD HINT MESSAGE CHECK WHEN LEAVING A SHORTCUT
        private void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            orig(self, pos, newRoom, spitOutAllSticks);

            if (self.slugcatStats.name.value == "Guide" && !self.dead && self.room != null && self.abstractCreature.world.game.IsStorySession && self.room.game.cameras[0].hud != null)
            {
                if (!shownWaterHint && self.room.water)
                {
                    self.room.game.cameras[0].hud.textPrompt.AddMessage("Water is a friend", 20, 200, false, false);
                    self.room.game.cameras[0].hud.textPrompt.AddMessage("Submerging grants temporary buffs", 20, 200, false, false);
                    shownWaterHint = true;
                }
                
            }
        }

        

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadImage("atlases/icon_LanternSpear");
            Futile.atlasManager.LoadImage("atlases/icon_clover");
            Futile.atlasManager.LoadAtlas("guidesprites/body-spots");
            Futile.atlasManager.LoadAtlas("guidesprites/face-blush");
            Futile.atlasManager.LoadAtlas("guidesprites/head");
            Futile.atlasManager.LoadAtlas("guidesprites/head-gills");
            Futile.atlasManager.LoadAtlas("guidesprites/hips-spots");
            Futile.atlasManager.LoadAtlas("guidesprites/hipsleft-spots");
            Futile.atlasManager.LoadAtlas("guidesprites/hipsright-spots");
            Futile.atlasManager.LoadAtlas("guidesprites/legs-spots");
            Futile.atlasManager.LoadAtlas("guidesprites/tail");
        }
    }
}

public static class ScavSatusClass
{
    public class ScavStatus
    {
        public bool isBaby;
        public bool isWarden;
        public bool isCompanion;

        //public int age;

        public int kingMask;
        public int glyphMark;

        public ScavStatus(Scavenger scav)
        {

            //age = scav.room.world.game.GetStorySession.saveState.cycleNumber;

            UnityEngine.Random.seed = scav.abstractCreature.ID.RandomSeed;
            if (UnityEngine.Random.value < 0.2f && !scav.Elite && !scav.King)
            {
                this.isBaby = true;
            }
            if(!isBaby && UnityEngine.Random.value < 0.1f)
            {
                this.isWarden = true;
                
            }

            /*if(scav.abstractCreature.ID.number == 5144)
            {
                this.isCompanion = true;
            }*/
        }
    }

    private static readonly ConditionalWeakTable<Scavenger, ScavStatus> ScavCWT = new();
    public static ScavStatus GetScav(this Scavenger scav) => ScavCWT.GetValue(scav, _ => new(scav));

}

public static class CritStatusClass
{
    public class CritStatus
    {
        public bool isHarvested;
        public int harvestCount;

        public bool havenScav;

        public bool isMonster;
        public bool isInfant;
        

        public CritStatus(Creature crit)
        {

            /*UnityEngine.Random.seed = crit.abstractCreature.ID.RandomSeed;

            if (UnityEngine.Random.value < 0.2f)
            {
                this.isMonster = true;
            }
            if (!isMonster && UnityEngine.Random.value < 0.1f)
            {
                this.isInfant = true;

            }*/


        }

        
    }
    private static readonly ConditionalWeakTable<Creature, CritStatus> CritCWT = new();
    public static CritStatus GetCrit(this Creature crit) => CritCWT.GetValue(crit, _ => new(crit));


}

public static class GuideStatusClass
{
    public class GuideStatus
    {
        // Define your variables to store here!
        //state slippery and countdown
        public int slipperyTime;
        public bool slippery;
        public bool artiSpawn;
        public bool SpearKey;

        public bool IsHarvested;
        public int harvestCounter;

        public readonly bool IsGuide;
        public readonly Player player;

        public bool SpritesReady;
        public int BodySpotsSprite;
        public int HipsSpotsSprite;
        public int LegsSpotsSprite;
        //public int HeadGillsSprite;
        public UpperHavenGills topGills;
        public int FaceBlushSprite;
        public int[] TasselSpriteA = new int[5];
        public int[] TasselSpriteB = new int[5];
        public int[] TailSpots = new int[5];
        

        public Color BodyColor;
        public Color EyesColor;
        public Color GillsColor;
        public Color SpotsColor;
        public Color TasselAColor;
        public Color TasselBColor;
        

        public GuideStatus(Player player)
        {
            // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
            IsGuide = player.slugcatStats.name.value == "Guide";
            if (!IsGuide) return;

            this.player = player;
            artiSpawn = false;
            SpearKey = false;
            harvestCounter = 0;
            
        }

        public void SetupColors()
        {
            var pg = (PlayerGraphics)player.graphicsModule;

            BodyColor = new PlayerColor("Body").GetColor(pg) ?? Custom.hexToColor("e8f5ca");
            EyesColor = new PlayerColor("Eyes").GetColor(pg) ?? Custom.hexToColor("00271f");
            GillsColor = new PlayerColor("Gills").GetColor(pg) ?? Custom.hexToColor("26593c");
            SpotsColor = new PlayerColor("Spots").GetColor(pg) ?? Custom.hexToColor("60c0bb");
            TasselAColor = new PlayerColor("Tassels").GetColor(pg) ?? Custom.hexToColor("12a23e");
            TasselBColor = new Color(TasselAColor.r - 5, TasselAColor.g - 5, TasselAColor.b - 5, 0.5f);
            
            
        }
    }

    // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
    private static readonly ConditionalWeakTable<Player, GuideStatus> CWT = new();
    public static GuideStatus GetCat(this Player player) => CWT.GetValue(player, _ => new(player));
    public static bool IsGuide(this Player player, out GuideStatus guide) => (guide = player.GetCat()).IsGuide;
    public static bool IsGuide(this PlayerGraphics pg, out GuideStatus guide) => IsGuide(pg.player, out guide);


    public static void Follow(this FSprite sprite, FSprite originalSprite)
    {
        sprite.SetPosition(originalSprite.GetPosition());
        sprite.rotation = originalSprite.rotation;
        sprite.scaleX = originalSprite.scaleX;
        sprite.scaleY = originalSprite.scaleY;
        sprite.isVisible = originalSprite.isVisible;
        sprite.alpha = originalSprite.alpha;
        sprite.anchorX = originalSprite.anchorX;
        sprite.anchorY = originalSprite.anchorY;
    }
}

public static class MediumStatusClass
{
    public class MediumStatus
    {
        public readonly bool IsMedium;
        public readonly Player player;

        public bool spritesReady;
        //public int HeadGillsSprite;
        public int FaceBlushSprite;
        public int FaceEchoSprite;
        public int BodyEchoSprite;
        public int HipsEchoSprite;
        public int ArmEchoSprite;
        public int LegEchoSprite;

        public int[] HeadTentacleSprite = new int[3];
        public int[] TailTentacleSprite = new int[4];
        //might want to make some reusable Tentacle class tbh, trianglemeshes need a lot of variables

        public Color BodyColor;
        public Color EyesColor;
        public Color GillsColor;

        public MediumStatus(Player player)
        {
            IsMedium = player.slugcatStats.name.value == "Medium";
            this.player = player;
        }

        public void SetupColors()
        {
            var pg = (PlayerGraphics)player.graphicsModule;

            BodyColor = new PlayerColor("Body").GetColor(pg) ?? Custom.hexToColor("e8f5ca");
            EyesColor = new PlayerColor("Eyes").GetColor(pg) ?? Custom.hexToColor("00271f");
            GillsColor = new PlayerColor("Gills").GetColor(pg) ?? Custom.hexToColor("26593c");
            
        }

        

    }
    private static readonly ConditionalWeakTable<Player, MediumStatus> CWT = new();
    public static MediumStatus GetMed(this Player player) => CWT.GetValue(player, _ => new(player));
    public static bool IsMedium(this Player player, out MediumStatus medium) => (medium = player.GetMed()).IsMedium;
    public static bool IsMedium(this PlayerGraphics pg, out MediumStatus medium) => IsMedium(pg.player, out medium);

    
}

public class LowerHavenGills : PlayerGraphics.AxolotlGills
{
    public LowerHavenGills(PlayerGraphics pg, int start, bool med = false) : base(pg, start)
    {
        MediumGills = med;
        this.pGraphics = pg;
        this.startSprite = start;
        this.rigor = 0.5873646f;
        float num = 1.310689f;
        this.colored = true;
        this.graphic = MediumGills ? 5 : 4;
        this.graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + this.graphic.ToString()).sourcePixelSize.y;
        int num2 = MediumGills? 2 : 3;
        this.scalesPositions = new Vector2[num2 * 2];
        this.scaleObjects = new PlayerGraphics.AxolotlScale[this.scalesPositions.Length];
        this.backwardsFactors = new float[this.scalesPositions.Length];
        float num3 = 0.1542603f;
        float num4 = 0.1759363f;
        for (int i = 0; i < num2; i++)
        {
            float y = 0.03570603f;
            float num5 = 0.659981f;
            float num6 = 0.9722961f;
            float num7 = 0.3644831f;
            if (i == 1)
            {
                y = 0.02899241f;
                num5 = 0.76459f;
                num6 = 0.6056554f;
                num7 = 0.9129724f;
            }
            if (i == 2)
            {
                y = 0.02639332f;
                num5 = 0.7482835f;
                num6 = 0.7223744f;
                num7 = 0.4567381f;
            }
            for (int j = 0; j < 2; j++)
            {
                this.scalesPositions[i * 2 + j] = new Vector2((j != 0) ? num5 : (-num5), y);
                this.scaleObjects[i * 2 + j] = new PlayerGraphics.AxolotlScale(pGraphics);
                this.scaleObjects[i * 2 + j].length = Mathf.Lerp(2.5f, 15f, num * num6);
                this.scaleObjects[i * 2 + j].width = Mathf.Lerp(0.65f, 1.2f, num3 * num);
                this.backwardsFactors[i * 2 + j] = num4 * num7;
            }
        }
        this.numberOfSprites = ((!this.colored) ? this.scalesPositions.Length : (this.scalesPositions.Length * 2));
        this.spritesOverlap = PlayerGraphics.AxolotlGills.SpritesOverlap.InFront;
    }

    public new void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.pGraphics.owner == null)
        {
            return;
        }
        for (int i = this.startSprite + this.scalesPositions.Length - 1; i >= this.startSprite; i--)
        {
            Vector2 vector = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
            float f = 0f;
            float num = 0f;
            if (i < this.startSprite + this.scalesPositions.Length / 2)
            {
                vector.x -= 5f;
            }
            else
            {
                num = 180f;
                vector.x += 5f;
            }
            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(this.scaleObjects[i - this.startSprite].lastPos, this.scaleObjects[i - this.startSprite].pos, timeStacker)) + num;
            sLeaser.sprites[i].scaleX = this.scaleObjects[i - this.startSprite].width * Mathf.Sign(f);
            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length].x = vector.x - camPos.x;
                sLeaser.sprites[i + this.scalesPositions.Length].y = vector.y - camPos.y;
                sLeaser.sprites[i + this.scalesPositions.Length].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(this.scaleObjects[i - this.startSprite].lastPos, this.scaleObjects[i - this.startSprite].pos, timeStacker)) + num;
                sLeaser.sprites[i + this.scalesPositions.Length].scaleX = this.scaleObjects[i - this.startSprite].width * Mathf.Sign(f);
                if (i < this.startSprite + this.scalesPositions.Length / 2)
                {
                    sLeaser.sprites[i + this.scalesPositions.Length].scaleX *= -1f;
                }
                if (i >= this.startSprite + this.scalesPositions.Length / 2 && MediumGills)
                {
                    sLeaser.sprites[i].isVisible = false;
                }
            }
            if (i < this.startSprite + this.scalesPositions.Length / 2)
            {
                sLeaser.sprites[i].scaleX *= -1f;
            }
            if (i >= this.startSprite + this.scalesPositions.Length / 2 && MediumGills)
            {
                sLeaser.sprites[i].isVisible = false;
            }
        }
        for (int j = this.startSprite + this.scalesPositions.Length - 1; j >= this.startSprite; j--)
        {
            sLeaser.sprites[j].color = this.baseColor;
            if (this.colored)
            {
                sLeaser.sprites[j + this.scalesPositions.Length].color = Color.Lerp(this.effectColor, this.baseColor, this.pGraphics.malnourished / 1.75f);
            }
        }
    }

    public bool MediumGills;
}

public class UpperHavenGills
{
    public UpperHavenGills(PlayerGraphics pg, int start, bool med = false)
    {
        xOffset = 3f;
        yOffset = 5f;
        MediumGills = med;

        this.pGraphics = pg;
        this.startSprite = start;
        this.rigor = 0.5873646f;
        float num = 1.310689f;
        this.colored = true;
        this.graphic = MediumGills? 5 : 4;
        this.graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + this.graphic.ToString()).sourcePixelSize.y;
        int num2 = 1;
        this.scalesPositions = new Vector2[num2 * 2];
        this.scaleObjects = new PlayerGraphics.AxolotlScale[this.scalesPositions.Length];
        this.backwardsFactors = new float[this.scalesPositions.Length];
        float num3 = 0.1542603f;
        float num4 = 0.1759363f;
        for (int i = 0; i < num2; i++)
        {
            float x = 0.03570603f;
            float num5 = 0.659981f;
            float num6 = 0.9722961f;
            float num7 = 0.3644831f;

            for (int j = 0; j < 2; j++)
            {
                this.scalesPositions[i * 2 + j] = new Vector2((j != 0) ? x : (-x), num5);
                this.scaleObjects[i * 2 + j] = new PlayerGraphics.AxolotlScale(pGraphics)
                {
                    length = Mathf.Lerp(2.5f, 15f, num * num6),
                    width = Mathf.Lerp(0.65f, 1.2f, num3 * num)
                };
                this.backwardsFactors[i * 2 + j] = num4 * num7;
            }
        }
        this.numberOfSprites = ((!this.colored) ? this.scalesPositions.Length : (this.scalesPositions.Length * 2));
        this.spritesOverlap = SpritesOverlap.InFront;
    }

    public void Update()
    {
        for (int i = 0; i < this.scaleObjects.Length; i++)
        {
            Vector2 pos = this.pGraphics.owner.bodyChunks[0].pos;
            Vector2 pos2 = this.pGraphics.owner.bodyChunks[1].pos;
            float num = 0f;
            float num2 = 0f;
            int num3 = i % (this.scaleObjects.Length / 2);
            float num4 = num2 / (float)(this.scaleObjects.Length / 2);
            if (i >= this.scaleObjects.Length / 2)
            {
                num = 0f;
                pos.x += xOffset;
            }
            else
            {
                pos.x -= xOffset;
            }
            pos.y += yOffset;
            Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
            float f = Custom.VecToDeg(this.pGraphics.lookDirection);
            Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
            Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));
            if (this.scalesPositions[i].x < 0.2f)
            {
                a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, this.scalesPositions[i].x), 2f) * 2f;
            }
            a2 = Vector2.Lerp(a2, vector, Mathf.Pow(this.backwardsFactors[i], 1f)).normalized;
            Vector2 vector2 = pos + a2 * this.scaleObjects[i].length;
            if (!Custom.DistLess(this.scaleObjects[i].pos, vector2, this.scaleObjects[i].length / 2f))
            {
                Vector2 a3 = Custom.DirVec(this.scaleObjects[i].pos, vector2);
                float num5 = Vector2.Distance(this.scaleObjects[i].pos, vector2);
                float num6 = this.scaleObjects[i].length / 2f;
                this.scaleObjects[i].pos += a3 * (num5 - num6);
                this.scaleObjects[i].vel += a3 * (num5 - num6);
            }
            this.scaleObjects[i].vel += Vector2.ClampMagnitude(vector2 - this.scaleObjects[i].pos, 10f) / Mathf.Lerp(5f, 1.5f, this.rigor);
            this.scaleObjects[i].vel *= Mathf.Lerp(1f, 0.8f, this.rigor);
            this.scaleObjects[i].ConnectToPoint(pos, this.scaleObjects[i].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
            this.scaleObjects[i].Update();
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (this.pGraphics.owner == null)
        {
            return;
        }
        for (int i = this.startSprite + this.scalesPositions.Length - 1; i >= this.startSprite; i--)
        {
            Vector2 vector = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
            float f = 0f;
            float num = 45f;
            if (i < this.startSprite + this.scalesPositions.Length / 2)
            {
                vector.x -= xOffset;
            }
            else
            {
                //num = 180f;
                vector.x += xOffset;
            }
            vector.y += yOffset;

            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(this.scaleObjects[i - this.startSprite].lastPos, this.scaleObjects[i - this.startSprite].pos, timeStacker)) + num;
            sLeaser.sprites[i].scaleX = this.scaleObjects[i - this.startSprite].width * Mathf.Sign(f);

            if (i >= this.startSprite + this.scalesPositions.Length / 2 && MediumGills)
            {
                sLeaser.sprites[i].isVisible = false;
            }

            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length].x = vector.x - camPos.x;
                sLeaser.sprites[i + this.scalesPositions.Length].y = vector.y - camPos.y;
                sLeaser.sprites[i + this.scalesPositions.Length].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(this.scaleObjects[i - this.startSprite].lastPos, this.scaleObjects[i - this.startSprite].pos, timeStacker)) + num;
                sLeaser.sprites[i + this.scalesPositions.Length].scaleX = this.scaleObjects[i - this.startSprite].width * Mathf.Sign(f);
                if (i < this.startSprite + this.scalesPositions.Length / 2)
                {
                    sLeaser.sprites[i + this.scalesPositions.Length].scaleX *= -1f;
                }
                if (i >= this.startSprite + this.scalesPositions.Length / 2 && MediumGills)
                {
                    sLeaser.sprites[i].isVisible = false;
                }
            }
            if (i < this.startSprite + this.scalesPositions.Length / 2)
            {
                sLeaser.sprites[i].scaleX *= -1f;
            }
        }
        for (int j = this.startSprite + this.scalesPositions.Length - 1; j >= this.startSprite; j--)
        {
            sLeaser.sprites[j].color = this.baseColor;
            if (this.colored)
            {
                sLeaser.sprites[j + this.scalesPositions.Length].color = Color.Lerp(this.effectColor, this.baseColor, this.pGraphics.malnourished / 1.75f);
            }
        }
    }

    public void SetGillColors(Color baseCol, Color effectCol)
    {
        this.baseColor = baseCol;
        this.effectColor = effectCol;
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        this.palette = palette;
        for (int i = this.startSprite + this.scalesPositions.Length - 1; i >= this.startSprite; i--)
        {
            sLeaser.sprites[i].color = this.baseColor;
            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length].color = this.effectColor;
            }
        }
    }
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = this.startSprite + this.scalesPositions.Length - 1; i >= this.startSprite; i--)
        {
            sLeaser.sprites[i] = new FSprite("LizardScaleA" + this.graphic.ToString(), true);
            sLeaser.sprites[i].scaleY = this.scaleObjects[i - this.startSprite].length / this.graphicHeight;
            sLeaser.sprites[i].anchorY = 0.1f;
            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length] = new FSprite("LizardScaleB" + this.graphic.ToString(), true);
                sLeaser.sprites[i + this.scalesPositions.Length].scaleY = this.scaleObjects[i - this.startSprite].length / this.graphicHeight;
                sLeaser.sprites[i + this.scalesPositions.Length].anchorY = 0.1f;
            }
        }
    }

    public PlayerGraphics.AxolotlScale[] scaleObjects;

    public float[] backwardsFactors;

    public int graphic;

    public float graphicHeight;

    public float rigor;

    public float scaleX;

    public bool colored;

    public Vector2[] scalesPositions;

    public PlayerGraphics pGraphics;

    public int numberOfSprites;

    public int startSprite;

    public RoomPalette palette;

    public UpperHavenGills.SpritesOverlap spritesOverlap;

    public Color baseColor;

    public Color effectColor;

    public float xOffset;
    public float yOffset;

    public bool MediumGills;

    public class SpritesOverlap : ExtEnum<SpritesOverlap>
    {
        // Token: 0x06004691 RID: 18065 RVA: 0x004BA8D5 File Offset: 0x004B8AD5
        public SpritesOverlap(string value, bool register = false) : base(value, register)
        {
        }

        public static readonly SpritesOverlap Behind = new("Behind", true);
        public static readonly SpritesOverlap BehindHead = new("BehindHead", true);
        public static readonly SpritesOverlap InFront = new("InFront", true);
    }
}