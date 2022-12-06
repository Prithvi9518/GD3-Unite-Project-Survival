#define DEMO_STATES

using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using GD.Engine;
using GD.Engine.Globals;

namespace GD.App
{
    public enum GameState
    {
        Default,
        GeneratorRoomOpen,
        GeneratorOn,
        Win,
        Lose
    }

    public class StateManager : GameComponent
    {
        private double maxTimeInMS;
        private double totalElapsedTimeMS;
        private double minutesLeft;
        private double secondsLeft;
        private bool stopTime = false;

        private GameState currentState;

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
                    HandleStateChange(newState);
                    break;

                case EventActionType.OnReachExit:
                    CheckWin();
                    break;

                default:
                    break;
            }
        }

        private void HandleStateChange(GameState newState)
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

                case GameState.GeneratorOn:

#if DEMO_STATES
                    System.Diagnostics.Debug.WriteLine("Generator is now on");

                    RaiseAlarmEvent(EventActionType.OnPlay2D);
#endif

                    break;

                case GameState.Win:

                    System.Diagnostics.Debug.WriteLine("You Win!");
                    // Alarm sound playing twice for some reason - will fix later
                    RaiseAlarmEvent(EventActionType.OnStop);
                    RaiseAlarmEvent(EventActionType.OnStop);

                    break;

                default:
                    break;
            }
        }

        private bool CheckWin()
        {
            if ((currentState == GameState.GeneratorOn || currentState == GameState.Win)
                && totalElapsedTimeMS < AppData.MAX_GAME_TIME_IN_MSECS)
            {
                HandleStateChange(GameState.Win);
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Need to restore power first!");
            }

            return false;
        }

        private void RaiseAlarmEvent(EventActionType eventActionType)
        {
            object[] parameters = { "alarm-sound" };
            EventDispatcher.Raise(new EventData(EventCategoryType.Sound, eventActionType, parameters));
        }

        public override void Update(GameTime gameTime)
        {
            if (!stopTime)
                totalElapsedTimeMS += gameTime.ElapsedGameTime.Milliseconds;

            if (totalElapsedTimeMS >= maxTimeInMS)
            {
                stopTime = true;
                //object[] parameters = { "You win!", totalElapsedTimeMS, "win_soundcue" };
                //EventDispatcher.Raise(
                //    new EventData(EventCategoryType.Player,
                //    EventActionType.OnLose,
                //    parameters));
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