using SS.Content;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SS.Modules.Inventories
{
	/// <summary>
	/// An inventory that has slots constrained to single resource ID.
	/// </summary>
	public sealed class InventoryConstrained : Module, IInventory
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

		void Awake()
		{
			UnityAction<GameObject> showTooltip = ( GameObject obj ) =>
			{
				if( obj == this.gameObject )
				{
					IInventory inventory = obj.GetComponent<IInventory>();
					if( inventory == null )
					{
						return;
					}

#warning if the inventory is constrained, we know which resources can be added. Display 0/capacity.
#warning Add GetSlots method (returns null id and 0 amount, if the id is not defined yet - unconstrained empty slot).
#warning Inventory unconstrained should allow for adding multiple of the same resource type?
					Dictionary<string, int> items = this.GetAll();

					ToolTip.Create( 200.0f, this.GetComponent<SSObject>().displayName );

					foreach( var kvp in items )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( kvp.Key );
						ToolTip.AddText( resourceDef.icon, kvp.Value.ToString() + "/" + this.GetMaxCapacity( kvp.Key ) );
					}
					ToolTip.ShowAt( Input.mousePosition );
				}
			};
			UnityAction<GameObject> moveTooltip = ( GameObject obj ) =>
			{
				if( obj == this.gameObject )
				{
					ToolTip.MoveTo( Input.mousePosition, true );
				}
			};

			UnityAction<GameObject> hideTooltip = ( GameObject obj ) =>
			{
				if( obj == this.gameObject )
				{
					ToolTip.Hide();
				}
			};

			this.onAdd.AddListener( ( string id, int amount ) =>
			{
				if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
				{
					showTooltip( this.gameObject );
				}
			} );
			this.onRemove.AddListener( ( string id, int amount ) =>
			{
				if( this.isEmpty )
				{
					if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
					{
						hideTooltip( this.gameObject );
					}
				}
				else
				{
					if( MouseOverHandler.currentObjectMouseOver == this.gameObject )
					{
						showTooltip( this.gameObject );
					}
				}
			} );

			MouseOverHandler.onMouseEnter.AddListener( showTooltip );
			MouseOverHandler.onMouseStay.AddListener( moveTooltip );
			MouseOverHandler.onMouseExit.AddListener( hideTooltip );

			Damageable damageable = this.GetComponent<Damageable>();
			if( damageable != null )
			{
				damageable.onDeath.AddListener( () =>
				{
					hideTooltip( this.gameObject );
					MouseOverHandler.onMouseEnter.RemoveListener( showTooltip );
					MouseOverHandler.onMouseStay.RemoveListener( moveTooltip );
					MouseOverHandler.onMouseExit.RemoveListener( hideTooltip );
				} );
			}

			SSObject ssObject = this.GetComponent<SSObject>();

			if( ssObject is IHUDObject )
			{
				// integrate hud.
				IHUDObject hudObj = (IHUDObject)ssObject;


				Transform hudResourceTransform = hudObj.hud.transform.Find( "Resource" );
				if( hudResourceTransform != null )
				{
					Transform hudResourceIconTransform = hudResourceTransform.Find( "Icon" );
					if( hudResourceIconTransform != null )
					{
						Image hudResourceIcon = hudResourceIconTransform.GetComponent<Image>();
						TextMeshProUGUI hudAmount = hudObj.hud.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();

						// Make the inventory update the HUD wien resources are added/removed.
						this.onAdd.AddListener( ( string id, int amtAdded ) =>
						{
						// Set the icon to the first slot that contains a resource.
						foreach( var kvp in this.GetAll() )
							{
								if( kvp.Key == "" )
								{
									continue;
								}
								hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon; // this can be null.
								hudAmount.text = kvp.Value.ToString();

								hudResourceIcon.gameObject.SetActive( true );
								hudAmount.gameObject.SetActive( true );
								break;
							}
						} );
						this.onRemove.AddListener( ( string id, int amtRemoved ) =>
						{
							if( this.isEmpty )
							{
								hudResourceIcon.gameObject.SetActive( false );
								hudAmount.gameObject.SetActive( false );
							}
							else
							{
							// Set the icon to the first slot that contains a resource.
							foreach( var kvp in this.GetAll() )
								{
									if( kvp.Key == "" )
									{
										continue;
									}
									hudResourceIcon.sprite = DefinitionManager.GetResource( kvp.Key ).icon; // this can be null.
									hudAmount.text = kvp.Value.ToString();
									break;
								}
							}
						} );
					}
				}
			}
			Damageable dam = this.GetComponent<Damageable>();
			if( dam != null )
			{
				dam.onDeath.AddListener( () =>
				{
					if( !this.isEmpty )
					{
						TAIGoal.DropoffToNew.DropOffInventory( this, this.transform.position );
					}
				} );
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




		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public override ModuleData GetData()
		{
			InventoryConstrainedData data = new InventoryConstrainedData();

			data.items = this.GetAll();

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is InventoryConstrainedDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is InventoryConstrainedData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			InventoryConstrainedDefinition def = (InventoryConstrainedDefinition)_def;
			InventoryConstrainedData data = (InventoryConstrainedData)_data;
			
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