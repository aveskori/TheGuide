using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using CritType = CreatureTemplate.Type;
using Guide.Objects;
using MoreSlugcats;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using Guide.WorldChanges;

namespace Guide.Guide
{
    public class GuideCrafts
    {
        
        public static void Hooks()
        {
            On.Player.GrabUpdate += Player_GrabUpdate;


        }

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            //Centipede shells
            //Infant Centi shells
            //Red centi shells
            //centiwing shells
            //centiwing wings

            //Dandi Peach fuzz


            orig(self, eu);
            
            
            

            if (ScavBehaviorTweaks.FindNearbyGuide(self.room) != null)
            {
                if(self.GetCat().harvestCounter > 0)
                {
                    Limb myHand = (self.graphicsModule as PlayerGraphics).hands[0];
                    myHand.mode = Limb.Mode.HuntAbsolutePosition;
                    myHand.absoluteHuntPos = self.bodyChunks[0].pos;
                }
                for (int i = 0; i < 3; i++) //loop check each grasp
                {
                    PhysicalObject item = self.grasps[i]?.grabbed; //item = each object grabbed

                    if (item != null && self.GetCat().harvestCounter == 40) // if one grasp isnt null ADD INPUT CHECK HERE
                    {
                        if(item is Hazer && !(item as Hazer).dead)
                        {
                            (item as Hazer).inkLeft = 0;
                            (item as Hazer).hasSprayed = true;
                            (item as Hazer).Die();
                            self.room.PlaySound(SoundID.Tentacle_Plant_Grab_Slugcat, self.mainBodyChunk);
                            //sound effect probably
                            AbstractPhysicalObject hazerSac = new HazerSacAbstract(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID());
                            self.room.abstractRoom.AddEntity(hazerSac);
                            hazerSac.RealizeInRoom();
                            //apo realize
                            return;
                        }
                    }
                    
                }
            }
            return;
            
            
        }

        
    }
}
