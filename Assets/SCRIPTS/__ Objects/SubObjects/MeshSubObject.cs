using UnityEngine;

namespace SS.Objects.SubObjects
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class MeshSubObject : SubObject
	{
		private MeshRenderer __meshRenderer;
		private MeshRenderer meshRenderer
		{
			get
			{
				if( this.__meshRenderer == null )
				{
					this.__meshRenderer = this.GetComponent<MeshRenderer>();
				}
				return this.__meshRenderer;
			}
		}

		private MeshFilter __meshFilter;
		private MeshFilter meshFilter
		{
			get
			{
				if( this.__meshFilter == null )
				{
					this.__meshFilter = this.GetComponent<MeshFilter>();
				}
				return this.__meshFilter;
			}
		}

		public Mesh GetMesh()
		{
			return this.meshFilter.mesh;
		}

		public void SetMesh( Mesh mesh )
		{
			this.meshFilter.mesh = mesh;
		}

		public Material GetMaterial()
		{
			return this.meshRenderer.material;
		}

		public void SetMaterial( Material material )
		{
			this.meshRenderer.material = material;
		}
	}
}
