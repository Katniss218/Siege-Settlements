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

		public float? health { get; set; }
		public float? movementSpeed { get; set; }
		public float? rotationSpeed { get; set; }

		public TacticalGoalData tacticalGoalData { get; set; }


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

			this.tacticalGoalData = SSObjectData.DeserializeTacticalGoalKFF( serializer );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "Guid", this.guid );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteQuaternion( "", "Rotation", this.rotation );

			serializer.WriteInt( "", "FactionId", this.factionId );

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

			SSObjectData.SerializeTacticalGoalKFF( serializer, this.tacticalGoalData );

			this.SerializeModulesKFF( serializer );
		}
	}
}