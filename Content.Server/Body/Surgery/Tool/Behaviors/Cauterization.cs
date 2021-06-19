using Content.Server.Body.Surgery.Events.Popups;
using Content.Shared.Body.Part;
using Content.Shared.Body.Surgery;
using Content.Shared.Body.Surgery.Events.Popups;
using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.Body.Surgery.Target;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Body.Surgery.Tool.Behaviors
{
    public class Cauterization : ISurgeryBehavior
    {
        private IEventBus EventBus => IoCManager.Resolve<IEntityManager>().EventBus;

        [DataField("locId")]
        private string? LocId { get; } = null;

        public bool CanPerform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            var msg = new PerformingSurgeryCheckEvent(target);
            EventBus.RaiseLocalEvent(surgeon.Owner.Uid, msg);

            return msg.Performing;
        }

        public bool Perform(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            var msg = new TryStopSurgeryEvent(target);
            EventBus.RaiseLocalEvent(surgeon.Owner.Uid, msg);

            return msg.Stopped;
        }

        public void OnPerformDelayBegin(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (LocId == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;

            var receiverMsg = new GetPopupReceiverEvent();
            EventBus.RaiseLocalEvent(target.Owner.Uid, receiverMsg);
            var targetReceiver = receiverMsg.Receiver;

            var surgeonMsg = new DoSurgeonBeginPopupEvent(surgeonOwner, targetReceiver, target.Owner, LocId);
            EventBus.RaiseEvent(EventSource.Local, surgeonMsg);

            var performingMsg = new PerformingSurgeryOnSelfCheckEvent();
            EventBus.RaiseLocalEvent(surgeon.Owner.Uid, performingMsg);

            if (!performingMsg.PerformingOnSelf)
            {
                var targetMsg = new DoTargetBeginPopupEvent(surgeonOwner, target.Owner, LocId);
                EventBus.RaiseEvent(EventSource.Local, targetMsg);
            }

            var othersMsg = new DoOutsiderBeginPopupEvent(surgeonOwner, targetReceiver, target.Owner, LocId);
            EventBus.RaiseEvent(EventSource.Local, othersMsg);
        }

        public void OnPerformSuccess(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (LocId == null)
            {
                return;
            }

            var surgeonOwner = surgeon.Owner;
            var bodyOwner = target.Owner.GetComponentOrNull<SharedBodyPartComponent>()?.Body?.Owner ?? target.Owner;
            var eventBus = target.Owner.EntityManager.EventBus;

            var surgeonPopup = new DoSurgeonSuccessPopup(surgeonOwner, bodyOwner, target.Owner, LocId);
            eventBus.RaiseEvent(EventSource.Local, surgeonPopup);

            if (bodyOwner != surgeonOwner)
            {
                var targetPopup = new DoTargetSuccessPopup(surgeonOwner, bodyOwner, LocId);
                eventBus.RaiseEvent(EventSource.Local, targetPopup);
            }

            var outsiderPopup = new DoOutsiderSuccessPopup(surgeonOwner, bodyOwner, target.Owner, LocId);
            eventBus.RaiseEvent(EventSource.Local, outsiderPopup);
        }
    }
}
