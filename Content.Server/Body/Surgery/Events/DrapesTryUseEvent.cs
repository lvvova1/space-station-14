using Content.Shared.Body.Surgery.Operation;
using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.Body.Surgery.Target;
using Robust.Shared.GameObjects;

namespace Content.Server.Body.Surgery.Events
{
    public class DrapesTryUseEvent : EntityEventArgs
    {
        public DrapesTryUseEvent(
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryOperationPrototype operation)
        {
            Surgeon = surgeon;
            Target = target;
            Operation = operation;
        }

        public SurgeonComponent Surgeon { get; }
        public SurgeryTargetComponent Target { get; }
        public SurgeryOperationPrototype Operation { get; }
        public bool Used { get; set; }
    }
}
