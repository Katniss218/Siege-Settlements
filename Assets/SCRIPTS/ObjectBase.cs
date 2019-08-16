using UnityEngine;

namespace SS
{
	public class ObjectBase : MonoBehaviour
	{
		public const string GRAPHICS_GAMEOBJECT_NAME = "graphics";

		public string id;

		private GameObject __graphicsGameObject;
		public GameObject graphicsGameObject
		{
			get
			{
				if( this.__graphicsGameObject == null )
				{
					this.__graphicsGameObject = this.transform.Find( GRAPHICS_GAMEOBJECT_NAME ).gameObject;
				}
				return this.__graphicsGameObject;
			}
		}
		private MeshFilter __meshFilter;
		public MeshFilter meshFilter
		{
			get
			{
				if( this.__meshFilter == null )
				{
					this.__meshFilter = graphicsGameObject.GetComponent<MeshFilter>();
				}
				return this.__meshFilter;
			}
		}
		private MeshRenderer __meshRenderer;
		public MeshRenderer meshRenderer
		{
			get
			{
				if( this.__meshRenderer == null )
				{
					this.__meshRenderer = graphicsGameObject.GetComponent<MeshRenderer>();
				}
				return this.__meshRenderer;
			}
		}
	}
}