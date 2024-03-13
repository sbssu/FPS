using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    const float GRAVITY = -9.81f;

    [Header("Objeect")]
    [SerializeField] Transform cam;
    [SerializeField] WeaponController[] defaultWeapons;

    [Header("Status")]
    [Range(1.0f, 10.0f)]
    [SerializeField] float mouseSencitivity;    // �ΰ���.

    [Range(1.0f, 4.0f)]
    [SerializeField] float gravityScale;    // �߷� ���ӵ�.
    [SerializeField] float moveSpeed;       // �ȴ� �ӵ�.
    [SerializeField] float runSpeed;        // �޸��� �ӵ�.
    [SerializeField] float jumpHeight;      // ���� ����.

    WeaponController[] weapons;             // ���� ������� ���� �迭.

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
        weapons = new WeaponController[3];
        for (int i = 0; i < defaultWeapons.Length; i++)
        {
            weapons[i] = defaultWeapons[i];
            if (weapons[i] != null)
                weapons[i].Setup(this);
        }
    }

    void Update()
    {
        CheckGround();
        Movement();
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
    private void Gravity()
    {
        // gravity.
        verticalVelocity.y += gravity * Time.deltaTime;
        CollisionFlags flag = controller.Move(verticalVelocity * Time.deltaTime);
        if ((flag & CollisionFlags.Below) != 0)
            verticalVelocity.y = 0f;
    }
}
