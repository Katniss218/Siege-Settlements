using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS
{
	/// <summary>
	/// Allows an object to hold resources in it's inventory.
	/// </summary>
	public class InventoryUnconstrained : MonoBehaviour, IInventory
	{
		// this is a resource that can hold N different resource types, which types are not specified, and can change. No two slots can have the same resource type however.

		// The capacity is constant - no matter the slot, the capacity doesn't change.
		// (we want this so the capacity per resource stays constant).

		private struct SlotGroup
		{
			public string id;
			public int amount;

			public SlotGroup( string id, int amount )
			{
				this.id = id;
				this.amount = amount;
			}
		}
		
		private SlotGroup[] resources;

		private int capacity;

		public int slots
		{
			get
			{
				return this.resources.Length;
			}
		}

		[SerializeField] private _UnityEvent_string_int __onAdd = new _UnityEvent_string_int();
		public _UnityEvent_string_int onAdd { get { return this.__onAdd; } }
		[SerializeField] private _UnityEvent_string_int __onRemove = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get { return this.__onRemove; } }

		public void SetSlots( int slots, int capacity )
		{
			this.resources = new SlotGroup[slots];
			for( int i = 0; i < this.resources.Length; i++ )
			{
				this.resources[i] = new SlotGroup( "", 0 );
			}
			this.capacity = capacity;
		}

		/// <summary>
		/// Returns true, if the inventory is currently holding a resource.
		/// </summary>
		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.resources.Length; i++ )
				{
					if( this.resources[i].id == "" || this.resources[i].amount != 0 )
					{
						return false;
					}
				}
				return true;
			}
		}
		
		public Dictionary<string, int> GetAll()
		{
			if( this.isEmpty )
			{
				return null;
			}
			Dictionary<string, int> ret = new Dictionary<string, int>();
			for( int i = 0; i < this.resources.Length; i++ )
			{
				// If undefined slot, skip.
				if( this.resources[i].id == "" )
				{
					continue;
				}
				ret.Add( this.resources[i].id, this.resources[i].amount );
			}
			return ret;
		}
		
		public bool Has( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					return this.resources[i].amount >= amount;
				}
			}
			return false;
		}

		public bool CanHold( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == "" || this.resources[i].id == id )
				{
					return true;
				}
			}
			return false;
		}

		public bool CanHold( string id, int amount )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amount < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					return this.resources[i].amount + amount <= this.capacity;
				}
			}
			return false;
		}

		public int Add( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}


			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					int spaceLeft = this.capacity - this.resources[i].amount;
					if( spaceLeft < amountPref )
					{
						this.resources[i].amount = this.capacity;
						this.onAdd?.Invoke( id, spaceLeft );
						return spaceLeft;
					}
					else
					{
						this.resources[i].amount += amountPref;
						this.onAdd?.Invoke( id, amountPref );
						return amountPref;
					}
				}
			}

			// if no slots with the id were found, try occupying new one.

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == "" )
				{
					this.resources[i].id = id;
					int spaceLeft = this.capacity - this.resources[i].amount;
					if( spaceLeft < amountPref )
					{
						this.resources[i].amount = this.capacity;
						this.onAdd?.Invoke( id, spaceLeft );
						return spaceLeft;
					}
					else
					{
						this.resources[i].amount += amountPref;
						this.onAdd?.Invoke( id, amountPref );
						return amountPref;
					}
				}
			}
			throw new Exception( "The inventory doesn't contain any slots that can hold '" + id + "'." );
		}

		public int Remove( string id, int amountPref )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountPref < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					int spaceOccupied = this.resources[i].amount;
					if( spaceOccupied < amountPref )
					{
						this.resources[i].id = "";
						this.resources[i].amount = 0;
						this.onRemove?.Invoke( id, spaceOccupied );
						return spaceOccupied;
					}
					else
					{
						this.resources[i].amount -= amountPref;
						this.onRemove?.Invoke( id, amountPref );
						return amountPref;
					}
				}
			}
			throw new Exception( "The inventory doesn't contain any slots that can hold '" + id + "'." );
		}

		public void Clear()
		{
			Tuple<string, int>[] res = new Tuple<string, int>[this.resources.Length];

			for( int i = 0; i < this.resources.Length; i++ )
			{
				res[i] = new Tuple<string, int>( this.resources[i].id, this.resources[i].amount );
				this.resources[i].id = "";
				this.resources[i].amount = 0;
			}

			// Call event per each type, with the counted amount.
			for( int i = 0; i < res.Length; i++ )
			{
				this.onRemove?.Invoke( res[i].Item1, res[i].Item2 ); // be consistent, both inventories call after adding/removing.
			}
		}
	}
}