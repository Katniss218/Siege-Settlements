using System;
using TMPro;
using UnityEngine;
using SS.Levels;
using SS.Units;

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

		private static Sprite __tooltipBackground = null;
		public static Sprite toolTipBackground
		{
			get
			{
				if( __tooltipBackground == null )
				{
					__tooltipBackground = FindObjectOfType<Main>().__tooltipBackgroundSprite;
				}
				return __tooltipBackground;
			}
		}

		private static Shader __factionColorShader = null;
		public static Shader factionColorShader
		{
			get
			{
				if( __factionColorShader == null )
				{
					__factionColorShader = Resources.Load<Shader>( "Shaders/FactionColor" );
				}
				return __factionColorShader;
			}
		}

		private static Shader __unitShader = null;
		public static Shader unitShader
		{
			get
			{
				if( __unitShader == null )
				{
					__unitShader = Resources.Load<Shader>( "Shaders/GenericUnit" );
				}
				return __unitShader;
			}
		}

		private static Shader __resourceDepositShader = null;
		public static Shader resourceDepositShader
		{
			get
			{
				if( __resourceDepositShader == null )
				{
					__resourceDepositShader = Resources.Load<Shader>( "Shaders/ResourceDeposit" );
				}
				return __resourceDepositShader;
			}
		}

		private static Material __particleMaterial = null;
		public static Material particleMaterial
		{
			get
			{
				if( __particleMaterial == null )
				{
					__particleMaterial = Resources.Load<Material>( "ParticleMaterial" );
				}
				return __particleMaterial;
			}
		}

		public static AudioClip hit, loose, hitmelee;

		new public static Camera camera { get; private set; }
		[SerializeField] private Camera cam = null;

		private static TMP_FontAsset __mainFont = null;
		public static TMP_FontAsset mainFont
		{
			get
			{
				if( __mainFont == null )
				{
					__mainFont = Resources.Load<TMP_FontAsset>( "Chomsky SDF" );
				}
				return __mainFont;
			}
		}

		public static Main instance { get; private set; }

		[SerializeField] Sprite __tooltipBackgroundSprite = null;

		private void Awake()
		{
			if( instance != null )
			{
				throw new Exception( "Found 2 or more 'Main' scripts at one time." );
			}
			instance = this;

			camera = this.cam;
		}


		void Start()
		{
			LevelManager.PostInitLoad();

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