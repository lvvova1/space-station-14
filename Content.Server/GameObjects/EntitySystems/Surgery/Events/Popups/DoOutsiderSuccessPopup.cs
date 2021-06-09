using Robust.Shared.GameObjects;

namespace Content.Server.GameObjects.EntitySystems.Surgery.Events.Popups
{
    public class DoOutsiderSuccessPopup : EntityEventArgs
    {
        public IEntity Surgeon { get; }
        public IEntity? Target { get; }
        public IEntity Part { get; }
        public string Id { get; }

        public DoOutsiderSuccessPopup(IEntity surgeon, IEntity? target, IEntity part, string id)
        {
            Surgeon = surgeon;
            Target = target;
            Part = part;
            Id = id;
        }
    }
}
