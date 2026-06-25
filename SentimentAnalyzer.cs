using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot
{
    /// <summary>
    /// SentimentAnalyzer.cs
    /// Detects emotional tone from user input and returns an appropriate
    /// empathetic response prefix and tip.
    /// </summary>
    public class SentimentAnalyzer
    {
        // ── Trigger word lists per sentiment ─────────────────────────────────────
        private static readonly List<string> WorriedWords = new()
        {
            "worried", "scared", "afraid", "anxious", "nervous", "concerned",
            "frightened", "panic", "terrified", "uneasy", "helpless"
        };

        private static readonly List<string> FrustratedWords = new()
        {
            "frustrated", "annoyed", "angry", "confused", "tired", "fed up",
            "hate", "useless", "complicated", "too hard", "don't understand",
            "cant understand", "can't understand", "overwhelming"
        };

        private static readonly List<string> CuriousWords = new()
        {
            "curious", "interested", "tell me more", "learn", "want to know",
            "how does", "what is", "explain", "teach me", "show me", "find out",
            "more about", "details", "deep dive"
        };

        // ════════════════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Analyses the input string and returns the detected sentiment.
        /// Returns Sentiment.Neutral if no sentiment words are found.
        /// </summary>
        public Sentiment DetectSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Sentiment.Neutral;

            string lower = input.ToLower();

            if (ContainsAny(lower, WorriedWords))    return Sentiment.Worried;
            if (ContainsAny(lower, FrustratedWords)) return Sentiment.Frustrated;
            if (ContainsAny(lower, CuriousWords))    return Sentiment.Curious;

            return Sentiment.Neutral;
        }

        /// <summary>
        /// Returns an empathetic acknowledgement sentence for the detected sentiment.
        /// </summary>
        public string GetEmpathyPrefix(Sentiment sentiment)
        {
            return sentiment switch
            {
                Sentiment.Worried =>
                    "💙 It's completely understandable to feel worried — " +
                    "cybersecurity threats are real, but knowledge is your best defence. " +
                    "Here's a simple, actionable tip to help you stay safe:",

                Sentiment.Frustrated =>
                    "🤝 I understand this can be frustrating — cybersecurity has a lot of " +
                    "jargon. Let me simplify it for you with a clear, basic tip:",

                Sentiment.Curious =>
                    "✨ Great! I love that you're curious about staying safe online. " +
                    "Here's some extra information on this topic:",

                _ => string.Empty
            };
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════════
        private static bool ContainsAny(string input, IEnumerable<string> words)
            => words.Any(w => input.Contains(w));
    }

    /// <summary>Possible user sentiment states.</summary>
    public enum Sentiment
    {
        Neutral,
        Worried,
        Frustrated,
        Curious
    }
}
