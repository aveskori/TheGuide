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
    public class CherryHooks
    {
        public static void Hooks()
        {
            On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += CherryBreed;
            On.LizardCosmetics.TailFin.DrawSprites += CherryTailFin;
            On.LizardGraphics.HeadColor += CherryHeadColor;
            On.LizardGraphics.BodyColor += CherryBodyColor;
            On.LizardGraphics.ctor += CherryCosmetics;
            On.LizardVoice.GetMyVoiceTrigger += CherryVoice;
            On.LizardGraphics.ApplyPalette += CherryApplySprites;
            
            
        }


        private static void CherryApplySprites(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.lizard.Template.type == CustomTemplates.vanillaLizard)
            {
                self.ColorBody(sLeaser, new Color(0.9888f, 0.9098f, 0.8666f));
            }
        }

        private static SoundID CherryVoice(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
        {
            var res = orig(self);
            if (self.lizard is Lizard l && l.Template.type == CustomTemplates.vanillaLizard)
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

        private static void CherryCosmetics(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            //TailTufts, BodyStripes
            //BumpHawk, LongHeadScales, SpineSpikes, LongShounderScales, ShortBodyScales
            orig(self, ow);
            if (self.lizard.Template.type == CustomTemplates.vanillaLizard)
            {
                var state = Random.state;
                Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
                var num = self.startOfExtraSprites + self.extraSprites;

                num = self.AddCosmetic(num, new LizardCosmetics.JumpRings(self, num));
                num = self.AddCosmetic(num, new LizardCosmetics.LongShoulderScales(self, num));
                if (Random.value < 0.4f) 
                {
                    var e = new LizardCosmetics.SpineSpikes(self, num);
                    
                    num = self.AddCosmetic(num, e);
                }
                if(Random.value < 0.2f)
                {
                    var e = new LizardCosmetics.LongHeadScales(self, num);
                    e.colored = true;
                    
                    num = self.AddCosmetic(num, e);
                }
                if (Random.value < 0.8f)
                {
                    var e = new LizardCosmetics.BumpHawk(self, num);
                    
                    e.numberOfSprites = e.bumps * 2;
                    num = self.AddCosmetic(num, e);
                }
                Random.state = state;
            }
        }

        private static Color CherryBodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {
            Color res = orig(self, f);

            if (self.lizard.Template.type == CustomTemplates.vanillaLizard)
            {
                res = new Color(0.863f, 0.357f, 0.426f);
            }
            return res;
        }

        private static Color CherryHeadColor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
        {
            //0.851f, 0.086f, 0.192f
            Color res = orig(self, timeStacker);

            if (self.lizard.Template.type == CustomTemplates.vanillaLizard)
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

        private static void CherryTailFin(On.LizardCosmetics.TailFin.orig_DrawSprites orig, LizardCosmetics.TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.lGraphics.lizard.Template.type == CustomTemplates.cherryLizard)
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

        private static CreatureTemplate CherryBreed(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
            if (type == CustomTemplates.cherryLizard)
            {

                var temp = orig(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate); //Sets up the parameters to use Black Lizard’s parameters as a base.
                var breedparams = (temp.breedParameters as LizardBreedParams);
                temp.type = type; //set the type to our creature’s type
                temp.name = "Cherry Lizard";
                breedparams.tailSegments = Random.Range(10, 15);
                breedparams.standardColor = new(0.851f, 0.086f, 0.192f);
                //breedparams.tailColorationStart = 0f;
                temp.roamInRoomChance = 80f;
                temp.roamBetweenRoomsChance = 80f;
                temp.wormgrassTilesIgnored = true;
                temp.AccessibilityResistance(AItile.Accessibility.Climb);
                temp.doPreBakedPathing = false;
                temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard); //sets up pathing to use the same pathing as eels
                //temp.lungCapacity = 200;
                temp.jumpAction = "Lounge";
                temp.bodySize = 2f;
                temp.requireAImap = true;
                breedparams.limbThickness = 1.2f;
                breedparams.limbSize = 1.8f;
                temp.interestInOtherAncestorsCatches = 5f;
                temp.interestInOtherCreaturesCatches = 10f;
                breedparams.baseSpeed = 6f;
                breedparams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                breedparams.swimSpeed = 0.8f;
                
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
                breedparams.tongueAttackRange = 20;
                breedparams.tongue = true;
                breedparams.tongueSegments = 20;
                temp.dangerousToPlayer = breedparams.danger;
                temp.visualRadius = 250f;
                temp.throwAction = "Tongue";
                temp.socialMemory = true;


                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        }
    }

    public class ChrLizCritob : Critob
    {
        public ChrLizCritob() : base(CustomTemplates.cherryLizard)
        {
            Icon = new SimpleIcon("Kill_Spit_Lizard", new Color(0.85f, 0.20f, 0.45f));
            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.ChrLiz);
            CherryHooks.Hooks();
        }
        public override int ExpeditionScore()
        {
            return 5;
        }

        public override Color DevtoolsMapColor(AbstractCreature acrit)
        {
            return Color.magenta;
        }

        public override string DevtoolsMapName(AbstractCreature acrit)
        {
            return "ChrL";
        }

        public override IEnumerable<string> WorldFileAliases()
        {
            return new[] { "CherryL" }; 
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
            s.Rivals(CreatureTemplate.Type.CyanLizard, 1f);

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
            s.Eats(CreatureTemplate.Type.EggBug, .45f);
            s.Eats(CreatureTemplate.Type.JetFish, .1f);
            s.Eats(CreatureTemplate.Type.Centiwing, 0.5f);
            s.Eats(CreatureTemplate.Type.DropBug, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 1f);
            s.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);

            s.EatenBy(CreatureTemplate.Type.BigSpider, .35f);
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
