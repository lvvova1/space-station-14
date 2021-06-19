using Robust.Shared.GameObjects;

namespace Content.Shared.Body.Surgery.Events
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
