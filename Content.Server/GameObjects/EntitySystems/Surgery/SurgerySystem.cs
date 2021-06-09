using System.Collections.Generic;
using Content.Server.GameObjects.Components.Surgery.Tool;
using Content.Server.GameObjects.EntitySystems.Surgery.Events;
using Content.Server.GameObjects.EntitySystems.Surgery.Events.Popups;
using Content.Server.Utility;
using Content.Shared.GameObjects.Components.Body;
using Content.Shared.GameObjects.Components.Body.Part;
using Content.Shared.GameObjects.Components.Surgery.Operation;
using Content.Shared.GameObjects.Components.Surgery.Surgeon;
using Content.Shared.GameObjects.Components.Surgery.Target;
using Content.Shared.GameObjects.Components.Surgery.UI;
using Content.Shared.GameObjects.EntitySystems.Surgery;
using Content.Shared.Interfaces;
using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server.GameObjects.EntitySystems.Surgery
{
    public class SurgerySystem : SharedSurgerySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SurgeryDrapesComponent, ComponentStartup>(OnDrapesStartup);

            SubscribeLocalEvent<SurgeryDrapesComponent, DrapesTryUseEvent>(OnDrapesTryUse);
            SubscribeLocalEvent<SurgeryDrapesComponent, AfterInteractEvent>(OnDrapesAfterInteract);

            SubscribeLocalEvent<DoOutsiderBeginPopupEvent>(DoOutsiderBeginPopup);
            SubscribeLocalEvent<DoOutsiderSuccessPopup>(DoOutsiderSuccessPopup);
        }

        private void OnDrapesStartup(EntityUid uid, SurgeryDrapesComponent drapes, ComponentStartup args)
        {
            var ui = drapes.UserInterface;

            if (ui != null)
            {
                ui.OnReceiveMessage += msg => OnSurgeryDrapesUIMessage(drapes, msg);
            }
        }

        private void OnDrapesTryUse(EntityUid uid, SurgeryDrapesComponent component, DrapesTryUseEvent args)
        {
            TryUseDrapes(component, args.Surgeon, args.Target, args.Operation);
        }

        // TODO SURGERY: Add surgery for dismembered limbs
        private void OnDrapesAfterInteract(EntityUid uid, SurgeryDrapesComponent drapes, AfterInteractEvent args)
        {
            var target = args.Target;
            if (target == null)
            {
                return;
            }

            var user = args.User;
            if (user.TryGetComponent(out SurgeonComponent? surgeon) &&
                surgeon.Target != null &&
                IsPerformingSurgeryOn(surgeon, surgeon.Target))
            {
                if (surgeon.Target.SurgeryTags.Count == 0 &&
                    StopSurgery(surgeon))
                {
                    DoDrapesCancelPopups(drapes, surgeon, target);
                }

                args.Handled = true;
                return;
            }

            if (!user.TryGetComponent(out ActorComponent? actor) ||
                !target.TryGetComponent(out IBody? body))
            {
                return;
            }

            drapes.UserInterface?.Open(actor.PlayerSession);
            UpdateDrapesUI(drapes, body);

            args.Handled = true;
        }

        private void OnSurgeryDrapesUIMessage(SurgeryDrapesComponent drapes, ServerBoundUserInterfaceMessage message)
        {
            switch (message.Message)
            {
                case SurgeryOpPartSelectUIMsg msg:
                    if (!drapes.Owner.EntityManager.TryGetEntity(msg.Part, out var targetEntity))
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} with an invalid target entity id: {msg.Part}");
                        return;
                    }

                    if (!targetEntity.TryGetComponent(out SurgeryTargetComponent? target))
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} with an entity that has no {nameof(SurgeryTargetComponent)}: {targetEntity}");
                        return;
                    }

                    var surgeon = message.Session.AttachedEntity?.EnsureComponent<SurgeonComponent>();

                    if (surgeon == null)
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} with no attached entity of their own.");
                        return;
                    }

                    if (!_prototypeManager.TryIndex<SurgeryOperationPrototype>(msg.OperationId, out var operation))
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} with an invalid {nameof(SurgeryOperationPrototype)} id: {msg.OperationId}");
                        return;
                    }

                    // TODO SURGERY: Make each surgeon "know" a set of surgeries that they may perform
                    if (operation.Hidden)
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} that tried to start a hidden {nameof(SurgeryOperationPrototype)} with id: {msg.OperationId}");
                        return;
                    }

                    if (IsPerformingSurgeryOn(surgeon, target))
                    {
                        Sawmill.Warning(
                            $"Client {message.Session} sent {nameof(SurgeryOpPartSelectUIMsg)} to a start a {msg.OperationId} operation while already performing a {target.Operation?.ID} on {target.Owner}");
                        return;
                    }

                    TryUseDrapes(drapes, surgeon, target, operation);
                    break;
            }
        }

        public bool TryUseDrapes(
            SurgeryDrapesComponent drapes,
            SurgeonComponent surgeon,
            SurgeryTargetComponent target,
            SurgeryOperationPrototype operation)
        {
            if (TryStartSurgery(surgeon, target, operation))
            {
                DoDrapesStartPopups(drapes, surgeon, target.Owner, operation);
                return true;
            }

            return false;
        }

        private void DoDrapesStartPopups(
            SurgeryDrapesComponent drapes,
            SurgeonComponent surgeon,
            IEntity target,
            SurgeryOperationPrototype operation)
        {
            if (IsPerformingSurgeryOnSelf(surgeon))
            {
                if (target.TryGetComponent(out IBodyPart? part) &&
                    part.Body != null)
                {
                    var id = "surgery-prepare-start-self-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("zone", target),
                        ("procedure", operation.Name)));

                    id = "surgery-prepare-start-self-outsider-popup";
                    part.Body.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("part", target),
                        ("procedure", operation.Name)),
                        except: part.Body.Owner);
                }
                else
                {
                    var id = "surgery-prepare-start-self-no-zone-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("procedure", operation.Name)));

                    id = "surgery-prepare-start-self-no-zone-outsider-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner)));
                }
            }
            else
            {
                if (IsReceivingSurgeryOnPart(target, out var part, out var body))
                {
                    var id = "surgery-prepare-start-surgeon-popup";
                    body.Owner.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("target", body.Owner),
                        ("zone", part.Owner),
                        ("procedure", operation.Name)));

                    id = "surgery-prepare-start-target-popup";
                    surgeon.Owner.PopupMessage(body.Owner, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("zone", part.Owner)));

                    id = "surgery-prepare-start-outsider-popup";
                    surgeon.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("target", body.Owner),
                        ("zone", target)),
                        except: body.Owner);
                }
                else
                {
                    var id = "surgery-prepare-start-no-zone-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("target", target),
                        ("procedure", operation.Name)));

                    id = "surgery-prepare-start-no-zone-target-popup";
                    surgeon.Owner.PopupMessage(target, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner)));

                    id = "surgery-prepare-start-no-zone-outsider-popup";
                    surgeon.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("target", target)),
                        except: target);
                }
            }
        }

        private void DoDrapesCancelPopups(SurgeryDrapesComponent drapes, SurgeonComponent surgeon, IEntity target)
        {
            if (IsPerformingSurgeryOnSelf(surgeon))
            {
                if (target.TryGetComponent(out IBodyPart? part) &&
                    part.Body != null)
                {
                    var id = "surgery-prepare-cancel-self-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("zone", target)));

                    id = "surgery-prepare-cancel-self-outsider-popup";
                    part.Body.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("part", target)),
                        except: part.Body.Owner);
                }
                else
                {
                    var id = "surgery-prepare-cancel-self-no-zone-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner)));

                    id = "surgery-prepare-cancel-self-no-zone-outsider-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner)));
                }
            }
            else
            {
                if (IsReceivingSurgeryOnPart(target, out var part, out var body))
                {
                    var id = "surgery-prepare-cancel-surgeon-popup";
                    body.Owner.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("target", body.Owner),
                        ("zone", part.Owner)));

                    id = "surgery-prepare-cancel-target-popup";
                    surgeon.Owner.PopupMessage(body.Owner, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("zone", part.Owner)));

                    id = "surgery-prepare-cancel-outsider-popup";
                    surgeon.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("target", body.Owner),
                        ("zone", target)),
                        except: body.Owner);
                }
                else
                {
                    var id = "surgery-prepare-cancel-no-zone-surgeon-popup";
                    target.PopupMessage(surgeon.Owner, Loc.GetString(id,
                        ("item", drapes.Owner),
                        ("target", target)));

                    id = "surgery-prepare-cancel-no-zone-target-popup";
                    surgeon.Owner.PopupMessage(target, Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner)));

                    id = "surgery-prepare-cancel-no-zone-outsider-popup";
                    surgeon.Owner.PopupMessageOtherClients(Loc.GetString(id,
                        ("user", surgeon),
                        ("item", drapes.Owner),
                        ("target", target)),
                        except: target);
                }
            }
        }

        private void UpdateDrapesUI(SurgeryDrapesComponent drapes, IBody body)
        {
            var ui = drapes.UserInterface;
            if (ui == null)
            {
                return;
            }

            var parts = new List<EntityUid>();

            foreach (var (part, _) in body.Parts)
            {
                if (part.Owner.TryGetComponent(out SurgeryTargetComponent? surgery))
                {
                    parts.Add(surgery.Owner.Uid);
                }
            }

            var state = new SurgeryUIState(parts.ToArray());
            ui.SetState(state);
        }

        public void DoOutsiderBeginPopup(DoOutsiderBeginPopupEvent ev)
        {
            string msg;

            if (ev.Target == null)
            {
                var locId = $"surgery-step-{ev.Id}-begin-no-zone-outsider-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Part));
            }
            else if (ev.Surgeon == ev.Target)
            {
                var locId = $"surgery-step-{ev.Id}-begin-self-outsider-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }
            else
            {
                var locId = $"surgery-step-{ev.Id}-begin-outsider-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }

            ev.Surgeon.PopupMessageOtherClients(msg, _playerManager, except: ev.Target ?? ev.Surgeon);
        }

        public void DoOutsiderSuccessPopup(DoOutsiderSuccessPopup ev)
        {
            string msg;

            if (ev.Target == null)
            {
                var locId = $"surgery-step-{ev.Id}-success-no-zone-outsider-popup";
                msg =  _loc.GetString(locId, ("user", ev.Surgeon), ("part", ev.Part));
            }
            else if (ev.Surgeon == ev.Target)
            {
                var locId = $"surgery-step-{ev.Id}-success-self-outsider-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }
            else
            {
                var locId = $"surgery-step-{ev.Id}-success-outsider-popup";
                msg = _loc.GetString(locId, ("user", ev.Surgeon), ("target", ev.Target), ("part", ev.Part));
            }

            ev.Surgeon.PopupMessageOtherClients(msg, _playerManager, except: ev.Target ?? ev.Surgeon);
        }
    }
}
