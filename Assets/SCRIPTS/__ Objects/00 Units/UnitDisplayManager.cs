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

			//GameObject healthUI = UIUtils.InstantiateText( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 25.0f, -25.0f ), new Vector2( 200.0f, 25.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), SSObjectDFC.GetHealthString( u.health, u.healthMax ) );
			GameObject healthUI = UIUtils.InstantiateValueBar( SelectionPanel.instance.obj.transform, new GenericUIData( new Vector2( 0.0f, -25.0f ), new Vector2( 234.0f, 35.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ), new Vector2( 0.0f, 1.0f ) ), Levels.LevelDataManager.factions[u.factionId].color, u.healthPercent, SSObjectDFC.GetHealthString( u.health, u.healthMax ) );
			SelectionPanel.instance.obj.RegisterElement( "unit.health", healthUI.transform );

			if( !u.IsDisplaySafe() )
			{
				return;
			}

			bool blockManual = false;
			if( u.isCivilian )
			{
				if( !u.civilian.isEmployed )
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

			ActionPanel.instance.CreateButton( "civilian.autoduty", s, "Toggle automatic duty", "Makes the civilian deliver resources to where they are needed.",
				ActionButtonAlignment.LowerLeft, ActionButtonType.Object, () =>
			{
				cue.SetAutomaticDuty( !cue.isOnAutomaticDuty );
			} );
		}

		public static void CreateEmployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.employ", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/employ" )
				, "Employ civilian", "Makes the civilian employed at a specified workplace.", ActionButtonAlignment.LowerRight, ActionButtonType.Object, () =>
				{
					InputOverrideEmployment.EnableEmploymentInput( cue );
				} );
		}

		public static void CreateUnemployButton( CivilianUnitExtension cue )
		{
			ActionPanel.instance.CreateButton( "civilian.unemploy", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/unemploy" )
				, "Fire civilian", "Makes the civilian unemployed.", ActionButtonAlignment.LowerRight, ActionButtonType.Object, () =>
				{
					cue.workplace.UnEmploy( cue );
				} );
		}

		public static void CreateQueryButtons()
		{
			ActionPanel.instance.CreateButton( "unit.ap.dropoff", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/dropoff" )
			, "Drop off resources", "Select storage to drop off the resources at. OR, deliver resources to construction sites, barracks, etc.",
			ActionButtonAlignment.UpperLeft, ActionButtonType.Object, () =>
			{
				InputOverrideDropOffQuery.EnableInput();
			} );

			ActionPanel.instance.CreateButton( "unit.ap.pickup", AssetManager.GetSprite( AssetManager.BUILTIN_ASSET_ID + "Textures/pickup" )
			, "Pick up resources", "Select storage or resource deposit to pick the resources up from.", ActionButtonAlignment.UpperRight, ActionButtonType.Object, () =>
			{
				InputOverridePickUpQuery.EnableInput();
			} );
		}
	}
}