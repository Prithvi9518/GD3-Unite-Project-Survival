#region Pre-compiler directives

//#define DEMO
#define HI_RES

#endregion

using GD.Engine;
using GD.Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace GD.App
{
#if DEMO

    public enum CameraIDType : sbyte
    {
        First,
        Third,
        Security
    }

#endif

    public class AppData
    {
        #region Graphics

#if HI_RES
        public static readonly Vector2 APP_RESOLUTION = Resolutions.SixteenNine.HD;
#else
        public static readonly Vector2 APP_RESOLUTION = Resolutions.FourThree.VGA;
#endif

        #endregion Graphics

        #region World Scale

        public static readonly float SKYBOX_WORLD_SCALE = 2000;

        #endregion World Scale

        #region Camera - General

        public static readonly float CAMERA_FOV_INCREMENT_LOW = 1;
        public static readonly float CAMERA_FOV_INCREMENT_MEDIUM = 2;
        public static readonly float CAMERA_FOV_INCREMENT_HIGH = 4;

        #endregion

        #region Camera - First Person

        public static readonly string FIRST_PERSON_CAMERA_NAME = "fpc 1";
        public static readonly float FIRST_PERSON_MOVE_SPEED = 0.009f;
        public static readonly float FIRST_PERSON_STRAFE_SPEED = 0.6f * FIRST_PERSON_MOVE_SPEED;
        public static readonly Vector3 FIRST_PERSON_DEFAULT_CAMERA_POSITION = new Vector3(-10, 3.5f, 35);

        public static readonly float FIRST_PERSON_CAMERA_FCP = 3000;
        public static readonly float FIRST_PERSON_CAMERA_NCP = 0.1f;

        public static readonly float FIRST_PERSON_HALF_FOV
             = MathHelper.ToRadians(75);

        public static readonly float FIRST_PERSON_CAMERA_SMOOTH_FACTOR = 1f;

        #endregion Camera - First Person

        #region Camera - Third Person

        public static readonly string THIRD_PERSON_CAMERA_NAME = "third person camera";

        #endregion

        #region Camera - Security Camera

        public static readonly float SECURITY_CAMERA_MAX_ANGLE = 45;
        public static readonly float SECURITY_CAMERA_ANGULAR_SPEED_MUL = 50;
        public static readonly Vector3 SECURITY_CAMERA_ROTATION_AXIS = new Vector3(0, 1, 0);
        public static readonly string SECURITY_CAMERA_NAME = "security camera 1";

        #endregion Camera - Security Camera

        #region Camera - Curve

        public static readonly string CURVE_CAMERA_NAME = "curve camera 1";

        #endregion

        #region Input Key Mappings

        public static readonly Keys[] KEYS_ONE = { Keys.W, Keys.S, Keys.A, Keys.D };
        public static readonly Keys[] KEYS_TWO = { Keys.U, Keys.J, Keys.H, Keys.K };

        #endregion Input Key Mappings

        #region Game Variables

        public static readonly string GAME_TITLE = "Project Survival";
        public static readonly string SCENE_NAME = "shopping centre";

        public static readonly float DEFAULT_OBJECT_SCALE = 0.02f;

        #endregion Game Variables

        #region Movement Constants

        public static readonly float PLAYER_MOVE_SPEED = 0.1f;
        private static readonly float PLAYER_STRAFE_SPEED_MULTIPLIER = 0.75f;
        public static readonly float PLAYER_STRAFE_SPEED = PLAYER_STRAFE_SPEED_MULTIPLIER * PLAYER_MOVE_SPEED;

        //can use either same X-Y rotation for camera controller or different
        public static readonly float PLAYER_ROTATE_SPEED_SINGLE = 0.001f;

        //why bother? can you tilt your head at the same speed as you rotate it?
        public static readonly Vector2 PLAYER_ROTATE_SPEED_VECTOR2 = new Vector2(0.0004f, 0.0003f);

        public static readonly float PLAYER_ROTATE_MAX_X = 0.65f;
        public static readonly float PLAYER_ROTATE_MIN_X = -0.75f;

        public static readonly float PLAYER_DEFAULT_MULTIPLIER = 1f;
        public static readonly float PLAYER_RUN_MULTIPLIER = 2.5f;
        public static readonly float PLAYER_CROUCH_MULTIPLIER = 0.7f;

        public static readonly float PLAYER_CROUCH_HEIGHT_OFFSET = 1.75f;

        public static readonly float PLAYER_ROTATE_GAMEPAD_MULTIPLIER = 15f;

        #endregion Movement Constants

        #region Enemy Variables

        public static readonly float ENEMY_POSITION_Y = 2f;
        public static readonly float ENEMY_SCALE = 0.007f;

        public static readonly float ENEMY_MOVEMENT_SPEED = 0.005f;

        // ENEMY 1 - Middle Lanes Enemy
        // ENEMY 2 - Office Guarding Enemy
        // ENEMY 3 - Right Lane Enemy

        public static readonly Vector3 ENEMY_1_INITIAL_POS = new Vector3(-16, AppData.ENEMY_POSITION_Y, -120);
        public static readonly Vector3 ENEMY_2_INITIAL_POS = new Vector3(-39, AppData.ENEMY_POSITION_Y, -109);
        public static readonly Vector3 ENEMY_3_INITIAL_POS = new Vector3(30, AppData.ENEMY_POSITION_Y, -108);

        public static readonly List<Vector3> ENEMY_INITIAL_POSITIONS = new List<Vector3>()
        {
            ENEMY_1_INITIAL_POS,
            ENEMY_2_INITIAL_POS,
            ENEMY_3_INITIAL_POS
        };

        public static readonly List<Vector3> ENEMY_INITIAL_ROTATIONS = new List<Vector3>()
        {
            new Vector3(0, MathHelper.PiOver2, 0),
            new Vector3(0, MathHelper.Pi, 0),
            new Vector3(0, MathHelper.PiOver2, 0)
        };

        public static readonly List<List<Vector3>> ENEMY_WAYPOINTS_LIST = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
            ENEMY_1_INITIAL_POS,
            new Vector3(-16, AppData.ENEMY_POSITION_Y, -60),
            new Vector3(-2, AppData.ENEMY_POSITION_Y, -60),
            new Vector3(-2, AppData.ENEMY_POSITION_Y, -120)
            },

            new List<Vector3>()
            {
                ENEMY_2_INITIAL_POS,
                new Vector3(-39, AppData.ENEMY_POSITION_Y, -40)
            },

            new List<Vector3>()
            {
                ENEMY_3_INITIAL_POS,
                new Vector3(30, AppData.ENEMY_POSITION_Y, -12)
            }
        };

        public static readonly List<bool> ENEMY_MOVING_BOOLS = new List<bool>()
        {
            true,
            false,
            true
        };

        #endregion Enemy Variables

        #region Interactible Variables

        public static readonly float INTERACTION_DISTANCE = 6.5f;

        #endregion Interactible Variables

        #region Timer Variables

        internal static double MAX_GAME_TIME_IN_MSECS = 180000;

        #endregion Timer Variables

        #region Physics Variables

        public static readonly Vector3 GRAVITY = new Vector3(0, -9.81f, 0);

        #endregion

        #region Model Paths

        #region Enemy Model Paths

        public static readonly string ENEMY_MODEL_PATH = "Assets/Models/Enemies/hollow";

        #endregion Enemy Model Paths

        #region Walls, Floor and Ceiling Model Paths

        public static readonly string FLOOR_MODEL_PATH = "Assets/Models/Floors/ground_floor";
        public static readonly string CEILING_MODEL_PATH = "Assets/Models/Floors/ceiling";

        #endregion Walls, Floor and Ceiling Model Paths

        #region Office Room Model Paths

        public static readonly string OFFICE_NOTE_MODEL_PATH = "Assets/Models/Shopping Centre/Notes/office_room_note";

        #endregion Office Room Model Paths

        public static readonly string CHECKOUT_DESK_MODEL_BASE_PATH = "Assets/Models/Shopping Centre/Checkout Desks/";

        public static readonly string BENCH_MODELS_PATH = "Assets/Models/Shopping Centre/Benches/";

        public static readonly string BIN_MODELS_PATH = "Assets/Models/Shopping Centre/Bins/";

        public static readonly string BARRICADE_MODELS_PATH = "Assets/Models/Shopping Centre/Shopping Cart/Barricades/barricade_";

        #endregion Model Paths

        #region Texture Paths

        #region Enemy Texture Paths

        public static readonly string ENEMY_TEXTURE_PATH = "Assets/Textures/Enemies/black";

        #endregion Enemy Texture Paths

        public static readonly string WALL_TEXTURE_PATH = "Assets/Textures/walls";

        #region Office Room Texture Paths

        public static readonly string OFFICE_NOTE_TEXTURE_PATH = "Assets/Textures/Shopping Centre/Notes/office_room_note";

        #endregion Office Room Texture Paths

        public static readonly string SHELF_TEXTURE_PATH = "Assets/Textures/shop_shelf";

        public static readonly string CHECKOUT_DESK_TEXTURE_BASE_PATH = "Assets/Textures/Shopping Centre/Checkout Desk/";

        public static readonly string BIN_TEXTURES_PATH = "Assets/Textures/Shopping Centre/Bins/";

        public static readonly string SHOPPING_CART_TEXTURES_PATH = "Assets/Textures/Shopping Centre/Shopping Cart/";

        #endregion Texture Paths

        #region Font Paths

        public static readonly string PERF_FONT_PATH = "Assets/Fonts/Perf";

        #endregion Font Paths
    }
}