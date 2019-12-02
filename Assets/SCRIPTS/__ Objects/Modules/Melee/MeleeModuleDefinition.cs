using KFF;
using SS.Objects.Buildings;
using SS.Content;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class MeleeModuleDefinition : ModuleDefinition
	{
		public DamageType damageType { get; set; }
		public float damage { get; set; }
		public float armorPenetration { get; set; }

		public float attackRange { get; set; }
		public float attackCooldown { get; set; }
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

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			MeleeModule module = gameObject.AddComponent<MeleeModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}


		public override ModuleData GetIdentityData()
		{
			return new MeleeModuleData();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.damageType = (DamageType)serializer.ReadByte( "DamageType" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DamageType' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.damage = serializer.ReadFloat( "Damage" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Damage' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.armorPenetration = serializer.ReadFloat( "ArmorPenetration" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ArmorPenetration' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.attackRange = serializer.ReadFloat( "AttackRange" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'AttackRange' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.attackCooldown = serializer.ReadFloat( "AttackCooldown" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'AttackCooldown' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.attackSoundEffect = serializer.ReadAudioClipFromAssets( "AttackSound" );
			}
			catch
			{
				throw new Exception( "Missing 'AttackSound' (" + serializer.file.fileName + ")." );
			}

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "TraversibleSubObjects" );
			if( analysisData.isSuccess )
			{
				this.traversibleSubObjects = new Guid[analysisData.childCount];
				try
				{
					for( int i = 0; i < this.traversibleSubObjects.Length; i++ )
					{
						this.traversibleSubObjects[i] = Guid.ParseExact( serializer.ReadString( new Path( "TraversibleSubObjects.{0}", i ) ), "D" );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'TraversibleSubObjects' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing 'TraversibleSubObjects' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "DamageType", (byte)this.damageType );
			serializer.WriteFloat( "", "Damage", this.damage );
			serializer.WriteFloat( "", "ArmorPenetration", this.armorPenetration );

			serializer.WriteFloat( "", "AttackRange", this.attackRange );
			serializer.WriteFloat( "", "AttackCooldown", this.attackCooldown );
			serializer.WriteString( "", "AttackSound", (string)this.attackSoundEffect );

			string[] guidArray = new string[this.traversibleSubObjects.Length];
			for( int i = 0; i < guidArray.Length; i++ )
			{
				guidArray[i] = this.traversibleSubObjects[i].ToString( "D" );
			}
			serializer.WriteStringArray( "", "TraversibleSubObjects", guidArray );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}