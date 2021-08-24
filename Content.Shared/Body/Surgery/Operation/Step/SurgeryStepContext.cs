using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.Body.Surgery.Target;

namespace Content.Shared.Body.Surgery.Operation.Step
{
    public class SurgeryStepContext
    {
        public SurgeryStepContext(
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryToolComponent tool,
            SharedSurgerySystem surgerySystem,
            OperationStep step)
        {
            Surgery.Surgeon = surgeon;
            Surgery.Target = target;
            Shared.Tool = tool;
            SurgerySystem = surgerySystem;
            Operation.Step = step;
        }

        public SurgeonComponent Surgeon { get; }

        public SurgeryTargetComponent Target { get; }

        public SurgeryToolComponent Tool { get; }

        public SharedSurgerySystem SurgerySystem { get; }

        public OperationStep Step { get; }
    }
}
