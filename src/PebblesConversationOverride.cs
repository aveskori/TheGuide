using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugBase.Features;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;
using MoreSlugcats;
using RWCustom;

namespace Guide
{
    static class PebblesConversationOverride
    {
        public static readonly GameFeature<bool> CustomConversations = GameBool("CustomConversations");
        public static readonly Conversation.ID GuidePebblesConvo = new Conversation.ID("GuidePebblesConvo", true);
        public static void Hooks()
        {
            On.GhostConversation.AddEvents += GhostOverride;
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConvoOverride;
        }

        

        private static void PebblesConvoOverride(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            
            if (CustomConversations.TryGet(self.owner.oracle.room.game, out bool custom) && custom)
            {
                self.events = new List<Conversation.DialogueEvent>
                {
                      new Conversation.TextEvent(self, 0, self.Translate("Is this reaching you?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Why is it that you creatures insist on breaking into my structure and disrupting my work?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Not that you care, but I just dealt with one of your kind a few cycles ago. Like you, it is stuck in a cycle, a repeating pattern. Like you, it cannot leave. There was little I could do for it, just as there is little I can do for you."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Your destiny is intertwined with the Scavenger population, it seems. Bound together like family. That is what keeps you trapped here, isn't it?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("While I do not hold them or you in high regards, I acknowledge the connection you share."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("The last little beast that came through here was fueled by an insatiable rage burning from within them. You seem to be quite the opposite."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("For your safety and the safety of your family, I would strongly advise you guide them off of my facility grounds."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("The raging beast is explosive and vengeful, and I doubt it will stop until every last Scavenger here is dead."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("But we don't truly die here, do we?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("I apologize for getting off track, but you look very familiar. As I said, you are not the first little beast to wander into my chamber, and I doubt you will be the last."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Many, many cycles ago, two of your kind came to me. A mother and her child. They had your same strange adaptations. Unlike you, they were suffering, starving."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("I gave them the journey's end they sought: the old path. Being optimistic, I imagine they have made it by now, considering I have not seen them since."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Far to the west and below the earth lie your two options: Guide your new family to a safer home, but leave your kin and ascension behind. Or, abandon your new family for your kin, following their footsteps to reunite with them on the other side."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("The choice is yours little creature."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Now, leave me to my work."), 0),
                };
                Debug.Log($"CONVERSATION LOG!");
                return;
            }
           Debug.Log($"CONVERSATION LOG FAILED!");
           
           return;
            orig(self);
        }

        private static void GhostOverride(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
        {
            
        }
    }
}
