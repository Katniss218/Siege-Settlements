using KFF;
using SS.Objects.Buildings;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
using SS.Content;

namespace SS.Objects.Modules
{
	public class ResearchModuleDefinition : ModuleDefinition
	{
		public string[] researchableTechnologies { get; set; }
		public float researchSpeed { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( UnitDefinition ) ||
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true;
		}


		public override ModuleData GetIdentityData()
		{
			return new ResearchModuleData();
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.researchSpeed = serializer.ReadFloat( "ResearchSpeed" );
			this.researchableTechnologies = serializer.ReadStringArray( "ResearchableTechnologies" );
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteFloat( "", "ResearchSpeed", this.researchSpeed );
			serializer.WriteStringArray( "", "ResearchableTechnologies", this.researchableTechnologies );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}

		public override void AddModule( GameObject gameObject, Guid moduleId, ModuleData data )
		{
			ResearchModule module = gameObject.AddComponent<ResearchModule>();
			module.moduleId = moduleId;
			module.SetDefData( this, data );
		}
	}
}