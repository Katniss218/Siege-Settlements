using KFF;
using System;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a projectile, to and from file.
	/// </summary>
	public class ProjectileData : IKFFSerializable
	{
		public Guid guid { get; set; }

		public Vector3 position { get; set; }

		public bool isStuck { get; set; }
		public Quaternion stuckRotation { get; set; }
		public Vector3 velocity { get; set; }

		public int factionId { get; set; }

		public DamageType damageTypeOverride { get; set; }
		public float damageOverride { get; set; }
		public float armorPenetrationOverride { get; set; }
		

		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.guid = Guid.ParseExact( serializer.ReadString( "Guid" ), "D" );

			this.position = serializer.ReadVector3( "Position" );
			this.isStuck = serializer.ReadBool( "IsStuck" );
			if( this.isStuck )
			{
				this.stuckRotation = serializer.ReadQuaternion( "StuckRotation" );
			}
			else
			{
				this.velocity = serializer.ReadVector3( "Velocity" );
			}

			this.factionId = serializer.ReadInt( "FactionId" );

			this.damageTypeOverride = (DamageType)serializer.ReadByte( "DamageTypeOverride" );
			this.damageOverride = serializer.ReadFloat( "DamageOverride" );
			this.armorPenetrationOverride = serializer.ReadFloat( "ArmorPenetrationOverride" );
		}

		public void SerializeKFF( KFFSerializer serializer )
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