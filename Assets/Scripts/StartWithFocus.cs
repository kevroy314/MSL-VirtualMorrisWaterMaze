using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartWithFocus : MonoBehaviour {

    public InputField field;

	// Use this for initialization
	void Start () {
        field.Select();
        field.ActivateInputField();

    }
}
