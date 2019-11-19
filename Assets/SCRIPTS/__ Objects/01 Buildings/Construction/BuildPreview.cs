using SS.InputSystem;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Buildings
{
	public class BuildPreview : MonoBehaviour
	{
		public static bool isActive
		{
			get
			{
				return buildPreviewInstanceGameObject != null;
			}
		}

		private static GameObject buildPreviewInstanceGameObject;



		/// <summary>
		/// The building that's being built.
		/// </summary>
		public BuildingDefinition def;

		/// <summary>
		/// The build preview will treat objects in this mask as ground terrain.
		/// </summary>
		public LayerMask terrainMask;

		/// <summary>
		/// The build preview will treat objects in this mask as blocking the placement.
		/// </summary>
		public LayerMask objectsMask;

		/// <summary>
		/// The maximum difference between any 2 points (+- maxDeviation) underneath the building preview, and the position of the building preview, that will be still considered as valid placement spot.
		/// </summary>
		public float maxDeviationY = 0.2f;


		private Vector3 GetOverlapHitboxCenter( bool OffsetByDeviation )
		{
			const float epsilon = 0.01f;
			Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;

			Vector3 center = new Vector3( 0.0f, (this.def.size.y / 2.0f), 0.0f );

			if( OffsetByDeviation )
			{
				// Add maxDeviation, to make sure the overlap wouldn't block the (+) range of valid placement spots.
				center.y += this.maxDeviationY / 2.0f; // add deviation in local space.
			}

			center = localToWorldMatrix.MultiplyVector( center );
			center += this.transform.position;

			// Add epsilon to make sure the collider doesn't falsly collide with the ground at otherwise valid placement spot.
			center.y += epsilon;

			return center;
		}

		private bool IsOverlapping()
		{
			// Test the ground overlapping (only if the .
			Vector3 center;

			if( this.def.size.y > this.maxDeviationY )
			{
				// Subtract the max deviation from the height of the box.
				Vector3 size = this.def.size - new Vector3( 0.0f, this.maxDeviationY, 0.0f );

				center = this.GetOverlapHitboxCenter( true );

				if( Physics.OverlapBox( center, (size / 2.0f), this.transform.rotation, this.terrainMask ).Length > 0 )
				{
					return true;
				}
			}

			// Test objects overlapping with the object
			center = this.GetOverlapHitboxCenter( false );
			if( Physics.OverlapBox( center, (this.def.size / 2.0f), this.transform.rotation, this.objectsMask ).Length > 0 )
			{
				return true;
			}

			return false;
		}

		private bool IsOnFlatGround()
		{
			Vector3 halfSize = this.def.size * 0.5f;

			// Setup the 4 corners for raycast.
			Vector3[] pos = new Vector3[4]
			{
				new Vector3( -halfSize.x, this.maxDeviationY, -halfSize.z ),
				new Vector3( -halfSize.x, this.maxDeviationY, halfSize.z ),
				new Vector3( halfSize.x, this.maxDeviationY, -halfSize.z ),
				new Vector3( halfSize.x, this.maxDeviationY, halfSize.z )
			};

			// Setup the outputs for the 4 corners.
			float[] y = new float[4];

			// Check the 4 corners (do the raycast).
			Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
			RaycastHit hitInfo;
			for( int i = 0; i < 4; i++ )
			{
				pos[i] = localToWorldMatrix.MultiplyVector( pos[i] ) + this.transform.position;
				if( !Physics.Raycast( pos[i], Vector3.down, out hitInfo, 2.0f * this.maxDeviationY, this.terrainMask ) )
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
				if( y[i] < this.transform.position.y - this.maxDeviationY ) { return false; }
				if( y[i] > this.transform.position.y + this.maxDeviationY ) { return false; }
			}

			return true;
		}

		/// <summary>
		/// Checks if the building preview's current position is a valid placement spot.
		/// </summary>
		public bool CanBePlacedHere()
		{
			return !this.IsOverlapping() && this.IsOnFlatGround();
		}

		void Start()
		{
			// hook to the events to hide the preview.
		}

		private void Inp_CancelPlacement( InputQueue self )
		{
			Object.Destroy( this.gameObject );
			self.StopExecution();
		}

		private void Inp_TryPlace( InputQueue self )
		{
			if( this.CanBePlacedHere() )
			{
				BuildingData data = new BuildingData();
				data.guid = Guid.NewGuid();
				data.position = this.transform.position;
				data.rotation = this.transform.rotation;
				data.factionId = LevelDataManager.PLAYER_FAC;
				data.health = this.def.healthMax * Building.STARTING_HEALTH_PERCENT;
				data.constructionSaveState = new ConstructionSiteData();
				BuildingCreator.Create( this.def, data );

				Object.Destroy( this.gameObject );
			}
			self.StopExecution();
		}

		private void Inp_BlockSelectionOverride( InputQueue self )
		{
			// Used just to stop execution (is inserted before selection queue, so it blocks deselecting/selecting, when preview is active).
			self.StopExecution();
		}

		private void OnEnable()
		{
			// Need to override all 3 channels of mouse input, since selection uses all 3 of them (so all 3 need to be blocked to block selection).
			Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true ); // left
			Main.mouseInput.RegisterOnHold( MouseCode.RightMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.RightMouseButton, 10.0f, this.Inp_CancelPlacement, true );

			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true ); // right
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 10.0f, this.Inp_TryPlace, true );
		}

		private void OnDisable()
		{
			if( Main.mouseInput != null )
			{
				// The action is assigned to on release to block selection controller deselecting the object when placed
				//    (building get's placed on press, then preview removes itself from the input, and later on release it deselects).
				Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, this.Inp_BlockSelectionOverride ); // left
				Main.mouseInput.ClearOnHold( MouseCode.RightMouseButton, this.Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.RightMouseButton, this.Inp_CancelPlacement );

				Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, this.Inp_BlockSelectionOverride ); // right
				Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, this.Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, this.Inp_TryPlace );
			}
		}

		void Update()
		{
			MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();

			if( this.CanBePlacedHere() )
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetColor( "_FactionColor", Color.green );
				}
			}
			else
			{
				for( int i = 0; i < renderers.Length; i++ )
				{
					renderers[i].material.SetColor( "_FactionColor", Color.red );
				}
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

			buildPreview.terrainMask = ObjectLayer.TERRAIN_MASK;

			buildPreview.objectsMask = ObjectLayer.OBJECTS_MASK;

			SubObjectDefinition[] subObjectDefs;
			def.GetAllSubObjects( out subObjectDefs );

			for( int i = 0; i < subObjectDefs.Length; i++ )
			{
				if( subObjectDefs[i] is MeshSubObjectDefinition )
				{
					MeshSubObjectDefinition meshSubObjDef = ( MeshSubObjectDefinition)subObjectDefs[i];

					GameObject child = new GameObject( "Prev-sub" );

					child.transform.SetParent( gameObject.transform );
					child.transform.localPosition = meshSubObjDef.localPosition;
					child.transform.localRotation = meshSubObjDef.localRotation;

					MeshFilter meshFilter = child.AddComponent<MeshFilter>();
					meshFilter.mesh = meshSubObjDef.mesh;
					MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
					meshRenderer.material = MaterialManager.CreatePlacementPreview( new Color( 1, 0, 1 ) );
				}
			}
			
			buildPreviewInstanceGameObject = gameObject;

			return gameObject;
		}

		public static void Switch( BuildingDefinition def )
		{
			BuildPreview buildPreview = buildPreviewInstanceGameObject.GetComponent<BuildPreview>();
			BuildPreviewPositioner positioner = buildPreviewInstanceGameObject.GetComponent<BuildPreviewPositioner>();
			positioner.placementNodes = def.placementNodes;

			float max = Mathf.Max( def.size.x, def.size.y, def.size.z );
			positioner.nodesSearchRange = new Vector3( max, max, max );

			buildPreview.def = def;

			for( int i = 0; i < buildPreviewInstanceGameObject.transform.childCount; i++ )
			{
				Object.Destroy( buildPreviewInstanceGameObject.transform.GetChild( i ).gameObject );
			}

			SubObjectDefinition[] subObjectDefs;
			def.GetAllSubObjects( out subObjectDefs );

			for( int i = 0; i < subObjectDefs.Length; i++ )
			{
				if( subObjectDefs[i] is MeshSubObjectDefinition )
				{
					MeshSubObjectDefinition meshSubObjDef = (MeshSubObjectDefinition)subObjectDefs[i];

					GameObject child = new GameObject( "Prev-sub" );

					child.transform.SetParent( buildPreviewInstanceGameObject.transform );
					child.transform.localPosition = meshSubObjDef.localPosition;
					child.transform.localRotation = meshSubObjDef.localRotation;

					MeshFilter meshFilter = child.AddComponent<MeshFilter>();
					meshFilter.mesh = meshSubObjDef.mesh;
					MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
					meshRenderer.material = MaterialManager.CreatePlacementPreview( new Color( 1, 0, 1 ) );
				}
			}
		}

		public static void Destroy()
		{
			if( buildPreviewInstanceGameObject != null )
			{
				Object.Destroy( buildPreviewInstanceGameObject );
			}
		}
	}
}