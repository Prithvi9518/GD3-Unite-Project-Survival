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
            InventoryItem fuse = Application.InventoryManager.FindByName(AppData.FUSE_220V_NAME);
            if (fuse != null)
            {
                // Check whether generator room has been opened with keycard first
                if (Application.StateManager.CurrentGameState != GameState.GeneratorRoomOpen)
                    return;

                // Send event to remove fuse
                object[] parameters = { AppData.FUSE_220V_NAME };
                EventDispatcher.Raise(new EventData(EventCategoryType.Inventory,
                    EventActionType.OnObjectPicked, parameters));

                // Send event to StateManager - change state to reflect that fuse is now in.
                parameters = new object[] { GameState.FuseIn };
                EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                    EventActionType.OnChangeState, parameters));
            }
            else
            {
                // Check whether fuse is present in inventory
                InventoryItem wrongFuse = Application.InventoryManager.FindByName(AppData.FUSE_440V_NAME);
                if (wrongFuse != null)
                {
                    // use dialogue to hint to user that they have the wrong fuse
                    if (!(Application.StateManager.CurrentGameState == GameState.FuseIn))
                    {
                        object[] parameters = { DialogueState.PickRightFuse };
                        EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnTriggerDialogue, parameters));

                        System.Diagnostics.Debug.WriteLine("You have the wrong fuse");
                    }
                }
                else
                {
                    // otherwise, use dialogue to hint to user that they need a fuse
                    if (!(Application.StateManager.CurrentGameState == GameState.FuseIn))
                    {
                        object[] parameters = { DialogueState.NeedFuse };
                        EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnTriggerDialogue, parameters));

                        System.Diagnostics.Debug.WriteLine("You need a fuse");
                    }
                }
            }
        }
    }
}