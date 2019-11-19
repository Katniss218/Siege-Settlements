using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Extras
{
	public class Extra : SSObject, IObstacle
	{
		public NavMeshObstacle obstacle { get; private set; }
		new public BoxCollider collider { get; private set; }

		void Start()
		{
			this.obstacle = this.GetComponent<NavMeshObstacle>();
			this.collider = this.GetComponent<BoxCollider>();
		}
	}
}