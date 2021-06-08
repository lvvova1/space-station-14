using Content.Server.Utility;
using Content.Shared.GameObjects.Components.Surgery.UI;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Server.GameObjects.Components.Surgery.Tool
{
    [RegisterComponent]
    public class SurgeryDrapesComponent : Component
    {
        public override string Name => "SurgeryDrapes";

        [ViewVariables] public BoundUserInterface? UserInterface => Owner.GetUIOrNull(SurgeryUIKey.Key);
    }
}
