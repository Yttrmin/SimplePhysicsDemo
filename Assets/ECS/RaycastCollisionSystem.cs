using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateBefore(typeof(IntegrateSystem))]
class RaycastCollisionSystem : JobComponentSystem
{
	[ComputeJobOptimization]
	struct BuildRaycastCommands : IJobParallelFor
	{
		public Data EntityData;
		public float DeltaTime;
		public NativeArray<RaycastCommand> RaycastCommands;

		public void Execute(int index)
		{
			if (EntityData.SleepCounters[index].Value > 0)
			{
				return;
			}
			var position = EntityData.Positions[index].Value;
			var velocity = EntityData.Velocities[index].Value;
			float distance = length(velocity * DeltaTime);
			RaycastCommands[index] = new RaycastCommand(position, velocity, distance);
		}
	}

	[ComputeJobOptimization]
	struct AssignRaycastResults : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<RaycastHit> RaycastHits;
		public Data EntityData;

		public void Execute(int index)
		{
			if (EntityData.SleepCounters[index].Value > 0)
			{
				return;
			}
			EntityData.CollisionHits[index] = new CollisionHit {Value = RaycastHits[index]};
		}
	}

	private struct Data
	{
		[ReadOnly]
		public ComponentDataArray<Position> Positions;
		[ReadOnly]
		public ComponentDataArray<Velocity> Velocities;
		[ReadOnly]
		public ComponentDataArray<SleepCounter> SleepCounters;
		[WriteOnly]
		public ComponentDataArray<CollisionHit> CollisionHits;
		public int Length;
	}

	[Inject]
	private Data _Data;

	public NativeArray<RaycastCommand> RaycastCommands;
	public NativeArray<RaycastHit> RaycastHits;

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		// The Unity console can report an error that these were not disposed after you stop playing.
		// I'm assuming that's because it stops before the next execution so the collections are never disposed.
		// Not sure if that's an issue that actually matters.
		if (RaycastCommands.IsCreated || RaycastHits.IsCreated)
		{
			RaycastCommands.Dispose();
			RaycastHits.Dispose();
		}

		RaycastCommands = new NativeArray<RaycastCommand>(_Data.Length, Allocator.TempJob);
		RaycastHits = new NativeArray<RaycastHit>(_Data.Length, Allocator.TempJob);

		var buildJob = new BuildRaycastCommands
		{
			EntityData = _Data,
			DeltaTime = Time.deltaTime,
			RaycastCommands = RaycastCommands
		};
		var buildJobHandle = buildJob.Schedule(_Data.Length, 32, inputDeps);

		var raycastJobHandle = RaycastCommand.ScheduleBatch(RaycastCommands, RaycastHits, 32, buildJobHandle);

		var assignJob = new AssignRaycastResults
		{
			EntityData = _Data,
			RaycastHits = RaycastHits
		};
		var assignJobHandle = assignJob.Schedule(_Data.Length, 32, raycastJobHandle);
			
		return assignJobHandle;
	}
}