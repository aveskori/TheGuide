using System;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Guide.Objects;
using System.Linq;
using ScavengerCosmetic;

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
            On.Weapon.HitThisObject += Weapon_HitThisObject;
            

            //BABIES
            /*On.Scavenger.ctor += Scruffling_ctor;
            On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;
            On.ScavengerGraphics.Update += ScavengerGraphics_Update;
            On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
            //On.Scavenger.Update += PiggyBack;
            On.ScavengerGraphics.ScavengerHand.ctor += ScavengerHand_ctor;
            On.ScavengerGraphics.ScavengerLeg.ctor += ScavengerLeg_ctor;
            //On.ScavengerGraphics.AddToContainer += ScavengerGraphics_AddToContainer;
            */

            //On.ScavengerGraphics.ApplyPalette += ScavengerGraphics_ApplyPalette;

            //On.ScavengerAI.DecideBehavior += ScavengerAI_SpearControl;
            //On.ScavengerAI.DecideBehavior += LanternSpearControl;
            //On.ScavengerAI.ViolenceTypeAgainstCreature += LanternSpear_TargetCreature;
            //On.ScavengerAI.CreatureSpotted += ScavengerAI_Spear_TargetCreature;
            
        }

        






        /*private static void ScavengerGraphics_ApplyPalette(On.ScavengerGraphics.orig_ApplyPalette orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            string regionName = self.scavenger.room.world.region.name;
            if (self != null && rCam.room != null && 
                self.scavenger.GetScav().isWarden)
            {
                if(regionName == "SI" || regionName == "LC" || regionName == "UW")
                {
                    sLeaser.sprites[self.ShellSprite].color = new Color(0.1f, 0.98f, 0.1f);
                }
                if(regionName == "SL" || regionName == "LM")
                {
                    sLeaser.sprites[self.ShellSprite].color = new Color(0.1f, 0.1f, 0.98f);
                }
                if(regionName == "GW" || regionName == "DS" || regionName == "LF")
                {
                    sLeaser.sprites[self.ShellSprite].color = new Color(0.98f, 0.1f, 0.1f);
                }
                else
                {
                    sLeaser.sprites[self.ShellSprite].color = new Color(1f, 0.7f, 0f);
                }
            }
        }*/

        /*private static void ScavengerGraphics_AddToContainer(On.ScavengerGraphics.orig_AddToContainer orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (!self.scavenger.GetScav().isWarden) return;

            
            newContatiner ??= rCam.ReturnFContainer("Midground");

            newContatiner.AddChild(sLeaser.sprites[self.scavenger.GetScav().kingMask]);
            sLeaser.sprites[self.scavenger.GetScav().kingMask].MoveInFrontOfOtherNode(sLeaser.sprites[1]);

            newContatiner.AddChild(sLeaser.sprites[self.scavenger.GetScav().glyphMark]);
            sLeaser.sprites[self.scavenger.GetScav().glyphMark].MoveInFrontOfOtherNode(sLeaser.sprites[1]);
        }*/

        private static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
        {            
            if( self != null && obj != null && !(self.thrownBy as Scavenger).GetScav().isBaby &&    //scav has to be adult
                (self.thrownBy is Scavenger && obj is Player && (obj as Player).GetCat().IsGuide) ||    //if scavs throw spear at guide
                (self.thrownBy is Player) && obj is Scavenger && (self.thrownBy as Player).GetCat().IsGuide)   //or if guide throw spear at scav
            {
                
                return false; //otherwise, wont hit guide
            }
            return orig(self, obj); //babies can hit and be hit by guide cus funny
        }

        

        private static void ScavengerLeg_ctor(On.ScavengerGraphics.ScavengerLeg.orig_ctor orig, ScavengerGraphics.ScavengerLeg self, ScavengerGraphics owner, int num, int firstSprite)
        {
            orig(self, owner, num, firstSprite);
            if (self.scavenger.GetScav().isBaby)
            {
                self.legLength *= 0.5f;
            }
            if (self.scavenger.GetScav().isWarden)
            {
                self.legLength *= 1.5f;
            }
        }

        private static void ScavengerHand_ctor(On.ScavengerGraphics.ScavengerHand.orig_ctor orig, ScavengerGraphics.ScavengerHand self, ScavengerGraphics owner, int num, int firstSprite)
        {
            orig(self, owner, num, firstSprite);
            if (self.scavenger.GetScav().isBaby)
            {
                self.armLength *= 0.3f;
                
            }
            if (self.scavenger.GetScav().isWarden)
            {
                self.armLength *= 1.5f;
            }
        }

        private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.scavenger.GetScav().isBaby)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[self.EyeSprite(j, 0)].scaleX *= 2f;
                    
                }
            }
            if (self.scavenger.GetScav().isWarden)
            {
                for(int j = 0;j < 2; j++)
                {
                    sLeaser.sprites[self.EyeSprite(j, 0)].scaleY *= 0.8f;
                }

                /*sLeaser.sprites[self.scavenger.GetScav().kingMask].Follow(sLeaser.sprites[1]);
                sLeaser.sprites[self.scavenger.GetScav().glyphMark].Follow(sLeaser.sprites[2]);*/
            }
        }

        private static void ScavengerGraphics_Update(On.ScavengerGraphics.orig_Update orig, ScavengerGraphics self)
        {
            orig(self);
            if (self.scavenger.GetScav().isBaby)
            {
                self.spineLengths[0] *= 0.5f;
            }

            
            Player guide = FindNearbyGuide(self.scavenger.room);

            if (guide != null && !guide.inShortcut && guide.GetCat().IsGuide &&
                self.scavenger.GetScav().isBaby && Custom.Dist(self.scavenger.mainBodyChunk.pos, guide.mainBodyChunk.pos) < 20)
            {
                Limb myHand = self.hands[0];
                Limb yourHand = (guide.graphicsModule as PlayerGraphics).hands[1];
                yourHand.absoluteHuntPos = new Vector2(guide.mainBodyChunk.pos.x + 10, guide.mainBodyChunk.pos.y);
                myHand.mode = Limb.Mode.HuntAbsolutePosition;
                myHand.absoluteHuntPos = yourHand.absoluteHuntPos;
                myHand.huntSpeed = 0.5f;
                myHand.pos = yourHand.pos;
                
            }
        }

        private static void ScavengerGraphics_InitiateSprites(On.ScavengerGraphics.orig_InitiateSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.scavenger.GetScav().isBaby)
            {
                float mySize = 0.5f;
                sLeaser.sprites[self.ChestSprite].scaleX *= 1f;
                sLeaser.sprites[self.ChestSprite].scaleY *= 0.9f;
                sLeaser.sprites[self.HipSprite].scale *= mySize;
                sLeaser.sprites[self.HeadSprite].scale *= mySize;
                
                
            }
            if (self.scavenger.GetScav().isWarden)
            {
                


                float mySize = 1.5f;
                sLeaser.sprites[self.ChestSprite].scaleX *= 1f;
                sLeaser.sprites[self.ChestSprite].scaleY *= 0.9f;
                sLeaser.sprites[self.HipSprite].scale *= mySize;
                sLeaser.sprites[self.HeadSprite].scale *= 1.8f;

                //Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + self.scavenger.armorPieces);

                /*self.scavenger.GetScav().kingMask = sLeaser.sprites.Length;
                self.scavenger.GetScav().glyphMark = sLeaser.sprites.Length + 1;

                sLeaser.sprites[self.MaskSprite] = self.maskGfx. 
                sLeaser.sprites[self.scavenger.GetScav().glyphMark] = new FSprite("TinyGlyph1");*/
            }
        }

        private static void Scruffling_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
        {
            
            orig(self, abstractCreature, world);
            if (self.GetScav().isBaby)
            {
                float mySize = 0.4f;
                self.bodyChunks[0].rad *= mySize;
                self.bodyChunks[1].rad *= mySize;
                self.bodyChunks[2].rad *= mySize;
                self.bodyChunkConnections[0].distance *= mySize;
                self.bodyChunkConnections[1].distance *= mySize;
                self.LoseAllGrasps();

                self.lungs *= mySize;
                self.blockingSkill = 0f;
                self.dodgeSkill = 0f;
                self.meleeSkill = 0f;
                self.reactionSkill = 0f;

            }

            if (self.GetScav().isWarden)
            {
                self.armorPieces = 3;
                float mySize = 1.5f;
                self.bodyChunks[0].rad *= mySize;
                self.bodyChunks[1].rad *= mySize;
                self.bodyChunks[2].rad *= mySize;
                self.bodyChunkConnections[0].distance *= mySize;
                self.bodyChunkConnections[1].distance *= mySize;
                self.LoseAllGrasps();

                self.lungs *= mySize;
                self.blockingSkill = 1f;
                self.dodgeSkill = 1f;
                self.meleeSkill = 1f;
                self.reactionSkill = 1f;

                
            }
        }

        /*private static void ScavengerAI_SpearControl(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            
        }*/

        private static void ScavengerAI_FeedGuide(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            
            Player guide = FindNearbyGuide(self.scavenger.room);
            if(self.scavenger.GetScav().isBaby) { return; }
           
            if(self.scavenger.grasps.Any(x => x?.grabbed is IPlayerEdible) && guide != null) //if any items in inv are edible, and if guide != null
            {
                var itemFind = self.scavenger.grasps.IndexOf(self.scavenger.grasps.First(x => x?.grabbed is IPlayerEdible)); //find edible item index
                var item = self.scavenger.grasps[itemFind].grabbed; //find index value

                
                if (self.behavior == ScavengerAI.Behavior.Idle && ((guide.CurrentFood < guide.slugcatStats.foodToHibernate) || 
                    ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus")) && self.scavenger.animation != null)  //scavs will keep feeding guide despite current pip count if rotund world on, you're welcome willow <3
                {   
                    if (self.scavenger.grasps[0]?.grabbed is not IPlayerEdible) //if item in slot 1 
                    {
                        self.scavenger.MoveItemBetweenGrasps(item, itemFind, 0);
                        return;
                    }
                    
                    Debug.Log("step 1");
                    self.SetDestination(guide.abstractCreature.pos);

                    if (Custom.Dist(self.scavenger.mainBodyChunk.pos, guide.mainBodyChunk.pos) < 20 && self.scavenger.grasps[0]?.grabbed is IPlayerEdible)
                    {
                        Debug.Log("step 2");
                        Limb myHand = (self.scavenger.graphicsModule as ScavengerGraphics).hands[0];
                        myHand.mode = Limb.Mode.HuntAbsolutePosition;
                        myHand.absoluteHuntPos = guide.bodyChunks[0].pos;
                        myHand.pos = guide.bodyChunks[0].pos;

                        Debug.Log("step 3");
                        self.scavenger.room.PlaySound(SoundID.Slugcat_Swallow_Item, guide.bodyChunks[0]);
                        item.Destroy();
                        guide.AddFood(1);
                        return;
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
            orig(self, firstSpot, otherCreature);
            Player guide = FindNearbyGuide(self.scavenger.room);
            Room room = self.scavenger.room;
            var spearList = room.updateList.OfType<LanternSpear>();


            //attack
            if (guide != null && self != null && spearList != null)
            { //if lspear stuck in creature, target that creature

                foreach (LanternSpear spear in spearList)
                {
                    if (spear.stuckInChunk != null && spear.stuckInChunk.owner is Creature)
                    {

                    }
                }
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
            
            if (self.scavenger.room != null && obj != null && FindNearbyGuide(self.scavenger.room) != null && self.scavenger.room.world.game.IsStorySession)
            {
               
                if (self.scavenger.GetScav().isBaby)
                {
                    if(obj is Rock)
                    {
                        return 8;
                    }
                    return 0;
                }

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
                if(obj is Scavenger && (obj as Scavenger).GetScav().isBaby || obj is SwollenWaterNut)
                {
                    if (self.scavenger.room.abstractRoom.name.Contains("SCAVHAVEN"))
                    {
                        return 10;
                    }
                    return 8;
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
