using GD.App;
using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GD.Engine
{
    /// <summary>
    /// Adds collidable 1st person controller to camera using keyboard and mouse input
    /// </summary>
    public class OurCollidableFPController : OurFirstPersonController
    {
        #region Statics

        private static readonly float DEFAULT_JUMP_HEIGHT = 5;

        #endregion Statics

        #region Fields

        private CharacterCollider characterCollider;
        private Character characterBody;
        private float jumpHeight;

        //temp vars
        private Vector3 restrictedLook, restrictedRight;

        private bool insideOffice = false;

        #endregion Fields

        #region Constructors

        public OurCollidableFPController(GameObject gameObject,
            CharacterCollider characterCollider,
            float moveSpeed,
            float strafeSpeed, Vector2 rotationSpeed,
            float smoothFactor, bool isGrounded,
            float jumpHeight)
        : base(moveSpeed, strafeSpeed, rotationSpeed, smoothFactor, isGrounded)
        {
            this.jumpHeight = jumpHeight;
            //get the collider attached to the game object for this controller
            this.characterCollider = characterCollider;
            //get the body so that we can change its position when keys
            characterBody = characterCollider.Body as Character;
        }

        #endregion Constructors

        #region Actions - Update

        public override void Update(GameTime gameTime)
        {
            if (characterBody == null)
                throw new NullReferenceException("No body to move with this controller. You need to add the collider component before this controller!");

            if (Input.Gamepad.IsConnected())
            {
                HandleGamepadInput(gameTime);
            }
            else
            {
                HandleKeyboardInput(gameTime);
                HandleMouseInput(gameTime);
                HandleStrafe(gameTime, false);
            }

            CheckInsideOffice();

            //HandleJump(gameTime);
        }

        #endregion Actions - Update

        #region Actions - Input

        protected override void HandleKeyboardInput(GameTime gameTime)
        {
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
            {
                restrictedLook = transform.World.Forward; //we use Up instead of Forward
                restrictedLook.Y = 0;
                characterBody.Velocity += (moveSpeed * multiplier) * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (Input.Keys.IsPressed(Keys.S) || Input.Keys.IsPressed(Keys.Down))
            {
                restrictedLook = transform.World.Forward;
                restrictedLook.Y = 0;
                characterBody.Velocity -= (moveSpeed * multiplier) * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        protected override void HandleGamepadInput(GameTime gameTime)
        {
            if (!crouchEnabled)
            {
                HandleRun(true);
            }

            if (Input.Gamepad.WasJustPressed(Buttons.X))
            {
                crouchEnabled = !crouchEnabled;
                HandleCrouch();
            }

            #region Front and Back Movement

            if (Input.Gamepad.GetAxis(Buttons.LeftStick).Y > 0.5f)
            {
                restrictedLook = transform.World.Forward; //we use Up instead of Forward
                restrictedLook.Y = 0;
                characterBody.Velocity += (moveSpeed * multiplier) * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (Input.Gamepad.GetAxis(Buttons.LeftStick).Y < -0.5f)
            {
                restrictedLook = transform.World.Forward;
                restrictedLook.Y = 0;
                characterBody.Velocity -= (moveSpeed * multiplier) * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }

            #endregion Front and Back Movement

            #region Strafe Movement

            if (Input.Gamepad.GetAxis(Buttons.LeftStick).X < -0.5f)
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity -= (strafeSpeed * multiplier) * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (Input.Gamepad.GetAxis(Buttons.LeftStick).X > 0.5f)
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity += (strafeSpeed * multiplier) * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }

            #endregion Strafe Movement

            var changeInRotation = Input.Gamepad.GetAxis(Buttons.RightStick);

            rotation.X += changeInRotation.Y * rotationSpeed.Y *
                AppData.PLAYER_ROTATE_GAMEPAD_MULTIPLIER * gameTime.ElapsedGameTime.Milliseconds;

            ClampRotationX();

            rotation.Y -= changeInRotation.X * rotationSpeed.X *
                AppData.PLAYER_ROTATE_GAMEPAD_MULTIPLIER * gameTime.ElapsedGameTime.Milliseconds;

            //characterBody.AddBodyTorque(new Vector3(rotation.Y, rotation.X, 0));
            transform.Rotate(rotation);
        }

        private void HandleStrafe(GameTime gameTime, bool usingGamepad)
        {
            if (Input.Keys.IsPressed(Keys.A) || Input.Keys.IsPressed(Keys.Left))
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity -= (strafeSpeed * multiplier) * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (Input.Keys.IsPressed(Keys.D) || Input.Keys.IsPressed(Keys.Right))
            {
                restrictedRight = transform.World.Right;
                restrictedRight.Y = 0;
                characterBody.Velocity += (strafeSpeed * multiplier) * restrictedRight * gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                characterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        #region Crouch/Uncrouch

        protected override void HandleCrouch()
        {
            if (crouchEnabled)
            {
                needsToCrouch = true;
                multiplier = AppData.PLAYER_CROUCH_MULTIPLIER;
            }
            else
            {
                needsToCrouch = false;
                multiplier = AppData.PLAYER_DEFAULT_MULTIPLIER;
            }

            if (crouchEnabled && needsToCrouch)
            {
                //transform.SetTranslation(new Vector3(transform.Translation.X, crouchedHeight, transform.Translation.Z));
                SetCrouchedCollider();
                needsToCrouch = false;
            }
            else if (!crouchEnabled)
            {
                SetDefaultCollider();
                //transform.SetTranslation(new Vector3(transform.Translation.X, normalHeight, transform.Translation.Z));
            }
        }

        private void SetCrouchedCollider()
        {
            var crouchedCollider = new CharacterCollider(gameObject, true);

            gameObject.RemoveComponent<CharacterCollider>();
            gameObject.AddComponent(crouchedCollider);

            crouchedCollider.AddPrimitive(new Capsule(
                gameObject.Transform.Translation,
                Matrix.CreateRotationX(MathHelper.PiOver2),
                1, AppData.PLAYER_CROUCHED_CAPSULE_HEIGHT),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            crouchedCollider.Enable(gameObject, false, 1);

            characterCollider = crouchedCollider;
            characterBody = crouchedCollider.Body as Character;
        }

        private void SetDefaultCollider()
        {
            var defaultCollider = new CharacterCollider(gameObject, true);

            gameObject.RemoveComponent<CharacterCollider>();
            gameObject.AddComponent(defaultCollider);

            defaultCollider.AddPrimitive(new Capsule(
                gameObject.Transform.Translation,
                Matrix.CreateRotationX(MathHelper.PiOver2),
                1, AppData.PLAYER_DEFAULT_CAPSULE_HEIGHT),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            defaultCollider.Enable(gameObject, false, 1);

            characterCollider = defaultCollider;
            characterBody = defaultCollider.Body as Character;

            transform.Translate(0, AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION.Y, 0);
        }

        #endregion Crouch/Uncrouch

        private void HandleJump(GameTime gameTime)
        {
            if (Input.Keys.IsPressed(Keys.Space))
                characterBody.DoJump(jumpHeight);
        }

        private void CheckInsideOffice()
        {
            if (transform.Translation.X <= -34 && !insideOffice)
            {
                // Raise event to display subtitles in office
                object[] parameters = { DialogueState.NeedKeycardInOffice };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
                insideOffice = true;
            }
        }

        #endregion Actions - Input
    }
}