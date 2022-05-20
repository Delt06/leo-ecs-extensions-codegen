using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Systems
{
    public partial class UniformRotationSystem
    {
        [EcsRun]
        partial void Update(ref Rotation rotation, in UniformRotation uniformRotation)
        {
            var extraRotation = Quaternion.Euler(0, uniformRotation.Velocity * Time.deltaTime, 0);
            rotation.Value = extraRotation * rotation.Value;
        }
    }
}