using KFF;
using SS.Modules;
using SS.Modules.Inventories;
using SS.Objects.SubObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Content
{
	public abstract class ObjectDefinition : AddressableDefinition
	{
		private struct ModuleCacheItem
		{
			public ModuleDefinition module { get; set; }
			public Type moduleType { get; set; }
			public Guid moduleId { get; set; }

			public ModuleCacheItem( Guid moduleId, ModuleDefinition module )
			{
				this.moduleId = moduleId;
				this.module = module;
				this.moduleType = module.GetType();
			}
		}
				
		private List<ModuleCacheItem> moduleCache;
		
		/// <summary>
		/// The list of all sub-objects of this specific object.
		/// </summary>
		public List<SubObjectDefinition> subObjectCache { get; set; }


		protected ObjectDefinition( string id ) : base( id )
		{
			this.moduleCache = new List<ModuleCacheItem>();
			this.subObjectCache = new List<SubObjectDefinition>();
		}


		// Checks if a module can be added to this obj, given the obj type and the modules added before this one.
		private bool CanAddModuleType( ModuleDefinition mod )
		{
			if( !mod.CheckTypeDefConstraints( this.GetType() ) )
			{
				return false;
			}
			List<Type> moduleTypes = new List<Type>();
			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				moduleTypes.Add( this.moduleCache[i].moduleType );
			}
			if( !mod.CheckModuleDefConstraints( moduleTypes ) )
			{
				return false;
			}
			return true;
		}


		/// <summary>
		/// Gets a single module of type T (if found). Returns null if no module of specified type is present.
		/// </summary>
		public T GetModule<T>() where T : ModuleDefinition
		{
			Type wantedType = typeof( T );

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.moduleCache[i].moduleType == wantedType )
				{
					return this.moduleCache[i].module as T;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets every module of type T (if found). Returns empty array if no module of specified type is present.
		/// </summary>
		public T[] GetModules<T>() where T : ModuleDefinition
		{
			Type wantedType = typeof( T );
			List<T> ret = new List<T>();

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.moduleCache[i].moduleType == wantedType )
				{
					ret.Add( this.moduleCache[i].module as T );
				}
			}
			return ret.ToArray();
		}

		public void GetAllModules( out Guid[] moduleIds, out ModuleDefinition[] defs )
		{
			moduleIds = new Guid[this.moduleCache.Count];
			defs = new ModuleDefinition[this.moduleCache.Count];
			
			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				moduleIds[i] = this.moduleCache[i].moduleId;
				defs[i] = this.moduleCache[i].module;
			}
		}

		public void GetAllSubObjects( out SubObjectDefinition[] defs )
		{
			defs = new SubObjectDefinition[this.subObjectCache.Count];

			for( int i = 0; i < this.subObjectCache.Count; i++ )
			{
				defs[i] = this.subObjectCache[i];
			}
		}

		/// <summary>
		/// Adds a single module of type T to the object definition.
		/// </summary>
		public void AddModule<T>( Guid moduleId, T module ) where T : ModuleDefinition
		{
			if( !this.CanAddModuleType( module ) )
			{
				throw new Exception( "This module type can't be added to this object." );
			}

			this.moduleCache.Add( new ModuleCacheItem( moduleId, module ) );
		}
		
		protected void DeserializeModulesKFF( KFFSerializer serializer )
		{
			for( int i = 0; i < serializer.Analyze( "SubObjects").childCount; i++ )
			{
				string subObjectTypeString = serializer.ReadString( new Path( "SubObjects.{0}.TypeId", i ) );
				SubObjectDefinition subObjectDef = null;

				if( subObjectTypeString == MeshSubObjectDefinition.KFF_TYPEID )
				{
					subObjectDef = new MeshSubObjectDefinition();
				}
				else if( subObjectTypeString == ParticlesSubObjectDefinition.KFF_TYPEID )
				{
					subObjectDef = new ParticlesSubObjectDefinition();
				}

				serializer.Deserialize<IKFFSerializable>( new Path( "SubObjects.{0}", i ), subObjectDef );


				Guid guid = Guid.ParseExact( serializer.ReadString( new Path( "SubObjects.{0}.SubObjectId", i ) ), "D" );

				this.subObjectCache.Add( subObjectDef );
			}

			for( int i = 0; i < serializer.Analyze( "Modules" ).childCount; i++ )
			{
				string moduleTypeString = serializer.ReadString( new Path( "Modules.{0}.TypeId" , i) );
				ModuleDefinition module = null;
				
				if( moduleTypeString == MeleeModuleDefinition.KFF_TYPEID )
				{
					module = new MeleeModuleDefinition();
				}
				else if( moduleTypeString == RangedModuleDefinition.KFF_TYPEID )
				{
					module = new RangedModuleDefinition();
				}
				else if( moduleTypeString == BarracksModuleDefinition.KFF_TYPEID )
				{
					module = new BarracksModuleDefinition();
				}
				else if( moduleTypeString == ResearchModuleDefinition.KFF_TYPEID )
				{
					module = new ResearchModuleDefinition();
				}
				else if( moduleTypeString == InventoryConstrainedModuleDefinition.KFF_TYPEID )
				{
					module = new InventoryConstrainedModuleDefinition();
				}
				else if( moduleTypeString == InventoryUnconstrainedModuleDefinition.KFF_TYPEID )
				{
					module = new InventoryUnconstrainedModuleDefinition();
				}
				else if( moduleTypeString == ResourceDepositModuleDefinition.KFF_TYPEID )
				{
					module = new ResourceDepositModuleDefinition();
				}
				else if( moduleTypeString == ConstructorModuleDefinition.KFF_TYPEID )
				{
					module = new ConstructorModuleDefinition();
				}
				else
				{
					throw new Exception( "Unknown module type '" + moduleTypeString + "'." );
				}

				serializer.Deserialize<IKFFSerializable>( new Path( "Modules.{0}", i ), module );


				Guid guid = Guid.ParseExact( serializer.ReadString( new Path( "Modules.{0}.ModuleId", i ) ), "D" );

				this.AddModule( guid, module );
			}
		}

		protected void SerializeModulesKFF( KFFSerializer serializer )
		{
			SubObjectDefinition[] subObjectsArray;
			
			this.GetAllSubObjects( out subObjectsArray );

			serializer.SerializeArray<IKFFSerializable>( "", "SubObjects", subObjectsArray );

			for( int i = 0; i < subObjectsArray.Length; i++ )
			{
				string typeString = null;
				
				if( subObjectsArray[i] is MeshSubObjectDefinition )
				{
					typeString = MeshSubObjectDefinition.KFF_TYPEID;
				}
				else if( subObjectsArray[i] is ParticlesSubObjectDefinition )
				{
					typeString = ParticlesSubObjectDefinition.KFF_TYPEID;
				}
				else
				{
					throw new Exception( "Inknown sub-object type '" + subObjectsArray[i].GetType().Name + "'." );
				}

				serializer.WriteString( new Path( "Modules.{0}", i ), "TypeId", typeString );
				serializer.WriteString( new Path( "Modules.{0}", i ), "SubObjectId", subObjectsArray[i].subObjectId.ToString( "D" ) );
			}


			ModuleDefinition[] modulesArray;
			Guid[] moduleIdsArray;
			this.GetAllModules( out moduleIdsArray, out modulesArray );

			serializer.SerializeArray<IKFFSerializable>( "", "Modules", modulesArray );

			for( int i = 0; i < modulesArray.Length; i++ )
			{
				string moduleTypeString = null;

				if( modulesArray[i] is MeleeModuleDefinition )
				{
					moduleTypeString = MeleeModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is RangedModuleDefinition )
				{
					moduleTypeString = RangedModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is BarracksModuleDefinition )
				{
					moduleTypeString = BarracksModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResearchModuleDefinition )
				{
					moduleTypeString = ResearchModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryConstrainedModuleDefinition )
				{
					moduleTypeString = InventoryConstrainedModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is InventoryUnconstrainedModuleDefinition )
				{
					moduleTypeString = InventoryUnconstrainedModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is ResourceDepositModuleDefinition )
				{
					moduleTypeString = ResourceDepositModuleDefinition.KFF_TYPEID;
				}
				else if( modulesArray[i] is ConstructorModuleDefinition )
				{
					moduleTypeString = ConstructorModuleDefinition.KFF_TYPEID;
				}
				else
				{
					throw new Exception( "Inknown module type '" + modulesArray[i].GetType().Name + "'." );
				}

				serializer.WriteString( new Path( "Modules.{0}", i ), "TypeId", moduleTypeString );
				serializer.WriteString( new Path( "Modules.{0}", i ), "ModuleId", moduleIdsArray[i].ToString( "D" ) );
			}
		}
	}
}