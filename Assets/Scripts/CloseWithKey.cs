using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CloseWithKey : MonoBehaviour {
    public KeyCode quitKey = KeyCode.Escape;
    public enum CloseMode { Quit = 0, EscapeToScene = 1 };
    public CloseMode closeMode = CloseMode.Quit;
    public int sceneNumber = 0;

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(quitKey))
        {
            CameraFade.SetScreenOverlayColor(Color.black);

            if (closeMode == CloseMode.Quit)
                Application.Quit();
            else if (closeMode == CloseMode.EscapeToScene)
                SceneManager.LoadScene(sceneNumber);
        }
	}
}
