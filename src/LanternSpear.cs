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
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrLaSpear = new("AbstrLaSpear", true);
       
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

            if (p.Length < 8 )
            {
                p = new string[8];
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
    sealed class LSpearAbstract : AbstractSpear
    {
        //Sets types to hue, saturation, scale
        public new float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;
        
        //Properties
        public LSpearAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, null, pos, ID, false)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 0.5f;
            hue = 1f;
            type = LSpearFisobs.AbstrLaSpear;
        }


        //Lets object know when its in the same room as player
        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new LanternSpear(this, null);
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
            return "atlases/icon_LanternSpear";
        }
    }

    //Sprite
    sealed class LanternSpear : Spear, IDrawable
    {
        //Lantern Spear Constructor
        public LanternSpear(LSpearAbstract abstr, World world) : base(abstr, world)
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
            /*for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i] = new BodyChunk(this, i, Vector2.zero, 30f, mass / bodyChunks.Length);
            }*/

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
            pivotAtTip = false;
            lastPivotAtTip = false;
            stuckBodyPart = -1;
            firstChunk.loudness = 7f;

           
        }
        public Vector2[,] rag;
        
        private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData = new SharedPhysics.TerrainCollisionData();
        private float conRad = 7f;
        

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
            base.Update(eu);
            for (int i = 0; i < this.rag.GetLength(0); i++)
            {
                float t = (float)i / (float)(this.rag.GetLength(0) - 1);
                this.rag[i, 1] = this.rag[i, 0];
                this.rag[i, 0] += this.rag[i, 2];
                this.rag[i, 2] -= this.rotation * Mathf.InverseLerp(1f, 0f, (float)i) * 0.8f;
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
            //spear update
            this.lastPivotAtTip = this.pivotAtTip;
            this.pivotAtTip = (base.mode == Weapon.Mode.Thrown || base.mode == Weapon.Mode.StuckInCreature);
            if (this.addPoles && this.room.readyForAI)
            {
                if (this.abstractSpear.stuckInWallCycles >= 0)
                {
                    this.wasHorizontalBeam[1] = this.room.GetTile(this.stuckInWall.Value).horizontalBeam;
                    this.room.GetTile(this.stuckInWall.Value).horizontalBeam = true;
                    for (int k = -1; k < 2; k += 2)
                    {
                        this.wasHorizontalBeam[k + 1] = this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)k, 0f)).horizontalBeam;
                        if (!this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)k, 0f)).Solid)
                        {
                            this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)k, 0f)).horizontalBeam = true;
                        }
                    }
                }
                else
                {
                    this.wasHorizontalBeam[1] = this.room.GetTile(this.stuckInWall.Value).verticalBeam;
                    this.room.GetTile(this.stuckInWall.Value).verticalBeam = true;
                    for (int m = -1; m < 2; m += 2)
                    {
                        this.wasHorizontalBeam[m + 1] = this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)m)).verticalBeam;
                        if (!this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)m)).Solid)
                        {
                            this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)m)).verticalBeam = true;
                        }
                    }
                }
                this.addPoles = false;
                this.hasHorizontalBeamState = true;
            }

            //Lantern Update

            Lantern l = new Lantern(abstractPhysicalObject);

            
            Debug.Log($"%%%%% LANTERN FOR STATEMENT%%%%%");
            if (l.lightSource == null)
            {
                l.lightSource = new LightSource(base.bodyChunks[0].pos, false, new Color(1f, 0.2f, 0f), this);
                l.lightSource.affectedByPaletteDarkness = 0.5f;
                l.room.AddObject(l.lightSource);
                Debug.Log($"%%%% LIGHT SOURCE %%%%");
            }
            else
            {
                l.lightSource.setPos = new Vector2?(base.bodyChunks[0].pos);
                l.lightSource.setRad = new float?(250f * l.flicker[0, 0]);
                l.lightSource.setAlpha = new float?(1f);
                Debug.Log($"%%%% ELSE 1 %%%%");
                if (l.lightSource.slatedForDeletetion || l.lightSource.room != l.room)
                {
                    l.lightSource = null;
                }                
                Debug.Log($"%%%% ELSE 2 %%%%");
            }
            for (int i = 0; i < l.flicker.GetLength(0); i++)
            {
                l.flicker[i, 1] = l.flicker[i, 0];
                l.flicker[i, 0] += Mathf.Pow(Random.value, 3f) * 0.1f * ((Random.value < 0.5f) ? -1f : 1f);
                l.flicker[i, 0] = Custom.LerpAndTick(l.flicker[i, 0], l.flicker[i, 2], 0.05f, 0.033333335f);
                if (Random.value < 0.2f)
                {
                    l.flicker[i, 2] = 1f + Mathf.Pow(Random.value, 3f) * 0.2f * ((Random.value < 0.5f) ? -1f : 1f);
                }
                l.flicker[i, 2] = Mathf.Lerp(l.flicker[i, 2], 1f, 0.01f);
            }
            this.lastRotation = this.rotation;
            base.firstChunk.collideWithTerrain = (this.grabbedBy.Count == 0);
            if (this.grabbedBy.Count > 0)
            {
                l.rotation = Custom.PerpendicularVector(Custom.DirVec(base.bodyChunks[0].pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
                l.rotation.y = -Mathf.Abs(this.rotation.y);
            }

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

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
            if (this.stuckIns != null)
            {
                rCam.ReturnFContainer("HUD").AddChild(this.stuckIns.label);
            }
            sLeaser.sprites = new FSprite[7];
            // Adapted from ExplosiveSpear, makes spear and rag sprites
            sLeaser.sprites[1] = new FSprite("SmallSpear", true);
            sLeaser.sprites[0] = new FSprite("SpearRag", true);
            sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(this.rag.GetLength(0), false, true);
            sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
            sLeaser.sprites[2].alpha = rCam.game.SeededRandom(this.abstractPhysicalObject.ID.RandomSeed);
            // Added Lantern sprite
            sLeaser.sprites[3] = new FSprite("DangleFruit0A", true);
            sLeaser.sprites[4] = new FSprite("DangleFruit0B", true);
            sLeaser.sprites[5] = new FSprite("Futile_White");
            sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["LightSource"];
            sLeaser.sprites[6] = new FSprite("Futile_White");
            sLeaser.sprites[6].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            this.AddToContainer(sLeaser, rCam, null);
            
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            
            sLeaser.sprites[0].color = Color.green;
            sLeaser.sprites[1].color = Color.blue;
            sLeaser.sprites[2].color = Color.green;
            //spear DrawSprites
            if (this.stuckIns != null && this.room != null)
            {
                if (this.room.game.devToolsActive)
                {
                    sLeaser.RemoveAllSpritesFromContainer();
                    base.InitiateSprites(sLeaser, rCam);
                    base.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
                }
                if (this.stuckIns.relativePos)
                {
                    this.stuckIns.label.x = base.bodyChunks[0].pos.x + this.stuckIns.pos.x - camPos.x;
                    this.stuckIns.label.y = base.bodyChunks[0].pos.y + this.stuckIns.pos.y - camPos.y;
                }
                else
                {
                    this.stuckIns.label.x = this.stuckIns.pos.x;
                    this.stuckIns.label.y = this.stuckIns.pos.y;
                }
            }
            Vector2 vector4 = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            if (this.vibrate > 0)
            {
                vector4 += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
            }
            Vector3 v = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            for (int i = (this.bugSpear) ? 1 : 0; i >= 0; i--)
            {
                sLeaser.sprites[i].x = vector4.x - camPos.x;
                sLeaser.sprites[i].y = vector4.y - camPos.y;
                sLeaser.sprites[i].anchorY = Mathf.Lerp(this.lastPivotAtTip ? 0.85f : 0.5f, this.pivotAtTip ? 0.85f : 0.5f, timeStacker);
                sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
            }
            //rag drawsprites
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
            //lantern drawsprites
            Lantern l = new Lantern(abstractPhysicalObject);
            Vector2 vector2 = Vector2.Lerp(l.firstChunk.lastPos, l.firstChunk.pos, timeStacker);
            Vector2 vector3 = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            for (int i = 3; i < 5; i++)
            {
                sLeaser.sprites[i].x = vector2.x - camPos.x;
                sLeaser.sprites[i].y = vector2.y - camPos.y;
                sLeaser.sprites[i].rotation = Custom.VecToDeg(vector2);
            }
            sLeaser.sprites[5].x = vector2.x - vector3.x * 3f - camPos.x;
            sLeaser.sprites[5].y = vector2.y - vector3.y * 3f - camPos.y;
            sLeaser.sprites[5].scale = Mathf.Lerp(l.flicker[0, 1], l.flicker[0, 0], timeStacker) * 2f;
            sLeaser.sprites[6].x = vector2.x - vector3.x * 3f - camPos.x;
            sLeaser.sprites[6].y = vector2.y - vector3.y * 3f - camPos.y;
            sLeaser.sprites[6].scale = Mathf.Lerp(l.flicker[1, 1], l.flicker[1, 0], timeStacker) * 200f / 8f;
            if (base.slatedForDeletetion || l.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            
            
            sLeaser.sprites[0].color = new Color(0.25f, 1f, 0f);
            sLeaser.sprites[1].color = rCam.currentPalette.blackColor;
            sLeaser.sprites[2].color = new Color(0.25f, 1f, 0f);
            sLeaser.sprites[3].color = new Color(1f, 0.2f, 0f);
            sLeaser.sprites[4].color = new Color(1f, 1f, 1f);
            sLeaser.sprites[5].color = Color.Lerp(new Color(1f, 0.2f, 0f), new Color(1f, 1f, 1f), 0.3f);
            sLeaser.sprites[6].color = new Color(1f, 0.4f, 0.3f);
            
        }

        //Add sprite to game's current list of items present on current camera
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            int spriteLength = sLeaser.sprites.Length;    //get sLeaser array length 
            Debug.Log($"~~~~~~SLEASER SPRITES LENGTH: " + spriteLength + " ~~~~~~");     //prints sLeaser array length

            newContainer ??= rCam.ReturnFContainer("Items");

            for (int i = 0; i < spriteLength; i++)
            {
                Debug.Log($"~~~~~SPRITE " + i + " LOADED~~~~~~");     //logs which sprites are being fed through
                sLeaser.sprites[i].RemoveFromContainer();
            }
            
            
            newContainer.AddChild(sLeaser.sprites[0]);
            newContainer.AddChild(sLeaser.sprites[1]);
            newContainer.AddChild(sLeaser.sprites[2]);
            newContainer.AddChild(sLeaser.sprites[3]);
            newContainer.AddChild(sLeaser.sprites[4]);
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[5]);
            rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[6]);     //line referenced in exlog
            
            
        }
    }

    sealed class LSpearProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = false;

        public override void Grabability (Player player, ref Player.ObjectGrabability grabability)
        {
            //Player can only grab one Lspear at a time
            grabability = Player.ObjectGrabability.OneHand;
        }

        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            base.ScavWeaponUseScore(scav, ref score);
            score = 0;
        }


    }
}
