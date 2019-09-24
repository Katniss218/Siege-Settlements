using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Inventories
{
	/// <summary>
	/// An inventory that has slots constrained to single resource ID.
	/// </summary>
	public sealed class InventoryConstrained : MonoBehaviour, IInventory
	{
		private struct SlotGroup
		{
			public readonly string id;
			public int amount;
			public readonly int slotCapacity;

			public SlotGroup( string id, int amount, int capacity )
			{
				this.id = id;
				this.amount = amount;
				this.slotCapacity = capacity;
			}
		}

		public struct SlotInfo
		{
			/// <summary>
			/// What resource id this slot can take.
			/// </summary>
			public string id;

			/// <summary>
			/// How much resource this slot can take.
			/// </summary>
			public int capacity;

			public SlotInfo( string id, int capacity )
			{
				this.id = id;
				this.capacity = capacity;
			}
		}

		private SlotGroup[] resources;
		
		public int slotCount
		{
			get
			{
				return this.resources.Length;
			}
		}

		[SerializeField] private _UnityEvent_string_int __onAdd = new _UnityEvent_string_int();
		public _UnityEvent_string_int onAdd
		{
			get { return this.__onAdd; }
		}

		[SerializeField] private _UnityEvent_string_int __onRemove = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove
		{
			get { return this.__onRemove; }
		}



		/// <summary>
		/// Sets the slots of the inventory.
		/// </summary>
		public void SetSlots( params SlotInfo[] slots )
		{
			this.resources = new SlotGroup[slots.Length];
			for( int i = 0; i < this.resources.Length; i++ )
			{
				for( int j = 0; j < slots.Length; j++ )
				{
					if( this.resources[j].id == slots[i].id )
					{
						throw new Exception( "Can't have multiple slots with the same resource id." ); // because that doesn't make sense, just use bigger slot.
					}
				}

				this.resources[i] = new SlotGroup( slots[i].id, 0, slots[i].capacity );
			}
		}
		
		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.resources.Length; i++ )
				{
					if( this.resources[i].amount != 0 )
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
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					return this.resources[i].amount;
				}
			}
			// Resource is not present.
			return 0;
		}
		
		public Dictionary<string, int> GetAll()
		{
			if( this.isEmpty )
			{
				return new Dictionary<string, int>();
			}
			Dictionary<string, int> ret = new Dictionary<string, int>();
			for( int i = 0; i < this.resources.Length; i++ )
			{
				ret.Add( this.resources[i].id, this.resources[i].amount );
			}
			return ret;
		}

		public int GetMaxCapacity( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					return this.resources[i].slotCapacity;
				}
			}
			// No slot with the specified id.
			return 0;
		}

		public int Add( string id, int amountMax )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountMax < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}


			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					int spaceLeft = this.resources[i].slotCapacity - this.resources[i].amount;
					if( spaceLeft < amountMax )
					{
						this.resources[i].amount = this.resources[i].slotCapacity;
						this.onAdd?.Invoke( id, spaceLeft );
						return spaceLeft;
					}
					else
					{
						this.resources[i].amount += amountMax;
						this.onAdd?.Invoke( id, amountMax );
						return amountMax;
					}
				}
			}
			throw new Exception( "The inventory doesn't contain any slots that can hold '" + id + "'." );
		}

		public int Remove( string id, int amountMax )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}
			if( amountMax < 1 )
			{
				throw new ArgumentOutOfRangeException( "Amount can't be less than 1." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == id )
				{
					int spaceOccupied = this.resources[i].amount;
					if( spaceOccupied <= amountMax )
					{
						this.resources[i].amount = 0;
						this.onRemove?.Invoke( id, spaceOccupied );
						return spaceOccupied;
					}
					else
					{
						this.resources[i].amount -= amountMax;
						this.onRemove?.Invoke( id, amountMax );
						return amountMax;
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
				this.resources[i].amount = 0;
			}

			// Call the event after clearing, once per each type.
			for( int i = 0; i < res.Length; i++ )
			{
				this.onRemove?.Invoke( res[i].Item1, res[i].Item2 ); // be consistent, both inventories call after adding/removing.
			}
		}
	}
}