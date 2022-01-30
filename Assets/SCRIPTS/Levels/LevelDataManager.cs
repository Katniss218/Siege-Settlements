using KFF;
using SS.Diplomacy;
using SS.Objects;
using SS.Technologies;
using SS.TerrainCreation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SS.Levels
{
    /// <summary>
    /// Contains data, that's unique to the level or level save state and can't be represented by any other piece of data in the level's scene.
    /// </summary>
    public static class LevelDataManager
    {
        //
        //    General
        //

        /// <summary>
        /// The number of segments of the map (Read Only).
        /// </summary>
        public static int mapSegments { get; private set; }

        /// <summary>
        /// The y size of the map. In world space units (Read Only).
        /// </summary>
        public static float mapHeight { get; private set; }

        public static float mapSize { get { return mapSegments * TerrainMeshCreator.SEGMENT_SIZE; } }

        //
        //    Factions
        //

        /// <summary>
        /// The ID of the player's faction in a currently loaded level.
        /// </summary>
        public const int PLAYER_FAC = 0;

        /// <summary>
        /// Contains the amount of factions registered in the currently loaded level (Read Only).
        /// </summary>
        public static int factionCount { get; private set; }

        static FactionDefinition[] _factions;
        /// <summary>
        /// Contains definitions for every faction registered in the currently loaded level (Read Only).
        /// </summary>
        public static FactionDefinition[] factions
        {
            get
            {
                return _factions;
            }
            set
            {
                // if factions are reset, reset faction data as well.
                // and on tech state changed, since the factions don't match.
                _factionData = null;
                _factions = value;
                onTechStateChanged = new UnityEvent_int_string_TechnologyResearchProgress();
            }
        }


        static FactionData[] _factionData;
        /// <summary>
        /// Contains data for every faction registered in the currently loaded level (Read Only).
        /// </summary>
        public static FactionData[] factionData
        {
            get
            {
                return _factionData;
            }
            set
            {
                if( value != null && value.Length != _factions.Length )
                {
                    throw new Exception( "Faction definitions array must have the same length as the faction data array." );
                }
                _factionData = value;
            }
        }

        public class UnityEvent_int_string_TechnologyResearchProgress : UnityEvent<int, string, TechnologyResearchProgress> { }

        public static UnityEvent_int_string_TechnologyResearchProgress onTechStateChanged = new UnityEvent_int_string_TechnologyResearchProgress();

        public static void SetTech( int factionId, string id, TechnologyResearchProgress progress )
        {
            if( factionData[factionId].techs.TryGetValue( id, out TechnologyResearchProgress curr ) )
            {
                if( curr == progress )
                {
                    return;
                }
                else
                {
                    factionData[factionId].techs[id] = progress;
                    onTechStateChanged?.Invoke( factionId, id, progress );
                }
            }
            else
            {
                throw new Exception( "Unknown technology '" + id + "'." );
            }
        }


        /// <summary>
        /// Contains relations between every faction registered in the currently loaded level (Read Only).
        /// </summary>
        private static RelationMap<DiplomaticRelation> diplomaticRelations;

        public class _UnityEvent_int_int_DiplomaticRelation : UnityEvent<int, int, DiplomaticRelation> { }

        public static _UnityEvent_int_int_DiplomaticRelation onRelationChanged = new _UnityEvent_int_int_DiplomaticRelation();

        public static DiplomaticRelation GetRelation( int fac1, int fac2 )
        {
            return diplomaticRelations[fac1, fac2];
        }

        public static void SetRelation( int fac1, int fac2, DiplomaticRelation rel )
        {
            diplomaticRelations[fac1, fac2] = rel;
            onRelationChanged?.Invoke( fac1, fac2, rel );
        }


        //
        //    Quests
        //


        //
        //    Dialogues
        //







        public static void LoadMapData( KFFSerializer serializer )
        {
            mapSegments = serializer.ReadInt( "Map.Segments" );
            mapHeight = serializer.ReadFloat( "Map.Height" );
        }


        public static void LoadFactions( KFFSerializer serializer )
        {
            int count = serializer.Analyze( "List" ).childCount;
            factionCount = count;

            factions = new FactionDefinition[factionCount];
            for( int i = 0; i < factions.Length; i++ )
            {
                factions[i] = new FactionDefinition();
            }

            serializer.DeserializeArray( "List", factions );
        }

        public static void LoadFactionData( KFFSerializer serializer )
        {
            int count = serializer.Analyze( "List" ).childCount;
            if( count != factionCount )
            {
                throw new Exception( $"The number of faction data ({count}) doesn't match the number of factions in the level ({factionCount})." );
            }

            factionData = new FactionData[factionCount];

            for( int i = 0; i < factionData.Length; i++ )
            {
                factionData[i] = new FactionData();
            }

            serializer.DeserializeArray( "List", factionData );


            int relMatrixLength = serializer.Analyze( "RelationMatrix" ).childCount;

            sbyte[] array = serializer.ReadSByteArray( "RelationMatrix" );

            int supposedMatrixLength = RelationMap<DiplomaticRelation>.GetMatrixLength( factionCount );
            if( relMatrixLength != supposedMatrixLength )
            {
                throw new Exception( $"The no. entries in the faction relation matrix ({relMatrixLength}) doesn't match the number of factions in the level. Correct no. entries {supposedMatrixLength}." );
            }

            diplomaticRelations = new RelationMap<DiplomaticRelation>( factionCount, DiplomaticRelation.Neutral );
            onRelationChanged = new _UnityEvent_int_int_DiplomaticRelation();
            for( int i = 0; i < diplomaticRelations.MatrixLength; i++ )
            {
                diplomaticRelations[i] = (DiplomaticRelation)array[i];
            }
        }

        public static void LoadDaylightCycle( KFFSerializer serializer )
        {
            DaylightCycleController daylightCycle = Object.FindObjectOfType<DaylightCycleController>();
            if( daylightCycle == null )
            {
                throw new System.Exception( "Couldn't find DaylightCycleController object." );
            }

            try
            {
                daylightCycle.dayLength = serializer.ReadInt( "DaylightCycle.DayLength" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.DayLength' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.nightLength = serializer.ReadInt( "DaylightCycle.NightLength" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.NightLength' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.sunIntensity = serializer.ReadFloat( "DaylightCycle.SunIntensity" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.SunIntensity' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.moonIntensity = serializer.ReadFloat( "DaylightCycle.MoonIntensity" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.MoonIntensity' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.sunElevationAngle = serializer.ReadFloat( "DaylightCycle.SunElevationAngle" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.SunElevationAngle' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.moonElevationAngle = serializer.ReadFloat( "DaylightCycle.MoonElevationAngle" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.MoonElevationAngle' (" + serializer.file.fileName + ")." );
            }

            try
            {
                daylightCycle.ambientDayColor = serializer.ReadColor( "DaylightCycle.AmbientDayColor" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.AmbientDayColor' (" + serializer.file.fileName + ")." );
            }
            try
            {
                daylightCycle.ambientNightColor = serializer.ReadColor( "DaylightCycle.AmbientNightColor" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycle.AmbientNightColor' (" + serializer.file.fileName + ")." );
            }
        }

        public static void LoadDaylightCycleData( KFFSerializer serializer )
        {
            if( DaylightCycleController.instance == null )
            {
                throw new Exception( "Couldn't find DaylightCycleController object." );
            }

            try
            {
                DaylightCycleController.instance.time = serializer.ReadFloat( "DaylightCycleData.Time" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'DaylightCycleData.Time' (" + serializer.file.fileName + ")." );
            }
        }

        public static void LoadCameraData( KFFSerializer serializer )
        {


            if( CameraController.instance == null )
            {
                throw new Exception( "Couldn't find CameraController object." );
            }

            try
            {
                CameraController.instance.transform.position = serializer.ReadVector3( "CameraData.Position" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'CameraData.Position' (" + serializer.file.fileName + ")." );
            }

            try
            {
                CameraController.instance.transform.rotation = serializer.ReadQuaternion( "CameraData.Rotation" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'CameraData.Rotation' (" + serializer.file.fileName + ")." );
            }

            try
            {
                CameraController.instance.size = serializer.ReadInt( "CameraData.ZoomSize" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'CameraData.ZoomSize' (" + serializer.file.fileName + ")." );
            }
        }

        public static void LoadTimeData( KFFSerializer serializer )
        {
            GameTimeCounter gameTimeCounter = Object.FindObjectOfType<GameTimeCounter>();

            if( gameTimeCounter == null )
            {
                throw new Exception( "Couldn't find GameTimeCounter object." );
            }

            try
            {
                gameTimeCounter.gameTimeOffset = serializer.ReadFloat( "GameTimeElapsed" );
            }
            catch
            {
                throw new Exception( "Missing or invalid value of 'GameTimeElapsed' (" + serializer.file.fileName + ")." );
            }
        }

        public static void LoadSelectionGroupData( KFFSerializer serializer )
        {
            Guid[][] groups = new Guid[10][];


            KFFSerializer.AnalysisData analysisData = serializer.Analyze( "SelectionGroups.Group1" );
            groups[0] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[0][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group1.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group2" );
            groups[1] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[1][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group2.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group3" );
            groups[2] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[2][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group3.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group4" );
            groups[3] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[3][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group4.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group5" );
            groups[4] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[4][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group5.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group6" );
            groups[5] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[5][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group6.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group7" );
            groups[6] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[6][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group7.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group8" );
            groups[7] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[7][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group8.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group9" );
            groups[8] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[8][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group9.{0}", j ) );
            }

            analysisData = serializer.Analyze( "SelectionGroups.Group10" );
            groups[9] = new Guid[analysisData.childCount];
            for( int j = 0; j < analysisData.childCount; j++ )
            {
                groups[9][j] = serializer.ReadGuid( new Path( "SelectionGroups.Group10.{0}", j ) );
            }

            SelectionGroupUtils.SetSaveData( groups );
        }

        //
        //
        //


        public static void SaveFactionData( KFFSerializer serializer )
        {
            serializer.SerializeArray( "", "List", factionData );

            sbyte[] array = new sbyte[diplomaticRelations.MatrixLength];

            for( int i = 0; i < array.Length; i++ )
            {
                array[i] = (sbyte)diplomaticRelations[i];
            }

            serializer.WriteSByteArray( "", "RelationMatrix", array );
        }

        public static void SaveDaylightCycleData( KFFSerializer serializer )
        {
            if( DaylightCycleController.instance == null )
            {
                throw new Exception( "Couldn't find DaylightCycleController object." );
            }

            serializer.WriteClass( "", "DaylightCycleData" );
            serializer.WriteFloat( "DaylightCycleData", "Time", DaylightCycleController.instance.time );
        }

        public static void SaveCameraData( KFFSerializer serializer )
        {
            if( CameraController.instance == null )
            {
                throw new Exception( "Couldn't find CameraController object." );
            }

            serializer.WriteClass( "", "CameraData" );
            serializer.WriteVector3( "CameraData", "Position", CameraController.instance.transform.position );
            serializer.WriteQuaternion( "CameraData", "Rotation", CameraController.instance.transform.rotation );

            serializer.WriteInt( "CameraData", "ZoomSize", CameraController.instance.size );
        }


        public static void SaveTimeData( KFFSerializer serializer )
        {
            GameTimeCounter gameTimeCounter = Object.FindObjectOfType<GameTimeCounter>();

            if( gameTimeCounter == null )
            {
                throw new Exception( "Couldn't find GameTimeCounter object." );
            }

            serializer.WriteFloat( "", "GameTimeElapsed", gameTimeCounter.GetElapsed() );
        }

        public static void SaveSelectionGroupData( KFFSerializer serializer )
        {
            Guid[][] groups = SelectionGroupUtils.GetSaveData();

            serializer.WriteClass( "", "SelectionGroups" );

            serializer.WriteGuidArray( "SelectionGroups", "Group1", groups[0] );
            serializer.WriteGuidArray( "SelectionGroups", "Group2", groups[1] );
            serializer.WriteGuidArray( "SelectionGroups", "Group3", groups[2] );
            serializer.WriteGuidArray( "SelectionGroups", "Group4", groups[3] );
            serializer.WriteGuidArray( "SelectionGroups", "Group5", groups[4] );
            serializer.WriteGuidArray( "SelectionGroups", "Group6", groups[5] );
            serializer.WriteGuidArray( "SelectionGroups", "Group7", groups[6] );
            serializer.WriteGuidArray( "SelectionGroups", "Group8", groups[7] );
            serializer.WriteGuidArray( "SelectionGroups", "Group9", groups[8] );
            serializer.WriteGuidArray( "SelectionGroups", "Group10", groups[9] );
        }
    }
}