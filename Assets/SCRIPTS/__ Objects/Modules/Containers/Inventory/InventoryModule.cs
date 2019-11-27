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

namespace SS.Objects.Modules
{	
	public sealed class InventoryModule : SSModule
	{
		public const string KFF_TYPEID = "inventory";

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


		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		public _UnityEvent_string_int onAdd { get; private set; } = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get; private set; } = new _UnityEvent_string_int();


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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
				this.ShowTooltip( MouseOverHandler.currentObjectMouseOver );
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


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


		private void RegisterHUD()
		{
			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;


			Transform hudResourceTransform = hudObj.hud.transform.Find( "Resource" );
			if( hudResourceTransform == null )
			{
				return;
			}

			Transform hudResourceIconTransform = hudResourceTransform.Find( "Icon" );
			if( hudResourceIconTransform == null )
			{
				return;
			}

			Image hudResourceIcon = hudResourceIconTransform.GetComponent<Image>();
			TextMeshProUGUI hudResourceAmount = hudObj.hud.transform.Find( "Amount" ).GetComponent<TextMeshProUGUI>();

			// Make the inventory update the HUD wien resources are added/removed.
			this.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				for( int i = 0; i < this.slotCount; i++ )
				{
					if( this.slotGroups[i].isEmpty )
					{
						continue;
					}
					hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon;
					hudResourceAmount.text = "" + this.slotGroups[i].amount;

					hudResourceIcon.gameObject.SetActive( true );
					hudResourceAmount.gameObject.SetActive( true );
					break;
				}
			} );
			this.onRemove.AddListener( ( string id, int amtRemoved ) =>
			{
				if( this.isEmpty )
				{
					hudResourceIcon.gameObject.SetActive( false );
					hudResourceAmount.gameObject.SetActive( false );
				}
				else
				{
					for( int i = 0; i < this.slotCount; i++ )
					{
						if( this.slotGroups[i].isEmpty )
						{
							continue;
						}
						hudResourceIcon.sprite = DefinitionManager.GetResource( this.slotGroups[i].id ).icon;
						hudResourceAmount.text = "" + this.slotGroups[i].amount;
						break;
					}
				}
			} );
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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
				if( ret.ContainsKey( this.slotGroups[i].id ) )
				{
					ret[this.slotGroups[i].id] += this.slotGroups[i].amount;
				}
				else
				{
					ret.Add( this.slotGroups[i].id, this.slotGroups[i].amount );
				}
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
			List<int> indicesEmpty = new List<int>();
			List<int> indicesFull = new List<int>();
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						indicesEmpty.Add( i );
					}
				}
				else
				{// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						indicesFull.Add( i );
					}
				}
			}
			List<int> indices = new List<int>();
			indices.AddRange( indicesFull );
			indices.AddRange( indicesEmpty );

			int amountRemaining = amountMax;
			for( int i = 0; i < indices.Count; i++ )
			{
				int index = indices[i];
				int spaceInSlot = this.slotGroups[index].capacity - this.slotGroups[index].amount;
				int amountAdded = spaceInSlot > amountRemaining ? amountRemaining : spaceInSlot;

				this.slotGroups[index].amount += amountAdded;
				this.slotGroups[index].id = id;
				amountRemaining -= amountAdded;

				if( amountRemaining == 0 )
				{
					this.onAdd?.Invoke( id, amountMax );
					return amountMax;
				}
			}
			this.onAdd?.Invoke( id, amountMax - amountRemaining );
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

			int amountLeftToRemove = amountMax;
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					continue;
				}
				if( this.slotGroups[i].id == id )
				{
					int amountRemoved = this.slotGroups[i].amount > amountLeftToRemove ? amountLeftToRemove : this.slotGroups[i].amount;

					this.slotGroups[i].amount -= amountRemoved;
					if( this.slotGroups[i].amount == 0 )
					{
						this.slotGroups[i].id = "";
					}
					amountLeftToRemove -= amountRemoved;
					
					if( amountLeftToRemove == 0 )
					{
						this.onRemove?.Invoke( id, amountMax );
						return amountMax;
					}
				}
			}
			this.onRemove?.Invoke( id, amountMax - amountLeftToRemove );
			return amountMax - amountLeftToRemove;
		}

		public void Clear()
		{
			Dictionary<string, int> res = this.GetAll();

			for( int i = 0; i < this.slotCount; i++ )
			{
				this.slotGroups[i].id = "";
				this.slotGroups[i].amount = 0;
			}

			// Call the event after clearing, once per each type removed.
			foreach( var kvp in res )
			{
				this.onRemove?.Invoke( kvp.Key, kvp.Value ); // be consistent, all methods shall invoke after adding/removing.
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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

			this.icon = def.icon;
			this.slotGroups = new SlotGroup[def.slots.Length];
			for( int i = 0; i < this.slotCount; i++ )
			{
				this.slotGroups[i] = new SlotGroup( def.slots[i].slotId, def.slots[i].capacity );
			}

			// ------          DATA

			if( data.items != null )
			{
				if( data.items.Length != def.slots.Length )
				{
					throw new Exception( "Inventory slot count is not the same as data's slot count. Can't match the slots." );
				}
				for( int i = 0; i < data.items.Length; i++ )
				{
					this.slotGroups[i].id = data.items[i].id;
					this.slotGroups[i].amount = data.items[i].amount;
				}
			}
		}

#warning TODO! - inventories display items when displayed.
	}
}