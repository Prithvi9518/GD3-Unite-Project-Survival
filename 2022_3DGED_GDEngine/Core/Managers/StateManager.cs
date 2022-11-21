using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GD.Engine
{
    public enum ItemType : sbyte
    {
        Story,
        Health,
        Ammo,
        Quest,
        Prop
    }

    public class InventoryItem
    {
        public string uniqueID;
        public string name;
        public ItemType itemType;
        public string description;
        public int value;
        private string audioCueName;  //"boom"
    }

    /// <summary>
    /// Countdown/up timer and we need an inventory system
    /// </summary>
    public class StateManager : GameComponent
    {
        private double maxTimeInMS;
        private double totalElapsedTimeMS;
        private double minutesLeft;
        private double secondsLeft;
        private List<InventoryItem> inventory;
        private bool stopTime = false;

        public double CountdownTimeSecs
        {
            get => secondsLeft;
        }
        public double CountdownTimeMins
        {
            get => minutesLeft;
        }

        public StateManager(Game game, double maxTimeInMS) : base(game)
        {
            this.maxTimeInMS = maxTimeInMS;
            totalElapsedTimeMS = 0;
            inventory = new List<InventoryItem>();

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

        private bool CheckWinLose()
        {
            return false;
            //check individual game items
        }
    }
}