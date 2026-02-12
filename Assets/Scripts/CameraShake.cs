using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    public static bool enabledGlobal = true;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float intensity = 0.2f, float duration = 0.1f)
    {
        if (!enabledGlobal) return;
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private System.Collections.IEnumerator ShakeRoutine(float i, float d)
    {
        Vector3 start = transform.localPosition;

        float t = 0;
        while (t < d)
        {
            transform.localPosition = start + (Vector3)Random.insideUnitCircle * i;
            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = start;
    }
}
