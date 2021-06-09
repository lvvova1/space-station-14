using Robust.Shared.GameObjects;

namespace Content.Shared.GameObjects.EntitySystems.Surgery.Events.Popups
{
    public class DoTargetSuccessPopup : EntityEventArgs
    {
        public IEntity Surgeon { get; }
        public IEntity Part { get; }
        public string Id { get; }

        public DoTargetSuccessPopup(IEntity surgeon, IEntity part, string id)
        {
            Surgeon = surgeon;
            Part = part;
            Id = id;
        }
    }
}