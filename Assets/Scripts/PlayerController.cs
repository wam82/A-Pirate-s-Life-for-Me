
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public float moveSpeed;  // Forward movement speed
    [SerializeField] public float rotationSpeed;  // Turning speed
    [SerializeField] public Transform cameraPivot;  // Assign an empty GameObject as pivot

    private float _moveInput;
    private float _turnInput;

    void Update()
    {
        // Get movement input
        _moveInput = Input.GetAxis("Vertical");  // W/S or Up/Down Arrow
        _turnInput = Input.GetAxis("Horizontal");  // A/D or Left/Right Arrow

        // Move forward or backward
        transform.Translate(Vector3.forward * (_moveInput * moveSpeed * Time.deltaTime));

        // Rotate the ship left/right
        transform.Rotate(Vector3.up * (_turnInput * rotationSpeed * Time.deltaTime));
    }
}
