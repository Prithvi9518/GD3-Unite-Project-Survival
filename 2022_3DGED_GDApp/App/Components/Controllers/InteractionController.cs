//#define ALPHA_DEMO
#define TEST_INVENTORY

using GD.Engine.Events;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;

namespace GD.Engine
{
    /// <summary>
    /// Handles the player's interaction with items using a key/button press.
    ///
    /// Uses a boolean to keep track of whether the player is currently attempting to interact with an object.
    ///
    /// For the moment, the class listens to events in the Pickup category,
    /// and removes the object that was picked up from the scene.
    /// </summary>
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

#if ALPHA_DEMO
            MakeOfficeEnemyMove();
            ActivateGeneratorAlarm();
#endif

#if TEST_INVENTORY
            UseInventoryItems();
#endif
            //base.Update(gameTime);
        }

        /// <summary>
        /// In case of a successful pickup, the method removes the picked-up object from the scene
        /// </summary>
        /// <param name="eventData"></param>
        private void HandlePickup(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnPickup:

                    GameObject gameObject = eventData.Parameters[0] as GameObject;

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

        private void UseInventoryItems()
        {
            if (Input.Keys.IsPressed(Keys.F))
            {
                object[] parameters = { "fuse" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Inventory, EventActionType.OnObjectPicked, parameters));
            }
        }

        #region Alpha Demo Code

        private void MakeOfficeEnemyMove()
        {
            if (Input.Gamepad.IsConnected() && Input.Gamepad.WasJustPressed(Buttons.RightTrigger))
            {
                RaiseEnemyMoveEvents();
            }
            else if (Input.Keys.WasJustPressed(Keys.R))
            {
                RaiseEnemyMoveEvents();
            }
        }

        private void RaiseEnemyMoveEvents()
        {
            // Event to play glass breaking sound

            object[] parameters = { "glass-shatter" };

            EventDispatcher.Raise(
                new EventData(EventCategoryType.Sound,
                EventActionType.OnPlay2D,
                parameters)
                );

            // Event to make enemy move
            parameters = new object[] { "true" };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.NonPlayer, EventActionType.OnEnemyAlert, parameters)
                );
        }

        private void ActivateGeneratorAlarm()
        {
            if (Input.Gamepad.IsConnected() && Input.Gamepad.WasJustPressed(Buttons.LeftTrigger))
            {
                RaiseAlarmEvent();
            }
            else if (Input.Keys.WasJustPressed(Keys.G))
            {
                RaiseAlarmEvent();
            }

            if (Input.Gamepad.IsConnected() && Input.Gamepad.WasJustPressed(Buttons.LeftShoulder))
            {
                StopAlarm();
            }
            else if (Input.Keys.WasJustPressed(Keys.P))
            {
                StopAlarm();
            }
        }

        private void RaiseAlarmEvent()
        {
            object[] parameters = { "alarm-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));
        }

        private void StopAlarm()
        {
            object[] parameters = { "alarm-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnStop, parameters));
        }
    }

    #endregion Alpha Demo Code
}