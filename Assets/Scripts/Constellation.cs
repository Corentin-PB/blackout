using System;
using UnityEngine;
using Tools.Tween;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(ConstellationDetection))]
[RequireComponent(typeof(AudioSource))]
public class Constellation : MonoBehaviour {
    public Transform[] stars1;
    public Transform[] stars2;
    public Transform[] stars3;

    public AudioClip detectionSound;
    public AudioClip validationSound;

    private LineRenderer[] _lineRenderers;
    private AudioSource _audioSource;

    private ConstellationDetection _detection;

    private AudioSource _oneShotAudioSource;
    private AudioSource _musicAudioSource;

    private Material[] _starsMaterials;
    private Color _starsMaterialColor;
    private Color _starMaterialColorToLerpTo;

    private int _indexStarsMaterialColors;
    
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start() {
        _detection = GetComponent<ConstellationDetection>();
        SetupAudio();
        SetupLines();

        _detection.onDetectionChange.AddListener(OnDetection);
        _detection.onValidation.AddListener(OnValidation);
        
        _oneShotAudioSource = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>();
        _musicAudioSource = GameObject.FindWithTag("MusicSource").GetComponent<AudioSource>();

        _starsMaterials = new Material[15];

        _indexStarsMaterialColors = 0;
        for (int i = 3; i < transform.childCount; i++)
        {
            _starsMaterials[i - 3] = transform.GetChild(i).GetComponent<MeshRenderer>().material;
            _indexStarsMaterialColors++;
        }
        
        _starsMaterialColor = _starsMaterials[0].GetColor(EmissionColor);
        _starMaterialColorToLerpTo = _starsMaterialColor;
        _starMaterialColorToLerpTo *= 1.5f;
    }

    private void SetupAudio() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = detectionSound;
        _audioSource.loop = true;
        _audioSource.volume = 0;
        _audioSource.Play();
    }

    private void SetupLines() {

        _lineRenderers = new LineRenderer[3];

        Color c = new Color(1, 1, 1, 0);
        for (int i = 0; i < 3; ++i) {
            _lineRenderers[i] = transform.GetChild(i).GetComponent<LineRenderer>();
            Transform[] stars = i switch {
                0 => stars1,
                1 => stars2,
                _ => stars3
            };
            _lineRenderers[i].positionCount = stars.Length;
            for (int j = 0; j < stars.Length; j++) {
                _lineRenderers[i].SetPosition(j, stars[j].position);
            }
            _lineRenderers[i].material.color = c;
        }

    }

    private void OnDetection() {

        if (_detection.validationRatio > 0.03f && !_detection.isValidated) {
            _audioSource.volume = _detection.validationRatio;
            _musicAudioSource.volume = 1 - _detection.validationRatio;
        }
        
        if (_detection.validationRatio > 0.03f && !_detection.isValidated)
        {
            for (int i = 0; i < _indexStarsMaterialColors; i++)
            {
                Color c = Color.Lerp(_starsMaterialColor, _starMaterialColorToLerpTo, _detection.validationRatio);
                _starsMaterials[i].SetColor(EmissionColor, c);
            }
        }

        if (_detection.isValidated) {
            Color col = _starsMaterialColor;
            col.a = 0.05f;
            Color finalColor = Color.Lerp(col, Color.white, _detection.validationRatio);
        
            _lineRenderers[0].material.color = finalColor;
            _lineRenderers[1].material.color = finalColor;
            _lineRenderers[2].material.color = finalColor;
        }
    }

    private void OnValidation() {
        ColorTween.Create(GetHashCode().ToString() + 1, _starMaterialColorToLerpTo, _starsMaterialColor, 1.5f, Ease.Linear, t => {
            for (int i = 0; i < _indexStarsMaterialColors; i++)
            {
                _starsMaterials[i].SetColor(EmissionColor, t.Value);
            }
        });

        _audioSource.volume = 0f;
        FloatTween.Create(GetHashCode().ToString(), 0f, 1f, 3f, Ease.Linear, t => {
            _musicAudioSource.volume = t.Value;
        });
        _oneShotAudioSource.PlayOneShot(validationSound);
    }
}
