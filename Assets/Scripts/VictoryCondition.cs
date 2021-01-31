using Tools.Tween;
using UnityEngine;
using UnityEngine.Events;

public class VictoryCondition : MonoBehaviour {
    public ConstellationDetection[] targets;
    private bool _isFinished;

    public GameObject sphere;

    public UnityEvent m_EndEvent;

    void Start() {
        if (m_EndEvent == null)
            m_EndEvent = new UnityEvent();

        TweenStore.TryAttachToScene();
    }

    private void Update() {
        if (!_isFinished) {
            bool fullyFinished = true;
            foreach (var target in targets) {
                fullyFinished = fullyFinished && target.isValidated;
            }

            if (fullyFinished) {
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