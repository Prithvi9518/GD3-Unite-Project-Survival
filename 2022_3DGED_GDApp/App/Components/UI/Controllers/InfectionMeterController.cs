using GD.Engine;
using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;

namespace GD.App
{
    public class InfectionMeterController : Component
    {
        #region Fields

        private float currentValue;
        private float minValue;
        private float startValue;

        private TextureMaterial2D textureMaterial2D;

        #endregion Fields

        #region Properties

        public float CurrentValue
        {
            get
            {
                return currentValue;
            }
            set
            {
                currentValue = ((value >= minValue) && (value <= startValue)) ? value : 0;
            }
        }

        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = (value >= 0) ? value : 0;
            }
        }

        public float StartValue
        {
            get
            {
                return startValue;
            }
            set
            {
                startValue = (value >= 0) ? value : 0;
            }
        }

        #endregion Properties

        public InfectionMeterController(float startValue, float minValue)
        {
            StartValue = startValue;
            MinValue = minValue;
            CurrentValue = startValue;

            //listen for UI events to change the SourceRectangle
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleEvents);
        }

        #region Action - Events

        private void HandleEvents(EventData eventData)
        {
            //if (eventData.EventActionType == EventActionType.OnHealthDelta)
            //{
            //    //get the name of the ui object targeted by this event
            //    var targetObjectName = eventData.Parameters[0] as string;

            //    //is it for me?
            //    if (targetObjectName != null && gameObject.Name.Equals(targetObjectName))
            //        CurrentValue = currentValue + (int)eventData.Parameters[1];
            //}

            switch (eventData.EventActionType)
            {
                case EventActionType.OnInfectionDelta:
                    var targetObjectName = eventData.Parameters[0] as string;

                    if (targetObjectName != null && gameObject.Name.Equals(targetObjectName))
                    {
                        CurrentValue = (float)eventData.Parameters[1];
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion Action - Events

        public override void Update(GameTime gameTime)
        {
            //get material to access source rectangle
            if (textureMaterial2D == null)
                textureMaterial2D = gameObject.GetComponent<Renderer2D>().Material as TextureMaterial2D;

            //how much of a percentage of the width of the image does the current value represent?
            var widthMultiplier = minValue + currentValue / startValue;

            //now set the amount of visible rectangle using the current value
            textureMaterial2D.SourceRectangleWidth = (int)Math.Round(widthMultiplier * textureMaterial2D.OriginalSourceRectangle.Width);
        }
    }
}