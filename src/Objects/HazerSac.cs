using System;
using UnityEngine;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;

namespace Guide.Objects
{
    internal class HazerSacFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrHSac = new("HazerSac", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID mHSac = new("HazerSac", true);

        public HazerSacFisobs() : base(AbstrHSac)
        {
            Icon = new SimpleIcon("Kill_Hazer", Color.gray);
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(mHSac, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
           
            
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5)
            {
                p = new string[5];
            }

            var result = new HazerSacAbstract(world, saveData.Pos, saveData.ID)
            {
                hue = float.TryParse(p[0], out var h) ? h : 0,
                saturation = float.TryParse(p[1], out var s) ? s : 1,
                scaleX = float.TryParse(p[2], out var x) ? x : 1,
                scaleY = float.TryParse(p[3], out var y) ? y : 1,
            };

            // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CrateIcon below).
            if (unlock is SandboxUnlock u)
            {
                result.hue = u.Data / 1000f;

                if (u.Data == 0)
                {
                    result.scaleX += 0.2f;
                    result.scaleY += 0.2f;
                }
            }

            return result;
        }

        private static readonly HazerSacProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return properties;
        }


    }  //Fisobs Class

    public class HazerSacAbstract : AbstractPhysicalObject
    {
        public HazerSacAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, HazerSacFisobs.AbstrHSac, null, pos, ID)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 0.5f;
            hue = 1f;
        }

        public override void Realize()
        {
            base.Realize();
            if(realizedObject == null)
            {
                realizedObject = new HazerSac(this);
            }
        }

        public float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;
        

        public override string ToString()
        {
            return this.SaveToString($"{{hue}};{{saturation}};{{scaleX}};{{scaleY}}");
        }
    }  //Abstract Class

    public class HazerSacIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is HazerSacAbstract hsac ? (int)(hsac.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "icon_HazerSac";
        }
    }

    public class HazerSac : DartMaggot
    {

        
        public static void Hooks()
        {
            
            On.DartMaggot.ApplyPalette += HazerSac_ApplyPalette;
            On.DartMaggot.NormalUpdate += HazerSac_NormalUpdateFix;
        }

        private static void HazerSac_NormalUpdateFix(On.DartMaggot.orig_NormalUpdate orig, DartMaggot self)
        {
            
            if(self is HazerSac)
            {
                self.age = 0f;
            }
            orig(self);
        }

        

        private static void HazerSac_ApplyPalette(On.DartMaggot.orig_ApplyPalette orig, DartMaggot self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color inkColor = new Color(0.147f, 0.015f, 0.259f);
            orig(self, sLeaser, rCam, palette);
            if(self is HazerSac)
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    for(int j = 0; j < (sLeaser.sprites[i] as TriangleMesh).verticeColors.Length; j++)
                    {
                        (sLeaser.sprites[i] as TriangleMesh).verticeColors[j] = inkColor;
                    }
                }
            }
                       
        }

       

        public HazerSac(HazerSacAbstract abstr) : base(abstr)
        {
            
            /*float mass = 10f;
            var positions = new List<Vector2>();

            positions.Add(Vector2.zero);
            bodyChunks = new BodyChunk[positions.Count];

            bodyChunkConnections = new BodyChunkConnection[0];
            bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, 3f, mass / bodyChunks.Length);


            airFriction = base.airFriction;
            gravity = base.gravity;
            bounce = base.bounce;
            surfaceFriction = base.surfaceFriction;
            collisionLayer = base.collisionLayer;
            waterFriction = base.waterFriction;
            buoyancy = base.buoyancy;
            GoThroughFloors = base.GoThroughFloors;*/

        }

        /*public override void Update(bool eu)
        {
            base.Update(eu);
            if (grabbedBy.Count == 0)
            {
                bodyChunks[0].vel = new Vector2(bodyChunks[0].vel.x *= 0.65f, bodyChunks[0].vel.y);
            }
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            Vector2 center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            bodyChunks[0].HardSetPosition(new Vector2(0, 0) * 20f + center);
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);

            if (speed > 10)
            {
                room.PlaySound(SoundID.Dart_Maggot_Bounce_Off_Wall, bodyChunks[chunk].pos, 0.35f, 2f);
            }
        }

        public new void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(this.meshSegs, false, true);
            sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(this.meshSegs - 3, false, true);
            AddToContainer(sLeaser, rCam, null);
            Debug.Log("%%%% INIT SPRITES");
        }



        public new void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            /*base.AddToContainer(sLeaser, rCam, newContainer);
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);

            Debug.Log("%%%%% ADD TO CONTAINER");
        }*/

    }

    public class HazerSacProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = true;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
    }
}
