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
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;

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
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.Reset += PlayerGraphics_Reset;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            // ?
        }

        public static float MED_TENTACLE_GRAVITY = 0f;

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (self.IsMedium(out var med))
            {
                self.gills?.Update();
                med.topGills?.Update();

                for (int j = 0; j < med.tentacles.Length; j++)
                {
                    Vector2 b = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], 1f);
                    Vector2 a = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], 1f);
                    Vector2 vector = (a * 3f + b) / 4f;
                    float num = 1f;
                    float num2 = 28f;
                    Vector2 pos = j < 4? self.owner.bodyChunks[0].pos : self.owner.bodyChunks[1].pos;
                    var chunk = j < 4 ? self.owner.bodyChunks[1] : self.owner.bodyChunks[0];
                    Vector2 vector2 = j < 4 ? self.owner.bodyChunks[1].pos : self.owner.bodyChunks[0].pos;
                    for (int i = 0; i < med.tentacles[i].segments.Length; i++)
                    {
                        Vector2 ConnectedPoint = pos;

                        if (med.tentacles[j] is MedFaceTentacle faceTentacle)
                        {
                            var faceLoc = new Vector2(3f, 4f) + faceTentacle.FacePos + faceTentacle.CamPos;//top tentacle
                            if (j >= 5)
                            {
                                faceLoc = new Vector2(0f, 5f);
                            }

                            ConnectedPoint = faceLoc;
                        }
                        else if (med.tentacles[j] is MedTailTentacle tailTentacle)
                        {
                            var tailLoc = Custom.PerpendicularVector(Custom.DirVec(self.tail[tailTentacle.fromSeg].pos, self.tail[tailTentacle.toSeg].pos)) * (((self.tail[tailTentacle.fromSeg].rad + self.tail[tailTentacle.toSeg].rad) / 2f) * tailTentacle.outerOffset);
                            ConnectedPoint = ((self.tail[tailTentacle.fromSeg].pos + self.tail[tailTentacle.toSeg].pos) / 2f) + tailLoc;
                        }

                        var tentacle = med.tentacles[i].segments[i];
                        med.tentacles[i].segments[0].connectedPoint = new Vector2?(ConnectedPoint);
                        tentacle.Update();
                        tentacle.vel *= Mathf.Lerp(0.75f, 0.95f, num * (1f - chunk.submersion));
                        tentacle.vel.y -= Mathf.Lerp(0.1f, 0.5f, num) * (1f - chunk.submersion) * MED_TENTACLE_GRAVITY;
                        num = (num * 10f + 1f) / 11f;
                        bool flag3 = !Custom.DistLess(tentacle.pos, chunk.pos, 9f * (float)(i + 1));
                        if (flag3)
                        {
                            tentacle.pos = chunk.pos + Custom.DirVec(chunk.pos, tentacle.pos) * 9f * (float)(i + 1);
                        }
                        tentacle.vel += Custom.DirVec(vector2, tentacle.pos) * num2 / (Vector2.Distance(vector2, tentacle.pos) + 0.1f);
                        num2 *= 0.5f;
                        vector2 = pos;
                        pos = tentacle.pos;
                    }
                }
            }
        }

        private static void PlayerGraphics_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            orig(self);
            if (self.IsMedium(out var med))
            {
                for (int i = 0; i < med.tentacles.Length; i++)
                {
                    for (int j = 0; j < med.tentacles[i].segments.Length; j++)
                    {
                        med.tentacles[i].segments[j].Reset(self.player.firstChunk.pos);
                    }
                }
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (self.IsMedium(out var med))
            {
                med.tentacles = new MedTentacle[7];

                var bp = self.bodyParts.ToList();
                bp.RemoveAll(x => x is TailSegment);
                bp.AddRange(self.tail);

                for (int i = 0; i < med.tentacles.Length; i++)
                {
                    var tentacle = new TailSegment[3];

                    tentacle[0] = new(self, 1f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
                    tentacle[1] = new(self, 3f, 6.0f, tentacle[0], 0.85f, 1.0f, 0.5f, true);
                    tentacle[2] = new(self, 1f, 4.0f, tentacle[1], 0.85f, 1.0f, 0.5f, true);

                    bp.AddRange(tentacle);

                    if (i < 4)
                    {
                        if (i == 0) med.tentacles[i] = new MedTailTentacle(tentacle, 1, 1, 0f, 0.8f);
                        else if (i == 1) med.tentacles[i] = new MedTailTentacle(tentacle, 1, 2, 0.5f, -0.8f);
                        else if (i == 2) med.tentacles[i] = new MedTailTentacle(tentacle, 2, 2, 0f, 0.1f);
                        else med.tentacles[i] = new MedTailTentacle(tentacle, 3, 3, 0f, -0.1f);
                    }
                    else
                    {
                        var offset = new Vector2(3f, 4f);//top tentacle
                        if (i >= 5)
                        {
                            offset = new Vector2(0f, 5f);
                        }

                        med.tentacles[i] = new MedFaceTentacle(tentacle, default, default, offset);
                    }

                }
                self.bodyParts = bp.ToArray();
            }
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

            for (int i = 0; i < medium.tentacles.Length; i++)
            {
                if (medium.tentacles[i] is MedFaceTentacle tent)
                {
                    tent.FacePos = sLeaser.sprites[9].GetPosition();
                    tent.CamPos = camPos;
                }

                Vector2 ConnectedPoint = self.player.bodyChunks[0].pos;

                if (medium.tentacles[i] is MedFaceTentacle faceTentacle)
                {
                    var faceLoc = new Vector2(3f, 4f) + faceTentacle.FacePos + faceTentacle.CamPos;//top tentacle
                    if (i >= 5)
                    {
                        faceLoc = new Vector2(0f, 5f);
                    }

                    ConnectedPoint = faceLoc;
                }
                else if (medium.tentacles[i] is MedTailTentacle tailTentacle)
                {
                    var tailLoc = Custom.PerpendicularVector(Custom.DirVec(self.tail[tailTentacle.fromSeg].pos, self.tail[tailTentacle.toSeg].pos)) * (((self.tail[tailTentacle.fromSeg].rad + self.tail[tailTentacle.toSeg].rad) / 2f) * tailTentacle.outerOffset);
                    ConnectedPoint = ((self.tail[tailTentacle.fromSeg].pos + self.tail[tailTentacle.toSeg].pos) / 2f) + tailLoc;
                }

                DrawTentacle(sLeaser, timeStacker, camPos, medium.tentacles[i].segments, medium.tentacles[i].sprite, ConnectedPoint, 1);
            }

            
        }
        //copied from Pearlcat, what could go wrong?
        public static void DrawTentacle(RoomCamera.SpriteLeaser sLeaser, float timestacker, Vector2 camPos, TailSegment[]? ear, int earSprite, Vector2 attachPos, int earFlipDirection)
        {
            if (ear == null || ear.Length == 0) return;

            if (sLeaser.sprites[earSprite] is not TriangleMesh earMesh) return;

            // Draw Mesh
            var earRad = ear[0].rad;

            for (var segment = 0; segment < ear.Length; segment++)
            {
                var earPos = Vector2.Lerp(ear[segment].lastPos, ear[segment].pos, timestacker);


                var normalized = (earPos - attachPos).normalized;
                var perpendicularNormalized = Custom.PerpendicularVector(normalized);

                var distance = Vector2.Distance(earPos, attachPos) / 5.0f;

                if (segment == 0)
                    distance = 0.0f;

                earMesh.MoveVertice(segment * 4, attachPos - earFlipDirection * perpendicularNormalized * earRad + normalized * distance - camPos);
                earMesh.MoveVertice(segment * 4 + 1, attachPos + earFlipDirection * perpendicularNormalized * earRad + normalized * distance - camPos);

                if (segment >= ear.Length - 1)
                {
                    earMesh.MoveVertice(segment * 4 + 2, earPos - camPos);
                }
                else
                {
                    earMesh.MoveVertice(segment * 4 + 2, earPos - earFlipDirection * perpendicularNormalized * ear[segment].StretchedRad - normalized * distance - camPos);
                    earMesh.MoveVertice(segment * 4 + 3, earPos + earFlipDirection * perpendicularNormalized * ear[segment].StretchedRad - normalized * distance - camPos);
                }

                earRad = ear[segment].StretchedRad;
                attachPos = earPos;
            }
        }
        public static void GenerateTentacleMesh(RoomCamera.SpriteLeaser sLeaser, TailSegment[]? ear, int earSprite)
        {
            if (ear == null) return;

            int earMeshTriesLength = (ear.Length - 1) * 4;
            var earMeshTries = new TriangleMesh.Triangle[earMeshTriesLength + 1];

            for (int i = 0; i < ear.Length - 1; i++)
            {
                int indexTimesFour = i * 4;

                for (int j = 0; j <= 3; j++)
                {
                    earMeshTries[indexTimesFour + j] = new TriangleMesh.Triangle(indexTimesFour + j, indexTimesFour + j + 1, indexTimesFour + j + 2);
                }
            }

            earMeshTries[earMeshTriesLength] = new TriangleMesh.Triangle(earMeshTriesLength, earMeshTriesLength + 1, earMeshTriesLength + 2);
            sLeaser.sprites[earSprite] = new TriangleMesh("Futile_White", earMeshTries, false, false);
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

            for (int j = 0; j < medium.tentacles.Length; j++)
            {
                newContatiner.AddChild(sLeaser.sprites[medium.tentacles[j].sprite]);
                if (medium.tentacles[j] is MedFaceTentacle)
                {
                    sLeaser.sprites[medium.tentacles[j].sprite].MoveBehindOtherNode(sLeaser.sprites[medium.tentacles[j].sprite]);
                }
                else
                {
                    sLeaser.sprites[medium.tentacles[j].sprite].MoveInFrontOfOtherNode(sLeaser.sprites[2]);
                }
            }

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

            for (int i = 0; i < medium.tentacles.Length; i++)
            {
                medium.tentacles[0].sprite = sLeaser.sprites.Length + 5 + i + 1;
            }

            medium.topGills = new UpperHavenGills(self, sLeaser.sprites.Length + 13);
            self.gills = new LowerHavenGills(self, medium.topGills.startSprite + medium.topGills.numberOfSprites, true);

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 6 + self.gills.numberOfSprites + medium.topGills.numberOfSprites + 7);

            for (int j = 0; j < medium.tentacles.Length; j++)
            {
                GenerateTentacleMesh(sLeaser, medium.tentacles[j].segments, medium.tentacles[j].sprite);
            }

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

            const int length = 3;//Uhh btw slugpups still have the same number of tailsegments, so using 3 here is wrong
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
