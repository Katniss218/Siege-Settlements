using UnityEngine;

namespace SS
{
	[DisallowMultipleComponent]
	public abstract partial class TAIGoal : MonoBehaviour
	{
		protected static void ClearGoal( GameObject gameObject )
		{
			Object.DestroyImmediate( gameObject.GetComponent<TAIGoal>() );
		}

		/// <summary>
		/// Returns the data for this TAIGoal.
		/// </summary>
		public abstract TAIGoalData GetData();
	}
}