using UnityEngine.AI;

namespace SS.Objects
{
	public interface IMovable
	{
		float movementSpeed { get; set; }
		float rotationSpeed { get; set; }

		NavMeshAgent navMeshAgent { get; }
	}
}