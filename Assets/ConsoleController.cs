using SS.InputSystem;
using UnityEngine;

namespace SS
{
	public class ConsoleController : MonoBehaviour
	{
		[SerializeField] private GameObject console = null;
		
		void Inp_Tilde( InputQueue self )
		{
			Debug.Log( "A" );
			console.SetActive( !console.activeSelf );
		}
		
		void OnEnable()
		{
			Main.keyboardInput.RegisterOnPress( KeyCode.BackQuote, -666.69f, Inp_Tilde, true );
		}
		
		void OnDisable()
		{
			Main.keyboardInput.ClearOnPress( KeyCode.BackQuote, Inp_Tilde );
		}
	}
}