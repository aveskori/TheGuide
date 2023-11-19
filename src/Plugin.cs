using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using System.Runtime.CompilerServices;
using Fisobs.Core;
using LanternSpearFO;
using DressMySlugcat;
using MoreSlugcats;
using Guide;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

namespace GuideSlugBase
{
    //-- Setting slugbase and DMS as dependencies to ensure our mod loads after them
    //-- You can set DMS as a soft dependency since the mod will still work without it, it just won't have custom graphics
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    
    [BepInPlugin(MOD_ID, "Guide", "0.4.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "aveskori.guide";
        

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            // Custom Hooks -- Slugcat
            Content.Register(new LSpearFisobs());
            Content.Register(new VanLizCritob());

            On.Player.Update += Player_Update;
            On.Creature.Grasp.ctor += Grasp_ctor;
            PebblesConversationOverride.Hooks();
            On.JellyFish.Collide += JellyFish_Collide;  //Guide immunity to jellyfish stuns
            On.JellyFish.Update += JellyFish_Update; //AND JELLYFISH TICKLES...
            On.Centipede.Shock += Centipede_Shock;
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut; //HUD HINTS

            //On.AbstractCreature.ctor += BoomScugAbstr;

            // Custom Hooks -- Scavenger AI
            ScavBehaviorTweaks.Hooks();

            //-- Stops the game from lagging when devtools is enabled and there's scavs in the world
            IL.DenFinder.TryAssigningDen += DenFinder_TryAssigningDen;
        }

        /*private void BoomScugAbstr(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            self = new AbstractCreature(world, creatureTemplate, realizedCreature, pos, ID);
            self.state = new PlayerNPCState(self, 0);
            var pl = new Player(self, world);

            pl.npcCharacterStats = new SlugcatStats(MoreSlugcatsEnums.SlugcatStatsName.Artificer, false);

            pl.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
            pl.slugcatStats.name = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
            pl.playerState.slugcatCharacter = MoreSlugcatsEnums.SlugcatStatsName.Artificer;

            pl.abstractCreature.abstractAI = new SlugNPCAbstractAI(world, pl.abstractCreature);
            pl.abstractCreature.abstractAI.RealAI = new SlugNPCAI(self, world);

            //if artispawn = true, spawn arti in room UW_A13
        }*/

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
            if (self.slugcatStats.name.value == "Guide")
            {
                //when underwater, slippery = true, countdown starts
                if (self.animation == Player.AnimationIndex.DeepSwim)
                {
                    self.room.PlaySound(SoundID.Big_Spider_Spit, 0f, 0.50f, 1f);
                    self.GetCat().slipperyTime = 40 * 30;
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
                }
                else
                {
                    self.GetCat().slippery = false;
                    self.slugcatStats.runspeedFac = 1f;
                    self.slugcatStats.corridorClimbSpeedFac = 1f;
                    self.slugcatStats.poleClimbSpeedFac = 1f;
                }

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


        public static bool IsPostInit;
        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsPostInit) return;
                IsPostInit = true;

                //-- You can have the DMS sprite setup in a separate method and only call it if DMS is loaded
                //-- With this the mod will still work even if DMS isn't installed
                if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
                {
                    SetupDMSSprites();
                }

                Debug.Log($"Plugin dressmyslugcat.guide is loaded!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public void SetupDMSSprites()
        {
            //-- The ID of the spritesheet we will be using as the default sprites for our slugcat
            var sheetID = "aveskori.guide";

            //-- Each player slot (0, 1, 2, 3) can be customized individually
            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    //-- Make sure to use the same ID as the one used for our slugcat
                    Slugcat = "Guide",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        //-- You can customize which spritesheet and color each body part will use
                        new CustomSprite() { Sprite = "HEAD", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "FACE", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "GILLS", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "BODY", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "ARMS", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "HIPS", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "LEGS", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "HIPSRIGHT", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "HIPSLEFT", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "TAIL", SpriteSheetID = sheetID, Color = Color.white},
                        new CustomSprite() { Sprite = "TAILSPOTS", SpriteSheetID = sheetID, Color = Color.white}
                    },

                    //-- Customizing the tail size and color is also supported, values should be set between 0 and 1
                    CustomTail = new CustomTail()
                    {

                        Length = 4f,
                        Wideness = 1.5f,
                        Roundness = 0.9f

                    }
                });
            }
        }

 

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadImage("atlases/icon_LanternSpear");
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

        public GuideStatus()
        {
            // Initialize your variables here! (Anything not added here will be null or false or 0 (default values))
            artiSpawn = false;
        }
    }

    // This part lets you access the stored stuff by simply doing "self.GetCat()" in Plugin.cs or everywhere else!
    private static readonly ConditionalWeakTable<Player, GuideStatus> CWT = new();
    public static GuideStatus GetCat(this Player player) => CWT.GetValue(player, _ => new());
}
