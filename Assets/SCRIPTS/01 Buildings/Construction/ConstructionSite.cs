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

		public Func<bool> isCompleted;

		// to build a building you need the health to reach 100%.
		// to fix a building, you need the health to reach 50%.

		// each building needs a certain set of resources in order to reach 100%.
		// the amount of health gained is scaled according to the total amt needed (if the building reqs 100 stone and 900 wood, getting 500 wood will increase the health by 50%).
		// get scaling factor per each resource.

		private Transform graphicsTransform;
		private MeshRenderer meshRenderer;

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

		void Start()
		{

		}

		void Update()
		{
			if( UnityEngine.Random.Range( 0, 4 ) == 1 )
				AdvanceConstruction( new ResourceStack("resource.wood", 1 ) );
			
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
						Debug.LogWarning( "AdvanceConstruction: the amount of resource added was more than needed." );
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
		/// Overrides the required resources and instantly finishes construction.
		/// </summary>
		public void FinishConstruction()
		{
			this.meshRenderer.material.SetFloat( "_Progress", 1 );
			// remove the ConstructionSite script from gameObject.
			Destroy( this );
		}
		/*
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
		*/
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