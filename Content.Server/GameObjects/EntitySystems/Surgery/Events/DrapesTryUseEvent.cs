using Content.Shared.GameObjects.Components.Surgery.Operation;
using Content.Shared.GameObjects.Components.Surgery.Surgeon;
using Content.Shared.GameObjects.Components.Surgery.Target;
using Robust.Shared.GameObjects;

namespace Content.Server.GameObjects.EntitySystems.Surgery.Events
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
