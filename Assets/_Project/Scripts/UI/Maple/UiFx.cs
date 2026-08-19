using System.Collections;
using TMPro;
using UnityEngine;

namespace IdleMvp.UI.Maple
{
    /// <summary>Number count-up tween for currency/stat labels.</summary>
    public class NumberTween : MonoBehaviour
    {
        double _shown;
        double _target;
        System.Func<double, string> _fmt;
        TMP_Text _label;
        Coroutine _co;
        bool _init;

        public void SetValue(TMP_Text label, double target, System.Func<double, string> fmt)
        {
            _label = label;
            _fmt = fmt;
            if (!_init || !gameObject.activeInHierarchy)
            {
                // first show (or inactive) — snap, no tween
                _init = true;
                _shown = target;
                _target = target;
                if (_label != null) _label.text = _fmt(target);
                return;
            }
            if (System.Math.Abs(target - _target) < 0.0001) return;
            _target = target;
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Co());
        }

        IEnumerator Co()
        {
            const float dur = 0.45f;
            double from = _shown;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                float e = 1f - (1f - u) * (1f - u); // ease-out
                _shown = from + (_target - from) * e;
                if (_label != null) _label.text = _fmt(_shown);
                yield return null;
            }
            _shown = _target;
            if (_label != null) _label.text = _fmt(_target);
            _co = null;
        }
    }

    /// <summary>Tiny UI juice helpers (count-up, button punch).</summary>
    public static class UiFx
    {
        /// <summary>Count-up assignment for numeric labels — drop-in for `label.text = fmt(v)`.</summary>
        public static void TweenNum(TMP_Text label, double target, System.Func<double, string> fmt = null)
        {
            if (label == null) return;
            fmt = fmt ?? (v => UiKit.Num(v));
            var tw = label.GetComponent<NumberTween>();
            if (tw == null) tw = label.gameObject.AddComponent<NumberTween>();
            tw.SetValue(label, target, fmt);
        }

        /// <summary>Quick scale punch on a UI element (success feedback).</summary>
        public static void Punch(Component target, float strength = 0.12f)
        {
            if (target == null) return;
            var p = target.GetComponent<PunchTween>();
            if (p == null) p = target.gameObject.AddComponent<PunchTween>();
            p.Play(strength);
        }
    }

    public class PunchTween : MonoBehaviour
    {
        Vector3 _base = Vector3.one;
        Coroutine _co;
        bool _captured;

        public void Play(float strength)
        {
            if (!gameObject.activeInHierarchy) return;
            if (!_captured) { _base = transform.localScale; _captured = true; }
            if (_co != null) { StopCoroutine(_co); transform.localScale = _base; }
            _co = StartCoroutine(Co(strength));
        }

        IEnumerator Co(float strength)
        {
            const float dur = 0.22f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / dur);
                // out-back style: overshoot then settle
                float s = 1f + strength * Mathf.Sin(u * Mathf.PI) * (1f - u * 0.4f);
                transform.localScale = _base * s;
                yield return null;
            }
            transform.localScale = _base;
            _co = null;
        }

        void OnDisable()
        {
            if (_captured) transform.localScale = _base;
        }
    }
}
