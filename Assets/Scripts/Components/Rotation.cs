using System;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using Leopotam.EcsLite;
using UnityEngine;

namespace Components
{
    [EcsComponent]
    [EcsComponentWithView]
    [Serializable]
    public struct Rotation : IEcsAutoReset<Rotation>
    {
        public Quaternion Value;

        public void AutoReset(ref Rotation c)
        {
            c.Value = Quaternion.identity;
        }
    }
}