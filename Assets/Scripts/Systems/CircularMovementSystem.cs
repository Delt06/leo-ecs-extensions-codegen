using Components;
using DELTation.LeoEcsExtensions.CodeGen.Attributes;
using UnityEngine;

namespace Systems
{
    public class CircularMovementSystem
    {
        [EcsRun]
        private void Update(ref Position position, in CircularMovement circularMovement)
        {
            var angle = circularMovement.Speed * Time.time;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);
            position.Value = new Vector3(cos, 0, sin) * circularMovement.Radius;
        }
    }
}