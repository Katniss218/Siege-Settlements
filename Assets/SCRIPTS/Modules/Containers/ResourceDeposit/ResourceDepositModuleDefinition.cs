﻿using KFF;
using SS.Content;
using SS.Extras;
using SS.Modules.Inventories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Modules
{
	public class ResourceDepositModuleDefinition : ModuleDefinition
	{
		public const string KFF_TYPEID = "resource_deposit";

		public struct Slot : IKFFSerializable
		{
			public string resourceId { get; set; }
			public int capacity { get; set; }


			public void DeserializeKFF( KFFSerializer serializer )
			{
				this.resourceId = serializer.ReadString( "ResourceId" );
				this.capacity = serializer.ReadInt( "Capacity" );
			}

			public void SerializeKFF( KFFSerializer serializer )
			{
				serializer.WriteString( "", "ResourceId", this.resourceId );
				serializer.WriteInt( "", "Capacity", this.capacity );
			}
		}


		public Slot[] slots { get; set; }

		public bool isExtracted { get; set; }

		public AddressableAsset<AudioClip> mineSound { get; private set; }


		public override bool CheckTypeDefConstraints( Type objType )
		{
			return
				objType == typeof( ExtraDefinition );
		}

		public override bool CheckModuleDefConstraints( List<Type> modTypes )
		{
			return !(
				modTypes.Contains( typeof( InventoryConstrainedDefinition ) ) ||
				modTypes.Contains( typeof( InventoryUnconstrainedDefinition ) ) ||
				modTypes.Contains( typeof( ResourceDepositModuleDefinition ) ));
		}


		public override void DeserializeKFF( KFFSerializer serializer )
		{
			// resources
			this.slots = new Slot[serializer.Analyze( "Resources" ).childCount];
			serializer.DeserializeArray( "Resources", this.slots );

			this.isExtracted = serializer.ReadBool( "IsExtracted" );
			if( !this.isExtracted )
			{
				this.mineSound = serializer.ReadAudioClipFromAssets( "MineSound" );
			}
		}

		public override void SerializeKFF( KFFSerializer serializer )
		{
			// Cost
			serializer.SerializeArray( "", "Resources", this.slots );

			serializer.WriteBool( "", "IsExtracted", this.isExtracted );
			if( !this.isExtracted )
			{
				serializer.WriteString( "", "MineSound", (string)this.mineSound );
			}
		}

		public override void AddModule( GameObject gameObject, ModuleData data )
		{
			ResourceDepositModule module = gameObject.AddComponent<ResourceDepositModule>();
			module.SetDefData( this, data );
		}
	}
}