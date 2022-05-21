// ReSharper disable once CheckNamespace

using UnityEngine;

namespace DELTation.LeoEcsExtensions.Components
{
    public struct UnityRef<T> where T : Object
    {
        // ReSharper disable once UnassignedField.Global
        public T Object;
    }
}