using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

using SlugBase.DataTypes;
using static SlugBase.Features.FeatureTypes;
using Guide.WorldChanges;
using Guide.Creatures;
using Guide.Objects;
using Guide.Guide;
using Guide.Medium;
using SlugBase.Features;

namespace Guide.Medium
{
    internal class MediumGraphics
    {
        public static void Hooks()
        {
            //ctor
            On.Player.ctor += Medium_ctor;
            //On.PlayerGraphics.ctor += PlayerGraphics_ctor;


            //InitSprites
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            //DrawSprites
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            //AddToContainer
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            // ?
        }

        private const string SpritePrefix_ = "MediumSprites_";

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!self.IsMedium(out var medium)) return;

            sLeaser.sprites[medium.ArmEchoSprite].Follow(sLeaser.sprites[6]);
            sLeaser.sprites[medium.ArmEchoSprite].color = medium.BlackColor;
            sLeaser.sprites[8].color = medium.BlackColor;

            sLeaser.sprites[medium.HipsEchoSprite].Follow(sLeaser.sprites[1]); //
            sLeaser.sprites[medium.HipsEchoSprite].color = medium.BlackColor;

            sLeaser.sprites[medium.BodyEchoSprite].Follow(sLeaser.sprites[0]); //
            sLeaser.sprites[medium.BodyEchoSprite].color = medium.BlackColor;

            sLeaser.sprites[medium.LegEchoSprite].Follow(sLeaser.sprites[4]); //
            sLeaser.sprites[medium.LegEchoSprite].color = medium.BlackColor;

            sLeaser.sprites[medium.FaceBlushSprite].Follow(sLeaser.sprites[9]); //
            sLeaser.sprites[medium.FaceBlushSprite].color = medium.BlushColor;

            sLeaser.sprites[medium.FaceEchoSprite].Follow(sLeaser.sprites[3]); //
            sLeaser.sprites[medium.FaceEchoSprite].color = medium.EchoColor;

            self.gills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            medium.topGills?.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            self.gills?.SetGillColors(medium.BodyColor, medium.GillsColor);
            medium.topGills?.SetGillColors(medium.BodyColor, medium.GillsColor);

            self.gills?.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            medium.topGills?.ApplyPalette(sLeaser, rCam, rCam.currentPalette);

            
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            
            orig(self, sLeaser, rCam, newContatiner);
            if (!self.IsMedium(out var medium) || !medium.spritesReady) return;

            newContatiner ??= rCam.ReturnFContainer("Midground");

            newContatiner.AddChild(sLeaser.sprites[medium.FaceEchoSprite]);
            sLeaser.sprites[medium.FaceEchoSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);

            newContatiner.AddChild(sLeaser.sprites[medium.FaceBlushSprite]);
            sLeaser.sprites[medium.FaceBlushSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);

            newContatiner.AddChild(sLeaser.sprites[medium.ArmEchoSprite]);
            sLeaser.sprites[medium.ArmEchoSprite].MoveInFrontOfOtherNode(sLeaser.sprites[6]);

            newContatiner.AddChild(sLeaser.sprites[medium.BodyEchoSprite]);
            sLeaser.sprites[medium.BodyEchoSprite].MoveInFrontOfOtherNode(sLeaser.sprites[0]);

            newContatiner.AddChild(sLeaser.sprites[medium.LegEchoSprite]);
            sLeaser.sprites[medium.LegEchoSprite].MoveInFrontOfOtherNode(sLeaser.sprites[4]);

            newContatiner.AddChild(sLeaser.sprites[medium.HipsEchoSprite]);
            sLeaser.sprites[medium.HipsEchoSprite].MoveInFrontOfOtherNode(sLeaser.sprites[1]);

            for (int i = medium.topGills.startSprite; i < self.gills.startSprite + self.gills.numberOfSprites; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[9]);
            }

        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var isMedium = false;
            MediumStatusClass.MediumStatus medium = null;

            try
            {
                isMedium = self.IsMedium(out medium);
                if (isMedium)
                {
                    medium.spritesReady = false;
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            orig(self, sLeaser, rCam);
            if (!isMedium) return;

            medium.ArmEchoSprite = sLeaser.sprites.Length;
            medium.FaceEchoSprite = sLeaser.sprites.Length + 1; //black half with white dots near center, make color choice Echo color for this and the other accents
            medium.BodyEchoSprite = sLeaser.sprites.Length + 2; //Follow + MoveInFrontOfOtherNode for each sprite 
            medium.LegEchoSprite = sLeaser.sprites.Length + 3; //might do body spots sprite
            medium.HipsEchoSprite = sLeaser.sprites.Length + 4;
            medium.FaceBlushSprite = sLeaser.sprites.Length + 5;

            medium.topGills = new UpperHavenGills(self, sLeaser.sprites.Length + 6);
            self.gills = new LowerHavenGills(self, medium.topGills.startSprite + medium.topGills.numberOfSprites, true);

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 6 + self.gills.numberOfSprites + medium.topGills.numberOfSprites);

            //~~~~~~~~~~~~
            //Assign sprites
            sLeaser.sprites[medium.ArmEchoSprite] = new FSprite("pixel");
            sLeaser.sprites[medium.FaceEchoSprite] = new FSprite("pixel");
            sLeaser.sprites[medium.BodyEchoSprite] = new FSprite("pixel");
            sLeaser.sprites[medium.LegEchoSprite] = new FSprite("pixel");
            sLeaser.sprites[medium.HipsEchoSprite] = new FSprite("pixel");
            sLeaser.sprites[medium.FaceBlushSprite] = new FSprite("pixel");

            //medium.topGills.InitSprites here?
            medium.topGills.InitiateSprites(sLeaser, rCam);
            self.gills.InitiateSprites(sLeaser, rCam);

            medium.SetupColors();
            medium.spritesReady = true;
            self.AddToContainer(sLeaser, rCam, null);
        }

        /*Tail stuff, might come back to this later. I believe this is what's breaking Guide's tail and making it white instead of the body color, but I can
        * use this to make Medium's tail black.
        * For triangle mesh start positions, use tassel positions from Guide DrawSprites(). Maybe look at rag code? Want something that won't be affected
        * by gravity ideally
        */
        /*private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (!self.IsMedium(out var medium)) return;

            const int length = 3;
            const float wideness = 1.7f;
            const float roundness = 0.9f;

        }*/

        private static void Medium_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.GetMed().IsMedium)
            {
                self.setPupStatus(true);
            }
            
        }
    }
}
