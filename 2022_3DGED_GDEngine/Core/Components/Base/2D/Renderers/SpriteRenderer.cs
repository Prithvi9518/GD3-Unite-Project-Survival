using GD.App;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace GD.Engine
{
    /// <summary>
    /// Orchestrates the drawing/rendering of an object
    /// </summary>
    public class SpriteRenderer : Component
    {
        private SpriteMaterial material;  //textures, alpha
        private UIElement uiElement;      //text, texture
        private StateManager stateManager;

        public SpriteRenderer(SpriteMaterial material,
            UIElement uiElement)
        {
            this.material = material;
            this.uiElement = uiElement;
        }

        public SpriteRenderer(StateManager stateManager, UITextureElement uiElement)
        {
            this.stateManager = stateManager;
            this.uiElement = uiElement;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            uiElement.Draw(spriteBatch, transform, material);
        }
    }
}