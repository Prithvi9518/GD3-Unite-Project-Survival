using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;

namespace GD.Engine
{
    public class AccessMachineCollider : InteractibleCollider
    {
        public AccessMachineCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            // Check whether keycard is present in inventory

            InventoryItem keycard = Application.InventoryManager.FindByName(AppData.KEYCARD_NAME);

            if (keycard != null)
            {
                // If it is, send event to remove it
                object[] parameters = { AppData.KEYCARD_NAME };
                EventDispatcher.Raise(new EventData(EventCategoryType.Inventory,
                    EventActionType.OnObjectPicked, parameters));

                // Send event to StateManager - change state to reflect that generator room has opened.
                parameters = new object[] { GameState.GeneratorRoomOpen };
                EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                    EventActionType.OnChangeState, parameters));
            }
            else
            {
                // otherwise, use dialogue to hint to user that they need some sort of keycard
                if (!(Application.StateManager.CurrentGameState == GameState.GeneratorRoomOpen))
                {
                    object[] parameters = { DialogueState.GeneratorRoomClosed };
                    EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnTriggerDialogue, parameters));

                    System.Diagnostics.Debug.WriteLine("You need a keycard");
                }
            }
        }
    }
}