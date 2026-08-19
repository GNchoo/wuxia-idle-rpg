using IdleMvp.UI;
using UnityEngine;

namespace IdleMvp.Combat
{
    /// <summary>
    /// World-space field actor backed by a Hippo(Character Editor Megapack) rig.
    /// Keeps the same field-pixel coordinate API that FieldAutoHuntController
    /// drives (X, SetX, Face...). Hippo rig natively faces RIGHT (+scale);
    /// facing left flips to -scale.
    /// </summary>
    public class CharacterActorView : MonoBehaviour
    {
        const float DeathAnimSeconds = 1.1f;
        const float HitFlashSeconds = 0.2f;

        public float Hp { get; private set; }
        public float MaxHp { get; private set; }
        public bool Alive => Hp > 0f && !_dead;
        public bool IsBoss { get; private set; }
        public bool IsHero { get; private set; }
        public string PresetName { get; private set; }
        public float X => _fieldX;
        /// <summary>Platform floor index (0 = ground).</summary>
        public int Floor { get; private set; }
        public float GroundYPx => _groundY;

        const float FloorJumpSeconds = 0.55f;
        /// <summary>도약 웅크림 길이. 이 뒤엔 체공 자세로 바뀐다.</summary>
        const float TakeoffSeconds = 0.12f;
        float _floorLerpT; // >0 while transitioning between floors
        float _floorYFrom;
        float _takeoffT;

        /// <summary>Jump to another floor — Y lerps over the arc so it reads as a jump, not a teleport.</summary>
        public void SetFloor(int floor, float groundY)
        {
            if (Floor == floor || _dead) return;
            Floor = floor;
            _floorYFrom = _groundY;
            _groundY = groundY;
            _floorLerpT = FloorJumpSeconds;
            _jumpT = FloorJumpSeconds;
            _locoState = "";
            _takeoffT = TakeoffSeconds;
            PlayRig("crouch", 0f);   // 웅크렸다가 → 체공은 달리는 자세 (Update에서 전환)
            ApplyPosition();
        }

        /// <summary>Place on a floor with no transition (spawn).</summary>
        public void SetFloorInstant(int floor, float groundY)
        {
            Floor = floor;
            _groundY = groundY;
            ApplyPosition();
        }

        HippoActorController _hippo;
        bool HasRig => _hippo != null;
        Transform _model;
        SpriteRenderer[] _renderers;
        Color[] _baseColors; // part colors from customization — tint multiplies, restore = base
        Transform _hpRoot;
        Transform _hpFill;
        float _hpBarWidth;

        string _attackAnim = "attack_swing1";
        float _fieldX;
        float _groundY;
        float _sizePx;
        float _scale = 1f;
        float _facing = 1f; // +1 = facing right
        bool _moving;
        bool _dead;
        float _jumpT;
        float _hurtT;
        float _attackT;
        float _zOffset;
        string _locoState = "";

        static Sprite _whiteSprite;

        public void Setup(RectTransform field, string presetName, string attackAnim,
            float maxHp, bool hero, bool boss, float groundY, float sizePx, bool showHpBar = true,
            int wuxiaTier = 0, int wuxiaKind = 0)
        {
            IsHero = hero;
            IsBoss = boss;
            MaxHp = Mathf.Max(1f, maxHp);
            Hp = MaxHp;
            _groundY = groundY;
            _sizePx = sizePx;
            if (!string.IsNullOrEmpty(attackAnim))
                _attackAnim = attackAnim;

            FieldWorldStage.Ensure(field);

            // Slight per-actor depth so overlapping rigs don't z-fight; hero in front.
            _zOffset = hero ? -0.5f : Random.Range(0.05f, 0.45f);

            PresetName = presetName;
            BuildModel(presetName);
            BuildHpBar(hero || !showHpBar);
            ApplyScale();
            _fieldX = 0f;
            ApplyPosition();
            PlayLoco("idle", 0f);
        }

        void BuildModel(string presetName)
        {
            var prefab = Resources.Load<GameObject>("CharPresets/" + presetName);
            if (prefab != null)
            {
                var go = Instantiate(prefab, transform);
                go.name = presetName;
                _model = go.transform;
                _model.localPosition = Vector3.zero;
                _hippo = go.GetComponentInChildren<HippoActorController>();
                // 장비=외형: 히어로는 커스터마이징+장착 장비 티어가 겉모습을 결정
                if (_hippo != null && IsHero)
                    IdleMvp.Progression.HippoLookService.ApplyHero(_hippo.Char);
            }
            else
            {
                // Preset not synced into Resources yet — plain sprite keeps the game playable.
                var go = new GameObject("Fallback");
                go.transform.SetParent(transform, false);
                _model = go.transform;
                var sr = go.AddComponent<SpriteRenderer>();
                int bossCh = 1;
                var spx = IdleMvp.Progression.StageProgress.Instance;
                if (spx != null)
                {
                    var bossRow = IdleMvp.Progression.StageTable.Get(spx.StageIndex);
                    if (bossRow != null) bossCh = bossRow.chapter;
                }
                sr.sprite = IsHero ? GrowArt.Hero : (IsBoss ? GrowArt.BiomeBoss(bossCh) : GrowArt.EnemyVariant());
                sr.sortingOrder = 10;
            }

            _renderers = _model.GetComponentsInChildren<SpriteRenderer>(true);
        }

        void BuildHpBar(bool skip)
        {
            if (skip) return;

            EnsureWhiteSprite();
            var root = new GameObject("HpBar");
            root.transform.SetParent(transform, false);
            _hpRoot = root.transform;

            var bg = new GameObject("Bg").AddComponent<SpriteRenderer>();
            bg.transform.SetParent(_hpRoot, false);
            bg.sprite = _whiteSprite;
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
            bg.sortingOrder = 200;
            bg.transform.localScale = new Vector3(1f, 0.12f, 1f);

            var fill = new GameObject("Fill").AddComponent<SpriteRenderer>();
            fill.transform.SetParent(_hpRoot, false);
            fill.sprite = _whiteSprite;
            fill.color = new Color(0.95f, 0.25f, 0.3f);
            fill.sortingOrder = 201;
            fill.transform.localScale = new Vector3(0.96f, 0.08f, 1f);
            fill.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            _hpFill = fill.transform;
        }

        static void EnsureWhiteSprite()
        {
            if (_whiteSprite != null) return;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var px = new Color[4] { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(px);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }

        void ApplyScale()
        {
            var stage = FieldWorldStage.Instance;
            float unitsPerPx = stage != null ? stage.UnitsPerFieldPx : 0.01f;
            float targetH = _sizePx * unitsPerPx * (IsBoss ? 1.3f : 1f);

            float nativeH = MeasureNativeHeight();
            _scale = nativeH > 0.05f ? targetH / nativeH : 1f;
            ApplyFacingScale();

            if (_hpRoot != null)
            {
                _hpBarWidth = targetH * 0.55f;
                _hpRoot.localScale = new Vector3(_hpBarWidth, _hpBarWidth, 1f);
            }
        }

        float MeasureNativeHeight()
        {
            if (_renderers == null || _renderers.Length == 0) return 2f;
            bool has = false;
            var b = new Bounds();
            foreach (var r in _renderers)
            {
                if (r == null || r.sprite == null) continue;
                if (!has) { b = r.bounds; has = true; }
                else b.Encapsulate(r.bounds);
            }
            if (!has) return 2f;
            // bounds measured at scale 1 right after instantiate
            return Mathf.Max(0.05f, b.size.y);
        }

        void ApplyFacingScale()
        {
            if (_model == null) return;
            // Hippo rig natively faces right.
            _model.localScale = new Vector3(_facing >= 0f ? _scale : -_scale, _scale, 1f);
        }

        void ApplyPosition()
        {
            var stage = FieldWorldStage.Instance;
            if (stage == null) return;

            float jumpDur = _floorLerpT > 0f ? FloorJumpSeconds : 0.35f;
            float jumpPx = 0f;
            if (_jumpT > 0f)
            {
                float u = 1f - Mathf.Clamp01(_jumpT / jumpDur);
                jumpPx = Mathf.Sin(u * Mathf.PI) * (_floorLerpT > 0f ? 95f : 70f);
            }

            // Between floors: base Y eases from the old floor to the new one under the arc.
            float baseY = _groundY;
            if (_floorLerpT > 0f)
            {
                float t = 1f - Mathf.Clamp01(_floorLerpT / FloorJumpSeconds);
                baseY = Mathf.Lerp(_floorYFrom, _groundY, Mathf.SmoothStep(0f, 1f, t));
            }

            _lastFieldY = baseY + jumpPx;
            var pos = stage.FieldToWorld(new Vector2(_fieldX, _lastFieldY));
            pos.z = _zOffset;
            transform.position = pos;

            // HP바는 발아래 — 머리 위(h+0.15)에 두면 층 간격(150px)과 겹쳐
            // 윗층 몹의 바처럼 보인다 (유저 스크린샷 보고)
            if (_hpRoot != null)
                _hpRoot.position = transform.position + new Vector3(0f, -0.22f, 0f);
        }

        float _lastFieldY;

        /// <summary>실제 렌더 중인 필드 좌표 (점프·층 이동 보간 포함) — FX 앵커용.
        /// FloorY(Floor) 계산값은 이동 중 실위치와 어긋난다 (유저 스크린샷 보고).</summary>
        public Vector2 FieldAnchor => new Vector2(_fieldX, _lastFieldY);

        void Update()
        {
            if (_dead) { StepRigFrames(); return; }

            if (_jumpT > 0f || _floorLerpT > 0f)
            {
                _jumpT -= Time.deltaTime;
                if (_floorLerpT > 0f) _floorLerpT -= Time.deltaTime;
                // 도약 순간엔 웅크리고, 떠 있는 동안엔 달리는 자세 — 리그에 점프 클립이 없어 두 클립으로 만든다
                // 상체(공격)와 하체(이동)는 별도 레이어라 공격 중에도 다리 자세는 바꿔도 된다
                if (_takeoffT > 0f)
                {
                    _takeoffT -= Time.deltaTime;
                    if (_takeoffT <= 0f) PlayRig("run", 0f);
                }
                if (_jumpT <= 0f && _floorLerpT <= 0f)
                {
                    // 착지 — 이동 상태를 되돌린다. 이게 없으면 뛰고 나서 자세가 굳는다.
                    _locoState = "";
                    if (_attackT <= 0f) PlayLoco(_moving ? "walk" : "idle", 0f);
                }
                ApplyPosition();
            }

            if (_attackT > 0f)
            {
                _attackT -= Time.deltaTime;
                if (_attackT <= 0f)
                {
                    // 체공 중이면 서 있는 자세로 되돌리지 않는다 — 공격이 끝날 때마다 점프가 지워진다
                    if (_jumpT > 0f) PlayRig(_takeoffT > 0f ? "crouch" : "run", 0f);
                    else PlayLoco(_moving ? "walk" : "idle", 0f);
                }
            }

            if (_hurtT > 0f)
            {
                _hurtT -= Time.deltaTime;
                var c = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), Mathf.PingPong(_hurtT * 14f, 1f));
                Tint(_hurtT <= 0f ? Color.white : c);
            }

            StepRigFrames();
        }

        /// <summary>Snapshot per-part colors so hit flashes never wipe customization.</summary>
        void CacheBaseColors()
        {
            if (_renderers == null) return;
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _baseColors[i] = _renderers[i] != null ? _renderers[i].color : Color.white;
        }

        Color _permaTint = Color.white;

        /// <summary>Persistent tint (chapter/elite variation) — survives hit-flash resets.</summary>
        public void SetPermanentTint(Color c)
        {
            _permaTint = c;
            Tint(Color.white);
        }

        void Tint(Color c)
        {
            if (_renderers == null) return;
            if (_baseColors == null || _baseColors.Length != _renderers.Length)
                CacheBaseColors();
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r != null) r.color = _baseColors[i] * c * _permaTint;
            }
        }

        void PlayRig(string animName, float fade)
        {
            if (_hippo != null) _hippo.PlayAnimation(animName, fade);
        }

        /// <summary>
        /// 리그를 초당 몇 장으로 끊어 재생할지. 60fps 보간 그대로 두면 동작이 미끄러져
        /// 오히려 가벼워 보인다 — 2D 액션처럼 또각또각 끊어야 한 방의 무게가 산다.
        /// 8이면 0.4초 스윙이 준비-타격-마무리 3장 남짓으로 읽힌다.
        /// </summary>
        public static float RigFps = 8f;

        /// <summary>
        /// 공격 클립만 빠르게 흘린다. 스윙 클립이 0.67초라 8fps로는 5장이 넘어 뭉개지는데,
        /// 1.9배로 밀면 0.35초에 끝나 준비-타격-마무리 3장으로 떨어진다.
        /// 프레임 수를 줄이는 게 아니라 클립을 짧게 만드는 쪽이라 타격 간격(0.45초)과도 맞는다.
        /// </summary>
        public static float AttackAnimSpeed = 1.9f;

        /// <summary>
        /// 스윙을 시작하고 칼이 실제로 닿기까지. 3장 스윙의 2번째 장 = 딱 한 프레임 뒤다.
        /// 타격 판정·이펙트를 이만큼 미뤄야 "닿는 순간 터지는" 그림이 된다.
        /// RigFps를 바꾸면 같이 따라간다.
        /// </summary>
        public static float ImpactDelay => 1f / Mathf.Max(1f, RigFps);

        float _frameAccum;

        /// <summary>
        /// Animator를 꺼 두고 우리가 정한 간격으로만 밀어 준다.
        /// 위치는 매 프레임 갱신되므로(ApplyPosition) 이동은 부드럽고 자세만 끊긴다 — 2D 게임의 그 느낌.
        /// </summary>
        void StepRigFrames()
        {
            if (_hippo == null || _hippo.Char == null) return;
            var an = _hippo.Char.Animator;
            if (an == null || !an.isInitialized) return;
            if (RigFps <= 0f) { if (!an.enabled) an.enabled = true; return; }
            if (an.enabled) an.enabled = false;   // 자동 갱신을 끄고 수동으로만 민다

            float step = 1f / RigFps;
            float speed = _attackT > 0f ? AttackAnimSpeed : 1f;
            _frameAccum += Time.deltaTime;
            int guard = 4;   // 프레임 드랍 뒤 몰아치기 방지
            while (_frameAccum >= step && guard-- > 0)
            {
                _frameAccum -= step;
                an.Update(step * speed);
            }
            if (guard <= 0) _frameAccum = 0f;
        }

        void PlayLoco(string state, float fade)
        {
            if (_locoState == state && _attackT <= 0f) return;
            _locoState = state;
            PlayRig(state, fade);
        }

        public void SetMoving(bool moving)
        {
            if (_moving == moving) return;
            _moving = moving;
            if (_jumpT > 0f) return;   // 체공 중엔 걷기/서기로 덮지 않는다 (착지할 때 되돌린다)
            if (_attackT <= 0f && !_dead)
                PlayLoco(moving ? "walk" : "idle", 0.08f);
        }

        public void PlayAttack(float dir = 1f)
        {
            if (_dead) return;
            _attackT = 0.4f;
            _locoState = "";
            PlayRig(_attackAnim, 0f);
        }

        public void PlayJump()
        {
            if (_dead) return;
            _jumpT = 0.35f;
            PlayRig("jump", 0f);
            _attackT = 0.45f; // let jump anim settle back into loco afterwards
        }

        public void SetX(float x, float minX, float maxX)
        {
            float halfW = _sizePx * 0.3f;
            _fieldX = Mathf.Clamp(x, minX + halfW, maxX - halfW);
            ApplyPosition();
        }

        /// <summary>현재 바라보는 방향 (+1 오른쪽 / -1 왼쪽). 타깃 선택 가중치용.</summary>
        public float FacingSign => _facing;

        /// <summary>공격 모션 재생 중인가 — 이 동안 방향을 뒤집으면 스윙이 거울 반전돼 보인다.</summary>
        public bool IsAttacking => _attackT > 0f;

        public void Face(float targetX)
        {
            // 공격 모션 중 반전 금지 — 대상이 스윙 중에 죽고 다음 대상이 반대편이면
            // 모션 중간에 즉시 거울 반전돼 '회전하며 사라졌다 나타나는' 것처럼 보였다.
            if (_attackT > 0f) return;
            _facing = targetX >= _fieldX ? 1f : -1f;
            ApplyFacingScale();
        }

        public void TakeDamage(float dmg)
        {
            if (!Alive) return;
            Hp = Mathf.Max(0f, Hp - dmg);
            _hurtT = HitFlashSeconds;
            UpdateHpFill();

            if (Hp <= 0f)
            {
                Die();
            }
            else if (HasRig && !IsHero && _attackT <= 0f)
            {
                _locoState = "";
                PlayRig("hit1", 0f);
                _attackT = 0.25f;
            }
        }

        void UpdateHpFill()
        {
            if (_hpFill == null) return;
            float t = Mathf.Clamp01(Hp / MaxHp);
            _hpFill.localScale = new Vector3(0.96f * t, 0.08f, 1f);
            _hpFill.localPosition = new Vector3(-0.48f * (1f - t), 0f, -0.01f);
        }

        void Die()
        {
            _dead = true;
            if (_hpRoot != null) _hpRoot.gameObject.SetActive(false);
            if (HasRig)
            {
                PlayRig("die1", 0f);
                Destroy(gameObject, DeathAnimSeconds);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>Marks the actor as claimed by the controller; keeps corpse until anim finishes.</summary>
        public void ReleaseAfterDeath()
        {
            // Destroy already scheduled in Die(); fallback for spriteless actors.
            if (!HasRig)
                Destroy(gameObject);
        }

        public void ReviveFull()
        {
            _dead = false;
            Hp = MaxHp;
            gameObject.SetActive(true);
            if (_hpRoot != null) _hpRoot.gameObject.SetActive(true);
            UpdateHpFill();
            Tint(Color.white);
            PlayLoco("idle", 0f);
        }
    }
}
