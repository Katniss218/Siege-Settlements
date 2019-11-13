using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class MeshSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "MESH";

		public AddressableAsset<Mesh> mesh { get; set; }
		
		public MaterialDefinition materialData { get; set; }
		

		public override SubObject AddTo( GameObject gameObject )
		{
			GameObject child = new GameObject( "Sub-Object [" + KFF_TYPEID + "] '" + this.subObjectId.ToString( "D" ) + "'" );
			child.transform.SetParent( gameObject.transform );

			child.transform.localPosition = this.localPosition;
			child.transform.localRotation = this.localRotation;
			
			MeshFilter meshFilter = child.AddComponent<MeshFilter>();
			meshFilter.mesh = this.mesh;

			MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
			meshRenderer.material = MaterialManager.CreateMaterial( this.materialData );

			SubObject subObject = child.AddComponent<SubObject>();
			subObject.subObjectId = this.subObjectId;

			subObject.defaultPosition = this.localPosition;
			subObject.defaultRotation = this.localRotation;

			return subObject;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.subObjectId = Guid.ParseExact( serializer.ReadString( "SubObjectId" ), "D" );

			this.localPosition = serializer.ReadVector3( "LocalPosition" );
			this.localRotation = Quaternion.Euler( serializer.ReadVector3( "LocalRotationEuler" ) );
			
			this.mesh = serializer.ReadMeshFromAssets( "Mesh" );

			this.materialData = new MaterialDefinition();
			serializer.Deserialize( "Material", this.materialData );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "SubObjectId", this.subObjectId.ToString( "D" ) );

			serializer.WriteVector3( "", "LocalPosition", this.localPosition );
			serializer.WriteVector3( "", "LocalRotationEuler", this.localRotation.eulerAngles );

			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.Serialize( "", "Material", this.materialData );
		}
	}
}