using System;
using OneInside.Constants;
using QFSW.QC;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputReader), typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public Transform MainCameraTransform { get; private set; }
    [field: SerializeField] public CinemachineInputAxisController AxisController { get; private set; }
    [field: SerializeField] public PlayerAnimation PlayerAnimation { get; private set; }

    [Header("Movement Settings")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = DefaultPlayerConfig.Movement.WALK_SPEED;
    [field: SerializeField] public float RunSpeed { get; private set; } = DefaultPlayerConfig.Movement.RUN_SPEED;
    [field: SerializeField] public float RotationSpeed { get; private set; } = DefaultPlayerConfig.Movement.ROTATION_SPEED;
    [field: SerializeField] public float TurnSmoothTime { get; private set; } = DefaultPlayerConfig.Movement.TURN_SMOOTH_TIME;
    [field: SerializeField] public float JumpHeight { get; private set; } = DefaultPlayerConfig.Movement.JUMP_HEIGHT;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = DefaultPlayerConfig.Movement.GRAVITY;
    [SerializeField] private float groundedGravity = DefaultPlayerConfig.Movement.GROUNDED_GRAVITY;
    [SerializeField] private float maxDownSpeed = DefaultPlayerConfig.Movement.MAX_DOWN_SPEED;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = DefaultPlayerConfig.Movement.GROUND_CHECK_DISTANCE;

    //--------------------------------------
    // Private Variables
    //--------------------------------------
    private float moveSpeed;
    private float turnSmoothVelocity;
    private float verticalVelocity;

    private Animator playerAnimator;
    private bool isGrounded;

    private PlayerState playerState;

    private PlayerSystem playerManager;

    void Awake()
    {
        playerManager = OneInsideLevelSystem.Instance.PlayerManager;
    }

    void OnEnable()
    {
        verticalVelocity = CharacterController.velocity.y;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        playerAnimator = GetComponent<Animator>();
        playerState = GetComponent<PlayerState>();

        MainCameraTransform = Camera.main.transform;
        moveSpeed = WalkSpeed;
        InputReader.SprintEvent += Sprint;
        InputReader.JumpEvent += Jump;
        if (!playerManager)
            return;
        playerManager.OnEnableAllPlayersMovement += EnablePlayerMovement;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (!playerState.IsCanMove())
        {
            PlayerAnimation.ResetAnimation();
            return;
        }


        isGrounded = CheckGrounded();
        playerAnimator.SetBool("Floating", !isGrounded);

        Vector3 moveInput = new Vector3(InputReader.MovementValue.x, 0f, InputReader.MovementValue.y);
        UpdateMovementAnimation(moveInput);
        ApplyGravity();
    }

    private void UpdateMovementAnimation(Vector3 moveInput)
    {
        bool isMoving = moveInput != Vector3.zero;

        if (isMoving)
        {
            Vector3 targetDirection = Quaternion.Euler(0f, MainCameraTransform.eulerAngles.y, 0f) * moveInput;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            CharacterController.Move(moveDirection * moveSpeed * Time.deltaTime);

            playerState.SetWalking(true);
        }
        else
        {
            playerState.SetWalking(false);
        }
    }

    private void Sprint(bool sprint)
    {
        if (!IsOwner)
            return;

        moveSpeed = sprint ? RunSpeed : WalkSpeed;
        playerState.SetRunning(sprint);

    }

    private void Jump(bool value)
    {
        if (!IsOwner || !isGrounded || !playerState.IsCanMove() || value == false)
            return;

        verticalVelocity = JumpHeight;
        playerAnimator.SetTrigger("Jump");

        playerState.SetWalking(false);
        playerState.SetRunning(false);
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance)
            || CharacterController.isGrounded
            || math.abs(verticalVelocity) < 0.1f;
        ;
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, maxDownSpeed);
        }

        Vector3 verticalMovement = new Vector3(0f, verticalVelocity, 0f);
        CharacterController.Move(verticalMovement * Time.deltaTime);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;
        InputReader.SprintEvent -= Sprint;
        InputReader.JumpEvent -= Jump;
        if (!playerManager)
            return;
        playerManager.OnEnableAllPlayersMovement -= EnablePlayerMovement;
    }

    public void EnablePlayerMovement(bool shouldMove)
    {
        if (!shouldMove)
        {
            InputReader.DisableGameplayInput();
        }
        else
        {
            InputReader.EnableGameplayInput();
        }
        if (AxisController)
            AxisController.enabled = shouldMove;
    }

    public void SetWalkSpeed(float speed)
    {
        WalkSpeed = speed;
        Sprint(false);
    }

    public void SetRunSpeed(float speed)
    {
        RunSpeed = speed;
        Sprint(true);
    }
    

    private void OnDrawGizmos()
    {
        DrawCheckGround();
    }

    private void DrawCheckGround()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}
