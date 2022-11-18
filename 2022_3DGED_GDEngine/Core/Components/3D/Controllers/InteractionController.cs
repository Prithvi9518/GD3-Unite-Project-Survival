using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GD.Engine
{
    public class InteractionController : Component
    {
        private bool isInteracting = false;

        public InteractionController()
        {
        }

        public bool IsInteracting
        {
            get => isInteracting;
        }

        public override void Update(GameTime gameTime)
        {
            CheckInteracting();
            //base.Update(gameTime);
        }

        protected virtual void CheckInteracting()
        {
            if (Input.Gamepad.IsConnected())
            {
                if (Input.Gamepad.IsPressed(Buttons.Y))
                    isInteracting = true;
                else
                    isInteracting = false;
            }
            else
            {
                if (Input.Keys.IsPressed(Keys.E))
                    isInteracting = true;
                else
                    isInteracting = false;
            }
        }
    }
}