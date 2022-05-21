using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Systems
{
    public partial class PositionTransformSyncSystem
    {
        [EcsRun]
        partial void Update(Transform transform, in Position position)
        {
            transform.position = position.Value;
        }
    }
}