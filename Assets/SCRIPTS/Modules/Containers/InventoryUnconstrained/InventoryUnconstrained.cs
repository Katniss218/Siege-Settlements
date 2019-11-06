using SS.Levels.SaveStates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules.Inventories
{
	/// <summary>
	/// An inventory that can hold arbitrary resource in each of the slots (as long as there are no duplicate ids).
	/// </summary>
	public sealed class InventoryUnconstrained : Module, IInventory
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

#warning TODO! - Needs to communicate with HUD's on units and other objects (if applicable).
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



		void Start()
		{
			if( this.GetComponents<IInventory>().Length > 1 )
			{
				throw new Exception( "Can't have more than 1 IInventory on an object (only saving 1)." );
			}
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
		
		public int Get( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == "" )
				{
					continue;
				}
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
				// If undefined slot, skip.
				if( this.resources[i].id == "" )
				{
					continue;
				}
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

			bool foundEmpty = false;
			for( int i = 0; i < this.resources.Length; i++ )
			{
				if( this.resources[i].id == "" )
				{
					foundEmpty = true;
					continue;
				}
				if( this.resources[i].id == id )
				{
					return slotCapacity;
				}
			}
			// Empty slot is present.
			if( foundEmpty )
			{
				return slotCapacity;
			}
			// Every slot occupied by resource with different id.
			return 0;
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

			int? indexOfEmpty = null;
			for( int i = 0; i < this.resources.Length; i++ )
			{
				// Cache first empty slot, to add to it if there is no resource of specified type in the inventory.
				if( indexOfEmpty == null )
				{
					if( this.resources[i].id == "" )
					{
						indexOfEmpty = i;
						continue;
					}
				}
				// If ids are matching, add to the slot.
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

			// if no slots with the id were found, try a previously found empty slot.
			if( indexOfEmpty.HasValue )
			{
				this.resources[indexOfEmpty.Value].id = id;
				int spaceLeft = this.slotCapacity - this.resources[indexOfEmpty.Value].amount;
				if( spaceLeft < amountPref )
				{
					this.resources[indexOfEmpty.Value].amount = this.slotCapacity;
					this.onAdd?.Invoke( id, spaceLeft );
					return spaceLeft;
				}
				else
				{
					this.resources[indexOfEmpty.Value].amount += amountPref;
					this.onAdd?.Invoke( id, amountPref );
					return amountPref;
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

			// Call the event after clearing, once per each type.
			for( int i = 0; i < res.Length; i++ )
			{
				this.onRemove?.Invoke( res[i].Item1, res[i].Item2 ); // be consistent, both inventories call after adding/removing.
			}
		}



		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override ModuleData GetData()
		{
			InventoryUnconstrainedData data = new InventoryUnconstrainedData();

			data.items = this.GetAll();

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is InventoryUnconstrainedDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is InventoryUnconstrainedData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			InventoryUnconstrainedDefinition def = (InventoryUnconstrainedDefinition)_def;
			InventoryUnconstrainedData data = (InventoryUnconstrainedData)_data;

			this.resources = new SlotGroup[def.slotCount];
			for( int i = 0; i < this.resources.Length; i++ )
			{
				this.resources[i] = new SlotGroup( "", 0 );
			}
			this.slotCapacity = def.slotCapacity;


			foreach( var kvp in data.items )
			{
				this.Add( kvp.Key, kvp.Value );
			}
		}
	}
}