using UnityEngine;

public class EnemySFX : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip death;

    private void Awake()
    {
        if (!source)
            source = GetComponent<AudioSource>();
    }

    public void PlayHit()
    {
        source.PlayOneShot(hit);
    }

    public void PlayDeath()
    {
        source.PlayOneShot(death);
    }
}
