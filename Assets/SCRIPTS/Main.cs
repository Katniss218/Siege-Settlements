using SS.Projectiles;
using SS.Units;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using Katniss.Utils;
using SS.Levels;
using KFF;

namespace SS
{
	public class Main : MonoBehaviour
	{
		public static Color darkText
		{
			get { return new Color( 0.1f, 0.1f, 0.1f ); }
		}
		
		private static Canvas __canvas = null;
		public static Canvas canvas
		{
			get
			{
				if( __canvas == null )
				{
					__canvas = FindObjectOfType<Main>().transform.Find( "Canvas" ).GetComponent<Canvas>();
				}
				return __canvas;
			}
		}

		public static Sprite toolTipBackground
		{
			get
			{
				return FindObjectOfType<Main>().__toolTipBackground;
			}
		}

		public Shader factionColorShader;
		public Shader unitShader;
		public Shader resourceDepositShader;
		public Material particleMaterial;

		public Texture2D particleTex;

		public AudioClip hit, loose, hitmelee;

		public Texture2D unitNormal;

		new public static Camera camera = null;
		[SerializeField] private Camera cam = null;

		[SerializeField] private ResourcePanel resourcePanel = null;

		public static TMP_FontAsset __mainFont = null;
		public static TMP_FontAsset mainFont
		{
			get
			{
				if( __mainFont == null )
				{
					__mainFont = FindObjectOfType<Main>().interface_mainFont;
				}
				return __mainFont;
			}
		}

		public static Main instance { get; private set; }

		[SerializeField] Sprite __toolTipBackground = null;

		[SerializeField] TMP_FontAsset interface_mainFont = null;

		public AudioSource source;
		public Sprite b;
		public Sprite money;
		public Sprite wood;
		public Sprite stone;
		public Sprite iron;
		public Sprite sulphur;

		private void Awake()
		{
			if( instance != null )
			{
				throw new Exception( "Found 2 or more 'Main' scripts at one time." );
			}
			instance = this;

			camera = this.cam;

			if( !Directory.Exists( DataManager.dirPath ) )
				Directory.CreateDirectory( DataManager.dirPath );
			if( !Directory.Exists( AssetsManager.dirPath ) )
				Directory.CreateDirectory( AssetsManager.dirPath );
		}


		void Start()
		{
			LevelManager.PostInitLoad();
			
			source = this.gameObject.AddComponent<AudioSource>();
			hit = AssetsManager.getAudioClip( "Sounds/roar.wav" );
			loose = AssetsManager.getAudioClip( "Sounds/loose.wav" );
			hitmelee = AssetsManager.getAudioClip( "Sounds/melee.wav" );

		}

		void Update()
		{
			if( Input.GetMouseButtonDown( 1 ) )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					foreach( ISelectable obj in SelectionManager.selected )
					{
						if( obj == null )
						{
							continue;
						}
						if( obj is Unit )
						{
							((Unit)obj).SetDestination( hitInfo.point );
						}
					}
				}
			}
		}
	}
}