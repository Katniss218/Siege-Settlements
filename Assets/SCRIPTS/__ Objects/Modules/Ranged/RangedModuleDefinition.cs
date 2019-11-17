using KFF;
using SS.Objects.Buildings;
using SS.Content;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
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

		/// <summary>
		/// Contains a list of guids for each of the sub-objects that are rotated towards the target.
		/// </summary>
		public Guid[] traversibleSubObjects { get; set; }


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


		public override ModuleData GetIdentityData()
		{
			return new RangedModuleData();
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

			this.traversibleSubObjects = new Guid[serializer.Analyze( "TraversibleSubObjects" ).childCount];
			for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
			{
				this.traversibleSubObjects[i] = Guid.ParseExact( serializer.ReadString( new Path( "TraversibleSubObjects.{0}", i ) ), "D" );
			}
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

			string[] guidArray = new string[this.traversibleSubObjects.Length];
			for( int i = 0; i < guidArray.Length; i++ )
			{
				guidArray[i] = this.traversibleSubObjects[i].ToString( "D" );
			}
			serializer.WriteStringArray( "", "TraversibleSubObjects", guidArray );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			RangedModule module = gameObject.AddComponent<RangedModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}