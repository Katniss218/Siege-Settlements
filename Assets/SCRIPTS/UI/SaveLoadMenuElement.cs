using UnityEngine;

namespace SS.UI
{
	public class SaveLoadMenuElement : MonoBehaviour
	{
		public string levelId;
		public string levelSaveStateId;

		[HideInInspector]
		public TMPro.TMP_InputField levelNameInput;
		[HideInInspector]
		public TMPro.TMP_InputField levelSaveStateNameInput;


		public void _Trigger()
		{
			this.levelNameInput.text = this.levelId;
			this.levelSaveStateNameInput.text = this.levelSaveStateId;
		}
	}
}