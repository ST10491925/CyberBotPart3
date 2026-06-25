using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CybersecurityChatbot
{
    // ════════════════════════════════════════════════════════════════════════════
    //  DATA MODEL
    // ════════════════════════════════════════════════════════════════════════════

    public enum TaskStatus { Pending, Completed }

    public class CyberTask
    {
        private static int _nextId = 1;

        public int        Id          { get; }
        public string     Title       { get; set; }
        public string     Description { get; set; }
        public string?    Reminder    { get; set; }  // e.g. "in 3 days"
        public DateTime   CreatedAt   { get; }
        public TaskStatus Status      { get; set; }

        public CyberTask(string title, string description, string? reminder = null)
        {
            Id          = _nextId++;
            Title       = title;
            Description = description;
            Reminder    = reminder;
            CreatedAt   = DateTime.Now;
            Status      = TaskStatus.Pending;
        }

        public override string ToString()
        {
            string status  = Status == TaskStatus.Completed ? "[DONE]" : "[PENDING]";
            string remind  = Reminder != null ? $"  |  Reminder: {Reminder}" : "";
            return $"{status} #{Id}  {Title}{remind}";
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  TASK MANAGER  — in-memory "database" (List<CyberTask>)
    //  Task 1 requirement: store tasks with title, description, optional reminder
    // ════════════════════════════════════════════════════════════════════════════

    public class TaskManager
    {
        // In-memory task list (simulates DB table)
        private readonly List<CyberTask> _tasks = new();

        // ── CRUD operations ──────────────────────────────────────────────────────

        public CyberTask AddTask(string title, string description, string? reminder = null)
        {
            var task = new CyberTask(title, description, reminder);
            _tasks.Add(task);
            ActivityLog.Log($"Task added: '{title}'" + (reminder != null ? $" (Reminder: {reminder})" : ""));
            return task;
        }

        public bool CompleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return false;
            task.Status = TaskStatus.Completed;
            ActivityLog.Log($"Task completed: '{task.Title}'");
            return true;
        }

        public bool DeleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return false;
            _tasks.Remove(task);
            ActivityLog.Log($"Task deleted: '{task.Title}'");
            return true;
        }

        public IReadOnlyList<CyberTask> GetAll()     => _tasks.AsReadOnly();
        public IEnumerable<CyberTask>  GetPending()  => _tasks.Where(t => t.Status == TaskStatus.Pending);
        public IEnumerable<CyberTask>  GetCompleted()=> _tasks.Where(t => t.Status == TaskStatus.Completed);

        // ── NLP helper: extract task intent from free text ───────────────────────

        /// <summary>
        /// Detects if the user's input is a task-related command and processes it.
        /// Returns a response string or null if the input is not task-related.
        /// </summary>
        public string? TryHandleTaskCommand(string input)
        {
            string lower = input.ToLower().Trim();

            // "show tasks" / "list tasks" / "my tasks" / "what have you done for me"
            if (lower.Contains("show task") || lower.Contains("list task")
                || lower.Contains("my task") || lower.Contains("what have you done")
                || lower.Contains("view task") || lower == "tasks")
                return BuildTaskList();

            // "add task ..." / "create task ..." / "new task ..."
            if (lower.StartsWith("add task") || lower.StartsWith("create task")
                || lower.StartsWith("new task") || lower.StartsWith("add a task"))
            {
                string raw = StripPrefix(lower, "add task", "create task", "new task", "add a task");
                return ProcessAddTask(raw, input);
            }

            // "remind me to ..." / "set reminder ..."
            if (lower.StartsWith("remind me to") || lower.StartsWith("set reminder")
                || lower.StartsWith("remind me about") || lower.StartsWith("set a reminder"))
            {
                string raw = StripPrefix(lower, "remind me to", "set reminder", "remind me about", "set a reminder");
                return ProcessAddTask(raw, input, withReminder: true);
            }

            // "complete task 2" / "mark task 2 done" / "done task 2"
            if ((lower.Contains("complete task") || lower.Contains("mark") && lower.Contains("done")
                || lower.StartsWith("done task") || lower.StartsWith("finish task")))
            {
                int? id = ExtractNumber(lower);
                if (id.HasValue && CompleteTask(id.Value))
                    return $"Task #{id.Value} marked as completed. Well done!";
                return "I could not find that task. Type 'show tasks' to see your list.";
            }

            // "delete task 2" / "remove task 2"
            if (lower.Contains("delete task") || lower.Contains("remove task"))
            {
                int? id = ExtractNumber(lower);
                if (id.HasValue && DeleteTask(id.Value))
                    return $"Task #{id.Value} has been deleted.";
                return "I could not find that task. Type 'show tasks' to see your list.";
            }

            return null;  // Not a task command
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private string ProcessAddTask(string rawText, string originalInput, bool withReminder = false)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return "Please specify the task. Example: 'Add task - Enable two-factor authentication'";

            // Try to separate "in X days" reminder from main task text
            string? reminder = null;
            string title     = rawText.Trim(' ', '-', ':');

            if (title.Contains(" in "))
            {
                int idx = title.LastIndexOf(" in ");
                string tail = title[(idx + 4)..].Trim();
                if (tail.Length > 0 && (tail.Contains("day") || tail.Contains("hour") || tail.Contains("week")))
                {
                    reminder = "in " + tail;
                    title    = title[..idx].Trim();
                }
            }

            if (withReminder && reminder == null)
                reminder = "when convenient";

            // Capitalise first letter
            title = char.ToUpper(title[0]) + title[1..];

            // Build a short description from the topic if possible
            string description = BuildDescription(title);

            var task = AddTask(title, description, reminder);

            string response = $"Task added: '{task.Title}'\n{description}";
            if (reminder != null)
                response += $"\n\nReminder set for: {reminder}.";
            response += "\n\nWould you like to set a reminder for this task? (e.g. 'remind me in 3 days')";
            return response;
        }

        private static string BuildDescription(string title)
        {
            string lower = title.ToLower();
            if (lower.Contains("password"))
                return "Review and update your account passwords to ensure they are strong and unique.";
            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor"))
                return "Enable two-factor authentication on your important accounts to add an extra security layer.";
            if (lower.Contains("privacy") || lower.Contains("settings"))
                return "Review account privacy settings to ensure your personal data is protected.";
            if (lower.Contains("update") || lower.Contains("patch"))
                return "Install pending software and OS updates to patch security vulnerabilities.";
            if (lower.Contains("backup"))
                return "Create a backup of your important data to protect against data loss or ransomware.";
            return $"Cybersecurity task: {title}.";
        }

        private string BuildTaskList()
        {
            if (_tasks.Count == 0)
                return "You have no tasks yet.\n\nTry: 'Add task - Enable two-factor authentication'";

            var sb = new StringBuilder();
            sb.AppendLine("Here is a summary of your tasks:\n");

            var pending   = GetPending().ToList();
            var completed = GetCompleted().ToList();

            if (pending.Count > 0)
            {
                sb.AppendLine("PENDING:");
                foreach (var t in pending)
                    sb.AppendLine($"  {t}");
            }

            if (completed.Count > 0)
            {
                sb.AppendLine("\nCOMPLETED:");
                foreach (var t in completed)
                    sb.AppendLine($"  {t}");
            }

            sb.AppendLine($"\nTotal: {_tasks.Count} task(s)  |  {pending.Count} pending  |  {completed.Count} done");
            return sb.ToString().Trim();
        }

        private static string StripPrefix(string input, params string[] prefixes)
        {
            foreach (string p in prefixes)
            {
                if (input.StartsWith(p))
                    return input[p.Length..].Trim(' ', '-', ':');
            }
            return input;
        }

        private static int? ExtractNumber(string input)
        {
            foreach (var word in input.Split(' '))
                if (int.TryParse(word.Trim('#', ',', '.'), out int n)) return n;
            return null;
        }
    }
}
