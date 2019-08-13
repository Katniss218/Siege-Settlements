using SS.ResourceSystem;
using UnityEngine;

namespace SS.Buildings
{
	public class ConstructionData
	{
		public string[] resourceIds { get; private set; }
		public int[] resourcesRemaining { get; private set; }
		public int[] resourcesTotal { get; private set; }

		public ConstructionData( ResourceStack[] requiredResources )
		{
			this.resourceIds = new string[requiredResources.Length];
			this.resourcesRemaining = new int[requiredResources.Length];
			this.resourcesTotal = new int[requiredResources.Length];

			for( int i = 0; i < requiredResources.Length; i++ )
			{
				this.resourceIds[i] = requiredResources[i].id;
				this.resourcesRemaining[i] = requiredResources[i].amount;
				this.resourcesTotal[i] = requiredResources[i].amount;
			}
		}

		public bool IsCompleted()
		{
			for( int i = 0; i < resourcesRemaining.Length; i++ )
			{
				if( resourcesRemaining[i] != 0 )
				{
					return false;
				}
			}
			return true;
		}

		public float GetPercentCompleted()
		{
			float total = 0;
			for( int i = 0; i < resourcesRemaining.Length; i++ )
			{
				total += (float)(resourcesTotal[i] - resourcesRemaining[i]) / (float)resourcesTotal[i];
			}
			return total / resourcesRemaining.Length;
		}
	}
}