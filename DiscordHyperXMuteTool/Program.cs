using System;
using System.Drawing;
using System.Windows.Forms;
using DiscordHyperXMuteTool.Properties;

namespace DiscordHyperXMuteTool
{
    internal static class Program
    {
        public static readonly State State = new State();
        public static readonly Settings Settings = new Settings();

        [STAThread]
        private static void Main()
        {
            Settings.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApplicationContext());
            State.Dispose();
        }
    }

    internal sealed class TrayApplicationContext : ApplicationContext
    {
        private sealed class ToolStripMenuLabelItem : ToolStripMenuItem
        {
            public override bool CanSelect => false;
            protected override void OnClick(EventArgs e) { }

            public ToolStripMenuLabelItem() { }
            public ToolStripMenuLabelItem(string text) : base(text) { }
            public ToolStripMenuLabelItem(string text, Image image) : base(text, image) { }
        }

        private readonly NotifyIcon _icon;
        private readonly ContextMenuStrip _menu;

        private readonly Image _menuImage;

        private readonly IDisposable _stateSubscription;
        private readonly IDisposable _settingsSubscription;

        private SettingsForm _settingsForm;

        public TrayApplicationContext()
        {
            var menuNgenuityStatus = new ToolStripMenuLabelItem();
            var menuDiscordStatus = new ToolStripMenuLabelItem();
            var menuMicrophoneStatus = new ToolStripMenuLabelItem();
            var menuEnabledCheckbox = new ToolStripMenuItem("Enabled?", null, OnClickEnabled);
            var menuStartupCheckbox = new ToolStripMenuItem("Run on startup?", null, OnClickStartup);

            _menu = new ContextMenuStrip();
            _menu.Items.Add(new ToolStripMenuLabelItem(Application.ProductName, _menuImage = Resources.MenuIcon.ToBitmap()));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(menuNgenuityStatus);
            _menu.Items.Add(menuDiscordStatus);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(menuMicrophoneStatus);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(menuEnabledCheckbox);
            _menu.Items.Add(menuStartupCheckbox);
            _menu.Items.Add(new ToolStripMenuItem("Settings", null, OnClickSettings));
            _menu.Items.Add(new ToolStripMenuItem("Quit", null, OnClickQuit));

            _stateSubscription = Program.State.SubscribeComputed(state =>
            {
                menuNgenuityStatus.Text = state.NgenuityStatusText;
                menuNgenuityStatus.Image = state.NgenuityStatusImage;
                menuDiscordStatus.Text = state.DiscordStatusText;
                menuDiscordStatus.Image = state.DiscordStatusImage;
                menuMicrophoneStatus.Text = state.MicrophoneStatusText;
                menuMicrophoneStatus.Image = state.MicrophoneStatusImage;
            });

            _settingsSubscription = Program.Settings.SubscribeComputed(settings =>
            {
                menuEnabledCheckbox.Checked = settings.Enabled;
                menuStartupCheckbox.Checked = settings.RunOnStartup;
            });

            _icon = new NotifyIcon()
            {
                Text = Application.ProductName,
                Icon = Resources.Icon,
                ContextMenuStrip = _menu,
                Visible = true
            };
        }

        private void OnClickEnabled(object sender, EventArgs e)
        {
            Program.Settings.Enabled = !Program.Settings.Enabled;
            Program.Settings.Update();
        }
        private void OnClickStartup(object sender, EventArgs e)
        {
            Program.Settings.RunOnStartup = !Program.Settings.RunOnStartup;
            Program.Settings.Update();
        }

        private void OnClickSettings(object sender, EventArgs e)
        {
            if (_settingsForm != null)
            {
                _settingsForm.Focus();
                _settingsForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                _settingsForm = new SettingsForm();
                _settingsForm.FormClosed += (sender_, e_) =>
                {
                    _settingsForm = null;
                };
                _settingsForm.Show();
            }
        }

        private void OnClickQuit(object sender, EventArgs e)
        {
            _icon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stateSubscription?.Dispose();
                _settingsSubscription?.Dispose();
                _settingsForm?.Dispose();
                _icon?.Dispose();
                _menu?.Dispose();
                _menuImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
