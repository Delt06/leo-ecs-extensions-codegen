using System;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;

namespace Components
{
    [EcsComponent]
    [EcsComponentWithView]
    [Serializable]
    public struct UniformRotation
    {
        public float Velocity;
    }
}