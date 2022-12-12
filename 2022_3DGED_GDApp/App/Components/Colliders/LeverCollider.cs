using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;

namespace GD.App
{
    public class LeverCollider : InteractibleCollider
    {
        public LeverCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            object[] parameters;

            if (!(Application.StateManager.CurrentGameState == GameState.FuseIn))
            {
                parameters = new object[] { DialogueState.GeneratorNotWorking };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
            }
            else
            {
                // Send event to StateManager - change state to reflect that generator is now on.
                parameters = new object[] { GameState.GeneratorOn };
                EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                    EventActionType.OnChangeState, parameters));
            }
        }
    }
}