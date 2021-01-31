using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tools.Tween {
    public class TweenStore : MonoBehaviour {
        private static GameObject _root;

        private static readonly Dictionary<string, ITween> Dictionary = new Dictionary<string, ITween>();
        private static readonly List<string> ToDelete = new List<string>();
        private static readonly Dictionary<string, ITween> ToStore = new Dictionary<string, ITween>();

        public static bool LogWarning = true;

        public static void Store(string k, ITween t) {
            if (Dictionary.ContainsKey(k) && !ToDelete.Contains(k)) {
                if (LogWarning) {
                    print("Tween key already exists : " + k);
                }
            } else if (!ToStore.ContainsKey(k)) {
                ToStore.Add(k, t);
            }
        }

        public static void Delete(string k) {
            if (!Dictionary.ContainsKey(k)) {
                if (LogWarning) {
                    print("Tween key doesn't exist : " + k);
                }
            } else if (!ToDelete.Contains(k)) {
                ToDelete.Add(k);
                Dictionary.TryGetValue(k, out ITween t);
                t?.Pause();
            }
        }

        public static void Delete(ITween t) {
            Delete(t.Key);
        }

        public static bool Contains(string k) {
            return Dictionary.ContainsKey(k) && !ToDelete.Contains(k);
        }

        public static ITween Get(string k) {
            ToStore.TryGetValue(k, out ITween t1);
            Dictionary.TryGetValue(k, out ITween t2);
            return t1 ?? t2;
        }

        public static void TryAttachToScene() {
            if (_root == null && Application.isPlaying) {
                _root = GameObject.Find("RootLoader");
                if (_root == null) {
                    _root = new GameObject { name = "RootLoader" };
                }
                if (_root.GetComponent<TweenStore>() == null) {
                    _root.AddComponent<TweenStore>();
                }
                DontDestroyOnLoad(_root);
            }
        }

        private void Update() {
            float elapsedTime = Time.deltaTime;
            foreach (ITween t in Dictionary.Values) {
                t.Update(elapsedTime);
            }

            foreach (string s in ToDelete) {
                Dictionary.Remove(s);
            }
            ToDelete.Clear();

            foreach (string s in ToStore.Keys) {
                if (Dictionary.ContainsKey(s)) {
                    Dictionary.Remove(s);
                }
                ToStore.TryGetValue(s, out ITween t);
                if (t != null) {
                    Dictionary.Add(s, t);
                }
            }
            ToStore.Clear();
        }
    }

    public interface ITween {
        string Key { get; }
        bool IsRunning { get; }
        void Pause();
        void Resume();
        void Restart();
        void Finish();
        void Update(float elapsedTime);
    }

    public class Tween<T> : ITween {
        private EasingFunction.Function _easeFunc;
        private Action<Tween<T>> _func;

        public Ease Easing { set => _easeFunc = EasingFunction.GetEasingFunction(value); }
        public Action<Tween<T>> UpdateFunction { set => _func = value; }

        private Action<Tween<T>> _onComplete;
        private ITween _nextTween;

        private readonly Func<Tween<T>, T, T, float, T> _lerpFunc;

        private float _curDuration;

        public string Key { get; private set; }
        public bool IsRunning { get; private set; }
        public float Duration { get; set; }
        public float Progress { get; private set; }
        private T _end;
        private T _start;
        public T Value { get; private set; }

        protected Tween(Func<Tween<T>, T, T, float, T> lerpFunc) {
            _lerpFunc = lerpFunc;
        }

        private Tween<T> Setup(T start, T end, float duration, Ease ease, Action<Tween<T>> func) {
            _start = start;
            _end = end;
            Duration = duration;
            _easeFunc = EasingFunction.GetEasingFunction(ease);
            _func = func;
            Restart();
            return this;
        }

        private Tween<T> Build(string key, T start, T end, float duration, Ease ease, Action<Tween<T>> func) {
            Key = key;
            TweenStore.Store(key, this);
            return Setup(start, end, duration, ease, func);
        }

        public static void Delete(string key) {
            TweenStore.Delete(key);
        }

        private void SetKey(string k) {
            Key = k;
        }

        public void Pause() {
            IsRunning = false;
        }

        public void Resume() {
            IsRunning = true;
        }

        public void Restart() {
            _curDuration = 0f;
            Progress = 0f;
            Resume();
        }

        public void Finish() {
            Progress = 1f;
            Value = _end;
            _func(this);
            IsRunning = false;
            TweenStore.Delete(Key);
            if (_nextTween != null) {
                _nextTween.Restart();
                TweenStore.Store(Key, _nextTween);
            }
            _onComplete?.Invoke(this);
        }

        public Tween<T> Set(T start, T end, float duration, Ease ease, Action<Tween<T>> func) {
            _start = start;
            _end = end;
            Duration = duration;
            _easeFunc = EasingFunction.GetEasingFunction(ease);
            _func = func;
            return this;
        }

        public Tween<TNew> ContinueWith<TNew>(Tween<TNew> t, bool launchIfFinished = false) where TNew : struct {
            _nextTween = t;
            t.SetKey(Key);
            t.Pause();
            if (launchIfFinished && Progress >= 1) {
                _nextTween.Restart();
                TweenStore.Store(Key, _nextTween);
            }
            return t;
        }

        public Tween<T> OnComplete(Action<Tween<T>> onComplete, bool launchIfFinished = false) {
            _onComplete = onComplete;
            if (launchIfFinished && Progress >= 1) {
                _onComplete(this);
            }
            return this;
        }

        public void Update(float elapsedTime) {
            if (IsRunning) {
                _curDuration += elapsedTime;
                if (_curDuration <= Duration) {
                    Progress = _curDuration / Duration;
                    Value = _lerpFunc(this, _start, _end, _easeFunc(0, 1, Progress));
                    _func(this);
                } else {
                    Finish();
                }
            }
        }

        private static Tween<T> SetIfExists(string key, T start, T end, float duration, Ease ease, Action<Tween<T>> func) {
            if (TweenStore.Contains(key)) {
                try {
                    Tween<T> tw = (Tween<T>)TweenStore.Get(key);
                    tw.Set(start, end, duration, ease, func);
                    tw.Restart();
                    return tw;
                } catch (InvalidCastException) {
                    // Utils.Log("Tween are not of same type");
                }
            }
            return null;
        }

        public static Vector3Tween Create(string key, Vector3 start, Vector3 end, float duration, Ease ease, Action<Tween<Vector3>> func, bool updateIfExists = false) {
            Tween<Vector3> tw = updateIfExists ? Tween<Vector3>.SetIfExists(key, start, end, duration, ease, func) : null;
            return (Vector3Tween) (tw ?? (new Vector3Tween().Build(key, start, end, duration, ease, func)));
        }

        public static Vector2Tween Create(string key, Vector2 start, Vector2 end, float duration, Ease ease, Action<Tween<Vector2>> func, bool updateIfExists = false) {
            Tween<Vector2> tw = updateIfExists ? Tween<Vector2>.SetIfExists(key, start, end, duration, ease, func) : null;
            return (Vector2Tween)(tw ?? (new Vector2Tween().Build(key, start, end, duration, ease, func)));
        }

        public static FloatTween Create(string key, float start, float end, float duration, Ease ease, Action<Tween<float>> func, bool updateIfExists = false) {
            Tween<float> tw = updateIfExists ? Tween<float>.SetIfExists(key, start, end, duration, ease, func) : null;
            return (FloatTween)(tw ?? (new FloatTween().Build(key, start, end, duration, ease, func)));
        }

        public static ColorTween Create(string key, Color start, Color end, float duration, Ease ease, Action<Tween<Color>> func, bool updateIfExists = false) {
            Tween<Color> tw = updateIfExists ? Tween<Color>.SetIfExists(key, start, end, duration, ease, func) : null;
            return (ColorTween)(tw ?? (new ColorTween().Build(key, start, end, duration, ease, func)));
        }

        public static QuaternionTween Create(string key, Quaternion start, Quaternion end, float duration, Ease ease, Action<Tween<Quaternion>> func, bool updateIfExists = false) {
            Tween<Quaternion> tw = updateIfExists ? Tween<Quaternion>.SetIfExists(key, start, end, duration, ease, func) : null;
            return (QuaternionTween)(tw ?? (new QuaternionTween().Build(key, start, end, duration, ease, func)));
        }
    }

    public class Vector3Tween : Tween<Vector3> {
        private static Vector3 LerpVector3(Tween<Vector3> t, Vector3 start, Vector3 end, float progress) { return start + (end - start) * progress; }
        private static readonly Func<Tween<Vector3>, Vector3, Vector3, float, Vector3> LerpFunc = LerpVector3;
        public Vector3Tween() : base(LerpFunc) { }
    }

    public class Vector2Tween : Tween<Vector2> {
        private static Vector2 LerpVector2(Tween<Vector2> t, Vector2 start, Vector2 end, float progress) { return start + (end - start) * progress; }
        private static readonly Func<Tween<Vector2>, Vector2, Vector2, float, Vector2> LerpFunc = LerpVector2;
        public Vector2Tween() : base(LerpFunc) { }
    }

    public class FloatTween : Tween<float> {
        private static float Lerpfloat(Tween<float> t, float start, float end, float progress) { return start + (end - start) * progress; }
        private static readonly Func<Tween<float>, float, float, float, float> LerpFunc = Lerpfloat;
        public FloatTween() : base(LerpFunc) { }
    }

    public class ColorTween : Tween<Color> {
        private static Color LerpColor(Tween<Color> t, Color start, Color end, float progress) { return Color.Lerp(start, end, progress); }
        private static readonly Func<Tween<Color>, Color, Color, float, Color> LerpFunc = LerpColor;
        public ColorTween() : base(LerpFunc) { }
    }

    public class QuaternionTween : Tween<Quaternion> {
        private static Quaternion LerpQuaternion(Tween<Quaternion> t, Quaternion start, Quaternion end, float progress) { return Quaternion.Lerp(start, end, progress); }
        private static readonly Func<Tween<Quaternion>, Quaternion, Quaternion, float, Quaternion> LerpFunc = LerpQuaternion;
        public QuaternionTween() : base(LerpFunc) { }
    }
}