using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;

using SlugBase.DataTypes;
using static SlugBase.Features.FeatureTypes;
using Guide.WorldChanges;
using Guide.Creatures;
using Guide.Objects;
using Guide.Guide;
using Guide.Medium;
using SlugBase.Features;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;

namespace Guide.WorldChanges
{
    internal class CritGraphics
    {
        public static void Hooks()
        {
            On.LizardGraphics.InitiateSprites += LizardGraphics_InitiateSprites; //adds shader to crit sprites
            
            On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites; //disolve sprites if voidStabbed
            // ADD TO CONTAINER - MOVE SPRITES TO FG


            On.Weapon.HitThisObject += Weapon_HitThisObject; //Regular spears dont hit void creatures, hit with vSpear ticks voidStabbed to true and kills
            //CREATOR.CTOR - SET SELF.VOIDCREATURE TRUE
            On.AbstractCreature.ctor += AbstractCreature_ctor;
        }

        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if(creatureTemplate != null && self != null && (creatureTemplate.type == CreatureTemplate.Type.RedCentipede || creatureTemplate.type == CreatureTemplate.Type.RedLizard ||
                creatureTemplate.type == CreatureTemplate.Type.CyanLizard || creatureTemplate.type == CreatureTemplate.Type.BigEel))
            {
                self.voidCreature = true;
            }
        }

        public static void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.lizard.GetCrit().voidStabbed && self.lizard != null)
            {
                for(int i = 0; i < sLeaser.sprites.Length; i++)
                {
                   
                    sLeaser.sprites[i].alpha = Mathf.Lerp(1, 0, 0.2f);
                }
            }
        }

       

        public static void LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.lizard.abstractCreature.voidCreature && self.lizard.room.world.region.name != "HR")
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Hologram"];
                }
            }
            
        }

        //voided creatures outside of HR can only be hit by vSpears
        private static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
        {
            if (self != null && obj != null && (obj as Creature).abstractCreature.voidCreature && self.room.world.region.name != "HR")   
            {
                if(self is VoidSpear)
                {
                    (obj as Creature).GetCrit().voidStabbed = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return orig(self, obj); 
        }
    }
}
