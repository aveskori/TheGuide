
using LanternSpearFO;

using MoreSlugcats;
using RWCustom;

namespace Guide
{
    //Moved all Scav behavior mods here !!
    public class ScavBehaviorTweaks
    {

        public static void Hooks()
        {
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            On.ScavengerAI.DecideBehavior += ScavengerAI_DecideBehavior;
            On.ScavengerAI.SocialEvent += ScavengerAI_SocialEvent; //GUIDE TAKING SCAV ITEMS DOESN'T DECREASE REP
        }

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
            if (self.scavenger.room != null && obj != null && FindNearbyGuide(self.scavenger.room) != null)
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

        private static void ScavengerAI_DecideBehavior(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
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
