using Robust.Shared.GameObjects;

namespace Content.Shared.Body.Surgery.Events
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
