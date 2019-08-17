using UnityEngine;

namespace SS
{
	public class CameraController : MonoBehaviour
	{
		private int size;

		[SerializeField] private int defaultSize = 10;

		[SerializeField] private int minSize = 5;
		[SerializeField] private int maxSize = 30;

		[SerializeField] private float scrollMargin = 80.0f;

		new private Camera camera;


		[SerializeField] private float speed = 1.0f;
		[SerializeField] private float rotSpeed = 60.0f;

		void Awake()
		{
			this.camera = this.transform.GetChild( 0 ).GetComponent<Camera>();
		}

		void Start()
		{
			this.size = defaultSize;
			camera.orthographicSize = this.size;
		}

		private void ZoomIn()
		{
			size = Mathf.Clamp( --size, minSize, maxSize );
			camera.orthographicSize = this.size;
		}

		private void ZoomOut()
		{
			size = Mathf.Clamp( ++size, minSize, maxSize );
			camera.orthographicSize = this.size;
		}

		private void Rotate( float amount )
		{
			this.transform.Rotate( 0.0f, amount, 0.0f );
		}

		private void Translate( float x, float y, float z )
		{
			this.transform.Translate( x, y, z, Space.Self );
		}

		private void ResetCam()
		{
			size = defaultSize;
			camera.orthographicSize = this.size;
			this.transform.rotation = Quaternion.Euler( 0, 45, 0 );
		}

		void Update()
		{
			// Zoom
			if( Input.mouseScrollDelta.y < 0 )
			{
				ZoomOut();
			}
			else if( Input.mouseScrollDelta.y > 0 )
			{
				ZoomIn();
			}

			// Rotate CCW-CW
			if( Input.GetKey( KeyCode.Q ) )
			{
				Rotate( -rotSpeed * Time.deltaTime );
			}
			else if( Input.GetKey( KeyCode.E ) )
			{
				Rotate( rotSpeed * Time.deltaTime );
			}

			// Move Left-Right
			if( Input.GetKey( KeyCode.A ) || Input.mousePosition.x < scrollMargin )
			{
				Translate( -speed * Time.deltaTime * size, 0.0f, 0.0f );
			}
			else if( Input.GetKey( KeyCode.D ) || Input.mousePosition.x > Screen.currentResolution.width - scrollMargin )
			{
				Translate( speed * Time.deltaTime * size, 0.0f, 0.0f );
			}

			// Move Up-Down
			if( Input.GetKey( KeyCode.W ) || Input.mousePosition.y > Screen.currentResolution.height - scrollMargin )
			{
				Translate( 0.0f, 0.0f, speed * Time.deltaTime * size );
			}
			else if( Input.GetKey( KeyCode.S ) || Input.mousePosition.y < scrollMargin )
			{
				Translate( 0.0f, 0.0f, -speed * Time.deltaTime * size );
			}

			// Reset
			if( Input.GetKey( KeyCode.Keypad5 ) )
			{
				ResetCam();
			}
		}
	}
}