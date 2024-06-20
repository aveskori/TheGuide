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
using Noise;
using static MonoMod.InlineRT.MonoModRule;

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
            //On.Player.Update += Player_Update;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.ctor += Player_ctor;
            On.AdrenalineEffect.Update += AdrenalineEffect_Update;
        }

        private static void AdrenalineEffect_Update(On.AdrenalineEffect.orig_Update orig, AdrenalineEffect self, bool eu)
        {
            throw new NotImplementedException();
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (!self.IsMedium(out var medium)) return;

            self.gravity = 0.2f;
            self.airFriction = 0.2f;
            self.setPupStatus(true);
            
            
        }

        private static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {

            if (!self.IsMedium(out var medium)) return orig(self);

            if(self.FoodInStomach >= 2)
            {
                for(int i = 0; i < self.grasps.Length; i++)
                {
                    if ((self.grasps[i] != null && self.grasps[i].grabbed is Spear))
                    {
                        return true;
                    }
                    
                }
            }
            return false;
            
        }

        /*private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {

            if (!self.IsMedium(out var medium)) return;
            if (self.feetStuckPos != null)
            {
                medium.jumpCounter = 0;
            }
            if (!self.pyroJumpped && self.canJump <= 0 && (self.input[0].y >= 0 || 
                (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f))) && 
                self.Consious && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb && 
                self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.animation != Player.AnimationIndex.HangFromBeam && self.animation 
                != Player.AnimationIndex.ClimbOnBeam && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Swimming && 
                self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation 
                != Player.AnimationIndex.ZeroGPoleGrab && self.onBack == null)
            {
                self.gravity = 0f;
                float num3 = (float)self.input[0].x;
                float num4 = (float)self.input[0].y;
                while (num3 == 0f && num4 == 0f)
                {
                    num3 = (float)(((double)UnityEngine.Random.value <= 0.33) ? 0 : (((double)UnityEngine.Random.value <= 0.5) ? 1 : -1));
                    num4 = (float)(((double)UnityEngine.Random.value <= 0.33) ? 0 : (((double)UnityEngine.Random.value <= 0.5) ? 1 : -1));
                }
                
                self.bodyChunks[0].vel.x = 9f * num3;
                self.bodyChunks[0].vel.y = 9f * num4;
                self.bodyChunks[1].vel.x = 9f * num3;
                self.bodyChunks[1].vel.y = 9f * num4;
                
                
                self.room.AddObject(new LightningMachine.Impact(self.firstChunk.pos, 5f, medium.EchoColor));
                medium.jumpCounter++;
            }
        }*/

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

                    if (item != null && item is Spear && self.GetMed().craftCounter == 40 && self.GraspsCanBeCrafted())
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
