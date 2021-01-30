using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ConstellationDetection))]
public class Constellation : MonoBehaviour {
    public Transform[] stars;

    private LineRenderer _lineRenderer;

    private void Start() {
        _lineRenderer = GetComponent<LineRenderer>();

        _lineRenderer.positionCount = stars.Length;
        for (int i = 0; i < stars.Length; i++) {
            _lineRenderer.SetPosition(i, stars[i].position);
        }

        Color c = _lineRenderer.material.color;
        c.a = 0f;
        _lineRenderer.material.color = c;
    }
}
