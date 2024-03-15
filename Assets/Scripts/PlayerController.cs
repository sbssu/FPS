using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    const float GRAVITY = -9.81f;

    [Header("Objeect")]
    [SerializeField] Transform cam;
    [SerializeField] Gun[] defaultWeapons;

    [Header("Status")]
    [Range(1.0f, 10.0f)]
    [SerializeField] float mouseSencitivity;    // �ΰ���.

    [Range(1.0f, 4.0f)]
    [SerializeField] float gravityScale;    // �߷� ���ӵ�.
    [SerializeField] float moveSpeed;       // �ȴ� �ӵ�.
    [SerializeField] float runSpeed;        // �޸��� �ӵ�.
    [SerializeField] float jumpHeight;      // ���� ����.

    Gun[] weapons;             // ���� ������� ���� �迭.
    int gunIndex;                   // ������� ������ indesx.

    CharacterController controller;         // ĳ���� ��Ʈ�ѷ�.
    Vector3 verticalVelocity;               // ���� �ӷ�(=�߷� ���ӵ�)
    bool isGrounded;                        // ���� �� �ִ°�?
    float groundTimer;                      // ������ �������� �ð�.
    float horizontalAngle;                  // ���� ȸ�� ��.
    float verticalAngle;                    // ���� ȸ�� ��.

    bool isLock;                            // �÷��̾� ���� ���.
    bool isPause;                           // ���� �Ͻ�����.

    // parametor
    public float currentSpeed { get; private set; }
    public Vector2 direction { get; private set; }
    public float gravity => GRAVITY * gravityScale;


    private void Start()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        controller = GetComponent<CharacterController>();

        // �⺻ ���� ���� �ʱ�ȭ.
        weapons = new Gun[3];
        for (int i = 0; i < defaultWeapons.Length; i++)
        {
            weapons[i] = defaultWeapons[i];
            if (weapons[i] != null)
            {
                weapons[i].Setup(this);
                weapons[i].gameObject.SetActive(false);
            }
        }

        gunIndex = 0;

        // 0��° ���� ����.
        weapons[gunIndex].Pickup();
    }

    void Update()
    {
        CheckGround();
        Movement();
        Controller();
        Gravity();                
    }

    private void CheckGround()
    {
        // check ground.
        if (!controller.isGrounded)
        {
            if (isGrounded && (groundTimer += Time.deltaTime) >= 0.25f)
                isGrounded = false;
        }
        else
        {
            groundTimer = 0.0f;
            isGrounded = true;
        }
    }
    private void Movement()
    {
        if(isPause || isLock)
        {
            currentSpeed = 0f;
            return;
        }

        // movement.
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float finalSpeed = isRunning ? runSpeed : moveSpeed;

        // ������� Ű �Է��� �޾� ������ ���Ѵ�. �̶� ������ ũ�Ⱑ 1�� �ʰ��ϸ� ����ȭ(=ũ�⸦ 1��) �Ѵ�.
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (dir.sqrMagnitude > 1)
            dir.Normalize();

        dir = transform.TransformDirection(dir);                        // transform�� ����(=����)�� �������� ������ �����Ѵ�.
        controller.Move(dir * finalSpeed * Time.deltaTime);             // ��Ʈ�ѷ����� �̵��϶�� ����Ѵ�.
        currentSpeed = dir.sqrMagnitude;                                // ���� �ӷ� ���.

        // horizontal turn.
        float turnAngle = Input.GetAxis("Mouse X") * mouseSencitivity;  // ������� ���� ȸ�� ��.
        horizontalAngle += turnAngle;
        if (horizontalAngle > 360)
            horizontalAngle -= 360;
        else if (horizontalAngle < 0)
            horizontalAngle += 360;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, horizontalAngle, transform.localEulerAngles.z);

        // vertical turn.
        turnAngle = Input.GetAxis("Mouse Y") * -1f;
        verticalAngle = Mathf.Clamp(verticalAngle + turnAngle, -89f, 89f);
        cam.localEulerAngles = new Vector3(verticalAngle, cam.localEulerAngles.y, cam.localEulerAngles.z);

        // jump.
        float gravity = GRAVITY * gravityScale;
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGrounded = false;
        }
    }
    private void Controller()
    {
        if (isPause || isLock)
            return;

        // ���� ��ȯ.
        float wheel = Input.GetAxisRaw("Mouse ScrollWheel");
        if(wheel != 0)
        {
            int wasIndex = gunIndex;
            do
            {
                // ���� ��ȣ�� �����ų� null�� �ƴ� ���Ⱑ ���ö����� loop.
                gunIndex += (wheel > 0) ? -1 : 1;
                if (gunIndex < 0)
                    gunIndex = weapons.Length - 1;
                else if (gunIndex >= weapons.Length)
                    gunIndex = 0;
                if (gunIndex == wasIndex)
                    break;

            } while (weapons[gunIndex] == null);

            if(gunIndex != wasIndex)
            {
                weapons[wasIndex].PutAway();
                weapons[gunIndex].Pickup();
            }
        }

        // ����.
        weapons[gunIndex].IsTriggerDown = Input.GetButton("Fire");
        if (Input.GetKeyDown(KeyCode.R))
            weapons[gunIndex].Reload();
    }
    private void Gravity()
    {
        // gravity.
        verticalVelocity.y += gravity * Time.deltaTime;
        CollisionFlags flag = controller.Move(verticalVelocity * Time.deltaTime);
        if ((flag & CollisionFlags.Below) != 0)
            verticalVelocity.y = 0f;
    }
}
