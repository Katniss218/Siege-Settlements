using System;
using UnityEngine;

public class EveryFrameSingle : MonoBehaviour
{
	public Action everyFrame;
	
    void Update()
    {
		everyFrame();
    }
}
