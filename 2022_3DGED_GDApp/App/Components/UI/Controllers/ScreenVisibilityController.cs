using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System.Collections;

namespace GD.App
{
    public class ScreenVisibilityController : Component
    {
        private TextureMaterial2D textureMaterial2D;
        private bool isShown = false;

        public ScreenVisibilityController()
        {
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleUIEvent);
            //EventDispatcher.Subscribe(EventCategoryType.Menu, HandleMenuEvent);
        }

        private void HandleUIEvent(EventData eventData)
        {
            string targetGameObjectName = eventData.Parameters[0] as string;
            if (!gameObject.Name.Equals(targetGameObjectName)) return;

            switch (eventData.EventActionType)
            {
                case EventActionType.OnShow:
                    isShown = true;
                    //textureMaterial2D.SourceRectangleWidth = textureMaterial2D.OriginalSourceRectangle.Width;
                    break;

                case EventActionType.OnHide:
                    isShown = false;
                    //textureMaterial2D.SourceRectangleWidth = 0;
                    break;

                default:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (textureMaterial2D == null)
                textureMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextureMaterial2D;

            if (isShown)
            {
                textureMaterial2D.SourceRectangleWidth = textureMaterial2D.OriginalSourceRectangle.Width;
            }
            else
            {
                textureMaterial2D.SourceRectangleWidth = 0;
            }
        }
    }
}