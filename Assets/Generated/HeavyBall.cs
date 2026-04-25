using UnityEngine;

public class HeavyBall : MonoBehaviour
{
    [Tooltip("Multiplier for gravity. 1 = normal gravity, 0.5 = half gravity, 0 = no gravity")]
    public float gravityScale = 0.3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Apply custom gravity as acceleration (independent of mass)
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
    }
}
