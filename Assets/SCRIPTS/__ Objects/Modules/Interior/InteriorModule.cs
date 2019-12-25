using SS.Objects.Units;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModule : SSModule
	{
		public const string KFF_TYPEID = "interior";

		public class Slot
		{
			public Vector3 localPos { get; set; }
			public Quaternion localRot { get; set; }

			public Unit unitInside;
		}

		/// <summary>
		/// Specifies the local-space position of the entrance.
		/// </summary>
		public Vector3 entrancePosition { get; set; }

		/// <summary>
		/// Specifies the local-space rotation of the entrance.
		/// </summary>
		public Quaternion entranceRotation { get; set; }


		public Slot[] slots { get; set; }


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
			return this.transform.TransformPoint( this.entrancePosition );
		}

		public Quaternion EntranceWorldRotation()
		{
			return this.transform.rotation * this.entranceRotation;
		}


		public override ModuleData GetData()
		{
			throw new System.NotImplementedException();
		}

		public override void SetData( ModuleData data )
		{
			throw new System.NotImplementedException();
		}
	}
}
