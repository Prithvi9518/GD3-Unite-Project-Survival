#region Pre-compiler directives

//#define DEMO
#define SHOW_DEBUG_INFO
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
using System.Text;
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
        private BasicEffect labelEffect;

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

        private void DemoStateManagerEvent()
        {
            EventDispatcher.Subscribe(EventCategoryType.GameObject, HandleEvent);
        }

        private void HandleEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnWin:
                    System.Diagnostics.Debug.WriteLine(eventData.Parameters[0] as string);
                    break;

                case EventActionType.OnLose:
                    System.Diagnostics.Debug.WriteLine(eventData.Parameters[2] as string);
                    break;

                default:
                    break;
            }
        }

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

            //add scene manager and starting scenes
            InitializeScenes();

            //add collidable drawn stuff
            InitializeCollidableContent(worldScale);

            //add non-collidable drawn stuff
            InitializeNonCollidableContent(worldScale);

            //add UI and menu
            InitializeUI();
            InitializeMainMenu();
            //InitializeControlsMenu();

            #region Start Events - Menu etc

            //start the game paused
            EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnPause));
            EventDispatcher.Subscribe(EventCategoryType.Menu, HandleEnterControlsMenu);
            EventDispatcher.Subscribe(EventCategoryType.Menu, HandleExitControlsMenu);

            #endregion

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

        private void HandleEnterControlsMenu(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnEnterControlsMenu)
            {
                InitializeControlsMenu();
            }
        }

        private void HandleExitControlsMenu(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnExitControlsMenu)
            {
                InitializeMainMenu();
            }
        }

        private void InitializeMainMenu()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Buttons/btn_256x64");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/main_menu_bg_1280x720");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/INFECTED");
            Vector2 btnScale = Vector2.One;

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
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-450, -90), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
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
            material = new TextMaterial2D(spriteFont, "Start", new Vector2(60, 5), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Settings button and text

            menuGameObject = new GameObject("controls");

            menuGameObject.Transform = new Transform(
                new Vector3(btnScale, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-450, 0), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnEnterControlsMenu));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //button material and renderer
            material = new TextMaterial2D(spriteFont, "Controls", new Vector2(30, 5), Color.White, 0.8f);
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

            #region Add Exit button and text

            menuGameObject = new GameObject("exit");

            menuGameObject.Transform = new Transform(
                new Vector3(btnScale, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-450, 90), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
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

        private void InitializeControlsMenu()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Buttons/btn_256x64");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/controls-menu");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/INFECTED");
            Vector2 btnScale = Vector2.One;

            #region Create new menu scene

            //add new main menu scene
            var controlsMenuScene = new Scene2D("controls menu");

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
            controlsMenuScene.Add(menuGameObject);

            #endregion

            #region Add Play button and text

            menuGameObject = new GameObject("play");
            menuGameObject.Transform = new Transform(
            new Vector3(btnScale, 1), //s
            new Vector3(0, 0, 0), //r
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-0, 300), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            var buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnExitControlsMenu));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //material and renderer
            material = new TextMaterial2D(spriteFont, "Back", new Vector2(70, 5), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            controlsMenuScene.Add(menuGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to menu manager
            menuManager.Add(controlsMenuScene.ID, controlsMenuScene);

            //what menu do i see first?
            menuManager.SetActiveScene(controlsMenuScene.ID);

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

            #region Button Prompts UI

            SpriteFont spriteFont = Content.Load<SpriteFont>(AppData.PERF_FONT_PATH);
            Vector2 textScale = new Vector2(1.5f, 1.5f);

            uiGameObject = new GameObject(AppData.INTERACT_PROMPT_NAME);
            uiGameObject.Transform = new Transform(
                new Vector3(textScale, 1),
                Vector3.Zero,
                new Vector3(Application.Screen.ScreenCentre - textScale + new Vector2(0, 30), 0)
                );

            material = new TextMaterial2D(spriteFont, "", new Vector2(70, 5), Color.LightGreen, 0.8f);
            //add renderer to draw the text
            uiGameObject.AddComponent(new Renderer2D(material));

            uiGameObject.AddComponent(new TextPromptController());

            mainHUD.Add(uiGameObject);

            #endregion

            #region Dialogue Subtitles

            textScale = new Vector2(1.5f, 1.5f);

            uiGameObject = new GameObject(AppData.SUBTITLES_NAME);
            uiGameObject.Transform = new Transform(
                new Vector3(textScale, 1),
                Vector3.Zero,
                new Vector3(Application.Screen.ScreenCentre - textScale + new Vector2(-400, 250), 0)
                );

            material = new TextMaterial2D(spriteFont, new StringBuilder(""),
                new Vector2(0, 0),
                Color.LightGreen,
                0.8f
                );

            uiGameObject.AddComponent(new Renderer2D(material));

            uiGameObject.AddComponent(new DialogueController());

            mainHUD.Add(uiGameObject);

            #endregion

            #region Office Note Pop-Up

            uiGameObject = new GameObject("office note ui");

            Vector2 noteScale = new Vector2(0.6f, 1f);

            uiGameObject.Transform = new Transform(
                new Vector3(noteScale, 1),
                Vector3.Zero,
                new Vector3(Application.Screen.ScreenCentre - noteScale + new Vector2(-300, -150), 0)
                );

            texture = Content.Load<Texture2D>(AppData.OFFICE_NOTE_TEXTURE_PATH);
            material = new TextureMaterial2D(texture, Color.White);

            uiGameObject.AddComponent(new Renderer2D(material));

            uiGameObject.AddComponent(new NoteUIController());

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
            #region Demo Sound

            //var soundEffect =
            //    Content.Load<SoundEffect>("Assets/Audio/Diegetic/explode1");

            ////add the new sound effect
            //soundManager.Add(new Cue(
            //    "boom1",
            //    soundEffect,
            //    SoundCategoryType.Alarm,
            //    new Vector3(1, 1, 0),
            //    false));

            #endregion

            #region Old music and ambience

            //var MusicSound = Content.Load<SoundEffect>("Assets/Audio/Non-Diegetic/SoundTracks/HorrorSong");

            ////Add the new sound for background
            //soundManager.Add(new Cue(
            //    "HorrorMusic",
            //     MusicSound,
            //     SoundCategoryType.BackgroundMusic,
            //     new Vector3(0.1f, 1, 0),
            //     true));

            //var AmbientSound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Ambient/horror-ambience-7");

            ////Add the new sound for background
            //soundManager.Add(new Cue(
            //    "Ambient",
            //     AmbientSound,
            //     SoundCategoryType.BackgroundMusic,
            //     new Vector3(0.1f, 1, 0),
            //     false));

            #endregion

            var sound = Content.Load<SoundEffect>("Assets/Audio/Non-Diegetic/SoundTracks/Soundtrack");

            //Add the new sound for background
            soundManager.Add(new Cue(
                "HorrorMusic",
                 sound,
                 SoundCategoryType.BackgroundMusic,
                 new Vector3(0.1f, 0, 0),
                 true));

            // Glass breaking sound

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Glass/glass_smash_3");

            soundManager.Add(new Cue(
                AppData.GLASS_SHATTER_SOUND_NAME,
                sound,
                SoundCategoryType.Explosion,
                new Vector3(0.5f, 0, 0),
                false
                ));

            #region Radio Sound

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Radio/516618__arialfa07__radio-1");

            soundManager.Add(new Cue(
                AppData.RADIO_SOUND_NAME,
                sound,
                SoundCategoryType.Explosion,
                new Vector3(0.5f, 0, 0),
                false
                ));

            #endregion

            #region Pickup Sound

            sound = Content.Load<SoundEffect>("Assets/Audio/Non-Diegetic/Pickups/422709__niamhd00145229__inspect-item");

            soundManager.Add(new Cue(
                "pickup-sound",
                sound,
                SoundCategoryType.Pickup,
                new Vector3(0.2f, 0, 0),
                false
                ));

            #endregion

            #region Alarm Sound

            sound = Content.Load<SoundEffect>(
                    "Assets/Audio/Diegetic/Alarm/381957__jsilversound__security-alarm");

            soundManager.Add(new Cue(
                "alarm-sound",
                sound,
                SoundCategoryType.Alarm,
                new Vector3(0.08f, 0f, 0),
                true));

            #endregion

            #region Generator Sound

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Generator/Generator");

            soundManager.Add(new Cue(
                AppData.GENERATOR_SOUND_NAME,
                sound,
                SoundCategoryType.Generator,
                new Vector3(1f, 0, 0),
                true
                ));

            #endregion

            #region Enemy Sounds

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Enemies/false_notes");

            soundManager.Add(new Cue(
                AppData.ENEMY_SOUND_1_NAME,
                sound,
                SoundCategoryType.Enemy,
                new Vector3(0.5f, 0, 0),
                false
                ));

            #endregion

            #region Player Dialogue

            #region Intro

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Intro/intro_dialogue");
            soundManager.Add(new Cue(
                AppData.INTRO_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/General Dialogue/riverside_monologue");
            soundManager.Add(new Cue(
                AppData.RIVERSIDE_MONOLOGUE_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            #endregion

            #region Generator Room Dialogue

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Generator Room/off_course_the_generator_is_not_working");
            soundManager.Add(new Cue(
                AppData.GENERATOR_NOT_WORKING_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Generator Room/fuse_somewhere_supermarket");
            soundManager.Add(new Cue(
                AppData.FUSE_SOMEWHERE_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            #endregion

            #region Electronic Aisle

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Electronic Aisle/find_a_fuse");
            soundManager.Add(new Cue(
                AppData.WHERE_FIND_FUSE_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Generator Room/caution_voltage");
            soundManager.Add(new Cue(
                AppData.PICK_RIGHT_FUSE_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(1f, 0, 0),
                false
                ));

            #endregion

            #region Office Room

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Office Room/hollow_in_the_way");
            soundManager.Add(new Cue(
                AppData.HOLLOW_IN_THE_WAY_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Office Room/generator_busted");
            soundManager.Add(new Cue(
                AppData.GENERATOR_BUSTED_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            #endregion

            #region Timer

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Timer/find_a_way_out");
            soundManager.Add(new Cue(
                AppData.FIND_A_WAY_OUT_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Timer/time_to_get_out_of_here");
            soundManager.Add(new Cue(
                AppData.TIME_TO_GET_OUT_OF_HERE_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            sound = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Player/Dialogue/Timer/cannot_be_the_end_of_me");
            soundManager.Add(new Cue(
                AppData.CANT_BE_THE_END_OF_ME_DIALOGUE,
                sound,
                SoundCategoryType.Dialogue,
                new Vector3(0.8f, 0, 0),
                false
                ));

            #endregion

            #endregion
        }

        private void LoadTextures()
        {
            //load and add to dictionary
        }

        private void LoadModels()
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
            litEffect.FogEnd = 49.5f;

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

            #region Enemy Effect

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

            #endregion

            #region Label Effect

            labelEffect = new BasicEffect(_graphics.GraphicsDevice);
            labelEffect.TextureEnabled = true;
            labelEffect.LightingEnabled = true;

            labelEffect.DirectionalLight0.DiffuseColor = new Vector3(107 / 255f, 49 / 255f, 49 / 255f);
            labelEffect.DirectionalLight0.Direction = new Vector3(0, 0, -1);
            labelEffect.DirectionalLight0.SpecularColor = new Vector3(229 / 255f, 142 / 255f, 142 / 255f);

            labelEffect.DirectionalLight1.DiffuseColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            labelEffect.DirectionalLight1.Direction = new Vector3(0, -1, 0);
            labelEffect.DirectionalLight1.SpecularColor = new Vector3(101 / 255f, 105 / 255f, 105 / 255f);
            labelEffect.DirectionalLight1.Enabled = true;

            labelEffect.DirectionalLight2.DiffuseColor = new Vector3(10 / 255f, 10 / 255f, 9 / 255f);
            labelEffect.DirectionalLight2.Direction = new Vector3(0, 1, 0);
            labelEffect.DirectionalLight2.SpecularColor = new Vector3(101 / 255f, 105 / 255f, 105 / 255f);
            labelEffect.DirectionalLight2.Enabled = true;

            labelEffect.AmbientLightColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            labelEffect.FogEnabled = true;

            labelEffect.FogColor = new Vector3(27 / 255f, 26 / 255f, 26 / 255f);

            labelEffect.FogStart = 30f;
            labelEffect.FogEnd = 60f;

            labelEffect.PreferPerPixelLighting = true;

            #endregion
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

            #region Non-Collidable First Person Controller

            //cameraGameObject.AddComponent(new OurFirstPersonController(
            //    AppData.OLD_PLAYER_MOVE_SPEED, AppData.OLD_PLAYER_STRAFE_SPEED,
            //    AppData.PLAYER_ROTATE_SPEED_VECTOR2, AppData.FIRST_PERSON_CAMERA_SMOOTH_FACTOR, true));

            #endregion

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

            #region Collidable First Person Controller

            cameraGameObject.AddComponent(new OurCollidableFPController(cameraGameObject,
                    characterCollider,
                    AppData.PLAYER_MOVE_SPEED, AppData.PLAYER_STRAFE_SPEED,
                    AppData.PLAYER_ROTATE_SPEED_VECTOR2, AppData.FIRST_PERSON_CAMERA_SMOOTH_FACTOR, true,
                    AppData.PLAYER_COLLIDABLE_JUMP_HEIGHT));

            #endregion

            // Item interaction controller component
            cameraGameObject.AddComponent(new InteractionController());

            #region 3D Sound

            //added ability for camera to listen to 3D sounds
            cameraGameObject.AddComponent(new AudioListenerBehaviour());

            #endregion

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion First Person

            cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
        }

        private void InitializeCollidableContent(float worldScale)
        {
            InitializeShoppingCentre();
            InitializeCollidablePickups();
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

        private void InitializeCollidablePickups()
        {
            GDBasicEffect gdBasicEffect = new GDBasicEffect(litEffect);

            #region Office Keycard

            var gameObject = new GameObject(AppData.KEYCARD_NAME,
                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Collectible;

            gameObject.Transform = new Transform(0.04f * Vector3.One, Vector3.Zero, new Vector3(-57.4f, 2f, -70.2f));

            string texture_path = "Assets/Textures/Props/Office/keycard_albedo";
            string model_path = "Assets/Models/Keycard/keycard_unapplied";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var collider = new PickupCollider(gameObject, true, true);
            collider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                77 * gameObject.Transform.Scale
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
            InitializeWalls();
            InitializeShoppingCentreAssets();
            InitializeAisles();
            InitializeCoffeeShop();
            InitializeGreens();
        }

        //private void InitializeWalls()
        //{
        //    #region Shopping Centre Walls

        //    var gdBasicEffect = new GDBasicEffect(litEffect);
        //    var texture_path = "Assets/Textures/walls";
        //    GameObject gameObject = null;
        //    Renderer renderer = null;

        //    #region Main Walls

        //    string main_wall_base_path = "Assets/Models/Walls/wall_";

        //    for (int i = 1; i <= 8; i++)
        //    {
        //        gameObject = new GameObject("wall " + i, ObjectType.Static, RenderType.Opaque);
        //        gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

        //        string model_path = main_wall_base_path + i;

        //        renderer = InitializeRenderer(
        //            model_path,
        //            texture_path,
        //            gdBasicEffect,
        //            1
        //            );

        //        gameObject.AddComponent(renderer);

        //        sceneManager.ActiveScene.Add(gameObject);
        //    }

        //    #endregion

        //    #region Shutter Walls

        //    string shutter_wall_base_path = "Assets/Models/Walls/Shutter Walls/shutter_wall_";

        //    for (int i = 1; i <= 2; i++)
        //    {
        //        gameObject = new GameObject("shutter wall " + i, ObjectType.Static, RenderType.Opaque);
        //        gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

        //        string model_path = shutter_wall_base_path + i;

        //        renderer = InitializeRenderer(
        //            model_path,
        //            texture_path,
        //            gdBasicEffect,
        //            1
        //            );

        //        gameObject.AddComponent(renderer);

        //        sceneManager.ActiveScene.Add(gameObject);
        //    }

        //    #endregion

        //    #region Door Walls

        //    string doors_wall_base_path = "Assets/Models/Walls/Door Walls/door_wall_";

        //    for (int i = 1; i <= 2; i++)
        //    {
        //        gameObject = new GameObject("door wall " + i, ObjectType.Static, RenderType.Opaque);
        //        gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

        //        string model_path = doors_wall_base_path + i;

        //        renderer = InitializeRenderer(
        //            model_path,
        //            texture_path,
        //            gdBasicEffect,
        //            1
        //            );

        //        gameObject.AddComponent(renderer);

        //        sceneManager.ActiveScene.Add(gameObject);
        //    }

        //    #endregion Door Walls

        //    #endregion
        //}

        private void InitializeWalls()
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

            var gameObject = new GameObject(AppData.FLOOR_NAME,
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

            var gdBasicEffect = new GDBasicEffect(unlitEffect);

            #region Office Note

            var gameObject = new GameObject("office room note", ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-47.4f, 1.9f, -64.7f));

            Renderer renderer = InitializeRenderer(
                AppData.OFFICE_NOTE_MODEL_PATH,
                AppData.OFFICE_NOTE_TEXTURE_PATH,
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            Collider collider = new NoteCollider(gameObject, true, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    200 * gameObject.Transform.Scale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

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

            string benches_bottom_base_path = "Assets/Models/Shopping Centre/Benches/Bench Bases/bench_base_1";

            gameObject = new GameObject("bench base 3", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                benches_bottom_base_path,
                "Assets/Textures/Shopping Centre/Benches/legs",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "bench_base_2", new Vector3(0, 0, -8.2f));
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "bench_base_3", new Vector3(0, 0, -5.8f));
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "bench_base_4", new Vector3(0, 0, -8.2f));
            sceneManager.ActiveScene.Add(gameObject);

            #endregion Bench Bases

            #region Bench Tops

            string benches_top_base_path = "Assets/Models/Shopping Centre/Benches/Bench Tops/bench_top_";

            gameObject = new GameObject("bench top 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            string model_path = benches_top_base_path + "1";

            renderer = InitializeRenderer(
                model_path,
                "Assets/Textures/Shopping Centre/Benches/wood_top",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("bench top 2", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-25.5f, 1.6f, 32.1f));

            model_path = benches_top_base_path + "2";

            renderer = InitializeRenderer(
                model_path,
                "Assets/Textures/Shopping Centre/Benches/wood_top",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(115f * gameObject.Transform.Scale.X, 40f * gameObject.Transform.Scale.Y, 505f * gameObject.Transform.Scale.Z);
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

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -14f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

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
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(19.1f, 1.3f, 84.7f));

            renderer = InitializeRenderer(
                   AppData.BIN_MODELS_PATH + "Pallet/pallet",
                   AppData.BIN_TEXTURES_PATH + "Pallet/pallet_wood",
                   gdBasicEffect,
                   1,
                   Color.White
                   );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(350f * gameObject.Transform.Scale.X, 200f * gameObject.Transform.Scale.Y, 840f * gameObject.Transform.Scale.Z);
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

            #endregion Pallet

            #region Plastic Bottles

            int plasticBottleNumber = 1;
            string plastic_bottles_base_path = AppData.BIN_MODELS_PATH + "Plastic Bottles/plastic_bottle";

            gameObject = new GameObject("plastic bottle " + plasticBottleNumber, ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            renderer = InitializeRenderer(
                plastic_bottles_base_path,
                AppData.BIN_TEXTURES_PATH + "Plastic Bottle/plastic_bottle",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            plasticBottleNumber++;
            Random random = new Random();
            for (int i = 0; i < 40; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "plastic bottle " + plasticBottleNumber, new Vector3(random.Next(-31, 47), 0.4f, random.Next(-87, 87)));
                sceneManager.ActiveScene.Add(gameObject);
                plasticBottleNumber++;
            }

            #endregion Plastic Bottles

            #region Trash Pile

            int trashPileNumber = 1;

            gameObject = new GameObject("trash pile " + trashPileNumber,
                  ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, new Vector3(90, 0, 0), new Vector3(43.6f, 2.23f, -86.3f));
            string texture_path = "Assets/Textures/Shopping Centre/Bins/trash_pile";

            string trash_pile_model_path = "Assets/Models/Shopping Centre/Bins/trash_pile";

            renderer = InitializeRenderer(
                    trash_pile_model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            trashPileNumber++;

            gameObject = CloneModelGameObjectRandom(gameObject, "trash pile " + trashPileNumber, new Vector3(-22.5f, 0.8f, -82.4f));
            sceneManager.ActiveScene.Add(gameObject);
            trashPileNumber++;

            gameObject.Transform.SetRotation(0, 180, 0);

            gameObject = CloneModelGameObject(gameObject, "trash pile " + trashPileNumber, new Vector3(-2f, 0, -0.5f));
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObjectRandom(gameObject, "trash pile " + trashPileNumber, new Vector3(-18.6f, 0.8f, 41.1f));
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObjectRandom(gameObject, "trash pile " + trashPileNumber, new Vector3(38.9f, 0.8f, 43.3f));
            sceneManager.ActiveScene.Add(gameObject);
            trashPileNumber++;

            gameObject.Transform.SetRotation(0, 0, 0);

            #endregion Trash Pile

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
            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(25f, 1.2f, 86.5f));

            renderer = InitializeRenderer(
                base_fridges_base_path,
                texture_base_start + "Trolley/trolley_metal",
                gdBasicEffect,
                1,
                Color.White
                );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(450f * gameObject.Transform.Scale.X, 200f * gameObject.Transform.Scale.Y, 1300f * gameObject.Transform.Scale.Z);
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

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Trolleys

            #endregion Shopping Cart
        }

        private void InitializeScaffolding()
        {
            #region Scaffolding

            var gdBasicEffect = new GDBasicEffect(litEffect);

            GameObject gameObject = new GameObject("scaffolding 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, AppData.SCAFFOLDING_POSITION);

            Renderer renderer = InitializeRenderer(
                AppData.SCAFFOLDING_MDOEL_PATH,
                AppData.SCAFFOLDING_TEXTURE_PATH,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            Vector3 colliderScale = new Vector3(AppData.SCAFFOLDING_COLLIDER_SCALE_X * gameObject.Transform.Scale.X, AppData.SCAFFOLDING_COLLIDER_SCALE_Y * gameObject.Transform.Scale.Y,
                            AppData.SCAFFOLDING_COLLIDER_SCALE_Z * gameObject.Transform.Scale.Z);

            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    colliderScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 3; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "scaffolding " + (i + 1), AppData.SCAFFOLDING_OFFSET_Z, colliderScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Scaffolding
        }

        private void InitializeFridges()
        {
            #region Fridges

            GameObject gameObject = new GameObject("Fridge",
                       ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, AppData.FRIDGE_POSITION);

            Renderer renderer = InitializeRenderer(
                    AppData.FRIDGE_MDOEL_PATH,
                     AppData.FRIDGE_TEXTURE_PATH,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            Vector3 colliderScale = new Vector3(AppData.FRIDGE_COLLIDER_SCALE_X * gameObject.Transform.Scale.X, AppData.FRIDGE_COLLIDER_SCALE_Y * gameObject.Transform.Scale.Y,
                                         AppData.FRIDGE_COLLIDER_SCALE_Z * gameObject.Transform.Scale.Z);
            Collider collider = new Collider(gameObject, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    colliderScale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            for (int i = 0; i < 4; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "fridge " + (i + 1), AppData.FRIDGE_OFFSET_Z, colliderScale);
                sceneManager.ActiveScene.Add(gameObject);
            }

            #endregion Fridges
        }

        private void InitializeLights()
        {
            #region Lights

            var gdBasicEffect = new GDBasicEffect(litEffect);

            GameObject gameObject = new GameObject("ligth 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            Renderer renderer = InitializeRenderer
            (
                AppData.LIGHT_MODEL_PATH,
                AppData.LIGHT_TEXTURE_PATH,
                gdBasicEffect,
                1,
                Color.White
            );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            GameObject gameObjectLightTwo = CloneModelGameObject(gameObject, "light 2", AppData.LIGHT_OFFSET_X);
            sceneManager.ActiveScene.Add(gameObjectLightTwo);

            GameObject gameObjectLightThree = CloneModelGameObject(gameObjectLightTwo, "light 3", AppData.LIGHT_OFFSET_X);
            sceneManager.ActiveScene.Add(gameObjectLightThree);

            int j = 0;

            for (int i = 0; i < 3; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "light " + (j + i + 4), AppData.LIGHT_OFFSET_Z);
                sceneManager.ActiveScene.Add(gameObject);

                gameObjectLightTwo = CloneModelGameObject(gameObjectLightTwo, "light " + (j + i + 5), AppData.LIGHT_OFFSET_Z);
                sceneManager.ActiveScene.Add(gameObjectLightTwo);

                gameObjectLightThree = CloneModelGameObject(gameObjectLightThree, "light " + (j + i + 6), AppData.LIGHT_OFFSET_Z);
                sceneManager.ActiveScene.Add(gameObjectLightThree);

                j += 2;
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

            Collider collider = new ExitDoorCollider(gameObject, true, true);
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

            string model_path = "Assets/Models/Shopping Centre/Vending Machines/vending_machine_1";
            string texture_path = "Assets/Textures/Shopping Centre/Vending Machines/";

            var gameObject = new GameObject("vending machine 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-29.5f, 2.6f, 5f));

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path + "coke",
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            var aisleScale = new Vector3(160f * gameObject.Transform.Scale.X, 260f * gameObject.Transform.Scale.Y, 250f * gameObject.Transform.Scale.Z);
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

            string[] texturePathsVendingMachines = new string[] { "coke", "pepsi", "sprite" };

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -7f), aisleScale, model_path, texture_path + texturePathsVendingMachines[i + 1]);
                sceneManager.ActiveScene.Add(gameObject);
            }

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
            InitializeLabels();

            InitializePreparedFoodsAisle();
            InitializeToysAisle();
            InitializeBeveragesAisle();

            #endregion
        }

        private void InitializeBeveragesAisle()
        {
            #region Beverage Aisle

            #region Broken Bottle

            int brokeBottleNumber = 1;

            var gameObject = new GameObject("broken bottle " + brokeBottleNumber,
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(38.3f, 0.3f, -32.6f));

            string model_path = "Assets/Models/Aisles/Beverages/broken_bottle";

            string texture_path = "Assets/Textures/Aisles/Beverages/broken_bottle";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(100f * gameObject.Transform.Scale.X,
                20f * gameObject.Transform.Scale.Y, 60f * gameObject.Transform.Scale.Z);

            Collider collider = new BrokenGlassCollider(gameObject, true, true);
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

            Random random = new Random();

            brokeBottleNumber++;
            for (int i = 0; i < 4; i++)
            {
                gameObject = CloneModelGameObjectRandom(
                    gameObject,
                    "broken bottle " + brokeBottleNumber,
                    new Vector3(random.Next(36, 43), 0.3f, random.Next(-76, -21)),
                    aisleScale,
                    true,
                    ColliderType.BrokenGlass);

                sceneManager.ActiveScene.Add(gameObject);

                brokeBottleNumber++;
            }

            #endregion Broken Bottle

            #region Bottle

            int bottleNumber = 1;

            gameObject = new GameObject("bottle " + bottleNumber,
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(31f, 4.2f, -72.3f));

            model_path = "Assets/Models/Aisles/Beverages/bottle";

            texture_path = "Assets/Textures/Aisles/Beverages/heineken_label";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );
            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> ginBottlesGameObjects = new List<GameObject>();
            ginBottlesGameObjects.Add(gameObject);

            random = new Random();

            bottleNumber++;
            for (int i = 0; i < 12; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "bottle " + brokeBottleNumber, new Vector3(0, 0, 3f));
                gameObject.Transform.SetRotation(0, random.Next(0, 180), 0);
                sceneManager.ActiveScene.Add(gameObject);
                ginBottlesGameObjects.Add(gameObject);
                bottleNumber++;
            }

            for (int i = 0; i < ginBottlesGameObjects.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(ginBottlesGameObjects[i], "bottle " + brokeBottleNumber, new Vector3(0, -1.4f, 3.2f), model_path, AppData.BOTTLE_LABELS_LIST[random.Next(0, AppData.BOTTLE_LABELS_LIST.Count)], enemyEffect);
                gameObject.Transform.SetRotation(0, random.Next(0, 180), 0);
                sceneManager.ActiveScene.Add(gameObject);
                bottleNumber++;
            }

            for (int i = 0; i < ginBottlesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(ginBottlesGameObjects[i], "bottle " + brokeBottleNumber, new Vector3(0, -2.9f, 4.2f), model_path, AppData.BOTTLE_LABELS_LIST[random.Next(0, AppData.BOTTLE_LABELS_LIST.Count)], enemyEffect);
                gameObject.Transform.SetRotation(0, random.Next(0, 180), 0);
                sceneManager.ActiveScene.Add(gameObject);
                bottleNumber++;
            }

            for (int i = 0; i < ginBottlesGameObjects.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(ginBottlesGameObjects[i], "bottle " + brokeBottleNumber, new Vector3(1.5f, -1.4f, 3.2f), model_path, AppData.BOTTLE_LABELS_LIST[random.Next(0, AppData.BOTTLE_LABELS_LIST.Count)], enemyEffect);
                gameObject.Transform.SetRotation(0, random.Next(0, 180), 0);
                sceneManager.ActiveScene.Add(gameObject);
                bottleNumber++;
            }

            for (int i = 0; i < ginBottlesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(ginBottlesGameObjects[i], "bottle " + brokeBottleNumber, new Vector3(1.5f, -2.9f, 4.2f), model_path, AppData.BOTTLE_LABELS_LIST[random.Next(0, AppData.BOTTLE_LABELS_LIST.Count)], enemyEffect);
                gameObject.Transform.SetRotation(0, random.Next(0, 180), 0);
                sceneManager.ActiveScene.Add(gameObject);
                bottleNumber++;
            }

            #endregion Bottle

            #endregion Beverage Aisle
        }

        private void InitializeLabels()
        {
            #region Aisle Labels

            int labelNumber = 1;

            var gameObject = new GameObject("label " + labelNumber,
                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);

            string model_path = "Assets/Models/Aisles/Label/label";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    AppData.LABELS_LIST[0],
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            List<GameObject> labelGameObjects = new List<GameObject>();
            labelGameObjects.Add(gameObject);

            labelNumber++;

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "label " + labelNumber, new Vector3(16.5f, 0, 0), "Assets/Models/Aisles/Label/label", AppData.LABELS_LIST[i + 1]);
                labelGameObjects.Add(gameObject);
                sceneManager.ActiveScene.Add(gameObject);
                labelNumber++;
            }

            for (int i = 0; i < labelGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(labelGameObjects[i], "label " + labelNumber, new Vector3(0, 0, -52.2f), "Assets/Models/Aisles/Label/label", AppData.LABELS_LIST[i + 4]);
                sceneManager.ActiveScene.Add(gameObject);
                labelNumber++;
            }

            gameObject = CloneModelGameObject(labelGameObjects[0], "label " + labelNumber, new Vector3(-11.9f, 0, -26.5f), "Assets/Models/Aisles/Label/label", AppData.LABELS_LIST[3]);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion Aisle Labels
        }

        private void InitializeGreens()
        {
            Random random = new Random();
            int weedNumber = 1;

            #region Weed Tulip

            GameObject gameObject = new GameObject("weed " + weedNumber,
                  ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0.4f, 1f));
            string texture_path = "Assets/Textures/Shopping Centre/Greens/weed_tulip";

            string model_path = "Assets/Models/Shopping Centre/Greens/weed_tulip";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            weedNumber++;

            for (int i = 0; i < 200; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "weed " + weedNumber, new Vector3(random.Next(-31, 47), 0.4f, random.Next(-87, 87)));
                sceneManager.ActiveScene.Add(gameObject);
                weedNumber++;
            }

            #endregion Weed Tulip

            #region Weed Flower

            gameObject = new GameObject("weed " + weedNumber,
                 ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0.4f, 1f));
            texture_path = "Assets/Textures/Shopping Centre/Greens/weed_flower";

            model_path = "Assets/Models/Shopping Centre/Greens/weed_flower";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            weedNumber++;

            for (int i = 0; i < 200; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "weed " + weedNumber, new Vector3(random.Next(-31, 47), 0.4f, random.Next(-87, 87)));
                sceneManager.ActiveScene.Add(gameObject);
                weedNumber++;
            }

            #endregion Weed Flower

            #region Weed

            gameObject = new GameObject("weed " + weedNumber,
                ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0.4f, 1f));
            texture_path = "Assets/Textures/Shopping Centre/Greens/weed";

            model_path = "Assets/Models/Shopping Centre/Greens/weed_flower";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            weedNumber++;

            for (int i = 0; i < 200; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "weed " + weedNumber, new Vector3(random.Next(-31, 47), 0.4f, random.Next(-87, 87)));
                sceneManager.ActiveScene.Add(gameObject);
                weedNumber++;
            }

            #endregion Weed

            #region Bushes

            gameObject = new GameObject("weed " + weedNumber,
              ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 2.5f, 1f));
            texture_path = "Assets/Textures/Shopping Centre/Greens/bush";

            model_path = "Assets/Models/Shopping Centre/Greens/bush";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            weedNumber++;

            for (int i = 0; i < 20; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "weed " + weedNumber, new Vector3(random.Next(-4, 1), random.Next(1, 5), random.Next(-11, 35)));
                sceneManager.ActiveScene.Add(gameObject);
                weedNumber++;
            }

            for (int i = 0; i < 40; i++)
            {
                gameObject = CloneModelGameObjectRandom(gameObject, "weed " + weedNumber, new Vector3(random.Next(-31, 47), random.Next(1, 2), random.Next(-87, 87)));
                sceneManager.ActiveScene.Add(gameObject);
                weedNumber++;
            }

            #endregion Bushes
        }

        private void InitializeToysAisle()
        {
            #region Toys

            #region Computer Games

            int computerGamesNumber = 1;

            GameObject gameObject = new GameObject("computer game " + computerGamesNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, new Vector3(15f, 0, 0), new Vector3(15.6f, 2.6f, -26.4f));
            string texture_path = "Assets/Textures/Aisles/Toys/Computer Games/cod_ww2";

            string model_path = "Assets/Models/Aisles/Toys/computer_game";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> computerGameObjects = new List<GameObject>();
            computerGameObjects.Add(gameObject);

            computerGamesNumber++;

            for (int i = 0; i < 6; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "computer game " + computerGamesNumber, new Vector3(0, 0, -0.1f));
                sceneManager.ActiveScene.Add(gameObject);
                computerGameObjects.Add(gameObject);
                computerGamesNumber++;
            }

            float zAxis = -0.7f;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < computerGameObjects.Count; j++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[j], "computer game" + computerGamesNumber, new Vector3(0, 0, zAxis), model_path, "Assets/Textures/Aisles/Toys/Computer Games/god_of_war", enemyEffect);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                for (int k = 0; k < computerGameObjects.Count; k++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[k], "computer game " + computerGamesNumber, new Vector3(0, 0, zAxis - 2f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/gta_v", enemyEffect);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                zAxis += -0.7f;
            }

            gameObject = CloneModelGameObject(gameObject, "computer game" + computerGamesNumber, new Vector3(0.1f, -0.4f, -1.6f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/farcry_4");
            gameObject.Transform.SetRotation(90, -90, 0);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> computerGameObjectsTwo = new List<GameObject>();
            computerGameObjectsTwo.Add(gameObject);

            for (int i = 0; i < 5; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "computer game " + computerGamesNumber, new Vector3(0, 0.1f, 0.02f));
                sceneManager.ActiveScene.Add(gameObject);
                computerGameObjectsTwo.Add(gameObject);
                computerGamesNumber++;
            }

            for (int i = 0; i < computerGameObjectsTwo.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(computerGameObjectsTwo[i], "computer game " + computerGamesNumber, new Vector3(0.02f, -1.5f, -1f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/tekken_7", enemyEffect);
                sceneManager.ActiveScene.Add(gameObject);
                computerGamesNumber++;
            }

            for (int i = 0; i < computerGameObjectsTwo.Count; i++)
            {
                gameObject = CloneModelGameObject(computerGameObjectsTwo[i], "computer game " + computerGamesNumber, new Vector3(0.02f, -1.5f, -2.2f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/fallout_76", enemyEffect);
                gameObject.Transform.SetRotation(90, 45, 0);
                sceneManager.ActiveScene.Add(gameObject);
                computerGamesNumber++;
            }

            for (int i = 0; i < computerGameObjectsTwo.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(computerGameObjectsTwo[i], "computer game " + computerGamesNumber, new Vector3(0.04f, 0, -2f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/deus", enemyEffect);
                gameObject.Transform.SetRotation(85, -50, 0);
                sceneManager.ActiveScene.Add(gameObject);
                computerGamesNumber++;
            }

            zAxis = 0.01f;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < computerGameObjects.Count; j++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[j], "computer game" + computerGamesNumber, new Vector3(0f, -1.7f, zAxis), model_path, "Assets/Textures/Aisles/Toys/Computer Games/uncharted", enemyEffect);
                    gameObject.Transform.SetRotation(45f, 0, 0);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                for (int k = 0; k < computerGameObjects.Count; k++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[k], "computer game " + computerGamesNumber, new Vector3(0, -1.7f, zAxis - 1.5f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/outlast", enemyEffect);
                    gameObject.Transform.SetRotation(50f, 0, 1.2f);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                for (int l = 0; l < computerGameObjects.Count; l++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[l], "computer game " + computerGamesNumber, new Vector3(0, -1.7f, zAxis - 3f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/hellblade", enemyEffect);
                    gameObject.Transform.SetRotation(50f, 0, 1.2f);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                zAxis += -0.7f;
            }

            zAxis = -9f;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < computerGameObjects.Count; j++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[j], "computer game" + computerGamesNumber, new Vector3(0f, -0.2f, zAxis), model_path, "Assets/Textures/Aisles/Toys/Computer Games/farcry", enemyEffect);
                    gameObject.Transform.SetRotation(58f, 0, 0);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                for (int k = 0; k < computerGameObjects.Count; k++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[k], "computer game " + computerGamesNumber, new Vector3(0, -0.2f, zAxis - 1.5f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/mad_max", enemyEffect);
                    gameObject.Transform.SetRotation(58f, 0, 1.2f);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                for (int l = 0; l < computerGameObjects.Count; l++)
                {
                    gameObject = CloneModelGameObject(computerGameObjects[l], "computer game " + computerGamesNumber, new Vector3(0, -0.2f, zAxis - 3f), model_path, "Assets/Textures/Aisles/Toys/Computer Games/infamous", enemyEffect);
                    gameObject.Transform.SetRotation(58f, 0, 1.2f);
                    sceneManager.ActiveScene.Add(gameObject);
                    computerGamesNumber++;
                }

                zAxis += -0.7f;
            }

            #endregion Computer Games

            #region R2-D2

            int r2d2Number = 1;
            gameObject = new GameObject("R2-D2 " + r2d2Number,
                  ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Aisles/Toys/R2-D2/r2_blue";

            model_path = "Assets/Models/Aisles/Toys/r2_d2";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> r2d2GameObjects = new List<GameObject>();
            r2d2GameObjects.Add(gameObject);
            r2d2Number++;

            gameObject = CloneModelGameObject(gameObject, "R2-D2 " + r2d2Number, new Vector3(0, 0, 2f), "Assets/Models/Aisles/Toys/r2_d2", "Assets/Textures/Aisles/Toys/R2-D2/r2_green", enemyEffect);
            sceneManager.ActiveScene.Add(gameObject);
            r2d2GameObjects.Add(gameObject);
            r2d2Number++;

            gameObject = CloneModelGameObject(gameObject, "R2-D2 " + r2d2Number, new Vector3(0, 0, 2f), "Assets/Models/Aisles/Toys/r2_d2", "Assets/Textures/Aisles/Toys/R2-D2/r2_red", enemyEffect);
            sceneManager.ActiveScene.Add(gameObject);
            r2d2GameObjects.Add(gameObject);
            r2d2Number++;

            for (int i = 0; i < r2d2GameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(r2d2GameObjects[i], "R2-D2 " + r2d2Number, new Vector3(0, 0, 6f));
                sceneManager.ActiveScene.Add(gameObject);
                r2d2Number++;
            }

            #endregion R2-D2

            #region Board Games

            gameObject = new GameObject("Board Games" + r2d2Number,
                  ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Aisles/Toys/board_games";

            model_path = "Assets/Models/Aisles/Toys/board_games";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion Board Games

            #region Teddy Bears

            int teddyBearNumber = 1;
            gameObject = new GameObject("teddy bear " + teddyBearNumber,
                  ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, -0.2f, 0));
            texture_path = "Assets/Textures/Aisles/Toys/teddy_bear";

            model_path = "Assets/Models/Aisles/Toys/teddy_bear";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(unlitEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            teddyBearNumber++;

            for (int i = 0; i < 10; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "teddy bear " + teddyBearNumber, new Vector3(0, 0, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                teddyBearNumber++;
            }

            #endregion Teddy Bears

            gameObject = new GameObject(AppData.TOY_RADIO_NAME,
                 ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Collectible;

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(14.9f, 2.5f, -25f));
            texture_path = "Assets/Textures/Aisles/Toys/toy_radio";

            model_path = "Assets/Models/Aisles/Toys/toy_radio";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            //Collider collider = new PickupCollider(gameObject, true, true);
            //collider.AddPrimitive(
            //    new Box(
            //        gameObject.Transform.Translation,
            //        gameObject.Transform.Rotation,
            //        120 * gameObject.Transform.Scale
            //        ),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f)
            //    );

            //collider.Enable(gameObject, true, 10);
            //gameObject.AddComponent(collider);

            gameObject.AddComponent(new InteractableBehaviour());

            gameObject.AddComponent(new RadioController());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion Toys
        }

        private void InitializePreparedFoodsAisle()
        {
            #region Prepared Foods

            #region Cereal Box

            int cerealNumber = 1;

            var gameObject = new GameObject("cereal box " + cerealNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            var texture_path = "Assets/Textures/Aisles/Prepared Foods/cereal_box";

            var model_path = "Assets/Models/Aisles/Prepared Foods/cereal_box";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> cerealBoxesGameObjects = new List<GameObject>();
            cerealBoxesGameObjects.Add(gameObject);

            cerealNumber++;

            for (int i = 0; i < 2; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "cereal box " + cerealNumber, new Vector3(-0.26f, 0, 0));
                sceneManager.ActiveScene.Add(gameObject);
                cerealBoxesGameObjects.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, 0, -2f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, 0, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < 1; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, 0, 3f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, -1.5f, -2f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, -1.5f, -3f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, -1.5f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, -3f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            for (int i = 0; i < cerealBoxesGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(cerealBoxesGameObjects[i], "cereal box " + cerealNumber, new Vector3(0, -3f, -2f));
                sceneManager.ActiveScene.Add(gameObject);
                cerealNumber++;
            }

            #endregion Cereal Box

            #region Energy Drink

            int energyDrinkNumber = 1;

            gameObject = new GameObject("energy drink " + energyDrinkNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Aisles/Prepared Foods/energy_drink";

            model_path = "Assets/Models/Aisles/Prepared Foods/energy_drink";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> energyDrinksGameObjects = new List<GameObject>();
            energyDrinksGameObjects.Add(gameObject);

            energyDrinkNumber++;

            for (int i = 0; i < 10; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "energy drink " + energyDrinkNumber, new Vector3(0f, 0, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                energyDrinksGameObjects.Add(gameObject);
                energyDrinkNumber++;
            }

            for (int i = 0; i < energyDrinksGameObjects.Count - 4; i++)
            {
                gameObject = CloneModelGameObject(energyDrinksGameObjects[i], "energy drink " + energyDrinkNumber, new Vector3(0f, -1.5f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                energyDrinkNumber++;
            }

            for (int i = 0; i < energyDrinksGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(energyDrinksGameObjects[i], "energy drink " + energyDrinkNumber, new Vector3(-1.4f, -3f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                energyDrinkNumber++;
            }

            for (int i = 2; i < energyDrinksGameObjects.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(energyDrinksGameObjects[i], "energy drink " + energyDrinkNumber, new Vector3(0f, -3f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                energyDrinkNumber++;
            }

            #endregion Energy Drink

            #region Juice Carton

            int juiceCartonNumber = 1;

            gameObject = new GameObject("juice carton " + energyDrinkNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0, 5f));
            texture_path = "Assets/Textures/Aisles/Prepared Foods/juice_carton";

            model_path = "Assets/Models/Aisles/Prepared Foods/juice_carton";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> juiceCartonsGameObjects = new List<GameObject>();
            juiceCartonsGameObjects.Add(gameObject);

            juiceCartonNumber++;

            for (int i = 0; i < 4; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "juice carton " + juiceCartonNumber, new Vector3(0f, 0, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                juiceCartonsGameObjects.Add(gameObject);
                juiceCartonNumber++;
            }

            for (int i = 0; i < juiceCartonsGameObjects.Count - 1; i++)
            {
                gameObject = CloneModelGameObject(juiceCartonsGameObjects[i], "juice carton  " + juiceCartonNumber, new Vector3(0f, -1.5f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                juiceCartonNumber++;
            }

            for (int i = 0; i < juiceCartonsGameObjects.Count - 2; i++)
            {
                gameObject = CloneModelGameObject(juiceCartonsGameObjects[i], "juice carton  " + juiceCartonNumber, new Vector3(0f, -3f, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                juiceCartonNumber++;
            }

            #endregion Juice Carton

            #region Crisps

            int crispsNumber = 1;

            gameObject = new GameObject("crisps " + crispsNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, Vector3.Zero);
            texture_path = "Assets/Textures/Aisles/Prepared Foods/crisps";

            model_path = "Assets/Models/Aisles/Prepared Foods/crisps";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> crispsGameObjects = new List<GameObject>();
            crispsGameObjects.Add(gameObject);

            crispsNumber++;

            for (int i = 0; i < 6; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "crisps " + crispsNumber, new Vector3(0.1f, 0, 0f));
                sceneManager.ActiveScene.Add(gameObject);
                crispsGameObjects.Add(gameObject);
                crispsNumber++;
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < crispsGameObjects.Count; j++)
                {
                    gameObject = CloneModelGameObject(crispsGameObjects[j], "crisps " + crispsNumber, new Vector3(0f, 0, 1f + i));
                    sceneManager.ActiveScene.Add(gameObject);
                    crispsNumber++;
                }

                for (int k = 0; k < crispsGameObjects.Count; k++)
                {
                    gameObject = CloneModelGameObject(crispsGameObjects[k], "crisps " + crispsNumber, new Vector3(0f, 0, -1f - i));
                    sceneManager.ActiveScene.Add(gameObject);
                    crispsNumber++;
                }
            }

            #endregion Crisps

            #region Cans

            int canNumber = 1;

            gameObject = new GameObject("can " + canNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0, -2f));
            model_path = "Assets/Models/Aisles/Prepared Foods/can";
            texture_path = "Assets/Textures/Aisles/Prepared Foods/can_tomato_soup";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(enemyEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> cansGameObjects = new List<GameObject>();
            cansGameObjects.Add(gameObject);

            canNumber++;

            for (int i = 0; i < 12; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "can " + canNumber, new Vector3(0, 0, -0.5f));
                sceneManager.ActiveScene.Add(gameObject);
                cansGameObjects.Add(gameObject);
                canNumber++;
            }

            for (int i = 0; i < 12; i++)
            {
                gameObject = CloneModelGameObject(cansGameObjects[i], "can " + canNumber, new Vector3(0.5f, 0, 2f));
                sceneManager.ActiveScene.Add(gameObject);
                cansGameObjects.Add(gameObject);
                canNumber++;
            }

            float zAxis = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < cansGameObjects.Count; j++)
                {
                    gameObject = CloneModelGameObject(cansGameObjects[j], "can " + canNumber, new Vector3(0, -1.38f, zAxis), model_path, "Assets/Textures/Aisles/Prepared Foods/can_ministrone", enemyEffect);
                    sceneManager.ActiveScene.Add(gameObject);
                    canNumber++;
                }

                for (int k = 0; k < cansGameObjects.Count; k++)
                {
                    gameObject = CloneModelGameObject(cansGameObjects[k], "can " + canNumber, new Vector3(0, -2.9f, zAxis), model_path, "Assets/Textures/Aisles/Prepared Foods/can_pea_stew", enemyEffect);
                    sceneManager.ActiveScene.Add(gameObject);
                    canNumber++;
                }

                zAxis += -7f;
            }

            #endregion Cans

            #region Olive Oil

            int oliveOilNumber = 1;

            gameObject = new GameObject("olive oil " + oliveOilNumber,
                   ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(0.02f * Vector3.One, Vector3.Zero, new Vector3(0, 0, 1f));
            model_path = "Assets/Models/Aisles/Prepared Foods/olive_oil";
            texture_path = "Assets/Textures/Aisles/Prepared Foods/olive_oil";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);
            List<GameObject> oliveOilGameObjects = new List<GameObject>();
            cansGameObjects.Add(gameObject);

            oliveOilNumber++;

            for (int i = 0; i < 6; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "olive oil " + oliveOilNumber, new Vector3(0, 0, -1f));
                sceneManager.ActiveScene.Add(gameObject);
                oliveOilGameObjects.Add(gameObject);
                oliveOilNumber++;
            }

            for (int i = 0; i < 6; i++)
            {
                gameObject = CloneModelGameObject(oliveOilGameObjects[i], "olive oil " + oliveOilNumber, new Vector3(-0.5f, 0, 2f));
                sceneManager.ActiveScene.Add(gameObject);
                oliveOilGameObjects.Add(gameObject);
                oliveOilNumber++;
            }

            for (int i = 0; i < oliveOilGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(oliveOilGameObjects[i], "olive oil " + oliveOilNumber, new Vector3(0, -1.5f, 0.5f));
                sceneManager.ActiveScene.Add(gameObject);
                oliveOilNumber++;
            }

            for (int i = 0; i < oliveOilGameObjects.Count; i++)
            {
                gameObject = CloneModelGameObject(oliveOilGameObjects[i], "olive oil " + oliveOilNumber, new Vector3(0, -3f, -1f));
                sceneManager.ActiveScene.Add(gameObject);
                oliveOilNumber++;
            }

            #endregion Olive Oil

            #endregion Prepared Foods
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
                new Vector3(66.4f, 3f, 79.8f));

            var texture_path = "Assets/Textures/Props/Generator_Room/fuse_box_diffuse";

            var model_path = "Assets/Models/Generator Room/fuse_box";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            InteractibleCollider interactibleCollider = new FuseboxCollider(gameObject, true, true);
            interactibleCollider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 550
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            interactibleCollider.Enable(gameObject, true, 5);
            gameObject.AddComponent(interactibleCollider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Gate Access Machine

            gameObject = new GameObject(AppData.GATE_ACCESS_MACHINE_NAME,
                                ObjectType.Static, RenderType.Opaque);
            gameObject.GameObjectType = GameObjectType.Interactible;

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero,
                new Vector3(48.75f, 3.5f, 80.3f));

            texture_path = "Assets/Textures/Props/Generator_Room/access_card_machine_emission";

            model_path = "Assets/Models/Generator Room/gate_access_machine_test_2";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            interactibleCollider = new AccessMachineCollider(gameObject, true, true);
            interactibleCollider.AddPrimitive(new Box(
                gameObject.Transform.Translation,
                gameObject.Transform.Rotation,
                gameObject.Transform.Scale * 350
                ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            interactibleCollider.Enable(gameObject, true, 5);
            gameObject.AddComponent(interactibleCollider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Generator

            gameObject = new GameObject(AppData.GENERATOR_NAME,
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(64.6f, 2f, 85.2f));

            texture_path = "Assets/Textures/Props/Generator_Room/generator";

            model_path = "Assets/Models/Generator Room/generator";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(200f * gameObject.Transform.Scale.X, 180f * gameObject.Transform.Scale.Y, 250f * gameObject.Transform.Scale.Z);
            var collider = new Collider(gameObject, true);
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

            gameObject.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Lever

            gameObject = new GameObject("lever",
                                    ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(66.25f, 3.1f, 76.81f));

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

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(66.4f, 3f, 76.8f));

            texture_path = "Assets/Textures/Props/Generator_Room/panel_base_colour";

            model_path = "Assets/Models/Generator Room/panel";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            collider = new LeverCollider(gameObject, true, true);
            collider.AddPrimitive(
                new Box(
                    gameObject.Transform.Translation,
                    gameObject.Transform.Rotation,
                    400 * gameObject.Transform.Scale
                    ),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            collider.Enable(gameObject, true, 10);
            gameObject.AddComponent(collider);

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Electrical Box

            gameObject = new GameObject("electrical box",
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-0.8f, 0, 0));

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

            #region Manequins

            int manequinNumber = 1;
            gameObject = new GameObject("manequin " + manequinNumber,
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-0.8f, 0, 0));

            texture_path = "Assets/Textures/Props/Generator_Room/manequin";

            model_path = "Assets/Models/Generator Room/manequin";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            List<GameObject> manequinsGameObject = new List<GameObject>();
            manequinsGameObject.Add(gameObject);

            manequinNumber++;
            for (int i = 0; i < 3; i++)
            {
                gameObject = CloneModelGameObject(gameObject, "manequin ", new Vector3(3f, 0, 0));
                sceneManager.ActiveScene.Add(gameObject);
                manequinsGameObject.Add(gameObject);
                manequinNumber++;
            }

            for (int i = 0; i < manequinsGameObject.Count; i++)
            {
                gameObject = CloneModelGameObject(manequinsGameObject[i], "manequin ", new Vector3(0, 0, 1f));
                sceneManager.ActiveScene.Add(gameObject);
                manequinNumber++;
            }

            #endregion Manequins

            #region Voltage Sign

            gameObject = new GameObject("voltage sign",
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-0.8f, 0, 0));

            texture_path = "Assets/Textures/Props/Generator_Room/warning_volts";

            model_path = "Assets/Models/Generator Room/warning_volts";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion Voltage Sign

            #region Fan

            gameObject = new GameObject("fan",
                                        ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-1f, 0, 0));

            texture_path = "Assets/Textures/Props/Generator_Room/fan";

            model_path = "Assets/Models/Generator Room/fan";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    new GDBasicEffect(litEffect),
                    1
                    );

            gameObject.AddComponent(renderer);
            sceneManager.ActiveScene.Add(gameObject);

            #endregion Fan
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

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset, Vector3 colliderScale, string meshPath, string texturePath)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer cloneRenderer = InitializeRenderer(
                   meshPath,
                   texturePath,
                   new GDBasicEffect(unlitEffect),
                   1
                   );

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

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset, string meshPath, string texturePath)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer cloneRenderer = InitializeRenderer(
                   meshPath,
                   texturePath,
                   new GDBasicEffect(unlitEffect),
                   1
                   );

            gameObjectClone.AddComponent(cloneRenderer);
            return gameObjectClone;
        }

        private GameObject CloneModelGameObject(GameObject gameObject, string newName, Vector3 offset, string meshPath, string texturePath, BasicEffect effect)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                gameObject.Transform.Translation + offset
                );

            Renderer cloneRenderer = InitializeRenderer(
                   meshPath,
                   texturePath,
                   new GDBasicEffect(effect),
                   1
                   );

            gameObjectClone.AddComponent(cloneRenderer);
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

        private GameObject CloneModelGameObjectRandom(GameObject gameObject, string newName, Vector3 newTranslation)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                newTranslation
                );

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Renderer cloneRenderer = new Renderer(renderer.Effect, renderer.Material, renderer.Mesh);
            gameObjectClone.AddComponent(cloneRenderer);

            return gameObjectClone;
        }

        private GameObject CloneModelGameObjectRandom(GameObject gameObject, string newName,
            Vector3 newTranslation, Vector3 colliderScale)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                newTranslation
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

        private GameObject CloneModelGameObjectRandom(GameObject gameObject, string newName,
            Vector3 newTranslation, Vector3 colliderScale, bool isTrigger, ColliderType colliderType)
        {
            GameObject gameObjectClone = new GameObject(newName, gameObject.ObjectType, gameObject.RenderType);
            gameObjectClone.GameObjectType = gameObject.GameObjectType;

            gameObjectClone.Transform = new Transform(
                gameObject.Transform.Scale,
                gameObject.Transform.Rotation,
                newTranslation
                );

            Renderer renderer = gameObject.GetComponent<Renderer>();
            Renderer cloneRenderer = new Renderer(renderer.Effect, renderer.Material, renderer.Mesh);
            gameObjectClone.AddComponent(cloneRenderer);

            Collider cloneCollider;

            switch (colliderType)
            {
                case ColliderType.Default:
                    cloneCollider = new Collider(gameObjectClone, true, isTrigger);
                    break;

                case ColliderType.BrokenGlass:
                    cloneCollider = new BrokenGlassCollider(gameObjectClone, true, isTrigger);
                    break;

                default:
                    cloneCollider = new Collider(gameObjectClone, true, isTrigger);
                    break;
            }

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

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-60f, 1.3f, -74f));
            var texture_path = "Assets/Textures/Props/Office/ChairSetT2_Diffuse";

            var model_path = "Assets/Models/Office/office_chair";

            Renderer renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            var aisleScale = new Vector3(80f * gameObject.Transform.Scale.X, 200f * gameObject.Transform.Scale.Y, 100f * gameObject.Transform.Scale.Z);
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

            sceneManager.ActiveScene.Add(gameObject);

            #endregion

            #region Office Table

            gameObject = new GameObject("office table 1",
             ObjectType.Static, RenderType.Opaque);

            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One,
                Vector3.Zero, new Vector3(-57.8f, 1.6f, -72.7f));
            texture_path = "Assets/Textures/Props/Office/TabletextureSet1_Diffuse";

            model_path = "Assets/Models/Office/office_table_1";

            renderer = InitializeRenderer(
                    model_path,
                    texture_path,
                    gdBasicEffect,
                    1
                    );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(120f * gameObject.Transform.Scale.X, 190f * gameObject.Transform.Scale.Y, 320f * gameObject.Transform.Scale.Z);
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

            #endregion

            #region Office Shelves

            texture_path = "Assets/Textures/Props/Office/metal";
            string model_base_path = "Assets/Models/Office/Shelves/office_shelf_";

            gameObject = new GameObject("office shelf 1", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            model_path = model_base_path + "1";

            renderer = InitializeRenderer(
                model_path,
                texture_path,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("office shelf 2", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, Vector3.Zero);

            model_path = model_base_path + "2";

            renderer = InitializeRenderer(
                model_path,
                texture_path,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            sceneManager.ActiveScene.Add(gameObject);

            gameObject = new GameObject("office shelf 3", ObjectType.Static, RenderType.Opaque);
            gameObject.Transform = new Transform(AppData.DEFAULT_OBJECT_SCALE * Vector3.One, Vector3.Zero, new Vector3(-56.2f, 2.5f, -64.4f));

            model_path = model_base_path + "3";

            renderer = InitializeRenderer(
                model_path,
                texture_path,
                gdBasicEffect,
                1
                );

            gameObject.AddComponent(renderer);

            aisleScale = new Vector3(300f * gameObject.Transform.Scale.X, 220f * gameObject.Transform.Scale.Y, 100f * gameObject.Transform.Scale.Z);
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

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, -17f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(10f, 0, 0), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

            gameObject = CloneModelGameObject(gameObject, "Aisle ", new Vector3(0, 0, 17f), aisleScale);
            sceneManager.ActiveScene.Add(gameObject);

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
            renderManager.DrawOrder = 1;
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

            menuManager = new SceneManager<Scene2D>(this);
            menuManager.StatusType = StatusType.Updated;
            menuManager.IsPausedOnPlay = true;
            Components.Add(menuManager);

            var menuRenderManager = new Render2DManager(this, _spriteBatch, menuManager);
            menuRenderManager.StatusType = StatusType.Drawn;
            menuRenderManager.DrawOrder = 3;
            menuRenderManager.IsPausedOnPlay = true;
            Components.Add(menuRenderManager);

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
            #region Menu On/Off with U/P

            if (Input.Keys.WasJustPressed(Keys.P))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                    EventActionType.OnPause));
            }
            else if (Input.Keys.WasJustPressed(Keys.U))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                   EventActionType.OnPlay));
            }

            #endregion

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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