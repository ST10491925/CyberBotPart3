using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// ResponseManager.cs
    /// Manages keyword → response mappings using a Dictionary&lt;string, string&gt;
    /// and a List&lt;string&gt; of random cybersecurity tips.
    ///
    /// WHY Dictionary: O(1) keyword lookup is efficient and self-documenting.
    ///                 It directly maps a topic to its educational response, making
    ///                 the data structure semantically clear and easy to extend.
    /// </summary>
    public class ResponseManager
    {
        // ── GENERIC COLLECTION 1: Dictionary ────────────────────────────────────
        // Stores keyword groups (comma-separated) → full educational response
        private readonly Dictionary<string, string> _keywordResponses;

        // ── GENERIC COLLECTION 2: List ───────────────────────────────────────────
        // Stores random cybersecurity tips for the "Random Tip" button
        private readonly List<string> _randomTips;

        // ── GENERIC COLLECTION 3: Queue ──────────────────────────────────────────
        // Stores conversation history (last N exchanges)
        private readonly Queue<string> _conversationHistory;

        private const int MaxHistoryItems = 20;
        private static readonly Random _rng = new();

        public ResponseManager()
        {
            _keywordResponses   = BuildKeywordDictionary();
            _randomTips         = BuildTipsList();
            _conversationHistory = new Queue<string>();
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Scans the user's input against all registered keywords.
        /// Returns the matching response string or null if no keyword found.
        /// </summary>
        public string? GetKeywordResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.ToLower();

            // Each dictionary key is a comma-separated list of trigger words
            foreach (var (keyGroup, response) in _keywordResponses)
            {
                string[] triggers = keyGroup.Split(',',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (string trigger in triggers)
                {
                    if (lower.Contains(trigger))
                        return response;
                }
            }
            return null;
        }

        /// <summary>Returns a random tip from the tips list.</summary>
        public string GetRandomTip()
        {
            if (_randomTips.Count == 0)
                return "Stay safe online — always think before you click!";

            int idx = _rng.Next(_randomTips.Count);
            return _randomTips[idx];
        }

        /// <summary>Adds an exchange to the conversation history queue.</summary>
        public void AddToHistory(string entry)
        {
            _conversationHistory.Enqueue(entry);
            // Keep queue bounded
            while (_conversationHistory.Count > MaxHistoryItems)
                _conversationHistory.Dequeue();
        }

        /// <summary>Returns a snapshot of current conversation history.</summary>
        public IEnumerable<string> GetHistory() => _conversationHistory;

        /// <summary>Clears the conversation history queue.</summary>
        public void ClearHistory() => _conversationHistory.Clear();

        // ════════════════════════════════════════════════════════════════════════
        //  KEYWORD DICTIONARY — all required topics from specification
        // ════════════════════════════════════════════════════════════════════════
        private static Dictionary<string, string> BuildKeywordDictionary()
        {
            return new Dictionary<string, string>
            {
                // Password safety
                ["password,passwords,passphrase"] =
                    "🔐 Use a unique password for each account. Make it at least 12 characters " +
                    "with numbers, uppercase, lowercase, and symbols. Consider using a password " +
                    "manager — it remembers everything so you don't have to!",

                // Phishing / scam emails
                ["phish,phishing,scam email"] =
                    "🎣 Phishing attacks trick you into clicking bad links. Always check the " +
                    "sender's email address carefully and hover over links before clicking. " +
                    "NEVER share personal info via email — legitimate companies never ask for it!",

                // Safe browsing / HTTPS
                ["safe browsing,browsing safely,https"] =
                    "🔒 Look for the padlock icon in your address bar — it confirms the site uses " +
                    "HTTPS. Avoid entering sensitive information on HTTP-only sites, and NEVER " +
                    "use public Wi-Fi for banking or shopping without a VPN.",

                // Scams / fraud
                ["scam,scams,fraud,con"] =
                    "🚨 Scammers create urgency and fear to make you act without thinking. " +
                    "Take a breath, verify independently, and never send money or gift cards " +
                    "to someone you haven't met in person. When in doubt — hang up!",

                // Malware / viruses
                ["malware,virus,trojan,ransomware,spyware"] =
                    "🦠 Malware can steal your data or lock your files for ransom. Keep your " +
                    "antivirus updated, avoid downloading software from unknown sites, and back " +
                    "up your important files regularly — ideally offline or in the cloud.",

                // Privacy / data protection
                ["privacy,private,data protection,personal data"] =
                    "🕵️ Check your app permissions regularly and revoke what you don't need. " +
                    "Only share what's truly necessary. Review privacy settings on social " +
                    "media every few months — platforms often reset them after updates.",

                // Two-factor authentication
                ["two factor,2fa,mfa,two-factor,multi-factor"] =
                    "🔑 Two-factor authentication adds a critical second layer of security. " +
                    "Always enable it when available — it blocks 99.9% of automated account " +
                    "hacks! Use an authenticator app rather than SMS when possible.",

                // Software updates / patches
                ["update,updates,patch,patches"] =
                    "⚙️ Software updates fix security holes that attackers exploit. Turn on " +
                    "automatic updates for your operating system, browser, and apps. " +
                    "Outdated software is one of the most common entry points for hackers.",

                // VPN
                ["vpn,virtual private network"] =
                    "🌐 A VPN encrypts your internet traffic, protecting you on public Wi-Fi. " +
                    "Choose a reputable paid VPN — free VPNs often sell your data. " +
                    "Use it especially when banking or accessing sensitive accounts away from home.",

                // Social engineering
                ["social engineering,manipulation,pretexting"] =
                    "🎭 Social engineering exploits human psychology rather than technology. " +
                    "Attackers impersonate trusted people (IT support, bank staff) to get access. " +
                    "Always verify identity through official channels before sharing anything.",

                // Identity theft
                ["identity theft,identity fraud,impersonation"] =
                    "👤 Protect your ID number, banking details, and personal info carefully. " +
                    "Monitor your credit report regularly for unusual activity. " +
                    "If you suspect identity theft, act fast — contact your bank and SAPS immediately.",

                // Public Wi-Fi
                ["public wifi,public wi-fi,free wifi,hotspot"] =
                    "📶 Public Wi-Fi networks are hunting grounds for hackers. Avoid accessing " +
                    "banking or sensitive accounts on them. If you must use public Wi-Fi, " +
                    "enable a VPN first to encrypt your connection.",
            };
        }

        // ════════════════════════════════════════════════════════════════════════
        //  RANDOM TIPS LIST
        // ════════════════════════════════════════════════════════════════════════
        private static List<string> BuildTipsList()
        {
            return new List<string>
            {
                "Never click links in unexpected SMS messages — go directly to the official website.",
                "Use a password manager to generate and store strong, unique passwords.",
                "Enable two-factor authentication on your email account — it's your most important account!",
                "Back up your data regularly using the 3-2-1 rule: 3 copies, 2 different media, 1 offsite.",
                "Log out of accounts on shared or public computers — don't just close the browser.",
                "Think before you post — information shared online can be used for social engineering.",
                "Check bank statements weekly to catch unauthorised transactions early.",
                "Lock your phone with a strong PIN or biometrics — not just a pattern.",
                "Be cautious of QR codes in public places — they can redirect to malicious sites.",
                "Legitimate companies will never ask for your OTP (One-Time Password) over phone or email.",
                "Update your router's firmware and change the default admin password.",
                "Use a dedicated email address for online shopping to limit spam and phishing exposure.",
                "Review which apps have access to your camera and microphone — revoke unnecessary permissions.",
                "Enable 'Find My Device' on your smartphone in case it's lost or stolen.",
                "Freeze your credit if you're not actively applying for loans — it prevents identity theft.",
            };
        }
    }
}
