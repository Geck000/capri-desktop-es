using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CapriDesktop
{
    internal static class LocaleTests
    {
        internal static int Run(string output)
        {
            Directory.CreateDirectory(output);
            if (File.Exists(Path.Combine(output, "fallo.txt"))) File.Delete(Path.Combine(output, "fallo.txt"));
            I18n.Enabled = true;
            I18n.Trace = true;
            List<string> checks = new List<string>();
            try
            {
                Require(I18n.T("Settings") == "Ajustes", "static translation");
                Require(I18n.T("Best fit for this PC: Gemma 4 E4B") == "Recomendada para tu PC: Gemma 4 E4B", "model name preserved");
                Require(I18n.T("C:\\Users\\Capri\\Settings.json") == "C:\\Users\\Capri\\Settings.json", "paths preserved");
                Require(I18n.T("Ollama · gemma4:e4b") == "Ollama · gemma4:e4b", "provider and model IDs preserved");
                Require(I18n.T("Ctrl + Alt + V") == "Ctrl + Alt + V", "shortcuts preserved");
                checks.Add("PASS catálogo, traducción, modelos, rutas y atajos");
                Meeting meeting = new Meeting();
                string called;
                meeting.Start();
                meeting.Take("me llamo Álex", out called);
                Require(called == "Álex", "nombre en español");
                Require(Meeting.Pronouns("mis pronombres son él") == "he/him", "pronombres en español");
                meeting.Take("saltar", out called);
                meeting.Take("parar", out called);
                Require(!meeting.Asking, "saltar y parar en español");
                Require(I18n.T(Meeting.AskName).Contains("¿Cómo quieres que te llame?"), "presentación en español");
                Require(File.ReadAllText(I18n.HelpFile(output)).Contains("<html lang=\"es\">"), "ayuda española incluida");
                checks.Add("PASS presentación, nombre, pronombres, saltar/parar y ayuda local");
                CareLines care = new CareLines(Path.Combine(output, "recordatorios.md"));
                Require(care.Create(), "recordatorios españoles");
                Require(File.ReadAllText(Path.Combine(output, "recordatorios.md")).Contains("## Agua"), "encabezados españoles");
                Require(!care.Pick(CareKind.Water).Contains("water"), "selección de recordatorios españoles");
                using (EventWaitHandle signal = new EventWaitHandle(false, EventResetMode.AutoReset))
                using (Companion pet = new Companion(Path.Combine(output, "profile"), signal, true))
                using (Mainframe frame = new Mainframe(pet, SystemIcons.Application, "0.7.8"))
                {
                    pet.SetLanguage("Spanish");
                    Machine machine = new Machine { Ram = 32L << 30, Vram = 8L << 30, Gpu = "RTX 2070" };
                    frame.ShowSetup(machine, Engine.Fit(machine), false, null);
                    frame.Show();
                    Application.DoEvents();
                    I18n.Bind(frame);
                    Capture(frame, output, "panel");
                    Check(frame, "panel", checks);
                    frame.ShowSettings(true);
                    Application.DoEvents();
                    Capture(frame, output, "ajustes");
                    Check(frame, "ajustes", checks);
                    foreach (int dpi in new[] { 96, 144, 192 })
                        foreach (int percent in new[] { 100, 150, 200 })
                        {
                            frame.LayoutScale(dpi, 100);
                            frame.SizeMainframe(percent);
                            foreach (bool page in new[] { false, true })
                            {
                                frame.ShowSettings(page);
                                string problem = frame.TextFits();
                                Require(problem == null, dpi + " DPI / " + percent + "%: " + problem);
                            }
                        }
                    frame.LayoutScale(0, 0);
                    frame.SizeMainframe(100);
                    frame.ShowSettings(true);
                    checks.Add("PASS panel y ajustes al 100, 150 y 200% con DPI 96, 144 y 192");
                    frame.SetUpCustom();
                    Application.DoEvents();
                    Capture(frame.ServerPanel, output, "modelo-de-voz");
                    frame.ServerPanel.Close();
                    frame.ShowSecurity();
                    Application.DoEvents();
                    foreach (Form form in Application.OpenForms) if (form != frame && form.GetType().Name == "SecuritySheet") Capture(form, output, "seguridad");
                    frame.CloseSecurity();
                    foreach (float zoom in new[] { 1f, 1.5f, 2f })
                    {
                        string securityProblem = frame.SecurityAt(zoom, Path.Combine(output, "seguridad-" + (int)(zoom * 100) + ".png"));
                        Require(securityProblem == null, "seguridad al " + zoom + ": " + securityProblem);
                    }
                    frame.ShowLayersWindow();
                    Application.DoEvents();
                    Require(frame.LayersCrowded == null, "capas: " + frame.LayersCrowded);
                    foreach (Form form in Application.OpenForms) if (form != frame && form.GetType().Name == "LayersSheet") Capture(form, output, "capas");
                    frame.CloseLayers();
                    foreach (float zoom in new[] { 1.25f, 1.5f, 2f })
                    {
                        string layerProblem = frame.LayersAt(zoom, Path.Combine(output, "capas-" + (int)(zoom * 100) + ".png"));
                        Require(layerProblem == null, "capas al " + zoom + ": " + layerProblem);
                    }
                    frame.ShowExtras();
                    Application.DoEvents();
                    Require(frame.ExtrasCrowded == null, "extras: " + frame.ExtrasCrowded);
                    foreach (Form form in Application.OpenForms) if (form != frame && form.GetType().Name == "ExtrasSheet") Capture(form, output, "extras");
                    frame.CloseExtras();
                    checks.Add("PASS ventanas de seguridad, capas y extras en español");
                    frame.Hide();
                }
                using (Notice notice = new Notice("Free voice, runs on your PC.", "Voice", false))
                {
                    Capture(notice, output, "aviso");
                    Require(notice.Fits, "aviso");
                    checks.Add("PASS avisos en español sin recortes");
                }
                File.WriteAllLines(Path.Combine(output, "resultado.txt"), checks);
                File.WriteAllLines(Path.Combine(output, "revisar-idioma.txt"), I18n.Untranslated);
                return 0;
            }
            catch (Exception error)
            {
                File.WriteAllLines(Path.Combine(output, "resultado.txt"), checks);
                File.WriteAllText(Path.Combine(output, "fallo.txt"), error.ToString());
                File.WriteAllLines(Path.Combine(output, "revisar-idioma.txt"), I18n.Untranslated);
                return 1;
            }
        }

        private static void Check(Mainframe frame, string name, List<string> checks)
        {
            string fit = frame.TextFits();
            Require(fit == null, name + ": " + fit);
            checks.Add("PASS textos y controles sin solapamientos: " + name);
        }

        private static void Capture(Form form, string output, string name)
        {
            I18n.Bind(form);
            form.CreateControl();
            using (Bitmap image = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                image.Save(Path.Combine(output, name + ".png"));
            }
        }

        private static void Require(bool fact, string name) { if (!fact) throw new Exception(name); }
    }
}
