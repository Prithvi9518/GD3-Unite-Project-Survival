using GD.App;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.Engine
{
    public class InteractibleCollider : Collider
    {
        protected GameObject playerGameObject;

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

            bool isInteracting = controller.IsInteracting;

            if (isInteracting)
                HandleInteraction();

            //base.HandleResponse(parentGameObject);
        }

        protected virtual void CheckButtonPromptState()
        {
            float distance = Vector3.Distance(playerGameObject.Transform.Translation, gameObject.Transform.Translation);
            if (distance > AppData.INTERACTION_DISTANCE)
            {
                RaiseButtonPromptUIEvent(PromptState.NoPrompt);
            }
            else
            {
                RaiseButtonPromptUIEvent(PromptState.InteractPrompt);
            }
        }

        protected virtual void RaiseButtonPromptUIEvent(PromptState promptState)
        {
            object[] parameters = { AppData.INTERACT_PROMPT_NAME, promptState };

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
                CheckButtonPromptState();
            }

            base.Update(gameTime);
        }
    }
}