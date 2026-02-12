using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;

    private TextMeshProUGUI text;
    private float timer;
    private bool initialized;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
        {
            Debug.LogError("DamageNumber: No TextMeshPro found in children!", this);
            enabled = false;
        }
    }

    public void Init(int amount, Color color)
    {
        if (text == null) return;

        text.text = amount.ToString();
        text.color = color;
        timer = 0f;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || text == null) return;

        Vector3 drift = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f);
        transform.position += drift * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);

        var c = text.color;
        c.a = alpha;
        text.color = c;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
