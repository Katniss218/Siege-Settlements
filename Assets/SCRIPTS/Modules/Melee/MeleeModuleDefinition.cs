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
	public class MeleeModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "melee";

		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
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
			serializer.WriteString( "", "AttackSound", (string)this.attackSoundEffect );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			MeleeModule module = gameObject.AddComponent<MeleeModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}