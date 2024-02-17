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

    internal class CentiShellFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType abstrCentiShell = new("Centipede Shell", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID mCentiShell = new("Centipede Shell", true);

        public CentiShellFisobs() : base(abstrCentiShell)
        {
            Icon = new SimpleIcon("Kill_Centipede1", Color.gray);
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(mCentiShell, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5)
            {
                p = new string[5];
            }

            var result = new CentiShellAbstract(world, saveData.Pos, saveData.ID)
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

        private static readonly CentiShellProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return properties;
        }
    }

    public class CentiShellAbstract : AbstractPhysicalObject
    {
        public CentiShellAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, CentiShellFisobs.abstrCentiShell, null, pos, ID)
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
                realizedObject = new CentiShell(this);
            }
        }

        public float scaleX;
        public float scaleY;
        public float hue;
        public float saturation;
        



        public override string ToString()
        {
            return this.SaveToString($"{{RWCustom.Custom.colorToHex(backshellColor)}};{{hue}};{{scaleX}};{{scaleY}}");
        }
    }  //Abstract Class

 public class CentiShellIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CentiShellAbstract cshell ? (int)(cshell.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "icon_CentiShell";
        }
    }   

    public class CentiShell : PlayerCarryableItem, IDrawable
    {

        private static float Rand => UnityEngine.Random.value;

        public float rotation;
        public float lastRotation;
        public float rotVel;
        public float lastDarkness = -1f;
        public float darkness;
        public Vector2 pos;
        public Vector2 vel;

        private Color blackColor;
        private Color earthColor;

        private readonly float rotationOffset;

        public CentiShellAbstract Abstr { get; }

        public CentiShell(CentiShellAbstract abstr) : base(abstr)
        {
            Abstr = abstr;

            bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4 * (Abstr.scaleX + Abstr.scaleY), 0.35f) { goThroughFloors = true } };
            bodyChunks[0].lastPos = bodyChunks[0].pos;
            bodyChunks[0].vel = vel;

            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.6f;
            surfaceFriction = 0.45f;
            collisionLayer = 1;
            waterFriction = 0.92f;
            buoyancy = 0.75f;

            rotation = Rand * 360f;
            lastRotation = rotation;

            rotationOffset = Rand * 30 - 15;
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

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("CentipedeBackShell", true);
            sLeaser.sprites[1] = new FSprite("CentipedeBackShell", true);
            AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);

            Debug.Log("%%%%% ADD TO CONTAINER");
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            float num = Mathf.InverseLerp(305f, 380f, timeStacker);
            pos.y -= 20f * Mathf.Pow(num, 3f);
            float num2 = Mathf.Pow(1f - num, 0.25f);
            lastDarkness = darkness;
            darkness = rCam.room.Darkness(pos);
            darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(pos);

            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
                sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
                sLeaser.sprites[i].scaleY = num2 * Abstr.scaleY;
                sLeaser.sprites[i].scaleX = num2 * Abstr.scaleX;
            }

            

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }

        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {            
            sLeaser.sprites[0].color = palette.blackColor;
            sLeaser.sprites[1].color = Color.HSVToRGB(Abstr.hue, Abstr.saturation, darkness);
            
        }
    }

    

    public class CentiShellProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = true;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
    }
    
}
