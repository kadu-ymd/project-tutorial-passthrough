using UnityEngine;

public class PinchBallSpawner : MonoBehaviour
{
    [Tooltip("Ball prefab to spawn on pinch")]
    public GameObject ballPrefab;

    [Tooltip("Left hand OVRHand component")]
    public OVRHand leftHand;

    [Tooltip("Right hand OVRHand component")]
    public OVRHand rightHand;

    [Tooltip("Forward offset from the pinch point so the ball doesn't spawn inside the hand")]
    public float spawnForwardOffset = 0.05f;

    private bool _leftWasPinching;
    private bool _rightWasPinching;

    void Update()
    {
        CheckHand(leftHand, ref _leftWasPinching);
        CheckHand(rightHand, ref _rightWasPinching);
    }

    void CheckHand(OVRHand hand, ref bool wasPinching)
    {
        if (hand == null)
            return;

        bool isPinching = hand.IsDataValid && hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        if (isPinching && !wasPinching)
        {
            SpawnBall(hand);
        }

        wasPinching = isPinching;
    }

    void SpawnBall(OVRHand hand)
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("PinchBallSpawner: No ball prefab assigned.");
            return;
        }

        Vector3 spawnPos = hand.IsPointerPoseValid
            ? hand.PointerPose.position
            : hand.transform.position;

        Vector3 forward = hand.IsPointerPoseValid
            ? hand.PointerPose.forward
            : hand.transform.forward;

        spawnPos += forward * spawnForwardOffset;

        Instantiate(ballPrefab, spawnPos, Quaternion.identity);
    }
}
