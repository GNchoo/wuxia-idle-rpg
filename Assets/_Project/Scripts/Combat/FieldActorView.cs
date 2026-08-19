using UnityEngine;
using UnityEngine.UI;

namespace IdleMvp.Combat
{
    /// <summary>
    /// UI-side field actor with HP, horizontal move, walk bob, attack lunge, hurt flash.
    /// </summary>
    public class FieldActorView : MonoBehaviour
    {
        public RectTransform Rect { get; private set; }
        public Image Image { get; private set; }
        public float Hp { get; private set; }
        public float MaxHp { get; private set; }
        public bool Alive => Hp > 0f && gameObject.activeSelf;
        public bool IsBoss { get; private set; }
        public bool IsHero { get; private set; }

        Image _hpFill;
        float _groundY;
        float _halfW;
        float _baseX;
        float _walkPhase;
        float _lungeT;
        float _lungeDir;
        float _hurtT;
        float _jumpT;
        bool _moving;
        Color _baseColor = Color.white;

        public void Setup(RectTransform parent, Sprite sprite, float maxHp, bool hero, bool boss, float groundY, float size)
        {
            IsHero = hero;
            IsBoss = boss;
            MaxHp = Mathf.Max(1f, maxHp);
            Hp = MaxHp;
            _groundY = groundY;

            var go = gameObject;
            go.transform.SetParent(parent, false);
            Rect = go.GetComponent<RectTransform>();
            if (Rect == null) Rect = go.AddComponent<RectTransform>();
            Image = go.GetComponent<Image>();
            if (Image == null) Image = go.AddComponent<Image>();

            Image.sprite = sprite;
            Image.color = Color.white;
            Image.preserveAspect = true;
            Image.raycastTarget = false;
            if (sprite == null)
                Image.color = hero ? new Color(0.35f, 0.55f, 0.95f) : new Color(0.85f, 0.35f, 0.35f);
            _baseColor = Image.color;

            float h = boss ? size * 1.35f : size;
            float w = boss ? size * 1.2f : size * 0.85f;
            _halfW = w * 0.5f;
            Rect.anchorMin = Rect.anchorMax = new Vector2(0.5f, 0f);
            Rect.pivot = new Vector2(0.5f, 0f);
            Rect.sizeDelta = new Vector2(w, h);
            Rect.anchoredPosition = new Vector2(0f, _groundY);
            _baseX = 0f;

            if (!hero)
            {
                var barBg = new GameObject("HpBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                barBg.transform.SetParent(transform, false);
                var bgRt = barBg.GetComponent<RectTransform>();
                bgRt.anchorMin = new Vector2(0.1f, 1f);
                bgRt.anchorMax = new Vector2(0.9f, 1f);
                bgRt.pivot = new Vector2(0.5f, 0f);
                bgRt.anchoredPosition = new Vector2(0f, 6f);
                bgRt.sizeDelta = new Vector2(0f, 10f);
                barBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

                var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fillGo.transform.SetParent(barBg.transform, false);
                var fillRt = fillGo.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                _hpFill = fillGo.GetComponent<Image>();
                _hpFill.color = new Color(0.95f, 0.25f, 0.3f);
                _hpFill.type = Image.Type.Filled;
                _hpFill.fillMethod = Image.FillMethod.Horizontal;
                _hpFill.fillAmount = 1f;
            }
        }

        void Update()
        {
            if (Rect == null || !Alive) return;

            float bob = 0f;
            if (_moving)
            {
                _walkPhase += Time.deltaTime * 12f;
                bob = Mathf.Abs(Mathf.Sin(_walkPhase)) * 8f;
            }
            else
            {
                _walkPhase += Time.deltaTime * 3f;
                bob = Mathf.Sin(_walkPhase) * 2.5f;
            }

            float lunge = 0f;
            if (_lungeT > 0f)
            {
                _lungeT -= Time.deltaTime;
                float u = 1f - Mathf.Clamp01(_lungeT / 0.16f);
                lunge = Mathf.Sin(u * Mathf.PI) * 28f * _lungeDir;
            }

            float jumpY = 0f;
            if (_jumpT > 0f)
            {
                _jumpT -= Time.deltaTime;
                float u = 1f - Mathf.Clamp01(_jumpT / 0.35f);
                jumpY = Mathf.Sin(u * Mathf.PI) * 70f;
            }

            float face = Mathf.Sign(Rect.localScale.x == 0 ? 1f : Rect.localScale.x);
            Rect.anchoredPosition = new Vector2(_baseX + lunge * face, _groundY + bob + jumpY);

            if (_hurtT > 0f)
            {
                _hurtT -= Time.deltaTime;
                Image.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.35f), Mathf.PingPong(_hurtT * 18f, 1f));
                if (_hurtT <= 0f) Image.color = _baseColor;
            }
        }

        public void SetMoving(bool moving) => _moving = moving;

        public void PlayAttack(float dir = 1f)
        {
            _lungeT = 0.16f;
            _lungeDir = Mathf.Sign(dir == 0f ? 1f : dir);
        }

        public void PlayJump()
        {
            _jumpT = 0.35f;
        }

        public void SetX(float x, float minX, float maxX)
        {
            x = Mathf.Clamp(x, minX + _halfW, maxX - _halfW);
            _baseX = x;
            // Y applied in Update (bob/jump)
            if (Rect != null)
                Rect.anchoredPosition = new Vector2(_baseX, Rect.anchoredPosition.y);
        }

        public float X => _baseX;

        public void Face(float targetX)
        {
            if (Rect == null) return;
            float mag = Mathf.Abs(Rect.localScale.y);
            if (mag < 0.01f) mag = 1f;
            Rect.localScale = new Vector3(targetX >= X ? mag : -mag, mag, 1f);
        }

        public void TakeDamage(float dmg)
        {
            if (!Alive) return;
            Hp = Mathf.Max(0f, Hp - dmg);
            _hurtT = 0.22f;
            if (_hpFill != null)
                _hpFill.fillAmount = Hp / MaxHp;
            if (Hp <= 0f)
                gameObject.SetActive(false);
        }

        public void ReviveFull()
        {
            Hp = MaxHp;
            gameObject.SetActive(true);
            if (_hpFill != null) _hpFill.fillAmount = 1f;
            Image.color = _baseColor;
        }
    }
}
