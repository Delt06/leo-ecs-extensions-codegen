using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using DELTation.LeoEcsExtensions.Components;
using UnityEngine;

namespace Systems
{
    public partial class PositionTransformSyncSystem
    {
        [EcsRun]
        partial void Update(in UnityRef<Transform> transform, in Position position)
        {
            transform.Object.position = position.Value;
        }
    }
}