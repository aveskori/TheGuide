using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using Guide.Objects;
using RWCustom;
using MoreSlugcats;

namespace Guide.Medium
{
    internal class MediumAbilities
    {
        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("super_jump");
        public static void Hooks()
        {
            //Super Jump
            //SpearCraft
            On.Player.GrabUpdate += Medium_SpearCraft;
            On.Player.Jump += Medium_SuperJump;

        }

        

        private static void Medium_SpearCraft(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if(self != null && self.room != null) //craft timer counter
            {
                if (self.GetMed().craftCounter > 0)
                {
                    Limb myHand = (self.graphicsModule as PlayerGraphics).hands[0];
                    myHand.mode = Limb.Mode.HuntAbsolutePosition;
                    myHand.absoluteHuntPos = self.bodyChunks[0].pos;
                }
                for (int i = 0; i < 2; i++)
                {
                    PhysicalObject item = self.grasps[i]?.grabbed;

                    if (item != null && item is Spear && self.GetMed().craftCounter == 40 && self.FoodInStomach >= 2)
                    {
                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk);
                        self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, new UnityEngine.Color(1f, 0.8f, 0.01f), null, 30, 120));
                        (item as Spear).Destroy();
                        self.SubtractFood(2);
                        self.room.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_A, self.mainBodyChunk);
                        AbstractPhysicalObject voidSpear = new VoidSpearAbstract(self.room.world, (item as Spear), self.abstractCreature.pos, self.room.game.GetNewID());
                        self.room.abstractRoom.AddEntity(voidSpear);
                        voidSpear.RealizeInRoom();
                        return;
                    }
                }
            }
        }

        private static void Medium_SuperJump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);
            if(self.GetMed().IsMedium && SuperJump.TryGet(self, out var superJump))
            {
                self.jumpBoost *= 2f + superJump;
            }
            
        }

        public MediumAbilities()
        {
            
        }
    }
}
