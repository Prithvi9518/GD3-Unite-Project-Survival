#region Pre-compiler directives

//#define DEMO
#define SHOW_DEBUG_INFO
//#define SHOW_TIMER_TEXT

#endregion

using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using GD.Core;
using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using GD.Engine.Inputs;
using GD.Engine.Managers;
using GD.Engine.Parameters;
using GD.Engine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using Application = GD.Engine.Globals.Application;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Cue = GD.Engine.Managers.Cue;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace GD.App
{
    public class Main : Game
    {
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect unlitEffect;
        private BasicEffect litEffect;

        private CameraManager cameraManager;
        private SceneManager sceneManager;
        private SoundManager soundManager;
        private PhysicsManager physicsManager;
        private RenderManager renderManager;
        private EventDispatcher eventDispatcher;
        private StateManager stateManager;

        #endregion Fields

        #region Constructors

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion Constructors

        #region Actions - Initialize

        protected override void Initialize()
        {
            //moved spritebatch initialization here because we need it in InitializeDebug() below
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //core engine - common across any game
            InitializeEngine(AppData.APP_RESOLUTION, true, true);

            //game specific content
            InitializeLevel(AppData.GAME_TITLE, AppData.SKYBOX_WORLD_SCALE);

#if SHOW_DEBUG_INFO
            InitializeDebug();
#endif

#if SHOW_TIMER_TEXT
            InitializeTimerText();
#endif

            base.Initialize();
        }

        #endregion Actions - Initialize

        #region Actions - Level Specific

        protected override void LoadContent()
        {
            //moved spritebatch initialization to Main::Initialize() because we need it in InitializeDebug()
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void InitializeLevel(string title, float worldScale)
        {
            //set game title
            SetTitle(title);

            //load sounds, textures, models etc
            LoadMediaAssets();

            //initialize curves used by cameras
            InitializeCurves();

            //initialize rails used by cameras
            InitializeRails();

            //add scene manager and starting scenes
            InitializeScenes();

            //add collidable drawn stuff
            //InitializeCollidableContent(worldScale);

            //add non-collidable drawn stuff
            InitializeNonCollidableContent(worldScale);
        }

        private void SetTitle(string title)
        {
            Window.Title = title.Trim();
        }

        private void LoadMediaAssets()
        {
            //sounds, models, textures
            LoadSounds();
            LoadTextures();
            LoadModels();
        }

        private void LoadSounds()
        {
            var soundEffect =
                Content.Load<SoundEffect>("Assets/Audio/Diegetic/explode1");

            //add the new sound effect
            soundManager.Add(new Cue(
                "boom1",
                soundEffect,
                SoundCategoryType.Alarm,
                new Vector3(1, 1, 0),
                false));
        }

        private void LoadTextures()
        {
            //load and add to dictionary
        }

        private void LoadModels()
        {
            //load and add to dictionary
        }

        private void InitializeCurves()
        {
            //load and add to dictionary
        }

        private void InitializeRails()
        {
            //load and add to dictionary
        }

        private void InitializeScenes()
        {
            //initialize a scene
            var scene = new Scene(AppData.SCENE_NAME);

            //add scene to the scene manager
            sceneManager.Add(scene.ID, scene);

            //don't forget to set active scene
            sceneManager.SetActiveScene(AppData.SCENE_NAME);
        }

        private void InitializeEffects()
        {
            //only for skybox with lighting disabled
            unlitEffect = new BasicEffect(_graphics.GraphicsDevice);
            unlitEffect.TextureEnabled = true;

            //all other drawn objects
            litEffect = new BasicEffect(_graphics.GraphicsDevice);
            litEffect.TextureEnabled = true;
            litEffect.LightingEnabled = true;
            litEffect.EnableDefaultLighting();
        }

        private void InitializeCameras()
        {
            //camera
            GameObject cameraGameObject = null;

            #region First Person

            cameraGameObject = new GameObject(AppData.FIRST_PERSON_CAMERA_NAME);

            cameraGameObject.Transform = new Transform(null, null, AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION);

            // Camera component
            cameraGameObject.AddComponent(
                new Camera(
                AppData.FIRST_PERSON_HALF_FOV, //MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                AppData.FIRST_PERSON_CAMERA_NCP, //0.1f,
                AppData.FIRST_PERSON_CAMERA_FCP,
                new Viewport(0, 0, _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight))); // 3000

            // First person controller component
            cameraGameObject.AddComponent(new FirstPersonController(AppData.FIRST_PERSON_MOVE_SPEED, AppData.FIRST_PERSON_STRAFE_SPEED,
                AppData.PLAYER_ROTATE_SPEED_VECTOR2, true));

            // Item interaction controller component
            cameraGameObject.AddComponent(new InteractionController());

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion First Person

            cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
        }

        private void InitializeCollidableContent(float worldScale)
        {
        }

        private void InitializeNonCollidableContent(float worldScale)
        {
            //create sky
            //InitializeSkyBoxAndGround(worldScale);

            InitializeShoppingCentre();

            InitializeEnemies();

            // testing interactable code
            //TestingInteractableItem();
        }

        private Renderer InitializeRenderer(string modelPath, string texturePath, GDBasicEffect effect, float alpha)
        {
            var model = Content.Load<Model>(modelPath);
            var texture = Content.Load<Texture2D>(texturePath);
            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);

            return new Renderer(effect,
                new Material(texture, alpha),
                mesh);
        }

        private Renderer InitializeRenderer(string modelPath, string texturePath, GDBasicEffect effect, float alpha, Color color)
        {
            var model = Content.Load<Model>(modelPath);
            var texture = Content.Load<Texture2D>(texturePath);
            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);

            return new Renderer(effect,
                new Material(texture, alpha, color),
                mesh);
        }

        private void InitializeEnemies()
        {
            Renderer enemyRenderer = null;
            GameObject gameObject = null;
            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            for (int i = 0; i < AppData.ENEMY_WAYPOINTS_LIST.Count; i++)
            {
                enemyRenderer = InitializeRenderer(
                AppData.ENEMY_MODEL_PATH,
                AppData.ENEMY_TEXTURE_PATH,
                gdBasicEffect,
                1
                );

                gameObject = new GameObject("enemy " + (i + 1), ObjectType.Static, RenderType.Opaque);

                gameObject.Transform = new Transform(
                AppData.ENEMY_SCALE * Vector3.One,
                AppData.ENEMY_INITIAL_ROTATIONS[i],
                AppData.ENEMY_INITIAL_POSITIONS[i]
                );

                gameObject.AddComponent(enemyRenderer);

                gameObject.AddComponent(new EnemyPatrolBehaviour(
                    AppData.ENEMY_WAYPOINTS_LIST[i], AppData.ENEMY_MOVEMENT_SPEED, AppData.ENEMY_MOVING_BOOLS[i])
                    );

                sceneManager.ActiveScene.Add(gameObject);
            }
        }

        private void InitializeTimerText()
        {
            //intialize the utility component
            var perfUtility = new PerfUtility(this, _spriteBatch,
                new Vector2(10, 10),
                new Vector2(0, 22));

            //set the font to be used
            var spriteFont = Content.Load<SpriteFont>(AppData.PERF_FONT_PATH);

            //add TimerInfo to the info list
            float timerScale = 1.5f;

            perfUtility.infoList.Add(
                new TimerInfo(_spriteBatch, spriteFont, "Time Remaining: ", Color.OrangeRed, timerScale * Vector2.One)
                );

            //add to the component list otherwise it wont have its Update or Draw called!
            Components.Add(perfUtility);
        }

        private void InitializeShoppingCentre()
        {
            InitializeOfficeModels();
            InitializeGeneratorRoomModels();
            InitializeFloors();
            InitializeWalls();
            InitializeShoppingCentreAssets();
            InitializeAisles();
            InitializeCoffeeShop();
        }

        private void InitializeFloors()
        {
            var gdBasicEffect = new GDBasicEffect(litEffect);

            #region Floor

            var gameObject = new GameObject("ground_floor",
               ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer(
                AppData.FLOOR_MODEL_PATH,
                AppData.WALL_TEXTURE_PATH,
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Floor

            #region Ceiling

            gameObject = new GameObject("ceiling",
            ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                AppData.CEILING_MODEL_PATH,
                AppData.WALL_TEXTURE_PATH,
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeShoppingCentreAssets()
        {
            InitializeDoors();
            InitializeVendingMachine();
            InitializeFridges();
            InitializeLights();
            InitializeScaffolding();
            InitializeShoppingCart();
            InitializeBins();
            InitializeBenches();
            InitializeCheckoutDesks();
            InitializeNotes();
        }

        private void InitializeNotes()
        {
            #region Notes

            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            #region Office Note

            var gameObject = new GameObject("office room note", ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer(
                AppData.OFFICE_NOTE_MODEL_PATH,
                AppData.OFFICE_NOTE_TEXTURE_PATH,
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Office Note

            #endregion Notes
        }

        private void InitializeCheckoutDesks()
        {
            #region Checkout Desks

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            #region Belt

            string belt_base_path = AppData.CHECKOUT_DESK_MODEL_BASE_PATH + "Belt/belt_";

            for (int i = 1; i <= 3; i++)
            {
                gameObject = new GameObject("belt " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = belt_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.CHECKOUT_DESK_TEXTURE_BASE_PATH + "belt",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Belt

            #region Cashier

            string cashier_path = AppData.CHECKOUT_DESK_MODEL_BASE_PATH + "Cashier/cashier_";

            for (int i = 1; i <= 3; i++)
            {
                gameObject = new GameObject("cashier " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = cashier_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.CHECKOUT_DESK_TEXTURE_BASE_PATH + "cash_register",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);

                #endregion Cashier

                #endregion Checkout Desks
            }
        }

        private void InitializeBenches()
        {
            #region Benches

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            #region Bench Bases

            string benches_bottom_base_path = AppData.BENCH_MODELS_PATH + "Bench Bases/bench_base_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("bench base " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = benches_bottom_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    "Assets/Textures/Shopping Centre/Benches/legs",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Bench Bases

            #region Bench Tops

            string benches_top_base_path = AppData.BENCH_MODELS_PATH + "Bench Tops/bench_top_";

            for (int i = 1; i <= 2; i++)
            {
                gameObject = new GameObject("bench top " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = benches_top_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    "Assets/Textures/Shopping Centre/Benches/wood_top",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Bench Tops

            #endregion Benches
        }

        private void InitializeBins()
        {
            #region Bins

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            #region Bin Bags

            string bins_bags_base_path = AppData.BIN_MODELS_PATH + "Bin Bags/bin_bag_";

            for (int i = 1; i <= 6; i++)
            {
                gameObject = new GameObject("bin bag " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = bins_bags_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.BIN_TEXTURES_PATH + "Bin Bag/bin_bag",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Bin Bags

            #region Bin Baskets

            string bins_baskets_base_path = AppData.BIN_MODELS_PATH + "Bin Baskets/bin_basket_";

            for (int i = 1; i <= 3; i++)
            {
                gameObject = new GameObject("bin basket " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = bins_baskets_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.BIN_TEXTURES_PATH + "Bin Basket/bin_basket",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Bin Baskets

            #region Fire Extinguisher

            string fire_extinguishers_base_path = AppData.BIN_MODELS_PATH + "Fire Extinguishers/fire_extinguisher_";

            for (int i = 1; i <= 2; i++)
            {
                gameObject = new GameObject("fire extinguisher " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = fire_extinguishers_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.BIN_TEXTURES_PATH + "Fire Extinguisher/fire_extinguisher",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Fire Extinguisher

            #region Large Bin

            gameObject = new GameObject("large bin", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                   AppData.BIN_MODELS_PATH + "Large Bin/large_bin",
                   AppData.BIN_TEXTURES_PATH + "Large Bin/large_bin",
                   gdBasicEffect,
                   1,
                   Color.White
                   );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Large Bin

            #region Pallet

            gameObject = new GameObject("pallet", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                   AppData.BIN_MODELS_PATH + "Pallet/pallet",
                   AppData.BIN_TEXTURES_PATH + "Pallet/pallet_wood",
                   gdBasicEffect,
                   1,
                   Color.White
                   );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Pallet

            #region Plastic Bottles

            string plastic_bottles_base_path = AppData.BIN_MODELS_PATH + "Plastic Bottles/plastic_bottle_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("plastic bottle " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = plastic_bottles_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    AppData.BIN_TEXTURES_PATH + "Plastic Bottle/plastic_bottle",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Plastic Bottles

            #endregion Bins
        }

        private void InitializeShoppingCart()
        {
            #region Shopping Cart

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string texture_base_start = AppData.SHOPPING_CART_TEXTURES_PATH;

            #region Barricades

            string barricades_base_path = AppData.BARRICADE_MODELS_PATH;

            for (int i = 1; i <= 5; i++)
            {
                gameObject = new GameObject("barricade " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = barricades_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_base_start + "Barricade/barricade",
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Barricades

            #region Trolleys

            string base_fridges_base_path = "Assets/Models/Shopping Centre/Shopping Cart/Trolleys/trolley_";

            for (int i = 1; i <= 8; i++)
            {
                gameObject = new GameObject("trolley " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = base_fridges_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_base_start + "Trolley/trolley_metal",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Trolleys

            #endregion Shopping Cart
        }

        private void InitializeScaffolding()
        {
            #region Scaffolding

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string scaffolding_base_path = "Assets/Models/Shopping Centre/Scaffolding/scaffolding_";

            for (int i = 1; i <= 3; i++)
            {
                gameObject = new GameObject("scaffolding " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = scaffolding_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    "Assets/Textures/Shopping Centre/Scaffolding/scaffolding",
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Scaffolding
        }

        private void InitializeFridges()
        {
            #region Fridges

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string fridges_base_path = "Assets/Models/Shopping Centre/Fridges/fridge_";

            for (int i = 1; i <= 5; i++)
            {
                gameObject = new GameObject("fridge " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = fridges_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    "Assets/Textures/Shopping Centre/Fridge/fridge",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Fridges
        }

        private void InitializeLights()
        {
            #region Lights

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            string coffee_chairs_base_path = "Assets/Models/Shopping Centre/Lights/light_";
            Renderer renderer = null;

            for (int i = 1; i <= 15; i++)
            {
                gameObject = new GameObject("ligth " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = coffee_chairs_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    "Assets/Textures/Shopping Centre/Light/light_rust",
                    gdBasicEffect,
                    1,
                    Color.White
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Lights
        }

        private void InitializeDoors()
        {
            InitializeGeneratorDoor();
            InitializeExit();
            InitializeShutter();
        }

        private void InitializeGeneratorDoor()
        {
            #region Generator Door

            var gameObject = new GameObject("generator door",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Generator Room Door/generator_room_door",
                    "Assets/Textures/Shopping Centre/Doors/Generator Room Door/generator_room_door",
                    new GDBasicEffect(unlitEffect),
                    1,
                    Color.White
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Generator Door
        }

        private void InitializeExit()
        {
            #region Exit

            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            #region Exit Door

            var gameObject = new GameObject("exit door",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/exit_door",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_door",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Exit Door

            #region Exit Sign

            gameObject = new GameObject("exit sign",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/Exit Sign/exit_sign",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_sign",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Exit Sign

            #endregion Exit
        }

        private void InitializeVendingMachine()
        {
            #region Vending Machines

            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            string model_path = "Assets/Models/Shopping Centre/Vending Machines/vending_machine_";
            string texture_path = "Assets/Textures/Shopping Centre/Vending Machines/";

            #region Coke Vending Machine

            var gameObject = new GameObject("vending machine 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer(
                    model_path + "1",
                    texture_path + "coke",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Coke Vending Machine

            #region Pepsi Vending Machine

            gameObject = new GameObject("vending machine 2", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    model_path + "2",
                    texture_path + "pepsi",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Pepsi Vending Machine

            #region Sprite Vending Machine

            gameObject = new GameObject("vending machine 3", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    model_path + "3",
                    texture_path + "sprite",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Pepsi Vending Machine

            #endregion Vending Machines
        }

        private void InitializeCoffeeShop()
        {
            InitializeCoffeeShopStall();
            InitializeChairs();
            InitializeTables();
            InitializeWaterDispensers();
        }

        private void InitializeWaterDispensers()
        {
            #region Water Dispensers

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string texture_path = "Assets/Textures/Shopping Centre/Water Dispenser/water_dispenser";
            string model_base_path = "Assets/Models/Coffee Shop/water dispensers/water_dispenser_";

            for (int i = 0; i < 2; i++)
            {
                gameObject = new GameObject("water dispenser " + (i + 1),
                    ObjectType.Static, RenderType.Opaque);

                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                renderer = InitializeRenderer(
                    model_base_path + (i + 1),
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Water Dispensers
        }

        private void InitializeCoffeeShopStall()
        {
            #region Coffee Shop

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            Renderer renderer = null;

            string model_path = "Assets/Models/Coffee Shop/coffee_shop/coffee_shop_";
            string texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/";

            #region Coffee Shop Base

            var gameObject = new GameObject("coffee shop base",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    model_path + "base",
                    texture_path + "steel",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Coffee Shop Base

            #region Coffee Shop Cover

            gameObject = new GameObject("coffee shop cover",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    model_path + "cover",
                    texture_path + "cover",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Coffee Shop Cover

            #region Coffee Shop Panel

            gameObject = new GameObject("coffee shop panel",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    model_path + "panel",
                    texture_path + "wood",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Coffee Shop Panel

            #endregion
        }

        private void InitializeChairs()
        {
            #region Chairs

            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            var texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/Chair/chair_wood";

            GameObject gameObject = null;
            Renderer renderer = null;

            string coffee_chairs_base_path = "Assets/Models/Coffee Shop/chairs/coffee_chair_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("coffee chair " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = coffee_chairs_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion
        }

        private void InitializeTables()
        {
            #region Tables

            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            var model_base_path = "Assets/Models/Coffee Shop/tables/table_";
            var texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/Table/table_wood";

            GameObject gameObject = null;
            Renderer renderer = null;

            for (int i = 0; i < 2; i++)
            {
                gameObject = new GameObject("Table " + (i + 1),
                    ObjectType.Static, RenderType.Opaque);

                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                renderer = InitializeRenderer(
                model_base_path + (i + 1),
                texture_path,
                gdBasicEffect,
                1
                );
                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion
        }

        private void InitializeAisles()
        {
            #region Aisles

            var texture_path = AppData.SHELF_TEXTURE_PATH;
            InitializeClothesAisle(texture_path);
            InitializeBeautyAisle(texture_path);
            InitializeBeveragesAisle(texture_path);
            InitializeElectronicsAisle(texture_path);
            InitializePreparedFoodsAisle(texture_path);
            InitializeProducedFoodsAisle(texture_path);
            InitializeToysAisle(texture_path);
            InitializeWallAisle(texture_path);

            #endregion
        }

        private void InitializeShutter()
        {
            #region Shutter

            var gameObject = new GameObject("shutter",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            var texture_path = "Assets/Textures/Shopping Centre/Shutter/shutter_rust";

            var model_path = "Assets/Models/Shopping Centre/Doors/Shutter/shutter_model";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Shutter
        }

        private void InitializeGeneratorRoomModels()
        {
            #region Fuse Box

            var gameObject = new GameObject("fuse box",
                                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture_path = "Assets/Textures/Props/Generator_Room/fuse_box_diffuse";

            var model_path = "Assets/Models/Generator Room/fuse_box";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Gate Access Machine

            gameObject = new GameObject("gate access machine",
                                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/access_card_machine_emission";

            model_path = "Assets/Models/Generator Room/gate_access_machine";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Generator

            gameObject = new GameObject("generator",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/generator";

            model_path = "Assets/Models/Generator Room/generator";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Lever

            gameObject = new GameObject("lever",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/lever_base_colour";

            model_path = "Assets/Models/Generator Room/lever";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Panel

            gameObject = new GameObject("panel",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/panel_base_colour";

            model_path = "Assets/Models/Generator Room/panel";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Electrical Box

            gameObject = new GameObject("electrical box",
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/metal";

            model_path = "Assets/Models/Generator Room/electrical_box";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeWalls()
        {
            #region Shopping Centre Walls

            var gdBasicEffect = new GDBasicEffect(litEffect);
            var texture_path = "Assets/Textures/walls";
            GameObject gameObject = null;
            Renderer renderer = null;

            #region Main Walls

            string main_wall_base_path = "Assets/Models/Walls/wall_";

            for (int i = 1; i <= 8; i++)
            {
                gameObject = new GameObject("wall " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = main_wall_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion

            #region Shutter Walls

            string shutter_wall_base_path = "Assets/Models/Walls/Shutter Walls/shutter_wall_";

            for (int i = 1; i <= 2; i++)
            {
                gameObject = new GameObject("shutter wall " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = shutter_wall_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion

            #region Door Walls

            string doors_wall_base_path = "Assets/Models/Walls/Door Walls/door_wall_";

            for (int i = 1; i <= 2; i++)
            {
                gameObject = new GameObject("door wall " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = doors_wall_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Door Walls

            #endregion
        }

        #region Aisles

        private void InitializeWallAisle(string texture_path)
        {
            var gameObject = new GameObject("wall aisle",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            var model_path = "Assets/Models/Aisles/Wall Aisle/wall_aisle";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);
        }

        private void InitializeToysAisle(string texture_path)
        {
            #region Toys Aisle Shelf

            var gameObject = new GameObject("toys aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Toys/toys_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeProducedFoodsAisle(string texture_path)
        {
            #region Produced Foods Aisle Shelf

            var gameObject = new GameObject("produced foods aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Produced Foods/produced_foods_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializePreparedFoodsAisle(string texture_path)
        {
            #region Prepared Foods Aisle Shelf

            var gameObject = new GameObject("prepared foods aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Prepared Foods/prepared_foods_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeElectronicsAisle(string texture_path)
        {
            #region Electronics Aisle Shelf

            var gameObject = new GameObject("electronics aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Electronics/electronics_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Electronics Aisle Shelf

            #region Fuse

            gameObject = new GameObject("fuse", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform
                (AppData.DEFAULT_OBJECT_SCALE * 0.1f * Vector3.One,
                new Vector3(MathHelper.PiOver2, 0, 0),
                new Vector3(-10, 2.75f, -67));

            model_path = "Assets/Models/Fuse/fuse";
            texture_path = "Assets/Textures/Props/Fuse/Material_Base_Color";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            gameObject.AddComponent(new InteractableBehaviour());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Fuse
        }

        private void InitializeBeveragesAisle(string texture_path)
        {
            #region Beverages Aisle Shelf

            var gameObject = new GameObject("beverages aisle shelf",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Beverages/beverages_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeBeautyAisle(string texture_path)
        {
            #region Beauty Aisle Shelf

            var gameObject = new GameObject("beauty aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Beauty/beauty_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeClothesAisle(string texture_path)
        {
            #region Clothes Aisle Shelf

            var gameObject = new GameObject("clothes aisle shelf",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var model_path = "Assets/Models/Aisles/Clothes/clothes_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        #endregion

        private void InitializeOfficeModels()
        {
            GDBasicEffect gdBasicEffect = new GDBasicEffect(unlitEffect);

            #region Office Chair

            var gameObject = new GameObject("office chair",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            var texture_path = "Assets/Textures/Props/Office/ChairSetT2_Diffuse";

            var model_path = "Assets/Models/Office/office_chair";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Table

            gameObject = new GameObject("office table",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            texture_path = "Assets/Textures/Props/Office/TabletextureSet1_Diffuse";

            model_path = "Assets/Models/Office/office_table";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Shelves

            texture_path = "Assets/Textures/Props/Office/metal";
            string model_base_path = "Assets/Models/Office/Shelves/office_shelf_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("office shelf " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                model_path = model_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Office Shelves

            #region Office Rug

            gameObject = new GameObject("office rug",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Props/Crates/crate1";

            model_path = "Assets/Models/Office/office_rug";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office PC

            gameObject = new GameObject("office pc",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Props/Generator_Room/metal";

            model_path = "Assets/Models/Office/office_pc";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Screen

            gameObject = new GameObject("office screen",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Props/Generator_Room/metal";

            model_path = "Assets/Models/Office/office_screen";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Keyboard

            gameObject = new GameObject("office keyboard",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Props/Generator_Room/metal";

            model_path = "Assets/Models/Office/office_keyboard";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Keycard

            gameObject = new GameObject("office keycard",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.04f * Vector3.One, Vector3.Zero, new Vector3(-67, 2f, -109));
            texture_path = "Assets/Textures/Props/Office/keycard_albedo";

            model_path = "Assets/Models/Keycard/keycard_unapplied";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            gameObject.AddComponent(new InteractableBehaviour());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Drawers

            texture_path = "Assets/Textures/Props/Generator_Room/metal";
            model_base_path = "Assets/Models/Office/Drawers/office_drawer_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("office drawer " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

                model_path = model_base_path + i;

                renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

                gameObject.AddComponent(renderer);

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion
        }

        private void InitializeSkyBoxAndGround(float worldScale)
        {
            float halfWorldScale = worldScale / 2.0f;

            GameObject quad = null;
            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            var quadMesh = new QuadMesh(_graphics.GraphicsDevice);

            //skybox - back face
            quad = new GameObject("skybox back face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), null, new Vector3(0, 0, -halfWorldScale));
            var texture = Content.Load<Texture2D>("Assets/Textures/Skybox/back");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - left face
            quad = new GameObject("skybox left face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(90), 0), new Vector3(-halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/left");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - right face
            quad = new GameObject("skybox right face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(-90), 0), new Vector3(halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/right");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - top face
            quad = new GameObject("skybox top face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(MathHelper.ToRadians(90), MathHelper.ToRadians(-90), 0), new Vector3(0, halfWorldScale, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/sky");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - front face
            quad = new GameObject("skybox front face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(-180), 0), new Vector3(0, 0, halfWorldScale));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/front");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //ground
            quad = new GameObject("ground");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(MathHelper.ToRadians(-90), 0, 0), new Vector3(0, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Foliage/Ground/grass1");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);
        }

        #endregion Actions - Level Specific

        #region Actions - Engine Specific

        private void InitializeEngine(Vector2 resolution, bool isMouseVisible, bool isCursorLocked)
        {
            //add support for mouse etc
            InitializeInput();

            //add game effects
            InitializeEffects();

            //add dictionaries to store and access content
            InitializeDictionaries();

            //add camera, scene manager
            InitializeManagers();

            //share some core references
            InitializeGlobals();

            //set screen properties (incl mouse)
            InitializeScreen(resolution, isMouseVisible, isCursorLocked);

            //add game cameras
            InitializeCameras();
        }

        private void InitializeGlobals()
        {
            //Globally shared commonly accessed variables
            Application.Main = this;
            Application.GraphicsDeviceManager = _graphics;
            Application.GraphicsDevice = _graphics.GraphicsDevice;
            Application.Content = Content;

            //Add access to managers from anywhere in the code
            Application.CameraManager = cameraManager;
            Application.SceneManager = sceneManager;
            Application.SoundManager = soundManager;

            Application.StateManager = stateManager;
        }

        private void InitializeInput()
        {
            //Globally accessible inputs
            Input.Keys = new KeyboardComponent(this);
            Components.Add(Input.Keys);
            Input.Mouse = new MouseComponent(this);
            Components.Add(Input.Mouse);
            Input.Gamepad = new GamepadComponent(this);
            Components.Add(Input.Gamepad);
        }

        /// <summary>
        /// Sets game window dimensions and shows/hides the mouse
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="isMouseVisible"></param>
        /// <param name="isCursorLocked"></param>
        private void InitializeScreen(Vector2 resolution, bool isMouseVisible, bool isCursorLocked)
        {
            Screen screen = new Screen();

            //set resolution
            screen.Set(resolution, isMouseVisible, isCursorLocked);

            //set global for re-use by other entities
            Application.Screen = screen;

            //set starting mouse position i.e. set mouse in centre at startup
            Input.Mouse.Position = screen.ScreenCentre;

            ////calling set property
            //_graphics.PreferredBackBufferWidth = (int)resolution.X;
            //_graphics.PreferredBackBufferHeight = (int)resolution.Y;
            //IsMouseVisible = isMouseVisible;
            //_graphics.ApplyChanges();
        }

        private void InitializeManagers()
        {
            //add event dispatcher for system events - the most important element!!!!!!
            eventDispatcher = new EventDispatcher(this);
            //add to Components otherwise no Update() called
            Components.Add(eventDispatcher);

            //add support for multiple cameras and camera switching
            cameraManager = new CameraManager(this);
            //add to Components otherwise no Update() called
            Components.Add(cameraManager);

            //big kahuna nr 1! this adds support to store, switch and Update() scene contents
            sceneManager = new SceneManager(this);
            //add to Components otherwise no Update()
            Components.Add(sceneManager);

            //big kahuna nr 2! this renders the ActiveScene from the ActiveCamera perspective
            renderManager = new RenderManager(this, new ForwardSceneRenderer(_graphics.GraphicsDevice));
            Components.Add(renderManager);

            //add support for playing sounds
            soundManager = new SoundManager();
            //why don't we add SoundManager to Components? Because it has no Update()
            //wait...SoundManager has no update? Yes, playing sounds is handled by an internal MonoGame thread - so we're off the hook!

            //add the physics manager update thread
            physicsManager = new PhysicsManager(this);
            Components.Add(physicsManager);

            //add state manager for inventory and countdown
            stateManager = new StateManager(this, AppData.MAX_GAME_TIME_IN_MSECS);
            Components.Add(stateManager);
        }

        private void InitializeDictionaries()
        {
            //TODO - add texture dictionary, soundeffect dictionary, model dictionary
        }

        private void InitializeDebug()
        {
            //intialize the utility component
            var perfUtility = new PerfUtility(this, _spriteBatch,
                new Vector2(10, 10),
                new Vector2(0, 22));

            //set the font to be used
            var spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Perf");

            //add components to the info list to add UI information
            float headingScale = 1f;
            float contentScale = 0.9f;
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Performance ------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new FPSInfo(_spriteBatch, spriteFont, "FPS:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Camera -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new CameraNameInfo(_spriteBatch, spriteFont, "Name:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new CameraPositionInfo(_spriteBatch, spriteFont, "Pos:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new CameraRotationInfo(_spriteBatch, spriteFont, "Rot:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Object -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new ObjectInfo(_spriteBatch, spriteFont, "Objects:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Hints -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Use mouse scroll wheel to change security camera FOV, F1-F4 for camera switch", Color.White, contentScale * Vector2.One));

            //add to the component list otherwise it wont have its Update or Draw called!
            Components.Add(perfUtility);
        }

        #endregion Actions - Engine Specific

        #region Actions - Update, Draw

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //update all drawn game objects in the active scene
            //sceneManager.Update(gameTime);

            //update active camera
            //cameraManager.Update(gameTime);

#if DEMO

            if (Input.Keys.WasJustPressed(Keys.B))
            {
                object[] parameters = { "boom1" };
                EventDispatcher.Raise(
                    new EventData(EventCategoryType.Player,
                    EventActionType.OnWin,
                    parameters));

                //    Application.SoundManager.Play2D("boom1");
            }

            #region Demo - Camera switching

            if (Input.Keys.IsPressed(Keys.F1))
                cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F2))
                cameraManager.SetActiveCamera(AppData.SECURITY_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F3))
                cameraManager.SetActiveCamera(AppData.CURVE_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F4))
                cameraManager.SetActiveCamera(AppData.THIRD_PERSON_CAMERA_NAME);

            #endregion Demo - Camera switching

#endif
            //fixed a bug with components not getting Update called
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //get active scene, get camera, and call the draw on the active scene
            //sceneManager.ActiveScene.Draw(gameTime, cameraManager.ActiveCamera);

            base.Draw(gameTime);
        }

        #endregion Actions - Update, Draw
    }
}