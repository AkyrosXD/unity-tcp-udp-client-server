using UnityEngine;

public class MouseLook : MonoBehaviour
{
    float rotx = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
        }
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X") * 500 * Time.deltaTime;
            float my = Input.GetAxis("Mouse Y") * 500 * Time.deltaTime;
            rotx -= my;
            rotx = Mathf.Clamp(rotx, -90, 90);
            transform.localRotation = Quaternion.Euler(rotx, 0, 0);
            transform.parent.Rotate(Vector3.up * mx);
        }
    }
}
