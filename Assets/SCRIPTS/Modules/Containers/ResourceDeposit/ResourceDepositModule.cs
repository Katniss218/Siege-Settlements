using SS;
using SS.Content;
using SS.Extras;
using SS.Levels.SaveStates;
using SS.Modules.Inventories;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Modules
{
	public class ResourceDepositModule : Module
	{
		public const float MINING_SPEED = 2.0f;

		/// <summary>
		/// If true, the resource can be mined instantly.
		/// </summary>
		public bool isTypeExtracted { get; set; }

		public AudioClip miningSound { get; set; }

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

		void Start()
		{

			
			UnityAction<GameObject> showTooltip = ( GameObject obj ) =>
			{
				if( obj == gameObject )
				{
					ResourceDepositModule deposit = obj.GetComponent<ResourceDepositModule>();
					if( deposit == null )
					{
						return;
					}

					Dictionary<string, int> itemsInDeposit = this.GetAll();

#warning can be not extra.
					ToolTip.Create( 200.0f, this.GetComponent<Extra>().displayName );

					foreach( var kvp in itemsInDeposit )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() + "/" + this.GetMaxCapacity( kvp.Key ) );
					}
					ToolTip.ShowAt( Input.mousePosition );
				}
			};
			UnityAction<GameObject> moveTooltip = ( GameObject obj ) =>
			{
				if( obj == gameObject )
				{
					ToolTip.MoveTo( Input.mousePosition, true );
				}
			};

			UnityAction<GameObject> hideTooltip = ( GameObject obj ) =>
			{
				if( obj == gameObject )
				{
					ToolTip.Hide();
				}
			};



			this.onAdd.AddListener((string id, int amount) =>
			{
				if(MouseOverHandler.currentObjectMouseOver == gameObject )
				{
					showTooltip( gameObject );
				}
			} );

			this.onRemove.AddListener((string id, int amount) =>
			{
				if( this.isEmpty )
				{
					if(MouseOverHandler.currentObjectMouseOver == gameObject )
					{
						hideTooltip( gameObject );
					}
					Object.Destroy(gameObject );

					MouseOverHandler.onMouseEnter.RemoveListener(showTooltip );
					MouseOverHandler.onMouseStay.RemoveListener(moveTooltip );
					MouseOverHandler.onMouseExit.RemoveListener(hideTooltip );
				}
				else
				{
					if(MouseOverHandler.currentObjectMouseOver == gameObject )
					{
						showTooltip( gameObject );
					}
				}
			} );

			MouseOverHandler.onMouseEnter.AddListener(showTooltip );
			MouseOverHandler.onMouseStay.AddListener(moveTooltip );
			MouseOverHandler.onMouseExit.AddListener(hideTooltip );
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

		public override ModuleData GetData()
		{
			ResourceDepositModuleData data = new ResourceDepositModuleData();

			data.items = this.GetAll();

			return data;
		}

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is ResourceDepositModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is ResourceDepositModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			ResourceDepositModuleDefinition def = (ResourceDepositModuleDefinition)_def;
			ResourceDepositModuleData data = (ResourceDepositModuleData)_data;

			this.resources = new SlotGroup[def.slots.Length];
			for( int i = 0; i < this.resources.Length; i++ )
			{
				for( int j = 0; j < def.slots.Length; j++ )
				{
					if( this.resources[j].id == def.slots[i].resourceId )
					{
						throw new Exception( "Can't have multiple slots with the same resource id." ); // because that doesn't make sense, just use bigger slot.
					}
				}

				this.resources[i] = new SlotGroup( def.slots[i].resourceId, 0, def.slots[i].capacity );
			}


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