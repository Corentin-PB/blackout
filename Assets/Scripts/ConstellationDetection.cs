using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class ConstellationDetection : MonoBehaviour {
    [Header("General")] public float distanceToPoint = 5f;
    public float distanceRange = 1f;
    public Vector3 direction = Vector3.forward;
    [Range(0, 1f)] public float positionDelta = 0.05f;

    [Range(0, 1f)] public float visionDelta = 0.05f;

    [Tooltip("Temps de validation en secondes")]
    public float validationTime = 2f;

    [Header("Debug")] public bool visualiseVision = true;

    [HideInInspector] public bool isValidated;
    public float validationRatio;
    private float _oldValidationRatio;
    private float _validationVelocity;

    private Transform _target;

    [HideInInspector] public UnityEvent onValidation = new UnityEvent();
    [HideInInspector] public UnityEvent onDetectionChange = new UnityEvent();

    private void Start() {
        if (Camera.main is { }) _target = Camera.main.transform;
    }

    private bool CheckDetection() {
        bool visionCheck = Mathf.Abs(Vector3.Dot(direction.normalized, _target.forward.normalized)) > 1 - visionDelta;
        bool positionCheck =
            Mathf.Abs(Vector3.Dot(direction.normalized, (_target.position - transform.position).normalized)) >
            1 - positionDelta;
        float distanceToTarget = Vector3.Distance(_target.position, transform.position);
        bool distanceCheck =
            (distanceToTarget > distanceToPoint) && (distanceToTarget < distanceToPoint + distanceRange);
        return visionCheck && distanceCheck && positionCheck;
    }

    private void Update() {
        if (CheckDetection()) {
            validationRatio = Mathf.SmoothDamp(validationRatio, 1f, ref _validationVelocity, validationTime);
        } else {
            validationRatio = Mathf.SmoothDamp(validationRatio, 0f, ref _validationVelocity, 1f);
        }

        if (Math.Abs(_oldValidationRatio - validationRatio) > 0.001f) {
            _oldValidationRatio = validationRatio;
            onDetectionChange.Invoke();
        }

        if (!isValidated && validationRatio > 0.90f) {
            isValidated = true;
            onValidation.Invoke();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConstellationDetection))]
public class DetectionTargetEditor : Editor {
    public override void OnInspectorGUI() {
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

        var position = dt.transform.position;
        Ray r = new Ray(position, topDelta);
        Plane p = new Plane(-dt.direction.normalized, rangeEnd);
        p.Raycast(r, out float distanceToLastPlane);
        float circleSize = Vector3.Distance(rangeEnd, start + topDelta * distanceToLastPlane);

        Ray r2 = new Ray(position, topDelta);
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

        if (dt.visualiseVision) {
            Handles.color = Color.green;
            Vector3 start3 = (rangeStart + rangeEnd) / 2f;
            float angle3 = Mathf.Acos(1 - dt.visionDelta);
            Vector3 topDelta3 = Vector3.RotateTowards(-dt.direction.normalized, dirUp, angle3, 100f).normalized;
            Ray r3 = new Ray(start3, topDelta3);
            Plane p3 = new Plane(dt.direction.normalized, position);
            p3.Raycast(r3, out float distanceToView);
            float circleSize3 = Vector3.Distance(position, start3 + topDelta3 * distanceToView);
            
            Handles.DrawWireDisc(position, dt.direction, circleSize3);
        }
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
                            var position = transform.position;
                            dt.direction = (position - dt.transform.position).normalized;
                            dt.distanceToPoint = Vector3.Distance(position, dt.transform.position) - dt.distanceRange / 2f;
                            EditorWindow view = EditorWindow.GetWindow<SceneView>();
                            view.Repaint();
                        }
                    }
                }
                break;
        }
    }
}
#endif
