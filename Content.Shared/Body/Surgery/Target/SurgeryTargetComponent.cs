using System.Collections.Generic;
using Content.Shared.Body.Surgery.Operation;
using Content.Shared.Body.Surgery.Surgeon;
using Content.Shared.NetIDs;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Players;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Shared.Body.Surgery.Target
{
    [RegisterComponent]
    public class SurgeryTargetComponent : Component
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override string Name => "SurgeryTarget";
        public override uint? NetID => ContentNetIDs.SURGERY_TARGET;

        private SurgeonComponent? _surgeon;
        [ViewVariables] private string? _operationId;

        /// <summary>
        ///     Modified in <see cref="SharedSurgerySystem.TryAddSurgeryTag"/> and
        ///     <see cref="SharedSurgerySystem.TryRemoveSurgeryTag"/>
        /// </summary>
        [ViewVariables]
        [DataField("tags")]
        public List<SurgeryTag> SurgeryTags { get; } = new();

        [ViewVariables]
        public SurgeonComponent? Surgeon
        {
            get => _surgeon;
            set => this.SetAndDirtyIfChanged(ref _surgeon, value);
        }

        [ViewVariables]
        public SurgeryOperationPrototype? Operation
        {
            get => _operationId == null
                ? null
                : _prototypeManager.Index<SurgeryOperationPrototype>(_operationId);
            set => this.SetAndDirtyIfChanged(ref _operationId, value?.ID);
        }

        [ViewVariables]
        public IEnumerable<SurgeryOperationPrototype> PossibleSurgeries =>
            _prototypeManager.EnumeratePrototypes<SurgeryOperationPrototype>();

        public override ComponentState GetComponentState(ICommonSession player)
        {
            return new SurgeryTargetComponentState(_surgeon?.Owner.Uid, _operationId, SurgeryTags);
        }

        public override void HandleComponentState(ComponentState? curState, ComponentState? nextState)
        {
            base.HandleComponentState(curState, nextState);

            if (curState is not SurgeryTargetComponentState state)
            {
                return;
            }

            _surgeon = state.Surgeon == null
                ? null
                : Owner.EntityManager
                    .GetEntity(state.Surgeon.Value)
                    .EnsureComponent<SurgeonComponent>();

            _operationId = state.Operation;

            SurgeryTags.Clear();
            SurgeryTags.AddRange(state.Tags);
        }
    }
}
