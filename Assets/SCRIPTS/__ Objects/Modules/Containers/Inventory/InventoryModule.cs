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
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SS.Objects.Modules
{
    [DisallowMultipleComponent]
    [UseHud( typeof( HUDInventory ), "hudInventory" )]
    public sealed class InventoryModule : SSModule, ISelectDisplayHandler, IPopulationBlocker
    {
        public const string KFF_TYPEID = "inventory";

        public class Slot
        {
            /// <summary>
            /// The resource ID of a resource that's assigned to the slot (constrained).
            /// </summary>
            public readonly string slotId;

            /// <summary>
            /// The resource currently in the slot.
            /// </summary>
            public string id;

            /// <summary>
            /// The amount currently in the slot.
            /// </summary>
            public int amount;
            /// <summary>
            /// The maximum amount this slot can hold (this is the original value - in the definition).
            /// </summary>
            public int capacity;
            /// <summary>
            /// The maximum amount this slot can hold (this is custom value).
            /// </summary>
            public int? capacityOverride; // used by population scaling.

            /// <summary>
            /// Returns the actual capacity of the slot (capacity or override).
            /// </summary>
            public int actualCapacity
            {
                get
                {
                    return capacityOverride.HasValue ? capacityOverride.Value : capacity;
                }
            }

            /// <summary>
            /// Checks whether or not the slot is "constrained" (able to take only a specific resource).
            /// </summary>
            public bool isConstrained { get { return this.slotId != ""; } }

            /// <summary>
            /// Checks whether the slot is 'truly' empty.
            /// </summary>
            public bool isEmpty { get { return this.amount == 0 || this.id == ""; } }

            /// <summary>
            /// Checks if the slot is 'truly' full.
            /// </summary>
            public bool isFull { get { return this.amount == this.actualCapacity; } }

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
        /// Is the inventory marked as storage? - Used by faction cache
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
                        Dictionary<string, int> resources = this.GetAll();

                        // update the stored resources cache.
                        foreach( var resource in resources )
                        {
                            // if becomes storage - add, else - remove
                            if( value )
                                LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] += resource.Value;
                            else
                                LevelDataManager.factionData[fac.factionId].resourcesStoredCache[resource.Key] -= resource.Value;
                        }

                        // remove every slot from the storage space.
                        foreach( var slot in this.slots )
                        {
                            // if becomes storage - add, else - remove
                            if( value )
                                this.ProcessStorageSpaceSlot( slot, fac.factionId, 1 );
                            else
                                this.ProcessStorageSpaceSlot( slot, fac.factionId, -1 );
                        }
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
            List<Slot> ret = new List<Slot>();

            foreach( var slot in this.slots )
            {
                ret.Add( slot );
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Adds or removes the resources in a given slot rom the faction caches.
        /// </summary>
        /// <param name="addOrRemove">1 for "add", -1 for "remove"</param>
        private void ProcessStorageSpaceSlot( Slot slotToProcess, int factionId, int addOrRemove )
        {
            if( factionId >= LevelDataManager.factionData.Length || factionId < 0 )
            {
                Debug.LogWarning( $"Provided faction id of '{factionId}' is invalid." );
                return;
            }

            // if the slot is constrained - add/remove the slot's preferred resource from the cache.
            if( slotToProcess.isConstrained )
            {
                LevelDataManager.factionData[factionId].storageSpaceCache[slotToProcess.slotId] += addOrRemove * slotToProcess.actualCapacity;

                if( factionId == LevelDataManager.PLAYER_FAC )
                {
                    ResourcePanel.instance.UpdateResourceEntry( slotToProcess.slotId,
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[slotToProcess.slotId],
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[slotToProcess.slotId],
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[slotToProcess.slotId] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[slotToProcess.slotId] );
                }
            }
            // if the slot is unconstrained, but has stuff inside - add/remove from the specific resource entry.
            else if( !slotToProcess.isEmpty )
            {
                LevelDataManager.factionData[factionId].storageSpaceCache[slotToProcess.id] += addOrRemove * slotToProcess.actualCapacity;

                if( factionId == LevelDataManager.PLAYER_FAC )
                {
                    ResourcePanel.instance.UpdateResourceEntry( slotToProcess.id,
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[slotToProcess.id],
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[slotToProcess.id],
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[slotToProcess.id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[slotToProcess.id] );
                }
            }
            else
            {
                List<string> storedResources = new List<string>( LevelDataManager.factionData[factionId].storageSpaceCache.Keys );

                foreach( var key in storedResources )
                {
                    LevelDataManager.factionData[factionId].storageSpaceCache[key] += addOrRemove * slotToProcess.actualCapacity;

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
        public void SetSlots( Slot[] slots )
        {
            // remove previous slots (if any)
            if( this.slots != null )
            {
                if( this.isStorage )
                {
                    if( this.ssObject is IFactionMember )
                    {
                        IFactionMember fac = (IFactionMember)this.ssObject;
                        foreach( var slot in this.slots )
                        {
                            this.ProcessStorageSpaceSlot( slot, fac.factionId, -1 );
                        }
                    }
                }
            }

            // add new slots
            this.slots = new Slot[slots.Length];

            for( int i = 0; i < slots.Length; i++ )
            {
                this.slots[i] = slots[i];
            }

            if( this.isStorage )
            {
                if( this.ssObject is IFactionMember )
                {
                    IFactionMember fac = (IFactionMember)this.ssObject;
                    foreach( var slot in this.slots )
                    {
                        this.ProcessStorageSpaceSlot( slot, fac.factionId, 1 );
                    }
                }
            }
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
                            foreach( var slot in this.slots )
                            {
                                if( fromFac != SSObjectDFC.FACTIONID_INVALID )
                                {
                                    this.ProcessStorageSpaceSlot( slot, fromFac, -1 );
                                }

                                this.ProcessStorageSpaceSlot( slot, toFac, 1 );
                            }
                        }
                    }
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
                            foreach( var slot in this.slots )
                            {
                                if( usUnus.isUsable )
                                    this.ProcessStorageSpaceSlot( slot, fac.factionId, 1 );
                                else
                                    this.ProcessStorageSpaceSlot( slot, fac.factionId, -1 );
                            }
                        }
                    } );
                }
            }


            base.Awake();
        }

        public override void OnObjDestroyed()
        {
            Dictionary<string, int> allResources = this.GetAll();

            foreach( var resource in allResources )
            {
                TacticalDropOffGoal.ExtractAndDrop( this.transform.position, this.transform.rotation, resource.Key, resource.Value );
            }

            if( this.ssObject is IFactionMember )
            {
                IFactionMember fac = (IFactionMember)this.ssObject;

                foreach( var resource in allResources )
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
                        foreach( var slot in slots )
                        {
                            this.ProcessStorageSpaceSlot( slot, fac.factionId, -1 );
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
                // If any of the slots is not empty, the inventory is not empty.
                foreach( var slot in this.slots )
                {
                    if( !slot.isEmpty )
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
                // If any of the slots can hold something, the inventory is not empty.
                foreach( var slot in this.slots )
                {
                    if( slot.isEmpty )
                    {
                        return false;
                    }

                    if( slot.amount < slot.actualCapacity )
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the total amount of resource stored in this inventory.
        /// </summary>
        public int Get( string id )
        {
            if( string.IsNullOrEmpty( id ) )
            {
                throw new ArgumentNullException( "InventoryModule.Get - Id can't be null or empty." );
            }

            int total = 0;

            foreach( var slot in this.slots )
            {
                if( slot.id == id )
                {
                    total += slot.amount;
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

            Dictionary<string, int> resourceDict = new Dictionary<string, int>();

            foreach( var slot in this.slots )
            {
                if( slot.isEmpty )
                {
                    continue;
                }

                if( resourceDict.ContainsKey( slot.id ) )
                {
                    resourceDict[slot.id] += slot.amount;
                }
                else
                {
                    resourceDict.Add( slot.id, slot.amount );
                }
            }

            return resourceDict;
        }

        public int GetSpaceLeft( string id )
        {
            if( string.IsNullOrEmpty( id ) )
            {
                throw new ArgumentNullException( "InventoryModule.GetSpaceLeft - Id can't be null or empty." );
            }

            int total = 0;

            foreach( var slot in this.slots )
            {
                if( slot.isEmpty )
                {
                    // If it can hold this resource
                    if( !slot.isConstrained || slot.slotId == id )
                    {
                        total += slot.actualCapacity;
                    }
                }
                else
                {
                    // If it can hold this resource
                    if( (!slot.isConstrained && slot.id == id) || slot.slotId == id )
                    {
                        total += slot.actualCapacity - slot.amount;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// Tries to add a given amount of given resource to the inventory.
        /// </summary>
        /// <returns>The amount that was added</returns>
        public int Add( string id, int amountMax )
        {
            if( string.IsNullOrEmpty( id ) )
            {
                throw new ArgumentNullException( "InventoryModule.Add - Id can't be null or empty." );
            }

            if( amountMax < 1 )
            {
                throw new ArgumentOutOfRangeException( "InventoryModule.Add - Amount can't be less than 1." );
            }

            List<Slot> slotsSorted = new List<Slot>( this.slots );
            slotsSorted.Sort( ( s1, s2 ) => (s1.actualCapacity - s1.amount).CompareTo( (s2.actualCapacity - s2.amount) ) );

            int amountRemaining = amountMax;

            foreach( var slot in slotsSorted )
            {
                if( slot.id == id || slot.slotId == id || slot.isEmpty )
                {
                    int spaceInSlot = slot.actualCapacity - slot.amount;

                    int amountAdded = (spaceInSlot > amountRemaining) ? amountRemaining : spaceInSlot;

                    // --  --  --

                    if( this.ssObject is IFactionMember )
                    {
                        IFactionMember fac = (IFactionMember)this.ssObject;

                        LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] += amountAdded;

                        if( this.isStorage )
                        {
                            LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] += amountAdded;

                            // if the slot WAS empty and is not constrained - remove the capacity from every resource's cache (except the one being added)
                            if( !slot.isConstrained && slot.isEmpty )
                            {
                                // resources stored by the player.
                                List<string> storedResourceIds = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );

                                foreach( var key in storedResourceIds )
                                {
                                    if( key == id )
                                    {
                                        continue;
                                    }

                                    LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] -= slot.actualCapacity;

                                    if( fac.factionId == LevelDataManager.PLAYER_FAC ) // only update if the inventory changed was player's (it would update with correct value, but unnecessary assignment)
                                    {
                                        ResourcePanel.instance.UpdateResourceEntry( key,
                                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key],
                                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key],
                                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[key] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[key] );
                                    }
                                }
                            }
                        }

                        if( fac.factionId == LevelDataManager.PLAYER_FAC ) // only update if the inventory changed was player's (it would update with correct value, but unnecessary assignment)
                        {
                            ResourcePanel.instance.UpdateResourceEntry( id,
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id],
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id],
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id] );
                        }
                    }

                    // --  --  --

                    slot.amount += amountAdded;
                    slot.id = id;

                    amountRemaining -= amountAdded;

                    if( amountRemaining == 0 )
                    {
                        this.onAdd?.Invoke( id, amountMax );
                        this.TryUpdateSlots_UI();

                        return amountMax;
                    }
                }
            }

            this.onAdd?.Invoke( id, amountMax - amountRemaining );
            this.TryUpdateSlots_UI();

            return amountMax - amountRemaining;
        }

        /// <summary>
        /// Tries to remove a given amount of given resource from the inventory.
        /// </summary>
        /// <returns>The amount that was removed</returns>
        public int Remove( string id, int amountMax )
        {
            if( string.IsNullOrEmpty( id ) )
            {
                throw new ArgumentNullException( "InventoryModule.Remove - Id can't be null or empty." );
            }

            if( amountMax < 1 )
            {
                throw new ArgumentOutOfRangeException( "InventoryModule.Remove - Amount can't be less than 1." );
            }

            List<Slot> slotsSorted = new List<Slot>( this.slots );
            slotsSorted.Sort( ( s1, s2 ) => -(s1.actualCapacity - s1.amount).CompareTo( (s2.actualCapacity - s2.amount) ) );

            int amountLeftToRemove = amountMax;

            foreach( var slot in slotsSorted )
            {
#warning - ideally it'd remove from the empty-most slot first - this is easy to do, just feed it the slots in a specific order.
                if( slot.isEmpty )
                {
                    continue;
                }
                if( slot.id == id )
                {
                    int amountRemoved = (slot.amount > amountLeftToRemove) ? amountLeftToRemove : slot.amount;

                    slot.amount -= amountRemoved;

                    // --  --  --

                    if( this.ssObject is IFactionMember )
                    {
                        IFactionMember fac = (IFactionMember)this.ssObject;

                        LevelDataManager.factionData[fac.factionId].resourcesAvailableCache[id] -= amountRemoved;

                        if( this.isStorage )
                        {
                            LevelDataManager.factionData[fac.factionId].resourcesStoredCache[id] -= amountRemoved;

                            if( slot.amount == 0 )
                            {
                                // if the slot became empty and is not constrained - add the capacity to every resource's cache
                                if( !slot.isConstrained && slot.isEmpty )
                                {
                                    List<string> storedResourceIds = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );

                                    foreach( var key in storedResourceIds )
                                    {
                                        if( key == slot.id )
                                        {
                                            continue;
                                        }

                                        LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] += slot.actualCapacity;

                                        if( fac.factionId == LevelDataManager.PLAYER_FAC ) // only update if the inventory changed was player's (it would update with correct value, but unnecessary assignment)
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

                        // update removed resource (every time it was partially removed currencly)
                        if( fac.factionId == LevelDataManager.PLAYER_FAC ) // only update if the inventory changed was player's (it would update with correct value, but unnecessary assignment)
                        {
                            ResourcePanel.instance.UpdateResourceEntry( id,
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id],
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id],
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].resourcesAvailableCache[id] >= LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].storageSpaceCache[id] );
                        }
                    }

                    // --  --  --

                    if( slot.amount == 0 )
                    {
                        slot.id = "";
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

            foreach( var slot in slots )
            {
                slot.id = "";
                slot.amount = 0;
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
                                List<string> storedResources = new List<string>( LevelDataManager.factionData[fac.factionId].storageSpaceCache.Keys );

                                foreach( var key in storedResources )
                                {
                                    if( key == this.slots[i].id )
                                    {
                                        continue;
                                    }

#warning extract this to a method.
                                    LevelDataManager.factionData[fac.factionId].storageSpaceCache[key] -= this.slots[i].actualCapacity;

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