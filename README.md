# Cybersecurity Awareness Bot — Part 3/POE

## How to Run
1. Open `CybersecurityChatbot.csproj` in Visual Studio 2022
2. Press **F5** or **Ctrl+F5** to build and run
3. Enter your name when prompted and start chatting

---

## Part 3 Tasks Implemented

### Task 1 — Task Assistant with Reminders
- `add task - Enable two-factor authentication`
- `add task - Review privacy settings in 3 days`
- `show tasks` / `my tasks`
- `complete task 1` / `delete task 2`
- `remind me to update my password in 3 days`
- All tasks stored in `List<CyberTask>` (in-memory DB) in `TaskManager.cs`

### Task 2 — Cybersecurity Quiz (Mini-Game)
- Type `quiz` to start
- 16 questions (10 selected randomly per game)
- Mix of multiple-choice and true/false formats
- Immediate feedback + explanation after every answer
- Final score with performance message
- Implemented in `QuizEngine.cs`

### Task 3 — NLP Simulation
- Recognises varied phrasings: "remind me to...", "set a reminder", "don't let me forget"
- Intent detection via `Dictionary<string, List<string>>` in `NlpProcessor.cs`
- Extracts time expressions ("in 3 days", "tomorrow") for reminders
- Keyword extraction for enriched cybersecurity responses

### Task 4 — Activity Log
- Static `ActivityLog` class logs every significant action with timestamp
- View via: `activity log`, `show log`, `what have you done`
- Stores last 20 entries, displayed in the **Activity Log** tab
- Logs: session start/end, quiz start/complete, tasks added/completed/deleted, topics discussed

---

## UI — What Changed from Part 2
- **4-tab navigation**: Chat | Tasks | Quiz | Activity Log
- **Sidebar**: quick topic buttons + Part 3 shortcut buttons
- **Chat bubbles**: colour-coded by type (green=bot, amber=empathy, red=alert)
- **Typing animation** on all bot responses
- **Status bar**: live user name + online indicator
- Complete new colour palette: deep navy `#070B14`, electric blue `#3B82F6`, emerald `#10B981`

---

## File Structure
```
CybersecurityChatbot/
├── App.xaml                  — Styles, colours, control templates
├── App.xaml.cs
├── MainWindow.xaml           — Full 4-tab WPF UI
├── MainWindow.xaml.cs        — UI event handlers
├── ChatbotEngine.cs          — Orchestrates ALL features (Part 3 upgraded)
├── TaskManager.cs            — Task 1: tasks + reminders (NEW)
├── QuizEngine.cs             — Task 2: quiz game engine (NEW)
├── NlpAndActivityLog.cs      — Task 3: NLP + Task 4: Activity Log (NEW)
├── ResponseManager.cs        — Keyword responses + tips (unchanged)
├── MemoryStore.cs            — User memory (unchanged)
├── SentimentAnalyzer.cs      — Sentiment detection (unchanged)
├── DelegateHandlers.cs       — Delegates (unchanged)
├── AsciiArt.cs               — Logo (unchanged)
├── AudioPlayer.cs            — Voice greeting (unchanged)
└── CybersecurityChatbot.csproj
```

## GitHub Checklist
- Minimum 6 commits with meaningful messages
- Minimum 3 tags/releases (v1.0, v2.0, v3.0)
- README included
