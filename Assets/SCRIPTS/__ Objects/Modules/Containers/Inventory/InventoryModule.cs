using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.ResourceSystem;
using SS.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects.Modules
{
	public sealed class InventoryModule : SSModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "inventory";

		public struct SlotGroup
		{
			public readonly string slotId;
			public string id;
			public int amount;
			public int capacity; // this is original value in definition.
			public int? capacityOverride; // this is custom value.

			public bool isConstrained { get { return this.slotId != ""; } }
			public bool isEmpty { get { return this.amount == 0 || this.id == ""; } }

			public SlotGroup( string slotId, int slotCapacity )
			{
				this.slotId = slotId ?? "";
				this.id = "";
				this.capacity = slotCapacity;
				this.capacityOverride = null;
				this.amount = 0;
			}
		}
		
		SlotGroup[] slotGroups;


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

		bool __isStorage;
		/// <summary>
		/// Is the inventory marked as storage?
		/// </summary>
		public bool isStorage
		{
			get
			{
				return this.__isStorage;
			}
			set
			{
				if( value == this.__isStorage )
				{
					return;
				}

				// if the object belongs to a faction - update the stored resources cache.
				if( this.ssObject is IFactionMember )
				{
					IFactionMember fac = (IFactionMember)this.ssObject;
					Dictionary<string, int> res = this.GetAll();
					foreach( var resource in res )
					{
						if( value )
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] += resource.Value;
						else
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] -= resource.Value;
					}
				}

				this.__isStorage = value;
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
			this.slotGroups = new SlotGroup[slotGroups.Length];
			for( int i = 0; i < slotGroups.Length; i++ )
			{
				this.slotGroups[i] = slotGroups[i];
			}
		}



		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		public _UnityEvent_string_int onAdd { get; private set; } = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get; private set; } = new _UnityEvent_string_int();


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		
		
		private void UpdateHud( HUDInventory hudInventory )
		{
			if( this.isEmpty )
			{
				hudInventory.HideResource();
			}
			else
			{
				Dictionary<string, int> res = this.GetAll();
				foreach( var kvp in res )
				{
					hudInventory.DisplayResource( DefinitionManager.GetResource( kvp.Key ), kvp.Value, res.Count > 1 );
					return;
				}
			}
		}
		
		private void RegisterHUD()
		{
			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;


			HUDInventory hudInventory = hudObj.hud.GetComponent<HUDInventory>();
			if( hudInventory != null )
			{
				// Make the inventory update the HUD wien resources are added/removed.
				this.onAdd.AddListener( ( string id, int amtAdded ) =>
				{
					this.UpdateHud( hudInventory );
				} );
				this.onRemove.AddListener( ( string id, int amtRemoved ) =>
				{
					this.UpdateHud( hudInventory );
				} );
			}
		}

		void Awake()
		{
			// needs to be registered on Awake(), since the resources (data) are assigned before Start() happens.

			if( this.ssObject is IHUDHolder )
			{
				this.RegisterHUD();
			}
			

			if( this.ssObject is IFactionMember )
			{
				IFactionMember fac = (IFactionMember)this.ssObject;
				this.onAdd.AddListener( ( string id, int amount ) =>
				{
					// will just throw an exception if non-existing resource is added/removed.
					LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] += amount;
					if( this.isStorage )
					{
						LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] += amount;
					}

					if( fac.factionId == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdateResourceEntry( id,
							LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id],
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] );
					}
				} );
				this.onRemove.AddListener( ( string id, int amount ) =>
				{
					// will just throw an exception if non-existing resource is added/removed.
					LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] -= amount;
					if( this.isStorage )
					{
						LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] -= amount;
					}

					if( fac.factionId == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdateResourceEntry( id,
							LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id],
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] );
					}
				} );
				fac.onFactionChange.AddListener( ( int fromFac, int toFac ) =>
				{
					Dictionary<string, int> res = this.GetAll();
					foreach( var resource in res )
					{
						LevelDataManager.factionData[fromFac].resourcesStoredCache[resource.Key] -= resource.Value;
						LevelDataManager.factionData[toFac].resourcesStoredCache[resource.Key] += resource.Value;

						ResourcePanel.instance.UpdateResourceEntry( resource.Key,
							LevelDataManager.factionData[toFac].resourcesAvailableCache[resource.Key],
							LevelDataManager.factionData[toFac].resourcesStoredCache[resource.Key] );
					}
				} );
			}
		}

		public override void OnObjDestroyed()
		{
			TacticalDropOffGoal.ExtractAndDrop( this.transform.position, this.transform.rotation, this.GetAll() );

			if( this.ssObject is IFactionMember )
			{
				IFactionMember fac = (IFactionMember)this.ssObject;

				Dictionary<string, int> res = this.GetAll();
				foreach( var resource in res )
				{
					LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[resource.Key] -= resource.Value;
					if( this.isStorage )
					{
						LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] -= resource.Value;
					}

					if( fac.factionId == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdateResourceEntry( resource.Key,
							LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[resource.Key],
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] );
					}
				}
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-


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
		
		public int GetSpaceLeft( string id )
		{
			if( string.IsNullOrEmpty( id ) )
			{
				throw new ArgumentNullException( "Id can't be null or empty." );
			}

			int total = 0;
			for( int i = 0; i < this.slotCount; i++ )
			{
				int realCapacity = this.slotGroups[i].capacityOverride == null ? this.slotGroups[i].capacity : this.slotGroups[i].capacityOverride.Value;
				if( this.slotGroups[i].isEmpty )
				{
					if( !this.slotGroups[i].isConstrained || this.slotGroups[i].slotId == id )
					{
						total += realCapacity;
					}
				}
				else
				{
					// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slotGroups[i].isConstrained && this.slotGroups[i].id == id) || this.slotGroups[i].slotId == id )
					{
						total += realCapacity - this.slotGroups[i].amount;
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
				int spaceInSlot = (this.slotGroups[i].capacityOverride == null ? this.slotGroups[i].capacity : this.slotGroups[i].capacityOverride.Value) - this.slotGroups[index].amount;
				int amountAdded = spaceInSlot > amountRemaining ? amountRemaining : spaceInSlot;

				this.slotGroups[index].amount += amountAdded;
				this.slotGroups[index].id = id;
				amountRemaining -= amountAdded;

				if( amountRemaining == 0 )
				{
					this.onAdd?.Invoke( id, amountMax );
					this.TryUpdateSlots_UI();
					return amountMax;
				}
			}
			this.onAdd?.Invoke( id, amountMax - amountRemaining );
			this.TryUpdateSlots_UI();
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
						this.TryUpdateSlots_UI();
						return amountMax;
					}
				}
			}
			this.onRemove?.Invoke( id, amountMax - amountLeftToRemove );
			this.TryUpdateSlots_UI();
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
			this.TryUpdateSlots_UI();
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

		public override void SetData( ModuleData _data )
		{
			InventoryModuleData data = ValidateDataType<InventoryModuleData>( _data );

			// ------          DATA

			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;

			HUDInventory hudInventory = hudObj.hud.GetComponent<HUDInventory>();

			if( data.items != null )
			{
				if( data.items.Length != this.slotCount )
				{
					throw new Exception( "Inventory slot count is not the same as data's slot count. Can't match the slots '1 to 1'." );
				}
				for( int i = 0; i < data.items.Length; i++ )
				{
					this.slotGroups[i].id = data.items[i].id;
					this.slotGroups[i].amount = data.items[i].amount;

					if( data.items[i].capacityOverride != null )
					{
						this.slotGroups[i].capacityOverride = data.items[i].capacityOverride;
					}

					if( this.ssObject is IFactionMember )
					{
						IFactionMember fac = (IFactionMember)this.ssObject;
						if( fac.factionId == LevelDataManager.PLAYER_FAC )
						{
							if( this.slotGroups[i].isEmpty )
							{
								continue;
							}
							LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[this.slotGroups[i].id] += this.slotGroups[i].amount;
							if( this.isStorage )
							{
								LevelDataManager.factionData[fac.factionId].resourcesStoredCache[this.slotGroups[i].id] += this.slotGroups[i].amount;
							}

							ResourcePanel.instance.UpdateResourceEntry( this.slotGroups[i].id,
								LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[this.slotGroups[i].id],
								LevelDataManager.factionData[fac.factionId].resourcesStoredCache[this.slotGroups[i].id] );
						}
					}
				}
			}
			
			if( hudInventory != null )
			{
				this.UpdateHud( hudInventory );
			}
		}


		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

		
		private void TryUpdateSlots_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			if( !this.ssObject.IsDisplaySafe() )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "inventory.slots" );
				
			this.ShowList();
		}

		private void ShowList()
		{
			GameObject[] gridElements = new GameObject[this.slotCount];
			// Initialize the grid elements' GameObjects.
			for( int i = 0; i < this.slotCount; i++ )
			{
				if( this.slotGroups[i].isEmpty )
				{
					gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/empty_resource" ) );

					UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), " - " );
					continue;
				}
				ResourceDefinition resDef = DefinitionManager.GetResource( this.slotGroups[i].id );
				gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), resDef.icon );

				int realCapacity = this.slotGroups[i].capacityOverride == null ? this.slotGroups[i].capacity : this.slotGroups[i].capacityOverride.Value;
				UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), this.slotGroups[i].amount + " / " + realCapacity );
			}

			GameObject list = UIUtils.InstantiateScrollableList( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 300.0f, 5.0f ), new Vector2( -330.0f, -55.0f ), Vector2.zero, Vector2.zero, Vector2.one ), gridElements );
			SelectionPanel.instance.obj.RegisterElement( "inventory.slots", list.transform );
		}

		public void OnDisplay()
		{
			if( !this.ssObject.IsDisplaySafe() )
			{
				return;
			}

			this.ShowList();
		}

		public void OnHide()
		{

		}
	}
}