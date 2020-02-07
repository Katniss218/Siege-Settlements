using SS.Content;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using SS.UI;
using System;

namespace SS.Objects
{
	public static class SSObjectCreator
	{
		public static void AnalyzeAttributes( SSObject ssObject, object obj )
		{
			Type type = obj.GetType();

			object[] attributes = type.GetCustomAttributes( true );

			for( int i = 0; i < attributes.Length; i++ )
			{
				if( attributes[i] is UseHudAttribute )
				{
					if( ssObject.hudBase == null )
					{
						ssObject.hudBase = HudContainer.CreateGameObject( ssObject );
					}

					UseHudAttribute attrib = (UseHudAttribute)attributes[i];

					object hudComponent = ssObject.hudBase.gameObject.AddComponent( attrib.hudType );

					type.GetProperty( attrib.fieldName ).SetValue( obj, hudComponent );
				}
			}
		}

		//
		//	Sub-Objects
		//


		public static void AssignSubObjects( SSObject ssObject, SSObjectDefinition def )
		{
			SubObjectDefinition[] subObjectDefinitions;

			def.GetAllSubObjects( out subObjectDefinitions );

			for( int i = 0; i < subObjectDefinitions.Length; i++ )
			{
				subObjectDefinitions[i].AddTo( ssObject );
			}

			ssObject.SealSubObjects();
		}

		
		//
		//	Modules
		//


		public static void AssignModules( SSObject ssObject, SSObjectDefinition def )
		{
			Guid[] moduleDefIds;
			SSModuleDefinition[] moduleDefinitions;

			def.GetAllModules( out moduleDefIds, out moduleDefinitions );

			for( int i = 0; i < moduleDefIds.Length; i++ )
			{
				moduleDefinitions[i].AddModule( ssObject, moduleDefIds[i] );
			}

			ssObject.SealModules();
		}
		
		public static void AssignModuleData( SSObject ssObject, SSObjectData data )
		{
			// Assigns data for every module present on the SSObject, that has data corresponding to it in the SSObjectData.

			SSModule[] modules = ssObject.GetModules();

			Guid[] moduleDataIds;
			SSModuleData[] moduleData;
			data.GetAllModules( out moduleDataIds, out moduleData );
			
			// for each module, find the corresponding data (if present), and assign it.
			for( int i = 0; i < modules.Length; i++ )
			{
				if( moduleDataIds.Length == 0 )
				{
					continue;
				}

				// Look through module data and assign the one with corresponding moduleId (if present).
				for( int j = 0; j < moduleDataIds.Length; j++ )
				{
					if( modules[i].moduleId == moduleDataIds[j] )
					{
						modules[i].SetData( moduleData[j] );
						break;
					}
				}
			}
		}

		public static void ExtractModulesToData( SSObject ssObject, SSObjectData data )
		{
			SSModule[] modules = ssObject.GetModules();
			for( int i = 0; i < modules.Length; i++ )
			{
				SSModuleData moduleData = modules[i].GetData();
				data.AddModuleData( modules[i].moduleId, moduleData );
			}
		}
	}
}