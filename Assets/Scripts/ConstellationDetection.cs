using Tools.Tween;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering.PostProcessing;

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

    public PostProcessVolume postProcessVolume;

    [Header("Debug")] public bool visualiseVision = true;

    [HideInInspector]
    public bool isValidated;
    private float _validationRatio;
    private float _validationVelocity = 0f;

    private Transform _target;
    private AudioSource _audioSource;
    private Constellation _constellation;

    private AudioSource _oneShotAudioSource;
    private AudioSource _musicAudioSource;

    private Material[] starsMaterials; 

    private Color colorToLerpUp;
    
    private void Start() {
        _target = Camera.main.transform;
        _constellation = GetComponent<Constellation>();
        _audioSource = GetComponent<AudioSource>();
        
        _oneShotAudioSource = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>();
        _musicAudioSource = GameObject.FindWithTag("MusicSource").GetComponent<AudioSource>();

        colorToLerpUp = transform.GetChild(3).GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");

        starsMaterials = new Material[15];
        
        for (int i = 3; i < transform.childCount; i++)
        {
            starsMaterials[i - 3] = transform.GetChild(i).GetComponent<MeshRenderer>().material;
        }
        
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
            _validationRatio = Mathf.SmoothDamp(_validationRatio, 0f, ref _validationVelocity, 1f);
        }

        if (_validationRatio > 0.03f && !isValidated) {
            _audioSource.volume = _validationRatio;
            _musicAudioSource.volume = 1 - _validationRatio;
        }
        
        if (_validationRatio > 0.03f)
        {
            /*colorParameter.Interp(Color.white, colorToLerpUp, _validationRatio);

            foreach (Transform child in transform)
            {
                if (child.GetInstanceID() == transform.GetInstanceID())
                {
                    for (int i = 3; i < child.childCount; i++)
                    {
                        
                        child.GetChild(i).GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", );
                    }
                }
                else
                {
                    
                }
            }*/
            
            // Color c = Color.Lerp(Color.white, colorToLerpUp, _validationRatio);
            // colorParameter.value = c;
            // bloomParameter.color.Override(colorParameter);
        }
        
        if (!isValidated && _validationRatio > 0.97f) {
            isValidated = true;
            OnValidated();
        }
    }

    public void OnValidated() {
        Color c = transform.GetChild(0).GetComponent<LineRenderer>().material.color;
        c.a = 1f;
        transform.GetChild(0).GetComponent<LineRenderer>().material.color = c;
        transform.GetChild(1).GetComponent<LineRenderer>().material.color = c;
        transform.GetChild(2).GetComponent<LineRenderer>().material.color = c;

        _audioSource.volume = 0f;
        FloatTween.Create(GetHashCode().ToString(), 0f, 1f, 3f, Ease.Linear, t => {
            _musicAudioSource.volume = t.Value;
        });
        _oneShotAudioSource.PlayOneShot(_constellation.validationSound);

    }
}

#if UNITY_EDITOR
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

        if (dt.visualiseVision) {
            Handles.color = Color.green;
            Vector3 start3 = (rangeStart + rangeEnd) / 2f;
            float angle3 = Mathf.Acos(1 - dt.visionDelta);
            Vector3 topDelta3 = Vector3.RotateTowards(-dt.direction.normalized, dirUp, angle3, 100f).normalized;
            Ray r3 = new Ray(start3, topDelta3);
            Plane p3 = new Plane(dt.direction.normalized, dt.transform.position);
            p3.Raycast(r3, out float distanceToView);
            float circleSize3 = Vector3.Distance(dt.transform.position, start3 + topDelta3 * distanceToView);
            
            Handles.DrawWireDisc(dt.transform.position, dt.direction, circleSize3);
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
#endif
