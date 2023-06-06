using System.Collections.Generic;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;
using RWCustom;



namespace LanternSpearFO
{
    sealed class LSpearFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrLaSpear = new("LanternSpear", true);
       
        public static readonly MultiplayerUnlocks.SandboxUnlockID laSpear = new("LanternSpear", true);
        public static readonly PlacedObject.MultiplayerItemData.Type GuideSpear = new("GuideSpear", true);

        public LSpearFisobs() : base(AbstrLaSpear)
        {
            Icon = new LSpearIcon();

            SandboxPerformanceCost = new(linear: 0.2f, 0f);

            RegisterUnlock(laSpear, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
        {
            string[] p = entitySaveData.CustomData.Split(';');

            if (p.Length < 5 )
            {
                p = new string[5];
            }

            var result = new LSpearAbstract(world, entitySaveData.Pos, entitySaveData.ID)
            {
                hue = float.TryParse(p[0], out var h) ? h : 0,
                saturation = float.TryParse(p[1], out var s) ? s : 1,
                scaleX = float.TryParse(p[2], out var x) ? x : 1,
                scaleY = float.TryParse(p[3], out var y) ? y : 1,
            };

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

        

        private static readonly LSpearProperties properties = new();
        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return base.Properties(forObject);
        }
    }

    //Abstract
    sealed class LSpearAbstract : AbstractPhysicalObject
    {
        //Sets types to hue, saturation, scale
        public float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;

        //Properties
        public LSpearAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, LSpearFisobs.AbstrLaSpear, null, pos, ID)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 0.5f;
            hue = 1f;
        }


        //Lets object know when its in the same room as player
        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new LanternSpear(this);
            }
            
        }

        

        public override string ToString()
        {
            return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
        }
    }

    //Lantern Spear icon class
    sealed class LSpearIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is LSpearAbstract lspear ? (int)(lspear.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "icon_LanternSpear";
        }
    }

    //Sprite
    sealed class LanternSpear : PhysicalObject, IDrawable
    {
        //Lantern Spear Constructor
        public LanternSpear(LSpearAbstract abstr) : base(abstr)
        {
            Random.State state = Random.state;
            Random.InitState(abstractPhysicalObject.ID.RandomSeed);
            this.rag = new Vector2[Random.Range(4, Random.Range(4, 10)), 6];
            Random.state = state;

            float mass = 40f;
            var positions = new List<Vector2>();

            positions.Add(Vector2.zero);

            bodyChunks = new BodyChunk[positions.Count];

            //create body chunk (collider)
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i] = new BodyChunk(this, i, Vector2.zero, 30f, mass / bodyChunks.Length);
            }

            bodyChunks[0].rad = 40f;

            airFriction = 0.99f;
            gravity = 1f;
            bounce = 0.3f;
            surfaceFriction = 1f;
            collisionLayer = 1;
            waterFriction = 0.5f;
            buoyancy = 0.3f;
            GoThroughFloors = false;

        }
        public Vector2[,] rag;
        private Vector3 lastRotation;
        private Vector3 rotation;
        private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
        private float conRad = 7f;
        public DebugLabel stuckIns;

        private Vector2 RagAttachPos(float timeStacker)
        {
            return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + Vector3.Slerp(this.lastRotation, this.rotation, timeStacker).ToVector2InPoints() * 15f;
        }
        
        //Places object down when spawned
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            this.ResetRag();
            Vector2 center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            bodyChunks[0].HardSetPosition(new Vector2(0, 0) * 20f + center);
        }

        public override void Update(bool eu)
        {
            //ExplosiveSpear rag code adapted
            for (int i = 0; i < this.rag.GetLength(0); i++)
            {
                float t = (float)i / (float)(this.rag.GetLength(0) - 1);
                this.rag[i, 1] = this.rag[i, 0];
                this.rag[i, 0] += this.rag[i, 2];
                //this.rag[i, 2] -= this.rotation.ToVector2InPoints * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
                this.rag[i, 4] = this.rag[i, 3];
                this.rag[i, 3] = (this.rag[i, 3] + this.rag[i, 5] * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                this.rag[i, 5] = (this.rag[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(this.rag[i, 0], this.rag[i, 1])), 0.3f)).normalized;
                if (this.room.PointSubmerged(this.rag[i, 0]))
                {
                    this.rag[i, 2] *= Custom.LerpMap(this.rag[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    this.rag[i, 2].y += 0.05f;
                    this.rag[i, 2] += Custom.RNV() * 0.1f;
                }
                else
                {
                    this.rag[i, 2] *= Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                    this.rag[i, 2].y -= this.room.gravity * Custom.LerpMap(Vector2.Distance(this.rag[i, 0], this.rag[i, 1]), 1f, 6f, 0.6f, 0f);
                    if (i % 3 == 2 || i == this.rag.GetLength(0) - 1)
                    {
                        SharedPhysics.TerrainCollisionData terrainCollisionData = this.scratchTerrainCollisionData.Set(this.rag[i, 0], this.rag[i, 1], this.rag[i, 2], 1f, new IntVector2(0, 0), false);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(this.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.VerticalCollision(this.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(this.room, terrainCollisionData);
                        this.rag[i, 0] = terrainCollisionData.pos;
                        this.rag[i, 2] = terrainCollisionData.vel;
                        if (terrainCollisionData.contactPoint.x != 0)
                        {
                            this.rag[i, 2].y *= 0.6f;
                        }
                        if (terrainCollisionData.contactPoint.y != 0)
                        {
                            this.rag[i, 2].x *= 0.6f;
                        }
                    }
                }
            }
            for (int j = 0; j < this.rag.GetLength(0); j++)
            {
                if (j > 0)
                {
                    Vector2 normalized = (this.rag[j, 0] - this.rag[j - 1, 0]).normalized;
                    float num = Vector2.Distance(this.rag[j, 0], this.rag[j - 1, 0]);
                    float d = (num > this.conRad) ? 0.5f : 0.25f;
                    this.rag[j, 0] += normalized * (this.conRad - num) * d;
                    this.rag[j, 2] += normalized * (this.conRad - num) * d;
                    this.rag[j - 1, 0] -= normalized * (this.conRad - num) * d;
                    this.rag[j - 1, 2] -= normalized * (this.conRad - num) * d;
                    if (j > 1)
                    {
                        normalized = (this.rag[j, 0] - this.rag[j - 2, 0]).normalized;
                        this.rag[j, 2] += normalized * 0.2f;
                        this.rag[j - 2, 2] -= normalized * 0.2f;
                    }
                    if (j < this.rag.GetLength(0) - 1)
                    {
                        this.rag[j, 3] = Vector3.Slerp(this.rag[j, 3], (this.rag[j - 1, 3] * 2f + this.rag[j + 1, 3]) / 3f, 0.1f);
                        this.rag[j, 5] = Vector3.Slerp(this.rag[j, 5], (this.rag[j - 1, 5] * 2f + this.rag[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(this.rag[j, 1], this.rag[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    this.rag[j, 0] = this.RagAttachPos(1f);
                    this.rag[j, 2] *= 0f;
                }
            }
            base.Update(eu);
        }

        //plays sound on impact
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
            if (speed > 10)
            {
                room.PlaySound(SoundID.Spear_Stick_In_Wall, bodyChunks[chunk].pos, 0.35f, 1f);
            }
        }
        //Resets rag pos
        public void ResetRag()
        {
            Vector2 vector = this.RagAttachPos(1f);
            for (int i = 0; i < this.rag.GetLength(0); i++)
            {
                this.rag[i, 0] = vector;
                this.rag[i, 1] = vector;
                this.rag[i, 2] *= 0f;
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (this.stuckIns != null)
            {
                rCam.ReturnFContainer("HUD").AddChild(this.stuckIns.label);
            }
            sLeaser.sprites = new FSprite[6];
            // Adapted from ExplosiveSpear, makes spear and rag sprites
            sLeaser.sprites[1] = new FSprite("SmallSpear", true);
            sLeaser.sprites[0] = new FSprite("SpearRag", true);
            sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(this.rag.GetLength(0), false, true);
            sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
            sLeaser.sprites[2].alpha = rCam.game.SeededRandom(this.abstractPhysicalObject.ID.RandomSeed);
            // Added Lantern sprite
            sLeaser.sprites[3] = new FSprite("DangleFruit0A", true);
            sLeaser.sprites[4] = new FSprite("DangleFruit0B", true);
            sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["LightSource"];
            this.AddToContainer(sLeaser, rCam, null);
            if (this.rag == null)
            {
                Debug.Log("GUIDESPEAR RAG NULL");
            }
            else
            {
                Debug.Log("AAAAAAAAAAA");
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Spear guideSpear = new(this.abstractPhysicalObject, rCam.game.world);
            guideSpear.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].color = Color.green;
            sLeaser.sprites[2].color = Color.green;
            
            
            float num = 0f;
            Vector2 a = this.RagAttachPos(timeStacker);
            for (int i = 0; i < this.rag.GetLength(0); i++)
            {
                float f = (float)i / (float)(this.rag.GetLength(0) - 1);
                Vector2 vector = Vector2.Lerp(this.rag[i, 1], this.rag[i, 0], timeStacker);
                float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * 3.1415927f)) * Vector3.Slerp(this.rag[i, 4], this.rag[i, 3], timeStacker).x;
                Vector2 normalized = (a - vector).normalized;
                Vector2 a2 = Custom.PerpendicularVector(normalized);
                float d = Vector2.Distance(a, vector) / 5f;
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4, a - normalized * d - a2 * (num2 + num) * 0.5f - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, a - normalized * d + a2 * (num2 + num) * 0.5f - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector + normalized * d - a2 * num2 - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, vector + normalized * d + a2 * num2 - camPos);
                a = vector;
                num = num2;
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Spear guideSpearPalette = new(this.abstractPhysicalObject, rCam.game.world);
            guideSpearPalette.ApplyPalette(sLeaser, rCam, palette);
            sLeaser.sprites[0].color = new Color(0.25f, 1f, 0f);
            sLeaser.sprites[1].color = new Color(0f, 0f, 0f);
            sLeaser.sprites[2].color = new Color(0.25f, 1f, 0f);
            sLeaser.sprites[3].color = new Color(1f, 0.2f, 0f);
            sLeaser.sprites[4].color = new Color(1f, 1f, 1f);
            sLeaser.sprites[5].color = new Color(1f, 0.4f, 0.3f);
        }

        //Add sprite to game's current list of items present on current camera
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                newContainer.AddChild(fsprite);
            }
        }
    }

    sealed class LSpearProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = false;

        public override void Grabability (Player player, ref Player.ObjectGrabability grabability)
        {
            //Player can only grab one Lspear at a time
            grabability = Player.ObjectGrabability.BigOneHand;
        }

        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            base.ScavWeaponUseScore(scav, ref score);
            score = 0;
        }


    }
}
