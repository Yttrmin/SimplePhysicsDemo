using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

class PhysicsSpawnerSystem : ComponentSystem
{
	private struct Data
	{
		[ReadOnly]
		ComponentDataArray<SimplePhysics> SimplePhysicsMarker;

		public int Length;
	}

	[Inject]
	private Data _Data;

	protected override void OnUpdate()
	{
		const int SpawnPerUpdate = 8;
		if (Time.frameCount % 1 == 0 && _Data.Length < 8192)
		{
			for (var i = 0; i < SpawnPerUpdate; i++)
			{
				var spawnDirection = normalize(new float3(0, -1, 0));
				var velocity = normalize((float3) Random.onUnitSphere + spawnDirection * 1.3f) *
					            Random.Range(2f, 10f); // spawn with velocities arching towards a target object
				var position = SimplePhysicsBootstrap.SpawnPosition;

				var entityManager = World.Active.GetOrCreateManager<EntityManager>();
				var newEntity = entityManager.CreateEntity(SimplePhysicsBootstrap.PhysicsCube);
				entityManager.SetComponentData<Position>(newEntity, new Position {Value = position});
				entityManager.SetComponentData<Velocity>(newEntity, new Velocity {Value = velocity});
				entityManager.AddSharedComponentData(newEntity, SimplePhysicsBootstrap.CubeRenderer);
			}
		}
	}
}
