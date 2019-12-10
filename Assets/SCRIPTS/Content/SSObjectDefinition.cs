using KFF;
using SS.Objects.Modules;
using SS.Objects.SubObjects;
using System;
using System.Collections.Generic;

namespace SS.Content
{
	public abstract class SSObjectDefinition : AddressableDefinition
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

		private List<SubObjectDefinition> subObjectCache { get; set; }


		protected SSObjectDefinition( string id ) : base( id )
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

			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.moduleCache[i].moduleId == moduleId )
				{
					throw new Exception( "A module with id '" + moduleId + "' has already been added to this object." );
				}
			}
			this.moduleCache.Add( new ModuleCacheItem( moduleId, module ) );
		}

		/// <summary>
		/// Adds a single module of type T to the object definition.
		/// </summary>
		public void AddSubObject<T>( T subObject ) where T : SubObjectDefinition
		{
			for( int i = 0; i < this.moduleCache.Count; i++ )
			{
				if( this.subObjectCache[i].subObjectId == subObject.subObjectId )
				{
					throw new Exception( "A module with id '" + subObject.subObjectId + "' has already been added to this object." );
				}
			}
			this.subObjectCache.Add( subObject );
		}


		protected void DeserializeModulesAndSubObjectsKFF( KFFSerializer serializer )
		{
			for( int i = 0; i < serializer.Analyze( "SubObjects" ).childCount; i++ )
			{
				string typeId = serializer.ReadString( new Path( "SubObjects.{0}.TypeId", i ) );
				SubObjectDefinition subObjectDef = SubObjectDefinition.TypeIdToDefinition( typeId );
				
				serializer.Deserialize<IKFFSerializable>( new Path( "SubObjects.{0}", i ), subObjectDef );
				Guid subObjectId = Guid.ParseExact( serializer.ReadString( new Path( "SubObjects.{0}.SubObjectId", i ) ), "D" );

				this.subObjectCache.Add( subObjectDef );
			}

			for( int i = 0; i < serializer.Analyze( "Modules" ).childCount; i++ )
			{
				string typeId = serializer.ReadString( new Path( "Modules.{0}.TypeId", i ) );

				ModuleDefinition module = ModuleDefinition.TypeIdToDefinition( typeId );

				serializer.Deserialize<IKFFSerializable>( new Path( "Modules.{0}", i ), module );
				Guid moduleId = Guid.ParseExact( serializer.ReadString( new Path( "Modules.{0}.ModuleId", i ) ), "D" );

				this.AddModule( moduleId, module );
			}
		}

		protected void SerializeModulesAndSubObjectsKFF( KFFSerializer serializer )
		{
			SubObjectDefinition[] subObjectsArray;

			this.GetAllSubObjects( out subObjectsArray );

			serializer.SerializeArray<IKFFSerializable>( "", "SubObjects", subObjectsArray );

			for( int i = 0; i < subObjectsArray.Length; i++ )
			{
				string typeId = SubObjectDefinition.DefinitionToTypeId( subObjectsArray[i] );

				serializer.WriteString( new Path( "Modules.{0}", i ), "TypeId", typeId );
				serializer.WriteString( new Path( "Modules.{0}", i ), "SubObjectId", subObjectsArray[i].subObjectId.ToString( "D" ) );
			}


			ModuleDefinition[] modulesArray;
			Guid[] moduleIdsArray;
			this.GetAllModules( out moduleIdsArray, out modulesArray );

			serializer.SerializeArray<IKFFSerializable>( "", "Modules", modulesArray );

			for( int i = 0; i < modulesArray.Length; i++ )
			{
				string typeId = ModuleDefinition.DefinitionToTypeId( modulesArray[i] );

				serializer.WriteString( new Path( "Modules.{0}", i ), "TypeId", typeId );
				serializer.WriteString( new Path( "Modules.{0}", i ), "ModuleId", moduleIdsArray[i].ToString( "D" ) );
			}
		}
	}
}