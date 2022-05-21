using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Systems
{
    public partial class RotationTransformSyncSystem
    {
        [EcsRun]
        partial void Update(Transform transform, in Rotation rotation)
        {
            transform.rotation = rotation.Value;
        }
    }
}