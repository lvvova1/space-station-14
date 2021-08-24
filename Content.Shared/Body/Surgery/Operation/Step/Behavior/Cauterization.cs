namespace Content.Shared.Body.Surgery.Operation.Step.Behavior
{
    public class Cauterization : IStepBehavior
    {
        public bool CanPerform(SurgeryStepContext context)
        {
            return context.SurgerySystem.IsPerformingSurgeryOn(context.Surgeon, context.Target);
        }

        public bool Perform(SurgeryStepContext context)
        {
            return context.SurgerySystem.StopSurgery(context.Surgeon, context.Target);
        }

        public void OnPerformDelayBegin(SurgeryStepContext context)
        {
            context.SurgerySystem.DoBeginPopups(context.Surgeon, context.Target.Owner, context.Step.Id);
        }

        public void OnPerformSuccess(SurgeryStepContext context)
        {
            context.SurgerySystem.DoSuccessPopups(context.Surgeon, context.Target.Owner, context.Step.Id);
        }
    }
}
