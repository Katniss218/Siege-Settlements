using Katniss.Utils;
using KFF;
using System;
using UnityEngine;

namespace SS
{
	public class MeleeModuleDefinition : IKFFSerializable
	{
		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
		public Tuple<string, AudioClip> attackSoundEffect { get; private set; }


		public void DeserializeKFF( KFFSerializer serializer )
		{
			this.damageType = (DamageType)serializer.ReadByte( "DamageType" );
			this.damage = serializer.ReadFloat( "Damage" );
			this.armorPenetration = serializer.ReadFloat( "ArmorPenetration" );

			this.attackRange = serializer.ReadFloat( "AttackRange" );
			this.attackCooldown = serializer.ReadFloat( "AttackCooldown" );
			this.attackSoundEffect = serializer.ReadAudioClipFromAssets( "AttackSound" );
		}

		public void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "DamageType", (byte)this.damageType );
			serializer.WriteFloat( "", "Damage", this.damage );
			serializer.WriteFloat( "", "ArmorPenetration", this.armorPenetration );

			serializer.WriteFloat( "", "AttackRange", this.attackRange );
			serializer.WriteFloat( "", "AttackCooldown", this.attackCooldown );
			serializer.WriteString( "", "AttackSound", this.attackSoundEffect.Item1 );
		}
	}
}