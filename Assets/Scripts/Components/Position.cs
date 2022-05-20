using System;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Components
{
    [EcsComponent]
    [EcsComponentWithView]
    [Serializable]
    public struct Position
    {
        public Vector3 Value;
    }
}