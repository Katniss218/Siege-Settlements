﻿using Katniss.Utils;
using KFF;
using SS.Data;
using System;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition
	{
		// Basic.
		public string displayName { get; set; }

		// Health-related
		public float healthMax { get; set; }
		public float slashArmor { get; set; }
		public float pierceArmor { get; set; }
		public float concussionArmor { get; set; }

		// Melee-related
		public bool isMelee { get; set; }
		public DamageType meleeDamageType { get; set; }
		public float meleeDamage { get; set; }
		public float meleeArmorPenetration { get; set; }
		public float meleeAttackRange { get; set; }
		public float meleeAttackCooldown { get; set; }
		public Tuple<string, AudioClip> meleeAttackSoundEffect { get; private set; }

		// Ranged-related
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

		// Movement-related
		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }
		
		// Display-related
		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }
		public Tuple<string, Sprite> icon { get; private set; }

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
				
				this.meleeAttackSoundEffect = serializer.ReadAudioClipFromAssets( "MeleeData.AttackSound" );
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
				
				this.rangedAttackSoundEffect = serializer.ReadAudioClipFromAssets( "RangedData.AttackSound" );
			}
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			this.albedo = serializer.ReadTexture2DFromAssets( "AlbedoTexture", TextureType.Albedo );
			this.normal = serializer.ReadTexture2DFromAssets( "NormalTexture", TextureType.Normal );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
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
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}