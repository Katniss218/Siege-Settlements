using SS.Diplomacy;
using SS.InputSystem;
using UnityEngine;

namespace SS.Objects.Buildings
{
	/// <summary>
	/// Component used to position the build preview in the scene.
	/// </summary>
	public class BuildPreviewPositioner : MonoBehaviour
	{
		private const float KBD_ROTATION_SPEED = 45.0f;

		public Vector3[] placementNodes;
		public Vector3 nodesSearchRange;

		private void RotateClockwise()
		{
			this.transform.Rotate( 0, KBD_ROTATION_SPEED * Time.deltaTime, 0 );
		}

		private void RotateCounterClockwise()
		{
			this.transform.Rotate( 0, -KBD_ROTATION_SPEED * Time.deltaTime, 0 );
		}

		private void SnapToGround()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( new Ray( this.transform.position + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down ), out hitInfo, float.MaxValue, ObjectLayer.TERRAIN_MASK ) )
			{
				this.transform.position = hitInfo.point;
			}
		}

		private void MoveToPointer()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, float.MaxValue, ObjectLayer.TERRAIN_MASK ) )
			{
				this.transform.position = hitInfo.point;
			}
		}


		private void Inp_RotateCCW( InputQueue self )
		{
			this.RotateCounterClockwise();
			self.StopExecution();
		}

		private void Inp_RotateCW( InputQueue self )
		{
			this.RotateClockwise();
			self.StopExecution();
		}


		void OnEnable()
		{
			Main.keyboardInput.RegisterOnHold( KeyCode.Q, 40.0f, this.Inp_RotateCCW, true );
			Main.keyboardInput.RegisterOnHold( KeyCode.E, 40.0f, this.Inp_RotateCW, true );
		}

		void OnDisable()
		{
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.ClearOnHold( KeyCode.Q, this.Inp_RotateCCW );
				Main.keyboardInput.ClearOnHold( KeyCode.E, this.Inp_RotateCW );
			}
		}

		private void Update()
		{
			if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
			{
				// Disappear the pointer.
				this.transform.position = new Vector3( -9999.0f, -9999.0f, 3000.0f );
				return;
			}




			// Position the preview at the mouse's position. Hide the preview if the raycast misses.
			this.MoveToPointer();

			// If not pressing SHIFT.
			// Snap the preview to placement nodes.
			if( !(Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) )
			{
				Vector3 cursorOnTerrain = this.transform.position; // since we are moving the obj to the cursor's position on the terrain.

				Collider[] colliders = Physics.OverlapBox( this.transform.position + new Vector3( 0, this.nodesSearchRange.y * 0.5f, 0 ), this.nodesSearchRange, this.transform.rotation, ObjectLayer.OBJECTS_MASK );

				float nodePairMinDst = float.MaxValue;
				Vector3? nodeSelf = null; // if null, no nodes found.
				Vector3? nodeTarget = null; // if null, no nodes found.

				float searchlength = this.nodesSearchRange.magnitude;

				for( int i = 0; i < colliders.Length; i++ )
				{
					Building building = colliders[i].GetComponent<Building>();

					if( building == null )
					{
						continue;
					}

					// Disable snapping to other factions' buildings
					if( building.factionId != Levels.LevelDataManager.PLAYER_FAC )
					{
						continue;
					}

					Vector3[] targetNodes = building.placementNodes;

					for( int j = 0; j < this.placementNodes.Length; j++ )
					{
						for( int k = 0; k < targetNodes.Length; k++ )
						{
							Matrix4x4 toWorldSelf = this.transform.localToWorldMatrix;
							Vector3 globalSelf = toWorldSelf.MultiplyVector( this.placementNodes[j] ) + this.transform.position;

							Matrix4x4 toWorldTarget = colliders[i].transform.localToWorldMatrix;
							Vector3 globalTarget = toWorldTarget.MultiplyVector( targetNodes[k] ) + colliders[i].transform.position;

							float dst = Vector3.Distance( globalSelf, globalTarget );
							if( dst <= searchlength && dst < nodePairMinDst )
							{
								nodeSelf = globalSelf;
								nodeTarget = globalTarget;
								nodePairMinDst = dst;
							}
						}
					}
				}

				if( nodeTarget.HasValue )
				{
					// Move the preview to overlap the 2 nodes.
					Vector3 diff = nodeTarget.Value - nodeSelf.Value;

					this.transform.Translate( diff, Space.World );

					// rotate the preview to face the mouse.
					Vector3 dirNodeToCursor = cursorOnTerrain - nodeTarget.Value;
					dirNodeToCursor.y = 0;
					Vector3 dirNodeToSelf = this.transform.position - nodeTarget.Value;
					dirNodeToSelf.y = 0;

					Quaternion rot = Quaternion.FromToRotation( dirNodeToSelf.normalized, dirNodeToCursor.normalized );
					this.transform.RotateAround( nodeTarget.Value, Vector3.up, rot.eulerAngles.y );
				}

				// snap the preview to ground.
				this.SnapToGround();
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.gray;

			Matrix4x4 toWorld = this.transform.localToWorldMatrix;
			for( int i = 0; i < this.placementNodes.Length; i++ )
			{
				Gizmos.DrawSphere( toWorld.MultiplyVector( this.placementNodes[i] ) + this.transform.position, 0.05f );
			}
		}
#endif
	}
}