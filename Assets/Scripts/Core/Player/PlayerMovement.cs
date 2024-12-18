using System;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(InputReader), typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public Transform MainCameraTransform { get; private set; }

    [Header("Movement Settings")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = 3f;
    [field: SerializeField] public float RunSpeed { get; private set; } = 6f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 15f;
    [field: SerializeField] public float TurnSmoothTime { get; private set; } = .1f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = 1f;
    [SerializeField] private float groundedGravity = -0.5f;

    private float moveSpeed;
    private float turnSmoothVelocity;
    private float verticalVelocity;
    private readonly float terminalVelocity = -53f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        MainCameraTransform = Camera.main.transform;
        moveSpeed = WalkSpeed;
        InputReader.SprintEvent += Sprint;
    }

    private void Update()
    {
        if (!IsOwner) return;
        Move();
        ApplyGravity();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        InputReader.SprintEvent -= Sprint;
    }

    private void Sprint(bool shouldSprint)
    {
        if (!IsOwner) return;
        moveSpeed = shouldSprint ? RunSpeed : WalkSpeed;
    }

    private void Move()
    {
        Vector3 direction = new Vector3(InputReader.MovementValue.x, 0f, InputReader.MovementValue.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + MainCameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            CharacterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (CharacterController.isGrounded)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }
        Vector3 verticalMovement = new Vector3(0f, verticalVelocity, 0f);
        CharacterController.Move(verticalMovement * Time.deltaTime);
    }
}