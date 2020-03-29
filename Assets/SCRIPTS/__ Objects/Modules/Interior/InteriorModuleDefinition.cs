using KFF;
using SS.Content;
using SS.Objects.Buildings;
using SS.Objects.Extras;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class InteriorModuleDefinition : SSModuleDefinition
	{
		public class Slot : IKFFSerializable
		{
			public Vector3 position { get; set; }
			public Quaternion rotation { get; set; }

			public PopulationSize maxPopulation { get; set; }

			public bool countsTowardsMaxPopulation { get; set; }
			public bool isHidden { get; set; }

			public string[] whitelistedUnits { get; set; }

			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.position = serializer.ReadVector3( "Position" );
				this.rotation = serializer.ReadQuaternion( "Rotation" );
				this.maxPopulation = (PopulationSize)serializer.ReadByte( "MaxPopulation" );
				this.countsTowardsMaxPopulation = serializer.ReadBool( "CountsTowardsMaxPopulation" );
				this.isHidden = serializer.ReadBool( "IsHidden" );

				this.whitelistedUnits = serializer.ReadStringArray( "WhitelistedUnits" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteVector3( "", "Position", this.position );
				serializer.WriteQuaternion( "", "Rotation", this.rotation );
				serializer.WriteByte( "", "MaxPopulation", (byte)this.maxPopulation );
				serializer.WriteBool( "", "CountsTowardsMaxPopulation", this.countsTowardsMaxPopulation );
				serializer.WriteBool( "", "IsHidden", this.isHidden );

				serializer.WriteStringArray( "", "WhitelistedUnits", this.whitelistedUnits );
			}
		}

		public class WorkerSlot : IKFFSerializable
		{
			public Vector3 position { get; set; }
			public Quaternion rotation { get; set; }

			public PopulationSize maxPopulation { get; set; }
			
			public bool isHidden { get; set; }
			
			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.position = serializer.ReadVector3( "Position" );
				this.rotation = serializer.ReadQuaternion( "Rotation" );
				this.maxPopulation = (PopulationSize)serializer.ReadByte( "MaxPopulation" );
				this.isHidden = serializer.ReadBool( "IsHidden" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteVector3( "", "Position", this.position );
				serializer.WriteQuaternion( "", "Rotation", this.rotation );
				serializer.WriteByte( "", "MaxPopulation", (byte)this.maxPopulation );
				serializer.WriteBool( "", "IsHidden", this.isHidden );
			}
		}


		public Slot[] slots { get; set; }
		public WorkerSlot[] workerSlots { get; set; }

		public Vector3? entrancePosition { get; set; }
		

		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition ) ||
				objType == typeof( ExtraDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true; // no module constraints
		}

		public override void AddModule( SSObject ssObject, Guid moduleId )
		{
			InteriorModule module = ssObject.AddModule<InteriorModule>( moduleId );
			module.displayName = this.displayName;
			module.icon = this.icon;
			module.entrancePosition = this.entrancePosition;

			InteriorModule.SlotGeneric[] slots = null;
			InteriorModule.SlotWorker[] workerSlots = null;
			if( this.slots != null )
			{
				slots = new InteriorModule.SlotGeneric[this.slots.Length];
				for( int i = 0; i < this.slots.Length; i++ )
				{
					InteriorModule.SlotGeneric slot = new InteriorModule.SlotGeneric();
					slot.localPos = this.slots[i].position;
					slot.localRot = this.slots[i].rotation;
					slot.maxPopulation = this.slots[i].maxPopulation;
					slot.countsTowardsMaxPopulation = this.slots[i].countsTowardsMaxPopulation;
					slot.isHidden = this.slots[i].isHidden;
					slot.whitelistedUnits = this.slots[i].whitelistedUnits;

					slots[i] = slot;
				}
			}
			
			if( this.workerSlots != null )
			{
				workerSlots = new InteriorModule.SlotWorker[this.workerSlots.Length];
				for( int i = 0; i < this.workerSlots.Length; i++ )
				{
					InteriorModule.SlotWorker slot = new InteriorModule.SlotWorker();
					slot.localPos = this.workerSlots[i].position;
					slot.localRot = this.workerSlots[i].rotation;
					slot.maxPopulation = this.workerSlots[i].maxPopulation;
					slot.isHidden = this.workerSlots[i].isHidden;

					workerSlots[i] = slot;
				}
			}
			
			module.SetSlots( slots, workerSlots );
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Slots" );
			if( analysisData.isSuccess )
			{
				this.slots = new Slot[analysisData.childCount];
				for( int i = 0; i < this.slots.Length; i++ )
				{
					this.slots[i] = new Slot();
				}
				serializer.DeserializeArray( "Slots", this.slots );
			}
			
			analysisData = serializer.Analyze( "WorkerSlots" );
			if( analysisData.isSuccess )
			{
				this.workerSlots = new WorkerSlot[analysisData.childCount];
				for( int i = 0; i < this.workerSlots.Length; i++ )
				{
					this.workerSlots[i] = new WorkerSlot();
				}
				serializer.DeserializeArray( "WorkerSlots", this.workerSlots );
			}

			if( serializer.Analyze( "EntrancePosition" ).isSuccess )
			{
				this.entrancePosition = serializer.ReadVector3( "EntrancePosition" );
			}
			else
			{
				this.entrancePosition = null;
			}

			this.displayName = serializer.ReadString( "DisplayName" );

			try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.SerializeArray( "", "Slots", this.slots );
			serializer.SerializeArray( "", "WorkerSlots", this.workerSlots );

			if( this.entrancePosition != null )
			{
				serializer.WriteVector3( "", "EntrancePosition", this.entrancePosition.Value );
			}
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}