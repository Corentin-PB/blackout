using System.Linq;
using UnityEngine;

public class VictoryCondition : MonoBehaviour {
    public ConstellationDetection[] targets;
    private bool _isFinished;

    public GameObject sphere;

    private int _oldValidCount;

    public AudioClip[] musics;
    private AudioSource _audioSource;

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = musics[_oldValidCount];
        _audioSource.Play();
    }

    private void Update() {
        if (!_isFinished) {
            int currentValidCount = targets.Count(target => target.isValidated);
            if (_oldValidCount != currentValidCount) {
                _oldValidCount = currentValidCount;
                float oldTime = _audioSource.time;
                _audioSource.clip = musics[_oldValidCount];
                _audioSource.Play();
                _audioSource.time = oldTime;
            }
            if (currentValidCount == targets.Length) {
                _isFinished = true;
                OnVictory();
            }
        }
    }

    private void OnVictory() {
        sphere.transform.localScale = Vector3.one * 4f;
    }
}
