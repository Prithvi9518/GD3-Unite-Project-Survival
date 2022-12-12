using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.App
{
    public class RadioController : Component
    {
        private bool radioPlaced = false;

        public bool RadioPlaced
        {
            get => radioPlaced;
            set => radioPlaced = value;
        }

        public RadioController()
        {
            EventDispatcher.Subscribe(EventCategoryType.Inventory, HandleInventoryEvent);
        }

        private void HandleInventoryEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnRemoveInventory:
                    radioPlaced = true;
                    break;

                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}