using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	[RequireComponent( typeof( MeshFilter ) )]
	[RequireComponent( typeof( MeshRenderer ) )]
	public class MeshPredicatedSubObject : SubObject
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

		public Dictionary<int, Mesh> meshes { get; set; }

		int __lookupKey;
		public int lookupKey
		{
			get
			{
				return this.__lookupKey;
			}
			set
			{
				this.__lookupKey = value;				
				this.meshFilter.mesh = this.meshes[value];
			}
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