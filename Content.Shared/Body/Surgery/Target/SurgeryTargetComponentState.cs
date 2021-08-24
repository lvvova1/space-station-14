using System;
using System.Collections.Generic;
using Content.Shared.NetIDs;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Body.Surgery.Target
{
    [Serializable, NetSerializable]
    public class SurgeryTargetComponentState : ComponentState
    {
        public SurgeryTargetComponentState(EntityUid? surgeon, string? operation, List<SurgeryTag> tags)
            : base(ContentNetIDs.SURGERY_TARGET)
        {
            Surgery.Surgeon = surgeon;
            Surgery.Operation = operation;
            Tags = tags;
        }

        public EntityUid? Surgeon { get; }

        public string? Operation { get; }

        public List<SurgeryTag> Tags { get; }
    }
}
