﻿using Leopotam.EcsLite;

// ReSharper disable once CheckNamespace
namespace DELTation.LeoEcsExtensions.Views.Components
{
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once UnusedTypeParameter
    public class ComponentView<T> where T : struct { }

    public class AutoResetComponentView<T> : ComponentView<T> where T : struct, IEcsAutoReset<T> { }
}