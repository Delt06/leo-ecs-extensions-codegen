using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using DELTation.LeoEcsExtensions.Components;
using UnityEngine;

namespace Systems
{
    public partial class RotationTransformSyncSystem
    {
        [EcsRun]
        partial void Update(in UnityRef<Transform> transform, in Rotation rotation)
        {
            transform.Object.rotation = rotation.Value;
        }
    }
}