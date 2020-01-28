using SS.Content;
using SS.UI;
using UnityEngine;

namespace SS.Objects.Units
{
	public static class UnitDisplayManager
	{
		public static void Display( Unit u )
		{
			SelectionPanel.instance.obj.SetIcon( u.icon );

			SelectionPanel.instance.obj.displayNameText.text = u.displayName;

			GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), SSObjectDFSC.GetHealthString( u.health, u.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );

			if( !u.IsDisplaySafe() )
			{
				return;
			}

			bool blockManual = false;
			if( u.isCivilian )
			{
				CivilianUnitExtension cue = u.GetComponent<CivilianUnitExtension>();

				if( !cue.isEmployed )
				{
					CreateAutodutyButton( u.civilian );
					CreateEmployButton( u.civilian );
				}
				else
				{
					CreateUnemployButton( u.civilian );
					blockManual = true;
				}
			}

			if( u.hasInventoryModule && !blockManual )
			{
				CreateQueryButtons();
			}
		}

		public static void Hide( Unit u )
		{

		}
		

		public static void CreateAutodutyButton( CivilianUnitExtension cue )
		{
			Sprite s = cue.isOnAutomaticDuty ?
				AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autodutyoff" ) :
				AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/autoduty" );

			ActionPanel.instance.CreateButton( "civilian.autoduty", s, "Toggle automatic duty", "Makes the civilian deliver resources to where they are needed.", () =>
			{
				cue.SetAutomaticDuty( !cue.isOnAutomaticDuty );
			} );
		}

		public static void CreateEmployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.employ", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/employ" )
				, "Employ civilian", "Makes the civilian employed at a specified workplace.", () =>
				{
					InputOverrideEmployment.EnableEmploymentInput( cue );
				} );
		}

		public static void CreateUnemployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.unemploy", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/unemploy" )
				, "Fire civilian", "Makes the civilian unemployed.", () =>
				{
					cue.workplace.UnEmploy( cue );
				} );
		}

		public static void CreateQueryButtons()
		{
			ActionPanel.instance.CreateButton( "unit.ap.dropoff", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/dropoff" )
			, "Drop off resources", "Select storage to drop off the resources at. OR, deliver resources to construction sites, barracks, etc.", () =>
			{
				InputOverrideDropOffQuery.EnableInput();
			} );

			ActionPanel.instance.CreateButton( "unit.ap.pickup", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/pickup" )
			, "Pick up resources", "Select storage or resource deposit to pick the resources up from.", () =>
			{
				InputOverridePickUpQuery.EnableInput();
			} );
		}
	}
}