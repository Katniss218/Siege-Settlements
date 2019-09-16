using katniss.Utils;
using UnityEngine;

namespace SS.Buildings
{
	public class BuildPreview : MonoBehaviour
	{
		public static bool isActive
		{
			get
			{
				return preview != null;
			}
		}

		private static GameObject preview;



		/// <summary>
		/// The building that's being built.
		/// </summary>
		public BuildingDefinition def;

		/// <summary>
		/// The build preview will treat objects in this mask as ground.
		/// </summary>
		public LayerMask groundMask;

		/// <summary>
		/// The build preview will treat objects in this mask as blocking the placement.
		/// </summary>
		public LayerMask overlapMask;

		/// <summary>
		/// The maximum difference between any 2 points (+- maxDeviation) underneath the building preview, and the position of the building preview, that will be still considered as valid placement spot.
		/// </summary>
		public float maxDeviation = 0.2f;

		private Vector3 GetOverlapHitboxCenter()
		{
			const float epsilon = 0.01f;
			Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;

			Vector3 center = new Vector3( 0.0f, (def.size.y / 2.0f), 0.0f );
			center = localToWorldMatrix.MultiplyVector( center );
			center += this.transform.position;
			// Add maxDeviation, to make sure the overlap wouldn't block the (+) range of valid placement spots.
			center.y += maxDeviation;
			// Add epsilon to make sure the collider doesn't falsly collide at still valid placement spot.
			center.y += epsilon;

			return center;
		}

		private bool IsOverlappingObjects()
		{
			Vector3 center = GetOverlapHitboxCenter();
			if( Physics.OverlapBox( center, def.size / 2.0f, this.transform.rotation, overlapMask ).Length > 0 )
			{
				return true;
			}
			return false;
		}

		private bool IsOnFlatGround()
		{
			float halfHeight = def.size.y / 2f;

			// Setup the 4 corners for raycast.
			Vector3[] pos = new Vector3[4]
			{
				new Vector3( -def.size.x, maxDeviation, -def.size.z ),
				new Vector3( -def.size.x, maxDeviation, def.size.z ),
				new Vector3( def.size.x, maxDeviation, -def.size.z ),
				new Vector3( def.size.x, maxDeviation, def.size.z )
			};

			// Setup the outputs for the 4 corners.
			float[] y = new float[4];

			// Check the 4 corners (do the raycast).
			Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
			RaycastHit hitInfo;
			for( int i = 0; i < 4; i++ )
			{
				pos[i] = localToWorldMatrix.MultiplyVector( pos[i] ) + this.transform.position;
				if( !Physics.Raycast( pos[i], Vector3.down, out hitInfo, 2.0f * maxDeviation, groundMask ) )
				{
					// If the raycast missed, that means there is a deep chasm at the checked position.
					return false;
				}
				y[i] = hitInfo.point.y;
			}

			// Check if the y-positions of corners are within the allowed deviation from the building's y-position.
			for( int i = 0; i < 4; i++ )
			{
				// the transform's position is always at the bottom of a building.
				if( y[i] < this.transform.position.y - maxDeviation ) { return false; }
				if( y[i] > this.transform.position.y + maxDeviation ) { return false; }
			}

			return true;
		}

		/// <summary>
		/// Checks if the building preview's current position is a valid placement spot.
		/// </summary>
		public bool CanBePlacedHere()
		{
			return !IsOverlappingObjects() && IsOnFlatGround();
		}

		void Start()
		{
			// hook to the events to hide the preview.
		}

		void Update()
		{
			if( Input.GetMouseButtonDown( 1 ) ) // right mouse button
			{
				Destroy( this.gameObject );
				return;
			}
			if( CanBePlacedHere() )
			{
				this.GetComponent<MeshRenderer>().material.SetColor( "_FactionColor", Color.green );

				if( Input.GetMouseButtonDown( 0 ) ) // left mouse button
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
		
		public static GameObject Create( BuildingDefinition def )
		{
			GameObject gameObject = new GameObject();
			BuildPreview buildPreview = gameObject.AddComponent<BuildPreview>();
			BuildPreviewPositioner positioner = gameObject.AddComponent<BuildPreviewPositioner>();
			positioner.placementNodes = def.placementNodes;

			float max = Mathf.Max( def.size.x, def.size.y, def.size.z );
			positioner.nodesSearchRange = new Vector3( max, max, max );

			buildPreview.def = def;

			buildPreview.groundMask = 1 << LayerMask.NameToLayer( "Terrain" );

			buildPreview.overlapMask =
				1 << LayerMask.NameToLayer( "Terrain" ) |
				1 << LayerMask.NameToLayer( "Units" ) |
				1 << LayerMask.NameToLayer( "Buildings" ) |
				1 << LayerMask.NameToLayer( "Heroes" ) |
				1 << LayerMask.NameToLayer( "Extras" );

			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.mesh = buildPreview.def.mesh.Item2;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = Main.materialFactionColored;

			meshRenderer.material.SetTexture( "_Albedo", Texture2DUtils.CreateBlank() );
			meshRenderer.material.SetTexture( "_Normal", null );
			meshRenderer.material.SetTexture( "_Emission", null );

			preview = gameObject;

			return gameObject;
		}

		public static void Destroy()
		{
			if( preview != null )
			{
				Destroy( preview );
			}
		}
	}
}