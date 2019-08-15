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
			// Check for the overlap.
			Matrix4x4 localToWorld = this.transform.localToWorldMatrix;
			Vector3 center = new Vector3( 0f, def.size.y / 2f, 0f );
			center = localToWorld.MultiplyVector( center ) + this.transform.position + new Vector3( 0, maxDeviation + 0.01f, 0 ); // add 0.01 so the collider is slightly above the ground and collision doesn't pick it up when it shouldn't.
			if( Physics.OverlapBox( center, def.size / 2, this.transform.rotation ).Length > 0 )
			{
				return false;
			}
			float halfHeight = def.size.y / 2f;

			// Check for the slope gradient.
			Vector3[] pos = new Vector3[4]
			{
				new Vector3( -def.size.x, halfHeight, -def.size.z ),
				new Vector3( -def.size.x, halfHeight, def.size.z ),
				new Vector3( def.size.x, halfHeight, -def.size.z ),
				new Vector3( def.size.x, halfHeight, def.size.z )
			};
			
			float[] y = new float[4];
			RaycastHit hitInfo;
			for( int i = 0; i < 4; i++ )
			{
				pos[i] = localToWorld.MultiplyVector( pos[i] ) + this.transform.position;
				if( !Physics.Raycast( pos[i], Vector3.down, out hitInfo, halfHeight + maxDeviation, groundMask ) )
				{
					return false;
				}
				y[i] = hitInfo.point.y;
			}
			
			Array.Sort( y );

			return Mathf.Abs( y[0] - y[3] ) < maxDeviation; // max 0.2 of variation.
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