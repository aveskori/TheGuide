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
using UnityEngine;
using MoreSlugcats;
using Noise;
using static MonoMod.InlineRT.MonoModRule;

namespace Guide.Medium
{
    internal class MediumAbilities
    {

        
        public static void Hooks()
        {
            //Super Jump
            //SpearCraft
            On.Player.GrabUpdate += Medium_SpearCraft;
            
            On.Player.Update += Player_Update;
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
            On.Player.ctor += Player_ctor;
            On.Player.SpitOutOfShortCut += Player_SpitOutOfShortCut;
            
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (!self.IsMedium(out var medium)) return;

            
            self.airFriction = 0.2f;
            self.customPlayerGravity = 0.5f;
            self.setPupStatus(true);
            self.HypothermiaGain *= 1.5f; 
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

         
         //0 - grounded, 1 - jumped once, 2 - echoJumped
        
        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            

            orig(self, eu);
            if(self.IsMedium(out var medium))
            {
                if (self.canJump > 0 || !self.Consious || self.Stunned || self.animation == Player.AnimationIndex.HangFromBeam
               || self.animation == Player.AnimationIndex.ClimbOnBeam || self.bodyMode == Player.BodyModeIndex.WallClimb ||
               self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.VineGrab ||
               self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.bodyMode == Player.BodyModeIndex.Swimming ||
               ((self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity <= 0.5f || self.gravity <= 0.5f) &&
               (self.wantToJump == 0 || !self.input[0].pckp))) //reset jumpCounter if the player is doing literally anything other than being in the air, also IDEALLY only runs when counter is above 0
                {
                    medium.jumpCounter = 0;
                }
                if (self.input[0].jmp && medium.jumpCounter == 0) //count up  whem jumping
                {
                    medium.jumpCounter = 1;
                }
                //double jump
                if (self.feetStuckPos == null && self.input[0].pckp && self.input[0].jmp && medium.jumpCounter == 1)
                {
                    Vector2 pos = self.firstChunk.pos;
                    medium.jumpCounter = 2;
                    self.room.PlaySound(MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Core_On, 0.5f, 0.7f, 0.6f);
                    self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.Lerp(new Color(1f, 1f, 1f), new Color(0f, 0.8f, 1f), UnityEngine.Random.value)));
                    if (self.input[0].x != 0)
                    {
                        self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.x, 0f) + 8f;
                        self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.x, 0f) + 7f;
                        self.jumpBoost = 10f;
                    }
                    if (self.input[0].x == 0 || self.input[0].y == 1)
                    {
                        self.bodyChunks[0].vel.y = 16f;
                        self.bodyChunks[1].vel.y = 15f;
                        self.jumpBoost = 10f;
                    }
                    if (self.input[0].y == 1)
                    {
                        self.bodyChunks[0].vel.x = 10f * (float)self.input[0].x;
                        self.bodyChunks[1].vel.x = 8f * (float)self.input[0].x;
                    }
                    self.animation = Player.AnimationIndex.Flip;
                    self.bodyMode = Player.BodyModeIndex.Default;
                }
                if (medium.jumpCounter == 2)
                {
                    Vector2 pos = self.bodyChunks[1].pos + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), 9f + Mathf.Lerp(-2f, 2f, UnityEngine.Random.value));
                    //self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, new UnityEngine.Color(1f, 0.8f, 0.01f), null, 30, 120));
                    self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, Color.Lerp(new Color(1f, 1f, 1f), new Color(1f, 0.8f, 0.01f), UnityEngine.Random.value), null, 30, 120));
                }
                
            }
            
            

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

                    if (item != null && item is Spear && self.GetMed().craftCounter == 40 && self.GraspsCanBeCrafted())
                    {
                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk);
                        self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, Color.Lerp(new Color(1f, 1f, 1f), new Color(1f, 0.8f, 0.01f), UnityEngine.Random.value), null, 30, 120));
                        self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, Color.Lerp(new Color(1f, 1f, 1f), new Color(1f, 0.8f, 0.01f), UnityEngine.Random.value), null, 30, 120));
                        (item as Spear).Destroy();
                        self.SubtractFood(2);
                        self.room.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_A, self.mainBodyChunk);
                        AbstractPhysicalObject voidSpear = new VoidSpearAbstract(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID());
                        self.room.abstractRoom.AddEntity(voidSpear);
                        voidSpear.RealizeInRoom();
                        return;
                    }
                }
            }
        }

        static bool shownJumpHint = false;
        static bool shownSpearHint = false;

        private static void Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, RWCustom.IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            orig(self, pos, newRoom, spitOutAllSticks);

            if (self.slugcatStats.name.value == "Medium" && !self.dead && self.room != null && self.abstractCreature.world.game.IsStorySession && self.room.game.cameras[0].hud != null)
            {
                if (!shownJumpHint)
                {
                    self.room.game.cameras[0].hud.textPrompt.AddMessage("While in the air, press JUMP + GRAB to perform a double jump.", 20, 200, false, false);
                    
                    shownJumpHint = true;
                }
                if(!shownSpearHint && self.grasps.Any(x => x?.grabbed is Spear) && self.FoodInStomach >= 2)
                {
                    self.room.game.cameras[0].hud.textPrompt.AddMessage("Some creatures cannot be killed by normal means.", 20, 200, false, false);
                    self.room.game.cameras[0].hud.textPrompt.AddMessage("Hold UP + GRAB to craft a Void Spear.", 20, 200, false, false);

                    shownSpearHint = true;
                }

            }
        }


        public MediumAbilities()
        {
            
        }
    }
}
