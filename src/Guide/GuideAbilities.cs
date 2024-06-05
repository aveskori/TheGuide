using Guide.WorldChanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guide.Guide
{
    internal class GuideAbilities
    {
        public GuideAbilities() { }
        public static void Hooks()
        {
            //Player Update
            //Creature Grasp Update
            //Jelly
            //Centi
            //Bubble Fruit pop
            //Lung Update
            //Shortcut HUD           
            On.Creature.Grasp.ctor += Grasp_ctor;

            On.JellyFish.Collide += JellyFish_Collide;  //Guide immunity to jellyfish stuns
            On.JellyFish.Update += JellyFish_Update; //AND JELLYFISH TICKLES...
            On.Centipede.Shock += Centipede_Shock; // if slippery, immune to centishocks
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut; //HUD HINTS
            //On.RegionGate.customKarmaGateRequirements += GuideGateFix;
            
            On.Player.GrabUpdate += BubbleFruitPop; //slippery ability causes bubblefruit to pop
            On.Player.LungUpdate += Player_LungUpdate; //Infinite capacity lungs, no more panic swim

        }

        //(Pocky-Raisin) Try looking into slugBase savedata, it's intended to be used like that
        //HHH DOESNT WORK YET
        //(Visiting FP with a LanternSpear ticks SpearKey to true, Guide gains access to LC and UY)
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

        private static void Player_LungUpdate(On.Player.orig_LungUpdate orig, Player self)
        {
            if (self.IsGuide(out var guide) || self.IsMedium(out var medium))
            {
                //basically makes infinite air and stops player from panicking while swimming  
                if (self.airInLungs < 1f)
                {
                    self.airInLungs = 1f;
                }
            }

        }

        private static void BubbleFruitPop(On.Player.orig_GrabUpdate orig, Player self, bool eu) //certified nut sweller
        {
            orig(self, eu);

            if (self.GetCat().IsGuide && self.GetCat().slippery
                && ScavBehaviorTweaks.FindNearbyGuide(self.room) != null)  //Player is guide, guide is slippery, guide is not null
            {
                for (int i = 0; i < 2; i++)
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
                    
                }
            }
            return;
        }

        private static void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
        {
            //check if player guide AND slippery, else run orig
            if (shockObj is Player && (shockObj as Player).slugcatStats.name.value == "Guide" && (shockObj as Player).GetCat().slippery)
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

        private static void JellyFish_Collide(On.JellyFish.orig_Collide orig, JellyFish self, PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            if ((otherObject as Player).slugcatStats?.name?.value == "Guide")
            {
                self.room.PlaySound(SoundID.Slugcat_Bite_Centipede, self.bodyChunks[0], false, 1.5f, 1.5f);
                return;
            }
            else
            {
                orig(self, otherObject, myChunk, otherChunk);
            }
        }

        private static void JellyFish_Update(On.JellyFish.orig_Update orig, JellyFish self, bool eu)
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

        private static void Grasp_ctor(On.Creature.Grasp.orig_ctor orig, Creature.Grasp self, Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
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

        static bool shownWaterHint = false;
        //bool shownJellyHint = false;
        //bool shownCentiHint = false;

        //HUD HINT MESSAGE CHECK WHEN LEAVING A SHORTCUT
        private static void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
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
    }

}
