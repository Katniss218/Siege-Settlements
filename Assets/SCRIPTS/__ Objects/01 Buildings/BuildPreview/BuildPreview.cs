using SS.Content;
using SS.InputSystem;
using SS.Levels;
using SS.Levels.SaveStates;
using SS.Objects.SubObjects;
using SS.Objects.Units;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SS.Objects.Buildings
{
    public class BuildPreview : MonoBehaviour
    {
        /// <summary>
        /// True if the build preview is active (enabled). Note, this will be true as long as the cursor is in 'place mode'.
        /// </summary>
        public static bool IsActive()
        {
            return buildPreviewInstanceGameObject != null;
        }

        private static GameObject buildPreviewInstanceGameObject;



        /// <summary>
        /// The build preview will treat objects in this mask as ground terrain.
        /// </summary>
        public LayerMask terrainMask;

        /// <summary>
        /// The build preview will treat objects in this mask as blocking the placement.
        /// </summary>
        public LayerMask objectsMask;

        /// <summary>
        /// The maximum difference between any 2 points (+- maxDeviation) underneath the building preview, and the position of the building preview, that will be still considered as valid placement spot.
        /// </summary>
        public float maxDeviationY = 0.2f;


        private SSObjectDefinition definition;
        private SSObjectData data;

        private Vector3 GetOverlapHitboxCenter( bool OffsetByDeviation )
        {
            const float epsilon = 0.01f;

            Vector3 size = this.GetSize();
            Vector3 center = new Vector3( 0.0f, (size.y / 2.0f), 0.0f );

            Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;

            if( OffsetByDeviation )
            {
                // Add maxDeviation, to make sure the overlap wouldn't block the (+) range of valid placement spots.
                center.y += this.maxDeviationY / 2.0f; // add deviation in local space.
            }

            center = localToWorldMatrix.MultiplyVector( center );
            center += this.transform.position;

            // Add epsilon to make sure the collider doesn't falsly collide with the ground at otherwise valid placement spot.
            center.y += epsilon;

            return center;
        }

        private bool IsOverlapping()
        {
            Vector3 size = this.GetSize();
            Vector3 center;

            // Overlap with the ground.
            if( size.y > this.maxDeviationY )
            {
                // Subtract the max deviation from the height of the box.
                Vector3 newSize = size - new Vector3( 0.0f, this.maxDeviationY, 0.0f );

                center = this.GetOverlapHitboxCenter( true );

                if( Physics.OverlapBox( center, (newSize / 2.0f), this.transform.rotation, this.terrainMask ).Length > 0 )
                {
                    return true;
                }
            }

            // Overlap with other objects.
            center = this.GetOverlapHitboxCenter( false );

            if( Physics.OverlapBox( center, (size / 2.0f), this.transform.rotation, this.objectsMask, QueryTriggerInteraction.Ignore ).Length > 0 )
            {
                return true;
            }

            return false;
        }

        private bool IsOnFlatGround()
        {
            Vector3 size = this.GetSize();

            Vector3 halfSize = size * 0.5f;

            // Setup the 4 corners for raycast.
            Vector3[] pos = new Vector3[4]
            {
                new Vector3( -halfSize.x, this.maxDeviationY, -halfSize.z ),
                new Vector3( -halfSize.x, this.maxDeviationY, halfSize.z ),
                new Vector3( halfSize.x, this.maxDeviationY, -halfSize.z ),
                new Vector3( halfSize.x, this.maxDeviationY, halfSize.z )
            };

            // Setup the outputs for the 4 corners.
            float[] y = new float[4];

            // Check the 4 corners (do the raycast).
            Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
            RaycastHit hitInfo;
            for( int i = 0; i < 4; i++ )
            {
                pos[i] = localToWorldMatrix.MultiplyVector( pos[i] ) + this.transform.position;
                if( !Physics.Raycast( pos[i], Vector3.down, out hitInfo, 2.0f * this.maxDeviationY, this.terrainMask ) )
                {
                    // If the raycast missed, that means there is a deep chasm at the checked position.
                    return false;
                }
                y[i] = hitInfo.point.y;
            }

            // Check if the y-positions of corners are within the allowed deviation from the building's y-position.
            for( int i = 0; i < 4; i++ )
            {
                // the transform's position is always at the bottom of a building.
                if( y[i] < this.transform.position.y - this.maxDeviationY ) { return false; }
                if( y[i] > this.transform.position.y + this.maxDeviationY ) { return false; }
            }

            return true;
        }

        public Vector3 GetSize()
        {
            if( this.definition is UnitDefinition )
            {
                return ((UnitDefinition)this.definition).size;
            }
            if( this.definition is BuildingDefinition )
            {
                return ((BuildingDefinition)this.definition).size;
            }

            throw new Exception( $"Unsupported definition type ({this.definition.GetType()}) was set to the build preview." );
        }

        /// <summary>
        /// Checks if the building preview's current position is a valid placement spot.
        /// </summary>
        public bool CanBePlacedHere()
        {
            return !this.IsOverlapping() && this.IsOnFlatGround();
        }

        void Start()
        {
            // hook to the events to hide the preview.
        }

        private void Inp_CancelPlacement( InputQueue self )
        {
            Object.Destroy( this.gameObject );
            self.StopExecution();
        }

        private void Inp_TryPlace( InputQueue self )
        {
            if( this.CanBePlacedHere() )
            {
                if( this.definition is UnitDefinition )
                {
                    UnitDefinition unitDefinition = (UnitDefinition)this.definition;
                    UnitData unitData = (UnitData)this.data;

                    unitData.position = this.transform.position;
                    unitData.rotation = this.transform.rotation;

                    Unit unit = UnitCreator.Create( unitDefinition, unitData.guid );
                    UnitCreator.SetData( unit, unitData );
                }

                if( this.definition is BuildingDefinition )
                {
                    BuildingDefinition buildingDefinition = (BuildingDefinition)this.definition;
                    BuildingData buildingData = (BuildingData)this.data;

                    buildingData.position = this.transform.position;
                    buildingData.rotation = this.transform.rotation;

                    Building building = BuildingCreator.Create( buildingDefinition, buildingData.guid );
                    BuildingCreator.SetData( building, buildingData );
                }

                Object.Destroy( this.gameObject );
            }
            self.StopExecution();
        }

        private void Inp_BlockSelectionOverride( InputQueue self )
        {
            // Used just to stop execution (is inserted before selection queue, so it blocks deselecting/selecting, when preview is active).
            if( !SelectionController.isDragging )
            {
#warning instead of this, when build preview is enabled, cancel the drag select action.
                self.StopExecution();
            }
        }

        private void OnEnable()
        {
            Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true ); // left
            Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true );
            Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 1000.0f, this.Inp_TryPlace, true ); // Doesn't break drag-selection when set to 1000

            // Need to override all 3 channels of mouse input, since selection uses all 3 of them (so all 3 need to be blocked to block selection).
            Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true ); // right
            Main.mouseInput.RegisterOnHold( MouseCode.RightMouseButton, 10.0f, this.Inp_BlockSelectionOverride, true );
            Main.mouseInput.RegisterOnRelease( MouseCode.RightMouseButton, 10.0f, this.Inp_CancelPlacement, true );

        }

        private void OnDisable()
        {
            if( Main.mouseInput != null )
            {
                // The action is assigned to on release to block selection controller deselecting the object when placed
                //    (building get's placed on press, then preview removes itself from the input, and later on release it deselects).
                Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, this.Inp_BlockSelectionOverride ); // left
                Main.mouseInput.ClearOnHold( MouseCode.RightMouseButton, this.Inp_BlockSelectionOverride );
                Main.mouseInput.ClearOnRelease( MouseCode.RightMouseButton, this.Inp_CancelPlacement );

                Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, this.Inp_BlockSelectionOverride ); // right
                Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, this.Inp_BlockSelectionOverride );
                Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, this.Inp_TryPlace );
            }
        }

        void Update()
        {
            MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();

            if( this.CanBePlacedHere() )
            {
                foreach( var renderer in renderers )
                {
                    renderer.material.SetColor( "_FactionColor", Color.green );
                }
            }
            else
            {
                foreach( var renderer in renderers )
                {
                    renderer.material.SetColor( "_FactionColor", Color.red );
                }
            }
        }

        public static void CreateOrSwitch( SSObjectDefinition def, SSObjectData data )
        {
            BuildPreview buildPreview;
            BuildPreviewPositioner positioner;

            if( !IsActive() )
            {
                GameObject gameObject = new GameObject();
                buildPreview = gameObject.AddComponent<BuildPreview>();
                positioner = gameObject.AddComponent<BuildPreviewPositioner>();

                buildPreview.terrainMask = ObjectLayer.TERRAIN_MASK;
                buildPreview.objectsMask = ObjectLayer.POTENTIALLY_INTERACTIBLE_MASK;

                buildPreviewInstanceGameObject = gameObject;
            }
            else
            {
                buildPreview = buildPreviewInstanceGameObject.GetComponent<BuildPreview>();
                positioner = buildPreviewInstanceGameObject.GetComponent<BuildPreviewPositioner>();

                // remove previous object's preview
                for( int i = 0; i < buildPreviewInstanceGameObject.transform.childCount; i++ )
                {
                    Object.Destroy( buildPreviewInstanceGameObject.transform.GetChild( i ).gameObject );
                }
            }

            if( def is BuildingDefinition )
            {
                BuildingDefinition buildingDefinition = (BuildingDefinition)def;
                positioner.attachmentNodes = buildingDefinition.placementNodes;

                float max = Mathf.Max( buildingDefinition.size.x, buildingDefinition.size.y, buildingDefinition.size.z );
                positioner.nodesSnapRange = new Vector3( max, max, max );
            }

            buildPreview.definition = def;
            buildPreview.data = data;

            def.GetAllSubObjects( out SubObjectDefinition[] subObjectDefs );

            foreach( var subObjectDef in subObjectDefs )
            {
                if( subObjectDef is MeshSubObjectDefinition )
                {
                    MeshSubObjectDefinition meshSubObjDef = (MeshSubObjectDefinition)subObjectDef;

                    GameObject child = new GameObject( "Prev-sub" );

                    child.transform.SetParent( buildPreviewInstanceGameObject.transform );
                    child.transform.localPosition = meshSubObjDef.localPosition;
                    child.transform.localRotation = meshSubObjDef.localRotation;

                    MeshFilter meshFilter = child.AddComponent<MeshFilter>();
                    meshFilter.mesh = meshSubObjDef.mesh;
                    MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
                    meshRenderer.material = MaterialManager.CreatePlacementPreview( new Color( 1, 0, 1 ) );
                }
            }
        }

        public static void Destroy()
        {
            if( buildPreviewInstanceGameObject != null )
            {
                Object.Destroy( buildPreviewInstanceGameObject );
            }
        }
    }
}