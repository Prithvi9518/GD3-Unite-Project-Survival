using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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

        private void SetGameState(GameState gameState)
        {
            this.currentState = gameState;
        }

        private bool CheckWinLose()
        {
            return false;
            //check individual game items
        }
    }
}