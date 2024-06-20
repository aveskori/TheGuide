using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using MoreSlugcats;
using Nutils.hook;

namespace Guide.Guide
{
    internal class GuideDreamSession : DreamGameSession
    {
        public GuideDreamSession(RainWorldGame game, SlugcatStats.Name name, DreamNutils ow, int dreamIndex) : base(game, name, ow)
        {
            this.dreamIndex = dreamIndex;
        }


    }
}
