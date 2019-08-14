using SS.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class Extra : MonoBehaviour, IDefinableBy<ExtraDefinition>
	{
		public string id { get; private set; }


		private Transform graphicsTransform;
		private MeshFilter meshFilter;
		private MeshRenderer meshRenderer;


		void Awake()
		{
			this.graphicsTransform = this.transform.GetChild( 0 );
			this.meshFilter = this.graphicsTransform.GetComponent<MeshFilter>();
			this.meshRenderer = this.graphicsTransform.GetComponent<MeshRenderer>();
		}

		void Start()
		{

		}

		void Update()
		{

		}

		public void AssignDefinition( ExtraDefinition def )
		{

			this.meshFilter.mesh = def.mesh.Item2;
			this.meshRenderer.material = Main.materialPlantTransparent;
			this.meshRenderer.material.SetTexture( "_Albedo", def.albedo.Item2 );
			this.meshRenderer.material.SetTexture( "_Normal", def.normal.Item2 );
			this.meshRenderer.material.SetFloat( "_Metallic", 0.0f );
			this.meshRenderer.material.SetFloat( "_Smoothness", 0.25f );
		}
		
		public static GameObject Create( ExtraDefinition def, Vector3 pos, Quaternion rot )
		{
			if( def == null )
			{
				throw new System.Exception( "Definition can't be null" );
			}
			GameObject container = new GameObject( "Extra (\"" + def.id + "\")" );

			GameObject gfx = new GameObject( "graphics" );
			gfx.transform.SetParent( container.transform );

			MeshFilter meshFilter = gfx.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gfx.AddComponent<MeshRenderer>();

			
			container.transform.position = pos;
			
			Extra extra = container.AddComponent<Extra>();
			extra.AssignDefinition( def );



			return container;
		}
	}
}