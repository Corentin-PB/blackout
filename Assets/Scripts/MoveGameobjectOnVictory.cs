using System.Collections;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using Tools.Tween;
using UnityEngine.Serialization;
using Ease = Tools.Tween.Ease;

public class MoveGameobjectOnVictory : MonoBehaviour
{
    [Header("General")]

    public Transform gameobjectAtFinalPosition;

    public VictoryCondition eventController;

    [Tooltip("Maximum delta between Event and the start of the animation")] [Min(0)]
    public float maxRandomDeltaBeforeStart = 0;

    [Tooltip("Time of travel between start position and target position in seconds")] [Min(0)]
    public float duration = 1f;

    [Tooltip("Max delta on travel time")] [Min(0)]
    public float randomDeltaOnDuration = 0;

    private float _randomStartDelta;

    [Header("Debug")]

    [Min(0)]
    public float finalPositionSphereSize = .3f;

    void Start()
    {
        eventController.RegisterNewEventListener(StartTravelOnEvent);
        _randomStartDelta = Random.Range(0, maxRandomDeltaBeforeStart);
    }

    private void StartTravelOnEvent()
    {
        StartCoroutine(ExecuteAfterTime(_randomStartDelta));
    }

    private IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        float finalDuration = duration + Random.Range(0, randomDeltaOnDuration);
        Vector3Tween.Create(
            this.GetHashCode().ToString(),
            transform.position,
            gameobjectAtFinalPosition.position,
            finalDuration,
            Ease.InOutCubic,
            (t) => { transform.position = t.Value; });
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MoveGameobjectOnVictory))]
    public class FinalPositionTargetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MoveGameobjectOnVictory mg = (MoveGameobjectOnVictory) target;
            DrawDefaultInspector();
            GUILayout.Space(10);
        }

        private void OnSceneGUI()
        {
            MoveGameobjectOnVictory mg = (MoveGameobjectOnVictory) target;
            Transform transform = mg.transform;

            if (mg.gameobjectAtFinalPosition != null)
            {
                Handles.color = Color.magenta;
                Vector3 finalPos = mg.gameobjectAtFinalPosition.position;
                Handles.DrawLine(transform.position, finalPos);
                Handles.SphereHandleCap(0,
                    finalPos,
                    Quaternion.identity,
                    mg.finalPositionSphereSize,
                    EventType.Repaint);
            }
        }
    }
#endif
}