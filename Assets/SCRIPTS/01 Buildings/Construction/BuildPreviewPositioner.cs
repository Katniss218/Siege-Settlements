using UnityEngine;

namespace SS.Buildings
{
	public class BuildPreviewPositioner : MonoBehaviour
	{
		private void FixedUpdate()
		{
			if( Input.GetKey( KeyCode.T ) )
			{
				this.transform.Rotate( 0, -45f * Time.fixedDeltaTime, 0 );
			}
			if( Input.GetKey( KeyCode.Y ) )
			{
				this.transform.Rotate( 0, 45f * Time.fixedDeltaTime, 0 );
			}
			RaycastHit hitInfo;
			if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo, float.MaxValue, 1 << LayerMask.NameToLayer( "Terrain" ) ) )
			{
				this.transform.position = hitInfo.point;
			}
		}
	}
}