using Katniss.Utils;
using KFF;
using System;
using UnityEngine;

namespace SS
{
	public class RangedModuleDefinition : IKFFSerializable
	{
		public string projectileId { get; set; }
		public int projectileCount { get; set; }
		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }
		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
		public float velocity { get; set; }
		public Vector3 localOffsetMin { get; set; }
		public Vector3 localOffsetMax { get; set; }
		public Tuple<string, AudioClip> attackSoundEffect { get; private set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.projectileId = serializer.ReadString( "ProjectileId" );
			this.projectileCount = serializer.ReadInt( "ProjectileCount" );
			this.damageType = (DamageType)serializer.ReadByte( "DamageType" );
			this.damage = serializer.ReadFloat( "Damage" );
			this.armorPenetration = serializer.ReadFloat( "ArmorPenetration" );
			this.attackRange = serializer.ReadFloat( "AttackRange" );
			this.attackCooldown = serializer.ReadFloat( "AttackCooldown" );
			this.velocity = serializer.ReadFloat( "Velocity" );
			this.localOffsetMin = serializer.ReadVector3( "LocalOffsetMin" );
			this.localOffsetMax = serializer.ReadVector3( "LocalOffsetMax" );

			this.attackSoundEffect = serializer.ReadAudioClipFromAssets( "AttackSound" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "ProjectileId", this.projectileId );
			serializer.WriteInt( "", "ProjectileCount", this.projectileCount );
			serializer.WriteByte( "", "DamageType", (byte)this.damageType );
			serializer.WriteFloat( "", "Damage", this.damage );
			serializer.WriteFloat( "", "ArmorPenetration", this.armorPenetration );
			serializer.WriteFloat( "", "AttackRange", this.attackRange );
			serializer.WriteFloat( "", "AttackCooldown", this.attackCooldown );
			serializer.WriteFloat( "", "Velocity", this.velocity );
			serializer.WriteVector3( "", "LocalOffsetMin", this.localOffsetMin );
			serializer.WriteVector3( "", "LocalOffsetMax", this.localOffsetMax );

			serializer.WriteString( "", "AttackSound", this.attackSoundEffect.Item1 );
		}
	}
}