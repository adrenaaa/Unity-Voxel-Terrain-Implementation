using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private CharacterController cc;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float speed;
    [SerializeField] private float moveAccel;
    [SerializeField] private Vector2 sens;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpVel = 5f;

    private float mouseX, mouseY;

    private Vector3 forward {
        get {
            return new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        }
    }

    private Vector3 right {
        get {
            return new Vector3(cam.right.x, 0f, cam.right.z).normalized;
        }
    }

    private Vector3 grav;
    private Vector3 moveDir, moveVel;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        Look();
        Move();
        Gravity();
        Jump();

        cam.position = camTarget.position;
    }

    private void Jump() {
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded) {
            grav.y = jumpVel;
        }
    }

    private void Gravity() {
        if (cc.isGrounded) {
            grav = Vector3.zero;
        }
        else {
            grav.y += gravity * Time.deltaTime;
            cc.Move(grav * Time.deltaTime);
        }
    }

    private void Look() {
        mouseX += Input.GetAxisRaw("Mouse X") * sens.x;
        mouseY -= Input.GetAxisRaw("Mouse Y") * sens.y;

        mouseY = Mathf.Clamp(mouseY, -89f, 89f);

        Quaternion rot = Quaternion.Euler(mouseY, mouseX, 0f);

        cam.rotation = rot;
    }

    private void Move() {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 dir = forward * y + right * x;
        moveDir = Vector3.SmoothDamp(moveDir, dir, ref moveVel, moveAccel);

        cc.Move(moveDir * speed * Time.deltaTime);
    }
}