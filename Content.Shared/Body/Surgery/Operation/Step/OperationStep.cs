using Content.Shared.Body.Surgery.Operation.Step.Behavior;
using Content.Shared.Body.Surgery.Operation.Step.Conditional;
using Content.Shared.Body.Surgery.Target;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Body.Surgery.Operation.Step
{
    [DataDefinition]
    public class OperationStep
    {
        [DataField("id", required: true)]
        public string Id { get; init; } = string.Empty;

        [DataField("conditional")]
        public IOperationStepConditional? Conditional { get; init; }

        [DataField("behavior", required: true, serverOnly: true)]
        public IStepBehavior? Behavior { get; init; }

        public bool Necessary(SurgeryTargetComponent target)
        {
            return Step.Conditional?.Necessary(target) ?? true;
        }

        public bool CanPerform(SurgeryStepContext context)
        {
            return Step.Behavior?.CanPerform(context) ?? false;
        }

        public bool Perform(SurgeryStepContext context)
        {
            return Step.Behavior?.Perform(context) ?? false;
        }

        public void OnPerformDelayBegin(SurgeryStepContext context)
        {
            Step.Behavior?.OnPerformDelayBegin(context);
        }

        public void OnPerformSuccess(SurgeryStepContext context)
        {
            Step.Behavior?.OnPerformSuccess(context);
        }

        public void OnPerformFail(SurgeryStepContext context)
        {
            Step.Behavior?.OnPerformFail(context);
        }
    }
}
