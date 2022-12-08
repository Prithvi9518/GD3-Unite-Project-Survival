using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;

namespace GD.Engine
{
    public class ExitDoorCollider : InteractibleCollider
    {
        public ExitDoorCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            // send event to game state to check time remaining. If time is still left,
            // change state to win state
            EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                EventActionType.OnReachExit));
        }
    }
}