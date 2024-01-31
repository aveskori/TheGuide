using System;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using JetBrains.Annotations;
using RWCustom;
using UnityEngine;


namespace Guide.Objects
{
    public class CloversFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrClover = new("AbstrClover", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID mClover = new("Clover", true);

        public CloversFisobs() : base(AbstrClover)
        {
            Icon = new CloverIcon();
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(mClover, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
        {
            var result = new CloverAbstract(world, null, entitySaveData.Pos, entitySaveData.ID, 0, 0, null); //this too
            return result;
        }


        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return base.Properties(forObject);
        }
    }

    sealed class CloverAbstract : AbstractConsumable
    {
        public override string ToString()
        {
            return this.SaveToString();
        }

        public CloverAbstract(World world, AbstractObjectType type, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData) :
            base(world, type, null, pos, ID, originRoom, placedObjectIndex, consumableData)
        {
            type = CloversFisobs.AbstrClover;

        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new Clover(this);
            }
        }
    }

    sealed class CloverIcon : Icon
    {
        // the issue??
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CloverAbstract clover ? (int)(clover.placedObjectIndex * 1000f) : 0; //dont know what to put for the clover.???
        }

        public override Color SpriteColor(int data)
        {
            return Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "atlases/icon_clover"; //exlog claims that this file doesnt exist but it does????
        }
    }

    sealed class Clover : SlimeMold
    {
        public Clover(CloverAbstract abstr) : base(abstr)
        {

        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
        }

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
        }

        public new void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public new void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            Color color = Color.Lerp(new Color(0.63f, 0.89f, 0.49f), palette.blackColor, 0.18f + 0.7f * rCam.PaletteDarkness()); //inside
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1].color = color + new Color(0.07f, 0.41f, 0.22f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness()); //outside
            sLeaser.sprites[2].color = Color.Lerp(new Color(0f, 1f, 0f), palette.blackColor, 0.3f); //red to black??

        }

        public new void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);

        }

        public new void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            base.BitByPlayer(grasp, eu);
        }
    }
}
