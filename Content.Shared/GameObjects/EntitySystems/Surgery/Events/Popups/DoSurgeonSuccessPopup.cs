using Robust.Shared.GameObjects;

namespace Content.Shared.GameObjects.EntitySystems.Surgery.Events.Popups
{
    public class DoSurgeonSuccessPopup : EntityEventArgs
    {
        public IEntity Surgeon { get; }
        public IEntity? Target { get; }
        public IEntity Part { get; }
        public string Id { get; }

        public DoSurgeonSuccessPopup(IEntity surgeon, IEntity? target, IEntity part, string id)
        {
            Surgeon = surgeon;
            Target = target;
            Part = part;
            Id = id;
        }
    }
}