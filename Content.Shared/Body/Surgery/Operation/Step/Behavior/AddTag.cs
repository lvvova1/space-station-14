using Robust.Shared.Localization;

namespace Content.Shared.Body.Surgery.Operation.Step.Behavior
{
    // TODO SURGERY deserialize only one instance like appearance visualizers
    public class AddTag : IStepBehavior
    {
        public bool CanPerform(SurgeryStepContext context)
        {
            return context.Tool.Steps.Contains(context.Step.Id) &&
                   context.SurgerySystem.CanAddSurgeryTag(context.Target, context.Step.Id);
        }

        public bool Perform(SurgeryStepContext context)
        {
            return context.SurgerySystem.TryAddSurgeryTag(context.Target, context.Step.Id);
        }

        public void OnPerformDelayBegin(SurgeryStepContext context)
        {
            context.SurgerySystem.DoBeginPopups(context.Surgeon, context.Target.Owner, context.Step.Id);
        }

        public void OnPerformSuccess(SurgeryStepContext context)
        {
            context.SurgerySystem.DoSuccessPopups(context.Surgeon, context.Target.Owner, context.Step.Id);
        }

        public void OnPerformFail(SurgeryStepContext context)
        {
            context.Surgeon.Owner.PopupMessage(Loc.GetString("surgery-step-not-useful"));
        }
    }
}
