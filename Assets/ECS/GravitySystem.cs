using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

class GravitySystem : JobComponentSystem
{
	[ComputeJobOptimization]
	private struct Data : IJobProcessComponentData<SleepCounter, Velocity>
	{
		public float DeltaTime;
		public float3 Gravity;
			
		public void Execute([ReadOnly]ref SleepCounter sleepCounter, ref Velocity velocity)
		{
			if (sleepCounter.Value == 0)
			{
				velocity.Value += Gravity * DeltaTime;
			}
			else
			{
				velocity.Value = new float3(0, 0, 0);
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new Data
		{
			DeltaTime = Time.deltaTime,
			Gravity = new float3(0, -9, 0)
		};
		return job.Schedule(this, 32, inputDeps);
	}
}