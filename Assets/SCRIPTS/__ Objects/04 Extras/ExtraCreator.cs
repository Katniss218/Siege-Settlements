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

		public static void SetDefData( GameObject gameObject, ExtraDefinition def, ExtraData data )
		{
			//
			//    SUB-OBJECTS
			//

			SSObjectCreator.AssignSubObjects( gameObject, def );

			//
			//    CONTAINER GAMEOBJECT
			//

			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			Extra extra = gameObject.GetComponent<Extra>();
			extra.definitionId = def.id;
			extra.displayName = def.displayName;


			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
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

			//
			//    MODULES
			//

			SSObjectCreator.AssignModules( gameObject, def, data );
		}

		private static GameObject CreateExtra( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.isStatic = true;
			container.layer = ObjectLayer.EXTRAS;

			//
			//    CONTAINER GAMEOBJECT
			//

			Extra extra = container.AddComponent<Extra>();
			extra.guid = guid;

			BoxCollider collider = container.AddComponent<BoxCollider>();
			
			return container;
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

		public static GameObject CreateEmpty( Guid guid )
		{
			GameObject gameObject = CreateExtra( guid );

			return gameObject;
		}

		public static GameObject Create( ExtraDefinition def, ExtraData data )
		{
			GameObject gameObject = CreateExtra( data.guid );

			SetDefData( gameObject, def, data );

			return gameObject;
		}
	}
}