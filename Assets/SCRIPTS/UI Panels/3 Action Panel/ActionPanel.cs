using UnityEngine;

namespace SS.UI
{
	[DisallowMultipleComponent]
	public class ActionPanel : MonoBehaviour
	{
		public static ActionPanel instance { get; private set; }

		void Awake()
		{
			if( instance != null )
			{
				throw new System.Exception( "There is another action panel active" );
			}
			instance = this;
		}
		
		void Start()
		{

		}
		
		void Update()
		{

		}
	}
}