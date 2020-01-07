using SS.Levels.SaveStates;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Objects.Extras
{
	public static class ExtraCreator
	{
		private const string GAMEOBJECT_NAME = "Extra";


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static void SetData( GameObject gameObject, ExtraData data )
		{
			//
			//    CONTAINER GAMEOBJECT
			//

			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );
			
			//
			//    MODULES
			//

			Extra extra = gameObject.GetComponent<Extra>();
			SSObjectCreator.AssignModuleData( extra, data );
		}

		private static GameObject CreateExtra( ExtraDefinition def, Guid guid )
		{
			GameObject gameObject = new GameObject( GAMEOBJECT_NAME + " - '" + def.id + "'" );
			gameObject.isStatic = true;
			gameObject.layer = ObjectLayer.EXTRAS;

			//
			//    CONTAINER GAMEOBJECT
			//

			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = !def.isObstacle;
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			if( def.isObstacle )
			{
				NavMeshObstacle obs = gameObject.AddComponent<NavMeshObstacle>();
				obs.size = def.size;
				obs.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
				obs.carving = true;
			}

			Extra extra = gameObject.AddComponent<Extra>();
			extra.guid = guid;
			extra.definitionId = def.id;
			extra.displayName = def.displayName;

			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( extra, def );

			return gameObject;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		/// <summary>
		/// Creates a new ExtraData from a GameObject.
		/// </summary>
		/// <param name="extra">The GameObject to extract the save state from. Must be an extra.</param>
		public static ExtraData GetData( Extra extra )
		{
			if( extra.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}

			ExtraData data = new ExtraData();
			data.guid = extra.guid;

			data.position = extra.transform.position;
			data.rotation = extra.transform.rotation;
			
			//
			// MODULES
			//

			SSObjectCreator.ExtractModulesToData( extra, data );


			return data;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		
		public static GameObject Create( ExtraDefinition def, Guid guid )
		{
			return CreateExtra( def, guid );
		}
	}
}