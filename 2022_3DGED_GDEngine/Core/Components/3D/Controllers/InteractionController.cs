using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GD.Engine
{
    public class InteractionController : Component
    {
        private bool isInteracting = false;

        public InteractionController()
        {
            EventDispatcher.Subscribe(EventCategoryType.Pickup, HandlePickup);
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

        private void HandlePickup(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnPickup:

                    GameObject gameObject = eventData.Parameters[0] as GameObject;
                    System.Diagnostics.Debug.WriteLine("Picked Up " + gameObject.Name);

                    Application.SceneManager.ActiveScene.Remove(
                        gameObject.ObjectType,
                        gameObject.RenderType,
                        (obj) => gameObject.Name.Equals(obj.Name)
                        );

                    break;

                default:
                    break;
            }
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