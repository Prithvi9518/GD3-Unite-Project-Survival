using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Text;

namespace GD.Engine
{
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
            float distance = Vector3.Distance(target.Transform.translation, transform.translation);
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

                if (targetDist <= 6.5f)
                {
                    object[] parameters = { gameObject };
                    EventDispatcher.Raise(
                        new EventData(EventCategoryType.Pickup, EventActionType.OnPickup, parameters)
                        );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Too far");
                }
            }
        }
    }
}