#define DEMO_STATES

using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using GD.Engine;
using GD.Engine.Globals;
using Microsoft.Xna.Framework.Audio;

namespace GD.App
{
    public enum GameState
    {
        Default,
        GeneratorRoomOpen,
        FuseIn,
        GeneratorOn,
        Win,
        Lose
    }

    public enum TimeState
    {
        Beginning,
        QuarterTime,
        HalfTime,
        ThreeFourthTime,
        End
    }

    public class StateManager : GameComponent
    {
        private double maxTimeInMS;
        private double totalElapsedTimeMS;
        private double minutesLeft;
        private double secondsLeft;
        private bool stopTime = false;

        private GameState currentState;
        private TimeState timeState;

        #region Properties

        public double TotalElapsedTimeMS
        {
            get => totalElapsedTimeMS;
        }

        public double CountdownTimeSecs
        {
            get => secondsLeft;
        }

        public double CountdownTimeMins
        {
            get => minutesLeft;
        }

        public GameState CurrentGameState
        {
            get => currentState;
        }

        public TimeState CurrentTimeState
        {
            get => timeState;
        }

        #endregion Properties

        public StateManager(Game game, double maxTimeInMS) : base(game)
        {
            this.maxTimeInMS = maxTimeInMS;
            totalElapsedTimeMS = 0;

            //Register
            EventDispatcher.Subscribe(EventCategoryType.GameState, HandleGameStateEvent);
        }

        private void HandleGameStateEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {
                case EventActionType.OnChangeState:
                    GameState newState = (GameState)eventData.Parameters[0];
                    HandleGameStateChange(newState);
                    break;

                case EventActionType.OnReachExit:
                    CheckWin();
                    break;

                default:
                    break;
            }
        }

        private void HandleGameStateChange(GameState newState)
        {
            this.currentState = newState;
            switch (currentState)
            {
                case GameState.GeneratorRoomOpen:
#if DEMO_STATES

                    System.Diagnostics.Debug.WriteLine("Generator room is now open");

                    Application.SceneManager.ActiveScene.Remove(
                        ObjectType.Static,
                        RenderType.Opaque,
                        (x) => x.Name == AppData.GENERATOR_DOOR_NAME);
#endif
                    break;

                case GameState.FuseIn:
                    System.Diagnostics.Debug.WriteLine("Fuse In");
                    break;

                case GameState.GeneratorOn:

                    //System.Diagnostics.Debug.WriteLine("Generator is now on");

                    // Send subtitles event
                    object[] parameters = { DialogueState.GeneratorWorking };
                    EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));

                    // Play alarm sound
                    RaiseAlarmEvent(EventActionType.OnPlay2D);

                    // Play generator sound
                    RaiseGeneratorSoundEvent(EventActionType.OnPlay3D);

                    break;

                case GameState.Win:

                    System.Diagnostics.Debug.WriteLine("You Escaped");

                    // Stop alarm sound: Alarm sound playing twice for some reason - will fix later
                    RaiseAlarmEvent(EventActionType.OnStop);
                    RaiseAlarmEvent(EventActionType.OnStop);

                    // Stop Generator Sound
                    RaiseGeneratorSoundEvent(EventActionType.OnStop);

                    break;

                case GameState.Lose:
                    System.Diagnostics.Debug.WriteLine("You Lose");
                    // Alarm sound playing twice for some reason - will fix later
                    RaiseAlarmEvent(EventActionType.OnStop);
                    RaiseAlarmEvent(EventActionType.OnStop);
                    break;

                default:
                    break;
            }
        }

        private void HandleTimeStateChange(TimeState newState)
        {
            //this.timeState = newState;
            //object[] parameters;

            //GameObject dialogue = Application.SceneManager.ActiveScene.Find(
            //    ObjectType.Static,
            //    RenderType.Opaque,
            //    (x) => x.Name == AppData.SUBTITLES_NAME
            //    );

            //DialogueController dialogueController = dialogue.GetComponent<DialogueController>();

            //switch (timeState)
            //{
            //    case TimeState.HalfTime:
            //        if (dialogueController.CurrentSubtitle == DialogueState.NoDialogue)
            //        {
            //            // Send event to show half-time subtitle
            //            parameters = new object[] { DialogueState.Time1 };
            //            EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
            //        }

            //        break;

            //    case TimeState.ThreeFourthTime:
            //        if (dialogueController.CurrentSubtitle == DialogueState.NoDialogue)
            //        {
            //            parameters = new object[] { DialogueState.Time2 };
            //            EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
            //        }

            //        break;

            //    default:
            //        break;
            //}
        }

        private bool CheckWin()
        {
            if ((currentState == GameState.GeneratorOn || currentState == GameState.Win)
                && totalElapsedTimeMS < AppData.MAX_GAME_TIME_IN_MSECS)
            {
                HandleGameStateChange(GameState.Win);
                return true;
            }
            else
            {
                // Raise event to show subtitles related to having no power in the building
                object[] parameters = { DialogueState.ExitDoorNoPower };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));

                System.Diagnostics.Debug.WriteLine("Need to restore power first!");
            }

            return false;
        }

        private bool CheckLose()
        {
            if (totalElapsedTimeMS >= AppData.MAX_GAME_TIME_IN_MSECS
                && currentState != GameState.Win && currentState != GameState.Lose)
            {
                HandleGameStateChange(GameState.Lose);
                return true;
            }

            return false;
        }

        private void RaiseAlarmEvent(EventActionType eventActionType)
        {
            object[] parameters = { "alarm-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, eventActionType, parameters));
        }

        private void RaiseGeneratorSoundEvent(EventActionType eventActionType)
        {
            var cameraAudioListener = Application.CameraManager.ActiveCamera.gameObject
                .GetComponent<AudioListenerBehaviour>().AudioListener;

            GameObject generator = Application.SceneManager.ActiveScene.Find(
                ObjectType.Static,
                RenderType.Opaque,
                (x) => x.Name == AppData.GENERATOR_NAME
                );

            var generatorAudioEmitter = generator.GetComponent<AudioEmitterBehaviour>()?.AudioEmitter;

            object[] parameters = { AppData.GENERATOR_SOUND_NAME, cameraAudioListener, generatorAudioEmitter };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, eventActionType, parameters));
        }

        public override void Update(GameTime gameTime)
        {
            if (!stopTime)
            {
                totalElapsedTimeMS += gameTime.ElapsedGameTime.Milliseconds;
                object[] parameters = { AppData.INFECTION_METER_NAME, (float)(totalElapsedTimeMS) };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnInfectionDelta, parameters));
            }
            else
            {
                object[] parameters = { AppData.INFECTION_METER_NAME, (float)(maxTimeInMS) };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnInfectionDelta, parameters));
            }

            if (totalElapsedTimeMS >= maxTimeInMS)
            {
                stopTime = true;
                CheckLose();
                //object[] parameters = { "You win!", totalElapsedTimeMS, "win_soundcue" };
                //EventDispatcher.Raise(
                //    new EventData(EventCategoryType.Player,
                //    EventActionType.OnLose,
                //    parameters));
            }

            if (totalElapsedTimeMS >= maxTimeInMS / 2 && timeState < TimeState.HalfTime)
            {
                HandleTimeStateChange(TimeState.HalfTime);
            }
            else if (totalElapsedTimeMS >= 0.75f * maxTimeInMS && timeState < TimeState.ThreeFourthTime)
            {
                HandleTimeStateChange(TimeState.ThreeFourthTime);
            }

            double countdownTime = Math.Abs((maxTimeInMS - totalElapsedTimeMS) / 1000d);
            minutesLeft = Math.Floor(countdownTime / 60);
            secondsLeft = Math.Round(countdownTime % 60);

            //check game state
            //if win then
            //CheckWinLose()
            //show win toast
            //if lose then
            //show lose toast
            //fade to black
            //show restart screen

            base.Update(gameTime);
        }
    }
}