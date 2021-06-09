using Content.Shared.GameObjects.Components.Surgery;
using Robust.Shared.GameObjects;

namespace Content.Shared.GameObjects.EntitySystems.Surgery.Events
{
    public class TryAddSurgeryTagEvent : EntityEventArgs
    {
        public TryAddSurgeryTagEvent(SurgeryTag tag)
        {
            Tag = tag;
        }

        public SurgeryTag Tag { get; }

        public bool Added { get; set; }
    }
}
