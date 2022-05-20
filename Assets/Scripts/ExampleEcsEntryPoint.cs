using DELTation.LeoEcsExtensions.Composition.Di;
using Systems;

public class ExampleEcsEntryPoint : EcsEntryPoint
{
    public override void PopulateSystems(EcsFeatureBuilder featureBuilder)
    {
        featureBuilder
            .CreateAndAdd<CircularMovementSystem>()
            .CreateAndAdd<UniformRotationSystem>()
            .CreateAndAdd<PositionTransformSyncSystem>()
            .CreateAndAdd<RotationTransformSyncSystem>()
            ;
    }
}