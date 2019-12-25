using SS.Objects.Modules;

namespace SS.Objects
{
	public class SSObjectHelper
	{
		public static void ReDisplayDisplayed()
		{
			SSObjectDFS displayed = Selection.displayedObject;
			SSModule displayedModule = Selection.displayedModule;
			if( displayed != null )
			{
				Selection.StopDisplaying();
				Selection.DisplayObject( displayed );
				if( displayedModule != null )
				{
					Selection.DisplayModule( displayed, displayedModule );
				}
			}
		}
	}
}