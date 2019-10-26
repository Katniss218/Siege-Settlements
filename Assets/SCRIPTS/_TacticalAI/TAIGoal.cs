using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public abstract partial class TAIGoal : MonoBehaviour
	{
		protected static void ClearGoal( GameObject gameObject )
		{
			TAIGoal goal = gameObject.GetComponent<TAIGoal>();
			Object.DestroyImmediate( goal );
		}

		public static void Assign( GameObject gameObject, TAIGoalData data )
		{
			if( gameObject == null )
			{
				return;
			}
			if( data == null )
			{
				return;
			}
			data.AssignTo( gameObject );
		}

		/// <summary>
		/// Returns the data for this TAIGoal.
		/// </summary>
		public abstract TAIGoalData GetData();
	}
}