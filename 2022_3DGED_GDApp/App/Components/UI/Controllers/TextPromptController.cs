using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;

namespace GD.App
{
    public class TextPromptController : Component
    {
        private bool isActive = false;

        private Vector3 position;
        private string displayText;

        private TextMaterial2D textMaterial;

        public string DisplayText
        {
            get => this.displayText;
            set => this.displayText = value;
        }

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
                        isActive = (bool)eventData.Parameters[1];

                        displayText = eventData.Parameters[2] as string;

                        //position = (Vector3)eventData.Parameters[3];
                        //System.Diagnostics.Debug.WriteLine($"position : {position}");
                    }

                    break;

                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            textMaterial = gameObject.GetComponent<Renderer2D>().Material as TextMaterial2D;

            if (isActive)
            {
                textMaterial.StringBuilder.Clear();
                textMaterial.StringBuilder.Append(displayText);
            }
            else
            {
                textMaterial.StringBuilder.Clear();
            }
        }
    }
}