using SS.Levels.SaveStates;
using SS.Objects.Units;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModule : SSModule
	{
		public const string KFF_TYPEID = "interior";
		
		public abstract class Slot
		{
			public Vector3 localPos { get; set; }
			public Quaternion localRot { get; set; }

			public Unit unitInside;

			public PopulationSize maxPopulation { get; set; }

			public bool isEmpty
			{
				get
				{
					return this.unitInside == null;
				}
			}

			public bool isHidden { get; set; }
		}

		public class SlotGeneric : Slot
		{
			public string[] whitelistedUnits { get; set; }
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
		
		public SlotGeneric[] slots { get; set; }
		public SlotCivilian[] civilianSlots { get; set; }
		public SlotWorker[] workerSlots { get; set; }


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


		public override ModuleData GetData()
		{
			return new InteriorModuleData();
		}

		public override void SetData( ModuleData data )
		{

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
					Unit u = this.slots[i].unitInside;
					
					u.transform.position = this.SlotWorldPosition( slots[i] );
					u.transform.rotation = this.SlotWorldRotation( slots[i] );
				}
			}

			this.oldPosition = this.transform.position;
			this.oldRotation = this.transform.rotation;
		}
	}
}
