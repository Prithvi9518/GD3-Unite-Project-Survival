using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public InventoryItemData itemData { get; private set; }
        public int amount { get; private set; }

        public InventoryItem(InventoryItemData itemData)
        {
            this.itemData = itemData;
            Increment();
        }

        public void Increment()
        {
            amount++;
        }

        public void Decrement()
        {
            amount--;
        }
    }

    public struct InventoryItemData
    {
        public string uniqueID;
        public string name;
        public ItemType itemType;
        public string description;
        public Texture2D icon;
        public string cueName;
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

        public void Add(InventoryItemData itemData)
        {
            if (inventory.Exists(x => (itemData.uniqueID == x.itemData.uniqueID)))
            {
                inventory.Find(x => (itemData.uniqueID == x.itemData.uniqueID)).Increment();
            }
            else
            {
                inventory.Add(new InventoryItem(itemData));
            }
        }

        public void Remove(InventoryItemData itemData)
        {
            Predicate<InventoryItem> uniqueIDMatch = x => (x.itemData.uniqueID == itemData.uniqueID);

            if (inventory.Exists(uniqueIDMatch))
            {
                inventory.Find(uniqueIDMatch).Decrement();

                InventoryItem item = inventory.Find(uniqueIDMatch);
                if (item.amount <= 0)
                    inventory.Remove(item);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Item does not exist in inventory");
            }
        }

        public void PrintInventory()
        {
            foreach (InventoryItem item in inventory)
                Console.WriteLine("Item name: " + item.itemData.name);
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
        }
    }
}