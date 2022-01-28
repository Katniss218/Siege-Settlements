using SS.AI.Goals;
using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using SS.ResourceSystem;
using SS.UI;
using SS.UI.HUDs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects.Modules
{
	[DisallowMultipleComponent]
	[UseHud( typeof( HUDInventory ), "hudInventory" )]
	public sealed class InventoryModule : SSModule, ISelectDisplayHandler, IPopulationBlocker
	{
		public const string KFF_TYPEID = "inventory";

		public struct Slot
		{
			public readonly string slotId;
			public string id;
			public int amount;
			public int capacity; // this is original value in definition.
			public int? capacityOverride; // this is custom value.

			public bool isConstrained { get { return this.slotId != ""; } }
			public bool isEmpty { get { return this.amount == 0 || this.id == ""; } }

			public Slot( string slotId, int slotCapacity )
			{
				this.slotId = slotId ?? "";
				this.id = "";
				this.capacity = slotCapacity;
				this.capacityOverride = null;
				this.amount = 0;
			}
		}

		Slot[] slots = null;


		/// <summary>
		/// Returns the amount of slots of this inventory.
		/// </summary>
		public int slotCount
		{
			get
			{
				return this.slots.Length;
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
				if( this.__isStorage == value )
				{
					return;
				}

				if( this.ssObject is IFactionMember )
				{
					IFactionMember fac = (IFactionMember)this.ssObject;
					
					if( fac.factionId != SSObjectDFC.FACTIONID_INVALID )
					{
						// --  --  --
						// if the object belongs to a faction - update the stored resources cache.
						Dictionary<string, int> res = this.GetAll();
						foreach( var resource in res )
						{
							// if becomes storage - add, else - remove
							if( value )
								LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] += resource.Value;
							else
								LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] -= resource.Value;
						}

						// --  --  --
						// remove every slot from the storage space.
						for( int i = 0; i < this.slotCount; i++ )
						{
							// if becomes storage - add, else - remove
							if( value )
								this.ProcessStorageSpaceSlot( i, fac.factionId, 1 );
							else
								this.ProcessStorageSpaceSlot( i, fac.factionId, -1 );
						}

						// --  --  --
					}
				}

				this.__isStorage = value;
			}
		}

		/// <summary>
		/// Returns a copy of every slot in the inventory.
		/// </summary>
		public Slot[] GetSlots()
		{
			Slot[] ret = new Slot[this.slotCount];
			for( int i = 0; i < this.slotCount; i++ )
			{
				ret[i] = this.slots[i];
			}
			return ret;
		}

		private void ProcessStorageSpaceSlot( int index, int factionId, int mode )
		{
			if( factionId >= LevelDataManager.factionData.Length || factionId < 0 )
			{
				Debug.LogWarning( "Tried accessing faction id of '" + factionId + "', failed." );
				return;
			}

			int realCapacity = this.slots[index].capacityOverride == null ? this.slots[index].capacity : this.slots[index].capacityOverride.Value;

			// if the slot is constrained - add/remove the slot's preferred resource from the cache.
			if( this.slots[index].isConstrained )
			{
				LevelDataManager.factionData[factionId].storageSpaceCache[this.slots[index].slotId] += mode * realCapacity;

				if( factionId == LevelDataManager.PLAYER_FAC )
				{
					ResourcePanel.instance.UpdateResourceEntry( this.slots[index].slotId,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[this.slots[index].slotId],
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[this.slots[index].slotId],
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[this.slots[index].slotId] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[this.slots[index].slotId] );
				}
			}
			// if the slot is unconstrained, but has stuff inside - add/remove from the specific resource entry.
			else if( !this.slots[index].isEmpty )
			{
				LevelDataManager.factionData[factionId].storageSpaceCache[this.slots[index].id] += mode * realCapacity;

				if( factionId == LevelDataManager.PLAYER_FAC )
				{
					ResourcePanel.instance.UpdateResourceEntry( this.slots[index].id,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[this.slots[index].id],
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[this.slots[index].id],
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[this.slots[index].id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[this.slots[index].id] );
				}
			}
			else
			{
				List<string> keys = new List<string>( LevelDataManager.factionData[factionId].storageSpaceCache.Keys );
				foreach( var key in keys )
				{
					LevelDataManager.factionData[factionId].storageSpaceCache[key] += mode * realCapacity;

					if( factionId == LevelDataManager.PLAYER_FAC )
					{
						ResourcePanel.instance.UpdateResourceEntry( key,
							LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key],
							LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key],
							LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key] );
					}
				}
			}
		}

		/// <summary>
		/// Sets the slots to the copy of the array.
		/// </summary>
		public void SetSlots( Slot[] slotGroups )
		{
			// --  --  --
			// remove previous slots (if any)
			if( this.slots != null )
			{
				if( this.isStorage )
				{
					if( this.ssObject is IFactionMember )
					{
						IFactionMember fac = (IFactionMember)this.ssObject;
						for( int i = 0; i < this.slotCount; i++ )
						{
							this.ProcessStorageSpaceSlot( i, fac.factionId, -1 );
						}
					}
				}
			}

			// --  --  --

			this.slots = new Slot[slotGroups.Length];
			for( int i = 0; i < slotGroups.Length; i++ )
			{
				this.slots[i] = slotGroups[i];
			}

			// --  --  --
			// add new slots

			if( this.isStorage )
			{
				if( this.ssObject is IFactionMember )
				{
					IFactionMember fac = (IFactionMember)this.ssObject;
					for( int i = 0; i < this.slotCount; i++ )
					{
						this.ProcessStorageSpaceSlot( i, fac.factionId, 1 );
					}
				}
			}

			// --  --  --
		}



		public class _UnityEvent_string_int : UnityEvent<string, int> { }

		public _UnityEvent_string_int onAdd { get; private set; } = new _UnityEvent_string_int();
		public _UnityEvent_string_int onRemove { get; private set; } = new _UnityEvent_string_int();


		public HUDInventory hudInventory { get; set; }

		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-
		// -=-  -  -=-  -  -=-  -  -=-  -  -=-  -  -=-

#warning TODO - when taking from a warehouse, it adds however much was taken to the total amount of resource
		private void UpdateHud()
		{
			if( this.isEmpty )
			{
				this.hudInventory.HideResource();
			}
			else
			{
				Dictionary<string, int> res = this.GetAll();
				foreach( var kvp in res )
				{
					this.hudInventory.DisplayResource( DefinitionManager.GetResource( kvp.Key ), kvp.Value, res.Count > 1 );
					return;
				}
			}
		}

		protected override void Awake()
		{
			// Make the inventory update the HUD wien resources are added/removed.
			this.onAdd.AddListener( ( string id, int amtAdded ) =>
			{
				this.UpdateHud();
			} );
			this.onRemove.AddListener( ( string id, int amtRemoved ) =>
			{
				this.UpdateHud();
			} );

			if( this.ssObject is IFactionMember )
			{
				IFactionMember fac = (IFactionMember)this.ssObject;

				fac.onFactionChange.AddListener( ( int fromFac, int toFac ) =>
				{
					Dictionary<string, int> res = this.GetAll();
					foreach( var resource in res )
					{
						LevelDataManager.factionData[fromFac].resourcesAvailableCache[resource.Key] -= resource.Value;
						LevelDataManager.factionData[toFac].resourcesAvailableCache[resource.Key] += resource.Value;

						LevelDataManager.factionData[fromFac].resourcesStoredCache[resource.Key] -= resource.Value;
						LevelDataManager.factionData[toFac].resourcesStoredCache[resource.Key] += resource.Value;
					}

					// --  --  --
					// "move" the storage space from old faction to new faction.

					if( this.isStorage )
					{
						// Don't move if the object is not usable (was removed from current fac already when it was set to unusable).
						if( !(this.ssObject is ISSObjectUsableUnusable) || (((ISSObjectUsableUnusable)this.ssObject).isUsable) )
						{
							for( int i = 0; i < this.slotCount; i++ )
							{
								if( fromFac != SSObjectDFC.FACTIONID_INVALID )
								{
									this.ProcessStorageSpaceSlot( i, fromFac, -1 );
								}
								this.ProcessStorageSpaceSlot( i, toFac, 1 );
							}
						}
					}

					// --  --  --
				} );

				if( this.ssObject is ISSObjectUsableUnusable )
				{
					ISSObjectUsableUnusable usUnus = (ISSObjectUsableUnusable)this.ssObject;

					usUnus.onUsableStateChanged.AddListener( () =>
					{
						if( this.isStorage )
						{
							if( fac.factionId == SSObjectDFC.FACTIONID_INVALID )
							{
								return;
							}
							for( int i = 0; i < this.slotCount; i++ )
							{
								if( usUnus.isUsable )
									this.ProcessStorageSpaceSlot( i, fac.factionId, 1 );
								else
									this.ProcessStorageSpaceSlot( i, fac.factionId, -1 );
							}
						}
					} );
				}
			}


			base.Awake();
		}

		public override void OnObjDestroyed()
		{
			foreach( var kvp in this.GetAll() )
			{
				TacticalDropOffGoal.ExtractAndDrop( this.transform.position, this.transform.rotation, kvp.Key, kvp.Value );
			}

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
				}

				if( !(this.ssObject is ISSObjectUsableUnusable) || (((ISSObjectUsableUnusable)this.ssObject).isUsable) )
				{
					if( this.isStorage )
					{
						for( int i = 0; i < this.slotCount; i++ )
						{
							this.ProcessStorageSpaceSlot( i, fac.factionId, -1 );
						}
					}
				}
			}

			base.OnObjDestroyed();
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
					if( !this.slots[i].isEmpty )
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool isFull
		{
			get
			{
				// If any of the slots is not empty (i.e. contains something, i.e. slot's amount is >0), then the whole inventory is not empty.
				for( int i = 0; i < this.slotCount; i++ )
				{
					if( this.slots[i].isEmpty )
					{
						return false;
					}
					int realCapacity = this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value;
					if( this.slots[i].amount < realCapacity )
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
				if( this.slots[i].id == id )
				{
					total += this.slots[i].amount;
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
				if( this.slots[i].isEmpty )
				{
					continue;
				}
				if( ret.ContainsKey( this.slots[i].id ) )
				{
					ret[this.slots[i].id] += this.slots[i].amount;
				}
				else
				{
					ret.Add( this.slots[i].id, this.slots[i].amount );
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
				int realCapacity = this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value;
				if( this.slots[i].isEmpty )
				{
					if( !this.slots[i].isConstrained || this.slots[i].slotId == id )
					{
						total += realCapacity;
					}
				}
				else
				{
					// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slots[i].isConstrained && this.slots[i].id == id) || this.slots[i].slotId == id )
					{
						total += realCapacity - this.slots[i].amount;
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
				if( this.slots[i].isEmpty )
				{
					if( !this.slots[i].isConstrained || this.slots[i].slotId == id )
					{
						indicesEmpty.Add( i );
					}
				}
				else
				{// if it can take any type, but only when there is no invalid type already there. OR if it only takes that valid type (can resource be placed in slot).
					if( (!this.slots[i].isConstrained && this.slots[i].id == id) || this.slots[i].slotId == id )
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
				int spaceInSlot = (this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value) - this.slots[index].amount;
				int amountAdded = spaceInSlot > amountRemaining ? amountRemaining : spaceInSlot;

				// --  --  --
				// if the slot is not constrained:       if the slot was previously empty - remove that slot from the storage space cache (every resource except the newly added one - since it now can hold only that resource).

				if( this.ssObject is IFactionMember )
				{
					IFactionMember fac = (IFactionMember)this.ssObject;
					LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] += amountAdded;
					if( this.isStorage )
					{
						LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] += amountAdded;
						if( !this.slots[index].isConstrained )
						{
							if( this.slots[index].isEmpty )
							{
								int realCapacity = this.slots[index].capacityOverride == null ? this.slots[index].capacity : this.slots[index].capacityOverride.Value;

								List<string> keys = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );
								foreach( var key in keys )
								{
									if( key == id )
									{
										continue;
									}
									LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] -= realCapacity;

									if( fac.factionId == LevelDataManager.PLAYER_FAC )
									{
										ResourcePanel.instance.UpdateResourceEntry( key,
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key],
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key],
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key] );
									}
								}
							}
						}
					}
					else
					{
						if( fac.factionId == LevelDataManager.PLAYER_FAC )
						{
							ResourcePanel.instance.UpdateResourceEntry( id,
								LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id],
								LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id],
								LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id] );
						}
					}
				}

				// --  --  --

				this.slots[index].amount += amountAdded;
				this.slots[index].id = id;
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
				if( this.slots[i].isEmpty )
				{
					continue;
				}
				if( this.slots[i].id == id )
				{
					int amountRemoved = this.slots[i].amount > amountLeftToRemove ? amountLeftToRemove : this.slots[i].amount;

					this.slots[i].amount -= amountRemoved;
					
					if( this.slots[i].amount == 0 )
					{

						// --  --  --
						// if the slot is not constrained:          if the slot is now empty - add that slot to the storage space cache (add every resource except the one that was there previously, since it's already added).
				
						if( this.ssObject is IFactionMember )
						{
							IFactionMember fac = (IFactionMember)this.ssObject;
							LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] -= amountRemoved;
							if( this.isStorage )
							{
								LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] -= amountRemoved;
								if( !this.slots[i].isConstrained )
								{
									if( this.slots[i].isEmpty )
									{
										int realCapacity = this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value;

										List<string> keys = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );
										foreach( var key in keys )
										{
											if( key == this.slots[i].id )
											{
												continue;
											}

											LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] += realCapacity;
											if( fac.factionId == LevelDataManager.PLAYER_FAC )
											{
												ResourcePanel.instance.UpdateResourceEntry( key,
													LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key],
													LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key],
													LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key] );
											}
										}
									}
								}
							}
							else
							{
								if( fac.factionId == LevelDataManager.PLAYER_FAC )
								{
									ResourcePanel.instance.UpdateResourceEntry( id,
										LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id],
										LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id],
										LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id] );
								}
							}
						}
						
						// --  --  --

						this.slots[i].id = "";
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
				this.slots[i].id = "";
				this.slots[i].amount = 0;
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


		public override SSModuleData GetData()
		{
			InventoryModuleData data = new InventoryModuleData();

			data.items = new InventoryModuleData.SlotData[this.slotCount];
			for( int i = 0; i < data.items.Length; i++ )
			{
				data.items[i] = new InventoryModuleData.SlotData( this.slots[i] );
			}

			return data;
		}

		public override void SetData( SSModuleData _data )
		{
			InventoryModuleData data = ValidateDataType<InventoryModuleData>( _data );

			// ------          DATA

			if( data.items != null )
			{
				if( data.items.Length != this.slotCount )
				{
					throw new Exception( "Inventory slot count is not the same as data's slot count. Can't match the slots '1 to 1'." );
				}
				for( int i = 0; i < data.items.Length; i++ )
				{
					this.slots[i].id = data.items[i].id;
					this.slots[i].amount = data.items[i].amount;

					if( this.ssObject is IFactionMember )
					{
						IFactionMember fac = (IFactionMember)this.ssObject;

						if( this.slots[i].isEmpty )
						{
							continue;
						}
						LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[this.slots[i].id] += this.slots[i].amount;
						if( this.isStorage )
						{
							LevelDataManager.factionData[fac.factionId].resourcesStoredCache[this.slots[i].id] += this.slots[i].amount;
							

							// --  --  --

							if( !this.slots[i].isConstrained )
							{
								int realCapacity = this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value;

								List<string> keys = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );
								foreach( var key in keys )
								{
									if( key == this.slots[i].id )
									{
										continue;
									}
									LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] -= realCapacity;
									if( fac.factionId == LevelDataManager.PLAYER_FAC )
									{
										ResourcePanel.instance.UpdateResourceEntry( key,
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key],
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key],
											LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key] );
									}
								}
							}
						}
					}
				}
			}

			this.UpdateHud();
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
				if( this.slots[i].isEmpty )
				{
					gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/empty_resource" ) );

					UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), " - " );
					continue;
				}
				ResourceDefinition resDef = DefinitionManager.GetResource( this.slots[i].id );
				gridElements[i] = UIUtils.InstantiateIcon( SelectionPanel.instance.obj.transform, new GenericUIData( Vector2.zero, new Vector2( 32.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), resDef.icon );

				int realCapacity = this.slots[i].capacityOverride == null ? this.slots[i].capacity : this.slots[i].capacityOverride.Value;
				UIUtils.InstantiateText( gridElements[i].transform, new GenericUIData( new Vector2( 32.0f, 0.0f ), new Vector2( 320.0f, 32.0f ), Vector2.zero, Vector2.zero, Vector2.zero ), this.slots[i].amount + " / " + realCapacity );
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

		public bool CanChangePopulation()
		{
			return this.isEmpty;
		}
	}
}