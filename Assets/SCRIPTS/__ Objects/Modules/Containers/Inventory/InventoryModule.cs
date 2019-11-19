using SS.Content;
using SS.Levels.SaveStates;
using SS.Objects;
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
	public class _UnityEvent_string_int : UnityEvent<string, int> { }
	
	public sealed class InventoryModule : SSModule
	{
		public struct SlotGroup
		{
			public readonly string slotId;
			public string id;
			public int amount;
			public readonly int capacity;

			public bool isConstrained { get { return this.slotId != ""; } }
			public bool isEmpty { get { return this.amount == 0 || this.id == ""; } }

			public SlotGroup( string slotId, int slotCapacity )
			{
				this.slotId = slotId ?? "";
				this.id = "";
				this.capacity = slotCapacity;
				this.amount = 0;
			}
		}

		private SlotGroup[] slotGroups;

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
		/// Returns the amount of slots of this inventory.
		/// </summary>
		public int slotCount
		{
			get
			{
				return this.slotGroups.Length;
			}
		}


		[SerializeField] private _UnityEvent_string_int __onAdd = new _UnityEvent_string_int();
		[SerializeField] private _UnityEvent_string_int __onRemove = new _UnityEvent_string_int();

		public _UnityEvent_string_int onAdd
		{
			get { return this.__onAdd; }
		}
		public _UnityEvent_string_int onRemove
		{
			get { return this.__onRemove; }
		}

		//##=====================================================##

		#region TOOLTIP INTEGRATION
		//

		private void ShowTooltip( GameObject mouseoveredObj )
		{
			if( mouseoveredObj != this.gameObject )
			{
				return;
			}

			ToolTip.Create( 200.0f, this.ssObject.displayName );

			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( this.slotGroups[i].isConstrained )
					{
						ResourceDefinition resourceDef = DefinitionManager.GetResource( this.slotGroups[i].slotId );
						ToolTip.AddText( resourceDef.icon, this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
					}
					else
					{
						ToolTip.AddText( AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/empty_resource" ), this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
					}
				}
				else
				{
					ResourceDefinition resourceDef = DefinitionManager.GetResource( this.slotGroups[i].id );
					ToolTip.AddText( resourceDef.icon, this.slotGroups[i].amount + " / " + this.slotGroups[i].capacity );
				}
			}
			ToolTip.ShowAt( Input.mousePosition );
		}

		private void MoveTooltip( GameObject mouseoveredObj )
		{
			if( mouseoveredObj != this.gameObject )
			{
				return;
			}

			ToolTip.MoveTo( Input.mousePosition, true );
		}

		private void HideTooltip( GameObject mouseoveredObj )
		{
			if( mouseoveredObj != this.gameObject )
			{
				return;
			}

			ToolTip.Hide();
		}
		
		private void RegisterTooltip()
		{
			this.onAdd.AddListener( ( string id, int amount ) =>
			{
				this.ShowTooltip( MouseOverHandler.currentObjectMouseOver );
			} );
			this.onRemove.AddListener( ( string id, int amount ) =>
			{
				if( this.isEmpty )
				{
					this.HideTooltip( MouseOverHandler.currentObjectMouseOver );
				}
				else
				{
					this.ShowTooltip( MouseOverHandler.currentObjectMouseOver );
				}
			} );

			MouseOverHandler.onMouseEnter.AddListener( this.ShowTooltip );
			MouseOverHandler.onMouseStay.AddListener( this.MoveTooltip );
			MouseOverHandler.onMouseExit.AddListener( this.HideTooltip );

			Damageable damageable = this.GetComponent<Damageable>();
			if( damageable != null )
			{
				damageable.onDeath.AddListener( () =>
				{
					this.HideTooltip( this.gameObject );
					MouseOverHandler.onMouseEnter.RemoveListener( this.ShowTooltip );
					MouseOverHandler.onMouseStay.RemoveListener( this.MoveTooltip );
					MouseOverHandler.onMouseExit.RemoveListener( this.HideTooltip );
				} );
			}
		}

		//
		#endregion
			
		//##=====================================================##

		#region HUD INTEGRATION
		//

		private void RegisterHUD()
		{
			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;


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
						for( int i = 0; i < this.slotCount; i++ )
						{
							if( this.slotGroups[i].isEmpty )
							{
								continue;
							}
							hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon; // this can be null.
							hudAmount.text = "" + this.slotGroups[i].amount;

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
							for( int i = 0; i < this.slotCount; i++ )
							{
								if( this.slotGroups[i].isEmpty )
								{
									continue;
								}
								hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon; // this can be null.
								hudAmount.text = "" + this.slotGroups[i].amount;
								break;
							}
						}
					} );
				}
			}
		}

		//
		#endregion
		
		//##=====================================================##

		private void RegisterDropOnDeath()
		{
			(this.ssObject as IDamageable).damageable.onDeath.AddListener( () =>
			{
				if( !this.isEmpty )
				{
					TAIGoal.DropoffToNew.DropOffInventory( this, this.transform.position );
				}
			} );
		}


		void Awake()
		{
			// needs to be registered on Awake(), since the resources (data) are assigned before Start() happens.
			this.RegisterTooltip();

			if( this.ssObject is IHUDHolder )
			{
				this.RegisterHUD();
			}
			if( this.ssObject is IDamageable )
			{
				this.RegisterDropOnDeath();
			}
		}

		public bool isEmpty
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.slotCount; i++ )
				{
					if( !this.slotGroups[i].isEmpty )
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

			int total = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].id == id )
				{
					total += this.slotGroups[i].amount;
				}
			}
			return total;
		}

		public Dictionary<string, int> GetAll()
		{
			if( this.isEmpty )
			{
				return new Dictionary<string, int>();
			}
			Dictionary<string, int> ret = new Dictionary<string, int>();
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					continue;
				}
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

			int total = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						total += this.slotGroups[i].capacity;
					}
				}
				else
				{
					// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						total += this.slotGroups[i].capacity - this.slotGroups[i].amount;
					}
				}
			}
			return total;
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

			// array of indices to the slots. (necessary since we want to fill up non-empty slots before filling up empty ones).
			List<int> indices = new List<int>();
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						indices.Add( i );
					}
				}
				else
				{// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						indices.Insert( 0, i );
					}
				}
			}
			int amountRemaining = amountMax;
			for( int i = 0; i < indices.Count; i++ )
			{
				int index = indices[i];
				int spaceInSlot = this.slotGroups[index].capacity - this.slotGroups[index].amount;
				int amountAdded = spaceInSlot > amountRemaining ? amountRemaining : spaceInSlot;

				this.slotGroups[index].amount += amountAdded;
				this.slotGroups[index].id = id;
				amountRemaining -= amountAdded;
				this.onAdd?.Invoke( id, amountAdded );

				if( amountRemaining == 0 )
				{
					return amountMax;
				}
			}
			return amountMax - amountRemaining;
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

			int amountRemoved = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					continue;
				}
				if( this.slotGroups[i].id == id )
				{
					if( this.slotGroups[i].amount <= amountMax )
					{
						int spaceOccupied = this.slotGroups[i].amount;
						this.slotGroups[i].amount = 0;
						this.slotGroups[i].id = "";
						amountRemoved += spaceOccupied;
						this.onRemove?.Invoke( id, spaceOccupied );
					}
					else
					{
						this.slotGroups[i].amount -= amountMax;
						amountRemoved += amountMax;
						this.onRemove?.Invoke( id, amountMax );
					}
					if( amountRemoved == amountMax )
					{
						return amountRemoved;
					}
				}
			}
			return amountRemoved;
		}

		public void Clear()
		{
#warning TODO! - call events based on the item type, not each slot.
			Tuple<string, int>[] res = new Tuple<string, int>[this.slotCount];

			for( int i = 0; i < this.slotCount; i++ )
			{
				res[i] = new Tuple<string, int>( this.slotGroups[i].id, this.slotGroups[i].amount );
				this.slotGroups[i].id = "";
				this.slotGroups[i].amount = 0;
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
			InventoryModuleData data = new InventoryModuleData();

			data.items = new InventoryModuleData.SlotData[this.slotCount];
			for( int i = 0; i < data.items.Length; i++ )
			{
				data.items[i] = new InventoryModuleData.SlotData( this.slotGroups[i] );
			}

			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public override void SetDefData( ModuleDefinition _def, ModuleData _data )
		{
			if( !(_def is InventoryModuleDefinition) )
			{
				throw new Exception( "Provided definition is not of the correct type." );
			}
			if( _def == null )
			{
				throw new Exception( "Provided definition is null." );
			}

			if( !(_data is InventoryModuleData) )
			{
				throw new Exception( "Provided data is not of the correct type." );
			}
			if( _data == null )
			{
				throw new Exception( "Provided data is null." );
			}

			InventoryModuleDefinition def = (InventoryModuleDefinition)_def;
			InventoryModuleData data = (InventoryModuleData)_data;

			this.slotGroups = new SlotGroup[def.slots.Length];
			for( int i = 0; i < this.slotCount; i++ )
			{
				this.slotGroups[i] = new SlotGroup( def.slots[i].slotId, def.slots[i].capacity );
			}


			for( int i = 0; i < data.items.Length; i++ )
			{
				this.slotGroups[i].id = data.items[i].id;
				this.slotGroups[i].amount = data.items[i].amount;

#warning TODO! - call events based on the item type, not each slot. (don't call with slot args, slots gan be accessed directly)
				this.onAdd?.Invoke( data.items[i].id, data.items[i].amount );
			}
		}
	}
}