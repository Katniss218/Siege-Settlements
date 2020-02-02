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
			public string[] whitelistedUnits { get; set; } = new string[0];
		}

		public class SlotWorker : Slot
		{
			public CivilianUnitExtension worker { get; set; }
		}

		/// <summary>
		/// Specifies the local-space position of the entrance.
		/// </summary>
		public Vector3? entrancePosition { get; set; }

		public SlotGeneric[] slots { get; set; } = new SlotGeneric[0];
		public SlotWorker[] workerSlots { get; set; } = new SlotWorker[0];
		
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



		public Vector3 SlotWorldPosition( Slot slot )
		{
			return this.transform.TransformPoint( slot.localPos );
		}

		public Quaternion SlotWorldRotation( Slot slot )
		{
			return this.transform.rotation * slot.localRot;
		}


		public Vector3 EntranceWorldPosition()
		{
			return this.entrancePosition == null ?
				this.transform.position :
				this.transform.TransformPoint( this.entrancePosition.Value );
		}


		public HUDInterior hudInterior { get; private set; } = null;

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



		public void UpdateSlotDisplay()
		{
			this.hudInterior.SetSlotCount( this.slots.Length, this.workerSlots.Length );
			
			if( Selection.IsDisplayedModule( this ) )
			{
				SelectionPanel.instance.obj.TryClearElement( "interior.slots" );
				SelectionPanel.instance.obj.TryClearElement( "interior.worker_slots" );
				
				DisplaySlotsList_UI();
				DisplayWorkerSlotsList_UI();
			}
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

		public override void OnObjSpawn()
		{
#warning todo cleanup.
			//this.UpdateSlotDisplay();
			base.OnObjSpawn();
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
				if( unit != null )
				{
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

					if( unit != null )
					{
						for( int j = 0; j < this.slots[i].whitelistedUnits.Length; j++ )
						{
							if( this.slots[i].whitelistedUnits[j] == unit.definitionId )
							{
								return i;
							}
						}
					}
					else
					{
						return i;
					}
				}
				return null;
			}
			
			return null;
		}
		
		private Vector3 oldPosition;
		private Quaternion oldRotation;

		protected override void Awake()
		{
#warning objects containing this module are selectable. So selectability of objects depends on the modules.
			// Cache the starting position & rotation.
			// Do this in 'Awake' to avoid position being already modified, by level load, at the point it gets to 'Start'.
			this.oldPosition = this.transform.position;
			this.oldRotation = this.transform.rotation;

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


		private void TrySelectInside_UI( IInteriorUser insideObj )
		{
			if( insideObj != null )
			{
				if( insideObj is SSObjectDFSC )
				{
					Selection.DeselectAll();
					Selection.TrySelect( (SSObjectDFSC)insideObj );
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
					TrySelectInside_UI( insideObj );
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
					TrySelectInside_UI( insideObj );
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
	}
}