#region Pre-compiler directives

//#define DEMO
#define HI_RES

#endregion

using GD.Engine;
using GD.Engine.Data;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Drawing;

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

        public static readonly Vector3 FIRST_PERSON_DEFAULT_CAMERA_POSITION = new Vector3(0.4f, 7f, 76f);
        public static readonly Vector3 OLD_FIRST_PERSON_DEFAULT_CAMERA_POSITION = new Vector3(-10, 3.5f, 35);

        public static readonly float FIRST_PERSON_CAMERA_FCP = 3000;
        public static readonly float FIRST_PERSON_CAMERA_NCP = 0.1f;

        public static readonly float FIRST_PERSON_HALF_FOV
             = MathHelper.ToRadians(45);

        public static readonly float FIRST_PERSON_CAMERA_SMOOTH_FACTOR = 0.1f;

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

        public static readonly float OLD_PLAYER_MOVE_SPEED = 0.01f;
        private static readonly float OLD_PLAYER_STRAFE_SPEED_MULTIPLIER = 0.75f;
        public static readonly float OLD_PLAYER_STRAFE_SPEED = OLD_PLAYER_STRAFE_SPEED_MULTIPLIER * OLD_PLAYER_MOVE_SPEED;

        public static readonly float PLAYER_MOVE_SPEED = 0.05f;
        private static readonly float PLAYER_STRAFE_SPEED_MULTIPLIER = 0.6f;
        public static readonly float PLAYER_STRAFE_SPEED = PLAYER_STRAFE_SPEED_MULTIPLIER * PLAYER_MOVE_SPEED;

        //can use either same X-Y rotation for camera controller or different
        public static readonly float PLAYER_ROTATE_SPEED_SINGLE = 0.001f;

        //why bother? can you tilt your head at the same speed as you rotate it?
        public static readonly Vector2 PLAYER_ROTATE_SPEED_VECTOR2 = new Vector2(0.004f, 0.003f);

        public static readonly float PLAYER_ROTATE_MAX_X = 8f;
        public static readonly float PLAYER_ROTATE_MIN_X = -10f;

        public static readonly float PLAYER_DEFAULT_MULTIPLIER = 1f;
        public static readonly float PLAYER_RUN_MULTIPLIER = 2.5f;
        public static readonly float PLAYER_CROUCH_MULTIPLIER = 0.9f;

        public static readonly float PLAYER_CROUCH_HEIGHT_OFFSET = 1.75f;

        public static readonly float OLD_PLAYER_ROTATE_GAMEPAD_MULTIPLIER = 15f;
        public static readonly float PLAYER_ROTATE_GAMEPAD_MULTIPLIER = 0.2f;

        public static readonly float PLAYER_COLLIDABLE_JUMP_HEIGHT = 5;

        public static readonly float PLAYER_DEFAULT_CAPSULE_HEIGHT = 3.6f;

        //public static readonly float PLAYER_CROUCHED_CAPSULE_HEIGHT = 2.3f;
        public static readonly float PLAYER_CROUCHED_CAPSULE_HEIGHT = 2.5f;

        #endregion Movement Constants

        #region Enemy Variables

        public static readonly float ENEMY_POSITION_Y = 4f;
        public static readonly float ENEMY_SCALE = 0.015f;

        public static readonly float ENEMY_MOVEMENT_SPEED = 0.005f;

        // ENEMY 1 - Middle Lanes Enemy
        // ENEMY 2 - Office Guarding Enemy
        // ENEMY 3 - Right Lane Enemy

        public static readonly Vector3 ENEMY_1_INITIAL_POS = new Vector3(6.8f, AppData.ENEMY_POSITION_Y, -79f);
        public static readonly Vector3 ENEMY_2_INITIAL_POS = new Vector3(-23f, AppData.ENEMY_POSITION_Y, -71f);
        public static readonly Vector3 ENEMY_3_INITIAL_POS = new Vector3(39f, AppData.ENEMY_POSITION_Y, -78f);

        public static readonly List<Vector3> ENEMY_INITIAL_POSITIONS = new List<Vector3>()
        {
            ENEMY_1_INITIAL_POS,
            ENEMY_2_INITIAL_POS,
            ENEMY_3_INITIAL_POS
        };

        public static readonly List<Vector3> ENEMY_INITIAL_ROTATIONS = new List<Vector3>()
        {
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 90, 0)
        };

        public static readonly List<List<Vector3>> ENEMY_WAYPOINTS_LIST = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
            ENEMY_1_INITIAL_POS,
            new Vector3(6.8f, AppData.ENEMY_POSITION_Y, -20.3f),
            new Vector3(24f, AppData.ENEMY_POSITION_Y, -20.3f),
            new Vector3(24f, AppData.ENEMY_POSITION_Y, -79f)
            },

            new List<Vector3>()
            {
                ENEMY_2_INITIAL_POS,
                new Vector3(-23f, AppData.ENEMY_POSITION_Y, -6f)
            },

            new List<Vector3>()
            {
                ENEMY_3_INITIAL_POS,
                new Vector3(39f, AppData.ENEMY_POSITION_Y, 23f)
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

        public static readonly float INTERACTION_DISTANCE = 3.5f;

        #endregion Interactible Variables

        #region Pickup Variables

        public static readonly Vector3 FUSE_220V_TRANSLATION = new Vector3(-2.4f, 2.3f, -74.8f);
        public static readonly Vector3 FUSE_440V_TRANSLATION = new Vector3(-0.7f, 2.3f, -28f);

        #endregion

        #region Timer Variables

        public static double MAX_GAME_TIME_IN_MSECS = 180000;
        //public static double MAX_GAME_TIME_IN_MSECS = 25000;

        #endregion Timer Variables

        #region Physics Variables

        public static readonly Vector3 GRAVITY = new Vector3(0, -9.81f, 0);

        #endregion

        #region Object Names

        public static readonly string GATE_ACCESS_MACHINE_NAME = "gate access machine";
        public static readonly string KEYCARD_NAME = "office keycard";

        public static readonly string GENERATOR_NAME = "generator";
        public static readonly string GENERATOR_DOOR_NAME = "generator door";
        public static readonly string FUSE_BOX_NAME = "fuse box";
        public static readonly string FUSE_220V_NAME = "fuse 220v";
        public static readonly string FUSE_440V_NAME = "fuse 440v";

        public static readonly string EXIT_DOOR_NAME = "exit door";
        public static readonly string EXIT_DOOR_FRAME_NAME = "exit door frame";

        #endregion

        #region UI Object Names

        public static readonly string INFECTION_METER_NAME = "infection meter";
        public static readonly string INTERACT_PROMPT_NAME = "interact prompt";
        public static readonly string SUBTITLES_NAME = "subtitles";

        #endregion

        #region Sound Object Names

        public static readonly string GENERATOR_SOUND_NAME = "generator-sound";

        #endregion

        public static readonly List<Vector3> WALL_TRANSLATIONS = new List<Vector3>()
        {
            // Left wall
            new Vector3(-42.4f, 5.5f, -21.5f),
            // Back wall
            new Vector3(0, 5.5f, 50.15f),
            // Right wall
            new Vector3(39f, 5.5f, -52)
        };

        public static readonly List<Vector3> WALL_ROTATIONS = new List<Vector3>()
        {
            // Left wall
            new Vector3(0, -0.55f, 0),
            // Back wall
            new Vector3(0, 0f, 0),
            // Right wall
            new Vector3(0, 0, 0)
        };

        public static readonly List<Vector3> WALL_SCALES = new List<Vector3>()
        {
            // Left wall
            DEFAULT_OBJECT_SCALE * new Vector3(0.3f,2.8f,35.5f),
            // Back wall
            DEFAULT_OBJECT_SCALE * new Vector3(35.5f, 2.8f, 0.3f),
            // Right wall
            DEFAULT_OBJECT_SCALE * new Vector3(0.3f,2.8f,38f),
        };

        #region Model Paths

        #region Enemy Model Paths

        public static readonly string ENEMY_MODEL_PATH = "Assets/Models/Enemies/Hollow01";

        #endregion Enemy Model Paths

        #region Walls, Floor and Ceiling Model Paths

        public static readonly string FLOOR_MODEL_PATH = "Assets/Models/Floors/ground_floor";
        public static readonly string CEILING_MODEL_PATH = "Assets/Models/Floors/ceiling";

        #endregion Walls, Floor and Ceiling Model Paths

        #region Office Room Model Paths

        public static readonly string OFFICE_NOTE_MODEL_PATH = "Assets/Models/Shopping Centre/Notes/office_room_note";

        #endregion Office Room Model Paths

        public static readonly string CHECKOUT_DESK_MODEL_BASE_PATH = "Assets/Models/Shopping Centre/Checkout Desks/";

        public static readonly string BIN_MODELS_PATH = "Assets/Models/Shopping Centre/Bins/";

        public static readonly string BARRICADE_MODELS_PATH = "Assets/Models/Shopping Centre/Shopping Cart/Barricades/barricade_";

        #endregion Model Paths

        #region Texture Paths

        #region Enemy Texture Paths

        public static readonly string ENEMY_TEXTURE_PATH = "Assets/Textures/Enemies/scales";

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

        #region Lights

        public static readonly string LIGHT_MODEL_PATH = "Assets/Models/Shopping Centre/Light/light";

        public static readonly string LIGHT_TEXTURE_PATH = "Assets/Textures/Shopping Centre/Light/light_rust";

        public static readonly Vector3 LIGHT_OFFSET_X = new Vector3(22f, 0, 0);

        public static readonly Vector3 LIGHT_OFFSET_Z = new Vector3(0, 0, 22f);

        #endregion Lights

        #region Scaffolding

        public static readonly string SCAFFOLDING_MDOEL_PATH = "Assets/Models/Shopping Centre/Scaffolding/scaffolding";

        public static readonly string SCAFFOLDING_TEXTURE_PATH = "Assets/Textures/Shopping Centre/Scaffolding/scaffolding";

        public static readonly float SCAFFOLDING_COLLIDER_SCALE_X = 115f;
        public static readonly float SCAFFOLDING_COLLIDER_SCALE_Y = 270f;
        public static readonly float SCAFFOLDING_COLLIDER_SCALE_Z = 250f;

        public static readonly Vector3 SCAFFOLDING_POSITION = new Vector3(45.9f, 2.5f, 42.7f);
        public static readonly Vector3 SCAFFOLDING_OFFSET_Z = new Vector3(0, 0, 5.4f);

        #endregion Scaffolding

        #region Fridges

        public static readonly string FRIDGE_MDOEL_PATH = "Assets/Models/Shopping Centre/Fridges/fridge";

        public static readonly string FRIDGE_TEXTURE_PATH = "Assets/Textures/Shopping Centre/Fridge/fridge";

        public static readonly float FRIDGE_COLLIDER_SCALE_X = 115f;
        public static readonly float FRIDGE_COLLIDER_SCALE_Y = 270f;
        public static readonly float FRIDGE_COLLIDER_SCALE_Z = 150f;

        public static readonly Vector3 FRIDGE_POSITION = new Vector3(47.5f, 2.6f, -82.4f);
        public static readonly Vector3 FRIDGE_OFFSET_Z = new Vector3(0, 0, 3.4f);

        #endregion Fridges

        #region Benches

        public static readonly string BENCH_BASE_MODEL_PATH = "Assets/Models/Shopping Centre/Benches/Bench Bases/bench_base";

        #endregion Benches

        #region Aisle Labels
        private static readonly string LABEL_BASE_PATH = "Assets/Textures/Aisles/Labels/aisle_";

        public static readonly List<string> LABELS_LIST = new List<string>()
        {
            LABEL_BASE_PATH + "1",
            LABEL_BASE_PATH + "2",
            LABEL_BASE_PATH + "3",
            LABEL_BASE_PATH + "4",
            LABEL_BASE_PATH + "5",
            LABEL_BASE_PATH + "6",
            LABEL_BASE_PATH + "7",

        };


        #endregion Aisle Labels

        #region Bottle Labels
        private static readonly string BOTTLE_LABELS_BASE_PATH = "Assets/Textures/Aisles/Beverages/";

        public static readonly List<string> BOTTLE_LABELS_LIST = new List<string>()
        {
            BOTTLE_LABELS_BASE_PATH + "budweiser_label",
            BOTTLE_LABELS_BASE_PATH + "heineken_label",
            BOTTLE_LABELS_BASE_PATH + "coors_label",
        };
        #endregion Bottle Labels

        #region Font Paths

        public static readonly string PERF_FONT_PATH = "Assets/Fonts/Perf";

        #endregion Font Paths
    }
}