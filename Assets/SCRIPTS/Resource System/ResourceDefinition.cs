using Katniss.Utils;
using KFF;
using SS.Content;
using System;
using UnityEngine;

namespace SS.ResourceSystem
{
	/// <summary>
	/// Defines a resource.
	/// </summary>
	public class ResourceDefinition : Definition
	{
		/// <summary>
		/// The display name of this resource.
		/// </summary>
		public string displayName { get; private set; }

		/// <summary>
		/// The icon of this resource.
		/// </summary>
		public Tuple<string, Sprite> icon { get; private set; }

		/// <summary>
		/// The default deposit type.
		/// </summary>
		public string defaultDeposit { get; set; }

		public Tuple<string, AudioClip> pickupSound { get; private set; }
		public Tuple<string, AudioClip> dropoffSound { get; private set; }

		public ResourceDefinition( string id ) : base( id )
		{

		}

		public override void DeserializeKFF( KFFSerializer serializer )
		{
			this.id = serializer.ReadString( "Id" );
			this.displayName = serializer.ReadString( "DisplayName" );

			this.defaultDeposit = serializer.ReadString( "DefaultDeposit" );

			this.icon = serializer.ReadSpriteFromAssets( "Icon" );

			this.pickupSound = serializer.ReadAudioClipFromAssets( "PickupSound" );
			this.dropoffSound = serializer.ReadAudioClipFromAssets( "DropoffSound" );
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			serializer.WriteString( "", "Id", this.id );
			serializer.WriteString( "", "DisplayName", this.displayName );

			serializer.WriteString( "", "DefaultDeposit", this.defaultDeposit );

			serializer.WriteString( "", "Icon", this.icon.Item1 );

			serializer.WriteString( "", "PickupSound", this.pickupSound.Item1 );
			serializer.WriteString( "", "DropoffSound", this.dropoffSound.Item1 );
		}
	}
}