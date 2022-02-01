using KFF;
using SS.Objects.Buildings;
using SS.Content;
using SS.Objects.Extras;
using SS.Objects.Heroes;
using SS.Levels.SaveStates;
using SS.Objects.Projectiles;
using SS.TerrainCreation;
using SS.UI;
using SS.Objects.Units;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using SS.Objects;

namespace SS.Levels
{
    /// <summary>
    /// Manages level-specific stuff.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public const string DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER = "__default__"; // filename of default save state.
        public const string DEFAULT_LEVEL_SAVE_STATE_DISPLAYNAME = ""; // display name of default save state

        /// <summary>
        /// The time stamp of when the last level was loaded (in units of time elapsed since the game's launch) (Read only).
        /// </summary>
        public static float lastLoadTime { get; private set; }

        /// <summary>
        /// Returns true if a level is currently loaded.
        /// </summary>
        public static bool isLevelLoaded
        {
            get
            {
                return !string.IsNullOrEmpty( currentLevelId ) && !string.IsNullOrEmpty( currentLevelSaveStateId );
            }
        }


        /// <summary>
        /// Contains the identifier of the currently loaded level. null if no level is loaded (Read Only).
        /// </summary>
        public static string currentLevelId { get; private set; }

        /// <summary>
        /// Contains the display name of the currently loaded level. null if no level is loaded (Read Only).
        /// </summary>
        public static string currentLevelDisplayName { get; private set; }


        /// <summary>
        /// Contains the identifier of the currently loaded level save state. null if no level save state is loaded (Read Only).
        /// </summary>
        public static string currentLevelSaveStateId { get; private set; }

        /// <summary>
        /// Contains the display name of the currently loaded level save state. null if no level save state is loaded (Read Only).
        /// </summary>
        public static string currentLevelSaveStateDisplayName { get; private set; }



        private static Scene? loadedLevelScene;


        const char REPLACEMENT_CHAR = '_';
        static readonly char[] INVALID_CHARS = new char[] { ' ', '/', '\\', '?', '*', ':', '<', '>', '|' };
        /// <summary>
        /// Converts displayname into valid filename (removes special path characters).
        /// </summary>
        public static string IdentifierFromDisplayName( string displayName )
        {
            StringBuilder sb = new StringBuilder();

            for( int i = 0; i < displayName.Length; i++ )
            {
                bool isValid = true;
                for( int j = 0; j < INVALID_CHARS.Length; j++ )
                {
                    if( displayName[i] == INVALID_CHARS[j] )
                    {
                        sb.Append( REPLACEMENT_CHAR );
                        isValid = false;
                        break;
                    }
                }

                if( isValid )
                {
                    sb.Append( displayName[i] );
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns the path to the 'Levels' directory (Read Only).
        /// </summary>
        public static string levelsDirectoryPath
        {
            get
            {
                return Application.streamingAssetsPath + System.IO.Path.DirectorySeparatorChar + "Levels";
            }
        }

        /// <summary>
        /// Returns path to the level's main directory ('../Levels/levelIdentifier/').
        /// </summary>
        public static string GetLevelMainDirectory( string levelIdentifier )
        {
            return levelsDirectoryPath + System.IO.Path.DirectorySeparatorChar
                + levelIdentifier;
        }

        /// <summary>
        /// Returns path to the level save state's main directory ('../Levels/levelIdentifier/SaveStates/levelSaveStateIdentifier/').
        /// </summary>
        public static string GetLevelSaveStateMainDirectory( string levelIdentifier, string levelSaveStateIdentifier )
        {
            return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "SaveStates" + System.IO.Path.DirectorySeparatorChar
                + levelSaveStateIdentifier;
        }


        /// <summary>
        /// Returns full data path (in the level's 'Data' directory).
        /// </summary>
        public static string GetFullDataPath( string levelIdentifier, string dataPath )
        {
            return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "Data" + System.IO.Path.DirectorySeparatorChar + dataPath;
        }

        /// <summary>
        /// Returns full assets path (in the level's 'Assets' directory).
        /// </summary>
        public static string GetFullAssetsPath( string levelIdentifier, string assetsPath )
        {
            return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "Assets" + System.IO.Path.DirectorySeparatorChar + assetsPath;
        }



        //
        //

        /// <summary>
        /// Returns full level path (in the level's main directory).
        /// </summary>
        public static string GetFullLevelPath( string levelIdentifier, string path )
        {
            return GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + path;
        }

        /// <summary>
        /// Returns full level save state path (in the level save state's main directory).
        /// </summary>
        public static string GetFullLevelSaveStatePath( string levelIdentifier, string levelSaveStateIdentifier, string path )
        {
            return GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + path;
        }





        /// <summary>
        /// Returns every non-default save state of the specified level.
        /// </summary>
        /// <param name="levelDirectoryPath">The path to the level's root directory.</param>
        public static string[] GetAllCustomSaveStates( string levelIdentifier )
        {
            if( string.IsNullOrEmpty( levelIdentifier ) )
            {
                throw new System.ArgumentNullException( "Level identifier can't be null or empty." );
            }

            string[] directories = System.IO.Directory.GetDirectories( GetLevelMainDirectory( levelIdentifier ) );

            List<string> directoriesWithSaveStates = new List<string>();

            for( int i = 0; i < directories.Length; i++ )
            {
                string[] files = System.IO.Directory.GetFiles( directories[i] );

                for( int j = 0; j < files.Length; j++ )
                {
                    if( System.IO.Path.GetFileName( files[j] ) == "save_state.kff" )
                    {
                        directoriesWithSaveStates.Add( directories[i] );
                        break;
                    }
                }
            }

            return directoriesWithSaveStates.ToArray();
        }


        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // UNLOADING

        /// <summary>
        /// Unloads the level gameobjects from the scene.
        /// </summary>
        public static void UnloadLevel( bool loadMenu, Action onAfterLevelUnloaded )
        {
            //#warning incomplete.
            if( !isLevelLoaded )
            {
                throw new System.Exception( "There's no level loaded. You must load a level first." );
            }

            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync( "Level - '" + currentLevelId + ":" + currentLevelSaveStateId + "'" );

            asyncOperation.completed += ( AsyncOperation oper ) =>
            {
                UnloadLevel_AfterUnloadLV( loadMenu );
                onAfterLevelUnloaded?.Invoke();
            };
        }

        private static void UnloadLevel_AfterUnloadLV( bool loadMenu )
        {
            DefinitionManager.Purge();
            AssetManager.Purge();
            AssetManager.sourceLevelId = null;
            Main.onHudLockChange.RemoveAllListeners();

            Selection.Clear();
            AudioManager.StopSounds();

            if( loadMenu )
            {
                SceneManager.UnloadSceneAsync( "Level GUI" );
                SceneManager.LoadScene( "MainMenu", LoadSceneMode.Additive );
            }

            loadedLevelScene = null;
            currentLevelId = null;
            currentLevelSaveStateId = null;
        }





        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // LOADING






        /// <summary>
        /// Loads the level into the scene. Uses the specified save state when loading.
        /// </summary>
        /// <param name="levelIdentifier">The level that is going to be loaded.</param>
        /// <param name="levelSaveStateIdentifier">The save state associated with the level. If null, the level's default save state will be used.</param>
        public static void LoadLevel( string levelIdentifier, string levelSaveStateIdentifier )
        {
            if( string.IsNullOrEmpty( levelIdentifier ) )
            {
                throw new Exception( "The level identifier can't be null or empty." );
            }
            if( isLevelLoaded )
            {
                throw new Exception( "There's already a level loaded. You must unload it first." );
            }

            if( levelSaveStateIdentifier == null ) // if specified save state == null, set to default save state.
            {
                levelSaveStateIdentifier = DEFAULT_LEVEL_SAVE_STATE_IDENTIFIER;
            }
            AssetManager.sourceLevelId = levelIdentifier;

            if( !SceneManager.GetSceneByName( "Level GUI" ).isLoaded )
            {
                SceneManager.LoadScene( "Level GUI", LoadSceneMode.Additive );
            }

            loadedLevelScene = SceneManager.CreateScene( "Level - '" + levelIdentifier + ":" + levelSaveStateIdentifier + "'" );
            SceneManager.SetActiveScene( loadedLevelScene.Value );

            if( SceneManager.GetSceneByName( "MainMenu" ).isLoaded )
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync( "MainMenu" );
                asyncOperation.completed += ( AsyncOperation oper ) =>
                {
                    LoadLevel_AfterUnloadMM( levelIdentifier, levelSaveStateIdentifier );
                };
            }
            else
            {
                LoadLevel_AfterUnloadMM( levelIdentifier, levelSaveStateIdentifier );
            }
        }

        private static void LoadLevel_AfterUnloadMM( string levelIdentifier, string levelSaveStateIdentifier )
        {
            Debug.Log( "LOADING LEVEL: '" + levelIdentifier + ":" + levelSaveStateIdentifier + "'" );
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            long totalLoadTime = 0;

            string pathLevel = GetLevelMainDirectory( levelIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level.kff";
            string pathLevelSaveState = GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ) + System.IO.Path.DirectorySeparatorChar + "level_save_state.kff";

            KFFSerializer serializerLevel;
            KFFSerializer serializerLevelSaveState;

            try
            {
                serializerLevel = KFFSerializer.ReadFromFile( pathLevel, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open level file '" + pathLevel + "'." );
            }

            try
            {
                serializerLevelSaveState = KFFSerializer.ReadFromFile( pathLevelSaveState, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open level file '" + pathLevel + "'." );
            }

            currentLevelDisplayName = serializerLevel.ReadString( "DisplayName" );
            currentLevelSaveStateDisplayName = serializerLevelSaveState.ReadString( "DisplayName" );

            // Set up the paths and serializers for definition files.

            string
                pathUnits = GetFullDataPath( levelIdentifier, "units.kff" ),
                pathBuildings = GetFullDataPath( levelIdentifier, "buildings.kff" ),
                pathProjectiles = GetFullDataPath( levelIdentifier, "projectiles.kff" ),
                pathHeroes = GetFullDataPath( levelIdentifier, "heroes.kff" ),
                pathExtras = GetFullDataPath( levelIdentifier, "extras.kff" ),

                pathResources = GetFullDataPath( levelIdentifier, "resources.kff" ),
                pathTechnologies = GetFullDataPath( levelIdentifier, "technologies.kff" ),
                pathFactions = GetFullDataPath( levelIdentifier, "factions.kff" );

            KFFSerializer
                serializerUnits,
                serializerBuildings,
                serializerProjectiles,
                serializerHeroes,
                serializerExtras,

                serializerResources,
                serializerTechnologies,

                serializerFactions;


            // Open the relevant definition files and parse their contents.
            sw.Start();
            try
            {
                serializerUnits = KFFSerializer.ReadFromFile( pathUnits, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathUnits + "'.", e );
            }

            try
            {
                serializerBuildings = KFFSerializer.ReadFromFile( pathBuildings, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathBuildings + "'.", e );
            }

            try
            {
                serializerProjectiles = KFFSerializer.ReadFromFile( pathProjectiles, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathProjectiles + "'.", e );
            }

            try
            {
                serializerHeroes = KFFSerializer.ReadFromFile( pathHeroes, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathHeroes + "'.", e );
            }

            try
            {
                serializerExtras = KFFSerializer.ReadFromFile( pathExtras, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathExtras + "'.", e );
            }


            try
            {
                serializerResources = KFFSerializer.ReadFromFile( pathResources, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathResources + "'.", e );
            }

            try
            {
                serializerTechnologies = KFFSerializer.ReadFromFile( pathTechnologies, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathTechnologies + "'.", e );
            }


            try
            {
                serializerFactions = KFFSerializer.ReadFromFile( pathFactions, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathFactions + "'.", e );
            }


            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Definition parsing: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();

            // Load the definitions using serializers.

            DefinitionManager.LoadUnitDefinitions( serializerUnits );
            DefinitionManager.LoadBuildingDefinitions( serializerBuildings );
            DefinitionManager.LoadProjectileDefinitions( serializerProjectiles );
            DefinitionManager.LoadHeroDefinitions( serializerHeroes );
            DefinitionManager.LoadExtraDefinitions( serializerExtras );

            DefinitionManager.LoadResourceDefinitions( serializerResources );
            DefinitionManager.LoadTechnologyDefinitions( serializerTechnologies );

            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Loading data & assets: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();

            // resource panel loaded via the scene additive.
            ResourcePanel.instance.InitReset();
            Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Game Scene/Object HUD Canvas" ) );
            Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Game Scene/ToolTip Canvas" ) );

            LevelDataManager.LoadMapData( serializerLevel );

            Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Game Scene/__ GAME MANAGER __" ), Vector3.zero, Quaternion.identity );
            Object.Instantiate( AssetManager.GetPrefab( AssetManager.BUILTIN_ASSET_ID + "Prefabs/Game Scene/Daylight Cycle" ), Vector3.zero, Quaternion.identity );

            LevelDataManager.LoadFactions( serializerFactions );
            LevelDataManager.LoadDaylightCycle( serializerLevel );

            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Loading level-scene prefabs: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();
            CreateTerrain(); // create "env" organizational gameobject, and load terrain from files.


            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Creating terrain: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();


            // Load the save state


            // Set up the paths and serializers for definition files.

            string
                pathFactionData = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_factions.kff" ),

                pathSavedUnits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_units.kff" ),
                pathSavedBuildings = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_buildings.kff" ),
                pathSavedProjectiles = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_projectiles.kff" ),
                pathSavedHeroes = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_heroes.kff" ),
                pathSavedExtras = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( levelIdentifier, levelSaveStateIdentifier ), "save_extras.kff" );

            KFFSerializer
                serializerFactionData,

                serializerSavedUnits,
                serializerSavedBuildings,
                serializerSavedProjectiles,
                serializerSavedHeroes,
                serializerSavedExtras;


            // Open the relevant definition files and parse their contents.

            try
            {
                serializerFactionData = KFFSerializer.ReadFromFile( pathFactionData, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathFactionData + "'." , e);
            }


            try
            {
                serializerSavedUnits = KFFSerializer.ReadFromFile( pathSavedUnits, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathSavedUnits + "'.", e );
            }

            try
            {
                serializerSavedBuildings = KFFSerializer.ReadFromFile( pathSavedBuildings, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathSavedBuildings + "'." , e);
            }

            try
            {
                serializerSavedProjectiles = KFFSerializer.ReadFromFile( pathSavedProjectiles, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathSavedProjectiles + "'.", e );
            }

            try
            {
                serializerSavedHeroes = KFFSerializer.ReadFromFile( pathSavedHeroes, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathSavedHeroes + "'.", e );
            }

            try
            {
                serializerSavedExtras = KFFSerializer.ReadFromFile( pathSavedExtras, DefinitionManager.FILE_ENCODING );
            }
            catch( Exception e )
            {
                if( e is KFFException )
                {
                    throw e;
                }
                throw new Exception( "Can't open file '" + pathSavedExtras + "'.", e );
            }

            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Save state parsing: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();

            // Load the necessary things using serializers.

            var sUnits = GetSavedUnits( serializerSavedUnits );
            var sBuildings = GetSavedBuildings( serializerSavedBuildings );
            var sProjectiles = GetSavedProjectiles( serializerSavedProjectiles );
            var sHeroes = GetSavedHeroes( serializerSavedHeroes );
            var sExtras = GetSavedExtras( serializerSavedExtras );

            LevelDataManager.LoadFactionData( serializerFactionData );
            LevelDataManager.LoadDaylightCycleData( serializerLevelSaveState );
            LevelDataManager.LoadCameraData( serializerLevelSaveState );
            LevelDataManager.LoadTimeData( serializerLevelSaveState );

            Unit[] units = new Unit[sUnits.Count];
            Building[] buildings = new Building[sBuildings.Count];
            Projectile[] projectiles = new Projectile[sProjectiles.Count];
            Hero[] heroes = new Hero[sHeroes.Count];
            Extra[] extras = new Extra[sExtras.Count];

            // Spawn every object on the map (no data present yet, because that might need other objects's guids).

            for( int i = 0; i < units.Length; i++ )
            {
                units[i] = UnitCreator.Create( sUnits[i].Item1, sUnits[i].Item2.guid );
            }
            for( int i = 0; i < buildings.Length; i++ )
            {
                buildings[i] = BuildingCreator.Create( sBuildings[i].Item1, sBuildings[i].Item2.guid );
            }
            for( int i = 0; i < projectiles.Length; i++ )
            {
                projectiles[i] = ProjectileCreator.Create( sProjectiles[i].Item1, sProjectiles[i].Item2.guid );
            }
            for( int i = 0; i < heroes.Length; i++ )
            {
                heroes[i] = HeroCreator.Create( sHeroes[i].Item1, sHeroes[i].Item2.guid );
            }
            for( int i = 0; i < extras.Length; i++ )
            {
                extras[i] = ExtraCreator.Create( sExtras[i].Item1, sExtras[i].Item2.guid );
            }

            // Set the data (guids stay the same).

            for( int i = 0; i < units.Length; i++ )
            {
                UnitCreator.SetData( units[i], sUnits[i].Item2 );
            }
            for( int i = 0; i < buildings.Length; i++ )
            {
                BuildingCreator.SetData( buildings[i], sBuildings[i].Item2 );
            }
            for( int i = 0; i < projectiles.Length; i++ )
            {
                ProjectileCreator.SetData( projectiles[i], sProjectiles[i].Item2 );
            }
            for( int i = 0; i < heroes.Length; i++ )
            {
                HeroCreator.SetData( heroes[i], sHeroes[i].Item2 );
            }
            for( int i = 0; i < extras.Length; i++ )
            {
                ExtraCreator.SetData( extras[i], sExtras[i].Item2 );
            }


            // Set inactive, since by default, there's no selection.
            SelectionPanel.instance.gameObject.SetActive( false );
            SelectionPanel.instance.moduleSubPanelTransform.gameObject.SetActive( false );
            ActionPanel.instance.gameObject.SetActive( false );

            sw.Stop();
            totalLoadTime += sw.ElapsedMilliseconds;
            Debug.Log( "Spawning game objects: " + sw.ElapsedMilliseconds + " ms" );
            sw.Reset();
            sw.Start();


            LevelDataManager.LoadSelectionGroupData( serializerLevelSaveState );

            currentLevelId = levelIdentifier;
            currentLevelSaveStateId = levelSaveStateIdentifier;

            lastLoadTime = Time.time;
        }

        private static List<Tuple<UnitDefinition, UnitData>> GetSavedUnits( KFFSerializer serializer )
        {
            List<Tuple<UnitDefinition, UnitData>> ret = new List<Tuple<UnitDefinition, UnitData>>();

            int count = serializer.Analyze( "List" ).childCount;
            for( int i = 0; i < count; i++ )
            {
                UnitData data = new UnitData();
                serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

                string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
                UnitDefinition def = DefinitionManager.GetUnit( defId );

                ret.Add( new Tuple<UnitDefinition, UnitData>( def, data ) );
            }

            return ret;
        }

        private static List<Tuple<BuildingDefinition, BuildingData>> GetSavedBuildings( KFFSerializer serializer )
        {
            List<Tuple<BuildingDefinition, BuildingData>> ret = new List<Tuple<BuildingDefinition, BuildingData>>();

            int count = serializer.Analyze( "List" ).childCount;
            for( int i = 0; i < count; i++ )
            {
                BuildingData data = new BuildingData();
                serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

                string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
                BuildingDefinition def = DefinitionManager.GetBuilding( defId );

                ret.Add( new Tuple<BuildingDefinition, BuildingData>( def, data ) );
            }

            return ret;
        }

        private static List<Tuple<ProjectileDefinition, ProjectileData>> GetSavedProjectiles( KFFSerializer serializer )
        {
            List<Tuple<ProjectileDefinition, ProjectileData>> ret = new List<Tuple<ProjectileDefinition, ProjectileData>>();

            int count = serializer.Analyze( "List" ).childCount;
            for( int i = 0; i < count; i++ )
            {
                ProjectileData data = new ProjectileData();
                serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

                string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
                ProjectileDefinition def = DefinitionManager.GetProjectile( defId );

                ret.Add( new Tuple<ProjectileDefinition, ProjectileData>( def, data ) );
            }

            return ret;
        }

        private static List<Tuple<HeroDefinition, HeroData>> GetSavedHeroes( KFFSerializer serializer )
        {
            List<Tuple<HeroDefinition, HeroData>> ret = new List<Tuple<HeroDefinition, HeroData>>();

            int count = serializer.Analyze( "List" ).childCount;
            for( int i = 0; i < count; i++ )
            {
                HeroData data = new HeroData();
                serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

                string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
                HeroDefinition def = DefinitionManager.GetHero( defId );

                ret.Add( new Tuple<HeroDefinition, HeroData>( def, data ) );
            }

            return ret;
        }

        private static List<Tuple<ExtraDefinition, ExtraData>> GetSavedExtras( KFFSerializer serializer )
        {
            List<Tuple<ExtraDefinition, ExtraData>> ret = new List<Tuple<ExtraDefinition, ExtraData>>();

            int count = serializer.Analyze( "List" ).childCount;
            for( int i = 0; i < count; i++ )
            {
                ExtraData data = new ExtraData();
                serializer.Deserialize( new Path( "List.{0}.Data", i ), data );

                string defId = serializer.ReadString( new Path( "List.{0}.DefinitionId", i ) );
                ExtraDefinition def = DefinitionManager.GetExtra( defId );

                ret.Add( new Tuple<ExtraDefinition, ExtraData>( def, data ) );
            }

            return ret;
        }


        //
        //
        //


        private static void CreateTerrain()
        {
            //const int size = 4; // the size of the map (in chunks).

            GameObject environment = new GameObject( "Environment" );

            LevelTerrainCreator.terrainParent = environment.transform;
            Texture2D color = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Colormap/colormap.png", TextureType.Color );

            Texture2D height = AssetManager.GetTexture2D( AssetManager.EXTERN_ASSET_ID + "Heightmap/heightmap.png", TextureType.Color );

            LevelTerrainCreator.SpawnMap( height, color, LevelDataManager.mapHeight );
            LevelTerrainCreator.UpdateNavMesh();
        }









        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        // SAVING






        /// <summary>
        /// Saves the scene to the specified level save state (level itself is the currently loaded ofc).
        /// </summary>
        /// <param name="saveSettings">The additional settings that define the behaviour of the method.</param>
        public static void SaveScene( string newLevelSaveStateDisplayName, string newLevelSaveStateId )
        {
            Unit[] units = SSObject.GetAllUnits();
            Building[] buildings = SSObject.GetAllBuildings();
            Projectile[] projectiles = SSObject.GetAllProjectiles();
            Hero[] heroes = SSObject.GetAllHeroes();
            Extra[] extras = SSObject.GetAllExtras();

            Tuple<string, UnitData>[] unitData = new Tuple<string, UnitData>[units.Length];
            Tuple<string, BuildingData>[] buildingData = new Tuple<string, BuildingData>[buildings.Length];
            Tuple<string, ProjectileData>[] projectileData = new Tuple<string, ProjectileData>[projectiles.Length];
            Tuple<string, HeroData>[] heroData = new Tuple<string, HeroData>[heroes.Length];
            Tuple<string, ExtraData>[] extraData = new Tuple<string, ExtraData>[extras.Length];

            for( int i = 0; i < unitData.Length; i++ )
            {
                unitData[i] = new Tuple<string, UnitData>( units[i].definitionId, UnitCreator.GetData( units[i] ) );
            }
            for( int i = 0; i < buildingData.Length; i++ )
            {
                buildingData[i] = new Tuple<string, BuildingData>( buildings[i].definitionId, BuildingCreator.GetData( buildings[i] ) );
            }
            for( int i = 0; i < projectileData.Length; i++ )
            {
                projectileData[i] = new Tuple<string, ProjectileData>( projectiles[i].definitionId, ProjectileCreator.GetData( projectiles[i] ) );
            }
            for( int i = 0; i < heroData.Length; i++ )
            {
                heroData[i] = new Tuple<string, HeroData>( heroes[i].definitionId, HeroCreator.GetData( heroes[i] ) );
            }
            for( int i = 0; i < extraData.Length; i++ )
            {
                extraData[i] = new Tuple<string, ExtraData>( extras[i].definitionId, ExtraCreator.GetData( extras[i] ) );
            }

            string path = GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId );
            if( !System.IO.Directory.Exists( path ) )
            {
                System.IO.Directory.CreateDirectory( path );
            }

            // Save the level save state file.

            string levelSaveStateFilePath = System.IO.Path.Combine( LevelManager.GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "level_save_state.kff" );
            KFFSerializer serializerSaveState = new KFFSerializer( new KFFFile( levelSaveStateFilePath ) );


            string
                pathFactionData = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_factions.kff" ),
                pathSavedUnits = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_units.kff" ),
                pathSavedBuildings = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_buildings.kff" ),
                pathSavedProjectiles = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_projectiles.kff" ),
                pathSavedHeroes = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_heroes.kff" ),
                pathSavedExtras = System.IO.Path.Combine( GetLevelSaveStateMainDirectory( currentLevelId, newLevelSaveStateId ), "save_extras.kff" );

            KFFSerializer
                serializerFactionData = new KFFSerializer( new KFFFile( pathFactionData ) ),
                serializerSavedUnits = new KFFSerializer( new KFFFile( pathSavedUnits ) ),
                serializerSavedBuildings = new KFFSerializer( new KFFFile( pathSavedBuildings ) ),
                serializerSavedProjectiles = new KFFSerializer( new KFFFile( pathSavedProjectiles ) ),
                serializerSavedHeroes = new KFFSerializer( new KFFFile( pathSavedHeroes ) ),
                serializerSavedExtras = new KFFSerializer( new KFFFile( pathSavedExtras ) );

            // Serialize the data into serializers.

            serializerSaveState.WriteString( "", "DisplayName", newLevelSaveStateDisplayName );
            LevelDataManager.SaveDaylightCycleData( serializerSaveState );
            LevelDataManager.SaveCameraData( serializerSaveState );
            LevelDataManager.SaveTimeData( serializerSaveState );
            LevelDataManager.SaveSelectionGroupData( serializerSaveState );

            LevelDataManager.SaveFactionData( serializerFactionData );


            SaveUnits( unitData, serializerSavedUnits );
            SaveBuildings( buildingData, serializerSavedBuildings );
            SaveProjectiles( projectileData, serializerSavedProjectiles );
            SaveHeroes( heroData, serializerSavedHeroes );
            SaveExtras( extraData, serializerSavedExtras );


            // Write the data in serializers to the respective files.

            serializerSaveState.WriteToFile( levelSaveStateFilePath, DefinitionManager.FILE_ENCODING );

            serializerFactionData.WriteToFile( pathFactionData, DefinitionManager.FILE_ENCODING );

            serializerSavedUnits.WriteToFile( pathSavedUnits, DefinitionManager.FILE_ENCODING );
            serializerSavedBuildings.WriteToFile( pathSavedBuildings, DefinitionManager.FILE_ENCODING );
            serializerSavedProjectiles.WriteToFile( pathSavedProjectiles, DefinitionManager.FILE_ENCODING );
            serializerSavedHeroes.WriteToFile( pathSavedHeroes, DefinitionManager.FILE_ENCODING );
            serializerSavedExtras.WriteToFile( pathSavedExtras, DefinitionManager.FILE_ENCODING );
        }

        private static void SaveUnits( Tuple<string, UnitData>[] units, KFFSerializer serializer )
        {
            serializer.WriteList( "", "List" );
            for( int i = 0; i < units.Length; i++ )
            {
                serializer.AppendClass( "List" );

                serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", units[i].Item1 );
                serializer.Serialize( new Path( "List.{0}", i ), "Data", units[i].Item2 );
            }
        }

        private static void SaveBuildings( Tuple<string, BuildingData>[] buildings, KFFSerializer serializer )
        {
            serializer.WriteList( "", "List" );
            for( int i = 0; i < buildings.Length; i++ )
            {
                serializer.AppendClass( "List" );

                serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", buildings[i].Item1 );
                serializer.Serialize( new Path( "List.{0}", i ), "Data", buildings[i].Item2 );
            }
        }

        private static void SaveProjectiles( Tuple<string, ProjectileData>[] projectiles, KFFSerializer serializer )
        {
            serializer.WriteList( "", "List" );
            for( int i = 0; i < projectiles.Length; i++ )
            {
                serializer.AppendClass( "List" );

                serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", projectiles[i].Item1 );
                serializer.Serialize( new Path( "List.{0}", i ), "Data", projectiles[i].Item2 );
            }
        }

        private static void SaveHeroes( Tuple<string, HeroData>[] heroes, KFFSerializer serializer )
        {
            serializer.WriteList( "", "List" );
            for( int i = 0; i < heroes.Length; i++ )
            {
                serializer.AppendClass( "List" );

                serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", heroes[i].Item1 );
                serializer.Serialize( new Path( "List.{0}", i ), "Data", heroes[i].Item2 );
            }
        }

        private static void SaveExtras( Tuple<string, ExtraData>[] extras, KFFSerializer serializer )
        {
            serializer.WriteList( "", "List" );
            for( int i = 0; i < extras.Length; i++ )
            {
                serializer.AppendClass( "List" );

                serializer.WriteString( new Path( "List.{0}", i ), "DefinitionId", extras[i].Item1 );
                serializer.Serialize( new Path( "List.{0}", i ), "Data", extras[i].Item2 );
            }
        }
    }
}