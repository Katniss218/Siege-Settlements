using System.Collections.Generic;
using UnityEngine;

namespace SS.Extras
{
	public class Extra : MonoBehaviour
	{
		private static List<Extra> _allExtras = new List<Extra>();

		public static Extra[] GetAllExtras()
		{
			return _allExtras.ToArray();
		}

		public string defId { get; set; }


		void OnEnable()
		{
			_allExtras.Add( this.gameObject );
		}

		void OnDisable()
		{
			_allExtras.Remove( this.gameObject );
		}
	}
}