using System;
using System.Collections.Generic;
using KFF;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using UnityEngine;
using SS.Content;
using SS.Objects.Buildings;

namespace SS.Objects.Modules
{
	public class ConstructorModuleDefinition : ModuleDefinition
	{
		public string[] constructibleBuildings { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( HeroDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true;
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			ConstructorModule module = gameObject.AddComponent<ConstructorModule>();
			module.moduleId = moduleId;
			module.displayName = this.displayName;
			module.icon = this.icon;
			module.constructibleBuildings = new BuildingDefinition[this.constructibleBuildings.Length];
			for( int i = 0; i < module.constructibleBuildings.Length; i++ )
			{
				module.constructibleBuildings[i] = DefinitionManager.GetBuilding( this.constructibleBuildings[i] );
			}
		}
		
		
		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.constructibleBuildings = serializer.ReadStringArray( "ConstructibleBuildings" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ConstructibleBuildings' (" + serializer.file.fileName + ")." );
			}

			this.displayName = serializer.ReadString( "DisplayName" );

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
			serializer.WriteStringArray( "", "ConstructibleBuildings", this.constructibleBuildings );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}