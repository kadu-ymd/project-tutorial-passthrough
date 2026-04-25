using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Default sound played when hitting a StalkerCube (used if the stalker has no sound assigned)")]
    public AudioClip defaultHitSound;

    [Tooltip("Minimum impact velocity to trigger a hit")]
    public float minImpactVelocity = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        StalkerObject stalker = collision.collider.GetComponent<StalkerObject>();
        bool isTargetBlock = collision.gameObject.name.Contains("TargetBlock");

        if (stalker != null || isTargetBlock)
        {
            if (stalker != null && collision.relativeVelocity.magnitude >= minImpactVelocity)
            {
                AudioClip soundToPlay = stalker.hitSound != null ? stalker.hitSound : defaultHitSound;
                if (soundToPlay != null)
                {
                    AudioSource.PlayClipAtPoint(soundToPlay, collision.contacts[0].point);
                }

                Destroy(stalker.gameObject);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(collision.transform.position);
            }

            Destroy(gameObject, 0.05f);
        }
    }
}
