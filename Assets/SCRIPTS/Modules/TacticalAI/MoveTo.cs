using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		public class MoveTo : TAIGoal
		{
			public Vector3 destination;

			public MoveTo( Vector3 destination )
			{
				this.destination = destination;
			}
			
			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( this.destination );
			}
		}
	}
}