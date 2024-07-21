using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;




namespace Guide.WorldChanges
{
    public static class CritStatusClass
    {
        public class CritStatus
        {
            public bool isHarvested;
            public int harvestCount;

            public bool havenScav;

            public bool isMonster;
            public bool isInfant;

            public Player player;

            public bool voidStabbed;


            public CritStatus(Creature crit)
            {

                /*UnityEngine.Random.seed = crit.abstractCreature.ID.RandomSeed;

                if (UnityEngine.Random.value < 0.2f)
                {
                    this.isMonster = true;
                }
                if (!isMonster && UnityEngine.Random.value < 0.1f)
                {
                    this.isInfant = true;

                }*/

                
            }


        }
        private static readonly ConditionalWeakTable<Creature, CritStatus> CritCWT = new();
        public static CritStatus GetCrit(this Creature crit) => CritCWT.GetValue(crit, _ => new(crit));


    }
}
