using Content.Shared.GameObjects.Components.Surgery;
using Robust.Shared.GameObjects;

namespace Content.Shared.GameObjects.EntitySystems.Surgery.Events
{
    public class CheckCanAddSurgeryTagEvent : EntityEventArgs
    {
        public CheckCanAddSurgeryTagEvent(SurgeryTag tag)
        {
            Tag = tag;
        }

        public SurgeryTag Tag { get; }

        public bool CanAdd { get; set; }
    }
}
