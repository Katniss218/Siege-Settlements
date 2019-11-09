﻿using SS.Levels.SaveStates;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SS.Extras
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
			extra.defId = def.id;
			extra.displayName = def.displayName;


			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.size = def.size;
			collider.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );

			NavMeshObstacle obs = gameObject.GetComponent<NavMeshObstacle>();
			obs.size = def.size;
			obs.center = new Vector3( 0.0f, def.size.y / 2.0f, 0.0f );
			obs.carving = def.size == Vector3.zero ? false : true;

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

			NavMeshObstacle obs = container.AddComponent<NavMeshObstacle>();

			return container;
		}


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public static string GetDefinitionId( GameObject gameObject )
		{
			if( !Extra.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid extra." );
			}

			Extra extra = gameObject.GetComponent<Extra>();
			return extra.defId;
		}

		/// <summary>
		/// Creates a new ExtraData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be an extra.</param>
		public static ExtraData GetData( GameObject gameObject )
		{
			if( !Extra.IsValid( gameObject ) )
			{
				throw new Exception( "GameObject '" + gameObject.name + "' is not a valid extra." );
			}

			ExtraData data = new ExtraData();

			Extra extra = gameObject.GetComponent<Extra>();
			if( extra.guid == null )
			{
				throw new Exception( "Guid was not assigned." );
			}
			data.guid = extra.guid.Value;

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			//
			// MODULES
			//

			SSObjectCreator.ExtractModules( gameObject, data );


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