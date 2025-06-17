// ✅ Snowball.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))] // XR Grab 기능
public class Snowball : MonoBehaviour
{
    public float growthRate = 0.04f; // 1초당 4cm 성장
    public float maxSize = 2.0f;
    public GameObject snowmanPrefab;

    [HideInInspector]
    public bool isGrowing = true;
    [HideInInspector]
    public Vector3? followTarget = null;

    private Rigidbody rb;
    private float initialScale = 0.1f;
    private float requiredSpeed = 0.2f;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.localScale = Vector3.one * initialScale;
        rb.mass = 1f;
        rb.isKinematic = false;
        rb.useGravity = true;

        // XRGrab 세팅
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.throwOnDetach = true;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;

        grabInteractable.selectExited.AddListener(OnRelease); // ❗ 던지기 이벤트 연결
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void FixedUpdate()
    {
        if (followTarget.HasValue && !grabInteractable.isSelected)
        {
            Vector3 direction = (followTarget.Value - transform.position);
            float distance = direction.magnitude;
            Vector3 targetVelocity = direction.normalized * Mathf.Clamp(distance * 5f, 0f, 5f);

            rb.velocity = targetVelocity;
        }
        else if (grabInteractable.isSelected)
        {
            // XR로 잡힌 상태에서는 이동 중지
            followTarget = null;
            rb.velocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (!isGrowing || grabInteractable.isSelected) return; // 잡힌 상태에선 성장 중단
        if (transform.localScale.x >= maxSize) return;

        bool onSnow = IsTouchingSnow();
        float speed = rb.velocity.magnitude;

        if (onSnow && speed > requiredSpeed)
        {
            float scaleRatio = transform.localScale.x / initialScale;
            float growth = growthRate * scaleRatio * Time.deltaTime;
            transform.localScale += Vector3.one * growth;
            rb.mass = transform.localScale.x * 10f;
        }
    }

    bool IsTouchingSnow()
    {
        return Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f)
            && hit.collider.CompareTag("Snow");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Snowball"))
        {
            float mySize = transform.localScale.x;
            float otherSize = collision.transform.localScale.x;

            if (mySize >= 1.5f && otherSize >= 1.0f)
            {
                Vector3 center = (transform.position + collision.transform.position) / 2f;

                if (Physics.Raycast(center + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
                {
                    if (snowmanPrefab == null)
                    {
                        Debug.LogError("❌ snowmanPrefab 연결 안됨");
                        return;
                    }

                    Vector3 spawnPos = new Vector3(center.x, 0.8f, center.z);  // ✅ 추가 보정 없음!
                    Instantiate(snowmanPrefab, spawnPos, Quaternion.identity);
                    Destroy(gameObject);
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.LogWarning("바닥 인식 실패");
                }
            }
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // 던지기 시 followTarget 비활성화 유지
        followTarget = null;
        isGrowing = false;
        Debug.Log("🎯 눈덩이 던져짐");
    }
}
