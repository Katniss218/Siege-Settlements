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
		

		public override SubObject AddTo( SSObject ssObject )
		{
			var subTuple = ssObject.AddSubObject<MeshSubObject>( this.subObjectId );

			subTuple.go.transform.localPosition = this.localPosition;
			subTuple.go.transform.localRotation = this.localRotation;

			subTuple.sub.defaultPosition = this.localPosition;
			subTuple.sub.defaultRotation = this.localRotation;
			subTuple.sub.SetMesh( this.mesh );
			subTuple.sub.SetMaterial( MaterialManager.CreateMaterial( this.materialData ) );

			return subTuple.Item2;
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.subObjectId = serializer.ReadGuid( "SubObjectId" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'SubObjectId' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.localPosition = serializer.ReadVector3( "LocalPosition" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LocalPosition' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.localRotation = Quaternion.Euler( serializer.ReadVector3( "LocalRotationEuler" ) );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'LocalRotationEuler' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.mesh = serializer.ReadMeshFromAssets( "Mesh" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing or invalid value of 'Mesh' (" + serializer.file.fileName + ")." );
			}

			this.materialData = new MaterialDefinition();
			try
			{
				serializer.Deserialize( "Material", this.materialData );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing or invalid value of 'Material' (" + serializer.file.fileName + ")." );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteGuid( "", "SubObjectId", this.subObjectId );

			serializer.WriteVector3( "", "LocalPosition", this.localPosition );
			serializer.WriteVector3( "", "LocalRotationEuler", this.localRotation.eulerAngles );

			serializer.WriteString( "", "Mesh", (string)this.mesh );
			serializer.Serialize( "", "Material", this.materialData );
		}
	}
}