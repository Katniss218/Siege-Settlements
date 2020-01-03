using KFF;
using SS.AI.Goals;
using SS.Content;
using SS.Objects;
using SS.Objects.Modules;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class UnitData : SSObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }
		public Quaternion rotation { get; set; }

		private int __factionId = 0;
		public int factionId
		{
			get
			{
				return this.__factionId;
			}
			set
			{
				if( value < 0 )
				{
					throw new Exception( "Can't set faction to outside of acceptable values." );
				}
				this.__factionId = value;
			}
		}

		public PopulationSize population { get; set; }

		public float? health { get; set; }
		public float? movementSpeed { get; set; }
		public float? rotationSpeed { get; set; }
		
		public TacticalGoalData tacticalGoalData { get; set; }

		public Tuple<Guid,Guid> inside { get; set; }
		public InteriorModule.SlotType insideSlotType { get; set; }
		public int insideSlotIndex { get; set; }
		
		public Tuple<Guid, Guid> workplace { get; set; }



		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.guid = serializer.ReadGuid( "Guid" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Guid' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.position = serializer.ReadVector3( "Position" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Position' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.rotation = serializer.ReadQuaternion( "Rotation" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Rotation' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.factionId = serializer.ReadInt( "FactionId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'FactionId' (" + serializer.file.fileName + ")." );
			}
			
			try
			{
				this.population = (PopulationSize)serializer.ReadByte( "Population" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Population' (" + serializer.file.fileName + ")." );
			}

			if( serializer.Analyze( "Health" ).isSuccess )
			{
				try
				{
					this.health = serializer.ReadFloat( "Health" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Health' (" + serializer.file.fileName + ")." );
				}
			}

			if( serializer.Analyze( "MovementSpeed" ).isSuccess )
			{
				try
				{
					this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'MovementSpeed' (" + serializer.file.fileName + ")." );
				}
			}
			if( serializer.Analyze( "RotationSpeed" ).isSuccess )
			{
				try
				{
					this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'RotationSpeed' (" + serializer.file.fileName + ")." );
				}
			}

			if( serializer.Analyze( "Inside" ).isSuccess )
			{
				Guid insideObj = serializer.ReadGuid( "Inside.ObjectGuid" );
				Guid insideMod = serializer.ReadGuid( "Inside.ModuleId" );
				this.inside = new Tuple<Guid, Guid>( insideObj, insideMod );

				this.insideSlotType = (InteriorModule.SlotType)serializer.ReadByte( "Inside.SlotType" );
				this.insideSlotIndex = serializer.ReadInt( "Inside.SlotIndex" );
			}

			if( serializer.Analyze( "Workplace" ).isSuccess )
			{
#warning kff reading object-module tuples abstracted away & just call a method in the extensions.
				Guid insideObj = serializer.ReadGuid( "Workplace.ObjectGuid" );
				Guid insideMod = serializer.ReadGuid( "Workplace.ModuleId" );
				this.workplace = new Tuple<Guid, Guid>( insideObj, insideMod );
			}
			
			this.tacticalGoalData = SSObjectData.DeserializeTacticalGoalKFF( serializer );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "Guid", this.guid );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );

			serializer.WriteByte( "", "Population", (byte)this.population );

			if( this.health != null )
			{
				serializer.WriteFloat( "", "Health", this.health.Value );
			}

			if( this.movementSpeed != null )
			{
				serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed.Value );
			}
			if( this.rotationSpeed != null )
			{
				serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed.Value );
			}

			if( this.inside != null )
			{
				serializer.WriteClass( "", "Inside" );
				serializer.WriteGuid( "Inside", "ObjectGuid", this.inside.Item1 );
				serializer.WriteGuid( "Inside", "ModuleId", this.inside.Item2 );
				serializer.WriteByte( "Inside", "SlotType", (byte)this.insideSlotType );
				serializer.WriteInt( "Inside", "SlotIndex", this.insideSlotIndex );
			}

			if( this.workplace != null )
			{
				serializer.WriteClass( "", "Workplace" );
				serializer.WriteGuid( "Workplace", "ObjectGuid", this.workplace.Item1 );
				serializer.WriteGuid( "Workplace", "ModuleId", this.workplace.Item2 );
			}

			SSObjectData.SerializeTacticalGoalKFF( serializer, this.tacticalGoalData );

			this.SerializeModulesKFF( serializer );
		}
	}
}