using System;
using UnityEngine;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;

namespace Guide.Objects
{
    internal class VoidSpearFisobs : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstrVSpear = new("VoidSpear", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID vSpear = new("VoidSpear", true);

        public VoidSpearFisobs() : base(AbstrVSpear)
        {
            Icon = new SimpleIcon("Symbol_HellSpear", Color.white);
            SandboxPerformanceCost = new(linear: 0.2f, 0f);
            RegisterUnlock(vSpear, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {


            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5)
            {
                p = new string[5];
            }

            var result = new VoidSpearAbstract(world, null, saveData.Pos, saveData.ID)
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

        private static readonly VoidSpearProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            return properties;
        }
    } //Fisobs Class

    public class VoidSpearAbstract : AbstractSpear
    {
        public VoidSpearAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID) : base(world, realizedObject, pos, ID, false)
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
                realizedObject = new VoidSpear(this, world);
                
            }
        }

        public new float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;


        public override string ToString()
        {
            return this.SaveToString($"{{hue}};{{saturation}};{{scaleX}};{{scaleY}}");
        }
    }  //Abstract Class

    public class VoidSpearIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is VoidSpearAbstract hsac ? (int)(hsac.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }
        public override string SpriteName(int data)
        {
            return "Symbol_HellSpear";
        }
    }

    public class VoidSpear : Spear
    {
        //public new bool bugSpear => true;
        



        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color spearColor = new Color(1f, 0.8f, 0f);
            sLeaser.sprites[1].color = palette.blackColor;
            sLeaser.sprites[0].color = spearColor;

        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
            Vector2 vector = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
            if (this.vibrate > 0)
            {
                vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
            }
            Vector3 v = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            for (int i = 1; i >= 0; i--)
            {
                sLeaser.sprites[i].x = vector.x - camPos.x;
                sLeaser.sprites[i].y = vector.y - camPos.y;
                sLeaser.sprites[i].anchorY = Mathf.Lerp(this.lastPivotAtTip ? 0.85f : 0.5f, this.pivotAtTip ? 0.85f : 0.5f, timeStacker);
                sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
            }
            if (this.blink > 0 && UnityEngine.Random.value < 0.5f)
            {
                sLeaser.sprites[1].color = this.blinkColor;
            }
            else
            {
                sLeaser.sprites[1].color = this.color;
            }
            
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[1] = new FSprite("FireBugSpear", true);
            sLeaser.sprites[0] = new FSprite("FireBugSpearColor", true);
            
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            
            if (newContainer == null)
            {
                newContainer = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
            }

            newContainer.AddChild(sLeaser.sprites[1]);
            newContainer.AddChild(sLeaser.sprites[0]);

        }

        public VoidSpear(VoidSpearAbstract abstr, World world) : base(abstr, world)
        {
            
        }
    }

    

    public class VoidSpearProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = true;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            base.ScavWeaponUseScore(scav, ref score);
            score = 0;
        }
    }
}
