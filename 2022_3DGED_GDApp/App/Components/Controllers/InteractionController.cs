//#define ALPHA_DEMO
//#define TEST_INVENTORY
//#define DEMO_STATES

using GD.App;
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

        private bool radioPlaced = false;

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

            if (radioPlaced)
                UseRadio();
            else
            {
                PlaceRadio();
            }

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

                    if (gameObject.Name == AppData.TOY_RADIO_NAME)
                        radioPlaced = false;

                    Application.SceneManager.ActiveScene.Remove(
                        gameObject.ObjectType,
                        gameObject.RenderType,
                        (obj) => gameObject.Name.Equals(obj.Name)
                        );

                    GameObject itemFound = Application.SceneManager.ActiveScene.Find(
                        gameObject.ObjectType,
                        gameObject.RenderType,
                        (obj) => gameObject.Name.Equals(obj.Name)
                        );
                    if (itemFound == null)
                    {
                        object[] parameters = { AppData.INTERACT_PROMPT_NAME, PromptState.NoPrompt };
                        EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnToggleButtonPrompt, parameters));
                    }

                    break;

                default:
                    break;
            }
        }

        protected virtual void CheckInteracting()
        {
            if (Input.Gamepad.IsConnected())
            {
                if (Input.Gamepad.WasJustPressed(Buttons.Y))
                    isInteracting = true;
                else
                    isInteracting = false;
            }
            else
            {
                if (Input.Keys.WasJustPressed(Keys.E))
                    isInteracting = true;
                else
                    isInteracting = false;
            }
        }

        private void PlaceRadio()
        {
            if (Input.Keys.WasJustPressed(Keys.R))
            {
                InventoryItem radio = Application.InventoryManager.FindByName(AppData.TOY_RADIO_NAME);
                if (radio != null)
                {
                    object[] parameters = { AppData.TOY_RADIO_NAME };
                    EventDispatcher.Raise(new EventData(EventCategoryType.Inventory, EventActionType.OnObjectPicked, parameters));

                    radioPlaced = true;
                }
            }
        }

        private void UseRadio()
        {
            if (Input.Keys.WasJustPressed(Keys.R) && radioPlaced)
            {
                object[] parameters = { AppData.GLASS_SHATTER_SOUND_NAME };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));
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

        private void StopGeneratorAlarm()
        {
            if (Input.Gamepad.IsConnected() && Input.Gamepad.WasJustPressed(Buttons.LeftShoulder))
            {
                StopAlarmEvent();
            }
            else if (Input.Keys.WasJustPressed(Keys.P))
            {
                StopAlarmEvent();
            }
        }

        private void StopAlarmEvent()
        {
            object[] parameters = { "alarm-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnStop, parameters));
        }
    }

    #endregion Alpha Demo Code
}