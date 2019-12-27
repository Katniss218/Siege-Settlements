using UnityEngine.AI;

namespace SS.Objects
{
	public interface IMovable
	{
		float movementSpeed { get; set; }
		float? movementSpeedOverride { get; set; }
		float rotationSpeed { get; set; }
		float? rotationSpeedOverride { get; set; }

		NavMeshAgent navMeshAgent { get; }
	}
}