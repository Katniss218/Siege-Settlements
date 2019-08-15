using SS.ResourceSystem;
using UnityEngine;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour
	{
		// TODO ----- generalize this to be able to construct other things than buildings, maybe stationary ballistas or units being trained in barracks.
		/// <summary>
		/// An array of resource types needed for construction (Read Only).
		/// </summary>
		public string[] resourceIds { get; private set; }
		/// <summary>
		/// An array of remaining resources, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		public int[] resourcesRemaining { get; private set; }
		/// <summary>
		/// An array of total amount of resources needed for construction, per resource type (use resourceIds[i] to get the Id) (Read Only).
		/// </summary>
		public int[] resourcesTotal { get; private set; }

		private Transform graphicsTransform;
		private MeshRenderer meshRenderer;

		/// <summary>
		/// Assigns a cost (in resources) to construct the building.
		/// </summary>
		public void AssignResources( ResourceStack[] requiredResources )
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

		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		/// <summary>
		/// Advances the construction (gives the resources) by a specified amount.
		/// </summary>
		public void AdvanceConstruction( ResourceStack stack )
		{
			for( int i = 0; i < resourceIds.Length; i++ )
			{
				if( resourceIds[i] == stack.id )
				{
					if( resourcesRemaining[i] - stack.amount < 0 )
					{
						Debug.LogWarning( "AdvanceConstruction: the amount of resource added was more than needed." );
					}
					resourcesRemaining[i] -= stack.amount;

					if( IsCompleted() )
					{
						FinishConstruction();
					}
					else
					{
						this.meshRenderer.material.SetFloat( "_Progress", GetPercentCompleted() );
					}
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the required resources and instantly finishes construction.
		/// </summary>
		public void FinishConstruction()
		{
			this.meshRenderer.material.SetFloat( "_Progress", 1 );
			// remove the ConstructionSite script from gameObject.
			Destroy( this );
		}

		/// <summary>
		/// Checks if there are no more resources needed.
		/// </summary>
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

		/// <summary>
		/// Gets the percent of construction's completion.
		/// </summary>
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