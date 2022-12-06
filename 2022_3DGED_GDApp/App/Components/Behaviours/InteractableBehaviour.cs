using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using System;

namespace GD.Engine
{
    /// <summary>
    /// Component that models the behaviour of objects that can be interacted with, or picked up by the player.
    ///
    /// For the moment, the component takes in a target GameObject(the player), and checks its distance from the target.
    /// If the player is within a certain range, it checks if they are pressing the key/button to interact
    /// through the InteractionController component. If so, it raises an event.
    /// </summary>
    public class InteractableBehaviour : Component
    {
        private GameObject target;
        private InteractionController controller;

        public InteractableBehaviour()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (target == null || target != Application.CameraManager.ActiveCamera.gameObject)
            {
                target = Application.CameraManager.ActiveCamera.gameObject;
            }
            controller = target.GetComponent<InteractionController>();

            CheckTrigger();

            //base.Update(gameTime);
        }

        public float GetDistance()
        {
            float distance = Vector3.Distance(target.Transform.Translation, transform.Translation);
            return distance;
        }

        public void CheckTrigger()
        {
            if (controller == null)
            {
                System.Diagnostics.Debug.WriteLine("Controller not found");
                return;
            }
            bool isInteracting = controller.IsInteracting;

            if (isInteracting)
            {
                float targetDist = GetDistance();

                if (targetDist <= AppData.INTERACTION_DISTANCE)
                {
                    HandleInteraction();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Too far");
                }
            }
        }

        private void HandleInteraction()
        {
            switch (this.gameObject.GameObjectType)
            {
                case GameObjectType.Collectible:
                    RaiseCollectibleEvents();
                    break;

                case GameObjectType.Interactible:
                    RaiseInteractibleEvents();
                    break;

                default:
                    break;
            }
        }

        private void RaiseInteractibleEvents()
        {
            // NEED TO REFACTOR THIS CODE - once colliders are in place, use separate collider classes for each object

            // Raise event based on what object it is.
            switch (this.gameObject.Name)
            {
                case "gate access machine":

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
                        // otherwise, send message/hint to user that they need some sort of keycard
                        System.Diagnostics.Debug.WriteLine("You need a keycard");
                    }

                    break;

                case "fuse box":
                    // Check whether fuse is present in inventory
                    InventoryItem fuse = Application.InventoryManager.FindByName(AppData.FUSE_NAME);
                    if (fuse != null)
                    {
                        // Check whether generator room has been opened with keycard first
                        if (Application.StateManager.CurrentGameState != GameState.GeneratorRoomOpen)
                            return;

                        // Send event to remove fuse
                        object[] parameters = { AppData.FUSE_NAME };
                        EventDispatcher.Raise(new EventData(EventCategoryType.Inventory,
                            EventActionType.OnObjectPicked, parameters));

                        // Send event to StateManager - change state to reflect that generator is now on.
                        parameters = new object[] { GameState.GeneratorOn };
                        EventDispatcher.Raise(new EventData(EventCategoryType.GameState,
                            EventActionType.OnChangeState, parameters));
                    }
                    else
                    {
                        // otherwise, send message/hint to user that they need a fuse
                        System.Diagnostics.Debug.WriteLine("You need a fuse");
                    }
                    break;

                default:
                    break;
            }
        }

        private void RaiseCollectibleEvents()
        {
            // Play pickup sound effect
            object[] parameters = { "pickup-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

            // Raise event to take object out of inventory and use it
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