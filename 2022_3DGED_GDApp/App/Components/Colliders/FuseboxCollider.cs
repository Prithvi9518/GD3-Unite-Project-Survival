using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;

namespace GD.Engine
{
    public class FuseboxCollider : InteractibleCollider
    {
        public FuseboxCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            // Check whether fuse is present in inventory
            InventoryItem fuse = Application.InventoryManager.FindByName(AppData.FUSE_NAME);
            if (fuse != null)
            {
                // Check whether generator room has been opened with keycard first
                if (Application.StateManager.CurrentGameState != GameState.GeneratorRoomOpen)
                    return;

                // Send event to remove fuse
                object[] parameters = { AppData.FUSE_NAME };
                EventDispatcher.Raise(new EventData(EventCategoryType.Inventory,
                    EventActionType.OnObjectPicked, parameters));

                // Send event to StateManager - change state to reflect that generator is now on.
                parameters = new object[] { GameState.GeneratorOn };
                EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                    EventActionType.OnChangeState, parameters));
            }
            else
            {
                // otherwise, send message/hint to user that they need a fuse
                System.Diagnostics.Debug.WriteLine("You need a fuse");
            }
        }
    }
}