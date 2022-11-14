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
        private FirstPersonController controller;

        public InteractableBehaviour()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (target == null || target != Application.CameraManager.ActiveCamera.gameObject)
            {
                target = Application.CameraManager.ActiveCamera.gameObject;
            }
            controller = target.GetComponent<FirstPersonController>();

            //System.Diagnostics.Debug.WriteLine($"Target: {target.Name}");

            float targetDist = GetDistance();
            System.Diagnostics.Debug.WriteLine($"Distance: {targetDist}");

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

            float targetDist = GetDistance();

            if (isInteracting && targetDist < 3f)
            {
                System.Diagnostics.Debug.WriteLine("Picked Up");
            }
        }
    }
}