using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;

namespace GD.App
{
    public enum PromptState
    {
        NoPrompt,
        InteractPrompt,
        PickupPrompt
    }

    public class TextPromptController : Component
    {
        private PromptState currentPromptState;

        private Vector3 position;

        private TextMaterial2D textMaterial2D;

        public Vector3 Position
        {
            get => this.position;
            set => this.position = value;
        }

        public TextPromptController()
        {
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleUIEvent);
        }

        private void HandleUIEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnToggleButtonPrompt:
                    if (gameObject.Name.Equals(eventData.Parameters[0] as string))
                    {
                        currentPromptState = (PromptState)eventData.Parameters[1];

                        //position = (Vector3)eventData.Parameters[2];
                        //System.Diagnostics.Debug.WriteLine($"position : {position}");
                    }

                    break;

                default:
                    break;
            }
        }

        private string GetPromptText(bool gamepadConnected)
        {
            switch (currentPromptState)
            {
                case PromptState.NoPrompt:
                    return "";

                case PromptState.InteractPrompt:
                    return (gamepadConnected) ? "Press Y to interact" : "Press E to interact";

                case PromptState.PickupPrompt:
                    return (gamepadConnected) ? "Press Y to pick up" : "Press E to pick up";

                default:
                    return "";
            }
        }

        public override void Update(GameTime gameTime)
        {
            textMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextMaterial2D;

            string text = GetPromptText(Input.Gamepad.IsConnected());
            textMaterial2D.StringBuilder.Clear();
            textMaterial2D.StringBuilder.Append(text);
        }
    }
}