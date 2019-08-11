using Katniss.Utils;
using KFF;
using SS.DataStructures;
using System;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : Definition
	{
		public float healthMax { get; set; }
		public float slashArmor { get; set; }
		public float pierceArmor { get; set; }
		public float concussionArmor { get; set; }

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

		public float movementSpeed { get; set; }
		public float rotationSpeed { get; set; }

		public float radius { get; set; }
		public float height { get; set; }

		public int inventorySize { get; set; }

		public Tuple<string, Mesh> mesh { get; private set; }
		public Tuple<string, Texture2D> albedo { get; private set; }
		public Tuple<string, Texture2D> normal { get; private set; }

		public UnitDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.slashArmor = serializer.ReadFloat( "SlashArmor" );
			this.pierceArmor = serializer.ReadFloat( "PierceArmor" );
			this.concussionArmor = serializer.ReadFloat( "ConcussionArmor" );
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
			}
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );
			this.inventorySize = serializer.ReadInt( "InventorySize" );
			string meshPath = serializer.ReadString( "Mesh" );
			this.mesh = new Tuple<string, Mesh>( meshPath, AssetsManager.GetMesh( meshPath ) );
			string albedoPath = serializer.ReadString( "AlbedoTexture" );
			this.albedo = new Tuple<string, Texture2D>( albedoPath, AssetsManager.GetTexture2D( albedoPath, TextureType.Albedo ) );
			string normalPath = serializer.ReadString( "NormalTexture" );
			this.normal = new Tuple<string, Texture2D>( normalPath, AssetsManager.GetTexture2D( normalPath, TextureType.Normal ) );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			// FIXME! - Add an in-game debug system for loading definitions (exception handling).
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.WriteFloat( "", "SlashArmor", this.slashArmor );
			serializer.WriteFloat( "", "PierceArmor", this.pierceArmor );
			serializer.WriteFloat( "", "ConcussionArmor", this.concussionArmor );
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
			}
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );
			serializer.WriteInt( "", "InventorySize", this.inventorySize );
			serializer.WriteString( "", "Mesh", this.mesh.Item1 );
			serializer.WriteString( "", "AlbedoTexture", this.albedo.Item1 );
			serializer.WriteString( "", "NormalTexture", this.normal.Item1 );
		}
	}
}