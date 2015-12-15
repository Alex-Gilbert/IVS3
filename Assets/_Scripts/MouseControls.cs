using UnityEngine;
using System.Collections;

public class MouseControls : MonoBehaviour
{
    public float mouseSensitivityX;
    public float mouseSensitivityY;

    public static bool rotate = true;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.LeftShift))
	        rotate = false;
	    if (Input.GetKeyUp(KeyCode.LeftShift))
	        rotate = true;

	    if (!rotate) return;

        float dx = Input.GetAxis("Mouse X");
        float dy = -Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.right, dy * Time.deltaTime * mouseSensitivityY);
	    transform.Rotate(Vector3.up, dx*Time.deltaTime*mouseSensitivityX);
    }
}
