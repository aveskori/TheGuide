using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugBase.Features;
using RWCustom;
using static SlugBase.Features.FeatureTypes;
using MoreSlugcats;

namespace Guide
{
    /*public class ScavengerBehaviorModification
    {
       
        public static readonly GameFeature<bool> ModifyScavengerBehavior = GameBool("ModifyScavengerBehavior");
        public static readonly ScavengerAI.Behavior CloseToGuide = new ScavengerAI.Behavior("CloseToGuide", true);
        public static readonly Scavenger.ScavengerAnimation.ID Sleep = new Scavenger.ScavengerAnimation.ID("Sleep", true);
        

        public static void Hooks()
        {
            
            
        }

        
        /*public class SleepAnimation : Scavenger.ScavengerAnimation
        {
            public SleepAnimation(Scavenger scavenger) : base(scavenger, Scavenger.ScavengerAnimation.ID.Sleep)
            {
                Scavenger.Sleep.sitPos = scavenger.room.GetWorldCoordinate(scavenger.occupyTile);
            }
        }*/

        /*private static void StayWithGuide(On.ScavengerAI.orig_ctor orig, ScavengerAI self, AbstractCreature creature, World world)
        {
            if (ModifyScavengerBehavior.TryGet(self.scavenger.room.game, out bool modify) && modify)
            {
                self.behavior = new ScavengerAI.Behavior("StayWithGuide", true);
                
                if ((self.behavior != ScavengerAI.Behavior.Flee) && (self.behavior != ScavengerAI.Behavior.EscapeRain) && (self.behavior != ScavengerAI.Behavior.Attack) && (self.behavior != ScavengerAI.Behavior.GuardOutpost))
                {
                    creature.abstractAI.InternalSetDestination(self.scavenger.room.game.Players[0].pos);
                    self.behavior = ScavengerAI.Behavior.Travel;
                }
            }
            orig(self, creature, world);
        }
    }*/
}
