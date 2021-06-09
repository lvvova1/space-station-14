using Robust.Shared.GameObjects;

namespace Content.Shared.GameObjects.EntitySystems.Surgery.Events.Popups
{
    public class DoTargetBeginPopupEvent : EntityEventArgs
    {
        public IEntity Surgeon { get; }
        public IEntity Target { get; }
        public string Id { get; }

        public DoTargetBeginPopupEvent(IEntity surgeon, IEntity target, string id)
        {
            Surgeon = surgeon;
            Target = target;
            Id = id;
        }
    }
}
