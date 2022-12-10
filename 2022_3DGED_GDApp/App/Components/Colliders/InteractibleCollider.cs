using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.Engine
{
    public class InteractibleCollider : Collider
    {
        private GameObject playerGameObject;

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

            playerGameObject = controller.gameObject;
            RaiseCollisionResponseEvents();

            bool isInteracting = controller.IsInteracting;

            if (isInteracting)
                HandleInteraction();

            //base.HandleResponse(parentGameObject);
        }

        protected virtual void RaiseCollisionResponseEvents()
        {
            RaiseButtonPromptUIEvent(true);
        }

        protected virtual void RaiseButtonPromptUIEvent(bool isActive)
        {
            string text = "";
            if (isActive)
                text = (Input.Gamepad.IsConnected()) ? "Press Y to interact" : "Press E to interact";

            object[] parameters = { AppData.INTERACT_PROMPT_NAME, isActive, text, new Vector3(0, 0, 1) };

            EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnToggleButtonPrompt, parameters));
        }

        /// <summary>
        /// Override this method to program interaction behaviour for specific interactibles
        /// Ex: the gate access machine and the fusebox will have different colliders that inherit from this class,
        /// and override this method to send different events.
        /// </summary>
        protected virtual void HandleInteraction()
        {
        }

        public override void Update(GameTime gameTime)
        {
            // Hacky way to get the button prompts to disappear upon stepping away from the collider

            if (playerGameObject != null)
            {
                if (Vector3.Distance(playerGameObject.Transform.Translation, gameObject.Transform.Translation) > AppData.INTERACTION_DISTANCE)
                {
                    RaiseButtonPromptUIEvent(false);
                }
            }

            base.Update(gameTime);
        }
    }
}