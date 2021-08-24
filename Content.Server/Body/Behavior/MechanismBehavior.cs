#nullable enable
using Content.Shared.Body;
using Content.Shared.Body.Mechanism;
using Content.Shared.Body.Part;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server.Body.Behavior
{
    public abstract class MechanismBehavior : SharedMechanismBehavior
    {
        private SharedMechanismComponent _parent = default!;

        public override SharedBodyComponent? Body => Server.Body.Part?.Body;

        public override SharedBodyPartComponent? Part => Parent.Part;

        public override SharedMechanismComponent Parent => _parent;

        public override IEntity Owner => Parent.Owner;

        public override void Initialize(SharedMechanismComponent parent)
        {
            _parent = parent;
        }

        public override void Startup()
        {
            if (Server.Body.Part == null)
            {
                return;
            }

            if (Server.Body == null)
            {
                AddedToPart(Server.Body.Part);
            }
            else
            {
                AddedToPartInBody(Server.Body, Server.Body.Part);
            }
        }

        public override void Update(float frameTime) { }

        public override void AddedToBody(SharedBodyComponent body)
        {
            DebugTools.AssertNotNull(Server.Body);
            DebugTools.AssertNotNull(body);

            OnAddedToBody(body);
        }

        public override void AddedToPart(SharedBodyPartComponent part)
        {
            DebugTools.AssertNotNull(Server.Body.Part);
            DebugTools.AssertNotNull(part);

            OnAddedToPart(part);
        }

        public override void AddedToPartInBody(SharedBodyComponent body, SharedBodyPartComponent part)
        {
            DebugTools.AssertNotNull(Server.Body);
            DebugTools.AssertNotNull(body);
            DebugTools.AssertNotNull(Server.Body.Part);
            DebugTools.AssertNotNull(part);

            OnAddedToPartInBody(body, part);
        }

        public override void RemovedFromBody(SharedBodyComponent old)
        {
            DebugTools.AssertNull(Server.Body);
            DebugTools.AssertNotNull(old);

            OnRemovedFromBody(old);
        }

        public override void RemovedFromPart(SharedBodyPartComponent old)
        {
            DebugTools.AssertNull(Server.Body.Part);
            DebugTools.AssertNotNull(old);

            OnRemovedFromPart(old);
        }

        public override void RemovedFromPartInBody(SharedBodyComponent oldBody, SharedBodyPartComponent oldPart)
        {
            DebugTools.AssertNull(Server.Body);
            DebugTools.AssertNull(Server.Body.Part);
            DebugTools.AssertNotNull(oldBody);
            DebugTools.AssertNotNull(oldPart);

            OnRemovedFromPartInBody(oldBody, oldPart);
        }

        protected virtual void OnAddedToBody(SharedBodyComponent body) { }

        protected virtual void OnAddedToPart(SharedBodyPartComponent part) { }

        protected virtual void OnAddedToPartInBody(SharedBodyComponent body, SharedBodyPartComponent part) { }

        protected virtual void OnRemovedFromBody(SharedBodyComponent old) { }

        protected virtual void OnRemovedFromPart(SharedBodyPartComponent old) { }

        protected virtual void OnRemovedFromPartInBody(SharedBodyComponent oldBody, SharedBodyPartComponent oldPart) { }
    }
}
