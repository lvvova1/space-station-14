using Content.Shared.Body.Part;
using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.Body.Surgery.Target;

namespace Content.Shared.Body.Surgery.Operation.Effect
{
    public class OrganExtractionEffect : IOperationEffect
    {
        public void Execute(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (surgeon.Mechanism == null ||
                !target.Owner.TryGetComponent(out SharedBodyPartComponent? part) ||
                !part.HasMechanism(surgeon.Mechanism))
            {
                return;
            }

            part.RemoveMechanism(surgeon.Mechanism);
        }
    }
}
