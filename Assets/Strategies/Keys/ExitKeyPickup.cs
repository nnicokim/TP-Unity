using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ExitKeyPickup : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private ExitDoor _exitDoor;

    [Header("Feedback")]
    [SerializeField] private string _pickupMessage = "Exit key obtained";
    [SerializeField, Min(0f)] private float _pickupMessageDuration = 2.5f;

    [Header("World Pickup Rotation")]
    [SerializeField] private bool _rotate = true;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    [SerializeField, Min(0f)] private float _rotationSpeed = 90f;
    [SerializeField] private Transform _visualRoot;

    private bool _pickedUp;

    public void SetExitDoor(ExitDoor exitDoor)
    {
        _exitDoor = exitDoor;
    }

    private void Awake()
    {
        Collider keyCollider = GetComponent<Collider>();
        if (keyCollider != null)
            keyCollider.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (_pickedUp || !_rotate || _rotationSpeed <= 0f)
            return;

        Vector3 axis = _rotationAxis.sqrMagnitude > 0f ? _rotationAxis.normalized : Vector3.up;
        Transform target = _visualRoot != null ? _visualRoot : transform;
        target.Rotate(axis, _rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other) => TryPickup(other);
    private void OnTriggerStay(Collider other) => TryPickup(other);

    private void TryPickup(Collider other)
    {
        if (_pickedUp || other == null)
            return;

        if (other.GetComponentInParent<PlayerHealth>() == null &&
            other.GetComponentInChildren<PlayerHealth>() == null)
            return;

        _pickedUp = true;

        if (_exitDoor != null)
        {
            _exitDoor.Unlock();
            _exitDoor.ShowTemporaryMessage(_pickupMessage, _pickupMessageDuration);
        }
        else
            Debug.LogWarning("ExitKeyPickup: no hay ExitDoor asignada.", this);

        Debug.Log("Llave de salida obtenida.");
        Destroy(gameObject);
    }
}
