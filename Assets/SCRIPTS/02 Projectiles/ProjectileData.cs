﻿using KFF;
using UnityEngine;

namespace SS.Levels.SaveStates
{
	/// <summary>
	/// Contains every information to successfully round-trip a projectile, to and from file.
	/// </summary>
	public class ProjectileData : IKFFSerializable
	{
		public Vector3 position { get; set; }
		public Vector3 velocity { get; set; }

		public int factionId { get; set; }

		public DamageType damageTypeOverride { get; set; }
		public float damageOverride { get; set; }
		public float armorPenetrationOverride { get; set; }
		
		
		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.position = serializer.ReadVector3( "Position" );
			this.velocity = serializer.ReadVector3( "Velocity" );

			this.factionId = serializer.ReadInt( "FactionId" );

			this.damageTypeOverride = (DamageType)serializer.ReadByte( "DamageTypeOverride" );
			this.damageOverride = serializer.ReadFloat( "DamageOverride" );
			this.armorPenetrationOverride = serializer.ReadFloat( "ArmorPenetrationOverride" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteVector3( "", "Position", this.position );
			serializer.WriteVector3( "", "Velocity", this.velocity );

			serializer.WriteInt( "", "FactionId", this.factionId );

			serializer.WriteByte( "", "DamageTypeOverride", (byte)this.damageTypeOverride );
			serializer.WriteFloat( "", "DamageOverride", this.damageOverride );
			serializer.WriteFloat( "", "ArmorPenetrationOverride", this.armorPenetrationOverride );
		}
	}
}