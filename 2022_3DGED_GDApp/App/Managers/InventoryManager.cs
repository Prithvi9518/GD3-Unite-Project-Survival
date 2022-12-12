using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using JigLibX.Collision;
using JigLibX.Geometry;
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

    public class InventoryItemData
    {
        public string uniqueID;
        public string name;
        public ItemType itemType;
        public string description;
        public Texture2D icon;
        public string cueName;
        public GameObject gameObject;

        public InventoryItemData(string uniqueID, string name, ItemType itemType, string description,
            Texture2D icon, string cueName, GameObject gameObject)
        {
            this.uniqueID = uniqueID;
            this.name = name;
            this.itemType = itemType;
            this.description = description;
            this.icon = icon;
            this.cueName = cueName;
            this.gameObject = gameObject;
        }
    }

    public class InventoryManager : PausableDrawableGameComponent
    {
        private List<InventoryItem> inventory;

        public InventoryManager(Game game, StatusType statusType) : base(game, statusType)
        {
            this.inventory = new List<InventoryItem>();
            EventDispatcher.Subscribe(EventCategoryType.Pickup, HandlePickupEvent);
            EventDispatcher.Subscribe(EventCategoryType.Inventory, HandleInventoryEvent);
        }

        private void HandlePickupEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnPickup:
                    InventoryItemData itemData = eventData.Parameters[1] as InventoryItemData;
                    Add(itemData);
                    PrintInventory();
                    break;

                default:
                    break;
            }
        }

        private void HandleInventoryEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnObjectPicked:
                    string id = eventData.Parameters[0] as string;
                    UseItem(id);
                    break;

                default:
                    break;
            }
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

        public InventoryItem FindByName(string name)
        {
            Predicate<InventoryItem> uniqueIDMatch = x => (x.itemData.uniqueID == name);

            if (inventory.Exists(uniqueIDMatch))
            {
                return inventory.Find(uniqueIDMatch);
            }

            return null;
        }

        public void UseItem(string uniqueID)
        {
            Predicate<InventoryItem> uniqueIDMatch = x => (x.itemData.uniqueID == uniqueID);

            if (inventory.Exists(uniqueIDMatch))
            {
                InventoryItem item = inventory.Find(uniqueIDMatch);
                System.Diagnostics.Debug.WriteLine("Using " + item.itemData.name);

                GameObject gameObject = item.itemData.gameObject;

                #region Check for PickupCollider

                PickupCollider pickupCollider = gameObject.GetComponent<PickupCollider>();
                if (pickupCollider != null)
                {
                    object[] parameters = { gameObject.Name };
                    EventDispatcher.Raise(new EventData(EventCategoryType.Inventory, EventActionType.OnRemoveInventory, parameters));
                }

                #endregion Check for PickupCollider

                #region Check for InteractableBehaviour - used when having problems with collider

                InteractableBehaviour interactableBehaviour = gameObject.GetComponent<InteractableBehaviour>();
                if (interactableBehaviour != null)
                {
                    System.Diagnostics.Debug.WriteLine("InteractableBehaviour found");

                    object[] parameters = { gameObject.Name };
                    EventDispatcher.Raise(new EventData(EventCategoryType.Inventory, EventActionType.OnRemoveInventory, parameters));
                }

                #endregion Check for InteractableBehaviour - used when having problems with collider

                #region Check For Radio

                RadioController radioController = gameObject.GetComponent<RadioController>();

                if (radioController != null)
                {
                    Transform playerTransform = Application.CameraManager.ActiveCamera.transform;

                    if (interactableBehaviour != null)
                    {
                        gameObject.Transform.SetTranslation(
                        playerTransform.Translation + playerTransform.World.Forward * 5
                        );

                        gameObject.Transform.SetTranslation(
                            gameObject.Transform.Translation.X,
                            AppData.OLD_FIRST_PERSON_DEFAULT_CAMERA_POSITION.Y - 3.2f,
                            gameObject.Transform.Translation.Z
                            );
                    }

                    Application.SceneManager.ActiveScene.Add(gameObject);
                }

                #endregion Check For Radio

                Remove(item.itemData);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Item does not exist in inventory");
            }
        }

        public void PrintInventory()
        {
            foreach (InventoryItem item in inventory)
                System.Diagnostics.Debug.WriteLine("Item name: " + item.itemData.name);
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
        }
    }
}