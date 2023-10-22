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
            //On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConvoOverride;
            On.SSOracleBehavior.InitateConversation += SSOracleBehavior_InitateConversation;
        }

        private static void SSOracleBehavior_InitateConversation(On.SSOracleBehavior.orig_InitateConversation orig, SSOracleBehavior self, Conversation.ID convoId, SSOracleBehavior.ConversationBehavior convBehav)
        {
            if (self.oracle.room.game.Players[0].realizedCreature is Player player && player.slugcatStats.name.value != "Guide")
            {
                orig(self, convoId, convBehav);
                return;
            }
            if (self.oracle.room.game.Players[0].realizedCreature is Player player1 && player1.slugcatStats.name.value == "Guide")
            {
                if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 1)
                {
                    self.action = SSOracleBehavior.Action.General_MarkTalk;
                    self.dialogBox.NewMessage("Is this reaching you?", 60);
                    self.dialogBox.NewMessage("...", 60);
                    convBehav.NewAction(SSOracleBehavior.Action.General_MarkTalk, MoreSlugcatsEnums.SSOracleBehaviorAction.MeetArty_Talking);
                    
                    self.dialogBox.NewMessage("Why is it that you creatures insist on breaking into my structure and disrupting me?", 120);
                    self.dialogBox.NewMessage("Strange beast. You don't look like a messenger.", 60);
                    self.dialogBox.NewMessage("You have a deep connection to the scavenger population, it seems.", 60);
                    self.dialogBox.NewMessage("Not that you care, but you are not the first slimy beast to crawl into my chamber.", 60);
                    self.dialogBox.NewMessage("One crimson creature, with an explosive rage a few cycles ago. Two more creatures -- similar to yourself -- many, many cycles before that.", 60);
                    self.dialogBox.NewMessage("Like you, they were stuck in a cycle. Like you, they cannot leave.", 60);
                    self.dialogBox.NewMessage("There was not much I could do for them, just as there is not much I can do for you.", 60);
                    self.dialogBox.NewMessage("Though, I believe I'm safe to assume that you do not desire ascension in the way others do.", 60);
                    self.dialogBox.NewMessage("Your connection with the scavengers keeps you bound here.", 60);
                    self.dialogBox.NewMessage("The crimson beast is currently cleaning up the roof of my structure.", 60);
                    self.dialogBox.NewMessage("For your sake, and your friends' sakes, I would suggest leaving them be.", 60);
                    self.dialogBox.NewMessage("Should they finish my task, I imagine they won't stop until every scavanger on my facility grounds is dead.", 60);
                    self.dialogBox.NewMessage("Through whatever means of communication you employ with them, I strongly advise leaving.", 60);
                    self.dialogBox.NewMessage("Go west, through the Farm Arrays. You will encounter the facility wall.", 60);
                    self.dialogBox.NewMessage("There is a gate that I will unlock for you. My overseers will observe your journey.", 60);
                    self.dialogBox.NewMessage("Once you and your family pass through, I will lock the gate. You cannot come back.", 60);
                    self.dialogBox.NewMessage("Best of luck.", 60);
                    self.dialogBox.NewMessage("Now leave.", 60);
                    self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
                    
                    self.action = SSOracleBehavior.Action.ThrowOut_Polite_ThrowOut;
                    return;
                }
                if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 2)
                {
                    self.action = SSOracleBehavior.Action.MeetWhite_SecondCurious;
                    self.dialogBox.NewMessage("You're back?", 60);
                    self.dialogBox.NewMessage("Come to disrupt me more, little beast?", 120);
                    self.dialogBox.NewMessage("Or perhaps... you're curious about the other creatures I'd mentioned..?", 60);
                    self.dialogBox.NewMessage("As I said before, they looked similar to you", 60);
                    self.dialogBox.NewMessage("Unlike you, they sought a way out of this place.", 60);
                    self.dialogBox.NewMessage("Past the Farm Arrays, where the land fissures, they found their solace.", 60);
                    self.dialogBox.NewMessage("If, for some reason, you wish to abandon your new family for these creatures...", 60);
                    self.dialogBox.NewMessage("Go there.", 60);
                    self.dialogBox.NewMessage("Now, if you'll excuse me, I need to get back to work.", 60);
                    self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
                    self.action = SSOracleBehavior.Action.ThrowOut_Polite_ThrowOut;
                    return;
                }
                if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 3)
                {
                    self.action = SSOracleBehavior.Action.MeetWhite_Talking;
                    self.dialogBox.NewMessage("I have nothing else for you.", 60);
                    self.action = SSOracleBehavior.Action.ThrowOut_Polite_ThrowOut;
                    return;
                }
                
            }
            orig(self, convoId, convBehav);
        }

        /*private static void PebblesConvoOverride(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            
            if (CustomConversations.TryGet(self.owner.oracle.room.game, out bool custom) && custom)
            {
                self.events = new List<Conversation.DialogueEvent>
                {
                      new Conversation.TextEvent(self, 0, self.Translate("Is this reaching you?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 5),
                      
                      new Conversation.TextEvent(self, 0, self.Translate("Why is it that you creatures insist on breaking into my structure and disrupting my work?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Not that you care, but I just dealt with one of your kind a few cycles ago. Like you, it is stuck in a cycle, a repeating pattern.\nLike you, it cannot leave. There was little I could do for it, just as there is little I can do for you."), 5),
                      new Conversation.TextEvent(self, 0, self.Translate("Your destiny is intertwined with the Scavenger population, it seems. Bound together like family.\n That is what keeps you trapped here, isn't it?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("While I do not hold them or you in high regards, I acknowledge the connection you share."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("The last little beast that came through here was fueled by an insatiable rage burning from within them.\nYou seem to be quite the opposite."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("For your safety and the safety of your family,\nI would strongly advise you guide them off of my facility grounds."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("The raging beast is explosive and vengeful, and I doubt it will stop until every last Scavenger here is dead."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("But we don't truly die here, do we?"), 0),
                      new Conversation.TextEvent(self, 0, self.Translate(". . ."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("I apologize for getting off track, but you look very familiar. As I said, you are not the first little\n beast to wander into my chamber, and I doubt you will be the last."), 5),
                      new Conversation.TextEvent(self, 0, self.Translate("Many, many cycles ago, two of your kind came to me. A mother and her child. They had your same strange adaptations.\nUnlike you, they were suffering, starving."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("I gave them the journey's end they sought: the old path.\nBeing optimistic, I imagine they have made it by now, considering I have not seen them since."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Far to the west and below the earth lie your two options: Guide your new family to a safer home, but leave your kin and ascension behind.\nOr, abandon your new family for your kin, following their footsteps to reunite with them on the other side."), 5),
                      new Conversation.TextEvent(self, 0, self.Translate("The choice is yours little creature."), 0),
                      new Conversation.TextEvent(self, 0, self.Translate("Now, leave me to my work."), 0),
                };
               
                
            }
           
           orig(self);
           
           
        }*/

        private static void GhostOverride(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
        {
            
        }
    }
}
