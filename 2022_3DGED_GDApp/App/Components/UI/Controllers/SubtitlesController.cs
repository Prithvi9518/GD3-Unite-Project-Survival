﻿using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GD.App
{
    public enum DialogueState
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
        #region Dictionaries

        private Dictionary<DialogueState, string> subtitlesDict = new Dictionary<DialogueState, string>
        {
            [DialogueState.Start1] = "I've got about three minutes to get out of here before the infection takes over.",
            [DialogueState.Start2] = "Looks like there's an exit door ahead. I can't go back the way  I came.",

            [DialogueState.ExitDoorNoPower] = "The door isn't opening. Looks like I need to restore the power first.",

            [DialogueState.GeneratorRoomClosed] = "AHHH dammit! I can't get in here.",

            [DialogueState.NeedKeycardInOffice] = "I need a keycard for the generator room. I hope there's one in here.",
            [DialogueState.NoteInOffice] = "Note mentions the generator being busted. Maybe this keycard will open the generator room?",

            [DialogueState.NeedFuse] = "I need a fuse. Surely there's one in here somewhere?",
            [DialogueState.WhereFuse] = "Where would I find a fuse? Maybe I'll check the electronic aisle.",

            [DialogueState.GeneratorWorking] = "The generator is working again!!",
            [DialogueState.TimeToRun] = "Time to GET OUTTA HERE!!!",

            [DialogueState.GeneralDialogue1] = "Riverside. I remember this place. It used to be great during the Halloween break." +
            " Scared the crap out of me.",
            [DialogueState.GeneralDialogue2] = "I have to get out of here. I'm not going to die in a supermarket.",
            [DialogueState.GeneralDialogue3] = "I have to get out of here. This can't be the end of me.",

            [DialogueState.Time1] = "I don't have much time left! I need to find a way out."
        };

        private Dictionary<DialogueState, float> durationsDict = new Dictionary<DialogueState, float>
        {
            [DialogueState.Start1] = 4500,
            [DialogueState.Start2] = 4500,

            [DialogueState.ExitDoorNoPower] = 4500,

            [DialogueState.GeneratorRoomClosed] = 3500,

            [DialogueState.NeedKeycardInOffice] = 3500,
            [DialogueState.NoteInOffice] = 4000,

            [DialogueState.NeedFuse] = 4000,
            [DialogueState.WhereFuse] = 4000,

            [DialogueState.GeneratorWorking] = 2500,
            [DialogueState.TimeToRun] = 2500,

            [DialogueState.GeneralDialogue1] = 5000,
            [DialogueState.GeneralDialogue2] = 3500,
            [DialogueState.GeneralDialogue3] = 3500,

            [DialogueState.Time1] = 4000
        };

        private Dictionary<DialogueState, float> delayedSubtitlesDurations = new Dictionary<DialogueState, float>
        {
            [DialogueState.GeneralDialogue1] = 5000,
            [DialogueState.GeneralDialogue3] = 15000,
            [DialogueState.WhereFuse] = 5000
        };

        #endregion Dictionaries

        private List<List<DialogueState>> subtitleSequences = new List<List<DialogueState>>()
        {
            // Starting and general dialogues
            new List<DialogueState>(){
                DialogueState.Start1,
                DialogueState.Start2,
                DialogueState.GeneralDialogue1,
                DialogueState.GeneralDialogue3
            },

            new List<DialogueState>(){DialogueState.NeedFuse, DialogueState.WhereFuse},

            // Dialogue when generator turns on
            new List<DialogueState>(){DialogueState.GeneratorWorking, DialogueState.TimeToRun}
        };

        private DialogueState currentSubtitle;
        private TextMaterial2D textMaterial2D;

        private float durationInMS;
        private float totalElapsedTimeInMS;

        private float tempElapsedTimeInMS;

        public SubtitlesController()
        {
            this.currentSubtitle = DialogueState.Start1;
            this.durationInMS = durationsDict.GetValueOrDefault(currentSubtitle, 0);

            EventDispatcher.Subscribe(EventCategoryType.UI, HandleUIEvent);
        }

        private void HandleUIEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnShowSubtitles:
                    DialogueState newState = (DialogueState)eventData.Parameters[0];
                    ChangeSubtitle(newState);
                    break;

                default:
                    break;
            }
        }

        private bool HandleSequencedSubtitleChange(GameTime gameTime)
        {
            // Check if the current subtitle is part of a sequence
            foreach (List<DialogueState> sequence in subtitleSequences)
            {
                // If it is, check it's index in the sequence
                if (sequence.Contains(currentSubtitle))
                {
                    int index = sequence.IndexOf(currentSubtitle);

                    // If it's not the last subtitle in the sequence, then move to the next subtitle
                    if (index != sequence.Count - 1)
                    {
                        DialogueState newSubtitle = sequence[index + 1];

                        // Check if there needs to be a delay between the current and next subtitles
                        if (delayedSubtitlesDurations.ContainsKey(newSubtitle))
                        {
                            textMaterial2D.StringBuilder.Clear();
                            DelayBetweenSubtitles(newSubtitle,
                                gameTime,
                                delayedSubtitlesDurations.GetValueOrDefault(newSubtitle, 0)
                                );
                        }
                        else
                        {
                            ChangeSubtitle(sequence[index + 1]);
                        }
                    }
                    else
                        ChangeSubtitle(DialogueState.NoSubtitle);

                    return true;
                }
            }

            return false;
        }

        private void ChangeSubtitle(DialogueState newSubtitle)
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

            if (currentSubtitle != DialogueState.NoSubtitle)
                totalElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;

            if (totalElapsedTimeInMS > durationInMS)
            {
                if (HandleSequencedSubtitleChange(gameTime)) return;

                ChangeSubtitle(DialogueState.NoSubtitle);

                //SubtitleState prevSubtitle = currentSubtitle;

                //textMaterial2D.StringBuilder.Clear();

                //if (prevSubtitle == SubtitleState.NeedFuse)
                //{
                //    DelayBetweenSubtitles(SubtitleState.WhereFuse,
                //        gameTime,
                //        delayedSubtitlesDurations.GetValueOrDefault(SubtitleState.WhereFuse)
                //        );
                //}
                //else
                //{
                //    tempElapsedTimeInMS = 0;
                //    ChangeSubtitle(SubtitleState.NoSubtitle);
                //}
            }
        }

        private void DelayBetweenSubtitles(DialogueState newSubtitle, GameTime gameTime, float delayInMS)
        {
            tempElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;
            if (tempElapsedTimeInMS > delayInMS)
            {
                ChangeSubtitle(newSubtitle);
                tempElapsedTimeInMS = 0;
            }
        }
    }
}