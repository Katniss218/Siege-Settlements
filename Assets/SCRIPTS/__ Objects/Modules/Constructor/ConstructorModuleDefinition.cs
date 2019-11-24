using System;
using System.Collections.Generic;
using KFF;
using SS.Objects.Buildings;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using UnityEngine;
using SS.Content;

namespace SS.Modules
{
	public class ConstructorModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "constructor";

		public string[] constructibleBuildings { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition ) ||
				objType == typeof( HeroDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return !(
				modTypes.Contains( typeof( ResearchModuleDefinition ) ) ||
				modTypes.Contains( typeof( BarracksModuleDefinition ) ) ||
				modTypes.Contains( typeof( ConstructorModuleDefinition ) ));
		}

		public override ModuleData GetIdentityData()
		{
			return new ConstructorModuleData();
		}

		
		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.constructibleBuildings = serializer.ReadStringArray( "ConstructibleBuildings" );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteStringArray( "", "ConstructibleBuildings", this.constructibleBuildings );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			ConstructorModule module = gameObject.AddComponent<ConstructorModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}