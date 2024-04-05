using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guide.Creatures
{
    internal class CentiKing
    {
        public CentiKing() { }

        public static void Hooks()
        {
            //On.CentipedeGraphics.InitiateSprites += CentipedeGraphics_InitiateSprites;
            On.CentipedeGraphics.ApplyPalette += CentipedeGraphics_ApplyPalette;
            //On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
            On.Centipede.ctor += Centipede_ctor;
        }

        private static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.GetCrit().isMonster && self.Centiwing)
            {
                self.bodyChunks = new BodyChunk[22];
                
            }
        }

        

        private static void CentipedeGraphics_ApplyPalette(On.CentipedeGraphics.orig_ApplyPalette orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if(self.centipede.GetCrit().isMonster && self.centipede.Centiwing)
            {
                for (int i = 0; i < self.totalSecondarySegments; i++)
                {
                    sLeaser.sprites[self.SecondarySegmentSprite(i)].color = new UnityEngine.Color(0.91f, 0.3f, 0.69f);
                }
            }
        }

        /*private static void CentipedeGraphics_InitiateSprites(On.CentipedeGraphics.orig_InitiateSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (self.centipede.Centiwing && self.centipede.GetCrit().isMonster)
            {
                sLeaser.sprites[self.wing]
            }
        }*/
    }
}
