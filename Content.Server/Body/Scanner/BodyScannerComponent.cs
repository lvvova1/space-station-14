#nullable enable
using Content.Shared.Body;
using Content.Shared.Body.Scanner;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Server.Body.Scanner
{
    [RegisterComponent]
    [ComponentReference(typeof(IActivate))]
    [ComponentReference(typeof(SharedBodyScannerComponent))]
    public class BodyScannerComponent : SharedBodyScannerComponent, IActivate
    {
        [ViewVariables] private BoundUserInterface? UserInterface => Owner.GetUIOrNull(BodyScannerUiKey.Key);

        void IActivate.Activate(ActivateEventArgs eventArgs)
        {
            if (!eventArgs.User.TryGetComponent(out ActorComponent? actor))
            {
                return;
            }

            var session = actor.PlayerSession;

            if (session.AttachedEntity == null)
            {
                return;
            }

            if (session.AttachedEntity.TryGetComponent(out SharedBodyComponent? body))
            {
                var state = InterfaceState(body);
                Server.UserInterface?.SetState(state);
            }

            Server.UserInterface?.Open(session);
        }

        public override void Initialize()
        {
            base.Initialize();

            Owner.EnsureComponentWarn<ServerUserInterfaceComponent>();

            if (Server.UserInterface != null)
            {
                Server.UserInterface.OnReceiveMessage += UserInterfaceOnOnReceiveMessage;
            }
        }

        private void UserInterfaceOnOnReceiveMessage(ServerBoundUserInterfaceMessage serverMsg) { }

        /// <summary>
        ///     Copy BodyTemplate and BodyPart data into a common data class that the client can read.
        /// </summary>
        private BodyScannerUIState InterfaceState(SharedBodyComponent body)
        {
            return new(body.Owner.Uid);
        }
    }
}
