﻿using SS.Levels.SaveStates;
using System;
using UnityEngine;

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
			//    GRAPHICS GAMEOBJECT
			//

			GameObject gfx = gameObject.transform.Find( Main.GRAPHICS_GAMEOBJECT_NAME ).gameObject;
			
			MeshFilter meshFilter = gfx.GetComponent<MeshFilter>();
			meshFilter.mesh = def.mesh;

			MeshRenderer meshRenderer = gfx.GetComponent<MeshRenderer>();
			
			meshRenderer.material = def.shaderType == MaterialType.PlantOpaque ? MaterialManager.CreatePlantOpaque( def.albedo, def.normal, null, def.metallic, def.smoothness, 0.3333f ) : MaterialManager.CreateOpaque( def.albedo, def.normal, null, 0.0f, 0.25f );

			//
			//    CONTAINER GAMEOBJECT
			//
			gameObject.transform.SetPositionAndRotation( data.position, data.rotation );

			Extra extra = gameObject.GetComponent<Extra>();
			extra.defId = def.id;
		}
		
		private static GameObject CreateExtra( Guid guid )
		{
			GameObject container = new GameObject( GAMEOBJECT_NAME );
			container.isStatic = true;
			container.layer = ObjectLayer.EXTRAS;

			GameObject gfx = new GameObject( Main.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;
			
			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			
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