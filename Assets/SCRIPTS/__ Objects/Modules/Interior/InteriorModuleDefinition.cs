using KFF;
using SS.Objects.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModuleDefinition : ModuleDefinition
	{
		public class Slot : IKFFSerializable
		{
			public Vector3 position { get; set; }
			public Quaternion rotation { get; set; }

			public PopulationSize maxPopulation { get; set; }


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.position = serializer.ReadVector3( "Position" );
				this.rotation = serializer.ReadQuaternion( "Rotation" );

				this.maxPopulation = (PopulationSize)serializer.ReadByte( "MaxPopulation" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteVector3( "", "Position", this.position );
				serializer.WriteQuaternion( "", "Rotation", this.rotation );
				serializer.WriteByte( "", "MaxPopulation", (byte)this.maxPopulation );
			}
		}


		public Slot[] slots { get; set; }

		public Vector3? entrancePosition { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true; // no module constraints
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			InteriorModule interior = gameObject.AddComponent<InteriorModule>();

			interior.slots = new InteriorModule.SlotGeneric[this.slots.Length];
			for( int i = 0; i < this.slots.Length; i++ )
			{
				InteriorModule.SlotGeneric slot = new InteriorModule.SlotGeneric();
				slot.localPos = this.slots[i].position;
				slot.localRot = this.slots[i].rotation;
				slot.maxPopulation = this.slots[i].maxPopulation;

				interior.slots[i] = slot;
			}

			interior.OnAfterSlotsChanged();
			interior.entrancePosition = this.entrancePosition;
			interior.moduleId = moduleId;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Slots" );
			this.slots = new Slot[analysisData.childCount];
			for( int i = 0; i < this.slots.Length; i++ )
			{
				this.slots[i] = new Slot();
			}
			serializer.DeserializeArray( "Slots", this.slots );

			if( serializer.Analyze( "EntrancePosition" ).isSuccess )
			{
				this.entrancePosition = serializer.ReadVector3( "EntrancePosition" );
			}
			else
			{
				this.entrancePosition = null;
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.slots );
			if( this.entrancePosition != null )
			{
				serializer.WriteVector3( "", "EntrancePosition", this.entrancePosition.Value );
			}
		}
	}
}