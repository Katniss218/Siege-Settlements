using KFF;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Objects.Modules
{
	public abstract class ModuleDefinition : IKFFSerializable
	{
		public AddressableAsset<Sprite> icon { get; set; }

		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckTypeDefConstraints( Type objType );

		/// <summary>
		/// Use this to constrain to which objects this definition can be added (return true to allow, false to disallow).
		/// </summary>
		public abstract bool CheckModuleDefConstraints( List<Type> modTypes );

		/// <summary>
		/// Used to create identity (default) data.
		/// </summary>
		public abstract ModuleData GetIdentityData();

		public abstract void AddModule( GameObject gameObject, Guid moduleId, ModuleData data );
		
		public abstract void DeserializeKFF( KFFSerializer serializer );
		public abstract void SerializeKFF( KFFSerializer serializer );

		public static ModuleDefinition TypeIdToDefinition( string typeId )
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
			throw new Exception( "Unknown module type '" + typeId + "'." );
		}

		public static string DefinitionToTypeId( ModuleDefinition def )
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
			throw new Exception( "Unknown module type '" + def.GetType().Name + "'." );
		}
	}
}