using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ConstellationDetection))]
[RequireComponent(typeof(AudioSource))]
public class Constellation : MonoBehaviour {
    public Transform[] stars;

    public AudioClip detectionSound;
    public AudioClip validationSound;

    private LineRenderer _lineRenderer;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = detectionSound;
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
