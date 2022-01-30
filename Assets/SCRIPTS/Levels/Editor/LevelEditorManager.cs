using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS.Editor
{
    public class LevelEditorManager : MonoBehaviour
    {
        // Level editor needs to be able to:

        /*
        
        - Load up the level to preview it.
            I'll try just a normal level but paused.
        - Place objects.
        - Edit the parameters of objects.
        - Edit the parameters of the level itself.

        */

        /*
        
        Level editor has several "states"

        - Select - you can select and box-select things
        - Move - you move whatever SINGLE item you have selected.
            Raycast from cursor.
            It'll snap to the ground.
        - Place - you place whatever is selected with default values
        - Remove - you delete things you click on
        - 


        Edit the build preview to enable spawning and moving of objects. (gonna be used in both the spawn and move - spawn just has default data).


        */

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}