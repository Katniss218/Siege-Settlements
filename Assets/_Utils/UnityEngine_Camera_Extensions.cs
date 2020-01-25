
namespace UnityEngine
{
	public static class _UnityEngine_Camera_Extensions
	{
		public static bool InContainedScreen( this Camera camera, Vector3 world, Vector2 screen1, Vector2 screen2 )
		{
			Vector3 posScreen = camera.WorldToScreenPoint( world );

			Vector2 min = new Vector3(
				screen1.x < screen2.x ? screen1.x : screen2.x,
				screen1.y < screen2.y ? screen1.y : screen2.y
				 );

			Vector2 max = new Vector3(
				screen1.x > screen2.x ? screen1.x : screen2.x,
				screen1.y > screen2.y ? screen1.y : screen2.y
				 );

			if( posScreen.x < min.x || posScreen.y < min.y )
			{
				return false;
			}
			if( posScreen.x > max.x || posScreen.y > max.y )
			{
				return false;
			}
			return true;
		}
	}
}
