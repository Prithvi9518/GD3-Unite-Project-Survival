using GD.App;

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
                if (parentGameObject.Name == AppData.FIRST_PERSON_CAMERA_NAME)
                    System.Diagnostics.Debug.WriteLine("InteractionController not found.");
                return;
            }

            bool isInteracting = controller.IsInteracting;

            if (isInteracting)
                HandleInteraction();

            //base.HandleResponse(parentGameObject);
        }

        /// <summary>
        /// Override this method to program interaction behaviour for specific interactibles
        /// Ex: the gate access machine and the fusebox will have different colliders that inherit from this class,
        /// and override this method to send different events.
        /// </summary>
        protected virtual void HandleInteraction()
        {
        }
    }
}