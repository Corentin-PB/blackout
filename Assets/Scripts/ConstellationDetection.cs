using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ConstellationDetection : MonoBehaviour {
    [Header("General")]
    public float distanceToPoint = 5f;
    public float distanceRange = 1f;
    public Vector3 direction = Vector3.forward;
    [Range(0, 1f)]
    public float positionDelta = 0.05f;

    [Range(0, 1f)]
    public float visionDelta = 0.05f;
    [Tooltip("Temps de validation en secondes")]
    public float validationTime = 2f;

    [HideInInspector]
    public bool isValidated;
    private float _validationRatio;
    private float _validationVelocity = 0f;

    private Transform _target;

    private void Start() {
        _target = Camera.main.transform;
    }

    public bool CheckDetection() {
        bool visionCheck = Mathf.Abs(Vector3.Dot(direction.normalized, _target.forward.normalized)) > 1 - visionDelta;
        bool positionCheck = Mathf.Abs(Vector3.Dot(direction.normalized, (_target.position - transform.position).normalized)) > 1 - positionDelta;
        float distanceToTarget = Vector3.Distance(_target.position, transform.position);
        bool distanceCheck = ( distanceToTarget > distanceToPoint ) && ( distanceToTarget < distanceToPoint + distanceRange );
        return visionCheck && distanceCheck && positionCheck;
    }

    private void Update() {
        if (CheckDetection()) {
            _validationRatio = Mathf.SmoothDamp(_validationRatio, 1f, ref _validationVelocity, validationTime);
        } else {
            _validationRatio = Mathf.SmoothDamp(_validationRatio, 0.2f, ref _validationVelocity, 1f);
        }

        transform.GetChild(0).transform.localScale = Vector3.one * _validationRatio;
        if (!isValidated && _validationRatio > 0.97f) {
            isValidated = true;
            OnValidated();
        }
    }

    public void OnValidated() {
        var material = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>()?.material;
        if (material is { })
            material.color = Color.green;
        Color c = GetComponent<LineRenderer>().material.color;
        c.a = 1f;
        GetComponent<LineRenderer>().material.color = c;
    }
}

[CustomEditor(typeof(ConstellationDetection))]
public class DetectionTargetEditor : Editor {
    public override void OnInspectorGUI() {
        ConstellationDetection dt = (ConstellationDetection) target;
        DrawDefaultInspector();
        GUILayout.Space(10);
        DropAreaGUI();
    }

    private void OnSceneGUI() {
        ConstellationDetection dt = (ConstellationDetection) target;
        Transform transform = dt.transform;

        Vector3 dirUp = Vector3.Cross(dt.direction.normalized, Vector3.left);
        Vector3 dirRight = Vector3.Cross(dt.direction.normalized, Vector3.down);

        Vector3 start = transform.position;
        Vector3 rangeStart = start + dt.direction.normalized * dt.distanceToPoint;
        Vector3 rangeEnd = rangeStart + dt.direction.normalized * dt.distanceRange;

        Handles.color = Color.green;
        Handles.DrawWireCube(start, Vector3.one * 0.05f);

        float angle = Mathf.Acos(1 - dt.positionDelta);

        Vector3 topDelta = Vector3.RotateTowards(dt.direction.normalized, dirUp, angle, 100f).normalized;
        Vector3 bottomDelta = Vector3.RotateTowards(dt.direction.normalized, -dirUp, angle, 100f).normalized;
        Vector3 rightDelta = Vector3.RotateTowards(dt.direction.normalized, dirRight, angle, 100f).normalized;
        Vector3 leftDelta = Vector3.RotateTowards(dt.direction.normalized, -dirRight, angle, 100f).normalized;

        Ray r = new Ray(dt.transform.position, topDelta);
        Plane p = new Plane(-dt.direction.normalized, rangeEnd);
        p.Raycast(r, out float distanceToLastPlane);
        float circleSize = Vector3.Distance(rangeEnd, start + topDelta * distanceToLastPlane);

        Ray r2 = new Ray(dt.transform.position, topDelta);
        Plane p2 = new Plane(-dt.direction.normalized, rangeStart);
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

    private void DropAreaGUI() {
        ConstellationDetection dt = (ConstellationDetection) target;
        
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\nDrag & Drop Transform here");

        switch (evt.type) {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences) {
                        GameObject go = draggedObject as GameObject;
                        // Do On Drag Stuff here
                        if (go is { } && go.TryGetComponent(out Transform transform)) {
                            dt.direction = (transform.position - dt.transform.position).normalized;
                            dt.distanceToPoint = Vector3.Distance(transform.position, dt.transform.position) - dt.distanceRange / 2f;
                            EditorWindow view = EditorWindow.GetWindow<SceneView>();
                            view.Repaint();
                        }
                    }
                }
                break;
        }
    }
}
