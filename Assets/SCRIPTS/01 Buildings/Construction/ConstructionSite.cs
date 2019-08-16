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
		/// The total cost of the building's construction/repair (sum of every resource).
		/// </summary>
		public int totalNeeded { get; private set; }

		/// <summary>
		/// Calculates how much (%) should be added to the building's health given the specified amount of resources.
		/// </summary>
		/// <param name="resourceAmt"></param>
		/// <returns></returns>
		public float GetHealthPercentGained( int resourceAmt )
		{
			return (float)resourceAmt / ((float)this.totalNeeded * 0.9f);
		}

		/// <summary>
		/// This function checks if the construction has finished, override this for custom condition.
		/// </summary>
		public Func<bool> isCompleted;
		
		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public _UnityEvent_ConstructionSite_ResourceStack onConstructionProgress = new _UnityEvent_ConstructionSite_ResourceStack();

		/// <summary>
		/// Is called when the construction progresses (resources are added).
		/// </summary>
		public UnityEvent onConstructionComplete = new UnityEvent();

		/// <summary>
		/// Assigns a cost (in resources) to construct the building.
		/// </summary>
		public void AssignResources( ResourceStack[] requiredResources )
		{
			this.totalNeeded = 0;
			this.resourceIds = new string[requiredResources.Length];
			this.resourcesRemaining = new int[requiredResources.Length];

			for( int i = 0; i < requiredResources.Length; i++ )
			{
				this.resourceIds[i] = requiredResources[i].id;
				this.resourcesRemaining[i] = requiredResources[i].amount;
				this.totalNeeded += requiredResources[i].amount;
			}
		}
		
		// placeholder for auto-building until the proper build mechanic comes into the game.
		void Update()
		{
			if( Input.GetKeyDown( KeyCode.B ) ) // wood
			{
				AdvanceConstructionBy( new ResourceStack( "resource.wood", 50 ) );
			}
			if( Input.GetKeyDown( KeyCode.N ) ) // stone
			{
				AdvanceConstructionBy( new ResourceStack( "resource.stone", 50 ) );
			}
		}

		/// <summary>
		/// Advances the construction (gives the resources) by a specified amount.
		/// </summary>
		public void AdvanceConstructionBy( ResourceStack stack )
		{
			for( int i = 0; i < this.resourceIds.Length; i++ )
			{
				if( this.resourceIds[i] == stack.id )
				{
					this.onConstructionProgress?.Invoke( this, stack );
					if( this.isCompleted() )
					{
						this.FinishConstruction();
					}
					break;
				}
			}
		}

		/// <summary>
		/// Overrides the required resources and instantly finishes construction. Removes the ConstructionSite script from the GameObject.
		/// </summary>
		public void FinishConstruction()
		{
			this.onConstructionComplete?.Invoke();
			Destroy( this );
		}
	}
}