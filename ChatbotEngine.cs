using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    /// <summary>
    /// ChatbotEngine.cs — Part 3 upgraded version.
    /// Adds: TaskManager (Task 1), QuizEngine (Task 2),
    ///       NlpProcessor (Task 3), ActivityLog (Task 4).
    /// All Part 1 + Part 2 logic is preserved unchanged.
    /// </summary>
    public class ChatbotEngine
    {
        private readonly ResponseManager   _responseManager;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private readonly MemoryStore       _memory;
        private readonly FormatBotMessage  _formatter;
        private readonly ValidateUserInput _validator;
        private readonly LogConversation   _logger;
        private readonly TaskManager       _taskManager;
        private readonly QuizEngine        _quizEngine;
        private readonly NlpProcessor      _nlp;

        public Func<string, MessageType, Task>? OnBotResponse { get; set; }

        private bool   _waitingForName;
        private bool   _waitingForTopic;
        private string _lastDetectedKeyword = string.Empty;

        private static readonly List<string> FollowUpResponses = new()
        {
            "Here's something more to consider: always approach cybersecurity as a habit, not a one-time task. Regular check-ins matter!",
            "An important point often overlooked: humans are the biggest vulnerability in any system. Always verify before you trust.",
            "Cybercriminals are constantly evolving. Staying informed through trusted sources (like CERT SA or SABRIC) is crucial.",
            "Remember: the best cybersecurity tool is a cautious, informed mind. No software can replace good judgement.",
        };

        private static readonly Random _rng = new();

        public ChatbotEngine()
        {
            _responseManager   = new ResponseManager();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _memory            = new MemoryStore();
            _taskManager       = new TaskManager();
            _quizEngine        = new QuizEngine();
            _nlp               = new NlpProcessor();
            _formatter         = DelegateHandlers.PersonalisedFormatter;
            _validator         = DelegateHandlers.StandardValidator;
            _logger            = DelegateHandlers.FileLogger;
        }

        public async Task StartConversationAsync()
        {
            ActivityLog.Log("Session started");
            await SendBotMessageAsync(
                "Hello! Welcome to the Cybersecurity Awareness Bot.\n" +
                "I help you stay safe online — ask about any cybersecurity topic,\n" +
                "take a quiz, manage your security tasks, or view your activity log.\n\n" +
                "What's your name?",
                MessageType.Bot);
            _waitingForName = true;
        }

        public async Task ProcessInputAsync(string rawInput)
        {
            string? input = _validator(rawInput);
            if (input == null) { await HandleEmptyInputAsync(); return; }

            if (_quizEngine.IsActive && _quizEngine.AwaitingAnswer)
            {
                string? quizReply = _quizEngine.ProcessAnswer(input);
                if (quizReply != null) { await SendBotMessageAsync(quizReply, MessageType.Bot); return; }
            }

            if (_waitingForName)  { await HandleNameInputAsync(input);  return; }
            if (_waitingForTopic) { await HandleTopicInputAsync(input); return; }

            string lower  = input.ToLower().Trim();
            string? intent = _nlp.DetectIntent(input);

            if (intent == "intent.quiz" || lower == "quiz" || lower.StartsWith("start quiz"))
            {
                string quizMsg = _quizEngine.StartQuiz();
                ActivityLog.Log("Quiz started by user");
                await SendBotMessageAsync(quizMsg, MessageType.Bot);
                return;
            }

            if (lower == "stop quiz" || lower == "quit quiz" || lower == "exit quiz")
            {
                _quizEngine.StopQuiz();
                await SendBotMessageAsync("Quiz stopped. Come back whenever you want to test your knowledge!", MessageType.Bot);
                return;
            }

            if (intent == "intent.log" || lower.Contains("activity log")
                || lower.Contains("show log") || lower.Contains("what have you done"))
            {
                await SendBotMessageAsync(ActivityLog.GetLog(), MessageType.Bot);
                return;
            }

            if (intent == "intent.task.add" || intent == "intent.task.view"
                || intent == "intent.reminder" || lower.Contains("task") || lower.Contains("remind"))
            {
                string? taskReply = _taskManager.TryHandleTaskCommand(input);
                if (taskReply != null) { await SendBotMessageAsync(taskReply, MessageType.Bot); return; }
            }

            if (intent == "intent.help" || lower is "help" or "?" or "menu")
            {
                await HandleHelpCommandAsync();
                return;
            }

            if (intent == "intent.tip")
            {
                string tip      = _responseManager.GetRandomTip();
                string userName = _memory.GetUserName();
                await SendBotMessageAsync(_formatter($"Quick Tip:\n\n{tip}", userName), MessageType.Bot);
                ActivityLog.Log("Random tip provided");
                return;
            }

            if (await HandleConversationalCommandsAsync(input)) return;

            Sentiment sentiment       = _sentimentAnalyzer.DetectSentiment(input);
            string?   keywordResponse = _responseManager.GetKeywordResponse(input);
            string?   detectedTopic   = _memory.ExtractTopicFromInput(input);

            if (detectedTopic != null)
            {
                _lastDetectedKeyword = detectedTopic;
                if (!_memory.HasFavouriteTopic) _memory.RememberFavouriteTopic(detectedTopic);
            }

            await BuildAndSendResponseAsync(input, sentiment, keywordResponse);
        }

        private async Task HandleNameInputAsync(string input)
        {
            _waitingForName = false;
            string? name = _memory.ExtractNameFromInput(input) ?? input.Split(' ')[0].Trim('!', '.', ',');
            _memory.RememberUserName(name);
            string userName = _memory.GetUserName();
            ActivityLog.Log($"User identified as: {userName}");
            await SendBotMessageAsync(
                $"Nice to meet you, {userName}!\n\n" +
                "I can help you with:\n" +
                "  Cybersecurity topics  |  Quiz game  |  Task manager  |  Activity log\n\n" +
                "Type 'help' anytime to see all features.\n\n" +
                "What cybersecurity topic interests you most?\n" +
                "(passwords / phishing / malware / 2FA / privacy / VPN...)",
                MessageType.Bot);
            _waitingForTopic = true;
        }

        private async Task HandleTopicInputAsync(string input)
        {
            _waitingForTopic = false;
            string topic    = _memory.ExtractTopicFromInput(input) ?? _memory.ExtractNameFromInput(input) ?? input.Trim();
            _memory.RememberFavouriteTopic(topic);
            string userName = _memory.GetUserName();
            string reply    = $"I will remember that you are interested in {topic}, {userName}. " +
                              "It is a crucial part of staying safe online.\n\n";
            string? keywordResp = _responseManager.GetKeywordResponse(input);
            reply += keywordResp ?? $"Feel free to ask me anything about {topic} or other cybersecurity topics!";
            ActivityLog.Log($"User topic preference: {topic}");
            await SendBotMessageAsync(_formatter(reply, userName), MessageType.Bot);
        }

        private async Task<bool> HandleConversationalCommandsAsync(string input)
        {
            string lower    = input.ToLower().Trim();
            string userName = _memory.GetUserName();

            if (lower is "hi" or "hello" or "hey" or "good morning" or "good afternoon"
                or "good evening" or "greetings" || lower.StartsWith("hi ") || lower.StartsWith("hello "))
            {
                await SendBotMessageAsync(_formatter($"Hello again, {userName}! How can I help you stay secure today?", userName), MessageType.Bot);
                return true;
            }

            if (lower.Contains("how are you") || lower.Contains("how're you"))
            {
                await SendBotMessageAsync(_formatter($"I am doing great, {userName} — always on guard against cyber threats!\nHow are YOU doing? Ask me anything about cybersecurity.", userName), MessageType.Bot);
                return true;
            }

            if (lower.Contains("purpose") || lower.Contains("what do you do") || lower.Contains("what can you do"))
            {
                await HandleHelpCommandAsync();
                return true;
            }

            if (lower.Contains("tell me more") || lower.Contains("more info") || lower.Contains("explain more") || lower == "more" || lower.Contains("another tip"))
            {
                string topicReminder = _memory.GetTopicReminder();
                string followUp      = FollowUpResponses[_rng.Next(FollowUpResponses.Count)];
                string msg           = string.IsNullOrEmpty(topicReminder) ? followUp : $"{topicReminder}\n\n{followUp}";
                if (!string.IsNullOrEmpty(_lastDetectedKeyword))
                {
                    string? extra = _responseManager.GetKeywordResponse(_lastDetectedKeyword);
                    if (extra != null) msg += $"\n\n{extra}";
                }
                await SendBotMessageAsync(_formatter(msg, userName), MessageType.Bot);
                return true;
            }

            if (lower.Contains("my name") || lower.Contains("remember me") || lower.Contains("who am i"))
            {
                await SendBotMessageAsync(
                    $"You are {userName}." +
                    (_memory.HasFavouriteTopic ? $" You are most interested in {_memory.GetFavouriteTopic()}." : " I do not have a favourite topic saved yet."),
                    MessageType.Bot);
                return true;
            }

            if (lower is "bye" or "goodbye" or "exit" or "quit" or "see you" || lower.StartsWith("bye ") || lower.Contains("goodbye"))
            {
                ActivityLog.Log("Session ended by user");
                await SendBotMessageAsync(_formatter($"Stay safe online, {userName}!\nRemember: think before you click, and when in doubt — do not!\nGoodbye for now!", userName), MessageType.Bot);
                return true;
            }

            return false;
        }

        private async Task HandleHelpCommandAsync()
        {
            string userName = _memory.GetUserName();
            await SendBotMessageAsync(
                $"Everything I can help you with, {userName}:\n\n" +
                "CYBERSECURITY TOPICS\n" +
                "  passwords  |  phishing  |  scams  |  malware  |  privacy\n" +
                "  2FA  |  safe browsing  |  VPN  |  software updates  |  social engineering\n\n" +
                "QUIZ  (Task 2)\n" +
                "  Type 'quiz' to start a 10-question cybersecurity knowledge test\n\n" +
                "TASK MANAGER  (Task 1)\n" +
                "  'add task - Enable 2FA'          |  'show tasks'\n" +
                "  'complete task 1'                 |  'delete task 2'\n" +
                "  'remind me to update password in 3 days'\n\n" +
                "ACTIVITY LOG  (Task 4)\n" +
                "  'activity log' or 'show log' to see recent actions\n\n" +
                "OTHER\n" +
                "  'random tip'  |  'tell me more'  |  'who am i'  |  'bye'",
                MessageType.Bot);
            ActivityLog.Log("Help menu displayed");
        }

        private async Task BuildAndSendResponseAsync(string input, Sentiment sentiment, string? keywordResponse)
        {
            string userName = _memory.GetUserName();
            string finalReply;
            MessageType msgType;

            bool hasSentiment = sentiment != Sentiment.Neutral;
            bool hasKeyword   = keywordResponse != null;

            if (hasSentiment && hasKeyword)
            {
                string empathy = _sentimentAnalyzer.GetEmpathyPrefix(sentiment);
                finalReply     = $"{empathy}\n\n{keywordResponse}";
                if (_memory.HasFavouriteTopic) finalReply += $"\n\nI will remember that you care about this, {userName}!";
                msgType = MessageType.Sentiment;
            }
            else if (hasSentiment && !hasKeyword)
            {
                string empathy   = _sentimentAnalyzer.GetEmpathyPrefix(sentiment);
                string tip       = _responseManager.GetRandomTip();
                string? topicResp = _memory.HasFavouriteTopic ? _responseManager.GetKeywordResponse(_memory.GetFavouriteTopic()) : null;
                finalReply = topicResp != null ? $"{empathy}\n\n{_memory.GetTopicReminder()}\n\n{topicResp}" : $"{empathy}\n\n{tip}";
                msgType    = MessageType.Sentiment;
            }
            else if (!hasSentiment && hasKeyword)
            {
                finalReply = keywordResponse!;
                msgType    = MessageType.Bot;
            }
            else
            {
                await HandleUnknownInputAsync(input);
                return;
            }

            string formatted = _formatter(finalReply, userName);
            _logger(input, formatted);
            _responseManager.AddToHistory($"USER: {input}  |  BOT: {formatted[..Math.Min(80, formatted.Length)]}...");
            if (!string.IsNullOrEmpty(_lastDetectedKeyword)) ActivityLog.Log($"Topic response: {_lastDetectedKeyword}");
            await SendBotMessageAsync(formatted, msgType);
        }

        public async Task HandleEmptyInputAsync()
        {
            string userName = _memory.HasUserName ? _memory.GetUserName() : string.Empty;
            string msg = string.IsNullOrEmpty(userName)
                ? "Please type a message so I can help you."
                : $"Please type a message so I can help you, {userName}.";
            await SendBotMessageAsync(msg, MessageType.Warning);
        }

        private async Task HandleUnknownInputAsync(string input)
        {
            bool isGibberish = input.Length < 20 && !input.Contains(' ') && !char.IsUpper(input[0]);
            string msg = isGibberish
                ? "I am not sure I understand. Could you rephrase that?\nType 'help' to see everything I can do."
                : $"I could not find a specific answer for that, {_memory.GetUserName()}.\nType 'help' to see all available topics and features.";
            await SendBotMessageAsync(msg, MessageType.Warning);
        }

        private async Task SendBotMessageAsync(string message, MessageType type)
        {
            if (OnBotResponse != null) await OnBotResponse(message, type);
        }

        public string GetRandomTip()       => _responseManager.GetRandomTip();
        public string GetCurrentUserName() => _memory.HasUserName ? _memory.GetUserName() : string.Empty;

        public void ClearHistory()
        {
            _responseManager.ClearHistory();
            _memory.ResetSession();
            _waitingForName  = false;
            _waitingForTopic = false;
        }
    }
}
