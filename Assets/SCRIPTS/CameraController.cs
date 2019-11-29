using SS.InputSystem;
using SS.Levels;
using UnityEngine;
using UnityEngine.EventSystems;

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

		private bool isVertical = false;

		private int __size;
		public int size
		{
			get
			{
				return this.__size;
			}
			set
			{
				this.__size = value;
				this.camera.orthographicSize = value;
			}
		}

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

			this.isVertical = !false;
			this.ToggleVertical();
		}

		private void ZoomIn()
		{
			this.size = Mathf.Clamp( --size, minSize, maxSize );
		}

		private void ZoomOut()
		{
			this.size = Mathf.Clamp( ++size, minSize, maxSize );
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

			this.transform.rotation = Quaternion.Euler( defaultRotX, defaultRotY, defaultRotZ );
		}

		private const float DEFAULT_OFFSET = 20.0f;
		private const float DEFAULT_ANGLE = 22.5f;

		private void ToggleVertical()
		{
			this.isVertical = !this.isVertical;
			if( this.isVertical )
			{
				this.camera.transform.localRotation = Quaternion.Euler( 90.0f, 0.0f, 0.0f );
				this.camera.transform.localPosition = new Vector3( 0.0f, DEFAULT_OFFSET, 0.0f );
			}
			else
			{
				this.camera.transform.localRotation = Quaternion.Euler( 90.0f - DEFAULT_ANGLE, 0.0f, 0.0f );
				this.camera.transform.localPosition = Vector3.zero;
				this.camera.transform.Translate( 0.0f, 0.0f, -DEFAULT_OFFSET, Space.Self );
			}
		}

		private bool CanMove()
		{
			if( this.isMovementLocked )
			{
				return false;
			}
			if( PauseManager.isPaused )
			{
				return false;
			}
			return true;
		}

		private void Inp_MoveLeft( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Translate( -speed * Time.deltaTime * size, 0.0f, 0.0f );
		}

		private void Inp_MoveRight( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Translate( speed * Time.deltaTime * size, 0.0f, 0.0f );
		}

		private void Inp_MoveForward( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Translate( 0.0f, 0.0f, speed * Time.deltaTime * size );
		}

		private void Inp_MoveBackward( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Translate( 0.0f, 0.0f, -speed * Time.deltaTime * size );
		}

		private void Inp_RotateCCW( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Rotate( -rotSpeed * Time.deltaTime );
		}

		private void Inp_RotateCW( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			Rotate( rotSpeed * Time.deltaTime );
		}

		private void Inp_ResetCamera( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			ResetCam();
		}

		private void Inp_ToggleVertical( InputQueue self )
		{
			if( !CanMove() )
			{
				return;
			}
			this.ToggleVertical();
		}

		void OnEnable()
		{
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.RegisterOnHold( KeyCode.A, 50.0f, Inp_MoveLeft, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.D, 50.0f, Inp_MoveRight, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.W, 50.0f, Inp_MoveForward, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.S, 50.0f, Inp_MoveBackward, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.LeftArrow, 50.0f, Inp_MoveLeft, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.RightArrow, 50.0f, Inp_MoveRight, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.UpArrow, 50.0f, Inp_MoveForward, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.DownArrow, 50.0f, Inp_MoveBackward, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.Q, 50.0f, Inp_RotateCCW, true );
				Main.keyboardInput.RegisterOnHold( KeyCode.E, 50.0f, Inp_RotateCW, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Keypad5, 50.0f, Inp_ResetCamera, true );
				Main.keyboardInput.RegisterOnPress( KeyCode.Keypad0, 50.0f, Inp_ToggleVertical, true );
			}
		}

		void OnDisable()
		{
			if( Main.keyboardInput != null )
			{
				Main.keyboardInput.ClearOnHold( KeyCode.A, Inp_MoveLeft );
				Main.keyboardInput.ClearOnHold( KeyCode.D , Inp_MoveRight );
				Main.keyboardInput.ClearOnHold( KeyCode.W, Inp_MoveForward );
				Main.keyboardInput.ClearOnHold( KeyCode.S, Inp_MoveBackward );
				Main.keyboardInput.ClearOnHold( KeyCode.LeftArrow, Inp_MoveLeft );
				Main.keyboardInput.ClearOnHold( KeyCode.RightArrow, Inp_MoveRight );
				Main.keyboardInput.ClearOnHold( KeyCode.UpArrow, Inp_MoveForward );
				Main.keyboardInput.ClearOnHold( KeyCode.DownArrow, Inp_MoveBackward );
				Main.keyboardInput.ClearOnHold( KeyCode.Q, Inp_RotateCCW );
				Main.keyboardInput.ClearOnHold( KeyCode.E, Inp_RotateCW );
				Main.keyboardInput.ClearOnPress( KeyCode.Keypad5, Inp_ResetCamera );
				Main.keyboardInput.ClearOnPress( KeyCode.Keypad0, Inp_ToggleVertical );
			}
		}

		void Update()
		{
			if( !CanMove() )
			{
				return;
			}


			if( EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject() )
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

			Vector3 clampedPos = this.transform.position;
			if( clampedPos.x < 0 )
			{
				clampedPos.x = 0;
			}
			else if( clampedPos.x > LevelDataManager.mapSize )
			{
				clampedPos.x = LevelDataManager.mapSize;
			}
			
			if( clampedPos.z < 0 )
			{
				clampedPos.z = 0;
			}
			else if( clampedPos.z > LevelDataManager.mapSize )
			{
				clampedPos.z = LevelDataManager.mapSize;
			}
			this.transform.position = clampedPos;
		}
	}
}