using System;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using System.Linq;
using Unity.Mathematics;

namespace Guide.Creatures
{
    internal class KelpTweaks
    {
        public KelpTweaks() { }

        public static void Hooks()
        {
            On.TentaclePlantGraphics.ApplyPalette += TentaclePlantGraphics_ApplyPalette;
            On.TentaclePlantGraphics.DrawSprites += TentaclePlantGraphics_DrawSprites;
            On.TentaclePlantGraphics.InitiateSprites += TentaclePlantGraphics_InitiateSprites;
            On.TentaclePlantGraphics.Update += TentaclePlantGraphics_Update;
            On.TentaclePlantGraphics.ctor += TentaclePlantGraphics_ctor;
        }

        private static void TentaclePlantGraphics_ctor(On.TentaclePlantGraphics.orig_ctor orig, TentaclePlantGraphics self, PhysicalObject ow)
        {
            if (self.plant.GetCrit().isMonster)
            {
                self.danglers = new Dangler[70];
            }
        }

        private static void TentaclePlantGraphics_Update(On.TentaclePlantGraphics.orig_Update orig, TentaclePlantGraphics self)
        {
            if (self.plant.GetCrit().isMonster)
            {
                
            }
        }

        private static void TentaclePlantGraphics_InitiateSprites(On.TentaclePlantGraphics.orig_InitiateSprites orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            throw new NotImplementedException();
        }

        private static void TentaclePlantGraphics_DrawSprites(On.TentaclePlantGraphics.orig_DrawSprites orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            throw new NotImplementedException();
        }

        private static void TentaclePlantGraphics_ApplyPalette(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            throw new NotImplementedException();
        }
    }
}
