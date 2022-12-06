using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Text;

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
                    if (this.gameObject.GameObjectType == GameObjectType.Collectible)
                    {
                        RaiseCollectibleEvents();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Too far");
                }
            }
        }

        private void RaiseCollectibleEvents()
        {
            // Play pickup sound effect
            object[] parameters = { "pickup-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

            // Raise event to remove object from scene
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