using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

class IntegrateSystem : JobComponentSystem
{
	[ComputeJobOptimization]
	private struct Data : IJobProcessComponentData<Velocity, CollisionHit, Position>
	{
		public float DeltaTime;

		public void Execute([ReadOnly]ref Velocity velocity, [ReadOnly]ref CollisionHit collisionHit, ref Position position)
		{
			if (((float3)collisionHit.Value.normal).Equals(new float3(0,0,0)))
			{
				position.Value += velocity.Value * length(velocity.Value * DeltaTime);
			}
			else
			{
				position.Value = ((float3)collisionHit.Value.point) + new float3(0, 0.1f, 0);
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new Data
		{
			DeltaTime = Time.deltaTime
		};
		return job.Schedule(this, 32, inputDeps);
	}
}
