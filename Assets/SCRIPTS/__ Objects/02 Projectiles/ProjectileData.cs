using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	public class ProjectileData : ObjectData
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }

		public bool isStuck { get; set; }
		public Quaternion stuckRotation { get; set; }
		public Vector3 velocity { get; set; }

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

		public DamageType damageTypeOverride { get; set; }
		public float damageOverride { get; set; }
		public float armorPenetrationOverride { get; set; }


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );
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
				this.isStuck = serializer.ReadBool( "IsStuck" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'IsStuck' (" + serializer.file.fileName + ")." );
			}

			if( this.isStuck )
			{
				try
				{
					this.stuckRotation = serializer.ReadQuaternion( "StuckRotation" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'StuckRotation' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				try
				{
					this.velocity = serializer.ReadVector3( "Velocity" );
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Velocity' (" + serializer.file.fileName + ")." );
				}
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
				this.damageTypeOverride = (DamageType)serializer.ReadByte( "DamageTypeOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DamageTypeOverride' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.damageOverride = serializer.ReadFloat( "DamageOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DamageOverride' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.armorPenetrationOverride = serializer.ReadFloat( "ArmorPenetrationOverride" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ArmorPenetrationOverride' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Guid", this.guid.ToString( "D" ) );

			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteBool( "", "IsStuck", this.isStuck );
			if( this.isStuck )
			{
				serializer.WriteQuaternion( "", "StuckRotation", this.stuckRotation );
			}
			else
			{
				serializer.WriteVector3( "", "Velocity", this.velocity );
			}

			serializer.WriteInt( "", "FactionId", this.factionId );

			serializer.WriteByte( "", "DamageTypeOverride", (byte)this.damageTypeOverride );
			serializer.WriteFloat( "", "DamageOverride", this.damageOverride );
			serializer.WriteFloat( "", "ArmorPenetrationOverride", this.armorPenetrationOverride );
		}
	}
}