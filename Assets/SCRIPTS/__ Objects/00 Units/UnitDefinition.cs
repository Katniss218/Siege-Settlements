using KFF;
using SS.Content;
using SS.Technologies;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Units
{
	public class UnitDefinition : ObjectDefinition, ITechsRequired
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
		//  MOVEMENT-RELATED
		//--------------------------------------

		private float __movementSpeed = 0.0f;
		public float movementSpeed
		{
			get { return this.__movementSpeed; }
			set
			{
				if( value < 0.0f )
				{
					throw new Exception( "'MovementSpeed' can't be less than 0." );
				}
				this.__movementSpeed = value;
			}
		}

		private float __rotationSpeed = 0.0f;
		public float rotationSpeed
		{
			get { return this.__rotationSpeed; }
			set
			{
				if( value < 0.0f )
				{
					throw new Exception( "'RotationSpeed' can't be less than 0." );
				}
				this.__rotationSpeed = value;
			}
		}


		//--------------------------------------------------------------------
		//  SIZE-RELATED
		//--------------------------------------

		private float __radius = 0.25f;
		public float radius
		{
			get { return this.__radius; }
			set
			{
				if( value <= 0.0f )
				{
					throw new Exception( "'Radius' can't be less than or equal to 0." );
				}
				this.__radius = value;
			}
		}

		private float __height = 0.25f;
		public float height
		{
			get { return this.__height; }
			set
			{
				if( value <= 0.0f )
				{
					throw new Exception( "'Height' can't be less than or equal to 0." );
				}
				this.__height = value;
			}
		}


		//--------------------------------------------------------------------
		//  PRODUCTION-RELATED
		//--------------------------------------

		public Dictionary<string, int> cost { get; private set; }

		private float __buildTime = 0.0f;
		public float buildTime
		{
			get { return this.__buildTime; }
			set
			{
				if( value < 0.0f )
				{
					throw new Exception( "'BuildTime' can't be less than 0." );
				}
				this.__buildTime = value;
			}
		}

		public string[] techsRequired { get; private set; } // the default techs required to unlock. TODO ----- interface for this? IUnlockable or sth


		//--------------------------------------------------------------------
		//  ASSETS
		//--------------------------------------
		
		public AddressableAsset<Sprite> icon { get; private set; }
		

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public UnitDefinition( string id ) : base( id )
		{

		}
		
		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );
			
			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );

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

			this.buildTime = serializer.ReadFloat( "BuildTime" );
			this.techsRequired = serializer.ReadStringArray( "TechsRequired" );
			
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );

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

			serializer.WriteFloat( "", "BuildTime", this.buildTime );
			serializer.WriteStringArray( "", "TechsRequired", this.techsRequired );
			
			serializer.WriteString( "", "Icon", (string)this.icon );

			this.SerializeModulesKFF( serializer );
		}
	}
}