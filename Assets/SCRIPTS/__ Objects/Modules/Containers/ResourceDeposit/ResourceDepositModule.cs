using SS.Content;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects.Modules
{
	public class ResourceDepositModule : SSModule, IMouseOverHandlerListener
	{
		public const string KFF_TYPEID = "resource_deposit";

		public const float MINING_SPEED = 1.0f;

		public struct SlotGroup
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

		public AudioClip miningSound { get; set; }

		internal SlotGroup[] slotGroups;

		public int slotCount
		{
			get
			{
				return this.slotGroups.Length;
			}
		}

		/// <summary>
		/// Returns a copy of every slot in the inventory.
		/// </summary>
		public SlotGroup[] GetSlots()
		{
			SlotGroup[] ret = new SlotGroup[this.slotCount];
			for( int i = 0; i < this.slotCount; i++ )
			{
				ret[i] = this.slotGroups[i];
			}
			return ret;
		}

		/// <summary>
		/// Sets the slots to the copy of the array.
		/// </summary>
		public void SetSlots( SlotGroup[] slotGroups )
		{
			for( int i = 0; i < slotGroups.Length; i++ )
			{
				for( int j = i + 1; j < slotGroups.Length; j++ )
				{
					if( slotGroups[i].id == slotGroups[j].id )
					{
						throw new Exception( "Can't have multiple slots with the same resource id." ); // because that doesn't make sense, just use bigger slot.
					}
				}
			}

			this.slotGroups = new SlotGroup[slotGroups.Length];
			for( int i = 0; i < slotGroups.Length; i++ )
			{
				this.slotGroups[i] = slotGroups[i];
			}
		}

		public class _UnityEvent_string_int : UnityEvent<string, int> { }
		
		public _UnityEvent_string_int onAdd { get; private set; } = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get; private set; } = new _UnityEvent_string_int();


		private void ShowTooltip()
		{
			Dictionary<string, int> itemsInDeposit = this.GetAll();

			ToolTip.Create( 200.0f, this.ssObject.displayName );

			foreach( var kvp in itemsInDeposit )
			{
				ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
				ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() + " / " + this.GetMaxCapacity( kvp.Key ) );
			}
			ToolTip.ShowAt( Input.mousePosition );
		}

		private void MoveTooltip()
		{
			ToolTip.MoveTo( Input.mousePosition, true );
		}

		private void HideTooltip()
		{
			ToolTip.Hide();
		}


		
		public void OnMouseEnterListener()
		{
			this.ShowTooltip();
		}

		public void OnMouseStayListener()
		{
			this.MoveTooltip();
		}

		public void OnMouseExitListener()
		{
			this.HideTooltip();
		}

		void Awake()
		{
			this.onAdd.AddListener( ( string id, int amount ) =>
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					this.ShowTooltip();
				}
			} );
			this.onRemove.AddListener( ( string id, int amount ) =>
			{
				if( this.isEmpty )
				{
					this.ssObject.Destroy();
				}
				else
				{
					if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
					{
						this.ShowTooltip();
					}
				}
			} );			
		}

		public override void OnObjDestroyed()
		{
			if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
			{
				this.HideTooltip();
			}
		}


		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.slotGroups.Length; i++ )
				{
					if( this.slotGroups[i].amount != 0 )
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

			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					return this.slotGroups[i].amount;
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
			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				ret.Add( this.slotGroups[i].id, this.slotGroups[i].amount );
			}
			return ret;
		}

		public int GetMaxCapacity( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					return this.slotGroups[i].slotCapacity;
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


			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					int spaceLeft = this.slotGroups[i].slotCapacity - this.slotGroups[i].amount;
					if( spaceLeft < amountMax )
					{
						this.slotGroups[i].amount = this.slotGroups[i].slotCapacity;
						this.onAdd?.Invoke( id, spaceLeft );
						return spaceLeft;
					}
					else
					{
						this.slotGroups[i].amount += amountMax;
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

			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					int spaceOccupied = this.slotGroups[i].amount;
					if( spaceOccupied <= amountMax )
					{
						this.slotGroups[i].amount = 0;
						this.onRemove?.Invoke( id, spaceOccupied );
						return spaceOccupied;
					}
					else
					{
						this.slotGroups[i].amount -= amountMax;
						this.onRemove?.Invoke( id, amountMax );
						return amountMax;
					}
				}
			}
			throw new Exception( "The inventory doesn't contain any slots that can hold '" + id + "'." );
		}

		public void Clear()
		{
			Tuple<string, int>[] res = new Tuple<string, int>[this.slotGroups.Length];

			for( int i = 0; i < this.slotGroups.Length; i++ )
			{
				res[i] = new Tuple<string, int>( this.slotGroups[i].id, this.slotGroups[i].amount );
				this.slotGroups[i].amount = 0;
			}

			// Call the event after clearing, once per each type.
			for( int i = 0; i < res.Length; i++ )
			{
				this.onRemove?.Invoke( res[i].Item1, res[i].Item2 ); // be consistent, both inventories call after adding/removing.
			}
		}

		public override ModuleData GetData()
		{
			ResourceDepositModuleData data = new ResourceDepositModuleData();

			data.items = this.GetAll();

			return data;
		}

		public override void SetData( ModuleData _data )
		{
			ResourceDepositModuleData data = ValidateDataType<ResourceDepositModuleData>( _data );
			
			// -----           DATA

			foreach( var kvp in data.items )
			{
				int capacity = this.GetMaxCapacity( kvp.Key );
				if( capacity == 0 )
				{
					throw new Exception( "This deposit can't hold '" + kvp.Key + "'." );
				}
				else
				{
					if( capacity < kvp.Value )
					{
						Debug.LogWarning( "This deposit can't hold " + kvp.Value + "x '" + kvp.Key + "'. - " + (kvp.Value - capacity) + "x resource has been lost." );
						this.Add( kvp.Key, capacity );
					}
					else
					{
						this.Add( kvp.Key, kvp.Value );
					}
				}
			}
		}
	}
}