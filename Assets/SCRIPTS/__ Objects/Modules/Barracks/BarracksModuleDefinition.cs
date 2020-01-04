using System;
using System.Collections.Generic;
using KFF;
using SS.Objects.Buildings;
using SS.Levels.SaveStates;
using SS.Objects.Units;
using UnityEngine;
using SS.Content;

namespace SS.Objects.Modules
{
	public class BarracksModuleDefinition : ModuleDefinition
	{
		public string[] trainableUnits { get; set; }
		public float trainSpeed { get; set; }

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
			BarracksModule module = gameObject.AddComponent<BarracksModule>();
			module.moduleId = moduleId;
			module.displayName = this.displayName;
			module.icon = this.icon;
			module.trainSpeed = this.trainSpeed;
			module.trainableUnits = new UnitDefinition[this.trainableUnits.Length];
			for( int i = 0; i < this.trainableUnits.Length; i++ )
			{
				module.trainableUnits[i] = DefinitionManager.GetUnit( this.trainableUnits[i] );
			}
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.trainSpeed = serializer.ReadFloat( "TrainSpeed" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TrainSpeed' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.trainableUnits = serializer.ReadStringArray( "TrainableUnits" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TrainableUnits' (" + serializer.file.fileName + ")." );
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
			serializer.WriteFloat( "", "TrainSpeed", this.trainSpeed );
			serializer.WriteStringArray( "", "TrainableUnits", this.trainableUnits );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}