using System;
using System.IO;
using System.Text;

namespace CybersecurityChatbot
{
    // ════════════════════════════════════════════════════════════════════════════
    //  DELEGATE DEFINITIONS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Delegate used to format bot messages before display.
    /// Allows swapping formatting logic at runtime without changing callers.
    /// </summary>
    public delegate string FormatBotMessage(string rawMessage, string userName);

    /// <summary>
    /// Delegate used to validate and clean user input before processing.
    /// Returns the sanitised input or null if input should be rejected.
    /// </summary>
    public delegate string? ValidateUserInput(string rawInput);

    /// <summary>
    /// Delegate used to log conversations to a file.
    /// </summary>
    public delegate void LogConversation(string userMessage, string botResponse);

    // ════════════════════════════════════════════════════════════════════════════
    //  DELEGATE HANDLER IMPLEMENTATIONS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DelegateHandlers.cs
    /// Contains concrete implementations for each delegate.
    ///
    /// WHY DELEGATES?
    ///   Delegates let us pass behaviour as a parameter, enabling:
    ///   - Pluggable message formatting (personalised vs. generic)
    ///   - Input validation logic that can be replaced without changing ChatbotEngine
    ///   - Optional logging that doesn't affect core bot logic
    /// </summary>
    public static class DelegateHandlers
    {
        // ── FormatBotMessage implementations ─────────────────────────────────────

        /// <summary>
        /// Personalises a bot message by appending the user's name contextually.
        /// Example: "Here's a tip for you, Lisa!"
        /// </summary>
        public static FormatBotMessage PersonalisedFormatter => (message, userName) =>
        {
            if (string.IsNullOrWhiteSpace(userName) || userName == "Friend")
                return message;

            // Only append name if message doesn't already contain it
            if (message.Contains(userName)) return message;

            // Add personalised sign-off on informational messages
            return message + $"\n\nStay safe, {userName}! 🛡️";
        };

        /// <summary>
        /// Adds a timestamp prefix to every bot message for audit purposes.
        /// </summary>
        public static FormatBotMessage TimestampedFormatter => (message, _) =>
            $"[{DateTime.Now:HH:mm:ss}]  {message}";

        // ── ValidateUserInput implementations ────────────────────────────────────

        /// <summary>
        /// Standard input validator:
        ///   - Trims whitespace
        ///   - Returns null for empty/whitespace-only strings
        ///   - Truncates messages longer than 500 characters gracefully
        /// </summary>
        public static ValidateUserInput StandardValidator => rawInput =>
        {
            if (string.IsNullOrWhiteSpace(rawInput)) return null;

            string cleaned = rawInput.Trim();

            if (cleaned.Length > 500)
                cleaned = cleaned[..500] + "...";   // Truncate but do NOT crash

            return cleaned;
        };

        // ── LogConversation implementations ──────────────────────────────────────

        /// <summary>
        /// Logs user+bot exchanges to a local log file in the application folder.
        /// If logging fails for any reason, the app continues normally.
        /// </summary>
        public static LogConversation FileLogger => (userMessage, botResponse) =>
        {
            try
            {
                string logDir  = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CybersecurityBot");
                Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, "conversation_log.txt");
                string entry   = new StringBuilder()
                    .AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]")
                    .AppendLine($"USER: {userMessage}")
                    .AppendLine($"BOT:  {botResponse}")
                    .AppendLine(new string('-', 60))
                    .ToString();

                File.AppendAllText(logPath, entry, Encoding.UTF8);
            }
            catch
            {
                // Logging failure must never crash the application
            }
        };

        /// <summary>
        /// No-op logger — used when logging is disabled.
        /// </summary>
        public static LogConversation NullLogger => (_, _) => { };
    }
}
