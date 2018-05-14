using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

static class MathExtensions
{
	// Vector3.Angle() calls Vector3.normalized which potentially returns Vector3.Zero, which is unsupported by Burst.
	// So we just implement it ourselves.
	public static float angle(float3 from, float3 to)
	{
		float f = Mathf.Clamp(dot(normalize(from), normalize(to)), -1f, 1f);
		if ((double)Mathf.Abs(f) > 0.999989986419678)
			return (double)f <= 0.0 ? 180f : 0.0f;
		return Mathf.Acos(f) * 57.29578f;
	}
}
