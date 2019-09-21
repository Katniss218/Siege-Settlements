using Katniss.Utils;
using UnityEngine;

namespace SS.Extras
{
	public static class ExtraCreator
	{
		public static GameObject Create( ExtraDefinition def, Vector3 pos, Quaternion rot )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Extra (\"" + def.id + "\")" );
			container.isStatic = true;
			container.layer = ObjectLayer.EXTRAS;

			GameObject gfx = new GameObject( GameObjectUtils.GRAPHICS_GAMEOBJECT_NAME );
			gfx.transform.SetParent( container.transform );
			gfx.isStatic = true;

			container.transform.SetPositionAndRotation( pos, rot );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			meshFilter.mesh = def.mesh.Item2;
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreatePlantTransparent( def.albedo.Item2, def.normal.Item2, null, 0.0f, 0.25f, 0.3333f );
			
			
			return container;
		}
	}
}