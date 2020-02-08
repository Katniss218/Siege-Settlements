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
	[UseHud(typeof(HUDInterior), "hudInterior")]
	public class InteriorModule : SSModule, ISelectDisplayHandler
	{
		public const string KFF_TYPEID = "interior";

		public enum SlotType : byte
		{
			Generic, // any unit can enter (whitelist-based)
			Worker // only the unit employed at this particular slot can enter.
		}

		public abstract class Slot
		{
			public Vector3 localPos { get; set; }
			public Quaternion localRot { get; set; }

			public IInteriorUser objInside;
			
			public PopulationSize maxPopulation { get; set; } = PopulationSize.x1;

			public bool isEmpty
			{
				get
				{
					return this.objInside == null;
				}
			}

			public bool isHidden { get; set; }

		}

		public class SlotGeneric : Slot
		{
#warning MaxPopulation is defined by a separate variable of the slot.
			//public bool countsTowardsMaxPopulation { get; set; }
			public string[] whitelistedUnits { get; set; } = new string[0];


			public static int GetMaxPopulationTotal( SlotGeneric[] slots )
			{
				int accumulator = 0;
				for( int i = 0; i < slots.Length; i++ )
				{
					accumulator += 1;
				}
				return accumulator;
			}
		}

		public class SlotWorker : Slot
		{
			public CivilianUnitExtension worker { get; set; }
		}



		/// <summary>
		/// Specifies the local-space position of the entrance.
		/// </summary>
		public Vector3? entrancePosition { get; set; }

		private SlotGeneric[] slots = new SlotGeneric[0];
		private SlotWorker[] workerSlots = new SlotWorker[0];
		
		public int SlotCount( SlotType type )
		{
			return type == SlotType.Generic ? this.slots.Length : this.workerSlots.Length;
		}

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
			
			if( this.ssObject is IFactionMember )
			{
				if( this.ssObject is ISSObjectUsableUnusable && !((ISSObjectUsableUnusable)this.ssObject).isUsable )
				{
					goto skip_cache;
				}
				IFactionMember factionMember = (IFactionMember)this.ssObject;
				if( factionMember.factionId != SSObjectDFC.FACTIONID_INVALID )
				{
					LevelDataManager.factionData[factionMember.factionId].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );
					LevelDataManager.factionData[factionMember.factionId].maxPopulationCache += SlotGeneric.GetMaxPopulationTotal( slots );
					ResourcePanel.instance.UpdatePopulationDisplay(
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
				}
			}
	skip_cache:

			this.slots = slots;
			this.workerSlots = workerSlots;

			this.hudInterior.SetSlotCount( this.slots.Length, this.workerSlots.Length );


			ReDisplaySlots_UI();
		}


		public Vector3 SlotWorldPosition( Slot slot )
		{
			return this.transform.TransformPoint( slot.localPos );
		}

		public Quaternion SlotWorldRotation( Slot slot )
		{
			return this.transform.rotation * slot.localRot;
		}

		public IInteriorUser GetUser( SlotType type, int i )
		{
			return type == SlotType.Generic ? this.slots[i].objInside : this.workerSlots[i].objInside;
		}
		public void SetUser( SlotType type, int i, IInteriorUser value )
		{
			if( type == SlotType.Generic ) { this.slots[i].objInside = value; return; }
			if( type == SlotType.Worker ) { this.workerSlots[i].objInside = value; return; }
		}

		public CivilianUnitExtension GetWorker( int i )
		{
			return this.workerSlots[i].worker;
		}
		public void SetWorker( int i, CivilianUnitExtension value )
		{
			this.workerSlots[i].worker = value;
		}


		public Vector3 EntranceWorldPosition()
		{
			return this.entrancePosition == null ?
				this.transform.position :
				this.transform.TransformPoint( this.entrancePosition.Value );
		}


		public HUDInterior hudInterior { get; private set; } = null;



		public List<CivilianUnitExtension> GetAllEmployed( WorkplaceModule workplace = null )
		{
			List<CivilianUnitExtension> ret = new List<CivilianUnitExtension>();
			for( int i = 0; i < this.workerSlots.Length; i++ )
			{
				if( this.workerSlots[i].worker == null )
				{
					continue;
				}

				if( workplace != null && this.workerSlots[i].worker.workplace != workplace )
				{
					continue;
				}

				ret.Add( this.workerSlots[i].worker );
			}
			return ret;
		}



		public void ExitAll()
		{
			for( int i = 0; i < this.slots.Length; i++ )
			{
				if( this.slots[i].objInside != null )
				{
					this.slots[i].objInside.SetOutside();
				}
			}
			for( int i = 0; i < this.workerSlots.Length; i++ )
			{
				if( this.workerSlots[i].objInside != null )
				{
					this.workerSlots[i].objInside.SetOutside();
				}
			}
		}
		
		public int? GetFirstValid( SlotType type, IInteriorUser interiorUser )
		{
			byte population = 1;
			Unit unit = null;
			if( interiorUser is Unit )
			{
				unit = (Unit)interiorUser;
				population = (byte)unit.population;
			}

			if( type == SlotType.Worker )
			{
				if( unit == null )
				{
					return null;
				}

				if( !unit.isCivilian )
				{
					return null;
				}

				for( int i = 0; i < this.workerSlots.Length; i++ )
				{
					if( population > (byte)this.workerSlots[i].maxPopulation )
					{
						continue;
					}

					if( this.workerSlots[i].worker != unit.civilian )
					{
						continue;
					}

					return i;
				}
			}
			else if( type == SlotType.Generic )
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

					if( unit == null )
					{
						return i;
					}
					else
					{
						for( int j = 0; j < this.slots[i].whitelistedUnits.Length; j++ )
						{
							if( this.slots[i].whitelistedUnits[j] == unit.definitionId )
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
#warning Add ssObject.isSelectable = true, for modules which require selectability.
			// Cache the starting position & rotation.
			// Do this in 'Awake' to avoid position being already modified, by level load, at the point it gets to 'Start'.
			this.oldPosition = this.transform.position;
			this.oldRotation = this.transform.rotation;

			if( this.ssObject is IFactionMember )
			{
				IFactionMember factionMember = (IFactionMember)this.ssObject;
				factionMember.onFactionChange.AddListener( (int before, int after) =>
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
					ResourcePanel.instance.UpdatePopulationDisplay(
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
						LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
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
							if( factionMember.factionId == LevelDataManager.PLAYER_FAC )
							{
								ResourcePanel.instance.UpdatePopulationDisplay(
									LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
									LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
							}
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
				for( int i = 0; i < this.slots.Length; i++ )
				{
					if( this.slots[i].isEmpty )
					{
						continue;
					}
					IInteriorUser u = this.slots[i].objInside;

					u.transform.position = this.SlotWorldPosition( slots[i] );
					u.transform.rotation = this.SlotWorldRotation( slots[i] );
				}
				this.oldPosition = this.transform.position;
				this.oldRotation = this.transform.rotation;
			}
		}

		public override void OnObjDestroyed()
		{
			LevelDataManager.factionData[((IFactionMember)this.ssObject).factionId].maxPopulationCache -= SlotGeneric.GetMaxPopulationTotal( this.slots );
			ResourcePanel.instance.UpdatePopulationDisplay(
				LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].populationCache,
				LevelDataManager.factionData[LevelDataManager.PLAYER_FAC].maxPopulationCache );
			this.ExitAll();
		}



		//
		//
		//

		
		
		public override SSModuleData GetData()
		{
			InteriorModuleData data = new InteriorModuleData();
			data.slots = new Dictionary<int, Guid>();
			for( int i = 0; i < this.slots.Length; i++ )
			{
				if( this.slots[i].objInside == null )
				{
					continue;
				}
				data.slots.Add( i, ((SSObject)this.slots[i].objInside).guid );
			}

			data.workerSlots = new Dictionary<int, Guid>();
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

			//return;
			foreach( var kvp in data.slots )
			{
				((IInteriorUser)SSObject.Find( kvp.Value )).SetInside( this, SlotType.Generic, kvp.Key );
			}
			foreach( var kvp in data.workerSlots )
			{
				((IInteriorUser)SSObject.Find( kvp.Value )).SetInside( this, SlotType.Worker, kvp.Key );
			}
		}


		//
		//
		//


		public void ReDisplaySlots_UI()
		{
			if( !Selection.IsDisplayedModule( this ) )
			{
				return;
			}

			SelectionPanel.instance.obj.TryClearElement( "interior.slots" );
			SelectionPanel.instance.obj.TryClearElement( "interior.worker_slots" );
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
			GameObject[] gridElements = new GameObject[this.slots.Length];
			for( int i = 0; i < this.slots.Length; i++ )
			{
				IInteriorUser insideObj = this.slots[i].objInside;
				gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData(), insideObj?.icon, () =>
				{
					TrySelectInside( insideObj );
				} );
			}
			GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 135, 0 ), new Vector2( -330.0f, 75 ), new Vector2( 0.5f, 1 ), Vector2.up, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "interior.slots", list.transform );
		}

		private void DisplayWorkerSlotsList_UI()
		{
			GameObject[] gridElements = new GameObject[this.workerSlots.Length];
			for( int i = 0; i < this.workerSlots.Length; i++ )
			{
				IInteriorUser insideObj = this.workerSlots[i].objInside;
				gridElements[i] = UIUtils.InstantiateIconButton( SelectionPanel.instance.obj.transform, new GenericUIData(), insideObj?.icon, () =>
				{
					TrySelectInside( insideObj );
				} );
			}
			GameObject list = UIUtils.InstantiateScrollableGrid( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 135, -77 ), new Vector2( -330.0f, 75 ), new Vector2( 0.5f, 1 ), Vector2.up, Vector2.one ), 72, gridElements );
			SelectionPanel.instance.obj.RegisterElement( "interior.worker_slots", list.transform );
		}


		public void OnDisplay()
		{
			DisplaySlotsList_UI();
			DisplayWorkerSlotsList_UI();
		}

		public void OnHide()
		{

		}

		

		//
		//
		//

		
		
		public static void GetSlot( InteriorModule interior, SlotType slotType, int slotIndex, out Slot slot, out HUDInteriorSlot slotHud )
		{
			slot = null;
			slotHud = null;

			if( slotType == SlotType.Generic )
			{
				slot = interior.slots[slotIndex];
				slotHud = interior.hudInterior.slots[slotIndex];
				return;
			}
			if( slotType == SlotType.Worker )
			{
				slot = interior.workerSlots[slotIndex];
				slotHud = interior.hudInterior.workerSlots[slotIndex];
			}
		}
	}
}