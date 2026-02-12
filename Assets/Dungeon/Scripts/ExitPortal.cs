using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CompleteGame();
        }
    }
}
