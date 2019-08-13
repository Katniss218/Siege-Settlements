﻿using System;
using TMPro;
using UnityEngine;
using SS.Levels;
using SS.Units;

namespace SS
{
	/// <summary>
	/// The main class, think of it like a Game Manager class.
	/// </summary>
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
				if( __canvas == null ) { __canvas = FindObjectOfType<Main>().transform.Find( "Canvas" ).GetComponent<Canvas>(); }
				return __canvas;
			}
		}

		private static Sprite __switcherObj = null;
		public static Sprite switcherObj
		{
			get
			{
				if( __switcherObj == null ) { __switcherObj = Resources.Load<Sprite>( "Textures/selection_toggle" ); }
				return __switcherObj;
			}
		}

		private static Sprite __switcherList = null;
		public static Sprite switcherList
		{
			get
			{
				if( __switcherList == null ) { __switcherList = Resources.Load<Sprite>( "Textures/selection_toggle_list" ); }
				return __switcherList;
			}
		}

		private static Sprite __tooltipBackground = null;
		public static Sprite toolTipBackground
		{
			get
			{
				if( __tooltipBackground == null ) { __tooltipBackground = Resources.Load<Sprite>( "Textures/tooltip_background" ); }
				return __tooltipBackground;
			}
		}

		private static Material __materialFactionColored = null;
		public static Material materialFactionColored
		{
			get
			{
				if( __materialFactionColored == null ) { __materialFactionColored = new Material( Resources.Load<Shader>( "Shaders/FactionColored" ) ); }
				return __materialFactionColored;
			}
		}

		private static Material __materialFactionColoredDestroyable = null;
		public static Material materialFactionColoredDestroyable
		{
			get
			{
				if( __materialFactionColoredDestroyable == null ) { __materialFactionColoredDestroyable = new Material( Resources.Load<Shader>( "Shaders/FactionColoredDestroyable" ) ); }
				return __materialFactionColoredDestroyable;
			}
		}

		private static Material __materialSolid = null;
		public static Material materialSolid
		{
			get
			{
				if( __materialSolid == null ) { __materialSolid = new Material( Resources.Load<Shader>( "Shaders/Solid" ) ); }
				return __materialSolid;
			}
		}

		private static Material __materialPlantTransparent = null;
		public static Material materialPlantTransparent
		{
			get
			{
				if( __materialPlantTransparent == null ) { __materialPlantTransparent = new Material( Resources.Load<Shader>( "Shaders/PlantTransparent" ) ); }
				return __materialPlantTransparent;
			}
		}

		private static Material __materialPlantSolid = null;
		public static Material materialPlantSolid
		{
			get
			{
				if( __materialPlantSolid == null ) { __materialPlantSolid = new Material( Resources.Load<Shader>( "Shaders/PlantSolid" ) ); }
				return __materialPlantSolid;
			}
		}

		private static Material __materialParticle = null;
		public static Material materialParticle
		{
			get
			{
				if( __materialParticle == null ) { __materialParticle = Resources.Load<Material>( "ParticleMaterial" ); }
				return __materialParticle;
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

		private void Awake()
		{
			// initialize the singleton
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