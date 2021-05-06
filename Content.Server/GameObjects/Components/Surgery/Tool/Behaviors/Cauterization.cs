using Content.Server.Utility;
using Content.Shared.GameObjects.Components.Body.Part;
using Content.Shared.GameObjects.Components.Surgery.Operation.Step;
using Content.Shared.GameObjects.Components.Surgery.Surgeon;
using Content.Shared.GameObjects.Components.Surgery.Target;
using Content.Shared.GameObjects.EntitySystems;
using Content.Shared.Interfaces;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.GameObjects.Components.Surgery.Tool.Behaviors
{
    public class Cauterization : ISurgeryBehavior
    {
        private SharedSurgerySystem SurgerySystem => EntitySystem.Get<SharedSurgerySystem>();

        [DataField("locId")]
        private string? LocId { get; } = null;

        public bool CanPerform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            return SurgerySystem.IsPerformingSurgeryOn(surgeon, target);
        }

        public bool Perform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            return SurgerySystem.StopSurgery(surgeon, target);
        }

        public void OnPerformDelayBegin(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (LocId == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;
            var targetReceiver = EntitySystem.Get<SharedSurgerySystem>().GetPopupReceiver(target);

            surgeonOwner.PopupMessage(SurgeryStepPrototype.SurgeonBeginPopup(surgeonOwner, targetReceiver, target.Owner, LocId));

            if (!SurgerySystem.IsPerformingSurgeryOnSelf(surgeon))
            {
                targetReceiver.PopupMessage(SurgeryStepPrototype.TargetBeginPopup(surgeonOwner, target.Owner, LocId));
            }

            surgeonOwner.PopupMessageOtherClients(SurgeryStepPrototype.OutsiderBeginPopup(surgeonOwner, targetReceiver, target.Owner, LocId), except: targetReceiver);
        }

        public void OnPerformSuccess(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (LocId == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;
            var bodyOwner = target.Owner.GetComponentOrNull<IBodyPart>()?.Body?.Owner ?? target.Owner;

            surgeonOwner.PopupMessage(SurgeryStepPrototype.SurgeonSuccessPopup(surgeonOwner, bodyOwner, target.Owner, LocId));

            if (bodyOwner != surgeonOwner)
            {
                bodyOwner.PopupMessage(SurgeryStepPrototype.TargetSuccessPopup(surgeonOwner, bodyOwner, LocId));
            }

            surgeonOwner.PopupMessageOtherClients(SurgeryStepPrototype.OutsiderSuccessPopup(surgeonOwner, bodyOwner, target.Owner, LocId), except: bodyOwner);
        }
    }
}
