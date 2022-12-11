using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using System;

namespace GD.Engine
{
    public class PickupCollider : InteractibleCollider
    {
        private bool pickedUp = false;

        public PickupCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
            EventDispatcher.Subscribe(EventCategoryType.Inventory, HandleInventoryEvent);
        }

        private void HandleInventoryEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnRemoveInventory:
                    string name = eventData.Parameters[0] as string;
                    if (name == gameObject.Name)
                    {
                        pickedUp = false;
                    }
                    break;

                default:
                    break;
            }
        }

        protected override void CheckButtonPromptState()
        {
            float distance = Vector3.Distance(playerGameObject.Transform.Translation, gameObject.Transform.Translation);
            if (distance > AppData.INTERACTION_DISTANCE)
            {
                RaiseButtonPromptUIEvent(PromptState.NoPrompt);
            }
            else
            {
                RaiseButtonPromptUIEvent(PromptState.PickupPrompt);
            }
        }

        protected override void HandleInteraction()
        {
            RaiseItemPickupEvents();
        }

        private void RaiseItemPickupEvents()
        {
            // boolean to prevent sound playing multiple times
            if (!pickedUp)
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

            pickedUp = true;
        }
    }
}