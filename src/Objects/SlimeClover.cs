using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace Guide.Objects
{
    sealed class SCloverFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrSClover = new("AbstrSClover", true);

        public static readonly MultiplayerUnlocks.SandboxUnlockID laSpear = new("SlimeClover", true);
        //public static readonly PlacedObject.MultiplayerItemData.Type GuideSpear = new("GuideSpear", true);


        public SCloverFisobs() : base(AbstrSClover)
        {
            Icon = new SCloverIcon();
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(laSpear, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }


        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
        {
            var result = new SCloverAbstract(world, AbstrSClover, null, entitySaveData.Pos, entitySaveData.ID, -1, -1, null);
            return result;
        }

        /*
        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
        {
            string[] p = entitySaveData.CustomData.Split(';');

            if (p.Length < 8 )
            {
                p = new string[8];
            }

            var result = new SCloverAbstract(world, entitySaveData.Pos, entitySaveData.ID)
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
        
        */


        private static readonly SlimeCloverProperties properties = new();
        public override ItemProperties Properties(PhysicalObject forObject)
        {
            //return base.Properties(forObject);
            return properties;
        }

    }

    //Abstract
    sealed class SCloverAbstract : AbstractConsumable
    {
        //WE PROBABLY DON'T NEED ANY SAVE DATA, AS ALL LSPEARS WILL PROBABLY BE THE SAME
        /*
        //Sets types to hue, saturation, scale
        public new float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;
        
        

        public override string ToString()
        {
            return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
        }
        

        //Properties
        public SCloverAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, null, pos, ID, false)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 1f;
            hue = 1f;
            type = SCloverFisobs.AbstrSClover;
        }
        */

        public override string ToString()
        {
            return this.SaveToString();
        }

        public SCloverAbstract(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData) : base(world, type, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
        {
            //GOOD ENOUGH?...
            type = SCloverFisobs.AbstrSClover;
        }

        //Lets object know when its in the same room as player
        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new SlimeClover(this);
            }
        }
    }

    //Lantern Spear icon class
    sealed class SCloverIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return 0; //NO SPECIAL DATA FOR US
        }

        public override Color SpriteColor(int data)
        {
            return Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "atlases/icon_LanternSpear";
        }
    }

    //Sprite
    sealed class SlimeClover : PlayerCarryableItem, IDrawable, IPlayerEdible
    {
        //Constructor
        public SlimeClover(AbstractPhysicalObject abstr) : base(abstr)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.12f);
            bodyChunkConnections = new BodyChunkConnection[0];
            canBeHitByWeapons = false;
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.2f;
            surfaceFriction = 0.7f;
            collisionLayer = 1;
            waterFriction = 0.95f;
            buoyancy = 1.1f;


            //DO WE ACTUALLY NEED THIS?
            Random.State state = Random.state;
            Random.InitState(abstractPhysicalObject.ID.RandomSeed);
            Random.state = state;
        }

        public AbstractConsumable AbstrConsumable;
        public Vector2 rotation;
        public Vector2 lastRotation;
        public Vector2? stuckPos;
        public Vector2? gravitateToPos;



        public override void Update(bool eu)
        {
            base.Update(eu);
            lastRotation = rotation;
            if (grabbedBy.Count > 0)
            {
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (firstChunk.ContactPoint.y < 0)
            {
                rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
                BodyChunk firstChunk = base.firstChunk;
                firstChunk.vel.x = firstChunk.vel.x * 0.8f;
            }

        }



        //Places object down when spawned
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            //BORKED. REPLACING WITH SOMETHING SIMPLER FOR NOW.
            /*
			if (!this.AbstrConsumable.isConsumed && this.AbstrConsumable.placedObjectIndex >= 0 && this.AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
			{
				this.gravitateToPos = new Vector2?(placeRoom.roomSettings.placedObjects[this.AbstrConsumable.placedObjectIndex].pos);
				IntVector2 tilePosition = this.room.GetTilePosition(placeRoom.roomSettings.placedObjects[this.AbstrConsumable.placedObjectIndex].pos);
				Random.State state = Random.state;
				Random.InitState(tilePosition.x + tilePosition.y);
				List<IntVector2> list = new List<IntVector2>();
				for (int i = 0; i < 8; i++)
				{
					list.Add(Custom.eightDirections[i]);
				}
				List<IntVector2> list2 = new List<IntVector2>();
				while (list.Count > 0)
				{
					int index = Random.Range(0, list.Count);
					list2.Add(list[index]);
					list.RemoveAt(index);
				}
				Vector2 vector = this.room.MiddleOfTile(tilePosition) + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
				for (int j = 0; j < list2.Count; j++)
				{
					if (this.room.GetTile(tilePosition + list2[j]).Solid)
					{
						Vector2 pos = this.room.MiddleOfTile(tilePosition + list2[j]) + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
						FloatRect? floatRect = new FloatRect?(Custom.RectCollision(pos, vector, this.room.TileRect(tilePosition + list2[j])));
						if (floatRect != null)
						{
							this.stuckPos = new Vector2?(floatRect.Value.GetCorner(FloatRect.CornerLabel.D));
							break;
						}
					}
				}
				base.firstChunk.HardSetPosition((this.stuckPos != null) ? this.stuckPos.Value : vector);
				Random.state = state;
			}
			else
			{
				base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
				this.rotation = Custom.RNV();
				this.lastRotation = this.rotation;
			}
            */

            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            rotation = Custom.RNV();
            lastRotation = rotation;

            //this.ResetSlime();
        }


        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) //override
        {
            //POPCORN VERSION
            sLeaser.sprites = new FSprite[3];
            sLeaser.sprites[0] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[1] = new FSprite("JetFishEyeA", true);
            sLeaser.sprites[2] = new FSprite("tinyStar", true);

            AddToContainer(sLeaser, rCam, null);
        }


        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) //override
        {
            // base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);

            sLeaser.sprites[0].x = vector.x - camPos.x;
            sLeaser.sprites[0].y = vector.y - camPos.y;
            sLeaser.sprites[1].x = vector.x - camPos.x;
            sLeaser.sprites[1].y = vector.y - camPos.y;
            sLeaser.sprites[2].x = vector.x - camPos.x;
            sLeaser.sprites[2].y = vector.y - camPos.y;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }


        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) //override
        {
            Color color = Color.Lerp(new Color(0.9f, 0.83f, 0.5f), palette.blackColor, 0.18f + 0.7f * rCam.PaletteDarkness());
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1].color = color + new Color(0.3f, 0.3f, 0.3f) * Mathf.Lerp(1f, 0.15f, rCam.PaletteDarkness());
            sLeaser.sprites[2].color = Color.Lerp(new Color(1f, 0f, 0f), palette.blackColor, 0.3f);
        }


        //Add sprite to game's current list of items present on current camera
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer) //override
        {
            if (newContainer == null)
            {
                newContainer = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }


        //PLAYEREDIBLE STUFF

        public int bites = 3;

        public int BitesLeft
        {
            get
            {
                return bites;
            }
        }


        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound(bites == 0 ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }


        public int FoodPoints
        {
            get
            {
                return 1;
            }
        }

        public bool Edible
        {
            get
            {
                return true;
            }
        }

        public bool AutomaticPickUp
        {
            get
            {
                return true;
            }
        }

        public void ThrowByPlayer()
        {
        }


    }

    sealed class SlimeCloverProperties : ItemProperties
    {
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
    }
}