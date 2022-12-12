using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GD.App
{
    public enum DialogueState
    {
        NoDialogue,

        Intro
    }

    public class DialogueManager : PausableGameComponent
    {
        private Dictionary<DialogueState, string> dialoguesDict = new Dictionary<DialogueState, string>
        {
            [DialogueState.Intro] = AppData.INTRO_DIALOGUE
        };

        private DialogueState currentDialogue;

        public DialogueManager(Game game, StatusType statusType)
            : base(game, statusType)
        {
            this.currentDialogue = DialogueState.Intro;
        }

        private void PlayCurrentDialogue()
        {
            object[] parameters = { dialoguesDict.GetValueOrDefault(currentDialogue, "") };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

            currentDialogue = DialogueState.NoDialogue;
        }

        public override void Update(GameTime gameTime)
        {
            if (!isPausedOnPlay)
            {
                PlayCurrentDialogue();
            }
        }
    }
}