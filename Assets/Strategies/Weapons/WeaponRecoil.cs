using UnityEngine;

[DisallowMultipleComponent]
public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField, Min(0f)] private float backwardKick = 0.08f;
    [SerializeField, Min(0f)] private float upwardRotation = 5f;
    [SerializeField, Min(0f)] private float sideRotation = 1f;

    [Header("Smoothing")]
    [SerializeField, Min(0.01f)] private float kickSnappiness = 28f;
    [SerializeField, Min(0.01f)] private float returnSpeed = 14f;

    private Vector3 _restLocalPosition;
    private Quaternion _restLocalRotation;
    private Vector3 _currentPositionOffset;
    private Vector3 _targetPositionOffset;
    private Vector3 _currentRotationOffset;
    private Vector3 _targetRotationOffset;
    private bool _hasRestPose;

    private void OnEnable()
    {
        ResetRestPose();
    }

    private void Update()
    {
        if (!_hasRestPose)
            ResetRestPose();

        _targetPositionOffset = Vector3.Lerp(_targetPositionOffset, Vector3.zero, returnSpeed * Time.deltaTime);
        _targetRotationOffset = Vector3.Lerp(_targetRotationOffset, Vector3.zero, returnSpeed * Time.deltaTime);

        _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _targetPositionOffset, kickSnappiness * Time.deltaTime);
        _currentRotationOffset = Vector3.Lerp(_currentRotationOffset, _targetRotationOffset, kickSnappiness * Time.deltaTime);

        transform.localPosition = _restLocalPosition + _currentPositionOffset;
        transform.localRotation = _restLocalRotation * Quaternion.Euler(_currentRotationOffset);
    }

    public void ResetRestPose()
    {
        _restLocalPosition = transform.localPosition;
        _restLocalRotation = transform.localRotation;
        _currentPositionOffset = Vector3.zero;
        _targetPositionOffset = Vector3.zero;
        _currentRotationOffset = Vector3.zero;
        _targetRotationOffset = Vector3.zero;
        _hasRestPose = true;
    }

    public void ApplyRecoil()
    {
        if (!_hasRestPose)
            ResetRestPose();

        _targetPositionOffset += Vector3.back * backwardKick;
        _targetRotationOffset += new Vector3(-upwardRotation, Random.Range(-sideRotation, sideRotation), 0f);
    }
}
