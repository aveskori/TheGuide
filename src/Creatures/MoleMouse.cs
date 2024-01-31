using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RWCustom;
using MoreSlugcats;
using On.MoreSlugcats;
using System.Runtime.CompilerServices;

namespace Guide.Creatures
{
    public class molemousecritob : Critob
    {
        public molemousecritob() : base(CustomTemplates.moleMouse)
        {
            Icon = new SimpleIcon("Kill_Mouse", new Color(0.5f, 0.4f, 0.4f));
            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.MMouse);
            mmHooks hooks = new mmHooks();
            hooks.applyHooks();
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new MouseAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new LanternMouse(acrit, acrit.world);
        }

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.LanternMouse, CustomTemplates.moleMouse, "MoleMouse").IntoTemplate();
            t.dangerousToPlayer = 1;
            t.grasps = 1;
            t.stowFoodInDen = true;
            t.shortcutColor = new Color(0.5f, 0.4f, 0.4f);

            return t;
        }
        //create state?
        public override void EstablishRelationships()
        {
            Relationships rels = new Relationships(CustomTemplates.moleMouse);
            rels.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 1f);
            rels.Eats(CreatureTemplate.Type.Slugcat, 1f);
            rels.Eats(CreatureTemplate.Type.JetFish, 1f);
            rels.Eats(CreatureTemplate.Type.Scavenger, 1f);
            rels.Eats(CreatureTemplate.Type.EggBug, 1f);
            rels.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
            rels.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
            rels.Eats(CreatureTemplate.Type.Hazer, 1f);
            rels.Eats(CreatureTemplate.Type.Spider, 1f);
            rels.Eats(CreatureTemplate.Type.VultureGrub, 1f);
            rels.Eats(CreatureTemplate.Type.TubeWorm, 1f);
            rels.Eats(CreatureTemplate.Type.Fly, 1f);

            rels.IsInPack(CustomTemplates.moleMouse, 4f);
            rels.IsInPack(CreatureTemplate.Type.BlackLizard, 5f);
            rels.IgnoredBy(CreatureTemplate.Type.BlackLizard);

            rels.EatenBy(CreatureTemplate.Type.Vulture, 1f);
            rels.EatenBy(CreatureTemplate.Type.KingVulture, 1f);



        }

        public class mmHooks
        {
            public void applyHooks()
            {
                On.MouseAI.ctor += GivePreyTracker;
                On.MouseAI.Update += Hunter;
                On.LanternMouse.Update += LanternMouse_Update;
                On.MouseAI.IUseARelationshipTracker_ModuleToTrackRelationship += Preyrelationshipfix;
                On.MouseGraphics.DrawSprites += MoleMouse_DrawSprites;
                On.LanternMouse.CarryObject += LanternMouse_CarryObject;

            }

            //WIP Find and follow Black Lizard
            /*public static Creature FindNearbyLizard(Room myRoom, Player player, MouseAI self, CreatureTemplate.Type lizard)
            {
                if (myRoom == null)
                    return null; //WE'RE NOT EVEN IN A ROOM TO CHECK

                if (self.mouse.room != null && CreatureTemplate.Type.BlackLizard != null)
                return null; //NO GUIDE FOUND. RETURN NULL
            }*/

            private void LanternMouse_CarryObject(On.LanternMouse.orig_CarryObject orig, LanternMouse self)
            {
                if (!self.safariControlled && self.grasps[0].grabbed is Creature && self.AI.DynamicRelationship((self.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats)
                {
                    self.AI.preyTracker.ForgetPrey((self.grasps[0].grabbed as Creature).abstractCreature);
                    self.LoseAllGrasps();
                    return;
                }
                PhysicalObject grabbed = self.grasps[0].grabbed;
                float num = self.mainBodyChunk.rad + self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad;
                Vector2 a = -Custom.DirVec(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos));
                float num2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass / (self.mainBodyChunk.mass + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass);
                num2 *= 0.2f * (1f - self.AI.stuckTracker.Utility());
                self.mainBodyChunk.pos += a * num2;
                self.mainBodyChunk.vel += a * num2;
                grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos -= a * (1f - num2);
                grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel -= a * (1f - num2);
                Vector2 vector = self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * num;
                Vector2 vector2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel - self.mainBodyChunk.vel;
                grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel = self.mainBodyChunk.vel;
                if (self.enteringShortCut == null && (vector2.magnitude * grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad)))
                {
                    self.LoseAllGrasps();
                }
                else
                {
                    grabbed.bodyChunks[self.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(self.abstractCreature.world.game.evenUpdate, vector);
                }
                if (self.grasps[0] != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        self.grasps[0].grabbed.PushOutOf(self.bodyChunks[i].pos, self.bodyChunks[i].rad, self.grasps[0].chunkGrabbed);
                    }
                }
            }

            private void MoleMouse_DrawSprites(On.MouseGraphics.orig_DrawSprites orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                orig(self, sLeaser, rCam, timeStacker, camPos);

                if (self.mouse.Template.type == CustomTemplates.moleMouse)
                {
                    molemousedata.TryGetValue(self.mouse, out moledata value);
                    for (int i = 0; i < value.bulbs.Length; i++)
                    {
                        sLeaser.sprites[value.currentsprite].color = self.blackColor;

                    }
                }
            }

            ConditionalWeakTable<LanternMouse, moledata> molemousedata = new ConditionalWeakTable<LanternMouse, moledata>();
            public class moledata
            {
                public int startsprite;
                public int currentsprite;
                public int numofsprites;
                public int numbulbs;
                public bool ready;
                public bulb[] bulbs;
            }

            public class bulb
            {
                public int firstsprite;
                public int numofsprites = 2;

            }

            private AIModule Preyrelationshipfix(On.MouseAI.orig_IUseARelationshipTracker_ModuleToTrackRelationship orig, MouseAI self, CreatureTemplate.Relationship relationship)
            {
                if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
                {
                    return self.preyTracker;
                }
                return orig(self, relationship);
            }

            private void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
            {
                orig(self, eu);
                if (self.Template.type == CustomTemplates.moleMouse)
                {
                    if (self.AI.behavior == MouseAI.Behavior.Hunt)
                    {
                        if (self.AI.preyTracker.MostAttractivePrey != null)
                        {
                            Tracker.CreatureRepresentation prey = self.AI.preyTracker.MostAttractivePrey;
                            Creature realprey = prey.representedCreature.realizedCreature;
                            if (Custom.DistLess(prey.representedCreature.pos, self.abstractCreature.pos, 4f))
                            {
                                self.Squeak(1f);
                                realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));


                            }
                        }
                    }
                }
            }

            private void Hunter(On.MouseAI.orig_Update orig, MouseAI self)
            {
                if (self.mouse.Template.type == CustomTemplates.moleMouse)
                {
                    self.preyTracker.Update();
                    orig(self);
                    AIModule aimoduule = self.utilityComparer.HighestUtilityModule();
                    if (aimoduule != null && aimoduule is PreyTracker)
                    {
                        self.behavior = MouseAI.Behavior.Hunt;
                    }
                    if (self.behavior == MouseAI.Behavior.Hunt)
                    {
                        if (self.preyTracker.MostAttractivePrey != null && !self.mouse.safariControlled)
                        {
                            self.creature.abstractAI.SetDestination(self.preyTracker.MostAttractivePrey.BestGuessForPosition());
                            self.mouse.runSpeed = Mathf.Lerp(self.mouse.runSpeed, 1f, 0.08f);
                        }
                    }
                }
                else
                {
                    orig(self);
                }
            }

            private void GivePreyTracker(On.MouseAI.orig_ctor orig, MouseAI self, AbstractCreature creature, World world)
            {
                orig(self, creature, world);
                if (self.mouse.Template.type == CustomTemplates.moleMouse)
                {
                    self.AddModule(new PreyTracker(self, 3, 2f, 10f, 40f, 0.5f));
                    self.utilityComparer.AddComparedModule(self.preyTracker, null, 1f, 1.5f);
                    self.AddModule(new StuckTracker(self, true, false));
                }
            }


        }
    }
}
