using GD.App;
using GD.Engine.Events;

namespace GD.Engine
{
    public class PickupCollider : InteractibleCollider
    {
        public PickupCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            RaiseCollectibleEvents();
        }

        private void RaiseCollectibleEvents()
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