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
using Fisobs.Creatures;

namespace GuideSlugBase
{
    //-- Setting slugbase and DMS as dependencies to ensure our mod loads after them
    //-- You can set DMS as a soft dependency since the mod will still work without it, it just won't have custom graphics
    [BepInDependency("slime-cubed.slugbase")]
    [BepInDependency("dressmyslugcat", BepInDependency.DependencyFlags.SoftDependency)]
    
    [BepInPlugin(MOD_ID, "Guide", "0.3.0")]
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
            //GuideGills.Hooks();
            PebblesConversationOverride.Hooks();


            // Custom Hooks -- Scavenger AI
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            //On.ScavengerAI.DecideBehavior += ScavengerAI_DecideBehavior;
            On.ScavengerAI.SocialEvent += ScavengerAI_SocialEvent;
            On.Scavenger.GrabbedObjectSnatched += Scavenger_GrabbedObjectSnatched;
            On.ScavengerAI.PlayerRelationship += ScavengerAI_PlayerRelationship;
            //ScavengerBehaviorModification.Hooks();   < very not done. currently breaks scavs entirely lmao
            
        }

        private CreatureTemplate.Relationship ScavengerAI_PlayerRelationship(On.ScavengerAI.orig_PlayerRelationship orig, ScavengerAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            CreatureTemplate.Relationship relationship = self.StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
            if (dRelation.trackerRep.representedCreature.realizedCreature is Player && (dRelation.trackerRep.representedCreature.realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 10f);
                (dRelation.state as ScavengerAI.ScavengerTrackState).taggedViolenceType = ScavengerAI.ViolenceType.None;
                
            }
            orig(self, dRelation);
            return relationship;
        }

        private void Scavenger_GrabbedObjectSnatched(On.Scavenger.orig_GrabbedObjectSnatched orig, Scavenger self, PhysicalObject grabbedObject, Creature thief)
        {
            orig(self, grabbedObject, thief);
            if ((self.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide" && self.room != null)
            {
                if (thief == self.room.game.Players[0].realizedCreature as Player)
                {
                    self.AI.agitation = 0f;
                    if (self.Elite)
                    {
                        self.AI.agitation = 0f;
                    }
                }
            }
            

        }

        private void ScavengerAI_SocialEvent(On.ScavengerAI.orig_SocialEvent orig, ScavengerAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
        {
            orig(self, ID, subjectCrit, objectCrit, involvedItem);
            if ((self.scavenger.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide" && involvedItem != null && self.scavenger.room != null)
            {
                if (ID == SocialEventRecognizer.EventID.Theft)
                {
                    self.scavenger.AI.agitation = 0f;
                    if (self.scavenger.Elite)
                    {
                        self.scavenger.AI.agitation = 0f;
                    }
                }
            }
            


        }

        private int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if ((self.scavenger.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                if (self.scavenger.room != null && obj != null)
                {
                    if (obj is DangleFruit)
                    {
                        return 2;
                    }
                    if (obj is WaterNut || obj is GooieDuck)
                    {
                        if (self.scavenger.room.game.IsStorySession && self.scavenger.room.world.region.name == "GW")
                        {
                            return 7;
                        }
                        else
                        {
                            return 3;
                        }
                        
                    }
                    if (obj is DandelionPeach)
                    {
                        if (self.scavenger.room.game.IsStorySession && self.scavenger.room.world.region.name == "SI")
                        {
                            return 2;
                        }
                        else
                        {
                            return 5;
                        }

                    }
                    if (obj is GlowWeed || obj is LillyPuck)
                    {
                        return 7;
                    }
                    if (obj is LanternSpear)
                    {
                        return 0;
                    }
                    if (obj is SlimeMold)
                    {
                        return 5;
                    }
                }
                return orig(self, obj, weaponFiltered);
            }
            else
            {
                return orig(self, obj, weaponFiltered);
            }
            
        }


        

        /*private void ScavengerAI_DecideBehavior(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            orig(self);
            int playerLocX = (self.scavenger.room.game.Players[0].pos.x) + 2;
            int playerLocY = self.scavenger.room.game.Players[0].pos.y;
            WorldCoordinate playerLocation = (playerLocX, playerLocY);
            if ((self.scavenger.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                
                //if no threat detected, if has food item > set destination to player
                if (self.behavior == ScavengerAI.Behavior.Idle && self.scavenger.room == (self.scavenger.room.game.Players[0].realizedCreature as Player).room && self.scavenger.room != null)
                {
                    self.SetDestination(self.scavenger.room.game.Players[0].pos);
                    
                }

                
            }

        }*/




        //tail release and slippery release
        private void Grasp_ctor(On.Creature.Grasp.orig_ctor orig, Creature.Grasp self, Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
        {

            orig(self, grabber, grabbed, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
            if ((self.grabber.room.game.Players[0].realizedCreature as Player).slugcatStats.name.value == "Guide")
            {
                
                if ((self.grabber.room.game.Players[0].realizedCreature as Player).grabbedBy.Count > 0 && slippery == true)
                {
                    self.grabber.room.PlaySound(SoundID.Water_Nut_Swell, grabbed.bodyChunks[1], false, 1f, 2f, false);
                    grabber.ReleaseGrasp(graspUsed);
                    grabbed.AllGraspsLetGoOfThisObject(true);
                    slipperyTime = 40 * 15;
                    slippery = true;
                }
                
            }
        }
        //state slippery and countdown
        int slipperyTime;
        bool slippery;
        
        int rnd = UnityEngine.Random.Range(0, 2);
        public bool Pebbles_Guide = true;
        
        
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self.room?.game?.Players[0]?.realizedCreature is Player player && player?.slugcatStats.name.value == "Guide")
            {
                
                //when underwater, slippery = true, countdown starts
                if (self.animation == Player.AnimationIndex.DeepSwim)
                {
                    self.room.PlaySound(SoundID.Big_Spider_Spit, player.bodyChunks[1], false, 1f, 2f, false);
                    slipperyTime = 40 * 30;
                    slippery = true;
                    self.slugcatStats.runspeedFac = 1.5f;
                    self.slugcatStats.corridorClimbSpeedFac = 1.6f;
                    self.slugcatStats.poleClimbSpeedFac = 1.6f;
                    self.waterFriction = 0.99f;
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

            }
            orig(self, eu);
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
                    Slugcat = "aveskori.guide",
                    PlayerNumber = i,
                    CustomSprites = new List<CustomSprite>
                    {
                        //-- You can customize which spritesheet and color each body part will use
                        new CustomSprite() { Sprite = "HEAD", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "FACE", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "GILLS", SpriteSheetID = sheetID, Color = Color.green },
                        new CustomSprite() { Sprite = "BODY", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "ARMS", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "HIPSRIGHT", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "HIPSLEFT", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "TAIL", SpriteSheetID = sheetID},
                        new CustomSprite() { Sprite = "TAILSPOTS", SpriteSheetID = sheetID, Color = Color.blue}
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