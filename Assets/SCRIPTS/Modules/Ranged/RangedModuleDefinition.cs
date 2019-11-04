using KFF;
using SS.Buildings;
using SS.Content;
using SS.Heroes;
using SS.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	public class RangedModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "ranged";

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
		public AddressableAsset<AudioClip> attackSoundEffect { get; private set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition ) ||
				objType == typeof( HeroDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true; // no module constraints
		}


		public override void DeserializeKFF( KFFSerializer serializer )
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

		public override void SerializeKFF( KFFSerializer serializer )
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

			serializer.WriteString( "", "AttackSound", (string)this.attackSoundEffect );
		}

		public override void AddModule( GameObject gameObject, ModuleData data )
		{
			RangedModule module = gameObject.AddComponent<RangedModule>();
			module.SetDefData( this, data );
		}
	}
}