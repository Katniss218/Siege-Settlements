using SS.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Allows an object to hold resources in it's inventory.
	/// </summary>
	public class InventorySingle : MonoBehaviour, IInventory
	{
		private string resourceId;
		private int resourceAmount;

		public int maxCapacity { get; set; }


		[SerializeField] private _UnityEvent_string_int __onAdd = new _UnityEvent_string_int();
		public _UnityEvent_string_int onAdd { get { return this.__onAdd; } }
		[SerializeField] private _UnityEvent_string_int __onRemove = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get { return this.__onRemove; } }

		public void SetCapacity( int maxCapacity )
		{
			this.resourceId = default;
			this.resourceAmount = default;
			this.maxCapacity = maxCapacity;
		}

		/// <summary>
		/// Returns true, if the inventory is currently holding a resource.
		/// </summary>
		public bool isEmpty
		{
			get
			{
				return string.IsNullOrEmpty( this.resourceId );
			}
		}

		public int Get( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "CanHold: Invalid argument 'id'." );
			}
			if( this.resourceId == id )
			{
				return this.resourceAmount;
			}
			else
			{
				return 0;
			}
		}

		public List<ResourceStack> GetAll()
		{
			if( this.isEmpty )
			{
				return null;
			}
			return new List<ResourceStack>() { new ResourceStack( this.resourceId, this.resourceAmount ) };
		}

		public bool Has( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new System.ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}
			if( this.isEmpty )
			{
				return true;
			}
			if( this.resourceId == id )
			{
				return this.resourceAmount >= amount;
			}
			return false;
		}

		public bool CanHold( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new System.ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}
			if( this.isEmpty )
			{
				return true;
			}
			if( this.resourceId == id )
			{
				return this.resourceAmount + amount <= this.maxCapacity;
			}
			return true;
		}

		public int Add( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new System.ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}
			if( !this.isEmpty && this.resourceId != id )
			{
				return 0;
			}
			int spaceLeft = this.maxCapacity - this.resourceAmount;
			int amtAdded = spaceLeft < amountPref ? spaceLeft : amountPref;

			this.resourceId = id;
			this.resourceAmount += amtAdded;

			this.onAdd?.Invoke( id, amtAdded );
			return amtAdded;
		}

		public int Remove( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new System.ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new System.ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			if( !this.isEmpty && this.resourceId != id )
			{
				return 0;
			}
			int amtRemoved = this.resourceAmount < amountPref ? this.resourceAmount : amountPref;

			this.resourceAmount -= amtRemoved;
			if( this.resourceAmount == 0 )
			{
				this.resourceId = null;
			}

			this.onRemove?.Invoke( id, amtRemoved );
			return amtRemoved;
		}

		public void Clear()
		{
			string id = this.resourceId;
			int amount = this.resourceAmount;
			this.resourceAmount = 0;

			this.resourceId = null;
			this.onRemove?.Invoke( id, amount ); // be consistent, both inventories call after adding/removing.
		}
	}
}