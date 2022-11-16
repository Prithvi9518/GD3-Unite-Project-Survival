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
        protected float multiplier = 1f;
        protected Vector2 rotationSpeed;
        private bool isGrounded;
        private bool crouchEnabled = false;
        private bool needsToCrouch = true;

        #endregion Fields

        #region Temps

        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;

        #endregion Temps

        #region Constructors

        public FirstPersonController(float moveSpeed, float strafeSpeed, float rotationSpeed, bool isGrounded = true)
    : this(moveSpeed, strafeSpeed, rotationSpeed * Vector2.One, isGrounded)
        {
        }

        public FirstPersonController(float moveSpeed, float strafeSpeed, Vector2 rotationSpeed, bool isGrounded = true)
        {
            this.moveSpeed = moveSpeed;
            this.strafeSpeed = strafeSpeed;
            this.rotationSpeed = rotationSpeed;
            this.isGrounded = isGrounded;
        }

        #endregion Constructors

        #region Actions - Update, Input

        public override void Update(GameTime gameTime)
        {
            HandleMouseInput(gameTime);
            HandleKeyboardInput(gameTime);
            HandleGamepadInput(gameTime);

            //System.Diagnostics.Debug.WriteLine($"crouchEnabled: {crouchEnabled}");
            //System.Diagnostics.Debug.WriteLine($"needsToCrouch: {needsToCrouch}");
        }

        protected virtual void HandleKeyboardInput(GameTime gameTime)
        {
            float runMultiplier = 2.5f;

            translation = Vector3.Zero;

            if (!crouchEnabled)
            {
                if (Input.Keys.IsPressed(Keys.LeftShift))
                    multiplier = runMultiplier;
                else
                {
                    multiplier = 1f;
                }
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

            float normalHeight = AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION.Y;
            float crouchedHeight = normalHeight - 1f;

            if (crouchEnabled && needsToCrouch)
            {
                transform.translation.Y = crouchedHeight;
                needsToCrouch = false;
            }
            else if (!crouchEnabled)
            {
                transform.translation.Y = normalHeight;
            }

            transform.Translate(translation);
        }

        protected virtual void HandleCrouch()
        {
            float crouchMultiplier = 0.7f;

            if (crouchEnabled)
            {
                needsToCrouch = true;
                multiplier = crouchMultiplier;
            }
            else
            {
                needsToCrouch = false;
                multiplier = 1f;
            }
        }

        protected virtual void HandleMouseInput(GameTime gameTime)
        {
            rotation = Vector3.Zero;
            var delta = Input.Mouse.Delta;

            if (delta.Length() != 0)
            {
                //Q - where are X and Y reversed?
                rotation.Y -= delta.X * rotationSpeed.X * gameTime.ElapsedGameTime.Milliseconds;
                rotation.X -= delta.Y * rotationSpeed.Y * gameTime.ElapsedGameTime.Milliseconds;
                transform.SetRotation(rotation);
            }
        }

        #endregion Actions - Update, Input

        #region Actions - Gamepad

        protected virtual void HandleGamepadInput(GameTime gameTime)
        {
            translation = Vector3.Zero;

            if (Input.Gamepad.IsConnected())
            {
                float runMultiplier = 2.5f;

                if (!crouchEnabled)
                {
                    if (Input.Gamepad.IsPressed(Buttons.LeftStick))
                        multiplier = runMultiplier;
                    else
                    {
                        multiplier = 1f;
                    }
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
            }

            transform.Translate(translation);

            var changeInRotation = Input.Gamepad.GetAxis(Buttons.RightStick);
            rotation.X += changeInRotation.Y * rotationSpeed.Y * 15 * gameTime.ElapsedGameTime.Milliseconds;
            rotation.Y -= changeInRotation.X * rotationSpeed.X * 15 * gameTime.ElapsedGameTime.Milliseconds;

            transform.Rotate(rotation);
        }

        #endregion Actions - Gamepad
    }
}