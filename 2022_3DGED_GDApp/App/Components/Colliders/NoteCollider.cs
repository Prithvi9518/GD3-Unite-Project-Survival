using GD.App;
using GD.Engine.Events;
using JigLibX.Geometry;

namespace GD.Engine
{
    public class NoteCollider : InteractibleCollider
    {
        private bool noteShown = false;

        public NoteCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            object[] parameters;
            if (!noteShown)
            {
                parameters = new object[] { true };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnToggleNote, parameters));

                noteShown = true;
            }
            else
            {
                parameters = new object[] { false };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnToggleNote, parameters));

                // Raise event to show subtitles
                parameters = new object[] { DialogueState.NoteInOffice };
                EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnTriggerDialogue, parameters));

                noteShown = false;
            }
        }
    }
}