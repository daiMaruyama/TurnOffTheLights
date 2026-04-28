using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Rigidbody rb;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;

    [Header("Recovery")]
    [SerializeField] Key _returnToStartKey = Key.R;

    float turnSmoothVelocity;
    Vector3 _initialPosition;
    Quaternion _initialRotation;

    void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[_returnToStartKey].wasPressedThisFrame)
        {
            ReturnToStartPosition();
        }
    }

    void FixedUpdate()
    {
        if (Keyboard.current == null || cameraTransform == null || rb == null) return;

        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;
        if (input == Vector2.zero)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        rb.MovePosition(rb.position + moveDir.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void ReturnToStartPosition()
    {
        transform.SetPositionAndRotation(_initialPosition, _initialRotation);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = _initialPosition;
            rb.rotation = _initialRotation;
        }
    }
}
