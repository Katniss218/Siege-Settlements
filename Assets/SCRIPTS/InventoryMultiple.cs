using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Allows an object to hold resources in it's inventory.
	/// </summary>
	public class InventoryMultiple : MonoBehaviour, IInventory
	{
		private string[] resourceId;
		private int[] resourceAmount;

		private int[] maxCapacity;

		public int slots { get; private set; }

		[SerializeField] private _UnityEvent_string_int __onAdd = new _UnityEvent_string_int();
		public _UnityEvent_string_int onAdd { get { return this.__onAdd; } }
		[SerializeField] private _UnityEvent_string_int __onRemove = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get { return this.__onRemove; } }

		public void SetCapacity( int slots, int[] maxCapacity )
		{
			if( maxCapacity.Length != slots )
			{
				throw new System.ArgumentOutOfRangeException( "MaxCapacity has different length than the number of slots." );
			}
			this.resourceId = new string[slots];
			this.resourceAmount = new int[slots];
			this.maxCapacity = maxCapacity;
		}

		/// <summary>
		/// Returns true, if the inventory is currently holding a resource.
		/// </summary>
		public bool isEmpty
		{
			get
			{
				for( int i = 0; i < this.slots; i++ )
				{
					if( !string.IsNullOrEmpty( this.resourceId[i] ) )
					{
						return false;
					}
				}
				return true;
			}
		}

		public int Get( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "CanHold: Invalid argument 'id'." );
			}
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == id )
				{
					return this.resourceAmount[i];
				}
			}
			return 0;
		}

		public List<ResourceStack> GetAll()
		{
			if( isEmpty )
			{
				return null;
			}
			List<ResourceStack> ret = new List<ResourceStack>();
			for( int i = 0; i < this.slots; i++ )
			{
				ret.Add( new ResourceStack( this.resourceId[i], this.resourceAmount[i] ) );
			}
			return ret;
		}

		public bool Has( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new System.ArgumentNullException( "Amount can't be less than 1." );
			}
			int total = 0;
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == id )
				{
					total += this.resourceAmount[i];
				}
			}
			return total >= amount;
		}

		public bool CanHold( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new System.ArgumentNullException( "Amount can't be less than 1." );
			}
			int remainingTotal = 0;
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == id )
				{
					remainingTotal += maxCapacity[i] - this.resourceAmount[i];
				}
				else
				{
					remainingTotal += this.maxCapacity[i];
				}
			}
			return remainingTotal >= amount;
		}

		public int Add( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new System.ArgumentNullException( "Amount can't be less than 1." );
			}
			int totalAdded = 0;
			int totalLeftToAdd = amountPref;
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == null )
				{
					this.resourceId[i] = id;
				}
				if( this.resourceId[i] == id )
				{
					int spaceLeft = this.maxCapacity[i] - this.resourceAmount[i];
					int amtAdded = spaceLeft < totalLeftToAdd ? spaceLeft : totalLeftToAdd;

					this.resourceAmount[i] += amtAdded;
					totalAdded += amtAdded;
					totalLeftToAdd -= amtAdded;
				}
			}
			
			this.onAdd?.Invoke( id, totalLeftToAdd );
			return totalAdded;
		}

		public int Remove( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new System.ArgumentNullException( "Amount can't be less than 1." );
			}
			int totalRemoved = 0;
			int totalLeftToRemove = amountPref;
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == id )
				{
					int amtRemoved = this.resourceAmount[i] < totalLeftToRemove ? this.resourceAmount[i] : totalLeftToRemove;

					this.resourceAmount[i] -= amtRemoved;
					if( this.resourceAmount[i] == 0 )
					{
						this.resourceId[i] = null;
					}

					totalRemoved += amtRemoved;
					totalLeftToRemove -= amtRemoved;
				}
			}

			this.onRemove?.Invoke( id, totalRemoved );
			return totalRemoved;
		}

		public void Clear()
		{
			Dictionary<string, int> totalsPerId = new Dictionary<string, int>();

			// Count the amount of each type.
			for( int i = 0; i < this.slots; i++ )
			{
				if( this.resourceId[i] == null )
				{
					continue;
				}
				if( totalsPerId.ContainsKey( this.resourceId[i] ) )
				{
					totalsPerId[this.resourceId[i]] += i;
				}
				else
				{
					totalsPerId.Add( this.resourceId[i], i );
				}
				this.resourceId[i] = default;
				this.resourceAmount[i] = default;
			}
			// Call event per each type, with the counted amount.
			foreach( var kvp in totalsPerId )
			{
				this.onRemove?.Invoke( kvp.Key, kvp.Value ); // be consistent, both inventories call after adding/removing.
			}
		}
	}
}