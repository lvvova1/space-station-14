#nullable enable
using System;
using System.Collections.Generic;
using Content.Shared.Body.Surgery.Operation.Step;
using Content.Shared.NetIDs;
using Robust.Shared.GameObjects;
using Robust.Shared.Players;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Shared.Body.Surgery.Tool
{
    [RegisterComponent]
    public class SurgeryToolComponent : Component
    {
        public override string Name => "SurgeryTool";
        public override uint? NetID => ContentNetIDs.SURGERY_TOOL;

        [DataField("delay")]
        private float _delay;

        public float Delay
        {
            get => _delay;
            set => this.SetAndDirtyIfChanged(ref _delay, value);
        }

        [DataField("steps", required: true, customTypeSerializer: typeof(PrototypeIdHashSetSerializer<SurgeryStepPrototype>))]
        public HashSet<string> Steps { get; } = new();

        public override ComponentState GetComponentState(ICommonSession player)
        {
            return new SurgeryToolComponentState(_delay);
        }

        public override void HandleComponentState(ComponentState? curState, ComponentState? nextState)
        {
            base.HandleComponentState(curState, nextState);

            if (curState is not SurgeryToolComponentState state)
            {
                return;
            }

            _delay = state.Delay;
        }
    }

    [Serializable, NetSerializable]
    public class SurgeryToolComponentState : ComponentState
    {
        public SurgeryToolComponentState(float delay) : base(ContentNetIDs.SURGERY_TOOL)
        {
            Delay = delay;
        }

        public float Delay { get; }
    }
}
