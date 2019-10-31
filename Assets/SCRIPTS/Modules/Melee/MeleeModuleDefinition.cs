using KFF;
using SS.Content;
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
		public AddressableAsset<AudioClip> attackSoundEffect { get; private set; }

		public override bool CanBeAddedTo( GameObject gameObject )
		{
			if( ((ObjectLayer.UNITS_MASK | ObjectLayer.BUILDINGS_MASK | ObjectLayer.HEROES_MASK) & (1 << gameObject.layer)) != (1 << gameObject.layer) )
			{
				return false;
			}
			return true;
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
	}
}