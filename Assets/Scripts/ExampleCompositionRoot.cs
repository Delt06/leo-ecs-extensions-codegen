using DELTation.DIFramework;
using DELTation.DIFramework.Containers;
using DELTation.LeoEcsExtensions.Composition.Di;

public class ExampleCompositionRoot : DependencyContainerBase
{
    protected override void ComposeDependencies(ICanRegisterContainerBuilder builder)
    {
        builder
            .RegisterEcsEntryPoint<ExampleEcsEntryPoint>()
            .AttachEcsEntryPointViewTo(gameObject)
            ;
    }
}