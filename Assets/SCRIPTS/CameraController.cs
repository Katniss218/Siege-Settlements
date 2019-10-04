using UnityEngine;

namespace SS
{
	/// <summary>
	/// Controls the camera, via user input.
	/// </summary>
	public class CameraController : MonoBehaviour
	{
		public const float defaultRotX = 0.0f;
		public const float defaultRotY = 45.0f;
		public const float defaultRotZ = 0.0f;

		public static CameraController instance { get; private set; }

		[SerializeField] private int defaultSize = 10;

		[SerializeField] private int minSize = 5;
		[SerializeField] private int maxSize = 30;

		[SerializeField] private float speed = 1.0f;
		[SerializeField] private float rotSpeed = 60.0f;

		[SerializeField] private float scrollMargin = 80.0f;

		private int size;

		new public Camera camera { get; private set; }

		public bool isMovementLocked { get; set; }

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There was more than 1 CameraController." );
			}
			instance = this;

			this.camera = this.transform.GetChild( 0 ).GetComponent<Camera>();
		}

		void Start()
		{
			this.size = defaultSize;
			this.camera.orthographicSize = this.size;
		}

		private void ZoomIn()
		{
			this.size = Mathf.Clamp( --size, minSize, maxSize );
			this.camera.orthographicSize = this.size;
		}

		private void ZoomOut()
		{
			this.size = Mathf.Clamp( ++size, minSize, maxSize );
			this.camera.orthographicSize = this.size;
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
			this.size = defaultSize;
			this.camera.orthographicSize = this.size;
			this.transform.rotation = Quaternion.Euler( defaultRotX, defaultRotY, defaultRotZ );
		}

		void Update()
		{
			if( this.isMovementLocked )
			{
				return;
			}
			if( PauseManager.isPaused )
			{
				return;
			}
			// Move Left-Right
			if( Input.GetKey( KeyCode.A ) )
			{
				Translate( -speed * Time.deltaTime * size, 0.0f, 0.0f );
			}
			else if( Input.GetKey( KeyCode.D ) )
			{
				Translate( speed * Time.deltaTime * size, 0.0f, 0.0f );
			}

			// Move Up-Down
			if( Input.GetKey( KeyCode.W ) )
			{
				Translate( 0.0f, 0.0f, speed * Time.deltaTime * size );
			}
			else if( Input.GetKey( KeyCode.S ) )
			{
				Translate( 0.0f, 0.0f, -speed * Time.deltaTime * size );
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


			if( UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
			{
				return;
			}

			// Zoom
			if( Input.mouseScrollDelta.y < 0 )
			{
				ZoomOut();
			}
			else if( Input.mouseScrollDelta.y > 0 )
			{
				ZoomIn();
			}

			if( Input.mousePosition.x < scrollMargin )
			{
				Translate( -speed * Time.deltaTime * size, 0.0f, 0.0f );
			}
			else if( Input.mousePosition.x > Screen.currentResolution.width - scrollMargin )
			{
				Translate( speed * Time.deltaTime * size, 0.0f, 0.0f );
			}
			if( Input.mousePosition.y > Screen.currentResolution.height - scrollMargin )
			{
				Translate( 0.0f, 0.0f, speed * Time.deltaTime * size );
			}
			else if( Input.mousePosition.y < scrollMargin )
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