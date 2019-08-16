using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition
	{
		public string displayName { get; set; }

		public float healthMax { get; set; }
		public float slashArmor { get; set; }
		public float pierceArmor { get; set; }
		public float concussionArmor { get; set; }

		public bool isMelee { get; set; }
		public DamageType meleeDamageType { get; set; }
		public float meleeDamage { get; set; }
		public float meleeArmorPenetration { get; set; }
		public float meleeAttackRange { get; set; }
		public float meleeAttackCooldown { get; set; }
		public Tuple<string, AudioClip> meleeAttackSoundEffect { get; private set; }

		public bool isRanged { get; set; }
		public string rangedProjectileId { get; set; }
		public int rangedProjectileCount { get; set; }
		public DamageType rangedDamageType { get; set; }
		public float rangedDamage { get; set; }
		public float rangedArmorPenetration { get; set; }
		public float rangedAttackRange { get; set; }
		public float rangedAttackCooldown { get; set; }
		public float rangedVelocity { get; set; }
		public Vector3 rangedLocalOffsetMin { get; set; }
		public Vector3 rangedLocalOffsetMax { get; set; }
		public Tuple<string, AudioClip> rangedAttackSoundEffect { get; private set; }

		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }
		
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }

		public UnitDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.slashArmor = serializer.ReadFloat( "SlashArmor" );
			this.pierceArmor = serializer.ReadFloat( "PierceArmor" );
			this.concussionArmor = serializer.ReadFloat( "ConcussionArmor" );
			this.isMelee = serializer.ReadBool( "IsMelee" );
			if( this.isMelee )
			{
				this.meleeDamageType = (DamageType)serializer.ReadByte( "MeleeData.DamageType" );
				this.meleeDamage = serializer.ReadFloat( "MeleeData.Damage" );
				this.meleeArmorPenetration = serializer.ReadFloat( "MeleeData.ArmorPenetration" );
				this.meleeAttackRange = serializer.ReadFloat( "MeleeData.AttackRange" );
				this.meleeAttackCooldown = serializer.ReadFloat( "MeleeData.AttackCooldown" );

				string meleeSfxPath = serializer.ReadString( "MeleeData.AttackSound" );
				this.meleeAttackSoundEffect = new Tuple<string, AudioClip>( meleeSfxPath, AssetsManager.GetAudioClip( meleeSfxPath ) );
			}
			this.isRanged = serializer.ReadBool( "IsRanged" );
			if( this.isRanged )
			{
				this.rangedProjectileId = serializer.ReadString( "RangedData.ProjectileId" );
				this.rangedProjectileCount = serializer.ReadInt( "RangedData.ProjectileCount" );
				this.rangedDamageType = (DamageType)serializer.ReadByte( "RangedData.DamageType" );
				this.rangedDamage = serializer.ReadFloat( "RangedData.Damage" );
				this.rangedArmorPenetration = serializer.ReadFloat( "RangedData.ArmorPenetration" );
				this.rangedAttackRange = serializer.ReadFloat( "RangedData.AttackRange" );
				this.rangedAttackCooldown = serializer.ReadFloat( "RangedData.AttackCooldown" );
				this.rangedVelocity = serializer.ReadFloat( "RangedData.Velocity" );
				this.rangedLocalOffsetMin = serializer.ReadVector3( "RangedData.LocalOffsetMin" );
				this.rangedLocalOffsetMax = serializer.ReadVector3( "RangedData.LocalOffsetMax" );

				string rangedSfxPath = serializer.ReadString( "RangedData.AttackSound" );
				this.rangedAttackSoundEffect = new Tuple<string, AudioClip>( rangedSfxPath, AssetsManager.GetAudioClip( rangedSfxPath ) );
			}
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

			string meshPath = serializer.ReadString( "Mesh" );
			this.mesh = new Tuple<string, Mesh>( meshPath, AssetsManager.GetMesh( meshPath ) );

			string albedoPath = serializer.ReadString( "AlbedoTexture" );
			this.albedo = new Tuple<string, Texture2D>( albedoPath, AssetsManager.GetTexture2D( albedoPath, TextureType.Albedo ) );

			string normalPath = serializer.ReadString( "NormalTexture" );
			this.normal = new Tuple<string, Texture2D>( normalPath, AssetsManager.GetTexture2D( normalPath, TextureType.Normal ) );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.WriteFloat( "", "SlashArmor", this.slashArmor );
			serializer.WriteFloat( "", "PierceArmor", this.pierceArmor );
			serializer.WriteFloat( "", "ConcussionArmor", this.concussionArmor );
			serializer.WriteBool( "", "IsMelee", this.isMelee );
			if( this.isMelee )
			{
				serializer.WriteClass( "", "MeleeData" );
				serializer.WriteByte( "MeleeData", "DamageType", (byte)this.meleeDamageType );
				serializer.WriteFloat( "MeleeData", "Damage", this.meleeDamage );
				serializer.WriteFloat( "MeleeData", "ArmorPenetration", this.meleeArmorPenetration );
				serializer.WriteFloat( "MeleeData", "AttackRange", this.meleeAttackRange );
				serializer.WriteFloat( "MeleeData", "AttackCooldown", this.meleeAttackCooldown );
				serializer.WriteString( "MeleeData", "AttackSound", this.meleeAttackSoundEffect.Item1 );
			}
			serializer.WriteBool( "", "IsRanged", this.isRanged );
			if( this.isRanged )
			{
				serializer.WriteClass( "", "RangedData" );
				serializer.WriteString( "RangedData", "ProjectileId", this.rangedProjectileId );
				serializer.WriteInt( "RangedData", "ProjectileCount", this.rangedProjectileCount );
				serializer.WriteByte( "RangedData", "DamageType", (byte)this.rangedDamageType );
				serializer.WriteFloat( "RangedData", "Damage", this.rangedDamage );
				serializer.WriteFloat( "RangedData", "ArmorPenetration", this.rangedArmorPenetration );
				serializer.WriteFloat( "RangedData", "AttackRange", this.rangedAttackRange );
				serializer.WriteFloat( "RangedData", "AttackCooldown", this.rangedAttackCooldown );
				serializer.WriteFloat( "RangedData", "Velocity", this.rangedVelocity );
				serializer.WriteVector3( "RangedData", "LocalOffsetMin", this.rangedLocalOffsetMin );
				serializer.WriteVector3( "RangedData", "LocalOffsetMax", this.rangedLocalOffsetMax );
				serializer.WriteString( "RangedData", "AttackSound", this.rangedAttackSoundEffect.Item1 );
			}
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );
			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
		}
	}
}