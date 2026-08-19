using System;
using System.Collections.Generic;
using System.Text;
using IdleMvp.UI.Maple;
using UnityEngine;

namespace IdleMvp.UI.Casual
{
    /// <summary>필요 재료 한 줄. 보유량이 부족하면 빨갛게 표시된다.</summary>
    public struct CostLine
    {
        public string Label;
        public double Need;
        public double Have;
        public bool Enough => Have >= Need;

        public static CostLine Of(string label, double need, double have)
            => new CostLine { Label = label, Need = need, Have = have };
    }

    /// <summary>
    /// 구매 에셋 팝업 프리팹을 그대로 쓰는 공용 확인창 / 획득창.
    ///
    /// 뽑기·강화가 누르자마자 실행되던 것을 이 확인창을 거치게 바꿨다.
    /// 확인창에는 무엇을 하는지, 필요한 재료와 현재 보유량이 함께 표시된다.
    /// </summary>
    public static class CasualDialogs
    {
        static Transform _host;
        static CasualPanel _confirm;
        static CasualPanel _reward;

        /// <summary>팝업을 붙일 부모(메인 캔버스). HUD가 만들어질 때 한 번 넣어준다.</summary>
        public static void SetHost(Transform host) => _host = host;

        public static bool HasHost => _host != null;

        // ---- 확인창 ---------------------------------------------------------

        /// <summary>
        /// 확인창을 띄운다. costs가 하나라도 부족하면 확인 버튼이 잠긴다.
        /// onConfirm은 사용자가 확인을 눌렀을 때만 불린다.
        /// </summary>
        public static void Confirm(string title, string body, IList<CostLine> costs, Action onConfirm)
        {
            if (_host == null) { onConfirm?.Invoke(); return; }   // 호스트 없으면 그냥 진행

            if (_confirm == null || !_confirm.Valid)
                _confirm = CasualPanel.Load("Popup_Checking", _host);
            if (_confirm == null) { onConfirm?.Invoke(); return; }

            bool affordable = true;
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(body)) sb.Append(body);

            if (costs != null && costs.Count > 0)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append("<size=90%>");
                for (int i = 0; i < costs.Count; i++)
                {
                    var c = costs[i];
                    if (!c.Enough) affordable = false;
                    // 부족한 재료는 빨갛게 — 왜 못 누르는지 바로 보이게
                    string color = c.Enough ? "#B8E0FF" : "#FF6B6B";
                    sb.Append("\n<color=").Append(color).Append('>')
                      .Append(c.Label).Append("  ")
                      .Append(UiKit.Num(c.Have)).Append(" / ").Append(UiKit.Num(c.Need))
                      .Append(c.Enough ? "" : "  (부족)")
                      .Append("</color>");
                }
                sb.Append("</size>");
            }

            _confirm.SetText("Text_Title", title);
            _confirm.SetText("Text_Info", sb.ToString());
            _confirm.SetText("Text_Ok", affordable ? "확인" : "재료 부족");
            var okText = _confirm.Get<TMPro.TMP_Text>("Text_Ok");
            if (okText != null)
            {
                okText.enableWordWrapping = false;   // '재료 부족'이 두 줄로 쪼개지지 않게
                okText.overflowMode = TMPro.TextOverflowModes.Overflow;
            }
            _confirm.SetInteractable("Button_Ok", affordable);

            _confirm.OnClick("Button_Ok", () =>
            {
                _confirm.Hide();
                if (affordable) onConfirm?.Invoke();
            });
            _confirm.WireClose();
            _confirm.Show();
        }

        public static void Confirm(string title, string body, Action onConfirm)
            => Confirm(title, body, null, onConfirm);

        // ---- 획득창 ---------------------------------------------------------

        /// <summary>보상 획득 확인창(21번). 아이콘과 수량을 보여준다.</summary>
        public static void Reward(string headline, Sprite icon, string amount, Action onOk = null)
        {
            if (_host == null) { onOk?.Invoke(); return; }

            if (_reward == null || !_reward.Valid)
                _reward = CasualPanel.Load("Popup_RewardGet", _host);
            if (_reward == null) { onOk?.Invoke(); return; }

            _reward.SetTextStartsWith("Text", amount);
            var label = _reward.Find("Label_Rewards");
            if (label != null)
            {
                var t = label.GetComponentInChildren<TMPro.TMP_Text>();
                if (t != null) t.text = headline;
            }
            if (icon != null) _reward.SetSprite("IconFrame", icon);

            _reward.OnClick("Button_Ok", () => { _reward.Hide(); onOk?.Invoke(); });
            _reward.Show();
        }
    }
}
