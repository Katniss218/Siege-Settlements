using Katniss.ModifierAffectedValues;
using KFF;
using SS.AI.Goals;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class HeroData : SSObjectData
	{
		public Modifier[] maxHealthModifiers { get; set; } = null;
		public Modifier[] movementSpeedModifiers { get; set; } = null;
		public Modifier[] rotationSpeedModifiers { get; set; } = null;

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

		public float health { get; set; }

		public TacticalGoalData tacticalGoalData { get; set; }


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "MaxHealthModifiers" );
			if( analysisData.isSuccess )
			{
				this.maxHealthModifiers = new Modifier[analysisData.childCount];
				for( int i = 0; i < this.maxHealthModifiers.Length; i++ ) { this.maxHealthModifiers[i] = new Modifier(); }

				serializer.DeserializeArray( "MaxHealthModifiers", this.maxHealthModifiers );
			}
			analysisData = serializer.Analyze( "MovementSpeedModifiers" );
			if( analysisData.isSuccess )
			{
				this.movementSpeedModifiers = new Modifier[analysisData.childCount];
				for( int i = 0; i < this.movementSpeedModifiers.Length; i++ ) { this.movementSpeedModifiers[i] = new Modifier(); }

				serializer.DeserializeArray( "MovementSpeedModifiers", this.movementSpeedModifiers );
			}
			analysisData = serializer.Analyze( "RotationSpeedModifiers" );
			if( analysisData.isSuccess )
			{
				this.rotationSpeedModifiers = new Modifier[analysisData.childCount];
				for( int i = 0; i < this.rotationSpeedModifiers.Length; i++ ) { this.rotationSpeedModifiers[i] = new Modifier(); }

				serializer.DeserializeArray( "RotationSpeedModifiers", this.rotationSpeedModifiers );
			}


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
				this.health = serializer.ReadFloat( "Health" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Health' (" + serializer.file.fileName + ")." );
			}

			this.tacticalGoalData = SSObjectData.DeserializeTacticalGoalKFF( serializer );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			if( this.maxHealthModifiers != null )
			{
				serializer.SerializeArray( "", "MaxHealthModifiers", this.maxHealthModifiers );
			}
			if( this.movementSpeedModifiers != null )
			{
				serializer.SerializeArray( "", "MovementSpeedModifiers", this.movementSpeedModifiers );
			}
			if( this.rotationSpeedModifiers != null )
			{
				serializer.SerializeArray( "", "RotationSpeedModifiers", this.rotationSpeedModifiers );
			}


			serializer.WriteGuid( "", "Guid", this.guid );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );
			serializer.WriteFloat( "", "Health", this.health );
			
			SSObjectData.SerializeTacticalGoalKFF( serializer, this.tacticalGoalData );

			this.SerializeModulesKFF( serializer );
		}
	}
}