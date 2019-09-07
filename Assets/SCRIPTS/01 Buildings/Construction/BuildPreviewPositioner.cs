using UnityEngine;

namespace SS.Buildings
{
	/// <summary>
	/// Component used to position the build preview in the scene.
	/// </summary>
	public class BuildPreviewPositioner : MonoBehaviour
	{
		private const float KBD_ROTATION_SPEED = 45.0f;

		private void RotateClockwise()
		{
			this.transform.Rotate( 0, KBD_ROTATION_SPEED * Time.fixedDeltaTime, 0 );
		}

		private void RotateCounterClockwise()
		{
			this.transform.Rotate( 0, -KBD_ROTATION_SPEED * Time.fixedDeltaTime, 0 );
		}

		private void MoveToPointer()
		{
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, float.MaxValue, 1 << LayerMask.NameToLayer( "Terrain" ) ) )
			{
				this.transform.position = hitInfo.point;
			}
		}

		private void FixedUpdate()
		{
			// Rotation.
			if( Input.GetKey( KeyCode.T ) )
			{
				this.RotateCounterClockwise();
			}
			if( Input.GetKey( KeyCode.Y ) )
			{
				this.RotateClockwise();
			}

			if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}
			// Position the preview at the mouse's position. Hide the preview if the raycast misses.
			this.MoveToPointer();
		}
	}
}