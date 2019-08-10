using SS.Projectiles;
using SS.Units;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using Katniss.Utils;

namespace SS
{
	public class Main : MonoBehaviour
	{
		public static Color darkText
		{
			get { return new Color( 0.1f, 0.1f, 0.1f ); }
		}
		// gamedata
		// - data
		// - assets
		//    here the path for stuff is.

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

		public AudioClip hit, loose;

		public Texture2D unitNormal;

		//[SerializeField] private Camera cam = null;

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

			if( !Directory.Exists( DataManager.dirPath ) )
				Directory.CreateDirectory( DataManager.dirPath );
			if( !Directory.Exists( AssetsManager.dirPath ) )
				Directory.CreateDirectory( AssetsManager.dirPath );

			DataManager.LoadDefaults();
			AssetsManager.LoadDefaults();

			//Mesh m = new Mesh();
			//m.vertices = new Vector3[] { Vector3.one };
		}


		void Start()
		{
			UnitDefinition defx = DataManager.FindDefinition<UnitDefinition>( "unit.heavy_cavalry" );
			for( int i = 0; i < 10; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 1 );
			}
			for( int i = 0; i < 10; i++ )
			{
				Unit.Create( defx, new Vector3( UnityEngine.Random.Range( -10f, 10f ), 0, UnityEngine.Random.Range( -10f, 10f ) ), 0 );
			}
			ResourceDepositDefinition def = DataManager.FindDefinition<ResourceDepositDefinition>( "resource_deposit.tree" );
			for( int i = 0; i < 75; i++ )
			{
				//float s = UnityEngine.Random.Range( 0.75f, 1.25f );

				float x = UnityEngine.Random.Range( -10f, 10f );
				float z = UnityEngine.Random.Range( -10f, 10f );

				if( Physics.Raycast( new Vector3( x, 50, z ), Vector3.down, out RaycastHit hit, 100 ) )
				{
					ResourceDeposit.Create( def, hit.point, Quaternion.Euler( 0f, UnityEngine.Random.Range( -180f, 180f ), 0f ) );
				}
			}
			def = DataManager.FindDefinition<ResourceDepositDefinition>( "resource_deposit.pine" );
			for( int i = 0; i < 50; i++ )
			{
				//float s = UnityEngine.Random.Range( 0.75f, 1.25f );

				float x = UnityEngine.Random.Range( -10f, 10f );
				float z = UnityEngine.Random.Range( -10f, 10f );

				if( Physics.Raycast( new Vector3( x, 50, z ), Vector3.down, out RaycastHit hit, 100 ) )
				{
					ResourceDeposit.Create( def, hit.point, Quaternion.Euler( 0f, UnityEngine.Random.Range( -180f, 180f ), 0f ) );
				}
			}
			ProjectileDefinition def2 = DataManager.FindDefinition<ProjectileDefinition>( "projectile.arrow" );
			Projectile.Create( def2, new Vector3( 0, 2, 0 ), new Vector3( 3, 3, 3 ), 0, 0f, null );

			ResourceDefinition _money = DataManager.FindDefinition<ResourceDefinition>( "resource.money" );
			ResourceDefinition _wood = DataManager.FindDefinition<ResourceDefinition>( "resource.wood" );
			ResourceDefinition _stone = DataManager.FindDefinition<ResourceDefinition>( "resource.stone" );
			ResourceDefinition _iron = DataManager.FindDefinition<ResourceDefinition>( "resource.iron" );
			ResourceDefinition _sulphur = DataManager.FindDefinition<ResourceDefinition>( "resource.sulphur" );

			resourcePanel.AddResourceEntry( _money, 700 );
			resourcePanel.AddResourceEntry( _wood, 400 );
			resourcePanel.AddResourceEntry( _stone, 600 );
			resourcePanel.AddResourceEntry( _iron, 100 );
			resourcePanel.AddResourceEntry( _sulphur, 0 );

			source = this.gameObject.AddComponent<AudioSource>();
			hit = AssetsManager.getAudioClip( "Sounds/roar.wav" );
			loose = AssetsManager.getAudioClip( "Sounds/loose.wav" );
			//source.Play();
		}

		void Update()
		{
			//ToolTip.MoveTo( Input.mousePosition, true );
		}
	}
}