using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static MathExtensions;

[UpdateAfter(typeof(IntegrateSystem))]
public class CollisionResponseSystem : JobComponentSystem
{
	[ComputeJobOptimization]
	private struct Data : IJobProcessComponentData<CollisionHit, Velocity, SleepCounter>
	{
		public void Execute([ReadOnly]ref CollisionHit collisionHit, ref Velocity velocity, ref SleepCounter sleepCounter)
		{
			if (!((float3)collisionHit.Value.normal).Equals(new float3(0,0,0)))
			{
				if (length(velocity.Value) <= 2f)
				{
					velocity.Value = new float3(0, 0, 0);
					sleepCounter.Value++;
				}
				else
				{
					velocity.Value = reflect(velocity.Value, collisionHit.Value.normal);
					
					var angleBetweenNormalAndUp = angle(collisionHit.Value.normal, new float3(0,1,0));
					var toLerp = angleBetweenNormalAndUp / 180f;
					velocity.Value = velocity.Value * lerp(0.5f, 1f, toLerp);
				}
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new Data();

		return job.Schedule(this, 32, inputDeps);
	}
}
