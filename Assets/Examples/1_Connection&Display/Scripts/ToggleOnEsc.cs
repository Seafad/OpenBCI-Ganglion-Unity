using UnityEngine;

public class ToggleOnEsc : MonoBehaviour {

    public GameObject target;

	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            target.SetActive(!target.activeSelf);
        }
	}
}
