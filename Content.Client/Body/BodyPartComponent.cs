using Content.Shared.Body.Part;
using Robust.Shared.GameObjects;

namespace Content.Client.Body
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedBodyPartComponent))]
    public class BodyPartComponent : SharedBodyPartComponent
    {
    }
}
