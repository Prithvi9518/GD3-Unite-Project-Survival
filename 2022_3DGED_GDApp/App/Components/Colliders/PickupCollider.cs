using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.Engine
{
    public class PickupCollider : InteractibleCollider
    {
        public PickupCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void RaiseButtonPromptUIEvent(bool isActive)
        {
            string text = "";
            if (isActive)
                text = (Input.Gamepad.IsConnected()) ? "Press Y to pick up" : "Press E to pick up";

            object[] parameters = { AppData.INTERACT_PROMPT_NAME, isActive, text, new Vector3(0, 0, 1) };

            EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnToggleButtonPrompt, parameters));
        }

        protected override void HandleInteraction()
        {
            RaiseItemPickupEvents();
        }

        private void RaiseItemPickupEvents()
        {
            // Play pickup sound effect
            object[] parameters = { "pickup-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

            // Raise event to add item to inventory
            InventoryItemData itemData = new InventoryItemData(
                this.gameObject.Name,
                this.gameObject.Name,
                ItemType.Quest,
                "test",
                null,
                null,
                this.gameObject
                );
            parameters = new object[] { gameObject, itemData };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.Pickup, EventActionType.OnPickup, parameters)
                );
        }
    }
}