using System;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using Leopotam.EcsLite;
using UnityEngine;

namespace DELTation.LeoEcsExtensions.CodeGen
{
    public partial class EcsSystemExample
    {
        private static void ConfigureDestroyFilter(EcsWorld.Mask filter) { }
        private static void ConfigureInitFilter(EcsWorld.Mask filter) { }
        private static void ConfigureRunFilter(EcsWorld.Mask filter) { }

        [EcsRun]
        partial void Update(int i, ref float c, in double d, in MyGenericStruct<float> myGenericStruct,
            EcsPool<byte> ints, Transform transform)
        {
            Console.Write(i);
        }

        [EcsInit]
        static partial void Init(int i, ref float d, ref char c) { }

        [EcsDestroy]
        partial void Destroy(int i, Transform transform) { }
    }

    [EcsComponent] [EcsComponentWithView] [Serializable]
    public struct MyGenericStruct<T>
    {
        public T Value;
    }

    [EcsComponent]
    public struct MyStruct { }

    [EcsComponent]
    public struct MyAutoResetStruct : IEcsAutoReset<MyAutoResetStruct>
    {
        public void AutoReset(ref MyAutoResetStruct c) { }
    }
}