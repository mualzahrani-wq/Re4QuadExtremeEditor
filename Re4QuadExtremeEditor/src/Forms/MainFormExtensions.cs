using System;
using System.Drawing;
using System.Windows.Forms;
using Re4QuadExtremeEditor.src;

namespace Re4QuadExtremeEditor
{
    /// <summary>
    /// Partial class extending MainForm with:
    /// - Dark Mode toggle
    /// - Improved movement/animation settings
    /// - macOS/Linux platform adaptations
    /// </summary>
    public partial class MainForm : Form
    {
        // ── Dark Mode ─────────────────────────────────────────────────────────

        /// <summary>
        /// Toggles Dark Mode on or off and re-applies the theme.
        /// Call this from a menu item or options screen.
        /// </summary>
        public void ToggleDarkMode(bool enable)
        {
            Globals.DarkModeEnabled = enable;
            DarkMode.IsDarkModeEnabled = enable;
            DarkMode.Apply(this);
            glControl?.Invalidate();
        }

        /// <summary>
        /// Applies Dark Mode on startup if enabled in config.
        /// Called at the end of the constructor after all controls are created.
        /// </summary>
        private void ApplyStartupTheme()
        {
            if (Globals.DarkModeEnabled)
            {
                DarkMode.IsDarkModeEnabled = true;
                DarkMode.Apply(this);
            }
        }

        // ── Movement / Timer optimizations ────────────────────────────────────

        /// <summary>
        /// Applies movement settings from Globals to the main timer.
        /// Call this after loading config to update the timer interval.
        /// </summary>
        public void ApplyMovementSettings()
        {
            myTimer.Interval = Globals.MovementTimerInterval;
        }

        // ── macOS / cross-platform adaptations ────────────────────────────────

        /// <summary>
        /// Applies platform-specific UI fixes.
        /// On macOS: fixes menu rendering, font scaling, and path separators.
        /// On Windows: enables dark title bar if dark mode is on.
        /// </summary>
        public void ApplyPlatformFixes()
        {
            if (Globals.IsMacOS)
            {
                // Fix path separators for macOS
                Globals.xscrDiretory = "xscr/";
                Globals.xfileDiretory = "xfile/";

                // macOS uses native menus — reduce manual rendering artifacts
                ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;

                // Fix font rendering on Retina displays
                Font macFont = new Font("Helvetica Neue", 9F, FontStyle.Regular);
                this.Font = macFont;

                // Adjust window title for macOS style
                this.Text = "Re4QuadExtremeEditor";
            }
            else if (Globals.IsWindows)
            {
                // Windows-specific: paths with backslash (already default)
                Globals.xscrDiretory = @"xscr\";
                Globals.xfileDiretory = @"xfile\";

                // Apply dark title bar if dark mode enabled
                if (Globals.DarkModeEnabled)
                    DarkMode.EnableDarkTitleBar(this);
            }
        }

        // ── Smooth Camera helpers ─────────────────────────────────────────────

        // Smooth camera interpolation targets
        private float _smoothCamX = 0f, _smoothCamY = 0f, _smoothCamZ = 0f;

        /// <summary>
        /// Lerp helper for smooth camera movement.
        /// </summary>
        private static float Lerp(float current, float target, float t)
            => current + (target - current) * t;

        /// <summary>
        /// Returns a speed multiplier adjusted for the current platform.
        /// On macOS with trackpad, reduce scroll speed slightly.
        /// </summary>
        public static float GetPlatformScrollMultiplier()
        {
            // Trackpad on macOS sends high-frequency small deltas
            return Globals.IsMacOS ? 0.3f : 1.0f;
        }
    }
}
