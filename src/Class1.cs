using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using IL.ScavengerCosmetic;

namespace AppleCat
{
    sealed class AppleTail : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrTail = new("Tail", true);
        
        public TailFisobs() : base(AbstrTail)
        {
            //RegisterUnlock(mTail, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }
    }

    //Abstract Constructor
    sealed class TailAbstract : AbstractPhysicalObject
    {
        public float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;

        public override string ToString()
        {
            return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
        }
        public TailAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, TailFisobs.AbstrCrate, null, pos, ID)
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
                realizedObject = new Tail(this);
            }
        }
    }

    //Physical Object
    sealed class Tail : PhysicalObject, GraphicsModule
    {
        //Constructor
        public Tail(TailAbstract abstr) : base(abstr)
        {
            float mass = 40f;
            var positions = new List<Vector2>();

            positions.Add(Vector2.Zero);
            bodyChunks = new BodyChunk[positions.Count];

            for(int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i] = new BodyChunk(this, i, UnityEngine.Vector2.zero, 30f, mass / bodyChunks.Length);
            }
            bodyChunks[0].rad = 40f;

            bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
            int connection = 0;
            for (int x = 0; x< bodyChunks.Length; x++)
            {
                for (int y = x + 1; y < bodyChunks.Length; y++)
                {
                    bodyChunkConnections[connection] = new BodyChunkConnection(bodyChunks[x], bodyChunks[y], Vector2.Distance(positions[x], positions[y]), BodyChunkConnection.Type.Normal, 0.5f, -1f);
                    connection++;
                }
            }

            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0f;
            waterFriction = 0.999f;
            buoyancy = 0.8f;
            GoThroughFloors = false;
            collisionLayer = 1;

        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            Vector2 center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            bodyChunks[0].HardSetPosition(new Vector2(0, 0) * 20f + center);
        }
    }
}
