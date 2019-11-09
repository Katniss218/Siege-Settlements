using KFF;
using SS.Content;
using SS.Modules;
using SS.Technologies;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Buildings
{
	[Serializable]
	public class BuildingDefinition : ObjectDefinition, ITechsRequired
	{
		private string __displayName = "<missing>";
		public string displayName
		{
			get { return this.__displayName; }
			set
			{
				this.__displayName = value ?? throw new Exception( "'DisplayName' can't be null." );
			}
		}

		//--------------------------------------------------------------------
		//  HEALTH-RELATED
		//--------------------------------------

		private float __healthMax = 1.0f;
		public float healthMax
		{
			get { return this.__healthMax; }
			set
			{
				if( value <= 0.0f )
				{
					throw new Exception( "'HealthMax' can't be less than or equal to 0." );
				}
				this.__healthMax = value;
			}
		}

		public Armor armor { get; set; }


		//--------------------------------------------------------------------
		//  SIZE-RELATED
		//--------------------------------------

		private Vector3 __size = new Vector3( 0.25f, 0.25f, 0.25f );
		public Vector3 size
		{
			get { return this.__size; }
			set
			{
				if( value.x <= 0.0f
				 || value.y <= 0.0f
				 || value.z <= 0.0f )
				{
					throw new Exception( "'Size' can't have values less than or equal to 0." );
				}
				this.__size = value;
			}
		}


		public Vector3[] placementNodes { get; set; } // len = 0, if empty

		public Vector3? entrance = null;


		public Dictionary<string, int> cost { get; private set; }
		

		//public ModuleDefinition barracks;
		//public ModuleDefinition research;
		public string[] techsRequired { get; private set; } // the default techs required to unlock.


		//--------------------------------------------------------------------
		//  ASSETS
		//--------------------------------------
		
		public AddressableAsset<AudioClip> buildSoundEffect { get; private set; }
		public AddressableAsset<AudioClip> deathSoundEffect { get; private set; }
		public AddressableAsset<Sprite> icon { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public BuildingDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );
			this.size = serializer.ReadVector3( "Size" );

			this.placementNodes = serializer.ReadVector3Array( "PlacementNodes" );

			if( serializer.Analyze( "Entrance" ).isSuccess )
			{
				this.entrance = serializer.ReadVector3( "Entrance" );
			}

			// Cost
			var analysisData = serializer.Analyze( "Cost" );
			this.cost = new Dictionary<string, int>( analysisData.childCount );
			for( int i = 0; i < analysisData.childCount; i++ )
			{
				string id = serializer.ReadString( new Path( "Cost.{0}.Id", i ) );
				int amt = serializer.ReadInt( new Path( "Cost.{0}.Amount", i ) );
				if( amt < 1 )
				{
					throw new Exception( "Can't have cost with amount less than or equal to 0." );
				}
				this.cost.Add( id, amt );
			}
			

			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );
			
			this.buildSoundEffect = serializer.ReadAudioClipFromAssets( "BuildSound" );
			this.deathSoundEffect = serializer.ReadAudioClipFromAssets( "DeathSound" );

			this.icon = serializer.ReadSpriteFromAssets( "Icon" );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			serializer.WriteVector3( "", "Size", this.size );

			serializer.WriteVector3Array( "", "PlacementNodes", this.placementNodes );

			if( this.entrance != null )
			{
				serializer.WriteVector3( "", "Entrance", this.entrance.Value );
			}

			// Cost
			serializer.WriteList( "", "Cost" );
			int i = 0;
			foreach( var kvp in this.cost )
			{
				serializer.AppendClass( "Cost" );
				serializer.WriteString( new Path( "Cost.{0}", i ), "Id", kvp.Key );
				serializer.WriteInt( new Path( "Cost.{0}", i ), "Amount", kvp.Value );
				i++;
			}

			
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );
			
			serializer.WriteString( "", "BuildSound", (string)this.buildSoundEffect );
			serializer.WriteString( "", "DeathSound", (string)this.deathSoundEffect );
			serializer.WriteString( "", "Icon", (string)this.icon );

			this.SerializeModulesKFF( serializer );
		}
	}
}