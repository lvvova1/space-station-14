using Robust.Shared.GameObjects;

namespace Content.Shared.Body.Surgery.Events.Popups
{
    public class DoSurgeonBeginPopupEvent : EntityEventArgs
    {
        public IEntity Surgeon { get; }
        public IEntity? Target { get; }
        public IEntity Part { get; }
        public string Id { get; }

        public DoSurgeonBeginPopupEvent(IEntity surgeon, IEntity? target, IEntity part, string id)
        {
            Surgeon = surgeon;
            Target = target;
            Part = part;
            Id = id;
        }
    }
}
