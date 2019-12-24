using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using System;
using UnityEngine;

namespace SS.Objects
{
	public static class SSObjectCreator
	{
		public static void AssignSubObjects( GameObject gameObject, SSObjectDefinition def )
		{
			SubObjectDefinition[] subObjectDefinitions;

			def.GetAllSubObjects( out subObjectDefinitions );

			for( int i = 0; i < subObjectDefinitions.Length; i++ )
			{
				subObjectDefinitions[i].AddTo( gameObject );
			}
		}


		/// <summary>
		/// Assigns data to each module that has it's data present in the SSObjectData.
		/// </summary>
		public static void AssignModuleData( SSObject ssObject, SSObjectData data )
		{
			SSModule[] modules = ssObject.GetModules();

			Guid[] moduleDataIds;
			ModuleData[] moduleData;
			data.GetAllModules( out moduleDataIds, out moduleData );
			
			//

			// for each module, find data.
			for( int i = 0; i < modules.Length; i++ )
			{
				if( moduleDataIds.Length == 0 )
				{
					//Debug.Log( "No module data corresponding to moduleId of '" + modules[i].moduleId.ToString( "D" ) + "' was found." );
					continue;
				}
				for( int j = 0; j < moduleDataIds.Length; j++ )
				{
					if( modules[i].moduleId == moduleDataIds[j] )
					{
						modules[i].SetData( moduleData[j] );
						break;
					}
					/*else if( j == moduleDataIds.Length - 1 )
					{
						Debug.Log( "No module data corresponding to moduleId of '" + modules[i].moduleId.ToString( "D" ) + "' was found.." );
					}*/
				}
			}
		}

		public static void AssignModules( GameObject gameObject, SSObjectDefinition def )
		{
			Guid[] moduleDefIds;
			ModuleDefinition[] moduleDefinitions;
			
			def.GetAllModules( out moduleDefIds, out moduleDefinitions );

			for( int i = 0; i < moduleDefIds.Length; i++ )
			{
				moduleDefinitions[i].AddModule( gameObject, moduleDefIds[i] );
			}
		}


		/*public static void AssignModules( GameObject gameObject, SSObjectDefinition def, SSObjectData data )
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
					Debug.Log( "No module data corresponding to moduleId of '" + moduleDefIds[i].ToString( "D" ) + "' was found. - Creating default data." );
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
						Debug.Log( "No module data corresponding to moduleId of '" + moduleDefIds[i].ToString( "D" ) + "' was found. - Creating default data." );
						moduleDefinitions[i].AddModule( gameObject, moduleDefIds[i], moduleDefinitions[i].GetIdentityData() );
					}
				}
			}
		}*/

		public static void ExtractModulesToData( SSObject ssObject, SSObjectData data )
		{
			SSModule[] modules = ssObject.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				ModuleData moduleData = modules[i].GetData();
				data.AddModuleData( modules[i].moduleId, moduleData );
			}
		}
	}
}