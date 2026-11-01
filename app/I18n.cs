using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CapriDesktop
{
    // Translate presentation only: provider names, settings values, user input and API payloads keep their original IDs.
    internal static class I18n
    {
        internal static bool Enabled = true;
        private static readonly HashSet<Control> Bound = new HashSet<Control>();
        private static readonly HashSet<ToolStripDropDown> BoundMenus = new HashSet<ToolStripDropDown>();
        internal static bool Trace;
        internal static readonly HashSet<string> Untranslated = new HashSet<string>();
        private static readonly Dictionary<string, string> Words = Load("Capri.Locale.es-ES.tsv");
        private static readonly Dictionary<string, string> Parts = Load("Capri.Locale.es-ES.fragments.tsv");

        private static Dictionary<string, string> Load(string resource)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream == null) throw new InvalidOperationException("Missing translation: " + resource);
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                        int tab = line.IndexOf('\t');
                        if (tab < 1) throw new InvalidDataException("Invalid translation line: " + resource);
                        result.Add(Unescape(line.Substring(0, tab)), Unescape(line.Substring(tab + 1)));
                    }
                }
            }
            return result;
        }

        private static string Unescape(string text) { return text.Replace("\\n", "\n").Replace("\\t", "\t"); }

        internal static string T(string text)
        {
            string result = Translate(text);
            if (Trace && result != null && Regex.IsMatch(result, @"\b(your|her|she|the|couldn't|doesn't|pick|press|click|needs|unpacking|downloading)\b", RegexOptions.IgnoreCase))
                lock (Untranslated) Untranslated.Add(result);
            return result;
        }

        private static string Translate(string text)
        {
            if (!Enabled || string.IsNullOrEmpty(text)) return text;
            string translated;
            if (Words.TryGetValue(text, out translated)) return translated;
            if (Words.TryGetValue(text + " >", out translated)) return translated.TrimEnd(' ', '>');
            if (text.EndsWith(".", StringComparison.Ordinal) && Words.TryGetValue(text.Substring(0, text.Length - 1), out translated)) return translated + ".";
            Match key = Regex.Match(text, @"^Using your (.+) key$");
            if (key.Success) return "Con tu clave de " + key.Groups[1].Value;
            Match freeVoice = Regex.Match(text, @"^Get her free voice \((.+)\), or pick CLOUD VOICE\.$");
            if (freeVoice.Success) return "Descarga su voz gratuita (" + freeVoice.Groups[1].Value + ") o elige VOZ EN LA NUBE.";
            int comma = text.IndexOf(", ", StringComparison.Ordinal);
            if (comma > 0 && Words.TryGetValue(text.Substring(0, comma), out translated)) return translated + ", " + T(text.Substring(comma + 2));
            Match step = Regex.Match(text, @"^([✓123]\s+)(.+)$");
            if (step.Success) return step.Groups[1].Value + T(step.Groups[2].Value);
            // Window titles sometimes capitalize an existing caption.
            if (text == text.ToUpperInvariant())
                foreach (KeyValuePair<string, string> pair in Words)
                    if (string.Equals(pair.Key, text, StringComparison.OrdinalIgnoreCase)) return pair.Value.ToUpperInvariant();
            if (text.IndexOf('\n') >= 0) return string.Join("\n", Array.ConvertAll(text.Split('\n'), T));
            if (text.IndexOf(" · ", StringComparison.Ordinal) >= 0) return string.Join(" · ", Array.ConvertAll(text.Split(new[] { " · " }, StringSplitOptions.None), T));
            string[] sentences = Regex.Split(text, @"(?<=[.!?])\s+(?=[A-ZÁÉÍÓÚ¿¡])");
            if (sentences.Length > 1) return string.Join(" ", Array.ConvertAll(sentences, T));
            string result = text;
            // Only explicitly listed affixes are substituted. Never translate arbitrary words inside a model, path or name.
            foreach (KeyValuePair<string, string> pair in Parts)
            {
                if (result.StartsWith(pair.Key, StringComparison.Ordinal)) result = pair.Value + result.Substring(pair.Key.Length);
                else if (result.EndsWith(pair.Key, StringComparison.Ordinal)) result = result.Substring(0, result.Length - pair.Key.Length) + pair.Value;
            }
            result = Regex.Replace(result, @"^(\d+) lines$", "$1 líneas");
            return result;
        }

        internal static void Bind(Control control)
        {
            if (!Enabled || !Bound.Add(control)) return;
            control.Disposed += delegate { Bound.Remove(control); };
            if (control is Form) control.Text = T(control.Text);
            if (!(control is TextBox)) control.AccessibleName = T(control.AccessibleName ?? control.Text);
            else if (!string.IsNullOrEmpty(control.AccessibleName)) control.AccessibleName = T(control.AccessibleName);
            control.TextChanged += delegate { if (!(control is TextBox)) control.AccessibleName = T(control.Text); };
            foreach (Control child in control.Controls) Bind(child);
            control.ControlAdded += delegate(object sender, ControlEventArgs e) { Bind(e.Control); };
        }

        internal static void Start(string[] args)
        {
            // The upstream regression suite checks logical English IDs. Spanish has a separate layout/catalog check.
            foreach (string arg in args) if (arg == "--smoke-test" || arg == "--verify") Enabled = false;
            Application.Idle += delegate
            {
                foreach (Form form in Application.OpenForms) Bind(form);
            };
        }

        internal static void Menu(ToolStripDropDown menu)
        {
            if (!Enabled || !BoundMenus.Add(menu)) return;
            Dictionary<ToolStripItem, Padding> padding = new Dictionary<ToolStripItem, Padding>();
            menu.Disposed += delegate { BoundMenus.Remove(menu); };
            menu.Opening += delegate
            {
                foreach (ToolStripItem item in menu.Items)
                {
                    item.AccessibleName = T(item.Text);
                    item.ToolTipText = T(item.ToolTipText);
                    int extra = Math.Max(0, TextRenderer.MeasureText(T(item.Text), item.Font).Width - TextRenderer.MeasureText(item.Text, item.Font).Width);
                    Padding original;
                    if (!padding.TryGetValue(item, out original)) padding[item] = original = item.Padding;
                    item.Padding = new Padding(original.Left, original.Top, original.Right + extra, original.Bottom);
                }
            };
        }

        internal static int Count { get { return Words.Count; } }

        internal static string HelpFile(string folder)
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "ayuda-es.html");
            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream("Capri.Locale.Ayuda.html"))
            using (FileStream destination = File.Create(path)) source.CopyTo(destination);
            return path;
        }
    }

    // Both measurement and painting use the same Spanish string, keeping the original control's logical Text intact.
    internal static class UiText
    {
        internal static Size MeasureText(string text, Font font) { return TextRenderer.MeasureText(I18n.T(text), font); }
        internal static Size MeasureText(string text, Font font, Size size, TextFormatFlags flags) { return TextRenderer.MeasureText(I18n.T(text), font, size, flags); }
        internal static Size MeasureText(IDeviceContext dc, string text, Font font, Size size, TextFormatFlags flags) { return TextRenderer.MeasureText(dc, I18n.T(text), font, size, flags); }
        internal static void DrawText(IDeviceContext dc, string text, Font font, Rectangle box, Color color, TextFormatFlags flags) { TextRenderer.DrawText(dc, I18n.T(text), font, box, color, flags); }
    }

    internal class UiLabel : Label
    {
        private TextFormatFlags Flags
        {
            get
            {
                TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl;
                if (AutoEllipsis) flags |= TextFormatFlags.EndEllipsis;
                if (TextAlign == ContentAlignment.MiddleLeft || TextAlign == ContentAlignment.MiddleCenter || TextAlign == ContentAlignment.MiddleRight) flags |= TextFormatFlags.VerticalCenter;
                if (TextAlign == ContentAlignment.BottomLeft || TextAlign == ContentAlignment.BottomCenter || TextAlign == ContentAlignment.BottomRight) flags |= TextFormatFlags.Bottom;
                if (TextAlign == ContentAlignment.TopCenter || TextAlign == ContentAlignment.MiddleCenter || TextAlign == ContentAlignment.BottomCenter) flags |= TextFormatFlags.HorizontalCenter;
                if (TextAlign == ContentAlignment.TopRight || TextAlign == ContentAlignment.MiddleRight || TextAlign == ContentAlignment.BottomRight) flags |= TextFormatFlags.Right;
                return flags | TextFormatFlags.WordBreak;
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (!I18n.Enabled) return base.GetPreferredSize(proposedSize);
            int width = MaximumSize.Width > 0 ? MaximumSize.Width : proposedSize.Width > 1 ? proposedSize.Width : int.MaxValue;
            Size size = UiText.MeasureText(Text, Font, new Size(width, int.MaxValue), Flags);
            return new Size(size.Width + Padding.Horizontal, size.Height + Padding.Vertical);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!I18n.Enabled) { base.OnPaint(e); return; }
            Rectangle area = new Rectangle(Padding.Left, Padding.Top, Math.Max(0, Width - Padding.Horizontal), Math.Max(0, Height - Padding.Vertical));
            UiText.DrawText(e.Graphics, Text, Font, area, Enabled ? ForeColor : SystemColors.GrayText, Flags);
        }
    }
}
