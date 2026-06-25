using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;

namespace CybersecurityChatbot
{
    /// <summary>
    /// AudioPlayer.cs
    /// Handles playing the WAV voice greeting.
    /// If the audio file is missing or playback fails, the app continues
    /// gracefully without crashing (Section G requirement).
    /// </summary>
    public class AudioPlayer
    {
        // Possible locations for the WAV file
        private static readonly string[] SearchPaths =
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "greeting.wav"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav"),
            Path.Combine(Directory.GetCurrentDirectory(), "Audio", "greeting.wav"),
        };

        private bool _audioAvailable;
        private string? _resolvedPath;

        public AudioPlayer()
        {
            // Locate the WAV file at startup
            foreach (var path in SearchPaths)
            {
                if (File.Exists(path))
                {
                    _resolvedPath   = path;
                    _audioAvailable = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Plays the greeting WAV asynchronously.
        /// If the file is missing, shows a polite informational message instead of crashing.
        /// </summary>
        public async Task PlayGreetingAsync()
        {
            if (!_audioAvailable || _resolvedPath == null)
            {
                // Do NOT crash — just log/show a notice
                await Task.Run(() =>
                    Application.Current?.Dispatcher.Invoke(() =>
                        ShowAudioNotice()));
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    using var player = new SoundPlayer(_resolvedPath);
                    player.Play();   // non-blocking
                }
                catch (Exception ex)
                {
                    // Playback failed — show notice, do NOT crash
                    Application.Current?.Dispatcher.Invoke(() =>
                        ShowAudioNotice(ex.Message));
                }
            });
        }

        private static void ShowAudioNotice(string? reason = null)
        {
            // A subtle status update — never a modal crash dialog
            // In a real app you'd update a status bar; here we use a non-blocking notification
            Console.WriteLine(
                reason == null
                    ? "Audio: greeting.wav not found — continuing without sound."
                    : $"Audio: playback failed ({reason}) — continuing without sound.");
        }

        public bool IsAudioAvailable => _audioAvailable;
    }
}
