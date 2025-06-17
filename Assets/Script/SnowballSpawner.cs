using UnityEngine;
using UnityEngine.InputSystem;

public class SnowballSpawner : MonoBehaviour
{
    public GameObject snowballPrefab;
    public Transform controllerTransform;
    public InputActionProperty triggerAction;
    public float maxDistance = 10f;
    public LayerMask snowLayerMask;

    private bool wasTriggerPressed = false;
    private GameObject currentSnowball;

    void OnEnable()
    {
        triggerAction.action.Enable();
    }

    void Update()
    {
        float trigger = triggerAction.action.ReadValue<float>();

        if (trigger > 0.5f && !wasTriggerPressed)
        {
            wasTriggerPressed = true;
            SpawnSnowball();
        }

        if (trigger > 0.5f && currentSnowball != null)
        {
            UpdateSnowballTarget();
        }

        if (trigger < 0.1f && wasTriggerPressed)
        {
            wasTriggerPressed = false;
            if (currentSnowball != null)
            {
                currentSnowball.GetComponent<Snowball>().followTarget = null;
                currentSnowball = null;
            }
        }
    }

    void SpawnSnowball()
    {
        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, snowLayerMask))
        {
            if (hit.collider.CompareTag("Snowman"))
            {
                Debug.Log("눈사람 위엔 생성하지 않음");
                return;
            }

            Vector3 spawnPos = hit.point + Vector3.up * 0.3f;
            currentSnowball = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.Log("Raycast 실패");
        }
    }

    void UpdateSnowballTarget()
    {
        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, snowLayerMask))
        {
            Snowball snowball = currentSnowball.GetComponent<Snowball>();
            if (snowball != null)
            {
                snowball.followTarget = hit.point;
            }
        }
    }
}
