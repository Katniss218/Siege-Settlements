using KFF;
using SS.Objects.Buildings;
using SS.Content;
using SS.Objects.Heroes;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using SS.Objects.SubObjects;

namespace SS.Objects.Modules
{
	public class MeleeModuleDefinition : SSModuleDefinition
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

		public override void AddModule( SSObject ssObject, Guid moduleId )
		{
			MeleeModule module = ssObject.AddModule<MeleeModule>( moduleId );
			//module.displayName = this.displayName;
			//module.icon = this.icon;
			module.attackRange = this.attackRange;

			module.damage = this.damage;
			module.damageType = this.damageType;
			module.armorPenetration = this.armorPenetration;

			module.attackCooldown = this.attackCooldown;
			module.attackSoundEffect = this.attackSoundEffect;

			module.traversibleSubObjects = new SubObject[this.traversibleSubObjects.Length];
			for( int i = 0; i < module.traversibleSubObjects.Length; i++ )
			{
				SubObject trav = module.ssObject.GetSubObject( this.traversibleSubObjects[i] );
				module.traversibleSubObjects[i] = trav ?? throw new Exception( "Can't find Sub-Object with Id of '" + this.traversibleSubObjects[i].ToString( "D" ) + "'." );
			}
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
						this.traversibleSubObjects[i] = serializer.ReadGuid( new Path( "TraversibleSubObjects.{0}", i ) );
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


			/*try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' (" + serializer.file.fileName + ")." );
			}*/
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteByte( "", "DamageType", (byte)this.damageType );
			serializer.WriteFloat( "", "Damage", this.damage );
			serializer.WriteFloat( "", "ArmorPenetration", this.armorPenetration );

			serializer.WriteFloat( "", "AttackRange", this.attackRange );
			serializer.WriteFloat( "", "AttackCooldown", this.attackCooldown );
			serializer.WriteString( "", "AttackSound", (string)this.attackSoundEffect );

			serializer.WriteGuidArray( "", "TraversibleSubObjects", this.traversibleSubObjects );

			//serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}