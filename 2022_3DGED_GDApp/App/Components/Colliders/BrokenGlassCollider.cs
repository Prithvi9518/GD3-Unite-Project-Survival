using GD.Engine;
using GD.Engine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD.App
{
    public class BrokenGlassCollider : Collider
    {
        private bool steppedOn = false;

        public BrokenGlassCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleResponse(GameObject parentGameObject)
        {
            InteractionController controller = parentGameObject.GetComponent<InteractionController>();

            if (controller == null)
            {
                if (parentGameObject.Name == AppData.FIRST_PERSON_CAMERA_NAME)
                    System.Diagnostics.Debug.WriteLine("InteractionController not found.");
                return;
            }

            // If collided with player, play glass breaking sound
            if (!steppedOn)
            {
                object[] parameters = { AppData.GLASS_SHATTER_SOUND_NAME };
                EventDispatcher.Raise(new EventData(EventCategoryType.Sound, EventActionType.OnPlay2D, parameters));

                steppedOn = true;
            }
        }
    }
}