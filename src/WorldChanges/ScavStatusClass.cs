using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Guide.WorldChanges
{
    public static class ScavSatusClass
    {
        public class ScavStatus
        {
            public bool isBaby;
            public bool isWarden;
            public bool isCompanion;

            //public int age;

            public int kingMask;
            public int glyphMark;

            public ScavStatus(Scavenger scav)
            {

                //age = scav.room.world.game.GetStorySession.saveState.cycleNumber;

                UnityEngine.Random.seed = scav.abstractCreature.ID.RandomSeed;
                if (UnityEngine.Random.value < 0.2f && !scav.Elite && !scav.King)
                {
                    this.isBaby = true;
                }
                if (!isBaby && UnityEngine.Random.value < 0.1f)
                {
                    this.isWarden = true;

                }

                /*if(scav.abstractCreature.ID.number == 5144)
                {
                    this.isCompanion = true;
                }*/
            }
        }

        private static readonly ConditionalWeakTable<Scavenger, ScavStatus> ScavCWT = new();
        public static ScavStatus GetScav(this Scavenger scav) => ScavCWT.GetValue(scav, _ => new(scav));

    }
}
