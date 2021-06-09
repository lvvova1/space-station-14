using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Content.Shared.GameObjects.Components.Body;
using Content.Shared.GameObjects.Components.Body.Part;
using Content.Shared.GameObjects.Components.Surgery;
using Content.Shared.GameObjects.Components.Surgery.Operation;
using Content.Shared.GameObjects.Components.Surgery.Operation.Messages;
using Content.Shared.GameObjects.Components.Surgery.Operation.Step;
using Content.Shared.GameObjects.Components.Surgery.Surgeon;
using Content.Shared.GameObjects.Components.Surgery.Surgeon.Messages;
using Content.Shared.GameObjects.Components.Surgery.Target;
using Content.Shared.GameObjects.EntitySystems.Surgery.Events;
using Content.Shared.GameObjects.EntitySystems.Surgery.Events.Popups;
using Content.Shared.Interfaces;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;

namespace Content.Shared.GameObjects.EntitySystems.Surgery
{
    [UsedImplicitly]
    public class SharedSurgerySystem : EntitySystem
    {
        public const string SurgeryLogId = "surgery";

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ILocalizationManager _loc = default!;

        protected ISawmill Sawmill { get; private set; } = default!;

        public override void Initialize()
        {
            base.Initialize();

            Sawmill = Logger.GetSawmill(SurgeryLogId);

            ValidateOperations();

            SubscribeLocalEvent<SurgeryTargetComponent, ComponentRemove>(OnTargetComponentRemoved);

            SubscribeLocalEvent<SurgeonComponent, SurgeonStartedOperation>(OnSurgeonStartedOperation);
            SubscribeLocalEvent<SurgeonComponent, SurgeonStoppedOperation>(OnSurgeonStoppedOperation);

            SubscribeLocalEvent<SurgeryTargetComponent, CheckCanAddSurgeryTagEvent>(OnCanAddSurgeryTag);
            SubscribeLocalEvent<SurgeryTargetComponent, TryAddSurgeryTagEvent>(OnTryAddSurgeryTag);
            SubscribeLocalEvent<SurgeryTargetComponent, OperationEnded>(HandleOperationEnded);
            SubscribeLocalEvent<SurgeryTargetComponent, GetPopupReceiverEvent>(OnGetPopupReceiver);

            SubscribeLocalEvent<DoSurgeonBeginPopupEvent>(DoSurgeonBeginPopup);
            SubscribeLocalEvent<DoTargetBeginPopupEvent>(DoTargetBeginPopup);
            SubscribeLocalEvent<DoSurgeonSuccessPopup>(DoSurgeonSuccessPopup);
            SubscribeLocalEvent<DoTargetSuccessPopup>(DoTargetSuccessPopup);
        }

        private void OnTargetComponentRemoved(EntityUid uid, SurgeryTargetComponent target, ComponentRemove args)
        {
            if (target.Surgeon == null || target.Operation == null)
            {
                return;
            }

            StopSurgery(target.Surgeon, target);
        }

        private void OnGetPopupReceiver(EntityUid uid, SurgeryTargetComponent target, GetPopupReceiverEvent args)
        {
            if (target.Owner.TryGetComponent(out IBodyPart? part) &&
                part.Body?.Owner != null)
            {
                args.Receiver = part.Body.Owner;
            }

            args.Receiver = target.Owner;
        }

        private void OnSurgeonStartedOperation(EntityUid uid, SurgeonComponent surgeon, SurgeonStartedOperation args)
        {
            args.Target.Surgeon = EntityManager.GetEntity(uid).GetComponent<SurgeonComponent>();
            args.Target.Operation = args.Operation;
        }

        private void OnSurgeonStoppedOperation(EntityUid uid, SurgeonComponent surgeon, SurgeonStoppedOperation args)
        {
            surgeon.SurgeryCancellation?.Cancel();
            surgeon.SurgeryCancellation = null;
            surgeon.Target = null;

            args.OldTarget.Surgeon = null;
            args.OldTarget.Operation = null;
            args.OldTarget.SurgeryTags.Clear();
        }

        private void HandleOperationEnded(EntityUid uid, SurgeryTargetComponent target, OperationEnded args)
        {
            target.Surgeon = null;
            target.Operation = null;
        }

        private void ValidateOperations()
        {
            foreach (var operation in _prototypeManager.EnumeratePrototypes<SurgeryOperationPrototype>())
            {
                foreach (var step in operation.Steps)
                {
                    if (!_prototypeManager.HasIndex<SurgeryStepPrototype>(step.Id))
                    {
                        throw new PrototypeLoadException(
                            $"Invalid {nameof(SurgeryStepPrototype)} found in surgery operation with id {operation.ID}: No step found with id {step}");
                    }
                }
            }
        }

        private CancellationTokenSource StartSurgery(
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryOperationPrototype operation)
        {
            StopSurgery(surgeon);

            surgeon.Target = target;

            var cancellation = new CancellationTokenSource();
            surgeon.SurgeryCancellation = cancellation;

            var message = new SurgeonStartedOperation(target, operation);
            RaiseLocalEvent(surgeon.Owner.Uid, message);

            return cancellation;
        }

        private bool TryStartSurgery(
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryOperationPrototype operation,
            [NotNullWhen(true)] out CancellationTokenSource? token)
        {
            if (surgeon.Target != null)
            {
                token = null;
                return false;
            }

            token = StartSurgery(surgeon, target, operation);
            return true;
        }

        protected bool TryStartSurgery(
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryOperationPrototype operation)
        {
            return TryStartSurgery(surgeon, target, operation, out _);
        }

        protected bool IsPerformingSurgery(SurgeonComponent surgeon)
        {
            return surgeon.Target != null;
        }

        protected bool IsPerformingSurgeryOn(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (surgeon.Target == target)
            {
                return true;
            }

            if (target.Owner.TryGetComponent(out IBodyPart? part) &&
                part.Body?.Owner == target.Owner)
            {
                return true;
            }

            return false;
        }

        protected bool IsPerformingSurgeryOnSelf(SurgeonComponent surgeon)
        {
            return surgeon.Target != null && IsPerformingSurgeryOn(surgeon, surgeon.Target);
        }

        protected bool IsReceivingSurgeryOnPart(IEntity target)
        {
            return IsReceivingSurgeryOnPart(target, out _, out _);
        }

        protected bool IsReceivingSurgeryOnPart(
            IEntity target,
            [NotNullWhen(true)] out IBodyPart? part,
            [NotNullWhen(true)] out IBody? body)
        {
            body = null;
            return target.TryGetComponent(out part) && (body = part.Body) != null;
        }

        /// <summary>
        ///     Tries to stop the surgery that the surgeon is performing.
        /// </summary>
        /// <returns>True if stopped, false otherwise even if no surgery was underway.</returns>
        protected bool StopSurgery(SurgeonComponent surgeon)
        {
            if (surgeon.Target == null)
            {
                return false;
            }

            var oldTarget = surgeon.Target;
            surgeon.Target = null;

            if (!surgeon.Owner.Deleted)
            {
                var message = new SurgeonStoppedOperation(oldTarget);
                RaiseLocalEvent(surgeon.Owner.Uid, message);
            }

            return true;
        }

        private bool StopSurgery(SurgeonComponent surgeon, SurgeryTargetComponent target)
        {
            if (surgeon.Target != target)
            {
                return false;
            }

            return StopSurgery(surgeon);
        }

        private bool CanAddSurgeryTag(SurgeryTargetComponent target, SurgeryTag tag)
        {
            if (target.Operation == null ||
                target.Operation.Steps.Count <= target.SurgeryTags.Count)
            {
                return false;
            }

            var nextStep = target.Operation.Steps[target.SurgeryTags.Count];
            if (!nextStep.Necessary(target) || nextStep.Id != tag.Id)
            {
                return false;
            }

            return true;
        }

        private void OnCanAddSurgeryTag(EntityUid uid, SurgeryTargetComponent target, CheckCanAddSurgeryTagEvent args)
        {
            args.CanAdd = CanAddSurgeryTag(target, args.Tag);
        }

        private void OnTryAddSurgeryTag(EntityUid uid, SurgeryTargetComponent target, TryAddSurgeryTagEvent args)
        {
            var msg = new CheckCanAddSurgeryTagEvent(args.Tag);
            EntityManager.EventBus.RaiseLocalEvent(uid, msg);

            if (!msg.CanAdd)
            {
                args.Added = false;
                return;
            }

            target.SurgeryTags.Add(args.Tag);
            CheckCompletion(target);

            args.Added = true;
        }

        private bool TryRemoveSurgeryTag(SurgeryTargetComponent target, SurgeryTag tag)
        {
            if (target.SurgeryTags.Count == 0 ||
                target.SurgeryTags[^1] != tag)
            {
                return false;
            }

            target.SurgeryTags.RemoveAt(target.SurgeryTags.Count - 1);
            return true;
        }

        private void CheckCompletion(SurgeryTargetComponent target)
        {
            if (target.Surgeon == null ||
                target.Operation == null ||
                target.Operation.Steps.Count > target.SurgeryTags.Count)
            {
                return;
            }

            var offset = 0;

            for (var i = 0; i < target.SurgeryTags.Count; i++)
            {
                var step = target.Operation.Steps[i + offset];

                if (!step.Necessary(target))
                {
                    offset++;
                    step = target.Operation.Steps[i + offset];
                }

                var tag = target.SurgeryTags[i];

                if (tag != step.Id)
                {
                    return;
                }
            }

            target.Operation.Effect?.Execute(target.Surgeon, target);
        }

        public void DoSurgeonBeginPopup(DoSurgeonBeginPopupEvent ev)
        {
            string msg;

            if (ev.Target == null)
            {
                var locId = $"surgery-step-{ev.Id}-begin-no-zone-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Part));
            }
            else if (ev.Surgeon == ev.Target)
            {
                var locId = $"surgery-step-{ev.Id}-begin-self-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }
            else
            {
                var locId = $"surgery-step-{ev.Id}-begin-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }

            ev.Surgeon.PopupMessage(msg);
        }

        public void DoTargetBeginPopup(DoTargetBeginPopupEvent ev)
        {
            var locId = $"surgery-step-{ev.Id}-begin-target-popup";
            var msg = _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Target));

            ev.Target.PopupMessage(msg);
        }

        public void DoSurgeonSuccessPopup(DoSurgeonSuccessPopup ev)
        {
            string msg;

            if (ev.Target == null)
            {
                var locId = $"surgery-step-{ev.Id}-success-no-zone-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Part));
            }
            else if (ev.Surgeon == ev.Target)
            {
                var locId = $"surgery-step-{ev.Id}-success-self-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }
            else
            {
                var locId = $"surgery-step-{ev.Id}-success-surgeon-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }

            ev.Surgeon.PopupMessage(msg);
        }

        public void DoTargetSuccessPopup(DoTargetSuccessPopup ev)
        {
            var locId = $"surgery-step-{ev.Id}-success-target-popup";
            var msg = _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Part));

            ev.Part.PopupMessage(msg);
        }
    }

    public class PerformingSurgeryCheckEvent : EntityEventArgs
    {
        public PerformingSurgeryCheckEvent(SurgeryTargetComponent target)
        {
            Target = target;
        }

        public SurgeryTargetComponent Target { get; }

        public bool Performing { get; set; }
    }

    public class PerformingSurgeryOnSelfCheckEvent : EntityEventArgs
    {
        public bool PerformingOnSelf { get; set; }
    }

    public class TryStopSurgeryEvent : EntityEventArgs
    {
        public TryStopSurgeryEvent(SurgeryTargetComponent target)
        {
            Target = target;
        }

        public SurgeryTargetComponent Target { get; }

        public bool Stopped { get; set; }
    }

    public class GetPopupReceiverEvent : EntityEventArgs
    {
        public IEntity? Receiver { get; set; }
    }
}
