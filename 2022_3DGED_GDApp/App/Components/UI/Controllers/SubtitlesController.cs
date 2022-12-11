using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GD.App
{
    public enum SubtitleState
    {
        NoSubtitle,

        Start1, //“I’ve got about three minutes to get out of here before the infection takes over.”
        Start2, //“Looks like there’s an exit door ahead. I can’t go back the way I came.”

        ExitDoorNoPower, //“The door isn’t opening. Looks I need to restore the power.”

        HollowBlockOffice, //“There is a hollow blocking the way. I need to distract it somehow.”
        NeedKeycardInOffice, //“I need a key card for the generator room. I hope there’s one in here.”
        NoteInOffice, //“Note mentions the generator being busted. Maybe this key card will open the generator room?”

        GeneratorRoomClosed, //“AHHH dammit! I can’t get in here.”
        GeneratorNotWorking, //“Off course the generator isn’t working!!!!!!”
        NeedFuse, //“I need a fuse. Surely there’s one in here somewhere?”
        WhereFuse, //“Where would I find the fuse? Maybe I’ll check the electronic aisle.”
        GeneratorWorking, //“The generator is working again!!!”.
        TimeToRun, //“Time to GET OUTTA HERE!

        MultipleFuses, //“There are multiple fuses here. I should take a few in case one of them is broken.”

        GeneralDialogue1, //“Riverside. I remember this place. It used to be great during the Halloween break. Scared the crap out of me.”
        GeneralDialogue2, //“I have to get out of here. I’m not going to die in a supermarket.”
        GeneralDialogue3, //“I have to get out of here. This can’t be the end of me.”

        AlcoholBottle, //“I can use the bottles here to distract the hollow.”
        AlcoholSmell,

        Time1, //“I don’t have much time left! I need to find a way out.”
        Time2, //“My skin is starting to itch!!!”
        Time3, //“I don’t feel so good!!”
        Time4 //“AHHHHHHHH!!!! That is painful!”
    }

    public class SubtitlesController : Component
    {
        private Dictionary<SubtitleState, string> subtitlesDict = new Dictionary<SubtitleState, string>
        {
            [SubtitleState.Start1] = "I've got about three minutes to get out of here before the infection takes over.",
            [SubtitleState.Start2] = "Looks like there's an exit door ahead. I can't go back the way  I came.",

            [SubtitleState.ExitDoorNoPower] = "The door isn't opening. Looks like I need to restore the power first.",

            [SubtitleState.GeneratorRoomClosed] = "AHHH dammit! I can't get in here.",

            [SubtitleState.NeedFuse] = "I need a fuse. Surely there's one in here somewhere?",
            [SubtitleState.WhereFuse] = "Where would I find a fuse? Maybe I'll check the electronic aisle.",

            [SubtitleState.GeneratorWorking] = "The generator is working again!!",
            [SubtitleState.TimeToRun] = "Time to GET OUTTA HERE!!!"
        };

        private Dictionary<SubtitleState, float> durationsDict = new Dictionary<SubtitleState, float>
        {
            [SubtitleState.Start1] = 4500,
            [SubtitleState.Start2] = 4500,

            [SubtitleState.ExitDoorNoPower] = 4500,

            [SubtitleState.GeneratorRoomClosed] = 3500,

            [SubtitleState.NeedFuse] = 4000,
            [SubtitleState.WhereFuse] = 4000,

            [SubtitleState.GeneratorWorking] = 2500,
            [SubtitleState.TimeToRun] = 2500
        };

        private List<List<SubtitleState>> subtitleSequences = new List<List<SubtitleState>>()
        {
            new List<SubtitleState>(){SubtitleState.Start1, SubtitleState.Start2},
            new List<SubtitleState>(){SubtitleState.GeneratorWorking, SubtitleState.TimeToRun}
        };

        private SubtitleState currentSubtitle;
        private TextMaterial2D textMaterial2D;

        private float durationInMS;
        private float totalElapsedTimeInMS;

        private float tempTimerInMS = 5000;
        private float tempElapsedTimeInMS;

        public SubtitlesController()
        {
            this.currentSubtitle = SubtitleState.Start1;
            this.durationInMS = durationsDict.GetValueOrDefault(currentSubtitle, 0);

            EventDispatcher.Subscribe(EventCategoryType.UI, HandleUIEvent);
        }

        private void HandleUIEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnShowSubtitles:
                    SubtitleState newState = (SubtitleState)eventData.Parameters[0];
                    ChangeSubtitle(newState);
                    break;

                default:
                    break;
            }
        }

        private bool HandleSequencedSubtitleChange()
        {
            // Check if the current subtitle is part of a sequence
            foreach (List<SubtitleState> sequence in subtitleSequences)
            {
                // If it is, check it's index in the sequence
                if (sequence.Contains(currentSubtitle))
                {
                    int index = sequence.IndexOf(currentSubtitle);

                    // If it's not the last subtitle in the sequence, then move to the next subtitle
                    if (index != sequence.Count - 1)
                        ChangeSubtitle(sequence[index + 1]);
                    else
                        ChangeSubtitle(SubtitleState.NoSubtitle);

                    return true;
                }
            }

            return false;
        }

        private void ChangeSubtitle(SubtitleState newSubtitle)
        {
            currentSubtitle = newSubtitle;
            totalElapsedTimeInMS = 0;
            durationInMS = durationsDict.GetValueOrDefault(currentSubtitle, 0);
        }

        public override void Update(GameTime gameTime)
        {
            textMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextMaterial2D;

            textMaterial2D.StringBuilder.Clear();

            string text = subtitlesDict.GetValueOrDefault(currentSubtitle, "");
            text = (text != "") ? "Ava:  " + text : text;

            textMaterial2D.StringBuilder.Append(text);

            if (currentSubtitle != SubtitleState.NoSubtitle)
                totalElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;

            if (totalElapsedTimeInMS > durationInMS)
            {
                if (HandleSequencedSubtitleChange()) return;

                SubtitleState prevSubtitle = currentSubtitle;

                textMaterial2D.StringBuilder.Clear();

                if (prevSubtitle == SubtitleState.NeedFuse)
                {
                    DelayBetweenSubtitles(SubtitleState.WhereFuse, gameTime);
                }
                else
                {
                    tempElapsedTimeInMS = 0;
                    ChangeSubtitle(SubtitleState.NoSubtitle);
                }
            }
        }

        private void DelayBetweenSubtitles(SubtitleState newSubtitle, GameTime gameTime)
        {
            tempElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;
            if (tempElapsedTimeInMS > tempTimerInMS)
            {
                ChangeSubtitle(SubtitleState.WhereFuse);
            }
        }
    }
}