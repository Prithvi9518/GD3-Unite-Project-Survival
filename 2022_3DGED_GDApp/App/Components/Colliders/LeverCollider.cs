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
            InventoryItem fuse = Application.InventoryManager.FindByName(AppData.FUSE_220V_NAME);
            if (fuse != null)
            {
            }
            else
            {
                if (!(Application.StateManager.CurrentGameState == GameState.GeneratorOn))
                {
                    object[] parameters = { DialogueState.GeneratorNotWorking };
                    EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
                }
            }
        }
    }
}