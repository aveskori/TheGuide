using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using Nutils.hook;

namespace Guide.Guide
{
    internal class GuideDream : DreamNutils
    {
        public GuideDream() { }

        public override bool IsSingleWorld
        {
            get
            {
                return false;
            }
        }

        public override DreamGameSession GetSession(RainWorldGame game, SlugcatStats.Name name)
        {
            bool flag = this.tmpIndex <= 3;
            DreamGameSession result;
            if (flag)
            {
                result = new GuideDreamSession(game, SlugcatStats.Name.Red, this, this.tmpIndex);
            }
        }
        private int tmpIndex;
    }
}
