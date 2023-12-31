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
                    self.action = MoreSlugcatsEnums.SSOracleBehaviorAction.MeetArty_Init;
                    self.dialogBox.NewMessage("Is this reaching you?", 60);
                    self.dialogBox.NewMessage("...", 60);
                    self.action = MoreSlugcatsEnums.SSOracleBehaviorAction.Pebbles_SlumberParty;
                    
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

        private static void GhostOverride(On.GhostConversation.orig_AddEvents orig, GhostConversation self)
        {
            
        }
    }
}
