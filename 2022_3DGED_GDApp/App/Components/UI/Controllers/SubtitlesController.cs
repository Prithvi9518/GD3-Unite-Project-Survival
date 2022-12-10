using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
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
        };

        private Dictionary<SubtitleState, float> durationsDict = new Dictionary<SubtitleState, float>
        {
            [SubtitleState.Start1] = 4500,
            [SubtitleState.Start2] = 4500
        };

        private List<List<SubtitleState>> subtitleSequences = new List<List<SubtitleState>>()
        {
            new List<SubtitleState>(){SubtitleState.Start1, SubtitleState.Start2}
        };

        private SubtitleState currentSubtitle;
        private TextMaterial2D textMaterial2D;

        private float durationInMS;
        private float totalElapsedTimeInMS;

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
                    break;

                default:
                    break;
            }
        }

        private void HandleSequencedSubtitleChange()
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
                }
            }
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
            textMaterial2D.StringBuilder.Append(subtitlesDict.GetValueOrDefault(currentSubtitle, ""));

            if (currentSubtitle != SubtitleState.NoSubtitle)
                totalElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;

            if (totalElapsedTimeInMS > durationInMS)
            {
                //currentSubtitle = SubtitleState.NoSubtitle;
                HandleSequencedSubtitleChange();
            }
        }
    }
}