using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects
{
	public interface IObstacle
	{
		NavMeshObstacle obstacle { get; }
		BoxCollider collider { get; }
	}
}
