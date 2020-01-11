using SS.Content;
using SS.InputSystem;
using SS.Objects;
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
					SSObjectDFS obj = Selection.displayedObject;

					WorkplaceModule workplace = hitInfo.collider.GetComponent<WorkplaceModule>();

					CivilianUnitExtension cue = obj.GetComponent<CivilianUnitExtension>();
					if( cue.workplace != null )
					{
						throw new System.Exception( "Tried employing employed." );
					}

					if( workplace != null )
					{
						workplace.Employ( cue );
						AudioManager.PlaySound( AssetManager.GetAudioClip( AssetManager.BUILTIN_ASSET_ID + "Sounds/ai_response" ), cue.transform.position );
					}
				}
			}
			

			DisableEmploymentInput();
			self.StopExecution();
		}

		public static CivilianUnitExtension cueTracker { get; private set; }

		private static void Inp_BlockSelectionOverride( InputQueue self )
		{
			self.StopExecution();
		}

		public static void EnableEmploymentInput( CivilianUnitExtension cue )
		{
			// Need to override all 3 channels of mouse input, since selection uses all 3 of them (so all 3 need to be blocked to block selection).
			Main.mouseInput.RegisterOnPress( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true ); // left
			Main.mouseInput.RegisterOnHold( MouseCode.RightMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.RightMouseButton, 9.0f, Inp_Cancel, true );

			Main.mouseInput.RegisterOnPress( MouseCode.LeftMouseButton, 9.0f, Inp_TryEmploy, true ); // right
			Main.mouseInput.RegisterOnHold( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			Main.mouseInput.RegisterOnRelease( MouseCode.LeftMouseButton, 9.0f, Inp_BlockSelectionOverride, true );
			cueTracker = cue;
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
			cueTracker = null;
		}
	}
}