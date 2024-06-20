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
using GuideSlugBase;
using Guide.Objects;

namespace Guide.WorldChanges
{
    static class PebblesConversationOverride
    {
        #region Enums
        public static class ConvoID
        {
            public static Conversation.ID SSConvo_FirstTalk_Guide = new("SSConvo_FirstTalk_Guide", true);
            public static Conversation.ID SSConvo_Talk_Guide = new("SSConvo_Talk_Guide", true);
        }
        public static class SubBehaviorID
        {
            public static SSOracleBehavior.SubBehavior.SubBehavID SSGuide = new("SSGuide", true);

        }
        public static class Action
        {
            public static SSOracleBehavior.Action SS_Init_Guide = new("SS_Init_Guide", true);
            public static SSOracleBehavior.Action SS_MeetGuide = new("SS_Meet_Guide", true);
            public static SSOracleBehavior.Action SS_MeetGuide_Images = new("SS_MeetGuide_Images", true);
            public static SSOracleBehavior.Action SS_Talk_Guide = new("SS_Talk_Guide", true);
        }
        
        
        #endregion

        public static void Hooks()
        {

            //On.SSOracleBehavior.InitateConversation += SSOracleBehavior_InitateConversation;
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
            //On.SSOracleBehavior.SpecialEvent += PebblesConvo_AddObject;
                      
        }

        
        //Literally dont even know what i was doing here tbh, i forgor lmaoo
        /*private static void PebblesConvo_AddObject(On.SSOracleBehavior.orig_SpecialEvent orig, SSOracleBehavior self, string eventName)
        {
            Vector2 startPos = self.lastPos;
            Vector2 endPos = new(self.oracle.room.Height / 2, self.oracle.room.Width / 2);
            var lightning = new LightningBolt(startPos, endPos, 1, 0.5f, 20f);
        }*/

        private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
        {
            
            if (self.owner.oracle.room.game.Players[0].realizedCreature is Player player && player.slugcatStats.name.value != "Guide")
            {
                orig(self);
                return;
            }
            #region Helpers
            void Say(string text)
            {
                self.events.Add(new Conversation.TextEvent(self, 0, text, 0));
            }
            void Say2(string text, int initialWait, int textLinger)
            {
                self.events.Add(new Conversation.TextEvent(self, initialWait, text, textLinger));
            }
            void Wait(int initialWait)
            {
                self.events.Add(new Conversation.WaitEvent(self, initialWait));
            }
            void AddObject(int initialWait, string eventName)
            {
                self.events.Add(new Conversation.SpecialEvent(self, initialWait, eventName));
            }
            void AddSpear()
            {
                AbstractPhysicalObject lSpear = new LSpearAbstract(self.owner.oracle.room.world, self.owner.oracle.abstractPhysicalObject.pos, self.owner.oracle.room.game.GetNewID());
                self.owner.oracle.room.abstractRoom.AddEntity(lSpear);
                lSpear.RealizeInRoom();
            }
            #endregion

            if(self.id == Conversation.ID.Pebbles_White)
            {
                Say("Is this reaching you?");
                Say2("...", 0, 5);
                Say("You're not the first of your kind to crawl into my can, and I doubt you'll be the last.<LINE>" + "Understand?");
                Wait(10);
                Say("Strange beast. You do not look like a messanger,<LINE>" + "and you aren't carrying anything of value to me.");
                //if has LSpear: "That tool you have. Did you make it yourself?"
                //show displays of guide with scavengers?
                Say("My overseers alert me to occurences on my facility grounds.<LINE>" + "Most of their notifications are useless.");
                Say2("You, however...", 0, 5);
                Say("You were taken from your family. Is that right?");
                Wait(5);
                Say("The expression on your face says so.");
                //Vulture taking Guide snapshot
                Say2("To this day, I'm unsure why that vulture took you so far.<LINE>" + "To this day, I'm unsure why you were saved.", 1, 5);
                //snapshot of Scavengers killing the vulture
                Say("Since our creators departed, boredom has made me more interested in day-to-day affairs.");
                Say("So I watched you.");
                Say2("You, and your family.", 0, 5);
                //snapshot of momma slug and medium, snapshot of guide and the scavs
                Say("I became... attached... to this story playing out on my facility grounds.");
                Say("It is clear your new family lies with the Scavenger population.");
                Say("Your old family has moved on.");
                Say("Through my direction, they have taken the old path.");
                Say("If you wish to join them, you will have to take that path as well.");
                Wait(5);
                Say("However...");
                Say2("There is something I've neglected to inform you of...", 2, 5);
                Say("As I've said, you aren't the first of your kind to crawl into my can.");
                Say("A crimson creature with an explosive rage is currently...");
                Say2("...cleaning up the top of my structure.", 5, 5);
                Say("It has a hatred for your new family, and by extension it will hate you as well.<LINE>" + "No matter how similar you may be.");
                Say("For their safety as well as your own, I would strongly advise you to guide them off of my facility grounds.");
                Say("Being the genius that I am, I've already scouted a suitable home for you and your family.");
                Say2("West of the Farm Arrays, past where the land fissures, stands a wall.<LINE>" + "This section of the wall was once a transport depot.", 0, 5);
                Say("It moved both ancients and materials alike from the outer regions into my facility grounds.");
                Say("Now, the sewers are flooded, and must of the rail support has decayed. It's overgrown with new life.");
                Say("I believe a place like this will be perfect for you and your family.");
                Say("Though it is locked behind a gate. I do not want to leave it open<LINE>" + "and risk the crimson beast following you out.");
                Say("Just a moment...");
                
                //Febbles makes the LSpear??
                AddSpear();
                Say("I've placed a key within this object. It will grant you access through gates that would be otherwise inaccessible to you.");
                Say("Now go.");
                Say("Best of luck.");
                return;
            }
        }

        

        

    }

    /*public class SSOracleMeetGuide : SSOracleBehavior.ConversationBehavior
    {
        public SSOracleMeetGuide(SSOracleBehavior owner) :  base(owner, )
    }*/
}
