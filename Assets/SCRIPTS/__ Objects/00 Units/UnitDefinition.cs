using KFF;
using SS.Content;
using SS.Technologies;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Units
{
	public class UnitDefinition : SSObjectDefinition, ITechsRequired
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


		//--------------------------------------------------------------------
		//  PRODUCTION-RELATED
		//--------------------------------------

		public Dictionary<string, int> cost { get; set; }

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

		public string[] techsRequired { get; set; } // the default techs required to unlock. TODO ----- interface for this? IUnlockable or sth
		
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
				this.size = serializer.ReadVector3( "Size" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Size' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			// Cost
			KFFSerializer.AnalysisData analysisData = serializer.Analyze( "Cost" );
			if( analysisData.isSuccess )
			{
				this.cost = new Dictionary<string, int>( analysisData.childCount );
				try
				{
					for( int i = 0; i < analysisData.childCount; i++ )
					{
						string id = serializer.ReadString( new Path( "Cost.{0}.Id", i ) );
						int amt = serializer.ReadInt( new Path( "Cost.{0}.Amount", i ) );
						if( amt < 1 )
						{
							throw new Exception( "Missing or invalid value of 'Cost' of '" + this.id + "' (" + serializer.file.fileName + ")." );
						}
						this.cost.Add( id, amt );
					}
				}
				catch
				{
					throw new Exception( "Missing or invalid value of 'Cost' of '" + this.id + "' (" + serializer.file.fileName + ")." );
				}
			}
			else
			{
				throw new Exception( "Missing or invalid value of 'Cost' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.buildTime = serializer.ReadFloat( "BuildTime" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'BuildTime' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.techsRequired = serializer.ReadStringArray( "TechsRequired" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'TechsRequired' of '" + this.id + "' (" + serializer.file.fileName + ")." );
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
			serializer.WriteFloat( "", "ViewRange", this.viewRange );

			serializer.WriteFloat( "", "MaxHealth", this.healthMax );
			serializer.Serialize( "", "Armor", this.armor );
			
			serializer.WriteFloat( "", "MovementSpeed", this.movementSpeed );
			serializer.WriteFloat( "", "RotationSpeed", this.rotationSpeed );
			serializer.WriteVector3( "", "Size", this.size );

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

			this.SerializeModulesAndSubObjectsKFF( serializer );
		}
	}
}