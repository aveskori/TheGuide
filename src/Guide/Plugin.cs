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
using static SlugBase.Features.FeatureTypes;
using Guide.WorldChanges;
using Guide.Creatures;
using Guide.Objects;
using Guide.Guide;
using Guide.Medium;
using SlugBase.Features;
using System.Diagnostics.Eventing.Reader;



namespace GuideSlugBase
{
    [BepInDependency("slime-cubed.slugbase")]
    
    [BepInPlugin(MOD_ID, "Haven", "0.4.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "aveskori.guide";
        


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            
            
            // Fisobs
            //Content.Register(new CloversFisobs());
            Content.Register(new HazerSacFisobs());
            HazerSac.Hooks();
            Content.Register(new LSpearFisobs());
            Content.Register(new SCloverFisobs());
            Content.Register(new CentiShellFisobs());

            // Guide Hooks
            GuideAbilities.Hooks();
            GuideCrafts.Hooks();
            On.Player.Update += Player_Update;

            //Medium Hooks (these are going to be in separate classes)
            MediumAbilities.Hooks();
            //MediumGraphics.Hooks();


            PebblesConversationOverride.Hooks();

            // Custom Hooks -- Scavenger AI
            ScavBehaviorTweaks.Hooks();
            

            //-- Stops the game from lagging when devtools is enabled and there's scavs in the world
            IL.DenFinder.TryAssigningDen += DenFinder_TryAssigningDen;
            
            // Critobs
            Content.Register(new VanLizCritob());
            VanHooks.Hooks();
            Content.Register(new ChrLizCritob());
            CherryHooks.Hooks();
            //Content.Register(new molemousecritob());
            Content.Register(new VoidSpearFisobs());
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
                On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (self.IsGuide(out var guide))
            {
                sLeaser.sprites[guide.FaceBlush[1]].color = Color.Lerp(guide.BlushColor, guide.BodyColor, 0.4f);
                sLeaser.sprites[guide.FaceBlush[0]].color = guide.BlushColor;
                sLeaser.sprites[guide.HipBlush].color = guide.BlushColor;
                sLeaser.sprites[guide.TailTextures[1]].color = guide.BlushColor;
                sLeaser.sprites[guide.TailTextures[0]].color = guide.SpotsColor;
                sLeaser.sprites[guide.HipSpots].color = guide.SpotsColor;
                guide.topGills?.SetGillColors(guide.BodyColor, guide.GillsColor);
                guide.topGills?.ApplyPalette(sLeaser, rCam, palette);
                self.gills?.SetGillColors(guide.BodyColor, guide.GillsColor);
                self.gills?.ApplyPalette(sLeaser, rCam, palette);
                for (int i = 0; i < 6; i++)
                {
                    sLeaser.sprites[guide.TasselSprite[i]].color = guide.TasselColor;
                }
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
            /*
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
            */
            if (self.RenderAsPup)
            {
                self.tail[0] = new(self, 9f, 2f, null, 0.85f, 1.0f, 1.0f, true);
                self.tail[1] = new(self, 7f, 3.5f, self.tail[0], 0.85f, 1.0f, 0.5f, true);
                self.tail[2] = new(self, 5f, 3.5f, self.tail[1], 0.85f, 1.0f, 0.5f, true);
                self.tail[3] = new(self, 3f, 3.5f, self.tail[2], 0.85f, 1.0f, 0.5f, true);
            }
            else
            {
                self.tail[0] = new(self, 9f, 4f, null, 0.85f, 1.0f, 1.0f, true);
                self.tail[1] = new(self, 7f, 7f, self.tail[0], 0.85f, 1.0f, 0.5f, true);
                self.tail[2] = new(self, 5f, 7f, self.tail[1], 0.85f, 1.0f, 0.5f, true);
                self.tail[3] = new(self, 3f, 7f, self.tail[2], 0.85f, 1.0f, 0.5f, true);
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

            guide.StartIndex = sLeaser.sprites.Length;

            int start = guide.StartIndex;
            //Add to array ~~~~~~~~~~~
            guide.HipSpots = start;
            guide.HipBlush = start + 1;

            guide.FaceBlush = new int[2];
            guide.FaceBlush[0] = start + 2;
            guide.FaceBlush[1] = start + 3;

            guide.TasselSprite = new int[6];
            guide.TasselSprite[0] = start + 4;
            guide.TasselSprite[1] = start + 5;
            guide.TasselSprite[2] = start + 6;
            guide.TasselSprite[3] = start + 7;
            guide.TasselSprite[4] = start + 8;
            guide.TasselSprite[5] = start + 9;

            guide.TailTextures = new int[2];
            guide.TailTextures[0] = start + 10;
            guide.TailTextures[1] = start + 11;

            guide.topGills = new UpperHavenGills(self, start + 12);//fixed gills
            self.gills = new LowerHavenGills(self, guide.topGills.startSprite + guide.topGills.numberOfSprites);

            Array.Resize(ref sLeaser.sprites, start + 12 + self.gills.numberOfSprites + guide.topGills.numberOfSprites); //Adds body spots to sprite array (5), add five more for the danglefruit sprite (5), add five more for inner tassel sprite (5), tail spot sprites (5)
            guide.EndIndex = sLeaser.sprites.Length;
            //~~~~~~~~~~~~~~~~~~~
            //Assign sprites ~~~~~~~~~~
            for (int j = guide.StartIndex; j < guide.EndIndex; j++)
            {
                sLeaser.sprites[j] = new FSprite("pixel");
            }

            guide.topGills.InitiateSprites(sLeaser, rCam);
            self.gills.InitiateSprites(sLeaser, rCam);

            for (int b = 0; b < 2; b++)
            {
                TriangleMesh.Triangle[] TailTris = new TriangleMesh.Triangle[]
{
                    new TriangleMesh.Triangle(0, 1, 2),
                    new TriangleMesh.Triangle(1, 2, 3),
                    new TriangleMesh.Triangle(4, 5, 6),
                    new TriangleMesh.Triangle(5, 6, 7),
                    new TriangleMesh.Triangle(8, 9, 10),
                    new TriangleMesh.Triangle(9, 10, 11),
                    new TriangleMesh.Triangle(12, 13, 14),
                    new TriangleMesh.Triangle(2, 3, 4),
                    new TriangleMesh.Triangle(3, 4, 5),
                    new TriangleMesh.Triangle(6, 7, 8),
                    new TriangleMesh.Triangle(7, 8, 9),
                    new TriangleMesh.Triangle(10, 11, 12),
                    new TriangleMesh.Triangle(11, 12, 13)
};
                TriangleMesh Mesh = new("Futile_White", TailTris, false, false);

                sLeaser.sprites[guide.TailTextures[b]] = Mesh;
            }

            for (int m = 0; m < 6; m++)
            {
                sLeaser.sprites[guide.TasselSprite[m]] = new FSprite("GuideTassel");
                sLeaser.sprites[guide.TasselSprite[m]].anchorY = 0.25f;
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

            for (int k = guide.StartIndex; k < guide.EndIndex; k++)
            {
                newContatiner.AddChild(sLeaser.sprites[k]);
            }

            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[0]);

            sLeaser.sprites[guide.HipSpots].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            sLeaser.sprites[guide.HipBlush].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            sLeaser.sprites[guide.FaceBlush[1]].MoveBehindOtherNode(sLeaser.sprites[9]);
            sLeaser.sprites[guide.FaceBlush[0]].MoveBehindOtherNode(sLeaser.sprites[9]);

            for (int j = guide.topGills.startSprite; j < self.gills.startSprite + self.gills.numberOfSprites; j++)
            {
                newContatiner.AddChild(sLeaser.sprites[j]);
                sLeaser.sprites[j].MoveBehindOtherNode(sLeaser.sprites[guide.FaceBlush[1]]);
            }

            for (int b = 0; b < 6; b++)
            {
                sLeaser.sprites[guide.TasselSprite[b]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            }

            sLeaser.sprites[guide.TailTextures[0]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
            sLeaser.sprites[guide.TailTextures[1]].MoveInFrontOfOtherNode(sLeaser.sprites[2]);

        }


        private const string SpritePrefix = "GuideSprites_";//What's this? (Vigaro put this in when adding the spots in DrawSprites)
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            void UpdateReplacement(int num, string tofind)
            {
                try
                {
                    if (true)
                    {
                        if (!sLeaser.sprites[num].element.name.Contains("Guide") && sLeaser.sprites[num].element.name.StartsWith(tofind)) sLeaser.sprites[num].SetElementByName("Guide" + sLeaser.sprites[num].element.name);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
            void UpdateCustom(int findindex, string find, string replace, int customindex)
            {
                try
                {
                    string origelement = sLeaser.sprites[findindex].element.name;

                    origelement = origelement.Replace(find, replace);

                    sLeaser.sprites[customindex].SetElementByName(origelement);
                    sLeaser.sprites[customindex].Follow(sLeaser.sprites[findindex]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            if (!self.IsGuide(out var guide)) return;

            sLeaser.sprites[1].scaleX += 0.2f;

            UpdateReplacement(3, "HeadA");
            UpdateReplacement(4, "LegsA");
            UpdateReplacement(5, "PlayerArm");
            UpdateReplacement(6, "PlayerArm");

            UpdateCustom(9, "Face", "GuideMask1", guide.FaceBlush[0]);
            UpdateCustom(9, "Face", "GuideMask2", guide.FaceBlush[1]);

            self.gills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            guide.topGills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            for (int i = 0; i < 2; i++)
            {
                var tailangle = Custom.AimFromOneVectorToAnother(self.tail[0].pos, self.tail[3].pos);
                bool rightsBehind = false;
                bool leftsBehind = false;
                string tailElement = "Right";

                if ((tailangle is > 0 and < 30) || (tailangle is < 0 and > -30))
                {
                    //all in front of tail
                    rightsBehind = false;
                    leftsBehind = false;
                    tailElement = "Top";
                }
                else if ((tailangle is > 150 and < 180) || (tailangle is < -150 and > -180) || self.player.room.gravity == 0f)
                {
                    //all behind tail
                    rightsBehind = true;
                    leftsBehind = true;
                    tailElement = "Bottom";
                }
                else
                {
                    if (tailangle > 0)
                    {
                        rightsBehind = false;
                        leftsBehind = true;
                        tailElement = "Right";
                    }
                    else
                    {
                        rightsBehind = true;
                        leftsBehind = false;
                        tailElement = "Left";
                    }
                }

                var tailSuffix = i == 0? "A" : "B";
                var tail = sLeaser.sprites[guide.TailTextures[i]] as TriangleMesh;

                if (sLeaser.sprites[2] is TriangleMesh baseMesh)
                {
                    if (tail != null)
                    {
                        for (int k = 0; k < baseMesh.vertices.Length; k++)
                        {
                            tail.MoveVertice(k, new Vector2(baseMesh.vertices[k].x, baseMesh.vertices[k].y));
                        }
                    }
                }

                if ("GuideTail" + tailElement + tailSuffix != sLeaser.sprites[guide.TailTextures[i]].element.name)
                {
                    tail.element = Futile.atlasManager.GetElementWithName("GuideTail" + tailElement + tailSuffix);

                    for (var q = tail.vertices.Length - 1; q >= 0; q--)
                    {
                        var perc = q / 2 / (float)(tail.vertices.Length / 2);

                        Vector2 uv;
                        if (q % 2 == 0)
                            uv = new Vector2(perc, 0f);
                        else if (q < tail.vertices.Length - 1)
                            uv = new Vector2(perc, 1f);
                        else
                            uv = new Vector2(1f, 0f);

                        // Map UV values to the element
                        uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                        uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                        tail.UVvertices[q] = uv;
                    }
                }
                var mainbody = i == 0 ? sLeaser.sprites[guide.HipBlush] : sLeaser.sprites[guide.HipSpots];
                mainbody.Follow(sLeaser.sprites[1]);
                mainbody.rotation -= 180f;
                var mainangle = sLeaser.sprites[0].rotation;
                string mainsuffix;
                if ((mainangle is > 0 and < 20) || (mainangle is < 0 and > -20))
                {
                    mainsuffix = "Front";
                }
                else if ((mainangle is > 20 and < 130) || (mainangle is < -20 and > -130))
                {
                    mainsuffix = "Side";

                    if (mainangle is < -20 and > -120)
                    {
                        mainbody.scaleX *= -1;
                    }
                    else
                    {
                        mainbody.scaleX *= 1;
                    }
                }
                else
                {
                    mainsuffix = "Back";
                }

                mainbody.SetElementByName("Guide" + (i == 0? "Blush" : "Spots") + mainsuffix);

                for (int k = 0; k < 3; k++)
                {
                    var tasselRight = i == 0;

                    FSprite tassel;
                    if (tasselRight)
                    {
                        tassel = sLeaser.sprites[guide.TasselSprite[k]];
                    }
                    else
                    {
                        tassel = sLeaser.sprites[guide.TasselSprite[k + 3]];
                    }
                    if (self.RenderAsPup) tassel.isVisible = false;
                    tassel.scale = Mathf.Lerp(1f, 0.4f, (k + 1) / 3f);
                    if (tasselRight)
                    {
                        if (rightsBehind) tassel.MoveBehindOtherNode(sLeaser.sprites[2]);
                        else tassel.MoveInFrontOfOtherNode(sLeaser.sprites[guide.TailTextures[0]]);
                    }
                    else
                    {
                        if (leftsBehind) tassel.MoveBehindOtherNode(sLeaser.sprites[2]);
                        else tassel.MoveInFrontOfOtherNode(sLeaser.sprites[guide.TailTextures[0]]);
                    }

                    //tail spine positions
                    float TasselspineLerp;
                    if (k == 0) TasselspineLerp = 0.25f;
                    else if (k == 1) TasselspineLerp = 0.45f;
                    else if (k == 2) TasselspineLerp = 0.65f;
                    else TasselspineLerp = 0.65f;

                    if (tasselRight && leftsBehind != rightsBehind) TasselspineLerp -= 0.075f;//slight data adjustments for if the tail is at a side angle

                    var tasselTail = self.SpinePosition(TasselspineLerp, timeStacker);
                    Vector2 TasselPos;
                    float TasselAngle;
                    if (rightsBehind == leftsBehind)//both on same layer(Tail face up or face down)?
                    {
                        TasselAngle = -50f * (tasselRight ? 1f : -1f) + Custom.VecToDeg(tasselTail.dir);
                        TasselPos = tasselTail.pos + tasselTail.perp * (tasselTail.rad * (tasselRight ? 0.6f : -0.6f));
                    }
                    else
                    {
                        TasselAngle = -50f * (tailangle > 0 ? 1f : -1f) + Custom.VecToDeg(tasselTail.dir);
                        var tasselPerp = tasselTail.perp;
                        if (tasselPerp.y <= 0) tasselPerp *= -1;

                        TasselPos = tasselTail.pos + tasselPerp * (tasselTail.rad * 0.75f);

                    }

                    tassel.SetPosition(TasselPos - camPos);
                    tassel.rotation = TasselAngle;
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

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            bool sFlag = !self.GetCat().slippery;

            orig(self, eu);

            if (self.GetCat().IsGuide)
            {
                //when underwater, slippery = true, countdown starts
                if (self.Submersion > 0.33f)
                {
                    if (sFlag)
                    {
                        self.room.PlaySound(SoundID.Red_Lizard_Spit_Hit_Player, 0f, 0.50f, 1f);
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
                /*
                "tunnel_speed": 1.5,
		        "climb_speed": [ 1, 1.2 ],
		        "walk_speed": [ 1.3, 1.5 ],
                    Guide JSON values interfering with run speed code, removed them*/

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
                    self.slugcatStats.runspeedFac = 1f;
                    self.slugcatStats.corridorClimbSpeedFac = 1f;
                    self.slugcatStats.poleClimbSpeedFac = 1f;
                }
                if (self.GetCat().harvestCounter < 40 && self.input[0].pckp && self.input[0].y > 0)
                {
                    self.GetCat().harvestCounter++;
                }
                else if (self.GetCat().harvestCounter > 0)
                {
                    self.GetCat().harvestCounter--;
                }
            }
            if (self.GetMed().IsMedium)
            {
                if(self.GetMed().craftCounter < 40 && self.input[0].pckp && self.input[0].y > 0)
                {
                    self.GetMed().craftCounter++;
                }
                else if(self.GetMed().craftCounter > 0)
                {
                    self.GetMed().craftCounter--;
                }
            }
        }

        //extra hunts for other guide features?
        

        

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadImage("atlases/icon_LanternSpear");
            Futile.atlasManager.LoadImage("atlases/icon_clover");

            Futile.atlasManager.LoadAtlas("guidesprites/GuideArm");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideHead");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideLegs");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideMainBody");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideMask1");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideMask2");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideTail");
            Futile.atlasManager.LoadAtlas("guidesprites/GuideTassel");

            Futile.atlasManager.LoadAtlas("mediumsprites/armright");
            Futile.atlasManager.LoadAtlas("mediumsprites/bodyecho");
            Futile.atlasManager.LoadAtlas("mediumsprites/faceleft");
            Futile.atlasManager.LoadAtlas("mediumsprites/faceright");
            Futile.atlasManager.LoadAtlas("mediumsprites/headecho");
            Futile.atlasManager.LoadAtlas("mediumsprites/hips");
            Futile.atlasManager.LoadAtlas("mediumsprites/legsleft");
        }
    }
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

        public int HipSpots;
        public int HipBlush;
        public UpperHavenGills topGills;

        public int[] TailTextures = new int[2];
        public int[] FaceBlush = new int[2];
        public int[] TasselSprite = new int[6];

        public int StartIndex;
        public int EndIndex;
        public bool SpritesReady;


        public Color BodyColor;
        public Color EyesColor;
        public Color GillsColor;
        public Color SpotsColor;
        public Color TasselColor;
        public Color BlushColor;
        

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
            BlushColor = new PlayerColor("Blush").GetColor(pg) ?? Custom.hexToColor("60c0bb");
            TasselColor = new PlayerColor("Tassels").GetColor(pg) ?? Custom.hexToColor("12a23e");
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
        public int BodySpotSprite;
        public int HipSpotSprite;
        public int FaceBlushSprite;
        public int FaceEchoSprite;
        public int BodyEchoSprite;
        public int HipsEchoSprite;
        public int ArmEchoSprite;
        public int LegEchoSprite;
        public UpperHavenGills topGills;
        public MedTentacle[] tentacles;

        public Color BodyColor;
        public Color BlushColor;
        public Color EyesColor;
        public Color GillsColor;
        public Color EchoColor;
        public Color BlackColor;

        public int craftCounter; //timer for crafting
        public int jumpCounter;

        public MediumStatus(Player player)
        {
            IsMedium = player.slugcatStats.name.value == "Medium";
            this.player = player;
            craftCounter = 0;
            BlackColor = Color.black;
            
        }

        public void SetupColors()
        {
            var pg = (PlayerGraphics)player.graphicsModule;

            BodyColor = new PlayerColor("Body").GetColor(pg) ?? Custom.hexToColor("e8f5ca");
            BlushColor = new PlayerColor("Blush").GetColor(pg) ?? Custom.hexToColor("bf7c53");
            EyesColor = new PlayerColor("Eyes").GetColor(pg) ?? Custom.hexToColor("00271f");
            GillsColor = new PlayerColor("Gills").GetColor(pg) ?? Custom.hexToColor("26593c");
            EchoColor = new PlayerColor("Echo").GetColor(pg) ?? Custom.hexToColor("fcc203");
        }

        

    }
    private static readonly ConditionalWeakTable<Player, MediumStatus> CWT = new();
    public static MediumStatus GetMed(this Player player) => CWT.GetValue(player, _ => new(player));
    public static bool IsMedium(this Player player, out MediumStatus medium) => (medium = player.GetMed()).IsMedium;
    public static bool IsMedium(this PlayerGraphics pg, out MediumStatus medium) => IsMedium(pg.player, out medium);
    

    
}

public class MedTentacle
{
    public int sprite;
    public TailSegment[] segments;

    public MedTentacle(TailSegment[] segments)
    {
        this.segments = segments;
    }
}

public class MedFaceTentacle : MedTentacle
{
    public Vector2 FacePos;
    public Vector2 CamPos;
    public Vector2 Offset;
    public MedFaceTentacle(TailSegment[] segs, Vector2 facePos, Vector2 camPos, Vector2 offset) : base(segs)
    {
        segments = segs;
        FacePos = facePos;
        CamPos = camPos;
        Offset = offset;
    }
}

public class MedTailTentacle : MedTentacle
{
    public int fromSeg;
    public int toSeg;
    public float segLerp;
    public float outerOffset;

    public MedTailTentacle(TailSegment[] segs, int from, int to, float lerp, float outer) : base(segs)
    {
        segments = segs;
        fromSeg = from;
        toSeg = to;
        segLerp = lerp;
        outerOffset = outer;
    }
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
        this.graphic = MediumGills ? 1 : 6;
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
    public new void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = this.startSprite + this.scalesPositions.Length - 1; i >= this.startSprite; i--)
        {
            sLeaser.sprites[i] = new FSprite("LizardScaleA" + this.graphic.ToString(), true);
            sLeaser.sprites[i].scaleY = this.scaleObjects[i - this.startSprite].length / this.graphicHeight * (MediumGills ? 1f : 0.5f);
            sLeaser.sprites[i].anchorY = 0.1f;
            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length] = new FSprite("LizardScaleB" + this.graphic.ToString(), true);
                sLeaser.sprites[i + this.scalesPositions.Length].scaleY = this.scaleObjects[i - this.startSprite].length / this.graphicHeight * (MediumGills ? 1f : 0.5f);
                sLeaser.sprites[i + this.scalesPositions.Length].anchorY = 0.1f;
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
        yOffset = 4f;
        MediumGills = med;

        this.pGraphics = pg;
        this.startSprite = start;
        this.rigor = 0.5873646f;
        float num = 1.310689f;
        this.colored = true;
        this.graphic = MediumGills? 1 : 6;
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
            float num = 50f;
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
            sLeaser.sprites[i].scaleY = (this.scaleObjects[i - this.startSprite].length / this.graphicHeight) * (MediumGills? 1f : 1f);
            sLeaser.sprites[i].anchorY = 0.1f;
            if (this.colored)
            {
                sLeaser.sprites[i + this.scalesPositions.Length] = new FSprite("LizardScaleB" + this.graphic.ToString(), true);
                sLeaser.sprites[i + this.scalesPositions.Length].scaleY = this.scaleObjects[i - this.startSprite].length / this.graphicHeight * (MediumGills ? 1f : 1f);
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