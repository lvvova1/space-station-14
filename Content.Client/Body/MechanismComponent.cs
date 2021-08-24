using Content.Shared.Body.Mechanism;
using Robust.Shared.GameObjects;

namespace Content.Client.Body
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedMechanismComponent))]
    public class MechanismComponent : SharedMechanismComponent
    {
    }
}
