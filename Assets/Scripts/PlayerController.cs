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
    [SerializeField] float mouseSencitivity;    // 민감도.

    [Range(1.0f, 4.0f)]
    [SerializeField] float gravityScale;    // 중력 가속도.
    [SerializeField] float moveSpeed;       // 걷는 속도.
    [SerializeField] float runSpeed;        // 달리는 속도.
    [SerializeField] float jumpHeight;      // 점프 높이.

    Gun[] weapons;             // 현재 장비중인 무기 배열.
    int gunIndex;                   // 장비중인 무기의 indesx.

    CharacterController controller;         // 캐릭터 컨트롤러.
    Vector3 verticalVelocity;               // 수직 속력(=중력 가속도)
    bool isGrounded;                        // 땅에 서 있는가?
    float groundTimer;                      // 땅에서 떨어지는 시간.
    float horizontalAngle;                  // 수평 회전 값.
    float verticalAngle;                    // 수직 회전 값.

    bool isLock;                            // 플레이어 제어 잠금.
    bool isPause;                           // 게임 일시정지.

    // parametor
    public float currentSpeed { get; private set; }
    public Vector2 direction { get; private set; }
    public float gravity => GRAVITY * gravityScale;


    private void Start()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        controller = GetComponent<CharacterController>();

        // 기본 소지 무기 초기화.
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

        // 0번째 무기 선택.
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

        // 사용자의 키 입력을 받아 방향을 구한다. 이때 벡터의 크기가 1을 초과하면 정규화(=크기를 1로) 한다.
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (dir.sqrMagnitude > 1)
            dir.Normalize();

        dir = transform.TransformDirection(dir);                        // transform의 정면(=로컬)을 기준으로 방향을 수정한다.
        controller.Move(dir * finalSpeed * Time.deltaTime);             // 컨트롤러에게 이동하라고 명령한다.
        currentSpeed = dir.sqrMagnitude;                                // 현재 속력 계산.

        // horizontal turn.
        float turnAngle = Input.GetAxis("Mouse X") * mouseSencitivity;  // 사용자의 수직 회전 값.
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

        // 무기 전환.
        float wheel = Input.GetAxisRaw("Mouse ScrollWheel");
        if(wheel != 0)
        {
            int wasIndex = gunIndex;
            do
            {
                // 같은 번호가 나오거나 null이 아닌 무기가 나올때까지 loop.
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

        // 공격.
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
