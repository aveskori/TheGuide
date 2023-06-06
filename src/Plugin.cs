using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using SlugBase;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.CompilerServices;
using Fisobs.Core;
using LanternSpearFO;
using Menu;
using DressMySlugcat;
using MoreSlugcats;
using Guide;

namespace GuideSlugBase
{
    //-- Setting slugbase and DMS as dependencies to ensure our mod loads after them
    //-- You can set DMS as a soft dependency since the mod will still work without it, it just won't have custom graphics
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    
    [BepInPlugin(MOD_ID, "Guide", "0.2.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "aveskori.applecat";


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            // Custom Hooks -- Slugcat
            Content.Register(new LSpearFisobs());
            On.Menu.MenuIllustration.ctor += MenuIllustration_ctor;
            On.Player.Update += Player_Update;
            On.Creature.Grasp.ctor += Grasp_ctor;
            GuideGills.Hooks();
            PebblesConversationOverride.Hooks();
           

            // Custom Hooks -- Scavenger AI
            On.ScavengerAI.CollectScore_ItemRepresentation_bool += ScavengerAI_CollectScore_ItemRepresentation_bool1;
            On.ScavengerAI.DecideBehavior += ScavengerAI_DecideBehavior;
            //ScavengerBehaviorModification.Hooks();   < very not done. currently breaks scavs entirely lmao
            
        }

        
        private int ScavengerAI_CollectScore_ItemRepresentation_bool1(On.ScavengerAI.orig_CollectScore_ItemRepresentation_bool orig, ScavengerAI self, ItemTracker.ItemRepresentation obj, bool weaponFiltered)
        {
            if ((self.scavenger.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                

                /*if (obj.representedItem is )
                {
                    
                }*/

            }
            return orig(self, obj, weaponFiltered);
        }



        private void ScavengerAI_DecideBehavior(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            orig(self);
            if ((self.scavenger.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                
                //if no threat detected, if has food item > set destination to player
                if (self.behavior == ScavengerAI.Behavior.Idle)
                {
                    self.SetDestination(self.scavenger.room.game.Players[0].pos);
                }
            }
            
        }

        //set consumable plant collection score


        //tail release and slippery release
        private void Grasp_ctor(On.Creature.Grasp.orig_ctor orig, Creature.Grasp self, Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
        {
            orig(self, grabber, grabbed, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
            if ((self.grabber.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                
                if ((self.grabber.room.game.Players[0].realizedCreature as Player).grabbedBy.Count > 0 && slippery == true)
                {
                    grabber.ReleaseGrasp(graspUsed);
                    grabbed.AllGraspsLetGoOfThisObject(true);
                    slipperyTime = 40 * 15;
                    slippery = true;
                }
                if ((self.grabber.room.game.Players[0].realizedCreature as Player).grabbedBy.Count > 0 && !slippery && tailAttached)
                {
                    //set tailAttached to false, set hasTail to false
                    //break tail connection
                    tailAttached = false;
                    grabbed.AllGraspsLetGoOfThisObject(true);
                    grabber.ReleaseGrasp(graspUsed);
                    self.grabber.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, 0f, 0.50f, 1f);
                }
            }
            
        }
        //state slippery and countdown
        int slipperyTime;
        bool slippery;
        bool tailAttached = true;
        int rnd = UnityEngine.Random.Range(0, 2);
        public bool Pebbles_Guide = true;
        
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.room?.game?.Players[0]?.realizedCreature is Player player && player?.slugcatStats.name.value == "Guide")
            {
                //when underwater, slippery = true, countdown starts
                if (self.animation == Player.AnimationIndex.DeepSwim)
                {
                    self.room.PlaySound(SoundID.Big_Spider_Spit, 0f, 0.50f, 1f);
                    slipperyTime = 40 * 30;
                    slippery = true;
                    self.slugcatStats.runspeedFac = 1.5f;
                    self.slugcatStats.corridorClimbSpeedFac = 1.6f;
                    self.slugcatStats.poleClimbSpeedFac = 1.6f;

                }
                //slippery countdown
                if (slipperyTime > 0)
                {
                    slipperyTime--;
                }
                else
                {
                    slippery = false;
                    self.slugcatStats.runspeedFac = 1f;
                    self.slugcatStats.corridorClimbSpeedFac = 1f;

                    self.slugcatStats.poleClimbSpeedFac = 1f;
                }
                //if tail is detached, extend food to hibernate
                if (!tailAttached)
                {
                    self.slugcatStats.foodToHibernate = 8;
                    
                }
                else
                {
                    self.slugcatStats.foodToHibernate = 5;
                }
                //set slide length based on slippery
                if (slippery)
                {
                    self.longBellySlide = true;
                }
                else
                {
                    self.longBellySlide = false;
                }
            }
            
        }

        //custom sleep screen
        private void MenuIllustration_ctor(On.Menu.MenuIllustration.orig_ctor orig, MenuIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, bool crispPixels, bool anchorCenter)
        {
            orig(self, menu, owner, folderName, fileName, pos, crispPixels, anchorCenter);
            if (menu is KarmaLadderScreen ladderScreen && ladderScreen.saveState != null)
            {
                SaveState save = ladderScreen.saveState;
                if(save != null && !tailAttached)
                {
                    fileName = fileName.Replace("guide_randomized", "guide_notail");
                }
                else if (save != null && tailAttached) 
                {
                    fileName.Replace("guide_randomized", "guide_" + rnd.ToString());
                }
            }
            fileName.Replace("guide_0", "guide_randomized");
            fileName.Replace("guide_1", "guide_randomized");
            fileName.Replace("guide_2", "guide_randomized");
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
            var sheetID = "aveskori.applecat";

            //-- Each player slot (0, 1, 2, 3) can be customized individually
            for (int i = 0; i < 4; i++)
            {
                SpriteDefinitions.AddSlugcatDefault(new Customization()
                {
                    //-- Make sure to use the same ID as the one used for our slugcat
                    Slugcat = "aveskori.applecat",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        //-- You can customize which spritesheet and color each body part will use
                        new CustomSprite() { Sprite = "HEAD", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "FACE", SpriteSheetID = sheetID, Color = Color.white },
                        
                        new CustomSprite() { Sprite = "BODY", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "ARMS", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "HIPSRIGHT", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "HIPSLEFT", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "TAIL", SpriteSheetID = sheetID, Color = Color.white },
                        new CustomSprite() { Sprite = "TAILSPOTS", SpriteSheetID = sheetID, Color = Color.white }
                    },

                    //-- Customizing the tail size and color is also supported, values should be set between 0 and 1
                    CustomTail = new CustomTail()
                    {
                        Length = i / 4f,
                        Wideness = i / 6f,
                        Roundness = 0.9f
                    }
                });
            }
        }
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}