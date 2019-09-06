using Katniss.Utils;
using KFF;
using System;
using UnityEngine;

namespace SS.Modules
{
	public class MeleeModuleDefinition : ModuleDefinition
	{
		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
		public Tuple<string, AudioClip> attackSoundEffect { get; private set; }

		public override void AddTo( GameObject obj )
		{
			ITargetFinder finder = obj.GetComponent<ITargetFinder>();
			if( finder == null )
			{
				throw new Exception( "MeleeModule requires an ITargetFinder component." );
			}

			DamageSource meleeDamageSource = obj.AddComponent<DamageSource>();
			meleeDamageSource.damageType = damageType;
			meleeDamageSource.damage = damage;
			meleeDamageSource.armorPenetration = armorPenetration;

			MeleeModule melee = obj.AddComponent<MeleeModule>();
			melee.damageSource = meleeDamageSource;
			melee.targetFinder = finder;
			melee.attackCooldown = attackCooldown;
			melee.attackRange = attackRange;
			melee.attackSoundEffect = attackSoundEffect.Item2;
		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.damageType = (DamageType)serializer.ReadByte( "DamageType" );
			this.damage = serializer.ReadFloat( "Damage" );
			this.armorPenetration = serializer.ReadFloat( "ArmorPenetration" );

			this.attackRange = serializer.ReadFloat( "AttackRange" );
			this.attackCooldown = serializer.ReadFloat( "AttackCooldown" );
			this.attackSoundEffect = serializer.ReadAudioClipFromAssets( "AttackSound" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
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