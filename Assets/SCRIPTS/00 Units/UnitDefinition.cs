using Katniss.Utils;
using KFF;
using SS.Data;
using SS.ResourceSystem;
using SS.Technologies;
using System;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition, ITechsRequired
	{
		// Basic.
		public string displayName { get; set; }

		// Health-related
		public float healthMax { get; set; }

		public Armor armor { get; set; }

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

		public ResourceStack[] cost { get; private set; }
		public float buildTime { get; private set; }
		public string[] techsRequired { get; private set; } // the default techs required to unlock. TODO ----- interface for this? IUnlockable or sth

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
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );
			var analysisData = serializer.Analyze( "MeleeModule" );
			if( analysisData.isSuccess )
			{
				this.isMelee = true;
				this.meleeDamageType = (DamageType)serializer.ReadByte( "MeleeModule.DamageType" );
				this.meleeDamage = serializer.ReadFloat( "MeleeModule.Damage" );
				this.meleeArmorPenetration = serializer.ReadFloat( "MeleeModule.ArmorPenetration" );
				this.meleeAttackRange = serializer.ReadFloat( "MeleeModule.AttackRange" );
				this.meleeAttackCooldown = serializer.ReadFloat( "MeleeModule.AttackCooldown" );
				
				this.meleeAttackSoundEffect = serializer.ReadAudioClipFromAssets( "MeleeModule.AttackSound" );
			}
			analysisData = serializer.Analyze( "RangedModule" );
			if( analysisData.isSuccess )
			{
				this.isRanged = true;
				this.rangedProjectileId = serializer.ReadString( "RangedModule.ProjectileId" );
				this.rangedProjectileCount = serializer.ReadInt( "RangedModule.ProjectileCount" );
				this.rangedDamageType = (DamageType)serializer.ReadByte( "RangedModule.DamageType" );
				this.rangedDamage = serializer.ReadFloat( "RangedModule.Damage" );
				this.rangedArmorPenetration = serializer.ReadFloat( "RangedModule.ArmorPenetration" );
				this.rangedAttackRange = serializer.ReadFloat( "RangedModule.AttackRange" );
				this.rangedAttackCooldown = serializer.ReadFloat( "RangedModule.AttackCooldown" );
				this.rangedVelocity = serializer.ReadFloat( "RangedModule.Velocity" );
				this.rangedLocalOffsetMin = serializer.ReadVector3( "RangedModule.LocalOffsetMin" );
				this.rangedLocalOffsetMax = serializer.ReadVector3( "RangedModule.LocalOffsetMax" );
				
				this.rangedAttackSoundEffect = serializer.ReadAudioClipFromAssets( "RangedModule.AttackSound" );
			}
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

			analysisData = serializer.Analyze( "Cost" );
			this.cost = new ResourceStack[analysisData.childCount];
			for( int i = 0; i < this.cost.Length; i++ )
			{
				this.cost[i] = new ResourceStack( "unused", 0 );
			}
			serializer.DeserializeArray( "Cost", this.cost );
			this.buildTime = serializer.ReadFloat( "BuildTime" );
			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );

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
			serializer.Serialize( "", "Armor", this.armor );
			if( this.isMelee )
			{
				serializer.WriteClass( "", "MeleeModule" );
				serializer.WriteByte( "MeleeModule", "DamageType", (byte)this.meleeDamageType );
				serializer.WriteFloat( "MeleeModule", "Damage", this.meleeDamage );
				serializer.WriteFloat( "MeleeModule", "ArmorPenetration", this.meleeArmorPenetration );
				serializer.WriteFloat( "MeleeModule", "AttackRange", this.meleeAttackRange );
				serializer.WriteFloat( "MeleeModule", "AttackCooldown", this.meleeAttackCooldown );

				serializer.WriteString( "MeleeModule", "AttackSound", this.meleeAttackSoundEffect.Item1 );
			}
			if( this.isRanged )
			{
				serializer.WriteClass( "", "RangedModule" );
				serializer.WriteString( "RangedModule", "ProjectileId", this.rangedProjectileId );
				serializer.WriteInt( "RangedModule", "ProjectileCount", this.rangedProjectileCount );
				serializer.WriteByte( "RangedModule", "DamageType", (byte)this.rangedDamageType );
				serializer.WriteFloat( "RangedModule", "Damage", this.rangedDamage );
				serializer.WriteFloat( "RangedModule", "ArmorPenetration", this.rangedArmorPenetration );
				serializer.WriteFloat( "RangedModule", "AttackRange", this.rangedAttackRange );
				serializer.WriteFloat( "RangedModule", "AttackCooldown", this.rangedAttackCooldown );
				serializer.WriteFloat( "RangedModule", "Velocity", this.rangedVelocity );
				serializer.WriteVector3( "RangedModule", "LocalOffsetMin", this.rangedLocalOffsetMin );
				serializer.WriteVector3( "RangedModule", "LocalOffsetMax", this.rangedLocalOffsetMax );

				serializer.WriteString( "RangedModule", "AttackSound", this.rangedAttackSoundEffect.Item1 );
			}
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );

			serializer.SerializeArray( "", "Cost", this.cost );
			serializer.WriteFloat( "", "BuildTime", this.buildTime );
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );

			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
			serializer.WriteString( "", "Icon", this.icon.Item1 );
		}
	}
}