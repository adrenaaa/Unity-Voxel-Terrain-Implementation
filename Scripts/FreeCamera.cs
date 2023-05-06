using UnityEngine;

public class FreeCamera : MonoBehaviour {
    public float moveSpeed;
    public Vector2 sens;

    float mouseX, mouseY;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        Move();
        Look();
    }

    void Move() {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 dir = transform.right * x + transform.forward * y;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Look() {
        mouseX += Input.GetAxisRaw("Mouse X") * sens.x;
        mouseY -= Input.GetAxisRaw("Mouse Y") * sens.y;

        mouseY = Mathf.Clamp(mouseY, -89f, 89f);

        Quaternion rot = Quaternion.Euler(mouseY, mouseX, 0f);

        transform.rotation = rot;
    }
}