using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ConstellationDetection))]
[RequireComponent(typeof(AudioSource))]
public class Constellation : MonoBehaviour {
    public Transform[] stars_1;
    public Transform[] stars_2;
    public Transform[] stars_3;

    public AudioClip detectionSound;
    public AudioClip validationSound;

    private LineRenderer[] _lineRenderers;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = detectionSound;
        _audioSource.loop = true;
        _audioSource.volume = 0;
        _audioSource.Play();

        _lineRenderers = new LineRenderer[3];

        _lineRenderers[0] = transform.GetChild(0).GetComponent<LineRenderer>();
        _lineRenderers[1] = transform.GetChild(1).GetComponent<LineRenderer>();
        _lineRenderers[2] = transform.GetChild(2).GetComponent<LineRenderer>();

        _lineRenderers[0].positionCount = stars_1.Length;
        for (int i = 0; i < stars_1.Length; i++) {
            _lineRenderers[0].SetPosition(i, stars_1[i].position);
        }

        _lineRenderers[1].positionCount = stars_2.Length;
        for (int i = 0; i < stars_2.Length; i++) {
            _lineRenderers[1].SetPosition(i, stars_2[i].position);
        }

        _lineRenderers[2].positionCount = stars_3.Length;
        for (int i = 0; i < stars_3.Length; i++) {
            _lineRenderers[2].SetPosition(i, stars_3[i].position);
        }

        Color c = _lineRenderers[0].material.color;
        c.a = 0f;
        _lineRenderers[0].material.color = c;
        _lineRenderers[1].material.color = c;
        _lineRenderers[2].material.color = c;
    }

    
}
