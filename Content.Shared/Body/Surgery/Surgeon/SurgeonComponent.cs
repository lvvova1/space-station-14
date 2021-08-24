using System.Threading;
using Content.Shared.Body.Mechanism;
using Content.Shared.Body.Part;
using Content.Shared.Body.Surgery.Target;
using Content.Shared.NetIDs;
using Robust.Shared.GameObjects;
using Robust.Shared.Players;

Surgeon
{
    [RegisterComponent]
    public class SurgeonComponent : Component
    {
        public override string Name => "Surgeon";
        public override uint? NetID => ContentNetIDs.SURGEON;

        private SurgeryTargetComponent? _target;
        private SharedMechanismComponent? _mechanism;

        public SurgeryTargetComponent? Target
        {
            get => _target;
            set => this.SetAndDirtyIfChanged(ref _target, value);
        }

        public SharedMechanismComponent? Mechanism
        {
            get => _mechanism;
            set => this.SetAndDirtyIfChanged(ref _mechanism, value);
        }

        public CancellationTokenSource? SurgeryCancellation { get; set; }

        public SharedBodyComponent? TargetedBody => _target == null
            ? null
            : _target.Owner.GetComponentOrNull<SharedBodyComponent>() ??
              _target.Owner.GetComponentOrNull<SharedBodyPartComponent>()?.Body;

        public override ComponentState GetComponentState(ICommonSession player)
        {
            return new SurgeonComponentState(_target?.Owner.Uid, _mechanism?.Owner.Uid);
        }

        public override void HandleComponentState(ComponentState? curState, ComponentState? nextState)
        {
            base.HandleComponentState(curState, nextState);

            if (curState is not SurgeonComponentState state)
            {
                return;
            }

            _target = state.Target == null
                ? null
                : Owner.EntityManager.GetEntity(state.Target.Value).GetComponent<SurgeryTargetComponent>();
        }
    }
}
