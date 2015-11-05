using UnityEngine;
using System.Collections;

public class MouseControls : MonoBehaviour
{
    public float mouseSensitivityX;
    public float mouseSensitivityY;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        float dx = Input.GetAxis("Mouse X");
        float dy = -Input.GetAxis("Mouse Y");
        
        transform.Rotate(Vector3.up, dx * Time.deltaTime * mouseSensitivityX);
        transform.Rotate(Vector3.right, dy * Time.deltaTime * mouseSensitivityY);
    }
}
