using UnityEngine;

public class SandaLizardFollow : MonoBehaviour
{
    public PlayerController player;          // Reference to player
    public float minimalDistanceBehind = 2f; // Desired distance behind player on z-axis
    public float adjustSpeed = 5f;            // Speed to correct distance if too close/far on Z axis
    public float xOffset = 0f;                // Unique x offset for this lizard relative to player lane
    public float smoothness = 5f;             // Smooth factor for horizontal follow

    private float laneDistance;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set on SandaLizardFollow!");
            enabled = false;
            return;
        }

        laneDistance = player.laneDistance;
    }

    void Update()
    {
        // Calculate target lane X based on player's lane + xOffset
        float baseLaneX = 0f;
        if (player.currentLane == 0)
            baseLaneX = -laneDistance;
        else if (player.currentLane == 2)
            baseLaneX = laneDistance;
        else
            baseLaneX = 0f;

        float targetX = baseLaneX + xOffset;

        // Smoothly follow X position
        float newX = Mathf.Lerp(transform.position.x, targetX, smoothness * Time.deltaTime);

        // Target Z should be minimalDistanceBehind behind player
        float targetZ = player.transform.position.z - minimalDistanceBehind;

        // Calculate distance difference on Z
        float distanceZ = targetZ - transform.position.z;

        // Move z-position smoothly to maintain minimal distance
        float moveZ = 0f;
        if (Mathf.Abs(distanceZ) > 0.1f) // threshold to prevent jitter
        {
            moveZ = Mathf.Sign(distanceZ) * adjustSpeed * Time.deltaTime;

            // Clamp so we don't overshoot targetZ
            if (Mathf.Abs(moveZ) > Mathf.Abs(distanceZ))
                moveZ = distanceZ;
        }

        // Set new position
        Vector3 newPos = new Vector3(newX, transform.position.y, transform.position.z + moveZ);
        transform.position = newPos;
    }
}
