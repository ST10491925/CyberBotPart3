using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CybersecurityChatbot
{
    // ════════════════════════════════════════════════════════════════════════════
    //  TASK 3 — NLP SIMULATION
    //  Recognises varied phrasings of the same intent using keyword detection.
    //  Uses string.Contains() + synonym maps (as required by the spec).
    // ════════════════════════════════════════════════════════════════════════════

    public class NlpProcessor
    {
        // ── Intent → synonym groups (simulated NLP) ──────────────────────────────
        // Each intent maps to a list of phrases that trigger it.
        private static readonly Dictionary<string, List<string>> _intentMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["intent.quiz"] = new()
            {
                "quiz", "start quiz", "test me", "cyber quiz", "play quiz",
                "test my knowledge", "knowledge test", "question me", "let's play",
                "trivia", "game", "challenge me", "cybersecurity quiz"
            },
            ["intent.task.add"] = new()
            {
                "add task", "create task", "new task", "add a task", "create a task",
                "save task", "make a task", "i need to", "i have to", "set a task",
                "note this", "remember to", "don't let me forget"
            },
            ["intent.task.view"] = new()
            {
                "show tasks", "list tasks", "my tasks", "view tasks", "see tasks",
                "what tasks", "what do i have", "what have you done for me",
                "pending tasks", "task list", "show my tasks"
            },
            ["intent.reminder"] = new()
            {
                "remind me", "set reminder", "reminder for", "set a reminder",
                "don't forget", "alert me", "notify me", "let me know in"
            },
            ["intent.log"] = new()
            {
                "activity log", "show log", "what have you done", "history",
                "recent actions", "show activity", "log", "what happened",
                "show history", "what did we do"
            },
            ["intent.tip"] = new()
            {
                "tip", "advice", "suggest", "random tip", "quick tip",
                "give me a tip", "help me", "any advice", "what should i do",
                "security tip"
            },
            ["intent.help"] = new()
            {
                "help", "what can you do", "commands", "topics", "?",
                "menu", "options", "features", "what can i ask"
            },
        };

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Detects the intent from user input using keyword matching (NLP simulation).
        /// Returns the intent key (e.g. "intent.quiz") or null if unrecognised.
        /// </summary>
        public string? DetectIntent(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string lower = input.ToLower().Trim();

            foreach (var (intent, phrases) in _intentMap)
            {
                if (phrases.Any(p => lower.Contains(p)))
                    return intent;
            }
            return null;
        }

        /// <summary>
        /// Extracts contextual keywords from input for enriched responses.
        /// Returns a list of detected cybersecurity topic keywords.
        /// </summary>
        public List<string> ExtractKeywords(string input)
        {
            var keywords = new List<string>();
            string lower = input.ToLower();

            var topicWords = new[] {
                "password", "phishing", "malware", "ransomware", "vpn",
                "2fa", "two factor", "firewall", "privacy", "scam",
                "update", "backup", "identity", "social engineering", "https"
            };

            keywords.AddRange(topicWords.Where(w => lower.Contains(w)));
            return keywords;
        }

        /// <summary>
        /// Tries to extract a time expression like "in 3 days" or "tomorrow".
        /// Used for reminders.
        /// </summary>
        public string? ExtractTimeExpression(string input)
        {
            string lower = input.ToLower();
            var timeExpressions = new[]
            {
                "tomorrow", "today", "in 1 day", "in 2 days", "in 3 days",
                "in 5 days", "in a week", "next week", "in 7 days",
                "in 1 hour", "in 2 hours", "this evening", "tonight"
            };

            return timeExpressions.FirstOrDefault(t => lower.Contains(t));
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  TASK 4 — ACTIVITY LOG
    //  Static singleton-style log accessible from all classes.
    //  Stores last 10 actions with timestamps.
    // ════════════════════════════════════════════════════════════════════════════

    public static class ActivityLog
    {
        private static readonly List<(DateTime Time, string Entry)> _log = new();
        private const int MaxEntries = 20;

        public static void Log(string action)
        {
            _log.Add((DateTime.Now, action));
            if (_log.Count > MaxEntries)
                _log.RemoveAt(0);
        }

        /// <summary>
        /// Returns the last N entries as a formatted string for display.
        /// </summary>
        public static string GetLog(int count = 10)
        {
            if (_log.Count == 0)
                return "No activity recorded yet.";

            var recent = _log.TakeLast(count).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Here is a summary of recent actions:\n");

            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"  {i + 1}. [{recent[i].Time:HH:mm:ss}]  {recent[i].Entry}");

            return sb.ToString().Trim();
        }

        public static void Clear() => _log.Clear();
    }
}
