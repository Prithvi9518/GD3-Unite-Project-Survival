using GD.Engine.Events;
using GD.Engine;
using Microsoft.Xna.Framework;

namespace GD.App
{
    public class NoteUIController : Component
    {
        private bool isVisible = false;
        private TextureMaterial2D textureMaterial2D;

        public NoteUIController()
        {
            isVisible = false;

            EventDispatcher.Subscribe(EventCategoryType.UI, HandleUIEvent);
        }

        private void HandleUIEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnToggleNote:
                    isVisible = (bool)eventData.Parameters[0];
                    break;

                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            textureMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextureMaterial2D;
            if (!isVisible)
                textureMaterial2D.SourceRectangleWidth = 0;
            else
                textureMaterial2D.SourceRectangleWidth = textureMaterial2D.OriginalSourceRectangle.Width;
        }
    }
}