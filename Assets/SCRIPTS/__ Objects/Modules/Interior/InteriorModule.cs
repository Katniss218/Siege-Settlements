using SS.Content;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using SS.UI;
using SS.UI.HUDs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
    [DisallowMultipleComponent]
    [UseHud( typeof( HUDInterior ), "hudInterior" )]
    public class InteriorModule : SSModule, ISelectDisplayHandler, IPopulationBlocker
    {
        public const string KFF_TYPEID = "interior";

        public enum SlotType : byte
        {
            /// <summary>
            /// Any unit can enter (whitelist-based).
            /// </summary>
            Generic,
            /// <summary>
            /// Only the unit employed at this particular SLOT can enter.
            /// </summary>
            Worker
        }

        public abstract class Slot
        {
            public Vector3 localPos { get; set; }
            public Quaternion localRot { get; set; }

            public IInteriorUser objInside;

            public PopulationSize maxPopulation { get; set; } = PopulationSize.x1;

            public float coverValue { get; set; }

            public bool isHidden { get; set; }

            public bool isEmpty
            {
                get
                {
                    return this.objInside == null;
                }
            }
        }

        public class SlotGeneric : Slot
        {
            public bool countsTowardsMaxPopulation { get; set; }

            public string[] whitelistedUnits { get; set; } = new string[0];

            /// <summary>
            /// Returns the total amount of population from slots that count towards it.
            /// </summary>
            public static int GetMaxPopulationTotal( SlotGeneric[] slots )
            {
                int accumulator = 0;

                foreach( var slot in slots )
                {
                    if( slot.countsTowardsMaxPopulation )
                    {
                        accumulator += (int)slot.maxPopulation;
                    }
                }

                return accumulator;
            }
        }

        public class SlotWorker : Slot
        {
            /// <summary>
            /// The worker that's assigned to the slot (but can be elsewhere at the moment).
            /// </summary>
            public CivilianUnitExtension assignedWorker { get; set; }
        }



        /// <summary>
        /// Specifies the local-space position of the entrance.
        /// </summary>
        public Vector3? entrancePosition { get; set; }

        SlotGeneric[] slots = new SlotGeneric[0];
        SlotWorker[] workerSlots = new SlotWorker[0];

        /// <summary>
        /// Returns a given slot.
        /// </summary>
        public (Slot slot, HUDInteriorSlot slotHud) GetSlot( SlotType slotType, int slotIndex )
        {
            if( slotType == SlotType.Generic )
            {
                return (this.slots[slotIndex], this.hudInterior.slots[slotIndex]);
            }
            if( slotType == SlotType.Worker )
            {
                return (this.workerSlots[slotIndex], this.hudInterior.workerSlots[slotIndex]);
            }

            throw new ArgumentException( "Can't get slot - invalid slot type." );
        }

        /// <summary>
        /// Returns the number of slots of a given type.
        /// </summary>
        public int SlotCount( SlotType type )
        {
            return type == SlotType.Generic ? this.slots.Length : this.workerSlots.Length;
        }

        /// <summary>
        /// Sets the slots of this interior.
        /// </summary>
        public void SetSlots( SlotGeneric[] slots, SlotWorker[] workerSlots )
        {
            if( slots == null )
            {
                slots = new SlotGeneric[0];
            }
            if( workerSlots == null )
            {
                workerSlots = new SlotWorker[0];
            }

            // Update the faction population caches
            if( this.ssObject is IFactionMember )
            {
                if( !(this.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.ssObject).isUsable) )
                {
                    IFactionMember factionMember = (IFactionMember)this.ssObject;

                    if( factionMember.factionId != SSObjectDFC.FACTIONID_INVALID )
                    {
                        LevelDataManager.factionData[factionMember.factionId].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );
                        LevelDataManager.factionData[factionMember.factionId].maxPopulationCache += SlotGeneric.GetMaxPopulationTotal( slots );
                    }
                    if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
                    {
                        ResourcePanel.instance.UpdatePopulationDisplay(
                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
                    }
                }
            }

            this.slots = slots;
            this.workerSlots = workerSlots;

            this.hudInterior.SetSlotCount( this.slots.Length, this.workerSlots.Length );

            ReDisplaySlots_UI();
        }

        /// <summary>
        /// Returns the (world) position associated with a given slot.
        /// </summary>
        public Vector3 SlotWorldPosition( Slot slot )
        {
            return this.transform.TransformPoint( slot.localPos );
        }

        /// <summary>
        /// Returns the (world) rotation associated with a given slot.
        /// </summary>
        public Quaternion SlotWorldRotation( Slot slot )
        {
            return this.transform.rotation * slot.localRot;
        }

        /// <summary>
        /// Returns whatever is currently occupying a slot.
        /// </summary>
        public IInteriorUser GetUser( SlotType type, int index )
        {
            return type == SlotType.Generic ? this.slots[index].objInside : this.workerSlots[index].objInside;
        }

        /// <summary>
        /// Sets something to be currently occupying a slot.
        /// </summary>
        public void SetUser( SlotType type, int index, IInteriorUser value )
        {
            if( type == SlotType.Generic )
            {
                this.slots[index].objInside = value;
                return;
            }

            if( type == SlotType.Worker )
            {
                this.workerSlots[index].objInside = value;
                return;
            }
        }

        /// <summary>
        /// Returns a worker assigned to a given slot.
        /// </summary>
        public CivilianUnitExtension GetWorker( int i )
        {
            return this.workerSlots[i].assignedWorker;
        }

        /// <summary>
        /// Assignes a worker to a given slot.
        /// </summary>
        public void SetWorker( int i, CivilianUnitExtension value )
        {
            this.workerSlots[i].assignedWorker = value;
        }

        /// <summary>
        /// Returns the (world) position of the entrance to this interior.
        /// </summary>
        public Vector3 EntranceWorldPosition()
        {
            return this.entrancePosition == null ? this.transform.position : this.transform.TransformPoint( this.entrancePosition.Value );
        }


        public HUDInterior hudInterior { get; private set; } = null;


        /// <summary>
        /// Returns the list of all workers employed at a particular workplace that are currently assigned to one of the slots.
        /// </summary>
        public List<CivilianUnitExtension> GetAllEmployed( WorkplaceModule workplace = null )
        {
            List<CivilianUnitExtension> employed = new List<CivilianUnitExtension>();

            foreach( var workerSlot in workerSlots )
            {
                if( workerSlot.assignedWorker == null )
                {
                    continue;
                }

                if( workplace != null && workerSlot.assignedWorker.workplace != workplace )
                {
                    continue;
                }

                employed.Add( workerSlot.assignedWorker );
            }

            return employed;
        }


        public void ExitAll()
        {
            List<Slot> allSlots = new List<Slot>();
            allSlots.AddRange( this.slots );
            allSlots.AddRange( this.workerSlots );

            foreach( var slot in allSlots )
            {
                if( slot.objInside != null )
                {
                    slot.objInside.MakeOutside();
                }
            }
        }

        /// <summary>
        /// Returns the index of the first slot that's valid for a given thing to enter. Null if this particular thing can't enter this particular interior.
        /// </summary>
        public int? GetFirstValid( SlotType type, IInteriorUser enteringObj )
        {
            byte population = 1;
            Unit enteringUnit = null;

            if( enteringObj is Unit )
            {
                enteringUnit = (Unit)enteringObj;
                population = (byte)enteringUnit.population;
            }

            if( type == SlotType.Worker )
            {
                if( enteringUnit == null )
                {
                    return null;
                }

                if( !enteringUnit.isCivilian )
                {
                    return null;
                }

                // first slot that can fit and isn't already assigned.
                for( int i = 0; i < this.workerSlots.Length; i++ )
                {
                    if( population > (byte)this.workerSlots[i].maxPopulation )
                    {
                        continue;
                    }

                    if( this.workerSlots[i].assignedWorker != enteringUnit.civilian )
                    {
                        continue;
                    }

                    return i;
                }

                return null;
            }

            if( type == SlotType.Generic )
            {
                for( int i = 0; i < this.slots.Length; i++ )
                {
                    if( population > (byte)this.slots[i].maxPopulation )
                    {
                        continue;
                    }

                    if( !this.slots[i].isEmpty )
                    {
                        continue;
                    }

                    if( enteringUnit == null )
                    {
                        return i;
                    }
                    else
                    {
                        foreach( var slotWhitelistedUnit in this.slots[i].whitelistedUnits )
                        {
                            if( slotWhitelistedUnit == enteringUnit.definitionId )
                            {
                                return i;
                            }
                        }
                    }
                }
                return null;
            }
            return null;
        }



        //
        //
        //



        private Vector3 oldPosition;
        private Quaternion oldRotation;

        protected override void Awake()
        {
            // Cache the starting position & rotation.
            // Do this in 'Awake' to avoid position being already modified, by level load, at the point it gets to 'Start'.
            this.oldPosition = this.transform.position;
            this.oldRotation = this.transform.rotation;

            this.ssObject.isSelectable = true;

            if( this.ssObject is IFactionMember )
            {
                IFactionMember factionMember = (IFactionMember)this.ssObject;

                // Update the faction population caches when the object changes its faction.
                factionMember.onFactionChange.AddListener( ( int before, int after ) =>
                {
                    if( this.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.ssObject).isUsable )
                    {
                        return;
                    }

                    if( before != SSObjectDFC.FACTIONID_INVALID )
                    {
                        LevelDataManager.factionData[before].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );
                    }

                    LevelDataManager.factionData[after].maxPopulationCache += SlotGeneric.GetMaxPopulationTotal( this.slots );

                    if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
                    {
                        ResourcePanel.instance.UpdatePopulationDisplay(
                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
                            LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
                    }
                } );

                if( this.ssObject is ISSObjectUsableUnusable )
                {
                    ISSObjectUsableUnusable usableUnusable = (ISSObjectUsableUnusable)this.ssObject;

                    usableUnusable.onUsableStateChanged.AddListener( () =>
                    {
                        if( factionMember.factionId != SSObjectDFC.FACTIONID_INVALID )
                        {
                            if( usableUnusable.isUsable )
                            {
                                LevelDataManager.factionData[factionMember.factionId].maxPopulationCache += SlotGeneric.GetMaxPopulationTotal( this.slots );
                            }
                            else
                            {
                                this.ExitAll();
                                LevelDataManager.factionData[factionMember.factionId].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );
                            }
                        }
                        if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
                        {
                            ResourcePanel.instance.UpdatePopulationDisplay(
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
                                LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
                        }
                    } );
                }
            }


            base.Awake();
        }

        private void Update()
        {
            // If the position or rotation is changed, update the carried units, and cache the new, changed position & rotation.
            if( this.transform.position != this.oldPosition || this.transform.rotation != this.oldRotation )
            {
                foreach( var slot in this.slots )
                {
                    if( slot.isEmpty )
                    {
                        continue;
                    }

                    IInteriorUser u = slot.objInside;
                    u.transform.position = this.SlotWorldPosition( slot );
                    u.transform.rotation = this.SlotWorldRotation( slot );
                }

                this.oldPosition = this.transform.position;
                this.oldRotation = this.transform.rotation;
            }
        }

        public override void OnObjDestroyed()
        {
            if( this.ssObject is IFactionMember )
            {
                IFactionMember factionMember = (IFactionMember)this.ssObject;

                LevelDataManager.factionData[factionMember.factionId].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );

                if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
                {
                    ResourcePanel.instance.UpdatePopulationDisplay(
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
                        LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
                }
            }

            this.ExitAll();
        }



        //
        //
        //



        public override SSModuleData GetData()
        {
            InteriorModuleData data = new InteriorModuleData()
            {
                slots = new Dictionary<int, Guid>(),
                workerSlots = new Dictionary<int, Guid>()
            };

            for( int i = 0; i < this.slots.Length; i++ )
            {
                if( this.slots[i].objInside == null )
                {
                    continue;
                }

                data.slots.Add( i, ((SSObject)this.slots[i].objInside).guid );
            }

            for( int i = 0; i < this.workerSlots.Length; i++ )
            {
                if( this.workerSlots[i].objInside == null )
                {
                    continue;
                }

                data.workerSlots.Add( i, ((SSObject)this.workerSlots[i].objInside).guid );
            }

            return data;
        }

        public override void SetData( SSModuleData _data )
        {
            InteriorModuleData data = ValidateDataType<InteriorModuleData>( _data );

            foreach( var kvp in data.slots )
            {
                ((IInteriorUser)SSObject.Find( kvp.Value )).MakeInside( this, SlotType.Generic, kvp.Key );
            }
            foreach( var kvp in data.workerSlots )
            {
                ((IInteriorUser)SSObject.Find( kvp.Value )).MakeInside( this, SlotType.Worker, kvp.Key );
            }
        }


        //
        //
        //

        const string ACTIONPANEL_ID_EXITALL = "interior.ap.exitall";
        const string SELECTIONPANEL_ID_SLOTS = "interior.slots";
        const string SELECTIONPANEL_ID_WORKER_SLOTS = "interior.worker_slots";

        private void ReDisplaySlots_UI()
        {
            if( !Selection.IsDisplayedModule( this ) )
            {
                return;
            }

            SelectionPanel.instance.obj.TryClearElement( SELECTIONPANEL_ID_SLOTS );
            SelectionPanel.instance.obj.TryClearElement( SELECTIONPANEL_ID_WORKER_SLOTS );
            DisplaySlotsList_UI();
            DisplayWorkerSlotsList_UI();
        }


        private void TrySelectInside( IInteriorUser insideObj )
        {
            if( insideObj != null )
            {
                if( insideObj is SSObjectDFC )
                {
                    Selection.DeselectAll();
                    Selection.TrySelect( (SSObjectDFC)insideObj );
                }
            }
        }

        private void DisplaySlotsList_UI()
        {
            List<GameObject> gridElements = new List<GameObject>( this.slots.Length );

            foreach( var slot in this.slots )
            {
                GameObject button = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData(), slot.objInside?.icon, () =>
                {
                    TrySelectInside( slot.objInside );
                } );

                gridElements.Add( button );
            }

            GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 135, 0 ), new Vector2( -330.0f, 75 ), new Vector2( 0.5f, 1 ), Vector2.up, Vector2.one ), 72, gridElements.ToArray() );
            SelectionPanel.instance.obj.RegisterElement( SELECTIONPANEL_ID_SLOTS, list.transform );
        }

        private void DisplayWorkerSlotsList_UI()
        {
            List<GameObject> gridElements = new List<GameObject>( this.slots.Length );

            foreach( var slot in this.workerSlots )
            {
                GameObject button = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData(), slot.objInside?.icon, () =>
                {
                    TrySelectInside( slot.objInside );
                } );

                gridElements.Add( button );
            }

            GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 135, -77 ), new Vector2( -330.0f, 75 ), new Vector2( 0.5f, 1 ), Vector2.up, Vector2.one ), 72, gridElements.ToArray() );
            SelectionPanel.instance.obj.RegisterElement( SELECTIONPANEL_ID_WORKER_SLOTS, list.transform );
        }


        public void OnDisplay()
        {
            DisplaySlotsList_UI();
            DisplayWorkerSlotsList_UI();

            ActionPanel.instance.CreateButton( ACTIONPANEL_ID_EXITALL, AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/exit_all" ), "Exit all", "Press to exit and select all objects from this interior.", ActionButtonAlignment.UpperRight, ActionButtonType.Module, () =>
            {
                List<SSObjectDFC> selectables = new List<SSObjectDFC>();

                // exit everyone.
                foreach( var slot in this.slots )
                {
                    if( slot.objInside != null )
                    {
                        if( slot.objInside is SSObjectDFC )
                        {
                            selectables.Add( (SSObjectDFC)slot.objInside );
                        }
                        slot.objInside.MakeOutside();
                    }
                }

                SelectionUtils.Select( selectables.ToArray(), SelectionMode.Replace );
            } );
        }

        public void OnHide()
        {

        }

        public bool CanChangePopulation()
        {
            return false;
        }
    }
}