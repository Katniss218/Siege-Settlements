using System;
using System.Collections.Generic;
using KFF;
using SS.Objects.Buildings;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using UnityEngine;
using SS.Content;

namespace SS.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "barracks";

		public string[] trainableUnits { get; set; }
		public float trainSpeed { get; set; }

		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition );
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
			return new BarracksModuleData();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.trainSpeed = serializer.ReadFloat( "TrainSpeed" );
			this.trainableUnits = serializer.ReadStringArray( "TrainableUnits" );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "TrainSpeed", this.trainSpeed );
			serializer.WriteStringArray( "", "TrainableUnits", this.trainableUnits );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			BarracksModule module = gameObject.AddComponent<BarracksModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}