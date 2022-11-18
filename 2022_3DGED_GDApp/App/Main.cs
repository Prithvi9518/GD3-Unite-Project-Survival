﻿#region Pre-compiler directives

#define DEMO
#define SHOW_DEBUG_INFO

#endregion

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
        private RenderManager renderManager;
        private EventDispatcher eventDispatcher;
        private GameObject playerGameObject;

#if DEMO

        private event EventHandler OnChanged;

#endif

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

#if DEMO

        private void DemoCode()
        {
            //shows how we can create an event, register for it, and raise it in Main::Update() on Keys.E press
            DemoEvent();
        }

        private void DemoEvent()
        {
            OnChanged += HandleOnChanged;
        }

        private void HandleOnChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{e} was sent by {sender}");
        }

#endif

        protected override void Initialize()
        {
#if DEMO
            DemoCode();
#endif

            //moved spritebatch initialization here because we need it in InitializeDebug() below
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //core engine - common across any game
            InitializeEngine(AppData.APP_RESOLUTION, true, true);

            //game specific content
            InitializeLevel("My Amazing Game", AppData.SKYBOX_WORLD_SCALE);

#if SHOW_DEBUG_INFO
            InitializeDebug();
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

            //add drawn stuff
            InitializeDrawnContent(worldScale);

            //add the player
            //InitializePlayer();
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
            var scene = new Scene("labyrinth");

            //add scene to the scene manager
            sceneManager.Add(scene.ID, scene);

            //don't forget to set active scene
            sceneManager.SetActiveScene("labyrinth");
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

            #region Third Person

            cameraGameObject = new GameObject(AppData.THIRD_PERSON_CAMERA_NAME);
            cameraGameObject.Transform = new Transform(null, null, null);
            cameraGameObject.AddComponent(new Camera(
                AppData.FIRST_PERSON_HALF_FOV, //MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                AppData.FIRST_PERSON_CAMERA_NCP, //0.1f,
                AppData.FIRST_PERSON_CAMERA_FCP,
                new Viewport(0, 0, _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight))); // 3000

            cameraGameObject.AddComponent(new ThirdPersonController());

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion

            #region First Person

            //camera 1
            cameraGameObject = new GameObject(AppData.FIRST_PERSON_CAMERA_NAME);
            cameraGameObject.Transform = new Transform(null, null, AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION);
            cameraGameObject.AddComponent(
                new Camera(
                AppData.FIRST_PERSON_HALF_FOV, //MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                AppData.FIRST_PERSON_CAMERA_NCP, //0.1f,
                AppData.FIRST_PERSON_CAMERA_FCP,
                new Viewport(0, 0, _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight))); // 3000

            //OLD
            //cameraGameObject.AddComponent(new FirstPersonCameraController(AppData.FIRST_PERSON_MOVE_SPEED, AppData.FIRST_PERSON_STRAFE_SPEED));

            //NEW
            cameraGameObject.AddComponent(new FirstPersonController(AppData.FIRST_PERSON_MOVE_SPEED, AppData.FIRST_PERSON_STRAFE_SPEED,
                AppData.PLAYER_ROTATE_SPEED_VECTOR2, true));
            cameraGameObject.AddComponent(new InteractionController());

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion First Person

            #region Security

            //camera 2
            cameraGameObject = new GameObject(AppData.SECURITY_CAMERA_NAME);

            cameraGameObject.Transform
                = new Transform(null,
                null,
                new Vector3(0, 2, 25));

            //add camera (view, projection)
            cameraGameObject.AddComponent(new Camera(
                MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                0.1f, 3500,
                new Viewport(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)));

            //add rotation
            cameraGameObject.AddComponent(new CycledRotationBehaviour(
                AppData.SECURITY_CAMERA_ROTATION_AXIS,
                AppData.SECURITY_CAMERA_MAX_ANGLE,
                AppData.SECURITY_CAMERA_ANGULAR_SPEED_MUL,
                TurnDirectionType.Right));

            //adds FOV change on mouse scroll
            cameraGameObject.AddComponent(new CameraFOVController(AppData.CAMERA_FOV_INCREMENT_LOW));

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion Security

            #region Curve

            Curve3D curve3D = new Curve3D(CurveLoopType.Oscillate);
            curve3D.Add(new Vector3(0, 2, 5), 0);
            curve3D.Add(new Vector3(0, 5, 10), 1000);
            curve3D.Add(new Vector3(0, 8, 25), 2500);
            curve3D.Add(new Vector3(0, 5, 35), 4000);

            cameraGameObject = new GameObject(AppData.CURVE_CAMERA_NAME);
            cameraGameObject.Transform =
                new Transform(null, null, null);
            cameraGameObject.AddComponent(new Camera(
                MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                0.1f, 3500,
                  new Viewport(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)));

            cameraGameObject.AddComponent(
                new CurveBehaviour(curve3D));

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion Curve

            cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
        }

        private void InitializeDrawnContent(float worldScale)
        {
            //create sky
            //InitializeSkyBoxAndGround(worldScale);

            InitializeShoppingCentre();

            // testing interactable code
            //TestingInteractableItem();
        }

        private void TestingInteractableItem()
        {
            var gameObject = new GameObject("interactable", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(0.7f * Vector3.One, null, new Vector3(0, 2, 1));
            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/sphere");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1),
                mesh));
            gameObject.AddComponent(new InteractableBehaviour());

            sceneManager.ActiveScene.Add(gameObject);
        }

        private void InitializeShoppingCentre()
        {
            InitializeOfficeModels();
            InitializeGeneratorRoomModels();

            #region Floor

            var gameObject = new GameObject("floor",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Floor/floor");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            InitializeWalls();

            InitializeClothesAisle();
            InitializeBeautyAisle();
            InitializeBeveragesAisle();
            InitializeElectronicsAisle();
            InitializePreparedFoodsAisle();
            InitializeProducedFoodsAisle();
            InitializeToysAisle();
        }

        private void InitializeGeneratorRoomModels()
        {
            #region Fuse Box

            var gameObject = new GameObject("fuse box",
                                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Generator Room/fuse_box");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Gate Access Machine

            gameObject = new GameObject("gate access machine",
                                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Generator Room/gate_access_machine");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Generator

            gameObject = new GameObject("generator",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Generator Room/generator");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Lever

            gameObject = new GameObject("lever",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Generator Room/lever");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Panel

            gameObject = new GameObject("panel",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Generator Room/panel");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Electrical Box

            gameObject = new GameObject("electrical box",
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Generator Room/electrical_box");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeWalls()
        {
            #region Shopping Centre Walls (Except Generator Room)

            var gameObject = new GameObject("shopping centre walls",
                            ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Walls/shopping_centre_walls");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Generator Room Walls

            gameObject = new GameObject("generator room walls",
                            ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Walls/generator_room_walls");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeToysAisle()
        {
            #region Toys Aisle Shelf

            var gameObject = new GameObject("toys aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Toys/toys_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeProducedFoodsAisle()
        {
            #region Produced Foods Aisle Shelf

            var gameObject = new GameObject("produced foods aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Produced Foods/produced_foods_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializePreparedFoodsAisle()
        {
            #region Prepared Foods Aisle Shelf

            var gameObject = new GameObject("prepared foods aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Prepared Foods/prepared_foods_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeElectronicsAisle()
        {
            #region Electronics Aisle Shelf

            var gameObject = new GameObject("electronics aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Electronics/electronics_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeBeveragesAisle()
        {
            #region Beverages Aisle Shelf

            var gameObject = new GameObject("beverages aisle shelf",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Beverages/beverages_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeBeautyAisle()
        {
            #region Beauty Aisle Shelf

            var gameObject = new GameObject("beauty aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Beauty/beauty_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeClothesAisle()
        {
            #region Clothes Aisle Shelf

            var gameObject = new GameObject("clothes aisle shelf",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Aisles/Clothes/clothes_aisle_shelf");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeOfficeModels()
        {
            #region Office Chair

            var gameObject = new GameObject("office chair",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One,
                new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            var model = Content.Load<Model>("Assets/Models/Office/office_chair");

            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Table

            gameObject = new GameObject("office table",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One,
                new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_table");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Shelves

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");
            string model_base_path = "Assets/Models/Office/Shelves/office_shelf_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("office shelf " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = model_base_path + i;
                model = Content.Load<Model>(model_path);

                mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
                gameObject.AddComponent(new Renderer(
                    new GDBasicEffect(litEffect),
                    new Material(texture, 1f, Color.White),
                    mesh));

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion

            #region Office Rug

            gameObject = new GameObject("office rug",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_rug");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office PC

            gameObject = new GameObject("office pc",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_pc");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Screen

            gameObject = new GameObject("office screen",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_screen");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Keyboard

            gameObject = new GameObject("office keyboard",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_keyboard");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Keycard

            gameObject = new GameObject("office keycard",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");

            model = Content.Load<Model>("Assets/Models/Office/office_keycard");

            mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
            gameObject.AddComponent(new Renderer(
                new GDBasicEffect(litEffect),
                new Material(texture, 1f, Color.White),
                mesh));

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Drawers

            texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");
            model_base_path = "Assets/Models/Office/Drawers/office_drawer_";

            for (int i = 1; i <= 4; i++)
            {
                gameObject = new GameObject("office drawer " + i, ObjectType.Static, RenderType.Opaque);
                gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

                string model_path = model_base_path + i;
                model = Content.Load<Model>(model_path);

                mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);
                gameObject.AddComponent(new Renderer(
                    new GDBasicEffect(litEffect),
                    new Material(texture, 1f, Color.White),
                    mesh));

                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion
        }

        private void InitializePlayer()
        {
            playerGameObject = new GameObject("player 1", ObjectType.Static, RenderType.Opaque);

            playerGameObject.Transform = new Transform(new Vector3(0.4f, 0.4f, 1),
                null, new Vector3(0, 0.2f, -2));
            var texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate2");
            var model = Content.Load<Model>("Assets/Models/sphere");
            var mesh = new Engine.ModelMesh(_graphics.GraphicsDevice, model);

            playerGameObject.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1),
                mesh));

            playerGameObject.AddComponent(new PlayerController(AppData.FIRST_PERSON_MOVE_SPEED, AppData.FIRST_PERSON_STRAFE_SPEED,
                AppData.PLAYER_ROTATE_SPEED_VECTOR2, true));

            sceneManager.ActiveScene.Add(playerGameObject);

            //set this as active player
            Application.Player = playerGameObject;
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
                Application.SoundManager.Play2D("boom1");

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

            #region Demo - Gamepad

            var thumbsL = Input.Gamepad.ThumbSticks(false);
            //   System.Diagnostics.Debug.WriteLine(thumbsL);

            var thumbsR = Input.Gamepad.ThumbSticks(false);
            //     System.Diagnostics.Debug.WriteLine(thumbsR);

            //    System.Diagnostics.Debug.WriteLine($"A: {Input.Gamepad.IsPressed(Buttons.A)}");

            #endregion Demo - Gamepad

            #region Demo - Raising events using GDEvent

            if (Input.Keys.WasJustPressed(Keys.E))
                OnChanged.Invoke(this, null); //passing null for EventArgs but we'll make our own class MyEventArgs::EventArgs later

            #endregion

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