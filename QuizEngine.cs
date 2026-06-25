using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    // ════════════════════════════════════════════════════════════════════════════
    //  DATA MODELS
    // ════════════════════════════════════════════════════════════════════════════

    public enum QuestionType { MultipleChoice, TrueFalse }

    public class QuizQuestion
    {
        public string       Question     { get; init; } = string.Empty;
        public List<string> Options      { get; init; } = new();
        public int          CorrectIndex { get; init; }  // index into Options list
        public string       Explanation  { get; init; } = string.Empty;
        public QuestionType Type         { get; init; }

        public string CorrectAnswer => Options[CorrectIndex];
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  QUIZ ENGINE  — Task 2
    //  Manages state machine: idle → active → awaiting answer → scoring
    // ════════════════════════════════════════════════════════════════════════════

    public class QuizEngine
    {
        // ── Question bank (15 questions: mixed MC + T/F) ─────────────────────────
        private static readonly List<QuizQuestion> _allQuestions = new()
        {
            // ── Multiple Choice ─────────────────────────────────────────────────
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What should you do if you receive an email asking for your password?",
                Options      = new() { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                CorrectIndex = 2,
                Explanation  = "Reporting phishing emails helps protect everyone. Legitimate organisations NEVER ask for passwords via email."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What does HTTPS indicate about a website?",
                Options      = new() { "A) The site is popular", "B) The connection is encrypted", "C) The site is government-approved", "D) The site loads faster" },
                CorrectIndex = 1,
                Explanation  = "HTTPS means the data between your browser and the website is encrypted, protecting it from eavesdroppers."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "Which is the STRONGEST password?",
                Options      = new() { "A) password123", "B) John1990", "C) Tr!5#kW@9$mL2", "D) qwerty" },
                CorrectIndex = 2,
                Explanation  = "A strong password uses a mix of uppercase, lowercase, numbers, and symbols — and avoids personal info."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What is two-factor authentication (2FA)?",
                Options      = new() { "A) Two passwords", "B) A backup email", "C) A second verification step beyond your password", "D) Logging in twice" },
                CorrectIndex = 2,
                Explanation  = "2FA requires something you KNOW (password) and something you HAVE (phone/code), making accounts far harder to breach."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What is ransomware?",
                Options      = new() { "A) Software that speeds up your PC", "B) Malware that encrypts your files and demands payment", "C) A type of antivirus", "D) A secure messaging app" },
                CorrectIndex = 1,
                Explanation  = "Ransomware locks your files until you pay criminals. Regular offline backups are your best defence."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "You're on public Wi-Fi. What should you use to stay secure?",
                Options      = new() { "A) Incognito mode", "B) A VPN", "C) A firewall", "D) Bluetooth" },
                CorrectIndex = 1,
                Explanation  = "A VPN encrypts all your traffic on public networks, hiding it from attackers on the same Wi-Fi."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What is social engineering in cybersecurity?",
                Options      = new() { "A) Building social media platforms", "B) Manipulating people to reveal confidential information", "C) Networking at tech events", "D) Programming social apps" },
                CorrectIndex = 1,
                Explanation  = "Social engineering attacks exploit human trust rather than technical vulnerabilities — always verify identities."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "How often should you update your software?",
                Options      = new() { "A) Never — updates break things", "B) Once a year", "C) As soon as updates are available", "D) Only when the PC is slow" },
                CorrectIndex = 2,
                Explanation  = "Updates patch security holes. Attackers actively exploit known vulnerabilities in outdated software."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What is phishing?",
                Options      = new() { "A) A type of malware that deletes files", "B) A deceptive attempt to steal credentials via fake emails/sites", "C) A method of encrypting data", "D) A network scanning tool" },
                CorrectIndex = 1,
                Explanation  = "Phishing tricks you into entering credentials on fake websites. Always verify URLs before clicking."
            },
            new QuizQuestion
            {
                Type         = QuestionType.MultipleChoice,
                Question     = "What should you do with software you no longer use?",
                Options      = new() { "A) Leave it installed — it may be useful", "B) Disable it", "C) Uninstall it to reduce attack surface", "D) Update it just in case" },
                CorrectIndex = 2,
                Explanation  = "Unused software can still have vulnerabilities. Uninstalling it removes potential entry points for attackers."
            },

            // ── True / False ────────────────────────────────────────────────────
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: Using the same password for multiple accounts is safe if the password is strong.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 1,
                Explanation  = "FALSE. If one site is breached, attackers try the same password on other sites — a technique called 'credential stuffing'."
            },
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: A padlock icon in your browser always means a website is trustworthy.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 1,
                Explanation  = "FALSE. The padlock only means the connection is encrypted — it does NOT mean the website itself is legitimate or safe."
            },
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: Antivirus software alone is enough to keep you secure online.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 1,
                Explanation  = "FALSE. Antivirus is one layer, but you also need updates, strong passwords, 2FA, and good habits."
            },
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: Public Wi-Fi networks are generally safe to use for online banking.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 1,
                Explanation  = "FALSE. Public Wi-Fi is unencrypted and can be monitored. Use a VPN or mobile data for banking."
            },
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: Two-factor authentication makes it significantly harder for attackers to access your accounts.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 0,
                Explanation  = "TRUE. 2FA blocks over 99% of automated attacks — even if attackers have your password, they cannot log in."
            },
            new QuizQuestion
            {
                Type         = QuestionType.TrueFalse,
                Question     = "TRUE or FALSE: You should click a link in an urgent email from your bank to 'verify your account'.",
                Options      = new() { "A) True", "B) False" },
                CorrectIndex = 1,
                Explanation  = "FALSE. This is a classic phishing tactic. Always go directly to your bank's website by typing the URL yourself."
            },
        };

        // ── State ────────────────────────────────────────────────────────────────
        private List<QuizQuestion> _activeQuestions = new();
        private int  _currentIndex = 0;
        private int  _score        = 0;
        private bool _isActive     = false;
        private bool _awaitingAnswer = false;

        private static readonly Random _rng = new();

        public bool IsActive       => _isActive;
        public bool AwaitingAnswer => _awaitingAnswer;

        // ════════════════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ════════════════════════════════════════════════════════════════════════

        public string StartQuiz()
        {
            // Shuffle and pick 10 questions
            _activeQuestions = new List<QuizQuestion>(_allQuestions);
            Shuffle(_activeQuestions);
            _activeQuestions = _activeQuestions.GetRange(0, Math.Min(10, _activeQuestions.Count));

            _currentIndex   = 0;
            _score          = 0;
            _isActive       = true;
            _awaitingAnswer = false;

            ActivityLog.Log("Quiz started");
            return $"Quiz started! You will answer {_activeQuestions.Count} cybersecurity questions.\nType the letter of your answer (A, B, C, or D).\n\n{GetCurrentQuestion()}";
        }

        public string GetCurrentQuestion()
        {
            if (_currentIndex >= _activeQuestions.Count)
                return FinishQuiz();

            var q = _activeQuestions[_currentIndex];
            _awaitingAnswer = true;

            string options = string.Join("\n", q.Options);
            return $"Question {_currentIndex + 1} of {_activeQuestions.Count}\n" +
                   $"{(q.Type == QuestionType.TrueFalse ? "[True/False]" : "[Multiple Choice]")}\n\n" +
                   $"{q.Question}\n\n{options}";
        }

        /// <summary>
        /// Processes the user's answer. Returns feedback + next question or final score.
        /// </summary>
        public string? ProcessAnswer(string input)
        {
            if (!_isActive || !_awaitingAnswer) return null;

            string lower = input.Trim().ToLower();

            // Accept: a/b/c/d, full option text, or the letter alone
            int? selectedIndex = ParseAnswerIndex(lower);
            if (selectedIndex == null)
                return "Please answer with A, B, C, or D.";

            _awaitingAnswer = false;
            var q = _activeQuestions[_currentIndex];

            bool correct = selectedIndex.Value == q.CorrectIndex;
            if (correct) _score++;

            string feedback = correct
                ? $"Correct! Well done.\n\n{q.Explanation}"
                : $"Incorrect. The correct answer was: {q.CorrectAnswer}\n\n{q.Explanation}";

            _currentIndex++;

            ActivityLog.Log($"Quiz Q{_currentIndex}: {(correct ? "correct" : "incorrect")}");

            if (_currentIndex >= _activeQuestions.Count)
                return feedback + "\n\n" + FinishQuiz();

            return feedback + $"\n\n{GetCurrentQuestion()}";
        }

        public void StopQuiz()
        {
            _isActive       = false;
            _awaitingAnswer = false;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private string FinishQuiz()
        {
            _isActive       = false;
            _awaitingAnswer = false;

            int total   = _activeQuestions.Count;
            int percent = (int)((double)_score / total * 100);

            string grade = percent switch
            {
                >= 90 => "Outstanding! You are a cybersecurity pro!",
                >= 70 => "Great job! You have solid cybersecurity knowledge.",
                >= 50 => "Not bad! Keep learning — every tip counts.",
                _     => "Keep practising! Cybersecurity awareness saves lives online."
            };

            ActivityLog.Log($"Quiz completed: {_score}/{total} ({percent}%)");

            return $"Quiz complete!\n\nYour score: {_score} / {total}  ({percent}%)\n\n{grade}\n\nType 'quiz' to play again!";
        }

        private static int? ParseAnswerIndex(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            char first = input.Trim()[0];
            return first switch
            {
                'a' => 0, 'b' => 1, 'c' => 2, 'd' => 3,
                _   => null
            };
        }

        private static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
