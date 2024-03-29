﻿using KFF;
using SS.AI.Goals;
using SS.Content;
using SS.Objects.Units;
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
		
		public TacticalGoalData[] tacticalGoalData { get; set; }
		public int tacticalGoalTag { get; set; }
				
		public Tuple<Guid, Guid, int> workplace { get; set; }
		public bool? isOnAutomaticDuty { get; set; }
		public bool? isWorking { get; set; }



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
			
			if( serializer.Analyze( "Workplace" ).isSuccess )
			{
				Guid insideObj = serializer.ReadGuid( "Workplace.ObjectGuid" );
				Guid insideMod = serializer.ReadGuid( "Workplace.ModuleId" );
				int slotIndex = serializer.ReadInt( "Workplace.SlotIndex" );
				this.workplace = new Tuple<Guid, Guid, int>( insideObj, insideMod, slotIndex );
			}

			if( serializer.Analyze( "IsWorking" ).isSuccess )
			{
				this.isWorking = serializer.ReadBool( "IsWorking" );
			}

			if( serializer.Analyze( "IsOnAutomaticDuty" ).isSuccess )
			{
				this.isOnAutomaticDuty = serializer.ReadBool( "IsOnAutomaticDuty" );
			}
			
			this.tacticalGoalData = SSObjectData.DeserializeTacticalGoalKFF( serializer, out int tag );
			this.tacticalGoalTag = tag;

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
						
			if( this.workplace != null )
			{
				serializer.WriteClass( "", "Workplace" );
				serializer.WriteGuid( "Workplace", "ObjectGuid", this.workplace.Item1 );
				serializer.WriteGuid( "Workplace", "ModuleId", this.workplace.Item2 );
				serializer.WriteInt( "Workplace", "SlotIndex", this.workplace.Item3 );
			}

			if( this.isWorking != null )
			{
				serializer.WriteBool( "", "IsWorking", this.isWorking.Value );
			}

			if( this.isOnAutomaticDuty != null )
			{
				serializer.WriteBool( "", "IsOnAutomaticDuty", this.isOnAutomaticDuty.Value );
			}

			SSObjectData.SerializeTacticalGoalKFF( serializer, this.tacticalGoalData, this.tacticalGoalTag );

			this.SerializeModulesKFF( serializer );
		}
	}
}