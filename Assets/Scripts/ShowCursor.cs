using UnityEngine;
using System.Collections;

public class ShowCursor : MonoBehaviour {

    public bool show = true;
	
	// Update is called once per frame
	void Update () {
        Cursor.visible = show;
        if(show)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }
}
