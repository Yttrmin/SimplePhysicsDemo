using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public sealed class SimplePhysicsBootstrap
{
	public static float3 SpawnPosition { get; private set; } = new float3(0, 3.92f, 0);
	public static float3 SpawnDirection { get; private set; } = new float3(0, -1, 0);
	public static EntityArchetype PhysicsCube { get; private set; }
	public static MeshInstanceRenderer CubeRenderer { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		var entityManager = World.Active.GetOrCreateManager<EntityManager>();

		PhysicsCube = entityManager.CreateArchetype(typeof(SimplePhysics), typeof(Position), typeof(Velocity), typeof(CollisionHit), typeof(SleepCounter), typeof(TransformMatrix));
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void InitializeWithScene()
	{
		CubeRenderer = GetLookFromPrototype("SimplePhysicsRenderPrototype");

		var entityManager = World.Active.GetOrCreateManager<EntityManager>();
		var cube = entityManager.CreateEntity(PhysicsCube);
		entityManager.SetComponentData(cube, new Position { Value = new float3(5, 5, 1)});
		entityManager.AddSharedComponentData(cube, CubeRenderer);
	}

	private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
	{
		var proto = GameObject.Find(protoName);
		var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
		Object.Destroy(proto);
		return result;
	}
}

public struct SimplePhysics : IComponentData
{
}

public struct Velocity : IComponentData
{
	public float3 Value;
}

public struct CollisionHit : IComponentData
{
	public RaycastHit Value;
}

public struct SleepCounter : IComponentData
{
	public int Value;
}