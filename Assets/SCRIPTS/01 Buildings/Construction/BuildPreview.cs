using System;
using UnityEngine;

namespace SS.Buildings
{
	public class BuildPreview : MonoBehaviour
	{
		public BuildingDefinition def;
		public LayerMask groundMask;
		
		const float maxDeviation = 0.2f;
		
		public bool CanBePlacedHere()
		{
			Matrix4x4 localToWorld = this.transform.localToWorldMatrix;
			Vector3 center = new Vector3( 0f, def.size.y / 2f, 0f );
			center = localToWorld.MultiplyVector( center ) + this.transform.position + new Vector3( 0, 0.01f, 0 );
			if( Physics.OverlapBox( center, def.size / 2, this.transform.rotation ).Length > 0 )
			{
				return false;
			}
			float halfHeight = def.size.y / 2f;
			Vector3 pos1 = new Vector3( -def.size.x, halfHeight, -def.size.z );
			Vector3 pos2 = new Vector3( -def.size.x, halfHeight, def.size.z );
			Vector3 pos3 = new Vector3( def.size.x, halfHeight, -def.size.z );
			Vector3 pos4 = new Vector3( def.size.x, halfHeight, def.size.z );
			pos1 = localToWorld.MultiplyVector( pos1 ) + this.transform.position;
			pos2 = localToWorld.MultiplyVector( pos2 ) + this.transform.position;
			pos3 = localToWorld.MultiplyVector( pos3 ) + this.transform.position;
			pos4 = localToWorld.MultiplyVector( pos4 ) + this.transform.position;

			float[] y = new float[4];
			RaycastHit hitInfo1;
			if( !Physics.Raycast( pos1, Vector3.down, out hitInfo1, halfHeight + maxDeviation, groundMask ) )
			{
				return false;
			}
			y[0] = hitInfo1.point.y;
			if( !Physics.Raycast( pos2, Vector3.down, out hitInfo1, halfHeight + maxDeviation, groundMask ) )
			{
				return false;
			}
			y[1] = hitInfo1.point.y;
			if( !Physics.Raycast( pos3, Vector3.down, out hitInfo1, halfHeight + maxDeviation, groundMask ) )
			{
				return false;
			}
			y[2] = hitInfo1.point.y;
			if( !Physics.Raycast( pos4, Vector3.down, out hitInfo1, halfHeight + maxDeviation, groundMask ) )
			{
				return false;
			}
			y[3] = hitInfo1.point.y;

			Array.Sort( y );

			return Mathf.Abs( y[0] - y[3] ) < 0.2f; // max 0.2 of variation.
		}

		private void Update()
		{
			if( CanBePlacedHere() )
			{
				this.GetComponent<MeshRenderer>().material.SetColor( "_FactionColor", Color.green );

				if( Input.GetKeyDown( KeyCode.I ) )
				{
					Building.Create( this.def, this.transform.position, this.transform.rotation, 0, true );
					Destroy( this.gameObject );
				}
			}
			else
			{
				this.GetComponent<MeshRenderer>().material.SetColor( "_FactionColor", Color.red );
			}
		}
	}
}