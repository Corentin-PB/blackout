using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DetectionTarget : MonoBehaviour {
    public float distanceToPoint = 5f;
    [Range(0, 5f)]
    public float distanceRange = 1f;
    public Vector3 direction = Vector3.forward;
    [Range(0, 1f)]
    public float directionDelta = 0.05f;

    [Range(0, 1f)]
    public float visionDelta = 0.05f;

    public bool visualizeDelta;

    public bool CheckDetection(Transform t) {
        bool visionCheck = Mathf.Abs(Vector3.Dot(direction, t.forward)) < directionDelta;
        bool positionCheck = Mathf.Abs(Vector3.Dot(direction, t.position - transform.position)) < directionDelta;
        float distanceToTarget = Vector3.Distance(t.position, transform.position);
        bool distanceCheck = ( distanceToTarget > distanceToPoint ) && ( distanceToTarget < distanceToPoint + distanceRange );
        return visionCheck && distanceCheck;
    }
}

[CustomEditor(typeof(DetectionTarget))]
public class DetectionTargetEditor : Editor {

    private void OnSceneGUI() {
        DetectionTarget dt = (DetectionTarget) target;
        Transform transform = dt.transform;

        Vector3 dirUp = Vector3.Cross(dt.direction.normalized, Vector3.left);
        Vector3 dirRight = Vector3.Cross(dt.direction.normalized, Vector3.down);
        
        Vector3 start = transform.position;
        Vector3 rangeStart = start + dt.direction * (dt.distanceToPoint - dt.distanceRange / 2);
        Vector3 rangeEnd = rangeStart + dt.direction * dt.distanceRange;
        
        Handles.color = Color.green;
        Handles.DrawWireCube(start, Vector3.one * 0.05f);
        if (dt.visualizeDelta) {
            Vector3 topDelta = Vector3.RotateTowards(dt.direction, dirUp, Mathf.PI * dt.directionDelta / 2f, 1f);
            Vector3 bottomDelta = Vector3.RotateTowards(dt.direction, -dirUp, Mathf.PI * dt.directionDelta / 2f, 1f);
            Vector3 rightDelta = Vector3.RotateTowards(dt.direction, dirRight, Mathf.PI * dt.directionDelta / 2f, 1f);
            Vector3 leftDelta = Vector3.RotateTowards(dt.direction, -dirRight, Mathf.PI * dt.directionDelta / 2f, 1f);
            
            Ray r = new Ray(dt.transform.position, topDelta);
            Plane p = new Plane(-dt.direction, rangeEnd);
            p.Raycast(r, out float distanceToLastPlane);
            float circleSize = Vector3.Distance(rangeEnd, start + topDelta * distanceToLastPlane);
            
            Ray r2 = new Ray(dt.transform.position, topDelta);
            Plane p2 = new Plane(-dt.direction, rangeStart);
            p2.Raycast(r2, out float distanceToFirstPlane);
            float circleSize2 = Vector3.Distance(rangeStart, start + topDelta * distanceToFirstPlane);

            Handles.color = Color.magenta;
            
            Handles.DrawLine(start, start + topDelta * distanceToFirstPlane);
            Handles.DrawLine(start, start + bottomDelta * distanceToFirstPlane);
            Handles.DrawLine(start, start + rightDelta * distanceToFirstPlane);
            Handles.DrawLine(start, start + leftDelta * distanceToFirstPlane);
            
            Handles.color = Color.yellow;

            Handles.DrawLine(start + topDelta * distanceToFirstPlane, start + topDelta * distanceToLastPlane);
            Handles.DrawLine(start + bottomDelta * distanceToFirstPlane, start + bottomDelta * distanceToLastPlane);
            Handles.DrawLine(start + rightDelta * distanceToFirstPlane, start + rightDelta * distanceToLastPlane);
            Handles.DrawLine(start + leftDelta * distanceToFirstPlane, start + leftDelta * distanceToLastPlane);
            
            Handles.DrawWireDisc(rangeStart, -dt.direction, circleSize2);
            
            Handles.DrawWireDisc(rangeEnd, -dt.direction, circleSize);
        }
    }
}
