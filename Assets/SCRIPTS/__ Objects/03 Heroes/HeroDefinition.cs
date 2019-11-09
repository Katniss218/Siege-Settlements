using KFF;
using SS.Content;
using SS.Modules;
using System;
using UnityEngine;

namespace SS.Heroes
{
	public class HeroDefinition : ObjectDefinition
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
		
		private string __displayTitle = "<missing>";
		public string displayTitle
		{
			get { return this.__displayTitle; }
			set
			{
				this.__displayTitle = value ?? throw new Exception( "'DisplayTitle' can't be null." );
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
		//  ASSETS
		//--------------------------------------
		
		public AddressableAsset<Sprite> icon { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public HeroDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );
			this.displayTitle = serializer.ReadString( "DisplayTitle" );

			this.healthMax = serializer.ReadFloat( "MaxHealth" );
			this.armor = new Armor();
			serializer.Deserialize( "Armor", this.armor );
			

			this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			this.radius = serializer.ReadFloat( "Radius" );
			this.height = serializer.ReadFloat( "Height" );
			
			this.icon = serializer.ReadSpriteFromAssets( "Icon" );

			this.DeserializeModulesKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "DisplayTitle", this.displayTitle );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );
			
			serializer.WriteString( "", "Icon", (string)this.icon );

			this.SerializeModulesKFF( serializer );
		}
	}
}