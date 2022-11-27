using GD.App;
using GD.Engine.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GD.Engine
{
    /// <summary>
    /// Adds simple non-collidable 1st person controller to camera using keyboard and mouse input
    /// </summary>
    public class FirstPersonController : Component
    {
        #region Fields

        protected float moveSpeed = 0.05f;
        protected float strafeSpeed = 0.025f;

        protected float multiplier = AppData.PLAYER_DEFAULT_MULTIPLIER;
        protected float runMultiplier = AppData.PLAYER_RUN_MULTIPLIER;
        protected float crouchMultiplier = AppData.PLAYER_CROUCH_MULTIPLIER;

        protected Vector2 rotationSpeed;

        private bool isGrounded;

        private bool crouchEnabled = false;
        private bool needsToCrouch = true;

        private float smoothFactor;

        #endregion Fields

        #region Temps

        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;
        private Vector2 oldDelta;

        #endregion Temps

        #region Constructors

        public FirstPersonController(float moveSpeed, float strafeSpeed, float rotationSpeed, float smoothFactor = 0.25f, bool isGrounded = true)
    : this(moveSpeed, strafeSpeed, rotationSpeed * Vector2.One, smoothFactor, isGrounded)
        {
        }

        public FirstPersonController(float moveSpeed, float strafeSpeed, Vector2 rotationSpeed, float smoothFactor, bool isGrounded)
        {
            this.moveSpeed = moveSpeed;
            this.strafeSpeed = strafeSpeed;
            this.rotationSpeed = rotationSpeed;
            this.smoothFactor = smoothFactor;
            this.isGrounded = isGrounded;
        }

        #endregion Constructors

        #region Actions - Update, Input

        public override void Update(GameTime gameTime)
        {
            if (Input.Gamepad.IsConnected())
            {
                HandleGamepadInput(gameTime);
            }
            else
            {
                HandleMouseInput(gameTime);
                HandleKeyboardInput(gameTime);
            }
        }

        protected virtual void HandleKeyboardInput(GameTime gameTime)
        {
            translation = Vector3.Zero;

            if (!crouchEnabled)
            {
                HandleRun(false);
            }

            if (Input.Keys.WasJustPressed(Keys.LeftControl))
            {
                crouchEnabled = !crouchEnabled;
                HandleCrouch();
            }

            if (Input.Keys.IsPressed(Keys.W) || Input.Keys.IsPressed(Keys.Up))
                translation += transform.World.Forward * (moveSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;
            else if (Input.Keys.IsPressed(Keys.S) || Input.Keys.IsPressed(Keys.Down))
                translation -= transform.World.Forward * (moveSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Keys.IsPressed(Keys.A) || Input.Keys.IsPressed(Keys.Left))
                translation += transform.World.Left * (strafeSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;
            else if (Input.Keys.IsPressed(Keys.D) || Input.Keys.IsPressed(Keys.Right))
                translation += transform.World.Right * (strafeSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;

            if (isGrounded)
                translation.Y = 0;

            transform.Translate(translation);
        }

        /// <summary>
        /// Increases the player's speed when the run key is pressed
        /// For the moment, the run key/button is E on keyboard, and Left Stick on gamepad
        /// </summary>
        /// <param name="usingGamepad"></param>
        protected virtual void HandleRun(bool usingGamepad)
        {
            if (usingGamepad)
            {
                if (Input.Gamepad.IsPressed(Buttons.LeftStick))
                    multiplier = runMultiplier;
                else
                {
                    multiplier = AppData.PLAYER_DEFAULT_MULTIPLIER;
                }
            }
            else
            {
                if (Input.Keys.IsPressed(Keys.LeftShift))
                    multiplier = runMultiplier;
                else
                {
                    multiplier = AppData.PLAYER_DEFAULT_MULTIPLIER;
                }
            }
        }

        /// <summary>
        /// Responsible for toggling the crouched/uncrouched state of player
        /// Decreases player speed when crouched, and brings down translation on Y-axis
        /// </summary>
        protected virtual void HandleCrouch()
        {
            if (crouchEnabled)
            {
                needsToCrouch = true;
                multiplier = crouchMultiplier;
            }
            else
            {
                needsToCrouch = false;
                multiplier = AppData.PLAYER_DEFAULT_MULTIPLIER;
            }

            float normalHeight = AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION.Y;
            float crouchedHeight = normalHeight - AppData.PLAYER_CROUCH_HEIGHT_OFFSET;

            if (crouchEnabled && needsToCrouch)
            {
                transform.SetTranslation(new Vector3(transform.Translation.X, crouchedHeight, transform.Translation.Z));
                needsToCrouch = false;
            }
            else if (!crouchEnabled)
            {
                transform.SetTranslation(new Vector3(transform.Translation.X, normalHeight, transform.Translation.Z));
            }
        }

        /// <summary>
        /// Clamps the vertical rotation of the first-person camera
        /// </summary>
        protected virtual void ClampRotationX()
        {
            if (rotation.X < AppData.PLAYER_ROTATE_MIN_X)
                rotation.X = AppData.PLAYER_ROTATE_MIN_X;
            else if (rotation.X > AppData.PLAYER_ROTATE_MAX_X)
                rotation.X = AppData.PLAYER_ROTATE_MAX_X;
        }

        protected virtual void HandleMouseInput(GameTime gameTime)
        {
            rotation = Vector3.Zero;
            var currentDelta = Input.Mouse.Delta;

            //smooth camera movement
            var newDelta = oldDelta.Lerp(currentDelta, smoothFactor);

            //did we move mouse?
            if (newDelta.Length() != 0)
            {
                //Q - where are X and Y reversed?
                rotation.Y -= newDelta.X * rotationSpeed.X * gameTime.ElapsedGameTime.Milliseconds;
                rotation.X -= newDelta.Y * rotationSpeed.Y * gameTime.ElapsedGameTime.Milliseconds;

                ClampRotationX();

                transform.SetRotation(rotation);
            }
            //store current to be used for next update of smoothing
            oldDelta = newDelta;
        }

        #endregion Actions - Update, Input

        #region Actions - Gamepad

        protected virtual void HandleGamepadInput(GameTime gameTime)
        {
            translation = Vector3.Zero;
            rotation = Vector3.Zero;

            if (!crouchEnabled)
            {
                HandleRun(true);
            }

            if (Input.Gamepad.WasJustPressed(Buttons.X))
            {
                crouchEnabled = !crouchEnabled;
                HandleCrouch();
            }

            if (Input.Gamepad.GetAxis(Buttons.LeftStick).Y > 0.5f)
                translation += transform.World.Forward * (moveSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;
            else if (Input.Gamepad.GetAxis(Buttons.LeftStick).Y < -0.5f)
                translation -= transform.World.Forward * (moveSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Gamepad.GetAxis(Buttons.LeftStick).X < -0.5f)
                translation += transform.World.Left * (strafeSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;
            else if (Input.Gamepad.GetAxis(Buttons.LeftStick).X > 0.5f)
                translation += transform.World.Right * (strafeSpeed * multiplier) * gameTime.ElapsedGameTime.Milliseconds;

            if (isGrounded)
                translation.Y = 0;

            transform.Translate(translation);

            var changeInRotation = Input.Gamepad.GetAxis(Buttons.RightStick);

            rotation.X += changeInRotation.Y * rotationSpeed.Y *
                AppData.PLAYER_ROTATE_GAMEPAD_MULTIPLIER * gameTime.ElapsedGameTime.Milliseconds;

            ClampRotationX();

            rotation.Y -= changeInRotation.X * rotationSpeed.X *
                AppData.PLAYER_ROTATE_GAMEPAD_MULTIPLIER * gameTime.ElapsedGameTime.Milliseconds;

            transform.Rotate(rotation);
        }

        #endregion Actions - Gamepad
    }
}