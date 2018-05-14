using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

class SleepSystem : JobComponentSystem
{
	public class RemoveSleepingBarrier : BarrierSystem
	{

	}

	private struct Data
	{
		[ReadOnly]
		public EntityArray Array;
		public ComponentDataArray<SleepCounter> SleepCounters;
	}

	[Inject] private Data _Data;
	[Inject] private RemoveSleepingBarrier _RemoveSleepingBarrier;

	[ComputeJobOptimization]
	private struct Job : IJob
	{
		[ReadOnly]
		public EntityArray Array;
		public ComponentDataArray<SleepCounter> SleepCounters;
		public EntityCommandBuffer CommandBuffer;

		public void Execute()
		{
			for (var i = 0; i < Array.Length; i++)
			{
				if (SleepCounters[i].Value > 0)
				{
					SleepCounters[i] = new SleepCounter { Value = SleepCounters[i].Value + 1 };
					if (SleepCounters[i].Value > 64)
					{
						CommandBuffer.DestroyEntity(Array[i]);
					}
				}
			}
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		return new Job
		{
			Array = _Data.Array,
			SleepCounters = _Data.SleepCounters,
			CommandBuffer = _RemoveSleepingBarrier.CreateCommandBuffer()
		}.Schedule(inputDeps);
	}
}