using UnityEngine;

public class RotateToMouse : MonoBehaviour {

    [SerializeField, Tooltip("Rotation Speed of the player.")] private float rotationSpeed = 5f;

	// Update is called once per frame
	void Update () {
        Rotate();
    }

    /// <summary>
    /// Rotates the player towards the mouse cursor
    /// </summary>
    private void Rotate() {
        Vector3 pointToLookAt = GetMousePosition();
        Vector3 dir = pointToLookAt - transform.position;
        Quaternion rotation = Quaternion.LookRotation(dir);
        Vector3 lookDir = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime).eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(0, lookDir.y, 0));
    }
    /// <summary>
    /// Get the position of the mouse in "World Space" (on a plane at (0, 0, 0))
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetMousePosition() {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLenght = 0;
        if (groundPlane.Raycast(cameraRay, out rayLenght)) {
            return cameraRay.GetPoint(rayLenght);
        }

        return Vector3.zero;
    }
}
