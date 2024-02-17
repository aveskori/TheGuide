using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using MoreSlugcats;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace Guide.Objects
{
    internal class LizSkullFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType abstrLizSkull = new("Lizard Skull", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID mLizSkull = new("Lizard Skull", true);

        public LizSkullFisobs() : base(abstrLizSkull)
        {
            Icon = new SimpleIcon("Kill_Hazer", Color.gray);
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(mLizSkull, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);

        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5)
            {
                p = new string[5];
            }

            var result = new LizSkullAbstract(world, saveData.Pos, saveData.ID)
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

        /*private static readonly LizardSkullProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return properties;
        }*/
    }

    public class LizSkullAbstract : AbstractPhysicalObject
    {
        public LizSkullAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, HazerSacFisobs.AbstrHSac, null, pos, ID)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 0.5f;
            hue = 1f;
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new LizSkull(this);
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

    public class LizSkullIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CentiShellAbstract hsac ? (int)(hsac.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "icon_LizSkull";
        }
    }

    public class LizSkull : PlayerCarryableItem, IDrawable
    {

        public Vector2 rotation;

        public Vector2 lastRotation;

        public Vector2? setRotation;
        public LizSkullAbstract Abstr { get; }

        public LizSkull(LizSkullAbstract abstr) : base(abstr)        
        {
            Abstr = abstr;

            float mass = 20f;
            var positions = new List<Vector2>();

            positions.Add(Vector2.zero);

            bodyChunks = new BodyChunk[positions.Count];

            //create body chunk (collider)
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i] = new BodyChunk(this, i, Vector2.zero, 30f, mass / bodyChunks.Length);
            }

            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
            bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];


            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.4f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            waterFriction = 0.98f;
            buoyancy = 0.4f;
            GoThroughFloors = false;
            firstChunk.loudness = 7f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            
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
                room.PlaySound(SoundID.Rock_Hit_Wall, bodyChunks[chunk].pos, 0.35f, 2f);
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("LizardHead0.0");
            sLeaser.sprites[1] = new FSprite("LizardLowerTeeth0.0");
            sLeaser.sprites[2] = new FSprite("LizardEyes0.0");
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
        }


        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {

            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);

            Debug.Log("%%%%% ADD TO CONTAINER");
        }

    }
}
