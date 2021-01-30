using UnityEngine;

public class VictoryCondition : MonoBehaviour {
    public ConstellationDetection[] targets;
    private bool _isFinished;

    public GameObject sphere;

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
    }
}
