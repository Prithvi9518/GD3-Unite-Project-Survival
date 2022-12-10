#region Pre-compiler directives

//#define DEMO
//#define SHOW_DEBUG_INFO
//#define SHOW_TIMER_TEXT

#endregion

using GD.Core;
using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using GD.Engine.Inputs;
using GD.Engine.Managers;
using GD.Engine.Parameters;
using GD.Engine.Utilities;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        private BasicEffect exitSignEffect;
        private BasicEffect sideWallEffect;
        private BasicEffect floorEffect;
        private BasicEffect fuse220VEffect;
        private BasicEffect fuse440VEffect;
        private BasicEffect enemyEffect;

        private CameraManager cameraManager;
        private SceneManager<Scene> sceneManager;
        private SoundManager soundManager;
        private PhysicsManager physicsManager;
        private RenderManager renderManager;
        private EventDispatcher eventDispatcher;
        private StateManager stateManager;
        private SceneManager<Scene2D> uiManager;
        private SceneManager<Scene2D> menuManager;

        private InventoryManager inventoryManager;

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
            InitializeDebug(true);
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
            InitializeCollidableContent(worldScale);

            //add non-collidable drawn stuff
            InitializeNonCollidableContent(worldScale);

            //add UI and menu
            InitializeUI();
            //InitializeMenu();

            //Raise all the events that I want to happen at the start
            object[] parameters = { "Ambient" };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.Sound,
                EventActionType.OnPlay2D,
                parameters));

            object[] parameters2 = { "HorrorMusic" };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.Sound,
                EventActionType.OnPlay2D,
                parameters2));
        }

        private void InitializeMenu()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Controls/genericbtn");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/exitmenuwithtrans");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/menu");
            Vector2 btnScale = new Vector2(0.8f, 0.8f);

            #region Create new menu scene

            //add new main menu scene
            var mainMenuScene = new Scene2D("main menu");

            #endregion

            #region Add Background Texture

            menuGameObject = new GameObject("background");
            var scaleToWindow = _graphics.GetScaleFactorForResolution(backGroundtexture, Vector2.Zero);
            //set transform
            menuGameObject.Transform = new Transform(
                new Vector3(scaleToWindow, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(0, 0, 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(backGroundtexture, Color.White, 1);
            menuGameObject.AddComponent(new Renderer2D(material));

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Play button and text

            menuGameObject = new GameObject("play");
            menuGameObject.Transform = new Transform(
            new Vector3(btnScale, 1), //s
            new Vector3(0, 0, 0), //r
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() - new Vector2(0, 30), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.Green, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            var buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnPlay));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //material and renderer
            material = new TextMaterial2D(spriteFont, "Play", new Vector2(70, 5), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Exit button and text

            menuGameObject = new GameObject("exit");

            menuGameObject.Transform = new Transform(
                new Vector3(btnScale, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(0, 30), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.Red, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnExit));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //button material and renderer
            material = new TextMaterial2D(spriteFont, "Exit", new Vector2(70, 5), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region demo - color change button

            // menuGameObject.AddComponent(new UIColorFlipOnTimeBehaviour(Color.Red, Color.Orange, 500));

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to menu manager
            menuManager.Add(mainMenuScene.ID, mainMenuScene);

            //what menu do i see first?
            menuManager.SetActiveScene(mainMenuScene.ID);

            #endregion
        }

        private void InitializeUI()
        {
            GameObject uiGameObject = null;
            Material2D material = null;
            Texture2D texture = Content.Load<Texture2D>("Assets/Textures/Infection Meter/progress_white");

            var mainHUD = new Scene2D("game HUD");

            #region Add Infection Meter UI

            uiGameObject = new GameObject(AppData.INFECTION_METER_NAME);

            // Set width and height of the meter here
            var infectionMeterTexture = new Texture2D(Application.GraphicsDevice, 250, 30);

            // Make sure the array size is width * height
            var infectionMeterPixels = new Color[250 * 30];

            for (int i = 0; i < infectionMeterPixels.Length; i++)
            {
                // Set the colour of meter here
                infectionMeterPixels[i] = Color.Teal;
            }

            // Debug - Just to see the end of the bar
            //for (int i = infectionMeterPixels.Length - 50; i < infectionMeterPixels.Length; i++)
            //{
            //    infectionMeterPixels[i] = Color.Red;
            //}

            uiGameObject.Transform = new Transform(
                new Vector3(1, 1, 0), //s
                new Vector3(0, 0, 0), //r
                new Vector3(_graphics.PreferredBackBufferWidth - texture.Width - 100,
                20, 0)); //t

            infectionMeterTexture.SetData(infectionMeterPixels);

            material = new TextureMaterial2D(infectionMeterTexture, Color.White);
            uiGameObject.AddComponent(new Renderer2D(material));

            uiGameObject.AddComponent(new InfectionMeterController((float)AppData.MAX_GAME_TIME_IN_MSECS, 0));

            mainHUD.Add(uiGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to manager
            uiManager.Add(mainHUD.ID, mainHUD);

            //what ui do i see first?
            uiManager.SetActiveScene(mainHUD.ID);

            #endregion
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

            var MusicSound = Content.Load<SoundEffect>("Assets/Audio/Non-Diegetic/SoundTracks/HorrorSong");

            //Add the new sound for background
            soundManager.Add(new Cue(
                "HorrorMusic",
                 MusicSound,
                 SoundCategoryType.BackgroundMusic,
                 new Vector3(0.1f, 1, 0),
                 true));

            var AmbientSound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Ambient/horror-ambience-7");

            //Add the new sound for background
            soundManager.Add(new Cue(
                "Ambient",
                 AmbientSound,
                 SoundCategoryType.BackgroundMusic,
                 new Vector3(0.1f, 1, 0),
                 false));

            // Glass breaking sound

            var glassBreakingSound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Glass/glass-shatter");

            soundManager.Add(new Cue(
                "glass-shatter",
                glassBreakingSound,
                SoundCategoryType.Explosion,
                new Vector3(1, 1, 0),
                false
                ));

            // Pickup sound

            var pickupSound = Content.Load<SoundEffect>("Assets/Audio/Non-Diegetic/Pickups/422709__niamhd00145229__inspect-item");

            soundManager.Add(new Cue(
                "pickup-sound",
                pickupSound,
                SoundCategoryType.Pickup,
                new Vector3(1, 1, 0),
                false
                ));

            // Alarm sound

            var alarmSound = Content.Load<SoundEffect>(
                "Assets/Audio/Diegetic/Alarm/381957__jsilversound__security-alarm");

            soundManager.Add(new Cue(
                "alarm-sound",
                alarmSound,
                SoundCategoryType.Alarm,
                new Vector3(0.3f, 0.5f, 0),
                true));
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

            #region Standard Lit Effect

            litEffect = new BasicEffect(_graphics.GraphicsDevice);
            litEffect.TextureEnabled = true;
            litEffect.LightingEnabled = true;

            litEffect.DirectionalLight0.DiffuseColor = new Vector3(107 / 255f, 49 / 255f, 49 / 255f);
            litEffect.DirectionalLight0.Direction = new Vector3(0, 0, -1);
            litEffect.DirectionalLight0.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);

            //litEffect.DirectionalLight1.DiffuseColor = new Vector3(154 / 255f, 158 / 255f, 157 / 255f);
            litEffect.DirectionalLight1.DiffuseColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            litEffect.DirectionalLight1.Direction = new Vector3(0, -1, 0);
            litEffect.DirectionalLight1.SpecularColor = new Vector3(101 / 255f, 105 / 255f, 105 / 255f);
            litEffect.DirectionalLight1.Enabled = true;

            litEffect.DirectionalLight2.DiffuseColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            litEffect.DirectionalLight2.Direction = new Vector3(0, 1, 0);
            litEffect.DirectionalLight2.SpecularColor = new Vector3(101 / 255f, 105 / 255f, 105 / 255f);
            litEffect.DirectionalLight2.Enabled = true;

            //litEffect.AmbientLightColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            litEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            //litEffect.AmbientLightColor = new Vector3(38 / 255f, 37 / 255f, 37 / 255f);

            litEffect.FogEnabled = true;
            //litEffect.FogColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            litEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            //litEffect.FogColor = new Vector3(38 / 255f, 37 / 255f, 37 / 255f);
            litEffect.FogStart = 7f;
            litEffect.FogEnd = 30f;

            litEffect.PreferPerPixelLighting = true;

            #endregion

            #region Exit Sign Emission Effect

            exitSignEffect = new BasicEffect(_graphics.GraphicsDevice);
            exitSignEffect.TextureEnabled = true;
            exitSignEffect.LightingEnabled = true;

            exitSignEffect.EmissiveColor = new Vector3(226 / 255f, 41 / 255f, 41 / 255f);

            exitSignEffect.DirectionalLight0.DiffuseColor = new Vector3(255 / 255f, 255 / 255f, 255 / 255f);
            exitSignEffect.DirectionalLight0.Direction = new Vector3(0, 0, -1);

            exitSignEffect.AmbientLightColor = new Vector3(232 / 255f, 71 / 255f, 76 / 255f);

            #endregion

            #region Side Walls Effect

            sideWallEffect = new BasicEffect(_graphics.GraphicsDevice);
            sideWallEffect.TextureEnabled = true;
            sideWallEffect.LightingEnabled = true;

            sideWallEffect.DirectionalLight0.DiffuseColor = new Vector3(30 / 255f, 29 / 255f, 29 / 255f);
            sideWallEffect.DirectionalLight0.Direction = new Vector3(1, 0, 0);
            sideWallEffect.DirectionalLight0.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);

            sideWallEffect.DirectionalLight1.DiffuseColor = new Vector3(30 / 255f, 29 / 255f, 29 / 255f);
            sideWallEffect.DirectionalLight1.Direction = new Vector3(-1, 0, 0);
            sideWallEffect.DirectionalLight1.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);
            sideWallEffect.DirectionalLight1.Enabled = true;

            sideWallEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            sideWallEffect.FogEnabled = true;
            sideWallEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            sideWallEffect.FogStart = 0f;
            sideWallEffect.FogEnd = 25f;

            sideWallEffect.PreferPerPixelLighting = true;

            #endregion

            #region Floor Effect

            floorEffect = new BasicEffect(_graphics.GraphicsDevice);
            floorEffect.TextureEnabled = true;
            floorEffect.LightingEnabled = true;

            //floorEffect.DirectionalLight0.DiffuseColor = new Vector3(30 / 255f, 29 / 255f, 29 / 255f);
            floorEffect.DirectionalLight0.DiffuseColor = new Vector3(40 / 255f, 36 / 255f, 36 / 255f);
            floorEffect.DirectionalLight0.Direction = new Vector3(0, -1, 0);
            //floorEffect.DirectionalLight0.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);
            floorEffect.DirectionalLight0.SpecularColor = new Vector3(33 / 255f, 30 / 255f, 30 / 255f);

            floorEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            floorEffect.FogEnabled = true;
            floorEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            floorEffect.FogStart = 0f;
            floorEffect.FogEnd = 25f;

            floorEffect.PreferPerPixelLighting = true;

            #endregion

            #region Fuse 220V Effect

            fuse220VEffect = new BasicEffect(_graphics.GraphicsDevice);
            fuse220VEffect.TextureEnabled = true;
            fuse220VEffect.LightingEnabled = true;

            fuse220VEffect.DirectionalLight0.DiffuseColor = new Vector3(40 / 255f, 36 / 255f, 36 / 255f);

            Vector3 fuseDirection = Vector3.Normalize(AppData.FUSE_220V_TRANSLATION - Vector3.Zero);
            fuse220VEffect.DirectionalLight0.Direction = fuseDirection;

            fuse220VEffect.DirectionalLight0.SpecularColor = new Vector3(33 / 255f, 30 / 255f, 30 / 255f);

            fuse220VEffect.EmissiveColor = new Vector3(0 / 255f, 247 / 255f, 255 / 255f);

            fuse220VEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            fuse220VEffect.FogEnabled = true;
            fuse220VEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            fuse220VEffect.FogStart = 0f;
            fuse220VEffect.FogEnd = 25f;

            fuse220VEffect.PreferPerPixelLighting = true;

            #endregion

            #region Fuse 440V Effect

            fuse440VEffect = new BasicEffect(_graphics.GraphicsDevice);
            fuse440VEffect.TextureEnabled = true;
            fuse440VEffect.LightingEnabled = true;

            fuse440VEffect.DirectionalLight0.DiffuseColor = new Vector3(40 / 255f, 36 / 255f, 36 / 255f);

            fuseDirection = Vector3.Normalize(AppData.FUSE_440V_TRANSLATION - Vector3.Zero);
            fuse440VEffect.DirectionalLight0.Direction = fuseDirection;

            fuse440VEffect.DirectionalLight0.SpecularColor = new Vector3(33 / 255f, 30 / 255f, 30 / 255f);

            fuse440VEffect.EmissiveColor = new Vector3(252 / 255f, 92 / 255f, 0 / 255f);

            fuse440VEffect.AmbientLightColor = new Vector3(238 / 255f, 114 / 255f, 42 / 255f);

            fuse440VEffect.FogEnabled = true;
            fuse440VEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            fuse440VEffect.FogStart = 0f;
            fuse440VEffect.FogEnd = 25f;

            fuse440VEffect.PreferPerPixelLighting = true;

            #endregion

            enemyEffect = new BasicEffect(_graphics.GraphicsDevice);
            enemyEffect.TextureEnabled = true;
            enemyEffect.LightingEnabled = true;

            enemyEffect.DirectionalLight0.DiffuseColor = new Vector3(156 / 255f, 149 / 255f, 196 / 255f);
            enemyEffect.DirectionalLight0.Direction = new Vector3(0, -1, 0);
            enemyEffect.DirectionalLight0.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);

            enemyEffect.DirectionalLight1.DiffuseColor = new Vector3(82 / 255f, 59 / 255f, 228 / 255f);
            enemyEffect.DirectionalLight1.Direction = new Vector3(0, 0, -1);
            enemyEffect.DirectionalLight1.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);
            enemyEffect.DirectionalLight1.Enabled = true;

            //enemyEffect.DirectionalLight2.DiffuseColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            //enemyEffect.DirectionalLight2.Direction = new Vector3(0, 1, 0);
            //enemyEffect.DirectionalLight2.SpecularColor = new Vector3(101 / 255f, 105 / 255f, 105 / 255f);
            //enemyEffect.DirectionalLight2.Enabled = true;

            enemyEffect.EmissiveColor = new Vector3(29 / 255f, 26 / 255f, 51 / 255f);

            enemyEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            enemyEffect.FogEnabled = true;
            enemyEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);
            enemyEffect.FogStart = 10f;
            enemyEffect.FogEnd = 70f;

            enemyEffect.PreferPerPixelLighting = true;
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

            #region Collision - Add capsule

            //adding a collidable surface that enables acceleration, jumping
            var characterCollider = new CharacterCollider(cameraGameObject, true);

            cameraGameObject.AddComponent(characterCollider);
            characterCollider.AddPrimitive(new Capsule(
                cameraGameObject.Transform.Translation,
                Matrix.CreateRotationX(MathHelper.PiOver2),
                1, AppData.PLAYER_DEFAULT_CAPSULE_HEIGHT),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            characterCollider.Enable(cameraGameObject, false, 1);

            #endregion

            // First person controller component

            //cameraGameObject.AddComponent(new OurFirstPersonController(
            //    AppData.PLAYER_MOVE_SPEED, AppData.PLAYER_STRAFE_SPEED,
            //    AppData.PLAYER_ROTATE_SPEED_VECTOR2, AppData.FIRST_PERSON_CAMERA_SMOOTH_FACTOR, true));

            cameraGameObject.AddComponent(new OurCollidableFPController(cameraGameObject,
                characterCollider,
                AppData.PLAYER_MOVE_SPEED, AppData.PLAYER_STRAFE_SPEED,
                AppData.PLAYER_ROTATE_SPEED_VECTOR2, AppData.FIRST_PERSON_CAMERA_SMOOTH_FACTOR, true,
                AppData.PLAYER_COLLIDABLE_JUMP_HEIGHT));

            // Item interaction controller component
            cameraGameObject.AddComponent(new InteractionController());

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion First Person

            cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
        }

        private void InitializeCollidableContent(float worldScale)
        {
            InitializeShoppingCentre();
            InitializeCollidablePickups();
            InitializeCollidableInteractibles();
        }

        private void InitializeNonCollidableContent(float worldScale)
        {
            //create sky
            //InitializeSkyBoxAndGround(worldScale);

            InitializeEnemies();

            //InitializePickups();
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

        private void InitializeCollidableInteractibles()
        {
            #region Gate Access Machine

            var gameObject = new GameObject(AppData.GATE_ACCESS_MACHINE_NAME,
                                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero,
                new Vector3(48.75f, 3.5f, 80.3f));

            string texture_path = "Assets/Textures/Props/Generator_Room/access_card_machine_emission";

            string model_path = "Assets/Models/Generator Room/gate_access_machine_test_2";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            InteractibleCollider collider = new AccessMachineCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 230
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Fuse Box

            gameObject = new GameObject(AppData.FUSE_BOX_NAME,
                                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(57.5f, 2.2f, 42.4f));

            texture_path = "Assets/Textures/Props/Generator_Room/fuse_box_diffuse";

            model_path = "Assets/Models/Generator Room/fuse_box_test";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            collider = new FuseboxCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 230
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Exit

            var gdBasicEffect = new GDBasicEffect(litEffect);

            //var gameObject = new GameObject("exit door",
            //        ObjectType.Static, RenderType.Opaque);

            //gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            #region Exit Door Frame

            gameObject = new GameObject(AppData.EXIT_DOOR_FRAME_NAME,
                      ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(
                AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(1.83f, 0.01f, -128.2f));

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/Test/frame",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_door",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Exit Door

            gameObject = new GameObject(AppData.EXIT_DOOR_NAME,
                       ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(
                AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(1.83f, 0f, -127.9f));

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/Test/door",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_door",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            collider = new ExitDoorCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                230 * new Vector3(
                    gameObject.Transform.Scale.X * 2f,
                    gameObject.Transform.Scale.Y * 3,
                    gameObject.Transform.Scale.Z * 1.9f
                    )
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Exit Sign

            gameObject = new GameObject("exit sign",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/Exit Sign/exit_sign",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_sign",
                    new GDBasicEffect(exitSignEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Exit Sign

            #endregion Exit
        }

        private void InitializeCollidablePickups()
        {
            GDBasicEffect gdBasicEffect = new GDBasicEffect(litEffect);

            #region Office Keycard

            var gameObject = new GameObject(AppData.KEYCARD_NAME,
                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Collectible;

            gameObject.Transform = new Transform(0.04f * Vector3.One, Vector3.Zero, new Vector3(-67, 2f, -109));

            string texture_path = "Assets/Textures/Props/Office/keycard_albedo";
            string model_path = "Assets/Models/Keycard/keycard_unapplied";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            var collider = new PickupCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 230
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Fuse 220V

            gameObject = new GameObject(AppData.FUSE_220V_NAME, ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Collectible;

            gameObject.Transform = new Transform
                (AppData.DEFAULT_OBJECT_SCALE * 0.05f * Vector3.One,
                new Vector3(90, 90, 0),
                AppData.FUSE_220V_TRANSLATION);

            model_path = "Assets/Models/Fuse/fuse";
            texture_path = "Assets/Textures/Props/Fuse/Material_Base_Color";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(fuse220VEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            collider = new PickupCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 8000
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Fuse

            #region Fuse 440V

            gameObject = new GameObject(AppData.FUSE_440V_NAME, ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Collectible;

            gameObject.Transform = new Transform
                (AppData.DEFAULT_OBJECT_SCALE * 0.05f * Vector3.One,
                new Vector3(90, 0, 0),
                AppData.FUSE_440V_TRANSLATION);

            model_path = "Assets/Models/Fuse/fuse";
            texture_path = "Assets/Textures/Props/Fuse/Material_Base_Color";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(fuse440VEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            collider = new PickupCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 8000
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 5);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Fuse
        }

        //private void InitializePickups()
        //{
        //    GDBasicEffect gdBasicEffect = new GDBasicEffect(litEffect);

        //    #region Office Keycard

        //    var gameObject = new GameObject(AppData.KEYCARD_NAME,
        //        ObjectType.Static, RenderType.Opaque);
        //    gameObject.GameObjectType = GameObjectType.Collectible;

        //    gameObject.Transform = new Transform(0.04f * Vector3.One, Vector3.Zero, new Vector3(-67, 2f, -109));
        //    string texture_path = "Assets/Textures/Props/Office/keycard_albedo";

        //    string model_path = "Assets/Models/Keycard/keycard_unapplied";

        //    Renderer renderer = InitializeRenderer(
        //            model_path,
        //            texture_path,
        //            gdBasicEffect,
        //            1
        //            );

        //    gameObject.AddComponent(renderer);

        //    gameObject.AddComponent(new InteractableBehaviour());

        //    sceneManager.ActiveScene.Add(gameObject);

        //    #endregion

        //    #region Fuse

        //    gameObject = new GameObject(AppData.FUSE_NAME, ObjectType.Static, RenderType.Opaque);
        //    gameObject.GameObjectType = GameObjectType.Collectible;

        //    gameObject.Transform = new Transform
        //        (AppData.DEFAULT_OBJECT_SCALE * 0.1f * Vector3.One,
        //        new Vector3(MathHelper.PiOver2, 0, 0),
        //        new Vector3(-10, 2.75f, -67));

        //    model_path = "Assets/Models/Fuse/fuse";
        //    texture_path = "Assets/Textures/Props/Fuse/Material_Base_Color";

        //    renderer = InitializeRenderer(
        //            model_path,
        //            texture_path,
        //            new GDBasicEffect(litEffect),
        //            1
        //            );

        //    gameObject.AddComponent(renderer);
        //    gameObject.AddComponent(new InteractableBehaviour());

        //    sceneManager.ActiveScene.Add(gameObject);

        //    #endregion Fuse
        //}

        private void InitializeEnemies()
        {
            Renderer enemyRenderer = null;
            GameObject gameObject = null;
            var gdBasicEffect = new GDBasicEffect(enemyEffect);

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
            //InitializeWalls();
            Initializew();
            InitializeShoppingCentreAssets();
            InitializeAisles();
            InitializeCoffeeShop();
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

        private void Initializew()
        {
            var gameObject = new GameObject("Wall 1",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(17.5f, 5.6f, 89f));

            var model_path = "Assets/Models/Walls/wall_11";

            Renderer renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(5070f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 105f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 2",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(67.5f, 5.6f, 76.3f));

            model_path = "Assets/Models/Walls/wall_12";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 1230f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 3",
           ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(59.35f, 5.6f, 63.5f));

            model_path = "Assets/Models/Walls/wall_13";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(880f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 105f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 4",
    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(49.8f, 5.6f, 75.8f));

            model_path = "Assets/Models/Walls/wall_14";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 1230f * gameObject.Transform.Scale.Z);
            //collider = new Collider(gameObject, true);
            //collider.AddPrimitive(
            //    new Box(
            //        gameObject.Transform.Translation,
            //        gameObject.Transform.Rotation,
            //        aisleScale
            //        ),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f)
            //    );

            //collider.Enable(gameObject, true, 10);
            //gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 5",
    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(49.8f, 5.5f, -12f));

            model_path = "Assets/Models/Walls/wall_15";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(sideWallEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 7700f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 6",
    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(-10f, 5.6f, -89f));

            model_path = "Assets/Models/Walls/wall_16";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(5780f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 105f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 7",
    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(-67.5f, 5.6f, -70.9f));

            model_path = "Assets/Models/Walls/wall_17";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 1800f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 8",
ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(-49f, 5.6f, -54.9f));

            model_path = "Assets/Models/Walls/wall_18";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(1800f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 105f * gameObject.Transform.Scale.Z);

            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 9",
ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(-30.5f, 5.6f, -70.9f));

            model_path = "Assets/Models/Walls/wall_19";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 1800f * gameObject.Transform.Scale.Z);
            //collider = new Collider(gameObject, true);
            //collider.AddPrimitive(
            //    new Box(
            //        gameObject.Transform.Translation,
            //        gameObject.Transform.Rotation,
            //        aisleScale
            //        ),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f)
            //    );

            //collider.Enable(gameObject, true, 10);
            //gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 10",
ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, new Vector3(0, -0.8f, 0), new Vector3(-32.5f, 5.6f, 18f));

            model_path = "Assets/Models/Walls/wall_20";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(sideWallEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 7200f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("Wall 11",
ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(9.4f, 5.6f, 75.6f));

            model_path = "Assets/Models/Walls/wall_21";

            renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/walls",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(105f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 1250f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Wall 12", new Vector3(-16.5f, 0, 0), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);
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
                new GDBasicEffect(floorEffect),
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(6800f * gameObject.Transform.Scale.X, gameObject.Transform.Scale.Z, 9000f * gameObject.Transform.Scale.Y);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                   aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

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

            var gdBasicEffect = new GDBasicEffect(litEffect);

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

            var gdBasicEffect = new GDBasicEffect(litEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            #region Belt

            string belt_base_path = AppData.CHECKOUT_DESK_MODEL_BASE_PATH + "Belt/belt_";

            gameObject = new GameObject("belt ", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(17.2f, 1f, 47.6f));

            String model_path = belt_base_path + "2";

            renderer = InitializeRenderer(
                model_path,
                AppData.CHECKOUT_DESK_TEXTURE_BASE_PATH + "belt",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(140f * gameObject.Transform.Scale.X, 95f * gameObject.Transform.Scale.Y, 300f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(-10.5f, 0, 0), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Belt

            #region Cashier

            string cashier_path = AppData.CHECKOUT_DESK_MODEL_BASE_PATH + "Cashier/cashier_";

            gameObject = new GameObject("cashier 2", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            model_path = cashier_path + "2";

            renderer = InitializeRenderer(
                model_path,
                AppData.CHECKOUT_DESK_TEXTURE_BASE_PATH + "cash_register",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("cashier 3", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(15.6f, 2.4f, 49.5f));

            model_path = cashier_path + "3";

            renderer = InitializeRenderer(
                model_path,
                AppData.CHECKOUT_DESK_TEXTURE_BASE_PATH + "cash_register",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(140f * gameObject.Transform.Scale.X, 160f * gameObject.Transform.Scale.Y, 100f * gameObject.Transform.Scale.Z);
            collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Cashier ", new Vector3(-10.5f, 0, 0), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Cashier

            #endregion Checkout Desks
        }

        private void InitializeBenches()
        {
            #region Benches

            var gdBasicEffect = new GDBasicEffect(litEffect);
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

            var gdBasicEffect = new GDBasicEffect(litEffect);
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

            string fire_extinguishers_base_path = AppData.BIN_MODELS_PATH + "Fire Extinguishers/fire_extinguisher_1";

            gameObject = new GameObject("fire extinguisher 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(36.4f, 3f, 87.9f));

            renderer = InitializeRenderer(
                fire_extinguishers_base_path,
                AppData.BIN_TEXTURES_PATH + "Fire Extinguisher/fire_extinguisher",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(60f * gameObject.Transform.Scale.X, 130f * gameObject.Transform.Scale.Y, 80f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(2f, 0, 0), aisleScale);
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

            var gdBasicEffect = new GDBasicEffect(litEffect);
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

            string base_fridges_base_path = "Assets/Models/Shopping Centre/Shopping Cart/Trolleys/trolley_1";

            gameObject = new GameObject("trolley 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                base_fridges_base_path,
                texture_base_start + "Trolley/trolley_metal",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            var gameObjectLeft = CloneModelGameObject(gameObject, "Aisle ", new Vector3(2f, 0, 0));
            sceneManager.ActiveScene.Add(gameObjectLeft);

            for (int i = 0; i < 3; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -2.7f));
                sceneManager.ActiveScene.Add(gameObject);
            }

            for (int i = 0; i < 3; i++)
            {
                gameObjectLeft = CloneModelGameObject(gameObjectLeft, "Aisle ", new Vector3(0, 0, -2.7f));
                sceneManager.ActiveScene.Add(gameObjectLeft);
            }

            var aisleScale = new Vector3(700f * gameObject.Transform.Scale.X, 200f * gameObject.Transform.Scale.Y, 600f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.transform.SetTranslation(new Vector3(23f, 2f, 80f));
            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Trolleys

            #endregion Shopping Cart
        }

        private void InitializeScaffolding()
        {
            #region Scaffolding

            var gdBasicEffect = new GDBasicEffect(litEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string scaffolding_base_path = "Assets/Models/Shopping Centre/Scaffolding/scaffolding_1";

            gameObject = new GameObject("scaffolding ", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(45.9f, 2.5f, 42.7f));

            renderer = InitializeRenderer(
                scaffolding_base_path,
                "Assets/Textures/Shopping Centre/Scaffolding/scaffolding",
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(115f * gameObject.Transform.Scale.X, 270f * gameObject.Transform.Scale.Y, 250f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 3; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, 5.4f), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Scaffolding
        }

        private void InitializeFridges()
        {
            #region Fridges

            var gameObject = new GameObject("Fridge",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(47.5f, 2.6f, -82.4f));

            var model_path = "Assets/Models/Shopping Centre/Fridges/fridge";

            Renderer renderer = InitializeRenderer(
                    model_path,
                     "Assets/Textures/Shopping Centre/Fridge/fridge",
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(115f * gameObject.Transform.Scale.X, 270f * gameObject.Transform.Scale.Y, 150f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 4; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, 3.4f), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Fridges
        }

        private void InitializeLights()
        {
            #region Lights

            var gdBasicEffect = new GDBasicEffect(litEffect);
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

            var gameObject = new GameObject(AppData.GENERATOR_DOOR_NAME,
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(49.9f, 3.4f, 76.48f));

            Renderer renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Generator Room Door/generator_room_door",
                    "Assets/Textures/Shopping Centre/Doors/Generator Room Door/generator_room_door",
                    new GDBasicEffect(litEffect),
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

            var gdBasicEffect = new GDBasicEffect(litEffect);

            //var gameObject = new GameObject("exit door",
            //        ObjectType.Static, RenderType.Opaque);

            //gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            #region Exit Door Frame

            var gameObject = new GameObject(AppData.EXIT_DOOR_FRAME_NAME,
                      ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(
                AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(11.9f, 4.1f, -88.7f));

            Renderer renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/exit_door_frame",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_door",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Exit Door

            gameObject = new GameObject(AppData.EXIT_DOOR_NAME,
                       ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(
                AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(11.9f, 2.65f, -88.7f));

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/exit_door",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_door",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(360f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 200f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            ////TODO: need to change this to a collider as the door doesnt require interaction
            //gameObject.AddComponent(new InteractableBehaviour());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Exit Sign

            gameObject = new GameObject("exit sign",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0.3f, 0, 1));

            renderer = InitializeRenderer(
                    "Assets/Models/Shopping Centre/Doors/Exit Door/Exit Sign/exit_sign",
                    "Assets/Textures/Shopping Centre/Doors/Exit Door/exit_sign",
                    new GDBasicEffect(exitSignEffect),
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

            var gdBasicEffect = new GDBasicEffect(litEffect);

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

            var gdBasicEffect = new GDBasicEffect(litEffect);
            GameObject gameObject = null;
            Renderer renderer = null;

            string texture_path = "Assets/Textures/Shopping Centre/Water Dispenser/water_dispenser";
            string model_base_path = "Assets/Models/Coffee Shop/water dispensers/water_dispenser_1";

            gameObject = new GameObject("water dispenser 1",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-26.4f, 2.2f, 73f));

            renderer = InitializeRenderer(
                model_base_path,
                texture_path,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(100f * gameObject.Transform.Scale.X, 180f * gameObject.Transform.Scale.Y, 80f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -3f), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Water Dispensers
        }

        private void InitializeCoffeeShopStall()
        {
            #region Coffee Shop

            var gdBasicEffect = new GDBasicEffect(litEffect);
            Renderer renderer = null;

            string model_path = "Assets/Models/Coffee Shop/coffee_shop/coffee_shop_";
            string texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/";

            #region Coffee Shop Base

            var gameObject = new GameObject("coffee shop base",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-26.3f, 2.8f, 51f));

            renderer = InitializeRenderer(
                    model_path + "base",
                    texture_path + "steel",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(650f * gameObject.Transform.Scale.X, 400f * gameObject.Transform.Scale.Y, 700f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);

            gameObject.AddComponent(collider);

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

            var gdBasicEffect = new GDBasicEffect(litEffect);
            var texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/Chair/chair_wood";

            GameObject gameObject = null;
            Renderer renderer = null;

            string coffee_chairs_base_path = "Assets/Models/Coffee Shop/chairs/coffee_chair_1";

            gameObject = new GameObject("coffee chair", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-13f, 1.6f, 75.4f));

            renderer = InitializeRenderer(
                coffee_chairs_base_path,
                texture_path,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(100f * gameObject.Transform.Scale.X, 180f * gameObject.Transform.Scale.Y, 80f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, 8f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(-8.5f, 0, 0), aisleScale, new Vector3(0, 180, 0));
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -8f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeTables()
        {
            #region Tables

            var gdBasicEffect = new GDBasicEffect(litEffect);

            var model_base_path = "Assets/Models/Coffee Shop/tables/table_";
            var texture_path = "Assets/Textures/Shopping Centre/Coffee Shop/Table/table_wood";

            GameObject gameObject = null;
            Renderer renderer = null;

            gameObject = new GameObject("Table 2",
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-16.9f, 1.3f, 75.4f));

            renderer = InitializeRenderer(
            model_base_path + "2",
            texture_path,
            gdBasicEffect,
            1
            );
            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(200f * gameObject.Transform.Scale.X, 100f * gameObject.Transform.Scale.Y, 150f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, 8f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        private void InitializeAisles()
        {
            #region Aisles

            var texture_path = AppData.SHELF_TEXTURE_PATH;
            InitializeClothesAisle(texture_path);
            InitializeFrontAisles(texture_path);
            InitializeBackAisles(texture_path);
            InitializeWallAisle(texture_path);

            #endregion
        }

        private void InitializeShutter()
        {
            #region Shutter

            var gameObject = new GameObject("shutter",
                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(1.2f, 5.6f, 78.6f));
            var texture_path = "Assets/Textures/Shopping Centre/Shutter/shutter_rust";

            var model_path = "Assets/Models/Shopping Centre/Doors/Shutter/shutter_model";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(800f * gameObject.Transform.Scale.X, 550f * gameObject.Transform.Scale.Y, 105f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Shutter
        }

        private void InitializeGeneratorRoomModels()
        {
            #region Fuse Box

            var gameObject = new GameObject(AppData.FUSE_BOX_NAME,
                                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero,
                new Vector3(57.5f, 2.2f, 42.4f));

            var texture_path = "Assets/Textures/Props/Generator_Room/fuse_box_diffuse";

            var model_path = "Assets/Models/Generator Room/fuse_box_test";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            gameObject.AddComponent(new InteractableBehaviour());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Gate Access Machine

            gameObject = new GameObject(AppData.GATE_ACCESS_MACHINE_NAME,
                                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero,
                Vector3.Zero);

            texture_path = "Assets/Textures/Props/Generator_Room/access_card_machine_emission";

            model_path = "Assets/Models/Generator Room/gate_access_machine_test_2";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            gameObject.AddComponent(new InteractableBehaviour());

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
                    new GDBasicEffect(litEffect),
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
                    new GDBasicEffect(litEffect),
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
                    new GDBasicEffect(litEffect),
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

        #region Aisles

        private void InitializeWallAisle(string texture_path)
        {
            var gameObject = new GameObject("toys aisle shelf",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(47.6f, 2.6f, -20.6f));

            var model_path = "Assets/Models/Aisles/Toys/toys_aisle_shelf";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(90f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 4520f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);
        }

        private void InitializeBackAisles(string texture_path)
        {
            #region Electronics Aisle Shelf

            var gameObject = new GameObject("electronics aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(31.6f, 2.6f, -50.2f));

            var model_path = "Assets/Models/Aisles/aisle_3";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(160f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 2550f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(-16.5f, 0, 0), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Electronics Aisle Shelf
        }

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset, Vector3 colliderScale)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Renderer cloneRenderer = new Renderer(renderer.Effect, renderer.Material, renderer.Mesh);
            gameObjectClone.AddComponent(cloneRenderer);

            Collider cloneCollider = new Collider(gameObjectClone, true);

            cloneCollider.AddPrimitive(
                new Box(
                    gameObjectClone.Transform.Translation,
                    gameObjectClone.Transform.Rotation,
                    colliderScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            cloneCollider.Enable(gameObjectClone, true, 10);
            gameObjectClone.AddComponent(cloneCollider);

            return gameObjectClone;
        }

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset, Vector3 colliderScale, Vector3 rotation)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Renderer cloneRenderer = new Renderer(renderer.Effect, renderer.Material, renderer.Mesh);
            gameObjectClone.AddComponent(cloneRenderer);

            Collider cloneCollider = new Collider(gameObjectClone, true);

            cloneCollider.AddPrimitive(
                new Box(
                    gameObjectClone.Transform.Translation,
                    gameObjectClone.Transform.Rotation,
                    colliderScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            cloneCollider.Enable(gameObjectClone, true, 10);
            gameObjectClone.AddComponent(cloneCollider);

            return gameObjectClone;
        }

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Renderer cloneRenderer = new Renderer(renderer.Effect, renderer.Material, renderer.Mesh);
            gameObjectClone.AddComponent(cloneRenderer);

            return gameObjectClone;
        }

        private void InitializeFrontAisles(string texture_path)
        {
            #region Beauty Aisle Shelf

            var gameObject = new GameObject("beauty aisle shelf",
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(31.6f, 2.6f, 7.6f));

            var model_path = "Assets/Models/Aisles/aisle_2";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(160f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 2240f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(-16.5f, 0, 0), aisleScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion
        }

        private void InitializeClothesAisle(string texture_path)
        {
            #region Clothes Aisle Shelf

            var gameObject = new GameObject("clothes aisle shelf",
             ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(-13.6f, 2.6f, -28.2f));

            var model_path = "Assets/Models/Aisles/aisle_4";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            var aisleScale = new Vector3(160f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 3000f * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    aisleScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion
        }

        #endregion

        private void InitializeOfficeModels()
        {
            GDBasicEffect gdBasicEffect = new GDBasicEffect(litEffect);

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
            var gdBasicEffect = new GDBasicEffect(litEffect);
            var quadMesh = new QuadMesh(_graphics.GraphicsDevice);

            //skybox - back face
            quad = new GameObject("skybox back face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), null, new Vector3(0, 0, -halfWorldScale));
            var texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_bk");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - left face
            quad = new GameObject("skybox left face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(90), 0), new Vector3(-halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_lf");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - right face
            quad = new GameObject("skybox right face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(-90), 0), new Vector3(halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_rt");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - top face
            quad = new GameObject("skybox top face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(MathHelper.ToRadians(90), MathHelper.ToRadians(-90), 0), new Vector3(0, halfWorldScale, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_up");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - front face
            quad = new GameObject("skybox front face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(0, MathHelper.ToRadians(-180), 0), new Vector3(0, 0, halfWorldScale));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_ft");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //ground
            quad = new GameObject("ground");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), new Vector3(MathHelper.ToRadians(-90), 0, 0), new Vector3(0, 0, 0));
            //texture = Content.Load<Texture2D>("Assets/Textures/Foliage/Ground/grass1");
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/gloomy_dn");
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
            Application.PhysicsManager = physicsManager;

            Application.StateManager = stateManager;

            Application.UISceneManager = uiManager;
            //Application.MenuSceneManager = menuManager;

            Application.InventoryManager = inventoryManager;
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
            sceneManager = new SceneManager<Scene>(this);
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
            physicsManager = new PhysicsManager(this, AppData.GRAVITY);
            Components.Add(physicsManager);

            //add state manager for inventory and countdown
            stateManager = new StateManager(this, AppData.MAX_GAME_TIME_IN_MSECS);
            Components.Add(stateManager);

            #region UI

            uiManager = new SceneManager<Scene2D>(this);

            // Change StatusType to Off when adding menus!
            uiManager.StatusType = StatusType.Drawn | StatusType.Updated;

            uiManager.IsPausedOnPlay = false;
            Components.Add(uiManager);

            var uiRenderManager = new Render2DManager(this, _spriteBatch, uiManager);

            // Change StatusType to Off when adding menus!
            uiRenderManager.StatusType = StatusType.Drawn | StatusType.Updated;

            uiRenderManager.DrawOrder = 2;
            uiRenderManager.IsPausedOnPlay = false;
            Components.Add(uiRenderManager);

            #endregion

            #region Menu

            //menuManager = new SceneManager<Scene2D>(this);
            //menuManager.StatusType = StatusType.Updated;
            //menuManager.IsPausedOnPlay = true;
            //Components.Add(menuManager);

            //var menuRenderManager = new Render2DManager(this, _spriteBatch, menuManager);
            //menuRenderManager.StatusType = StatusType.Drawn;
            //menuRenderManager.DrawOrder = 3;
            //menuRenderManager.IsPausedOnPlay = true;
            //Components.Add(menuRenderManager);

            #endregion

            inventoryManager = new InventoryManager(this, StatusType.Drawn | StatusType.Updated);
            Components.Add(inventoryManager);
        }

        private void InitializeDictionaries()
        {
            //TODO - add texture dictionary, soundeffect dictionary, model dictionary
        }

        private void InitializeDebug(bool showCollisionSkins = true)
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

            var infoFunction = (Transform transform) =>
            {
                return transform.Translation.GetNewRounded(1).ToString();
            };

            perfUtility.infoList.Add(new TransformInfo(_spriteBatch, spriteFont, "Pos:", Color.White, contentScale * Vector2.One,
                ref Application.CameraManager.ActiveCamera.transform, infoFunction));

            infoFunction = (Transform transform) =>
            {
                return transform.Rotation.GetNewRounded(1).ToString();
            };

            perfUtility.infoList.Add(new TransformInfo(_spriteBatch, spriteFont, "Rot:", Color.White, contentScale * Vector2.One,
                ref Application.CameraManager.ActiveCamera.transform, infoFunction));

            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Object -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new ObjectInfo(_spriteBatch, spriteFont, "Objects:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Hints -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Use mouse scroll wheel to change security camera FOV, F1-F4 for camera switch", Color.White, contentScale * Vector2.One));

            //add to the component list otherwise it wont have its Update or Draw called!
            // perfUtility.StatusType = StatusType.Drawn | StatusType.Updated;
            Components.Add(perfUtility);

            if (showCollisionSkins)
                Components.Add(new PhysicsDebugDrawer(this));
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
            GraphicsDevice.Clear(Color.Black);

            //get active scene, get camera, and call the draw on the active scene
            //sceneManager.ActiveScene.Draw(gameTime, cameraManager.ActiveCamera);

            base.Draw(gameTime);
        }

        #endregion Actions - Update, Draw
    }
}