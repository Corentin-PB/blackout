using System.Linq;
using UnityEngine;
using Tools.Tween;
using UnityEngine.Events;

public class VictoryCondition : MonoBehaviour {
    public ConstellationDetection[] targets;
    private bool _isFinished;

    public GameObject sphere;

    private int _oldValidCount;

    public AudioClip[] musics;
    private AudioSource _audioSource;

    public UnityEvent m_EndEvent = new UnityEvent();

    private void Start() {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = musics[_oldValidCount];
        _audioSource.Play();

        TweenStore.TryAttachToScene();
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
        m_EndEvent.Invoke();
    }

    public void RegisterNewEventListener(UnityAction call) {
        m_EndEvent.AddListener(call);
    }
}