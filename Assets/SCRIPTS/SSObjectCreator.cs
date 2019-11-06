﻿using SS.Content;
using SS.Modules;
using System;
using UnityEngine;

namespace SS
{
	public static class SSObjectCreator
	{
		public static void AssignModules( GameObject gameObject, ObjectDefinition def, ObjectData data )
		{
			Guid[] moduleDefIds;
			ModuleDefinition[] moduleDefinitions;

			Guid[] moduleDataIds;
			ModuleData[] moduleData;

			def.GetAllModules( out moduleDefIds, out moduleDefinitions );
			data.GetAllModules( out moduleDataIds, out moduleData );

			for( int i = 0; i < moduleDefIds.Length; i++ )
			{
				if( moduleDataIds.Length == 0 )
				{
					Debug.Log( "No module data corresponding to moduleId of '" + moduleDefIds[i].ToString( "D" ) + "' was found." );
					moduleDefinitions[i].AddModule( gameObject, moduleDefIds[i], moduleDefinitions[i].GetIdentityData() );
					continue;
				}
				for( int j = 0; j < moduleDataIds.Length; j++ )
				{
					if( moduleDefIds[i] == moduleDataIds[j] )
					{
						moduleDefinitions[i].AddModule( gameObject, moduleDefIds[i], moduleData[j] );
						break;
					}
					else if( j == moduleDataIds.Length - 1 )
					{
						Debug.Log( "No module data corresponding to moduleId of '" + moduleDefIds[i].ToString( "D" ) + "' was found." );
						moduleDefinitions[i].AddModule( gameObject, moduleDefIds[i], moduleDefinitions[i].GetIdentityData() );
					}
				}
			}
		}

		public static void ExtractModules( GameObject gameObject, ObjectData data )
		{
			Module[] modules = gameObject.GetComponents<Module>();
			for( int i = 0; i < modules.Length; i++ )
			{
				ModuleData moduleData = modules[i].GetData();
				data.AddModuleData( modules[i].moduleId, moduleData );
			}
		}
	}
}