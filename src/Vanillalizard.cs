using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RWCustom;
using MoreSlugcats;
using On.MoreSlugcats;

namespace Guide
{
    public class VanillaLizard
    {               
            //enum setup
            public static CreatureTemplate.Type vanillaLizard = new(nameof(VanillaLizard), true);
            public static void UnregisterValues()
            {
                if (vanillaLizard != null)
                {
                    vanillaLizard.Unregister();
                    vanillaLizard = null;
                }
            }
    }

        //sandbox setup
        public static class SandboxUnlockID
        {
            public static MultiplayerUnlocks.SandboxUnlockID VLiz = new(nameof(VLiz), true);
            public static void UnregisterValues()
            {
                if (VLiz != null)
                {
                    VLiz.Unregister();
                    VLiz = null;
                }
            }
        }
        public class VanHooks
        {
            public static void Hooks()
            {
                On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += VanBreed;
                On.LizardAI.LurkTracker.Update += LurkTracker_Update;
                On.LizardCosmetics.TailFin.DrawSprites += VanTailFin;
                On.LizardGraphics.HeadColor += VanHeadColor;
                On.LizardGraphics.BodyColor += VanBodyColor;
                On.LizardGraphics.ctor += VanCosmetics;
                On.LizardVoice.GetMyVoiceTrigger += VanVoice;
                On.LizardAI.ctor += VanSpit;
                On.LizardAI.Update += VnSpitFix;
                //On.LizardAI.ctor += AbLeap;
                On.LizardAI.AggressiveBehavior += VnSpitAggressive;
                On.LizardGraphics.ApplyPalette += VanApplySprites;
                
            
            }

        private static void VanApplySprites(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.lizard.Template.type == VanillaLizard.vanillaLizard)
            {
                self.ColorBody(sLeaser, new Color(0.9333f, 0.9098f, 0.8666f));
            }
        }

        /*private static void AbLeap(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if(self.lizard.Template.type == DrainLizard.drainLizard)
            {
            self.AddModule(new L);

            }
        }*/
            private static void VnSpitAggressive(On.LizardAI.orig_AggressiveBehavior orig, LizardAI self, Tracker.CreatureRepresentation target, float tongueChance)
            {
                orig(self, target, tongueChance);
                if (self.lizard.Template.type == VanillaLizard.vanillaLizard && target.VisualContact)
                {
                    self.lizard.JawOpen = Mathf.Clamp(self.lizard.JawOpen + 0.1f, 0f, 1f);
                }
            }

            private static void VnSpitFix(On.LizardAI.orig_Update orig, LizardAI self)
            {
                orig(self);
                if (self.lizard.Template.type == VanillaLizard.vanillaLizard && self.redSpitAI.spitting)
                {
                    self.lizard.EnterAnimation(Lizard.Animation.Spit, false);
                }
            }

            private static void VanSpit(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
            {
                orig(self, creature, world);

                if (self.lizard.Template.type == VanillaLizard.vanillaLizard)
                {
                    self.AddModule(new SuperHearing(self, self.tracker, 250f));

                    self.redSpitAI = new LizardAI.LizardSpitTracker(self);
                    self.AddModule(self.redSpitAI);
                }

            }

            private static SoundID VanVoice(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
            {
                var res = orig(self);
                if(self.lizard is Lizard l && l.Template.type == VanillaLizard.vanillaLizard)
                {
                    string[] array = new[] { "Green_A", "Eel_B", "Black_A" };
                    List<SoundID> list = new List<SoundID>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        SoundID soundID = SoundID.None;
                        string text = "Lizard_Voice_" + array[i];
                        if (ExtEnum<SoundID>.values.entries.Contains(text))
                        {
                            soundID = new SoundID(text, false);
                        }
                        if (soundID != SoundID.None && soundID.Index != -1 && self.lizard.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                        {
                            list.Add(soundID);
                        }
                    }
                    if (list.Count == 0)
                    {
                        res = SoundID.None;
                    }
                    else
                    {
                        res = list[Random.Range(0, list.Count)];
                    }

                }

                return res;
            }
            

            private static void VanCosmetics(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
            {
                orig(self, ow);
                if (self.lizard.Template.type == VanillaLizard.vanillaLizard)
                {
                    var state = Random.state;
                    Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
                    var num = self.startOfExtraSprites + self.extraSprites;
                    //self.ivarBodyColor = new Color(1f, 1f, 0.98f);
                    
                    
                    num = self.AddCosmetic(num, new LizardCosmetics.Whiskers(self, num));
                    if(Random.value < 0.4f)
                    {
                        var e = new LizardCosmetics.TailGeckoScales(self, num);
                        num = self.AddCosmetic(num, e);
                    }
                    /*if(Random.value < 0.2f)
                    {
                        var e = new LizardCosmetics.LongBodyScales(self, num);
                        e.colored = true;
                        
                        num = self.AddCosmetic(num, e);
                    }*/
                    if(Random.value < 0.8f)
                    {
                        var e = new LizardCosmetics.TailFin(self, num);
                        e.colored = false;
                        e.numberOfSprites = e.bumps * 2;
                        num = self.AddCosmetic(num, e);
                    }
                    Random.state = state;
                }
                
            }

            private static Color VanBodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
            {
                Color res = orig(self, f);
                
                if (self.lizard.Template.type == VanillaLizard.vanillaLizard)
                {
                    res = new Color(0.4667f, 0.3333f, 0.2f);
                }
                return res;

            }

            private static Color VanHeadColor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
            {
                Color res = orig(self, timeStacker);
                
                if(self.lizard.Template.type == VanillaLizard.vanillaLizard)
                {
                    //
                    if (self.whiteFlicker > 0 && (self.whiteFlicker > 15 || self.everySecondDraw))
                    {
                        return new Color();
                    }
                    float num = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBlink, self.blink, timeStacker) * 2f * 3.1415927f), 1.5f + self.lizard.AI.excitement * 1.5f);
                    if (self.headColorSetter != 0f)
                    {
                        num = Mathf.Lerp(num, (self.headColorSetter > 0f) ? 1f : 0f, Mathf.Abs(self.headColorSetter));
                    }
                    if (self.flicker > 10)
                    {
                        num = self.flickerColor;
                    }
                    num = Mathf.Lerp(num, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(self.lastVoiceVisualization, self.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(self.lastVoiceVisualizationIntensity, self.voiceVisualizationIntensity, timeStacker));
                    
                }
                return res;
            }

            private static void VanTailFin(On.LizardCosmetics.TailFin.orig_DrawSprites orig, LizardCosmetics.TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                orig(self, sLeaser, rCam, timeStacker, camPos);
                if (self.lGraphics.lizard.Template.type == VanillaLizard.vanillaLizard)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int num = i * self.bumps;
                        for (int j = self.startSprite; j < self.startSprite + self.bumps; j++)
                        {
                            float f = Mathf.Lerp(0.05f, self.spineLength / self.lGraphics.BodyAndTailLength, Mathf.InverseLerp((float)self.startSprite, (float)(self.startSprite + self.bumps - 1), (float)j));
                            sLeaser.sprites[j + num].color = self.lGraphics.BodyColor(f);
                        }

                    }
                }
            }

            private static void LurkTracker_Update(On.LizardAI.LurkTracker.orig_Update orig, LizardAI.LurkTracker self)
            {
                orig(self);
                if (self.AI.creature.creatureTemplate.type == VanillaLizard.vanillaLizard)
                {
                    self = new LizardAI.LurkTracker(self.AI, self.lizard);

                }

            }
        
        private static CreatureTemplate VanBreed(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
            if (type == VanillaLizard.vanillaLizard)
            {
                
                var temp = orig(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate); //Sets up the parameters to use Black Lizard’s parameters as a base.
                var breedparams = (temp.breedParameters as LizardBreedParams);
                temp.type = type; //set the type to our creature’s type
                temp.name = "Vanilla Lizard";
                breedparams.tailSegments = Random.Range(3, 5);
                breedparams.standardColor = new(0.5255f, 0.3212f, 0.1533f);
                //breedparams.tailColorationStart = 0f;
                temp.roamInRoomChance = 4f;
                temp.roamBetweenRoomsChance = 4f;
                temp.wormgrassTilesIgnored = true;
                temp.AccessibilityResistance(AItile.Accessibility.Climb);
                temp.doPreBakedPathing = false;
                temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SpitLizard); //sets up pathing to use the same pathing as eels
                //temp.lungCapacity = 200;
                temp.jumpAction = "Lounge";
                temp.bodySize = 2f;
                temp.requireAImap = true;
                breedparams.limbThickness = 1.7f;
                breedparams.limbSize = 1.5f;
                temp.interestInOtherAncestorsCatches = 5f;
                temp.interestInOtherCreaturesCatches = 10f;
                breedparams.baseSpeed = 6f;
                breedparams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedparams.swimSpeed = 1.2f;
                
                breedparams.loungeDelay = 10;
                breedparams.loungeDistance = 1500;
                breedparams.loungeSpeed = 1.5f;
                breedparams.preLoungeCrouch = 10;
                breedparams.loungeTendensy = 100f;
                breedparams.tailColorationExponent = 1f;
                breedparams.tamingDifficulty = 1.6f;
                breedparams.stepLength = 1.5f;
                breedparams.danger = 0.8f;
                breedparams.bodyMass = 1.5f;
                breedparams.headSize = 1.02f;                
                breedparams.bodySizeFac = 1.02f;
                breedparams.bodyLengthFac = 1.3f;
                breedparams.bodyStiffnes = 1f;
                breedparams.shakePrey = 3;
                breedparams.biteChance = 0.8f;
                breedparams.tongueWarmUp = 3;
                breedparams.tongueAttackRange = 3;
                breedparams.tongue = true;
                breedparams.tongueSegments = 3;
                temp.dangerousToPlayer = breedparams.danger;
                temp.visualRadius = 200f;
                temp.throwAction = "Spit";
                temp.socialMemory = true;


                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        }
    }

        public class VanLizCritob : Critob
    {
        public VanLizCritob() : base(VanillaLizard.vanillaLizard)
        {
            Icon = new SimpleIcon("Kill_Spit_Lizard", new Color(0.8f, 0.7f, 0.6f));
            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.VLiz);
            //abysshooks hooks = new abysshooks();
            VanHooks.Hooks();
        }


        public override int ExpeditionScore()
        {
            return 4;
        }

        public override Color DevtoolsMapColor(AbstractCreature acrit)
        {
            return Color.gray;
        }

        public override string DevtoolsMapName(AbstractCreature acrit)
        {
            return "VnL";
        }

        public override IEnumerable<string> WorldFileAliases()
        {
            return new[] { "VanillaL" };
        }

        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.Lizards, RoomAttractivenessPanel.Category.LikesWater };

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new LizardAI(acrit, acrit.world);

        }
        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new Lizard(acrit, acrit.world);
        }
        public override CreatureState CreateState(AbstractCreature acrit)
        {
            return new LizardState(acrit);
        }

        public override CreatureTemplate CreateTemplate()
        {
            return LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
        }

        public override void EstablishRelationships()
        {
            var s = new Relationships(Type);
            s.Rivals(CreatureTemplate.Type.LizardTemplate, 0.02f);

            s.PlaysWith(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 1f);
            
            s.IsInPack(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 1f);
            

            s.Antagonizes(CreatureTemplate.Type.BigNeedleWorm, 1f);

            s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);

            s.Fears(CreatureTemplate.Type.Vulture, .9f);
            s.Fears(CreatureTemplate.Type.KingVulture, 1f);
            s.Fears(CreatureTemplate.Type.RedCentipede, 1f);
            s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
            s.Fears(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, 1f);
            s.Fears(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
            s.Fears(CreatureTemplate.Type.PoleMimic, 0.5f);
            s.Fears(CreatureTemplate.Type.TentaclePlant, 1f);

            s.FearedBy(CreatureTemplate.Type.BigEel, .7f);
            s.FearedBy(CreatureTemplate.Type.Scavenger, 1f);
            s.FearedBy(CreatureTemplate.Type.Centipede, 1f);
            s.FearedBy(CreatureTemplate.Type.BigNeedleWorm, .8f);
            s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, .8f);

            s.Eats(CreatureTemplate.Type.Scavenger, .8f);
            s.Eats(CreatureTemplate.Type.Centipede, 1f);
            s.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
            s.Eats(CreatureTemplate.Type.Hazer, 1f);
            s.Eats(CreatureTemplate.Type.BigEel, .7f);
            s.Eats(CreatureTemplate.Type.CicadaA, .05f);
            s.Eats(CreatureTemplate.Type.LanternMouse, .3f);
            s.Eats(CreatureTemplate.Type.BigSpider, .35f);
            s.Eats(CreatureTemplate.Type.EggBug, .45f);
            s.Eats(CreatureTemplate.Type.JetFish, .1f);
            s.Eats(CreatureTemplate.Type.Centiwing, 0.5f);
            s.Eats(CreatureTemplate.Type.DropBug, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);

            s.EatenBy(CreatureTemplate.Type.BigSpider, .3f);
            s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
            s.EatenBy(CreatureTemplate.Type.Vulture, 1f);
            s.EatenBy(CreatureTemplate.Type.KingVulture, 1f);
            s.EatenBy(CreatureTemplate.Type.SpitterSpider, 1f);
            s.EatenBy(CreatureTemplate.Type.PoleMimic, 0.5f);
            s.EatenBy(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, 0.5f);
            s.EatenBy(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
            
        }
    }
        
}

        

        
        
    

