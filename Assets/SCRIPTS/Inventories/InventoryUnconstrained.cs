﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Inventories
{
	/// <summary>
	/// An inventory that can hold arbitrary resource in each of the slots (as long as there are no duplicate ids).
	/// </summary>
	public class InventoryUnconstrained : MonoBehaviour, IInventory
	{
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

		private int slotCapacity;

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
		/// <param name="slots">The number of slots.</param>
		/// <param name="slotCapacity">Capacity per slot.</param>
		public void SetSlots( int slots, int slotCapacity )
		{
			this.resources = new SlotGroup[slots];
			for( int i = 0; i < this.resources.Length; i++ )
			{
				this.resources[i] = new SlotGroup( "", 0 );
			}
			this.slotCapacity = slotCapacity;
		}

		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.resources.Length; i++ )
				{
					if( this.resources[i].id == "" )
					{
						continue;
					}
					if( this.resources[i].amount != 0 )
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
					return this.resources[i].amount + amount <= this.slotCapacity;
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
					int spaceLeft = this.slotCapacity - this.resources[i].amount;
					if( spaceLeft < amountPref )
					{
						this.resources[i].amount = this.slotCapacity;
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
					int spaceLeft = this.slotCapacity - this.resources[i].amount;
					if( spaceLeft < amountPref )
					{
						this.resources[i].amount = this.slotCapacity;
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
					if( spaceOccupied <= amountPref )
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