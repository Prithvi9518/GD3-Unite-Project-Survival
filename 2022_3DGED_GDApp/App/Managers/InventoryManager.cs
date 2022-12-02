using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD.App
{
    public enum ItemType : sbyte
    {
        Story,
        Health,
        Ammo,
        Quest,
        Prop
    }

    public class InventoryItem
    {
        public string uniqueID;
        public string name;
        public ItemType itemType;
        public string description;
        public int value;
        public string cueName;  //"boom"
    }

    public class InventoryManager : PausableDrawableGameComponent
    {
        private List<InventoryItem> inventory;

        public InventoryManager(Game game, StatusType statusType) : base(game, statusType)
        {
            this.inventory = new List<InventoryItem>();
            EventDispatcher.Subscribe(EventCategoryType.Pickup, HandlePickupEvent);
        }

        private void HandlePickupEvent(EventData eventData)
        {
        }

        public void PrintInventory()
        {
            foreach (InventoryItem item in inventory)
                Console.WriteLine("Item name: " + item.name);
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
        }
    }
}