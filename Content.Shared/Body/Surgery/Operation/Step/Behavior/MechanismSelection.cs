namespace Content.Shared.Body.Surgery.Operation.Step.Behavior
{
    public class MechanismSelection : IStepBehavior
    {
        public bool CanPerform(SurgeryStepContext context)
        {
            return context.Surgeon.Mechanism == null;
        }

        public bool Perform(SurgeryStepContext context)
        {
            context.SurgerySystem.OpenChooseMechanismUI(context.Surgeon, context.Target);
            return true;
        }
    }
}
