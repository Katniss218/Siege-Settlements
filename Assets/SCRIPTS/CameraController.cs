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

		void Update()
		{
			if( Input.mouseScrollDelta.y < 0 )
			{
				size = Mathf.Clamp( ++size, minSize, maxSize );
				camera.orthographicSize = this.size;

			}
			else if( Input.mouseScrollDelta.y > 0 )
			{
				size = Mathf.Clamp( --size, minSize, maxSize );
				camera.orthographicSize = this.size;

			}
			if( Input.GetKey( KeyCode.Q ) )
			{
				this.transform.Rotate( 0, -rotSpeed * Time.deltaTime, 0 );
			}
			if( Input.GetKey( KeyCode.E ) )
			{
				this.transform.Rotate( 0, rotSpeed * Time.deltaTime, 0 );
			}
			if( Input.GetKey( KeyCode.A ) || Input.mousePosition.x < scrollMargin )
			{
				this.transform.Translate( -speed * Time.deltaTime * size, 0.0f, 0.0f, Space.Self );
			}
			else if( Input.GetKey( KeyCode.D ) || Input.mousePosition.x > Screen.currentResolution.width - scrollMargin )
			{
				this.transform.Translate( speed * Time.deltaTime * size, 0.0f, 0.0f, Space.Self );
			}
			if( Input.GetKey( KeyCode.W ) || Input.mousePosition.y > Screen.currentResolution.height - scrollMargin )
			{
				this.transform.Translate( 0.0f, 0.0f, speed * Time.deltaTime * size, Space.Self );
			}
			else if( Input.GetKey( KeyCode.S ) || Input.mousePosition.y < scrollMargin )
			{
				this.transform.Translate( 0.0f, 0.0f, -speed * Time.deltaTime * size, Space.Self );
			}
			else if( Input.GetKey( KeyCode.Keypad5 ) )
			{
				size = defaultSize;
				camera.orthographicSize = this.size;
				this.transform.rotation = Quaternion.Euler( 0, 45, 0 );
			}
		}
	}
}