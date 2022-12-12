using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GD.App
{
    public enum DialogueState
    {
        NoDialogue,

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
        PickRightFuse,
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

    public class DialogueController : Component
    {
        #region Dictionaries

        private Dictionary<DialogueState, string> subtitlesDict = new Dictionary<DialogueState, string>
        {
            [DialogueState.Start1] = "I've got about three minutes to get out of here before the infection takes over.",
            [DialogueState.Start2] = "Looks like there's an exit door ahead. I can't go back the way  I came.",

            [DialogueState.ExitDoorNoPower] = "The door isn't opening. Looks like I need to restore the power first.",

            [DialogueState.GeneratorRoomClosed] = "AHHH dammit! I can't get in here.",

            [DialogueState.HollowBlockOffice] = "There is a hollow blocking the way. I need to distract it somehow.",
            [DialogueState.NeedKeycardInOffice] = "I need a keycard for the generator room. I hope there's one in here.",
            [DialogueState.NoteInOffice] = "Note mentions the generator being busted. Maybe this keycard will open the generator room?",

            [DialogueState.GeneratorNotWorking] = "Of course the generator isn't working!!",

            [DialogueState.NeedFuse] = "I need a fuse. Surely there's one in here somewhere?",
            [DialogueState.WhereFuse] = "Where would I find a fuse? Maybe I'll check the electronic aisle.",
            [DialogueState.PickRightFuse] = "The fuse box only takes 220 volts. I have to be careful not to take the wrong one.",

            [DialogueState.GeneratorWorking] = "The generator is working again!!",
            [DialogueState.TimeToRun] = "Time to GET OUTTA HERE!!!",

            [DialogueState.GeneralDialogue1] = "Riverside. I remember this place. It used to be great during the Halloween break." +
            " Scared the crap out of me.",
            [DialogueState.GeneralDialogue2] = "I have to get out of here. I'm not going to die in a supermarket.",
            [DialogueState.GeneralDialogue3] = "I have to get out of here. This can't be the end of me.",

            [DialogueState.Time1] = "I don't have much time left! I need to find a way out.",
            [DialogueState.Time2] = "I have to get out of here. This can't be the end of me."
        };

        private Dictionary<DialogueState, float> durationsDict = new Dictionary<DialogueState, float>
        {
            [DialogueState.Start1] = 4500,
            [DialogueState.Start2] = 4500,

            [DialogueState.ExitDoorNoPower] = 4500,

            [DialogueState.GeneratorRoomClosed] = 3500,

            [DialogueState.HollowBlockOffice] = 4500,
            [DialogueState.NeedKeycardInOffice] = 3500,
            [DialogueState.NoteInOffice] = 4000,

            [DialogueState.GeneratorNotWorking] = 4000,

            [DialogueState.NeedFuse] = 4000,
            [DialogueState.WhereFuse] = 4500,
            [DialogueState.PickRightFuse] = 4000,

            [DialogueState.GeneratorWorking] = 2000,
            [DialogueState.TimeToRun] = 2500,

            [DialogueState.GeneralDialogue1] = 5500,
            [DialogueState.GeneralDialogue2] = 3500,
            [DialogueState.GeneralDialogue3] = 3500,

            [DialogueState.Time1] = 4000,
            [DialogueState.Time2] = 4000
        };

        private Dictionary<DialogueState, float> delayedSubtitlesDurations = new Dictionary<DialogueState, float>
        {
            [DialogueState.GeneralDialogue1] = 5000,
            [DialogueState.GeneralDialogue3] = 25000,
            [DialogueState.WhereFuse] = 5000,
            [DialogueState.PickRightFuse] = 2000
        };

        private Dictionary<DialogueState, string> dialogueObjectDict = new Dictionary<DialogueState, string>
        {
            [DialogueState.Start1] = AppData.INTRO_DIALOGUE,
            [DialogueState.GeneralDialogue1] = AppData.RIVERSIDE_MONOLOGUE_DIALOGUE,

            [DialogueState.GeneratorNotWorking] = AppData.GENERATOR_NOT_WORKING_DIALOGUE,
            [DialogueState.NeedFuse] = AppData.FUSE_SOMEWHERE_DIALOGUE,
            [DialogueState.WhereFuse] = AppData.WHERE_FIND_FUSE_DIALOGUE,
            [DialogueState.PickRightFuse] = AppData.PICK_RIGHT_FUSE_DIALOGUE,

            [DialogueState.HollowBlockOffice] = AppData.HOLLOW_IN_THE_WAY_DIALOGUE,
            [DialogueState.NoteInOffice] = AppData.GENERATOR_BUSTED_DIALOGUE,

            [DialogueState.TimeToRun] = AppData.TIME_TO_GET_OUT_OF_HERE_DIALOGUE,

            [DialogueState.Time1] = AppData.FIND_A_WAY_OUT_DIALOGUE,
            [DialogueState.Time2] = AppData.CANT_BE_THE_END_OF_ME_DIALOGUE,
        };

        #endregion Dictionaries

        private List<List<DialogueState>> dialogueSequences = new List<List<DialogueState>>()
        {
            // Starting and general dialogues
            new List<DialogueState>(){
                DialogueState.Start1,
                DialogueState.Start2,
                DialogueState.GeneralDialogue1
            },

            new List<DialogueState>(){DialogueState.NeedFuse, DialogueState.WhereFuse, DialogueState.PickRightFuse},

            // Dialogue when generator turns on
            new List<DialogueState>(){DialogueState.GeneratorWorking, DialogueState.TimeToRun}
        };

        private DialogueState currentDialogue;
        private TextMaterial2D textMaterial2D;

        private float durationInMS;
        private float totalElapsedTimeInMS;

        private float tempElapsedTimeInMS;

        private bool dialoguePlayed = false;

        public DialogueState CurrentSubtitle
        {
            get => currentDialogue;
        }

        public DialogueController()
        {
            this.currentDialogue = DialogueState.Start1;
            this.durationInMS = durationsDict.GetValueOrDefault(currentDialogue, 0);

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
            foreach (List<DialogueState> sequence in dialogueSequences)
            {
                // If it is, check it's index in the sequence
                if (sequence.Contains(currentDialogue))
                {
                    int index = sequence.IndexOf(currentDialogue);

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
                        ChangeSubtitle(DialogueState.NoDialogue);

                    return true;
                }
            }

            return false;
        }

        private void ChangeSubtitle(DialogueState newDialogue)
        {
            currentDialogue = newDialogue;
            totalElapsedTimeInMS = 0;
            durationInMS = durationsDict.GetValueOrDefault(currentDialogue, 0);

            if (dialogueObjectDict.ContainsKey(currentDialogue))
                dialoguePlayed = false;
        }

        public override void Update(GameTime gameTime)
        {
            textMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextMaterial2D;

            textMaterial2D.StringBuilder.Clear();

            if (currentDialogue == DialogueState.GeneralDialogue1)
            {
                textMaterial2D.TextOffset = new Vector2(-200, 0);
            }
            string text = subtitlesDict.GetValueOrDefault(currentDialogue, "");
            text = (text != "") ? "Ava:  " + text : text;

            textMaterial2D.StringBuilder.Append(text);

            PlayCurrentDialogue();

            if (currentDialogue != DialogueState.NoDialogue)
                totalElapsedTimeInMS += gameTime.ElapsedGameTime.Milliseconds;

            if (totalElapsedTimeInMS > durationInMS)
            {
                if (HandleSequencedSubtitleChange(gameTime)) return;

                ChangeSubtitle(DialogueState.NoDialogue);

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

        private void PlayCurrentDialogue()
        {
            if (!dialoguePlayed)
            {
                string dialogueObjectName = dialogueObjectDict.GetValueOrDefault(this.currentDialogue, "");
                object[] parameters = { dialogueObjectName };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

                dialoguePlayed = true;
            }
        }
    }
}