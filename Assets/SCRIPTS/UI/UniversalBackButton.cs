using UnityEngine;

namespace SS.UI
{
	public class UniversalBackButton : MonoBehaviour
	{
		public GameObject panel;

		public void _Trigger()
		{
			Object.Destroy( this.panel );
		}
	}
}