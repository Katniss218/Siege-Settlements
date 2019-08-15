using SS.ResourceSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Buildings
{
	/// <summary>
	/// Represents a building that's being constructed.
	/// </summary>
	public class ConstructionSite : MonoBehaviour
	{
		public class _UnityEvent_ConstructionSite_ResourceStack : UnityEvent<ConstructionSite, ResourceStack> { }

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

		// the total amt of every resource needed for construction combined.
		public int totalNeeded { get; private set; }

		public float GetHealthPercentGained( int resourceAmt )
		{
			return (float)resourceAmt / ((float)this.totalNeeded * 0.9f);
		}

		/// <summary>
		/// This function checks if the construction has finished, override this for custom condition.
		/// </summary>
		public Func<bool> isCompleted;
		
		private Transform graphicsTransform;
		private MeshRenderer meshRenderer;

		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public _UnityEvent_ConstructionSite_ResourceStack onConstructionProgress = new _UnityEvent_ConstructionSite_ResourceStack();

		/// <summary>
		/// Assigns a cost (in resources) to construct the building.
		/// </summary>
		public void AssignResources( ResourceStack[] requiredResources )
		{
			this.totalNeeded = 0;
			this.resourceIds = new string[requiredResources.Length];
			this.resourcesRemaining = new int[requiredResources.Length];
			this.resourcesTotal = new int[requiredResources.Length];

			for( int i = 0; i < requiredResources.Length; i++ )
			{
				this.resourceIds[i] = requiredResources[i].id;
				this.resourcesRemaining[i] = requiredResources[i].amount;
				this.resourcesTotal[i] = requiredResources[i].amount;
				this.totalNeeded += requiredResources[i].amount;
			}
		}

		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
		}
		
		// placeholder for auto-building until the proper build mechanic comes into the game.
		void Update()
		{
			if( UnityEngine.Random.Range( 0, 4 ) == 1 )
			{
				AdvanceConstruction( new ResourceStack( "resource.wood", 1 ) );
			}
		}

		/// <summary>
		/// Advances the construction (gives the resources) by a specified amount.
		/// </summary>
		public void AdvanceConstruction( ResourceStack stack )
		{
			for( int i = 0; i < resourceIds.Length; i++ )
			{
				if( this.resourceIds[i] == stack.id )
				{
					if( this.resourcesRemaining[i] - stack.amount < 0 )
					{
						Debug.LogWarning( "AdvanceConstruction: Added more resource than needed (" + stack.amount + "/" + this.resourcesRemaining[i] + ")." );
					}
					this.resourcesRemaining[i] -= stack.amount;

					if( this.isCompleted() )
					{
						this.FinishConstruction();
					}
					this.onConstructionProgress?.Invoke( this, stack );
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the required resources and instantly finishes construction. Removes the ConstructionSite script from the GameObject.
		/// </summary>
		public void FinishConstruction()
		{
			this.meshRenderer.material.SetFloat( "_Progress", 1 );
			Destroy( this );
		}
	}
}