using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;
using MoreSlugcats;


namespace Guide.Creatures
{
    internal class ScrufflingCritob : Critob
    {
        public static readonly CreatureType Scruffling = new("Scruffling", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID ScrufflingUnlock = new("Scruffling", true);

        public ScrufflingCritob() : base(Scruffling)
        {
            LoadedPerformanceCost = 20f;
            SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
            CreatureName = "Scruffling";

            RegisterUnlock(killScore: KillScore.Configurable(2), ScrufflingUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.Scavenger, data: 0);
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new ScrufflingAI(acrit, (Scruffling)acrit.realizedCreature);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new Scruffling(acrit);
        }

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate t = new CreatureFormula(this)
            {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 1f),
                HasAI = true,
                InstantDeathDamage = 1,
                Pathing = PreBakedPathing.Ancestral(CreatureType.Scavenger),

                ConnectionResistances = new()
                {
                    Standard = new(1, Allowed),
                    ShortCut = new(1, Allowed),
                    NPCTransportation = new(1, Allowed),
                    OffScreenMovement = new(1, Allowed),
                    BetweenRooms = new(1, Allowed),
                },               

            }.IntoTemplate();

            t.smallCreature = true;
            t.hibernateOffScreen = true;
            t.lungCapacity = 0.25f;
            t.saveCreature = true;
            t.bodySize = 0.2f;
            t.quickDeath = true;
            t.meatPoints = 1;
            t.visualRadius = 400f;
            t.usesNPCTransportation = true;
            t.grasps = 2;

            return t;
        }

        public override void EstablishRelationships()
        {
            Relationships r = new(Scruffling);

            

            r.IsInPack(CreatureType.Scavenger, 1f);

            //eats batflies, infant noodle fly, infant centi
            r.Eats(CreatureType.Fly, 0.5f);
            r.Eats(CreatureType.SmallNeedleWorm, 0.7f);
            r.Eats(CreatureType.SmallCentipede, 0.8f);
            r.Eats(CreatureType.VultureGrub, 1f);

            //Afraid of, all lizards, vultures, player (if not guide), miros, dlls, spiders
            r.Fears(CreatureType.LizardTemplate, 1f);
            r.Fears(CreatureType.Vulture, 1f);
            r.Fears(CreatureType.KingVulture, 1f);
            r.Fears(CreatureType.MirosBird, 1f);
            r.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
            r.Fears(CreatureType.Spider, 1f);
            r.Fears(CreatureType.BigSpider, 1f);
            r.Fears(CreatureType.SpitterSpider, 1f);
        

            //eaten By ^
            r.EatenBy(CreatureType.LizardTemplate, 1f);
            r.EatenBy(CreatureType.Vulture, 1f);
            r.EatenBy(CreatureType.KingVulture, 1f);
            r.EatenBy(CreatureType.MirosBird, 1f);
            r.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);

        }
    }

    sealed class Scruffling : Scavenger, IPlayerEdible
    {
        int bites = 2;
        public int BitesLeft => bites;

        public int FoodPoints => 2;

        public bool Edible => true;

        public bool AutomaticPickUp => false;

        

        public void BitByPlayer(Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound(bites == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }

        public void ThrowByPlayer(){}

        public Scruffling(AbstractCreature acrit) : base(acrit, acrit.world)
        {
            //should inherit all of the funky stuff from scavs??
        }

        
    }

    sealed class ScrufflingProperties : ItemProperties
    {
        private readonly Scruffling scruffling;
        public ScrufflingProperties(Scruffling scruffling)
        {
            this.scruffling = scruffling;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }

        public override void Nourishment(Player player, ref int quarterPips)
        {
            if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                quarterPips = -1;
            }
            else
            {
                quarterPips = 4 * scruffling.FoodPoints;
            }
        }
    }

    sealed class ScrufflingAI : ScavengerAI
    {
        public ScrufflingAI(AbstractCreature acrit, Scruffling scruff) : base(acrit, acrit.world)
        {
            //should inherit from scavAI?
        }
    }

    sealed class ScrufflingGraphics : ScavengerGraphics
    {

        public ScrufflingGraphics(Scruffling scruff) : base(scruff)
        {

        }

        public static void Hooks()
        {
            On.ScavengerGraphics.InitiateSprites += ScavengerGraphics_InitiateSprites;
            On.ScavengerGraphics.Update += ScavengerGraphics_Update;
            On.ScavengerGraphics.DrawSprites += TinyBabie;
            On.ScavengerGraphics.ScavengerHand.ctor += ScavengerHand_ctor;
            On.ScavengerGraphics.ScavengerLeg.ctor += ScavengerLeg_ctor;
        }

        private static void ScavengerLeg_ctor(On.ScavengerGraphics.ScavengerLeg.orig_ctor orig, ScavengerLeg self, ScavengerGraphics owner, int num, int firstSprite)
        {
            orig(self, owner, num, firstSprite);
            if(self.scavenger is Scruffling)
            {
                self.legLength *= 0.5f;
            }
        }

        private static void ScavengerHand_ctor(On.ScavengerGraphics.ScavengerHand.orig_ctor orig, ScavengerHand self, ScavengerGraphics owner, int num, int firstSprite)
        {
            orig(self, owner, num, firstSprite);
            if(self.scavenger is Scruffling)
            {
                self.armLength *= 0.5f;
                
            }
        }

        private static void ScavengerGraphics_Update(On.ScavengerGraphics.orig_Update orig, ScavengerGraphics self)
        {
            orig(self);
            if(self.scavenger is Scruffling)
            {
                self.spineLengths[0] *= 0.5f;
            }
            
        }

        private static void ScavengerGraphics_InitiateSprites(On.ScavengerGraphics.orig_InitiateSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.scavenger is Scruffling)
            {                
                float mySize = 0.5f;
                sLeaser.sprites[self.ChestSprite].scaleX *= 0.85f;
                sLeaser.sprites[self.ChestSprite].scaleY *= mySize;
                sLeaser.sprites[self.HipSprite].scale *= mySize;
                sLeaser.sprites[self.HeadSprite].scale *= 0.85f;
            }
            
        }

        private static void TinyBabie(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if(self.scavenger is Scruffling)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[self.EyeSprite(j, 0)].scaleX *= 1.35f;
                }
            }
        }
    }
}
