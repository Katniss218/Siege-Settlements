using KFF;
using SS.Content;
using SS.Objects.Buildings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public class ResourceCollectorWorkplaceModuleDefinition : ModuleDefinition
	{
		public string resourceId { get; set; }

		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( BuildingDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return true; // no module constraints
		}

		public override void AddModule( SSObject ssObject, Guid moduleId )
		{
			ResourceCollectorWorkplaceModule module = ssObject.AddModule<ResourceCollectorWorkplaceModule>( moduleId );
			module.moduleId = moduleId;
			module.displayName = this.displayName;
			module.icon = this.icon;

			module.resourceId = this.resourceId;
			module.aoi = new AreaOfInfluence( ssObject.transform.position, 7.5f );
		}
		

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.resourceId = serializer.ReadString( "ResourceId" );
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
			serializer.WriteString( "", "ResourceId", this.resourceId );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "Icon", (string)this.icon );
		}
	}
}