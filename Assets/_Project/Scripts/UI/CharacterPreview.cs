using System.Collections;
using IdleMvp.Core;
using IdleMvp.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.UI
{
    /// <summary>
    /// Live/snapshot RenderTexture preview of the player's customized rig (Hippo).
    /// Reused by: HUD portrait (snapshot), character modal (live), equip preview.
    /// Attach via CharacterPreview.Attach(parent, ...). Cleans rig/cam/RT on destroy.
    /// </summary>
    public class CharacterPreview : MonoBehaviour
    {
        static int _slotCounter;

        RawImage _image;
        GameObject _rig;
        Camera _cam;
        RenderTexture _rt;
        int _slot;
        int _w, _h;
        float _ortho;
        float _focusY;
        bool _live;

        /// <summary>Create a preview RawImage under parent. live=false renders one frame per refresh.</summary>
        public static CharacterPreview Attach(Transform parent, string name, int width, int height,
            float orthoSize, float focusY, bool live)
        {
            // Inactive while wiring fields — OnEnable must not fire before sizes are set.
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<CharacterPreview>();
            p._image = go.GetComponent<RawImage>();
            p._image.raycastTarget = false;
            p._w = width;
            p._h = height;
            p._ortho = orthoSize;
            p._focusY = focusY;
            p._live = live;
            p._slot = ++_slotCounter;
            go.SetActive(true);
            return p;
        }

        public RectTransform Rect => (RectTransform)transform;

        void OnEnable()
        {
            AppearanceService.OnChanged += Rebuild;
            HippoLookService.OnChanged += Rebuild;
            JobProgress.OnJobChanged += Rebuild;
            Rebuild();
        }

        void OnDisable()
        {
            AppearanceService.OnChanged -= Rebuild;
            HippoLookService.OnChanged -= Rebuild;
            JobProgress.OnJobChanged -= Rebuild;
            Release();
        }

        void Rebuild()
        {
            Release();
            string preset = AppearanceService.PresetForJob(JobProgress.JobId);
            var prefab = Resources.Load<GameObject>("CharPresets/" + preset);
            if (prefab == null) return;

            var basePos = new Vector3(700f + _slot * 40f, 700f, 0f);
            _rig = Instantiate(prefab);
            _rig.name = "Preview_" + name;
            _rig.transform.position = basePos;
            var hippo = _rig.GetComponentInChildren<IdleMvp.Combat.HippoActorController>();
            if (hippo != null)
                HippoLookService.ApplyHero(hippo.Char);   // 초상화도 장비=외형

            _rt = new RenderTexture(_w, _h, 16);
            var camGo = new GameObject("PreviewCam_" + name);
            camGo.transform.position = basePos + new Vector3(0f, _focusY, -10f);
            _cam = camGo.AddComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = _ortho;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0f, 0f, 0f, 0f); // transparent — UI frame shows through
            _cam.targetTexture = _rt;
            _image.texture = _rt;

            if (!_live)
                StartCoroutine(SnapshotThenSleep());
        }

        IEnumerator SnapshotThenSleep()
        {
            // Two frames: rig initialization settles (expressions, weapon renderers).
            yield return null;
            yield return null;
            if (_cam != null)
            {
                _cam.Render();
                _cam.enabled = false;
            }
        }

        void Release()
        {
            if (_rig != null) Destroy(_rig);
            if (_cam != null) Destroy(_cam.gameObject);
            if (_rt != null) { _rt.Release(); Destroy(_rt); }
            _rig = null; _cam = null; _rt = null;
        }
    }
}
