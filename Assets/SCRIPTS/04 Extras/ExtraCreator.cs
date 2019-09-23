using Katniss.Utils;
using SS.Levels.SaveStates;
using UnityEngine;

namespace SS.Extras
{
	public static class ExtraCreator
	{
		private const string GAMEOBJECT_NAME = "Extra";



		public static string GetDefinitionId( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.HEROES )
			{
				throw new System.Exception( "The specified GameObject is not a hero." );
			}

			Extra extra = gameObject.GetComponent<Extra>();
			return extra.defId;
		}

		/// <summary>
		/// Creates a new ExtraData from a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject to extract the save state from. Must be an extra.</param>
		public static ExtraData GetSaveState( GameObject gameObject )
		{
			if( gameObject.layer != ObjectLayer.EXTRAS )
			{
				throw new System.Exception( "The specified GameObject is not an extra." );
			}

			ExtraData data = new ExtraData();

			data.position = gameObject.transform.position;
			data.rotation = gameObject.transform.rotation;

			return data;
		}

		public static GameObject Create( ExtraDefinition def, ExtraData data )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( GAMEOBJECT_NAME + " (\"" + def.id + "\")" );
			container.isStatic = true;
			container.layer = ObjectLayer.EXTRAS;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( data.position, data.rotation );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreatePlantTransparent( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.3333f );
			meshRenderer.material = def.shaderType == MaterialType.PlantOpaque ? MaterialManager.CreatePlantOpaque( def.albedo.Item2, def.normal.Item2, null, def.metallic, def.smoothness, 0.3333f ) : MaterialManager.CreateOpaque( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f );


			return container;
		}
	}
}