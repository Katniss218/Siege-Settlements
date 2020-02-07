using SS.Content;
using SS.InputSystem;
using SS.Objects.Modules;
using SS.Objects.Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SS
{
	public static class InputOverrideEmployment
	{
		// block mouse inputs.

		// left - assigns the employment.

		// right - cancels the blocking & custom input.


		/// <summary>
		/// The Civilian that this input override affects.
		/// </summary>
		public static CivilianUnitExtension affectedCivilian { get; private set; }



		private static void Inp_Cancel( InputQueue self )
		{
			DisableEmploymentInput();
			self.StopExecution();
		}

		private static void Inp_TryEmploy( InputQueue self )
		{
			if( !EventSystem.current.IsPointerOverGameObject() )
			{
				RaycastHit hitInfo;
				if( Physics.Raycast( Main.camera.ScreenPointToRay( Input.mousePosition ), out hitInfo ) )
				{
					ISelectDisplayHandler obj = Selection.displayedObject;

					if( !(obj is Unit) )
					{
						return;
					}
					Unit unit = (Unit)obj;

					if( !unit.isCivilian )
					{
						return;
					}

					if( unit.civilian.workplace != null )
					{
						throw new System.Exception( "Tried employing an already employed civilian." );
					}

					WorkplaceModule workplace = hitInfo.collider.GetComponent<WorkplaceModule>();
					if( workplace == null )
					{
						return;
					}

					workplace.Employ( unit.civilian );
					AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), unit.civilian.transform.position );
				}
			}
			DisableEmploymentInput();
			self.StopExecution();
		}
		
		private static void Inp_BlockSelectionOverride( InputQueue self )
		{
			self.StopExecution();
		}


		//
		//
		//

		
		public static void EnableEmploymentInput( CivilianUnitExtension cue )
		{
			// Need to override all 3 channels of mouse input, since selection uses all 3 of them (so all 3 need to be blocked to block selection).
			Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true ); // left
			Main.mouseInput.RegisterOnHold( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.RightMouseButton, 9.0f, Inp_Cancel, true );

			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 9.0f, Inp_TryEmploy, true ); // right
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			affectedCivilian = cue;
		}

		public static void DisableEmploymentInput()
		{
			if( Main.mouseInput != null )
			{
				// The action is assigned to on release to block selection controller deselecting the object when placed
				//    (building get's placed on press, then preview removes itself from the input, and later on release it deselects).
				Main.mouseInput.ClearOnPress( MouseCode.RightMouseButton, Inp_BlockSelectionOverride ); // left
				Main.mouseInput.ClearOnHold( MouseCode.RightMouseButton, Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.RightMouseButton, Inp_Cancel );

				Main.mouseInput.ClearOnPress( MouseCode.LeftMouseButton, Inp_TryEmploy ); // right
				Main.mouseInput.ClearOnHold( MouseCode.LeftMouseButton, Inp_BlockSelectionOverride );
				Main.mouseInput.ClearOnRelease( MouseCode.LeftMouseButton, Inp_BlockSelectionOverride );
			}
			affectedCivilian = null;
		}
	}
}