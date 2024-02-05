using System;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Guide.Objects;
using System.Linq;

namespace Guide.WorldChanges
{
    //Moved all Scav behavior mods here !!
    public class ScavBehaviorTweaks
    {
        public ScavBehaviorTweaks()
        {
        }

        public static readonly ScavengerAI.Behavior ToldToStay = new ScavengerAI.Behavior("ToldToStay", true);
        public static readonly ScavengerAI.Behavior ToldToFollow = new ScavengerAI.Behavior("ToldToFollow", true);

        public static void Hooks()
        {
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            On.ScavengerAI.DecideBehavior += ScavengerAI_FollowGuide;
            On.ScavengerAI.SocialEvent += ScavengerAI_SocialEvent; //GUIDE TAKING SCAV ITEMS DOESN'T DECREASE REP
            On.ScavengerAI.DecideBehavior += ScavengerAI_FeedGuide;
            //On.ScavengerAI.DecideBehavior += LanternSpearControl;
            //On.ScavengerAI.ViolenceTypeAgainstCreature += LanternSpear_TargetCreature;
        }

        private static void ScavengerAI_FeedGuide(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            //cycle through scavenger grasps, array[i], check if item i is a plant, if so, move to front
            Player guide = FindNearbyGuide(self.scavenger.room);
           
            if(self.scavenger.grasps.Any(x => x?.grabbed is IPlayerEdible) && guide != null)
            {
                var itemFind = self.scavenger.grasps.IndexOf(self.scavenger.grasps.First(x => x?.grabbed is IPlayerEdible));
                var item = self.scavenger.grasps[itemFind].grabbed;

                
                if (self.behavior == ScavengerAI.Behavior.Idle && guide.slugcatStats.malnourished || guide.CurrentFood < guide.slugcatStats.foodToHibernate && self.scavenger.animation != null)
                {  //move food item to front of inv, find guide, FEED

                    self.scavenger.MoveItemBetweenGrasps(item, itemFind, 0);
                    Debug.Log("step 1");
                    self.SetDestination(guide.abstractCreature.pos);
                    if (Custom.Dist(self.scavenger.mainBodyChunk.pos, guide.mainBodyChunk.pos) < 20)
                    {
                        Debug.Log("step 2");
                        self.scavenger.animation.id = Scavenger.ScavengerAnimation.ID.GeneralPoint;
                        Debug.Log("step 3");
                        self.scavenger.room.PlaySound(SoundID.Slugcat_Swallow_Item, guide.bodyChunks[0]);
                        item.Destroy();
                        guide.AddFood(1);
                    }
                    

                    /*if (guide.eatCounter < 1)
                    {
                        guide.eatCounter = 15;
                        PhysicalObject guideItem = guide.grasps[1]?.grabbed;
                        bool regrab = false;
                        if ((item as IPlayerEdible).BitesLeft == 1 && guideItem != null)
                            regrab = true;
                        if ((item as IPlayerEdible).BitesLeft == 1 && guide.SessionRecord != null)
                            guide.SessionRecord.AddEat(item);
                        if (item is Creature)
                            (item as Creature).SetKillTag(guide.abstractCreature);
                        //if (guide.graphicsModule != null) //SKIP THIS ACTUALLY IT'S FOR HAND MOVEMENT
                        //(guide.graphicsModule as PlayerGraphics).BiteFly(i);
                        // (item as IPlayerEdible).BitByPlayer(guide.grasps[i], eu);
                        bool eu = false; // ???
                        (item as IPlayerEdible).BitByPlayer(guide.grasps[1], eu);
                        if (regrab)
                            guide.SlugcatGrab(guideItem, 1);
                    }*/              
                }
                
            }
            return;
           
            
        }

        /*private static ScavengerAI.ViolenceType LanternSpear_TargetCreature(On.ScavengerAI.orig_ViolenceTypeAgainstCreature orig, ScavengerAI self, Tracker.CreatureRepresentation critRep)
        {
            ScavengerAI.ViolenceType v = new("Lethal", true);
            //check each body chunk
            for(int i = 0; i < 3; i++)
            {
                //if lSpear stuck in body chunk, target parent creature
                if(self.scavenger.room.)
                {

                }
            }
        }*/

        /*private static void LanternSpearControl(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            for (int i = 0; i < self.scavenger.room.game.Players.Count; i++)
            {
                //check if: In same room, Player is Guide, Guide is Valid, Guide is holding Lantern Spear
                if (self.scavenger.room.game.Players[i].realizedCreature is Player checkPlayer &&
                    checkPlayer != null && checkPlayer.room != null
                    && checkPlayer.slugcatStats.name.value == "Guide" && !(checkPlayer.dead || checkPlayer.inShortcut)
                    && (checkPlayer.grasps[0].grabbed is LanternSpear || checkPlayer.grasps[1].grabbed is LanternSpear) 
                    )
                {
                    
                    if ((checkPlayer.input[0].jmp) && !(checkPlayer.input[1].jmp) && (checkPlayer.bodyMode != Player.BodyModeIndex.Default)) //player.input == jump
                    {
                        if ((checkPlayer.input[0].y == -1 && checkPlayer.input[0].x == 0)) //down + jump (?), stay
                        {
                            //self.behavior = ScavengerAI.Behavior.GuardOutpost;
                            self.SetDestination(self.scavenger.abstractCreature.pos);
                            
                        }
                        if ((checkPlayer.input[0].y == 1 && checkPlayer.input[0].x == 0) && (checkPlayer.bodyMode != Player.BodyModeIndex.ClimbingOnBeam)) // up + jmp, return to normal
                        {
                            orig(self);
                            
                        }
                        for (int j = 1; j < 5; j++)
                        {
                            if ((checkPlayer.input[j].jmp))
                            {
                                Player closeGuide = FindNearbyGuide(self.scavenger.room);
                                if (closeGuide != null && Custom.Dist(self.scavenger.mainBodyChunk.pos, closeGuide.mainBodyChunk.pos) > 200)
                                {
                                    self.SetDestination(closeGuide.abstractCreature.pos);
                                }
                             
                            }

                        }
                    }
                }
                orig(self);
               
            }
        }*/

        /*private static void ScavengerAI_Spear_TargetCreature(On.ScavengerAI.orig_CreatureSpotted orig, ScavengerAI self, bool firstSpot, Tracker.CreatureRepresentation otherCreature)
        {
            // if LanternSpear stuck in otherCreature, see creature, go to attack
            if (LanternSpear.Mode == LanternSpear.Mode.StuckInCreature)
            {

            }
        }*/

        public static Player FindNearbyGuide(Room myRoom)
        {
            if (myRoom == null)
                return null; //WE'RE NOT EVEN IN A ROOM TO CHECK

            for (int i = 0; i < myRoom.game.Players.Count; i++)
            {
                if (myRoom.game.Players[i].realizedCreature is Player checkPlayer
                    && checkPlayer != null && checkPlayer.room != null //&& checkPlayer.room == myRoom //MAKE SURE THEY ARE IN OUR ROOM. or maybe not...
                    && checkPlayer.slugcatStats.name.value == "Guide"
                    && !(checkPlayer.dead || checkPlayer.inShortcut) //AND VALID 
                )
                {
                    return checkPlayer; //GUIDE FOUND! RETURN THE REFERENCE
                }
            }
            return null; //NO GUIDE FOUND. RETURN NULL
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        { //Custom Collect scores for extra items, plant consumables. Scavs will take and forage for these
            string regionName = self.scavenger.room.world.region.name;
            if (self.scavenger.room != null && obj != null && FindNearbyGuide(self.scavenger.room) != null)
            {
                if (obj is DangleFruit)
                {
                    return 2;
                }
                if (obj is WaterNut || obj is GooieDuck)
                {
                    if (self.scavenger.room.game.IsStorySession && regionName == "GW"
                        || regionName == "LM" || regionName == "DS" || regionName == "SL")
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
                    if (self.scavenger.room.game.IsStorySession && regionName == "SI")
                    {
                        return 2;
                    }
                    else
                    {
                        return 5;
                    }

                }
                if (obj is GlowWeed || obj is LillyPuck || obj is SeedCob || obj is HazerSac)
                {
                    return 7;
                }
                if (obj is LanternSpear)
                {
                    return 0;
                }
                if (obj is SlimeMold || obj is Centipede)
                {
                    return 5;
                }
                if (obj is EggBugEgg || obj is Hazer)
                {
                    return 4;
                }
            }
            return orig(self, obj, weaponFiltered);
        }

        private static void ScavengerAI_FollowGuide(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            orig(self);
            //if no threat detected, (if has food item) > set destination to player
            if (self.behavior == ScavengerAI.Behavior.Idle)
            {
                Player closeGuide = FindNearbyGuide(self.scavenger.room);
                if (closeGuide != null && Custom.Dist(self.scavenger.mainBodyChunk.pos, closeGuide.mainBodyChunk.pos) > 200)
                {
                    self.SetDestination(closeGuide.abstractCreature.pos); //self.scavenger.room.game.Players[0].pos
                }
            }
        }

        private static void ScavengerAI_SocialEvent(On.ScavengerAI.orig_SocialEvent orig, ScavengerAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
        {
            //GUIDE CAN COMMIT CRIMES...
            if (subjectCrit is Player && (subjectCrit as Player).slugcatStats.name.value == "Guide")
            {
                if (ID == SocialEventRecognizer.EventID.Theft
                    || ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt
                    || ID == SocialEventRecognizer.EventID.NonLethalAttack
                    || ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
                {
                    return; //JUST PRETEND IT DIDN'T HAPPEN...
                }
            }
            orig(self, ID, subjectCrit, objectCrit, involvedItem);
        }
    }
}
