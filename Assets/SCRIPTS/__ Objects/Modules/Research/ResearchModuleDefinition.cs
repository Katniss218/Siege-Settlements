using KFF;
using SS.Objects.Buildings;
using SS.Levels.SaveStates;
using System;
using System.Collections.Generic;
using UnityEngine;
using SS.Content;
using SS.Technologies;

namespace SS.Objects.Modules
{
	public class ResearchModuleDefinition : ModuleDefinition
	{
		public string[] researchableTechnologies { get; set; }
		public float researchSpeed { get; set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true;
		}

		public override void AddModule( GameObject gameObject, Guid moduleId )
		{
			ResearchModule module = gameObject.AddComponent<ResearchModule>();
			module.moduleId = moduleId;
			module.icon = this.icon;
			module.researchSpeed = this.researchSpeed;
			module.researchableTechnologies = new TechnologyDefinition[this.researchableTechnologies.Length];
			for( int i = 0; i < module.researchableTechnologies.Length; i++ )
			{
				module.researchableTechnologies[i] = DefinitionManager.GetTechnology( this.researchableTechnologies[i] );
			}
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.researchSpeed = serializer.ReadFloat( "ResearchSpeed" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ResearchSpeed' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.researchableTechnologies = serializer.ReadStringArray( "ResearchableTechnologies" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ResearchableTechnologies' (" + serializer.file.fileName + ")." );
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
			serializer.WriteFloat( "", "ResearchSpeed", this.researchSpeed );
			serializer.WriteStringArray( "", "ResearchableTechnologies", this.researchableTechnologies );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}