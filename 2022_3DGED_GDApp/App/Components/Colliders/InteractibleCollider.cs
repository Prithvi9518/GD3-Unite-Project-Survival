using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD.Engine
{
    public class InteractibleCollider : Collider
    {
        public InteractibleCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleResponse(GameObject parentGameObject)
        {
            InteractionController controller = parentGameObject.GetComponent<InteractionController>();

            if (controller == null)
            {
                System.Diagnostics.Debug.WriteLine("InteractionController not found.");
                return;
            }

            bool isInteracting = controller.IsInteracting;

            if (isInteracting)
                HandleInteraction();

            //base.HandleResponse(parentGameObject);
        }

        /// <summary>
        /// Override this method to program interaction behaviour for specific interactibles/pickups
        /// Ex: a keycard and a fuse will have different colliders that inherit from this class,
        /// and override this method to send different events.
        /// </summary>
        protected virtual void HandleInteraction()
        {
        }
    }
}