using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class Fists : MonoBehaviour
{
    [Header("Arm References")]
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;

    [Header("Punch Movement")]
    [SerializeField, Min(0f)] private float punchDistance = 0.35f;
    [SerializeField, Min(0.01f)] private float punchForwardDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float punchReturnDuration = 0.12f;
    [SerializeField, Min(0f)] private float cooldown = 0.35f;

    [Header("Damage")]
    [SerializeField, Min(0)] private int damage = 5;
    [SerializeField, Min(0f)] private float punchRange = 1.7f;
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Camera attackCamera;

    private Vector3 _leftArmStartLocalPosition;
    private Vector3 _rightArmStartLocalPosition;
    private bool _useLeftArm = true;
    private bool _isPunching;
    private bool _isEquipped = true;
    private float _nextAttackTime;

    private void Awake()
    {
        CacheArmPositions();
        ResolveAttackReferences();
    }

    public void SetEquipped(bool isEquipped)
    {
        _isEquipped = isEquipped;
    }

    public bool Attack()
    {
        if (!_isEquipped || _isPunching || Time.time < _nextAttackTime)
            return false;

        if (!HasRequiredReferences())
            return false;

        Transform arm = _useLeftArm ? leftArm : rightArm;
        Vector3 startPosition = _useLeftArm ? _leftArmStartLocalPosition : _rightArmStartLocalPosition;
        _useLeftArm = !_useLeftArm;

        ApplyPunchDamage();
        StartCoroutine(PunchRoutine(arm, startPosition));
        return true;
    }

    private bool HasRequiredReferences()
    {
        if (leftArm != null && rightArm != null)
            return true;

        Debug.LogWarning("Fists: faltan referencias. Asigna Left Arm y Right Arm en el Inspector.", this);
        return false;
    }

    private void CacheArmPositions()
    {
        if (leftArm != null)
            _leftArmStartLocalPosition = leftArm.localPosition;

        if (rightArm != null)
            _rightArmStartLocalPosition = rightArm.localPosition;
    }

    private void ResolveAttackReferences()
    {
        if (attackCamera == null)
            attackCamera = Camera.main;

        if (attackOrigin == null && attackCamera != null)
            attackOrigin = attackCamera.transform;
    }

    private IEnumerator PunchRoutine(Transform arm, Vector3 startPosition)
    {
        _isPunching = true;
        _nextAttackTime = Time.time + cooldown;

        Vector3 endPosition = startPosition + Vector3.forward * punchDistance;

        yield return MoveArm(arm, startPosition, endPosition, punchForwardDuration);
        yield return MoveArm(arm, endPosition, startPosition, punchReturnDuration);

        arm.localPosition = startPosition;
        _isPunching = false;
    }

    private IEnumerator MoveArm(Transform arm, Vector3 from, Vector3 to, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float time = Mathf.Clamp01(elapsedTime / duration);
            float smoothTime = Mathf.SmoothStep(0f, 1f, time);

            arm.localPosition = Vector3.LerpUnclamped(from, to, smoothTime);
            yield return null;
        }

        arm.localPosition = to;
    }

    private void ApplyPunchDamage()
    {
        ResolveAttackReferences();

        Ray ray = GetPunchRay();
        if (!Physics.Raycast(ray, out RaycastHit hit, punchRange, enemyMask, QueryTriggerInteraction.Collide))
            return;

        if (hit.transform.IsChildOf(transform.root))
            return;

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        damageable ??= hit.collider.GetComponentInChildren<IDamageable>();

        if (damageable == null)
            return;

        Debug.Log($"Fists golpeo a {hit.collider.name} (-{damage})");

        if (EventQueueManager.instance != null)
            EventQueueManager.instance.AddCommand(new CmdApplyDamage(damageable, damage));
        else
            damageable.ApplyDamage(damage);
    }

    private Ray GetPunchRay()
    {
        if (attackCamera != null)
            return attackCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Transform origin = attackOrigin != null ? attackOrigin : transform;
        return new Ray(origin.position, origin.forward);
    }
}
