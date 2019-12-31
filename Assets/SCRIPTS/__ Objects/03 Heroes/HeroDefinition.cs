using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.Heroes
{
	public class HeroDefinition : SSObjectDefinition
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

		private float __viewRange = 0.0f;
		public float viewRange
		{
			get { return this.__viewRange; }
			set
			{
				if( value <= 0.0f )
				{
					throw new Exception( "'ViewRange' can't be less than or equal to 0." );
				}
				this.__viewRange = value;
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

		public AddressableAsset<AudioClip> hurtSoundEffect { get; private set; }
		public AddressableAsset<AudioClip> deathSoundEffect { get; private set; }
		public AddressableAsset<Sprite> icon { get; private set; }


		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public HeroDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			try
			{
				this.id = serializer.ReadString( "Id" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Id' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.displayName = serializer.ReadString( "DisplayName" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DisplayName' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.displayTitle = serializer.ReadString( "DisplayTitle" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'DisplayTitle' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.viewRange = serializer.ReadFloat( "ViewRange" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'ViewRange' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.healthMax = serializer.ReadFloat( "MaxHealth" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'MaxHealth' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.armor = new Armor();
				serializer.Deserialize( "Armor", this.armor );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Armor' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}



			try
			{
				this.movementSpeed = serializer.ReadFloat( "MovementSpeed" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'MovementSpeed' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.rotationSpeed = serializer.ReadFloat( "RotationSpeed" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'RotationSpeed' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.radius = serializer.ReadFloat( "Radius" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Radius' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.height = serializer.ReadFloat( "Height" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Height' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}


			try
			{
				this.hurtSoundEffect = serializer.ReadAudioClipFromAssets( "HurtSound" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'HurtSound' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.deathSoundEffect = serializer.ReadAudioClipFromAssets( "DeathSound" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'DeathSound' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.icon = serializer.ReadSpriteFromAssets( "Icon" );
			}
			catch( KFFException )
			{
				throw new Exception( "Missing 'Icon' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}


			this.DeserializeModulesAndSubObjectsKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );
			serializer.WriteString( "", "DisplayTitle", this.displayTitle );
			serializer.WriteFloat( "", "ViewRange", this.viewRange );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteFloat( "", "Radius", this.radius );
			serializer.WriteFloat( "", "Height", this.height );

			serializer.WriteString( "", "HurtSound", (string)this.hurtSoundEffect );
			serializer.WriteString( "", "DeathSound", (string)this.deathSoundEffect );
			serializer.WriteString( "", "Icon", (string)this.icon );

			this.SerializeModulesAndSubObjectsKFF( serializer );
		}
	}
}