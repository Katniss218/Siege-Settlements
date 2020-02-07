using System;
using System.Collections.Generic;
using KFF;
using SS.Content;
using UnityEngine;

namespace SS.Objects.SubObjects
{
	public class MeshPredicatedSubObjectDefinition : SubObjectDefinition
	{
		public const string KFF_TYPEID = "MESH_PREDICATED";

		public Dictionary<int, AddressableAsset<Mesh>> meshes { get; set; }
		public MaterialDefinition materialData { get; set; }


		public override SubObject AddTo( SSObject ssObject )
		{
			if( this.meshes == null || this.meshes.Count < 1 )
			{
				throw new Exception( "Mesh dictionary must have at least 1 element." );
			}

			var sub = ssObject.AddSubObject<MeshPredicatedSubObject>( this.subObjectId );

			sub.Item1.transform.localPosition = this.localPosition;
			sub.Item1.transform.localRotation = this.localRotation;

			sub.Item2.defaultPosition = this.localPosition;
			sub.Item2.defaultRotation = this.localRotation;
			sub.Item2.meshes = new Dictionary<int, Mesh>( this.meshes.Count );
			int? firstKey = null;
			foreach( var kvp in this.meshes )
			{
				if( firstKey == null )
				{
					firstKey = kvp.Key;
				}
				sub.Item2.meshes.Add( kvp.Key, (Mesh)kvp.Value );
			}
			sub.Item2.lookupKey = firstKey.Value;
			sub.Item2.SetMaterial( MaterialManager.CreateMaterial( this.materialData ) );

			return sub.Item2;
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

			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Meshes" );
			if( analysisData.isSuccess )
			{
				this.meshes = new Dictionary<int, AddressableAsset<Mesh>>();
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						int key = serializer.ReadInt( new Path( "Meshes.{0}.Key", i ) );
						AddressableAsset<Mesh> mesh = serializer.ReadMeshFromAssets( new Path( "Meshes.{0}.Mesh", i ) );
						this.meshes.Add( key, mesh );
					}
				}
				catch( KFFException )
				{
					throw new Exception( "Missing or invalid value of 'Meshes' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing or invalid value of 'Meshes' (" + serializer.file.fileName + ")." );
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


			serializer.WriteList( "", "MeshMats" );
			int i = 0;
			foreach( var kvp in this.meshes )
			{
				serializer.AppendClass( "MeshMats" );
				serializer.WriteInt( new Path( "Cost.{0}", i ), "Key", kvp.Key );
				serializer.WriteString( new Path( "Cost.{0}", i ), "Mesh", (string)kvp.Value );
				i++;
			}
			serializer.Serialize( "", "Material", this.materialData );
		}
	}
}