using Content.Server.Body.Surgery.Events.Popups;
using Content.Shared.Body.Part;
using Content.Shared.Body.Surgery.Events;
using Content.Shared.Body.Surgery.Events.Popups;
using Content.Shared.Body.Surgery.Operation.Step;
using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.Body.Surgery.Target;
using Content.Shared.Notification.Managers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Body.Surgery.Tool.Behaviors
{
    public class StepSurgery : ISurgeryBehavior
    {
        [field: DataField("step", customTypeSerializer: typeof(PrototypeIdSerializer<SurgeryStepPrototype>))]
        private string? StepId { get; } = default;

        public SurgeryStepPrototype? Step => StepId == null
            ? null
            : IoCManager.Resolve<IPrototypeManager>().Index<SurgeryStepPrototype>(StepId);

        public bool CanPerform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (StepId == null)
            {
                return false;
            }

            var ev = new CheckCanAddSurgeryTagEvent(StepId);
            target.Owner.EntityManager.EventBus.RaiseLocalEvent(target.Owner.Uid, ev);

            return ev.CanAdd;
        }

        public bool Perform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            var step = Step;

            if (step == null)
            {
                return false;
            }

            var ev = new TryAddSurgeryTagEvent(step.ID);
            target.Owner.EntityManager.EventBus.RaiseLocalEvent(target.Owner.Uid, ev);

            return ev.Added;
        }

        public void OnPerformDelayBegin(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            var step = Step;

            if (step == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;
            var bodyOwner = target.Owner.GetComponentOrNull<SharedBodyPartComponent>()?.Body?.Owner ?? target.Owner;
            var eventBus = target.Owner.EntityManager.EventBus;

            var surgeonPopup = new DoSurgeonBeginPopupEvent(surgeonOwner, bodyOwner, target.Owner, step.LocId);
            eventBus.RaiseEvent(EventSource.Local, surgeonPopup);

            if (bodyOwner != surgeonOwner)
            {
                var targetPopup = new DoTargetBeginPopupEvent(surgeonOwner, bodyOwner, step.LocId);
                eventBus.RaiseEvent(EventSource.Local, targetPopup);
            }

            var outsiderPopup = new DoOutsiderBeginPopupEvent(surgeonOwner, bodyOwner, target.Owner, step.LocId);
            eventBus.RaiseEvent(EventSource.Local, outsiderPopup);
        }

        public void OnPerformSuccess(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            var step = Step;

            if (step == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;
            var bodyOwner = target.Owner.GetComponentOrNull<SharedBodyPartComponent>()?.Body?.Owner ?? target.Owner;
            var eventBus = target.Owner.EntityManager.EventBus;

            var surgeonPopup = new DoSurgeonSuccessPopup(surgeonOwner, bodyOwner, target.Owner, step.LocId);
            eventBus.RaiseEvent(EventSource.Local, surgeonPopup);

            if (bodyOwner != surgeonOwner)
            {
                var targetPopup = new DoTargetSuccessPopup(surgeonOwner, bodyOwner, step.LocId);
                eventBus.RaiseEvent(EventSource.Local, targetPopup);
            }

            var outsiderPopup = new DoOutsiderSuccessPopup(surgeonOwner, bodyOwner, target.Owner, step.LocId);
            eventBus.RaiseEvent(EventSource.Local, outsiderPopup);
        }

        public void OnPerformFail(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            surgeon.Owner.PopupMessage(Loc.GetString("surgery-step-not-useful"));
        }
    }
}
