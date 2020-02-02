using KFF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public abstract class SSModuleDefinition : IKFFSerializable
	{
		public string displayName { get; set; }

		public AddressableAsset<Sprite> icon { get; set; }

		//
		//
		//

		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		//
		//
		//
		
		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckTypeDefConstraints( Type objType );

		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckModuleDefConstraints( List<Type> modTypes );

		public abstract void AddModule( SSObject ssObject, Guid moduleId );

		//
		//
		//

		/// <summary>
		/// Reads a string type and returns an instance for that corresponding type.
		/// </summary>
		public static SSModuleDefinition TypeIdToInstance( string typeId )
		{
			if( typeId == MeleeModule.KFF_TYPEID )
			{
				return new MeleeModuleDefinition();
			}
			if( typeId == RangedModule.KFF_TYPEID )
			{
				return new RangedModuleDefinition();
			}
			if( typeId == BarracksModule.KFF_TYPEID )
			{
				return new BarracksModuleDefinition();
			}
			if( typeId == ResearchModule.KFF_TYPEID )
			{
				return new ResearchModuleDefinition();
			}
			if( typeId == InventoryModule.KFF_TYPEID )
			{
				return new InventoryModuleDefinition();
			}
			if( typeId == ResourceDepositModule.KFF_TYPEID )
			{
				return new ResourceDepositModuleDefinition();
			}
			if( typeId == ConstructorModule.KFF_TYPEID )
			{
				return new ConstructorModuleDefinition();
			}
			if( typeId == InteriorModule.KFF_TYPEID )
			{
				return new InteriorModuleDefinition();
			}


			if( typeId == TavernWorkplaceModule.KFF_TYPEID )
			{
				return new TavernWorkplaceModuleDefinition();
			}
			if( typeId == ResourceCollectorWorkplaceModule.KFF_TYPEID )
			{
				return new ResourceCollectorWorkplaceModuleDefinition();
			}
			throw new Exception( "Unknown module type '" + typeId + "'." );
		}

		/// <summary>
		/// Reads a instance and returns a string type for that corresponding instance.
		/// </summary>
		public static string InstanceToTypeId( SSModuleDefinition def )
		{
			if( def is MeleeModuleDefinition )
			{
				return MeleeModule.KFF_TYPEID;
			}
			if( def is RangedModuleDefinition )
			{
				return RangedModule.KFF_TYPEID;
			}
			if( def is BarracksModuleDefinition )
			{
				return BarracksModule.KFF_TYPEID;
			}
			if( def is ResearchModuleDefinition )
			{
				return ResearchModule.KFF_TYPEID;
			}
			if( def is InventoryModuleDefinition )
			{
				return InventoryModule.KFF_TYPEID;
			}
			if( def is ResourceDepositModuleDefinition )
			{
				return ResourceDepositModule.KFF_TYPEID;
			}
			if( def is ConstructorModuleDefinition )
			{
				return ConstructorModule.KFF_TYPEID;
			}
			if( def is InteriorModuleDefinition )
			{
				return InteriorModule.KFF_TYPEID;
			}


			if( def is TavernWorkplaceModuleDefinition )
			{
				return TavernWorkplaceModule.KFF_TYPEID;
			}
			if( def is ResourceCollectorWorkplaceModuleDefinition )
			{
				return ResourceCollectorWorkplaceModule.KFF_TYPEID;
			}
			throw new Exception( "Unknown module type '" + def.GetType().Name + "'." );
		}
	}
}