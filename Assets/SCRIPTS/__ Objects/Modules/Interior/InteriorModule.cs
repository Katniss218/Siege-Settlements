using SS.Levels.SaveStates;
using SS.Objects.Units;
using SS.UI;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModule : SSModule
	{
		public const string KFF_TYPEID = "interior";
		
		public enum SlotType : byte
		{
			Generic,
			Civilian,
			Worker
		}

		public abstract class Slot
		{
			public Vector3 localPos { get; set; }
			public Quaternion localRot { get; set; }

			public IEnterableInside objInside;

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

		public class SlotCivilian : Slot
		{

		}

		public class SlotWorker : Slot
		{

		}

		/// <summary>
		/// Specifies the local-space position of the entrance.
		/// </summary>
		public Vector3? entrancePosition { get; set; }

		public SlotGeneric[] slots { get; set; } = new SlotGeneric[0];
		public SlotCivilian[] civilianSlots { get; set; } = new SlotCivilian[0];
		public SlotWorker[] workerSlots { get; set; } = new SlotWorker[0];
		
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
			return this.entrancePosition == null ? this.transform.position : this.transform.TransformPoint( this.entrancePosition.Value );
		}

		public HUDInterior hudInterior { get; private set; } = null;
		
		public void OnAfterSlotsChanged()
		{
			this.hudInterior.SetSlotCount( this.slots.Length, this.civilianSlots.Length, this.workerSlots.Length );
		}

		public void GetOutAll()
		{
			for( int i = 0; i < this.slots.Length; i++ )
			{
				if( this.slots[i].objInside != null )
				{
					this.slots[i].objInside.SetOutside();
				}
			}
			for( int i = 0; i < this.civilianSlots.Length; i++ )
			{
				if( this.civilianSlots[i].objInside != null )
				{
					this.civilianSlots[i].objInside.SetOutside();
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

		public int? GetFirstValid( SlotType type, PopulationSize population, string unitId, bool isUnitCivilian, bool isWorkingHere )
		{
			if( type == SlotType.Generic )
			{
				for( int i = 0; i < this.slots.Length; i++ )
				{
					if( (byte)population > (byte)this.slots[i].maxPopulation )
					{
						continue;
					}

					if( !this.slots[i].isEmpty )
					{
						continue;
					}

					for( int j = 0; j < this.slots[i].whitelistedUnits.Length; j++ )
					{
						if( this.slots[i].whitelistedUnits[j] == unitId )
						{
							return i;
						}
					}

					return null;
				}
				return null;
			}
			if( type == SlotType.Civilian )
			{
				if( !isUnitCivilian )
				{
					return null;
				}
				for( int i = 0; i < this.civilianSlots.Length; i++ )
				{
					if( (byte)population > (byte)this.civilianSlots[i].maxPopulation )
					{
						continue;
					}

					if( !this.civilianSlots[i].isEmpty )
					{
						continue;
					}

					return i + this.slots.Length;
				}
				return null;
			}
			if( type == SlotType.Worker )
			{
				if( !isWorkingHere )
				{
					return null;
				}
				for( int i = 0; i < this.workerSlots.Length; i++ )
				{
					if( (byte)population > (byte)this.workerSlots[i].maxPopulation )
					{
						continue;
					}

					if( !this.workerSlots[i].isEmpty )
					{
						continue;
					}

					return i + this.slots.Length + this.civilianSlots.Length;
				}
				return null;
			}
			return null;
		}

		public Slot GetSlotAny( int slotIndex )
		{
			if( slotIndex < this.slots.Length )
			{
				return this.slots[slotIndex];
			}
			if( slotIndex < this.civilianSlots.Length )
			{
				return this.civilianSlots[slotIndex - this.slots.Length];
			}
			if( slotIndex < this.workerSlots.Length )
			{
				return this.workerSlots[slotIndex - this.slots.Length - this.civilianSlots.Length];
			}
			return null;
		}

		private void RegisterHUD()
		{
			// integrate hud.
			IHUDHolder hudObj = (IHUDHolder)this.ssObject;


			this.hudInterior = hudObj.hud.GetComponent<HUDInterior>();
		}

		void Awake()
		{
			if( this.ssObject is IHUDHolder )
			{
				this.RegisterHUD();
			}
		}

		private Vector3 oldPosition;
		private Quaternion oldRotation;

		private void Update()
		{
			if( this.transform.position != this.oldPosition || this.transform.rotation != this.oldRotation )
			{
				for( int i = 0; i < this.slots.Length; i++ )
				{
					if( this.slots[i].isEmpty )
					{
						continue;
					}
					IEnterableInside u = this.slots[i].objInside;
					
					u.transform.position = this.SlotWorldPosition( slots[i] );
					u.transform.rotation = this.SlotWorldRotation( slots[i] );
				}
			}

			this.oldPosition = this.transform.position;
			this.oldRotation = this.transform.rotation;
		}
		
		public override void OnObjDestroyed()
		{
			this.GetOutAll();
		}


		public override ModuleData GetData()
		{
			return new InteriorModuleData();
			// units inside are saved here or where?
		}

		public override void SetData( ModuleData data )
		{
			// units inside are saved here or where?
		}
	}
}
