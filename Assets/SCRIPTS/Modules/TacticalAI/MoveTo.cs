using UnityEngine;
using UnityEngine.AI;

namespace SS
{
	public abstract partial class TAIGoal
	{
		[RequireComponent( typeof( NavMeshAgent ) )]
		public class MoveTo : TAIGoal
		{
			public Vector3 destination { get; private set; }

			void Start()
			{
				this.GetComponent<NavMeshAgent>().SetDestination( this.destination );
			}

			/// <summary>
			/// Assigns a new MoveTo TAI goal to the GameObject.
			/// </summary>
			public static void AssignTAIGoal( GameObject gameObject, Vector3 destination )
			{
				TAIGoal.ClearGoal( gameObject );

				MoveTo moveTo = gameObject.AddComponent<TAIGoal.MoveTo>();

				moveTo.destination = destination;
			}
		}
	}
}