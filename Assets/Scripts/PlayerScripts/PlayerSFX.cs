using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource source;

    [Header("Clips")]
    [SerializeField] private AudioClip walk;
    [SerializeField] private AudioClip attack;
    [SerializeField] private AudioClip hit;

    private bool isWalking;

    private void Awake()
    {
        if (!source)
            source = GetComponent<AudioSource>();
    }

    public void PlayAttack()
    {
        source.PlayOneShot(attack);
    }

    public void PlayHit()
    {
        source.PlayOneShot(hit);
    }

    public void SetWalking(bool walking)
    {
        if (walking && !isWalking)
        {
            source.clip = walk;
            source.loop = true;
            source.Play();
            isWalking = true;
        }
        else if (!walking && isWalking)
        {
            source.Stop();
            isWalking = false;
        }
    }
}
