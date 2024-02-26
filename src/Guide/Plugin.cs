using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using System.Runtime.CompilerServices;

using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

using SlugBase.DataTypes;
using Guide.WorldChanges;
using Guide.Creatures;
using Guide.Objects;
using Guide.Guide;


namespace GuideSlugBase
{
    [BepInDependency("slime-cubed.slugbase")]
    
    [BepInPlugin(MOD_ID, "Guide", "0.5.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "aveskori.guide";
        

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            
            // Critobs
            Content.Register(new VanLizCritob());
            VanHooks.Hooks();
            Content.Register(new ChrLizCritob());
            CherryHooks.Hooks();
            Content.Register(new molemousecritob());
            //Content.Register(new ScrufflingCritob());
            
            // Fisobs
            //Content.Register(new CloversFisobs());
            Content.Register(new HazerSacFisobs());
            HazerSac.Hooks();
            Content.Register(new LSpearFisobs());
            Content.Register(new SCloverFisobs());
            Content.Register(new CentiShellFisobs());


            // Slugcat Hooks
            On.Player.Update += Player_Update;
            On.Creature.Grasp.ctor += Grasp_ctor;
            PebblesConversationOverride.Hooks();
            On.JellyFish.Collide += JellyFish_Collide;  //Guide immunity to jellyfish stuns
            On.JellyFish.Update += JellyFish_Update; //AND JELLYFISH TICKLES...
            On.Centipede.Shock += Centipede_Shock; // if slippery, immune to centishocks
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut; //HUD HINTS
            //On.RegionGate.customKarmaGateRequirements += GuideGateFix;
            GuideCrafts.Hooks();
            On.Player.GrabUpdate += BubbleFruitPop; //slippery ability causes bubblefruit to pop
;

            // Custom Hooks -- Scavenger AI
            ScavBehaviorTweaks.Hooks();
            

            //-- Stops the game from lagging when devtools is enabled and there's scavs in the world
            IL.DenFinder.TryAssigningDen += DenFinder_TryAssigningDen;
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
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
            
            /*if (sLeaser.sprites[2] is TriangleMesh tail)
            {
                //tail.element = Futile.atlasManager.GetElementWithName(SpritePrefix + "TailTexture");
                for (var i = tail.vertices.Length - 1; i >= 0; i--)
                {
                    var perc = i / 2 / (float)(tail.vertices.Length / 2);

                    Vector2 uv;
                    if (i % 2 == 0)
                        uv = new Vector2(perc, 0f);
                    else if (i < tail.vertices.Length - 1)
                        uv = new Vector2(perc, 1f);
                    else
                        uv = new Vector2(1f, 0f);

                    // Map UV values to the element
                    uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                    uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                    tail.UVvertices[i] = uv;
                }
            }*/

            guide.BodySpotsSprite = sLeaser.sprites.Length;
            guide.HipsSpotsSprite = sLeaser.sprites.Length + 1;
            guide.LegsSpotsSprite = sLeaser.sprites.Length + 2;
            guide.HeadGillsSprite = sLeaser.sprites.Length + 3;
            guide.FaceBlushSprite = sLeaser.sprites.Length + 4;

            guide.TasselSpriteA[0] = sLeaser.sprites.Length + 5;
            guide.TasselSpriteA[1] = sLeaser.sprites.Length + 6;
            guide.TasselSpriteA[2] = sLeaser.sprites.Length + 7;
            guide.TasselSpriteA[3] = sLeaser.sprites.Length + 8;
            guide.TasselSpriteA[4] = sLeaser.sprites.Length + 9;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 5 + 5); //Adds spots to sprite array, add five more for the danglefruit sptire

            sLeaser.sprites[guide.BodySpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.HipsSpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.LegsSpotsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.HeadGillsSprite] = new FSprite("pixel");
            sLeaser.sprites[guide.FaceBlushSprite] = new FSprite("pixel");

            for(int i = 0; i < 5; i++)
            {
                sLeaser.sprites[guide.TasselSpriteA[i]] = new FSprite("DangleFruit0A"); //adds 0, 1, 2, 3, 4
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
            
            newContatiner.AddChild(sLeaser.sprites[guide.HeadGillsSprite]);
            sLeaser.sprites[guide.HeadGillsSprite].MoveInFrontOfOtherNode(sLeaser.sprites[3]);
            
            newContatiner.AddChild(sLeaser.sprites[guide.FaceBlushSprite]);
            sLeaser.sprites[guide.FaceBlushSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);

            for(int i = 0; i < 5; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[guide.TasselSpriteA[i]]);
                sLeaser.sprites[guide.TasselSpriteA[i]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            }

            

        }

        private const string SpritePrefix = "GuideSprites_";
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            

            if (!self.IsGuide(out var guide)) return;

            sLeaser.sprites[1].scaleX += 0.15f;
            for(int j = 0; j < 5; j++)
            {
                sLeaser.sprites[guide.TasselSpriteA[j]].scale = 0.5f;
            }
            

            sLeaser.sprites[guide.BodySpotsSprite].Follow(sLeaser.sprites[0]);
            sLeaser.sprites[guide.BodySpotsSprite].color = guide.SpotsColor;
            
            sLeaser.sprites[guide.HipsSpotsSprite].Follow(sLeaser.sprites[1]);
            sLeaser.sprites[guide.HipsSpotsSprite].color = guide.SpotsColor;
            
            sLeaser.sprites[guide.LegsSpotsSprite].Follow(sLeaser.sprites[4]);
            sLeaser.sprites[guide.LegsSpotsSprite].color = guide.SpotsColor;
            
            sLeaser.sprites[guide.HeadGillsSprite].Follow(sLeaser.sprites[3]);
            sLeaser.sprites[guide.HeadGillsSprite].color = guide.GillsColor;

            sLeaser.sprites[guide.FaceBlushSprite].Follow(sLeaser.sprites[9]);
            sLeaser.sprites[guide.FaceBlushSprite].color = guide.SpotsColor;

            

            for(int i = 0; i < 5; i++)
            {
                sLeaser.sprites[guide.TasselSpriteA[i]].color = guide.TasselAColor;
            }

            for(var i = 0; i < 3; i++)
            {
                var offset = Custom.PerpendicularVector(Custom.DirVec(self.tail[i].pos, self.tail[i+1].pos)) * (self.tail[i].rad * 0.8f);
                offset.y = Mathf.Abs(offset.y);

                sLeaser.sprites[guide.TasselSpriteA[i]].SetPosition((self.tail[i].pos + self.tail[i+1].pos) / 2 - camPos + offset);
            }

            
            /*sLeaser.sprites[guide.TasselSpriteA[1]].SetPosition((self.tail[1].pos + self.tail[2].pos) / 2 - camPos + Custom.PerpendicularVector(Custom.DirVec(self.tail[1].pos, self.tail[2].pos)) * 9);
            sLeaser.sprites[guide.TasselSpriteA[2]].SetPosition((self.tail[2].pos + self.tail[3].pos) / 2 - camPos + Custom.PerpendicularVector(Custom.DirVec(self.tail[2].pos, self.tail[3].pos)) * 9);*/
            sLeaser.sprites[guide.TasselSpriteA[3]].SetPosition(self.tail[1].pos - camPos);
            sLeaser.sprites[guide.TasselSpriteA[4]].SetPosition(self.tail[2].pos - camPos);




            sLeaser.sprites[guide.BodySpotsSprite].element = Futile.atlasManager.GetElementWithName(SpritePrefix + "Spots_BodyA");
            if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + "Spots_" + sLeaser.sprites[4].element.name, out var element))
            {
                sLeaser.sprites[guide.LegsSpotsSprite].element = element;
            }
            if (Futile.atlasManager._allElementsByName.TryGetValue(SpritePrefix + "Gills_" + sLeaser.sprites[3].element.name, out element))
            {
                sLeaser.sprites[guide.HeadGillsSprite].element = element;
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
                    
                    if(self.FoodInStomach < self.slugcatStats.foodToHibernate) //if food in stomach is less than food to hibernate (Update = 40 fps, SlipperyTime = update rate * seconds
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


        bool shownHudHint = false;

        //HUD HINT MESSAGE CHECK WHEN LEAVING A SHORTCUT
        private void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            orig(self, pos, newRoom, spitOutAllSticks);

            if (!shownHudHint && self.slugcatStats.name.value == "Guide" && !self.dead && self.room != null && self.room.water && self.abstractCreature.world.game.IsStorySession && self.room.game.cameras[0].hud != null)
            {
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Water is a friend", 20, 200, false, false);
                self.room.game.cameras[0].hud.textPrompt.AddMessage("Submerging grants temporary buffs", 20, 200, false, false);
                shownHudHint = true;
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

        public int kingMask;
        public int glyphMark;

        public ScavStatus(Scavenger scav)
        {
            UnityEngine.Random.seed = scav.abstractCreature.ID.RandomSeed;
            if (UnityEngine.Random.value < 0.2f)
            {
                this.isBaby = true;
            }
            if(!isBaby && UnityEngine.Random.value < 0.1f)
            {
                this.isWarden = true;
                
            }
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
        

        public CritStatus(Creature crit)
        {
            
            
            

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
        public int HeadGillsSprite;
        public int FaceBlushSprite;
        public int[] TasselSpriteA = new int[5];
        public int[] TasselSpriteB;

        public Color BodyColor;
        public Color EyesColor;
        public Color GillsColor;
        public Color SpotsColor;
        public Color TasselAColor;

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