/* Script to track character with camera */

using UnityEngine;

public class CameraFollow : MonoBehaviour {
    // Class data
    public Transform target;
    public Vector3 offset;  
    public float smoothSpeed = 0.125f;  // Speed of camera

    private void LateUpdate() {
        // If target is defined, it is to be tracked
        if (target != null) {
            // Include offsets
            Vector3 pos = target.position + offset;
            
            // Smoothen position
            Vector3 newPos = Vector3.Lerp(transform.position, pos, smoothSpeed);
            
            transform.position = newPos;
        }
    }
}
