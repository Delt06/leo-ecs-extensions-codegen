using System;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Components
{
    [EcsComponent]
    [EcsComponentWithView]
    [Serializable]
    public struct CircularMovement
    {
        public float Speed;
        [Min(0f)]
        public float Radius;
    }
}