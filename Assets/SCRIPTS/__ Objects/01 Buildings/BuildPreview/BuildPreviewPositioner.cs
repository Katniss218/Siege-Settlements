using SS.InputSystem;
using UnityEngine;

namespace SS.Objects.Buildings
{
    /// <summary>
    /// Component used to position the build preview in the scene.
    /// </summary>
    public class BuildPreviewPositioner : MonoBehaviour
    {
        private const float KBD_ROTATION_SPEED = 45.0f;

        /// <summary>
        /// It'll snap these to existing buildings' nodes when placing.
        /// </summary>
        public Vector3[] attachmentNodes { get; set; }

        /// <summary>
        /// How far away should the snapping work.
        /// </summary>
        public Vector3 nodesSnapRange { get; set; }


        private void Rotate( float speed, bool smoothed = true ) // clockwise
        {
            if( smoothed )
                this.transform.Rotate( 0, speed * Time.deltaTime, 0 );
            else
                this.transform.Rotate( 0, speed, 0 );
        }

        private void ResetRotation()
        {
            this.transform.rotation = Quaternion.identity;
        }


        private void ShowAtCursor()
        {
            if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hitInfo, float.MaxValue, ObjectLayer.TERRAIN_MASK ) )
            {
                this.transform.position = hitInfo.point;
            }
        }

        private void Hide()
        {
            this.transform.position = new Vector3( -9999.0f, -9999.0f, 3000.0f );
        }

        private void SnapToGround()
        {
            if( Physics.Raycast( new Ray( this.transform.position + new Vector3( 0.0f, 50.0f, 0.0f ), Vector3.down ), out RaycastHit hitInfo, float.MaxValue, ObjectLayer.TERRAIN_MASK ) )
            {
                this.transform.position = hitInfo.point;
            }
        }



        private void Inp_RotateCCW( InputQueue self )
        {
            if( Input.GetKey( KeyCode.LeftShift ) )
            {
                this.Rotate( -KBD_ROTATION_SPEED );
            }
            self.StopExecution();
        }

        private void Inp_RotateCW( InputQueue self )
        {
            if( Input.GetKey( KeyCode.LeftShift ) )
            {
                this.Rotate( KBD_ROTATION_SPEED );
            }
            self.StopExecution();
        }

        private void Inp_RotateCCW90( InputQueue self )
        {
            if( !Input.GetKey( KeyCode.LeftShift ) )
            {
                this.Rotate( -45, false );
            }
            self.StopExecution();
        }

        private void Inp_RotateCW90( InputQueue self )
        {
            if( !Input.GetKey( KeyCode.LeftShift ) )
            {
                this.Rotate( 45, false );
            }
            self.StopExecution();
        }

        private void Inp_ResetRotation( InputQueue self )
        {
            this.ResetRotation();
            self.StopExecution();
        }

        void OnEnable()
        {
            Main.keyboardInput.RegisterOnHold( KeyCode.Q, 40.0f, this.Inp_RotateCCW, true );
            Main.keyboardInput.RegisterOnHold( KeyCode.E, 40.0f, this.Inp_RotateCW, true );
            Main.keyboardInput.RegisterOnPress( KeyCode.Q, 40.0f, this.Inp_RotateCCW90, true );
            Main.keyboardInput.RegisterOnPress( KeyCode.E, 40.0f, this.Inp_RotateCW90, true );
            Main.keyboardInput.RegisterOnPress( KeyCode.R, 40.0f, this.Inp_ResetRotation, true );
        }

        void OnDisable()
        {
            if( Main.keyboardInput != null )
            {
                Main.keyboardInput.ClearOnHold( KeyCode.Q, this.Inp_RotateCCW );
                Main.keyboardInput.ClearOnHold( KeyCode.E, this.Inp_RotateCW );
                Main.keyboardInput.ClearOnPress( KeyCode.Q, this.Inp_RotateCCW90 );
                Main.keyboardInput.ClearOnPress( KeyCode.E, this.Inp_RotateCW90 );
                Main.keyboardInput.ClearOnPress( KeyCode.R, this.Inp_ResetRotation );
            }
        }


        private void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                this.Hide();
                return;
            }

            this.ShowAtCursor();

            // If not pressing SHIFT.
            // Snap the preview to placement nodes.
            if( !(Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift )) )
            {
                Vector3 cursorOnTerrain = this.transform.position; // since we are moving the obj to the cursor's position on the terrain.

                Collider[] overlappingColliders = Physics.OverlapBox( this.transform.position + new Vector3( 0, this.nodesSnapRange.y * 0.5f, 0 ), this.nodesSnapRange, this.transform.rotation, ObjectLayer.POTENTIALLY_INTERACTIBLE_MASK );

                float nodePairMinDst = float.MaxValue;
                Vector3? nodeSelf = null; // if null, no nodes found.
                Vector3? nodeTarget = null; // if null, no nodes found.

                float snappingRange = this.nodesSnapRange.magnitude;

                foreach( var overlappingCollider in overlappingColliders )
                {
                    Building building = overlappingCollider.GetComponent<Building>();

                    if( building == null )
                    {
                        continue;
                    }

                    // Disable snapping to other factions' buildings.
                    if( building.factionId != Levels.LevelDataManager.PLAYER_FAC )
                    {
                        continue;
                    }

                    Vector3[] targetNodes = building.placementNodes;

                    // Find the closest pair of nodes.
                    foreach( var selfNode in this.attachmentNodes )
                    {
                        foreach( var targetNode in targetNodes )
                        {
                            Matrix4x4 toWorldSelf = this.transform.localToWorldMatrix;
                            Vector3 globalSelf = toWorldSelf.MultiplyPoint( selfNode );

                            Matrix4x4 toWorldTarget = overlappingCollider.transform.localToWorldMatrix;
                            Vector3 globalTarget = toWorldTarget.MultiplyPoint( targetNode );

                            float distance = Vector3.Distance( globalSelf, globalTarget );
                            if( distance <= snappingRange && distance < nodePairMinDst )
                            {
                                nodeSelf = globalSelf;
                                nodeTarget = globalTarget;
                                nodePairMinDst = distance;
                            }
                        }
                    }
                }

                if( nodeTarget.HasValue )
                {
                    // Move the preview to overlap the 2 nodes.

                    Vector3 diff = nodeTarget.Value - nodeSelf.Value;

                    this.transform.Translate( diff, Space.World );

                    // rotate the preview to face the mouse.

                    Vector3 dirNodeToCursor = cursorOnTerrain - nodeTarget.Value;
                    dirNodeToCursor.y = 0;
                    Vector3 dirNodeToSelf = this.transform.position - nodeTarget.Value;
                    dirNodeToSelf.y = 0;

                    Quaternion rot = Quaternion.FromToRotation( dirNodeToSelf.normalized, dirNodeToCursor.normalized );
                    this.transform.RotateAround( nodeTarget.Value, Vector3.up, rot.eulerAngles.y );
                }

                this.SnapToGround();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Matrix4x4 toWorld = this.transform.localToWorldMatrix;

            foreach( var node in this.attachmentNodes )
            {
                Gizmos.DrawSphere( toWorld.MultiplyPoint( node ), 0.05f );
            }
        }
#endif
    }
}