using GD.App;
using GD.Engine.Events;
using JigLibX.Geometry;

namespace GD.Engine
{
    public class NoteCollider : InteractibleCollider
    {
        public NoteCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleInteraction()
        {
            // Raise event to show subtitles
            object[] parameters = { DialogueState.NoteInOffice };
            EventDispatcher.Raise(new EventData(EventCategoryType.UI, EventActionType.OnShowSubtitles, parameters));
        }
    }
}