using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.Objects.Extras
{
	public class ExtraDefinition : ObjectDefinition
	{
		public string displayName { get; set; }

		//--------------------------------------------------------------------
		//  SIZE-RELATED
		//--------------------------------------

		public bool isObstacle { get; set; }

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
		

		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
		// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

		public ExtraDefinition( string id ) : base( id )
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
				this.isObstacle = serializer.ReadBool( "IsObstacle" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'IsObstacle' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			try
			{
				this.size = serializer.ReadVector3( "Size" );
			}
			catch
			{
				throw new Exception( "Missing or invalid value of 'Size' of '" + this.id + "' (" + serializer.file.fileName + ")." );
			}

			this.DeserializeModulesAndSubObjectsKFF( serializer );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteBool( "", "IsObstacle", this.isObstacle );
			serializer.WriteVector3( "", "Size", this.size );
			
			this.SerializeModulesAndSubObjectsKFF( serializer );
		}
	}
}