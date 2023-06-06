using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

namespace Guide
{
    static class GuideGills
    {
        public static readonly PlayerFeature<bool> gilled = PlayerBool("gilled");
        
        public static void Hooks()
        {
            On.PlayerGraphics.ctor += GuideGillsSetup;
            On.PlayerGraphics.InitiateSprites += GuideGillsinit;
            On.PlayerGraphics.AddToContainer += GillsContainer;
            On.PlayerGraphics.DrawSprites += GillDraw;
            On.PlayerGraphics.Update += GillsUpdate;
        }

        
        private static void GuideGillsinit(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (gilled.TryGet(self.player, out bool value) && value)
            {
                int SpriteIndex = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, SpriteIndex + 8);
                

                gillstorage.TryGetValue(self.player, out var thedata);
                thedata.initialgillloc = sLeaser.sprites.Length;
                thedata.initialgillloc = sLeaser.sprites.Length + 8;
                thedata.ready = true;
                //self.AddToContainer(sLeaser, rCam, null);

                sLeaser.sprites[SpriteIndex + 1] = new FSprite(thedata.sprite);
                sLeaser.sprites[SpriteIndex + 2] = new FSprite(thedata.sprite);


                /*for (int i = 0; 1 < 2; i++ )
                {
                    for (int j = 0; j < 3; j++ )
                    {
                        sLeaser.sprites[thedata.Gillsprite(i, j)] = new FSprite(thedata.sprite);
                        sLeaser.sprites[thedata.Gillsprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y;
                        sLeaser.sprites[thedata.Gillsprite(i, j)].anchorY = 0.1f;
                    }
                }*/


            }
            Debug.Log("GILLS GUIDE CHECK FAILED!");
            return;
        }

        public static ConditionalWeakTable<Player, Gilldata> gillstorage = new ConditionalWeakTable<Player, Gilldata>();

        public class Gilldata
        {
            public bool ready = false;
            public int initialgillloc;
            public string sprite = "LizardScaleA0";
            public WeakReference<Player> playerref;
            public Gilldata(Player player)
            {
                playerref = new WeakReference<Player>(player);
            }
            public UnityEngine.Vector2[] headpositions = new UnityEngine.Vector2[8];
            public FaceGills[] gills = new FaceGills[8];

            public class FaceGills : BodyPart
            {
                public FaceGills(GraphicsModule cosmetics) : base(cosmetics)
                {

                }
                public override void Update()
                {
                    base.Update();
                    if (this.owner.owner.room.PointSubmerged(this.pos))
                    {
                        this.vel *= 0.5f;
                    }
                    else
                    {
                        this.vel *= 0.9f;
                    }
                    this.lastPos = this.pos;
                    this.pos += this.vel;
                }
                public float length = 5f;
                public float width = 1f;
            }
            public UnityEngine.Color gillscolor = new UnityEngine.Color(1f, 1f, 1f);

            public int Gillsprite(int side, int pair)
            {
                return initialgillloc + side + pair + pair;
            }
        }


        private static void GillsUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (gilled.TryGet(self.player, out bool value) && value && gillstorage.TryGetValue(self.player, out Gilldata data))
            {
                int index = 0;
                for(int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 pos = self.owner.bodyChunks[0].pos; 
                        Vector2 pos2 = self.owner.bodyChunks[1].pos;
                        float num = 0f;
                        float num2 = 90f;
                        int num3 = index % (data.gills.Length / 2);
                        float num4 = num2 / (float)(data.gills.Length / 2);
                        if (i == 1)
                        {
                            num = 0f;
                            pos.x += 5f;
                        }
                        else
                        {
                            pos.x -= 5f;
                        }
                        Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
                        float f = Custom.VecToDeg(self.lookDirection);
                        Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
                        Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));
                        if (data.headpositions[index].y < 0.2f)
                        {
                            a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, data.headpositions[index].y), 2f) * 2f;
                        }
                        a2 = Vector2.Lerp(a2, vector, Mathf.Pow(0.0875f, 1f)).normalized;
                        Vector2 vector2 = pos + a2 * data.gills.Length;
                        if (!Custom.DistLess(data.gills[index].pos, vector2, data.gills[index].length / 2f))
                        {
                            Vector2 a3 = Custom.DirVec(data.gills[index].pos, vector2);
                            float num5 = Vector2.Distance(data.gills[index].pos, vector2);
                            float num6 = data.gills[index].length / 2f;
                            data.gills[index].pos += a3 * (num5 - num6);
                            data.gills[index].vel += a3 * (num5 - num6);
                        }
                        data.gills[index].vel += Vector2.ClampMagnitude(vector2 - data.gills[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
                        data.gills[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
                        data.gills[index].ConnectToPoint(pos, data.gills[index].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
                        data.gills[index].Update();
                        index++;
                    }
                }
            }
        }

        private static void GillDraw(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig (self, sLeaser, rCam, timeStacker, camPos);
            if (gilled.TryGet(self.player, out bool value) && value && self.player.room != null && gillstorage.TryGetValue(self.player, out Gilldata data))
            {
                int index = 0;
                for (int i = 0; i < 4; i++) //as i said before, basically just rivy's code.
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 vector = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
                        float f = 0f;
                        float num = 0f;
                        if (i == 0)
                        {
                            vector.x -= 5f;
                        }
                        else
                        {
                            num = 180f;
                            vector.x += 5f;
                        }
                        sLeaser.sprites[data.Gillsprite(i, j)].x = vector.x - camPos.x;
                        sLeaser.sprites[data.Gillsprite(i, j)].y = vector.y - camPos.y;
                        sLeaser.sprites[data.Gillsprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(data.gills[index].lastPos, data.gills[index].pos, timeStacker)) + num;
                        sLeaser.sprites[data.Gillsprite(i, j)].scaleX = 0.4f * Mathf.Sign(f);
                        sLeaser.sprites[data.Gillsprite(i, j)].color = sLeaser.sprites[1].color;
                        index++;
                    }
                }
            }
        }

        private static void GillsContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (gilled.TryGet(self.player, out bool value) && value && gillstorage.TryGetValue(self.player, out Gilldata data) && data.ready)
            {
                FContainer container = rCam.ReturnFContainer("Midground");
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        FSprite exgill = sLeaser.sprites[data.Gillsprite(i,j)];
                        container.AddChild(exgill);
                        
                    }
                    
                }
                data.ready = false;
            }
        }

        private static void GuideGillsSetup(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (gilled.TryGet(self.player, out bool value) && value)
            {
                gillstorage.Add(self.player, new Gilldata(self.player));
                gillstorage.TryGetValue(self.player, out Gilldata data);
                for (int i = 0; i < data.gills.Length; i++)
                {
                    data.gills[i] = new Gilldata.FaceGills(self);
                    data.headpositions[i] = new Vector2((i < data.gills.Length / 2 ? 0.7f : -0.7f), i == 1 ? 0.035f : 0.026f);
                }
            }
        }
    }
}
