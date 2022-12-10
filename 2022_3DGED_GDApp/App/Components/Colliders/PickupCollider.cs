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
        }

        protected override void RaiseCollisionResponseEvents()
        {
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