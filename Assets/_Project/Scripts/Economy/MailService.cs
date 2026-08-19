using System;
using System.Collections.Generic;
using IdleMvp.Adapters;
using UnityEngine;

namespace IdleMvp.Economy
{
    [Serializable]
    public class MailAttachment
    {
        public CurrencyId currency;
        public double amount;
        public double gold;
        public double redDiamond;
    }

    [Serializable]
    public class MailItem
    {
        public string id;
        public string title;
        public string body;
        public long createdUtcTicks;
        public bool claimed;
        public bool read;
        public MailAttachment attach = new MailAttachment();
    }

    /// <summary>Local mail queue for guild/raid/arena rewards.</summary>
    public class MailService : MonoBehaviour
    {
        public static MailService Instance { get; private set; }

        public List<MailItem> Inbox { get; private set; } = new List<MailItem>();
        public event Action OnChanged;

        const string PrefKey = "IdleGrow.Maple.Mail";

        public int UnreadCount
        {
            get
            {
                int n = 0;
                foreach (var m in Inbox)
                    if (!m.claimed) n++;
                return n;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        public void Send(string title, string body, double gold = 0, double rd = 0,
            CurrencyId extra = CurrencyId.Gold, double extraAmt = 0)
        {
            Inbox.Insert(0, new MailItem
            {
                id = Guid.NewGuid().ToString("N"),
                title = title,
                body = body,
                createdUtcTicks = DateTime.UtcNow.Ticks,
                attach = new MailAttachment
                {
                    gold = gold,
                    redDiamond = rd,
                    currency = extra,
                    amount = extraAmt
                }
            });
            Save();
            OnChanged?.Invoke();
        }

        public string Claim(string id)
        {
            var m = Inbox.Find(x => x.id == id);
            if (m == null) return "우편 없음";
            if (m.claimed) return "이미 수령";
            Grant(m);
            m.claimed = true;
            m.read = true;
            Save();
            OnChanged?.Invoke();
            return "우편 수령: " + m.title;
        }

        public string ClaimAll()
        {
            int n = 0;
            foreach (var m in Inbox)
            {
                if (m.claimed) continue;
                Grant(m);
                m.claimed = true;
                m.read = true;
                n++;
            }
            if (n == 0) return "수령할 우편 없음";
            Save();
            OnChanged?.Invoke();
            return $"우편 {n}건 수령";
        }

        void Grant(MailItem m)
        {
            if (m.attach == null) return;
            if (m.attach.gold > 0) WalletAdapter.Instance?.AddGold(m.attach.gold);
            if (m.attach.redDiamond > 0) WalletAdapter.Instance?.AddRedDiamond(m.attach.redDiamond);
            if (m.attach.amount > 0 && m.attach.currency != CurrencyId.Gold)
                CurrencyWallet.Instance?.Add(m.attach.currency, m.attach.amount);
        }

        void Save()
        {
            PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(new Wrap { items = Inbox.ToArray() }));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(PrefKey)) return;
            var w = JsonUtility.FromJson<Wrap>(PlayerPrefs.GetString(PrefKey));
            if (w?.items != null) Inbox = new List<MailItem>(w.items);
        }

        [Serializable]
        class Wrap
        {
            public MailItem[] items;
        }
    }
}
