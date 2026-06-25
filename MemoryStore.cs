using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// MemoryStore.cs
    /// Stores and retrieves user-specific information across the conversation.
    /// No database — everything is kept in memory (variables + Dictionary).
    /// </summary>
    public class MemoryStore
    {
        // ── User identity ────────────────────────────────────────────────────────
        private string? _userName;
        private string? _favouriteTopic;

        // ── Flexible extra memory (Dictionary for extensibility) ─────────────────
        private readonly Dictionary<string, string> _extraMemory = new();

        // ── Conversation state flags ─────────────────────────────────────────────
        public bool HasAskedForName     { get; set; }
        public bool HasAskedForTopic    { get; set; }
        public bool IsNewUser           => !HasUserName;

        // ════════════════════════════════════════════════════════════════════════
        //  USER NAME
        // ════════════════════════════════════════════════════════════════════════

        public bool HasUserName => !string.IsNullOrWhiteSpace(_userName);

        /// <summary>Stores the user's name, capitalising the first letter.</summary>
        public void RememberUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            name = name.Trim();
            _userName = char.ToUpper(name[0]) + name[1..].ToLower();
        }

        public string GetUserName() => _userName ?? "Friend";

        // ════════════════════════════════════════════════════════════════════════
        //  FAVOURITE TOPIC
        // ════════════════════════════════════════════════════════════════════════

        public bool HasFavouriteTopic => !string.IsNullOrWhiteSpace(_favouriteTopic);

        /// <summary>Stores the user's favourite cybersecurity topic.</summary>
        public void RememberFavouriteTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return;
            _favouriteTopic = topic.Trim().ToLower();
        }

        public string GetFavouriteTopic() => _favouriteTopic ?? string.Empty;

        /// <summary>
        /// Returns a contextual reminder sentence referencing the saved topic.
        /// Example: "Since you're interested in phishing, here's an extra tip..."
        /// </summary>
        public string GetTopicReminder()
        {
            if (!HasFavouriteTopic) return string.Empty;
            return $"Since you're interested in {_favouriteTopic}, " +
                   "here's an extra tip for you:";
        }

        // ════════════════════════════════════════════════════════════════════════
        //  GENERAL KEY-VALUE MEMORY
        // ════════════════════════════════════════════════════════════════════════

        public void Remember(string key, string value)
            => _extraMemory[key.ToLower()] = value;

        public string? Recall(string key)
            => _extraMemory.TryGetValue(key.ToLower(), out var val) ? val : null;

        // ════════════════════════════════════════════════════════════════════════
        //  METHODS
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>Extracts a first name from a message like "I'm Lisa" or "My name is Thabo".</summary>
        public string? ExtractNameFromInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.ToLower().Trim();

            // Strip common prefixes
            string[] prefixes =
            {
                "my name is ", "i'm ", "i am ", "call me ", "it's ", "its ",
                "name's ", "they call me ", "hi i'm ", "hello i'm "
            };

            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix))
                {
                    string remainder = input.Substring(prefix.Length).Trim();
                    // Take only the first word as the name
                    string[] parts = remainder.Split(' ', 2);
                    return parts[0].Trim('!', '.', ',', '?');
                }
            }

            // If the input is a single word, assume it's the name
            string[] words = input.Trim().Split(' ');
            if (words.Length == 1 && words[0].Length >= 2)
                return words[0].Trim('!', '.', ',', '?');

            return null;
        }

        /// <summary>
        /// Tries to extract a cybersecurity topic keyword from the input.
        /// Returns the matched keyword or null.
        /// </summary>
        public string? ExtractTopicFromInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string lower = input.ToLower();

            var topicKeywords = new Dictionary<string, string>
            {
                ["password"]       = "passwords",
                ["phish"]          = "phishing",
                ["scam"]           = "scams",
                ["malware"]        = "malware",
                ["virus"]          = "malware",
                ["privacy"]        = "privacy",
                ["browsing"]       = "safe browsing",
                ["https"]          = "safe browsing",
                ["2fa"]            = "two-factor authentication",
                ["two factor"]     = "two-factor authentication",
                ["mfa"]            = "two-factor authentication",
                ["update"]         = "software updates",
                ["vpn"]            = "VPNs",
                ["fraud"]          = "scams",
                ["identity"]       = "identity protection",
                ["social engineer"] = "social engineering",
            };

            foreach (var (keyword, topic) in topicKeywords)
            {
                if (lower.Contains(keyword))
                    return topic;
            }

            return null;
        }

        /// <summary>Resets conversation state (keeps user name/topic).</summary>
        public void ResetSession()
        {
            HasAskedForName  = false;
            HasAskedForTopic = false;
        }
    }
}
