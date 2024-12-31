using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DiscordHyperXMuteTool
{
    public partial class SettingsForm : Form
    {
        private IDisposable _stateSubscription;
        private IDisposable _settingsSubscription;

        public SettingsForm()
        {
            InitializeComponent();

            titleLabel.Text = Application.ProductName;
            versionLabel.Text = $"v{Application.ProductVersion}";

            onMuteKeyComboBox.DataSource = Enum.GetNames(typeof(Keycode));
            onUnmuteKeyComboBox.DataSource = Enum.GetNames(typeof(Keycode));
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            _stateSubscription = Program.State.SubscribeComputed(state =>
            {
                ngenuityStatusPictureBox.Image = state.NgenuityStatusImage;
                ngenuityStatusLabel.Text = state.NgenuityStatusText;
                discordStatusPictureBox.Image = state.DiscordStatusImage;
                discordStatusLabel.Text = state.DiscordStatusText;
                microphoneStatusPictureBox.Image = state.MicrophoneStatusImage;
                microphoneStatusLabel.Text = state.MicrophoneStatusText;
            });

            _settingsSubscription = Program.Settings.SubscribeComputed(settings =>
            {
                enabledCheckBox.Checked = settings.Enabled;
                runOnStartupCheckBox.Checked = settings.RunOnStartup;
                syncWithDiscordCheckBox.Checked = settings.SyncWithDiscord;
                if (Enum.IsDefined(typeof(Keycode), settings.OnMuteKey))
                {
                    onMuteKeyComboBox.SelectedItem = settings.OnMuteKey.ToString();
                }
                if (Enum.IsDefined(typeof(Keycode), settings.OnUnmuteKey))
                {
                    onUnmuteKeyComboBox.SelectedItem = settings.OnUnmuteKey.ToString();
                }
            });

            Disposed += (sender_, e_) =>
            {
                _stateSubscription?.Dispose();
                _settingsSubscription?.Dispose();
            };
        }

        private void enabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!Created) return;
            Program.Settings.Enabled = enabledCheckBox.Checked;
            Program.Settings.Update();
        }
        private void runOnStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!Created) return;
            Program.Settings.RunOnStartup = runOnStartupCheckBox.Checked;
            Program.Settings.Update();
        }
        private void syncWithDiscordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!Created) return;
            Program.Settings.SyncWithDiscord = syncWithDiscordCheckBox.Checked;
            Program.Settings.Update();
        }

        private void onMuteKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Created) return;
            if (Enum.TryParse(onMuteKeyComboBox.SelectedItem.ToString(), out Keycode result))
            {
                Program.Settings.OnMuteKey = result;
                Program.Settings.Update();
            }
        }
        private void onUnmuteKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Created) return;
            if (Enum.TryParse(onUnmuteKeyComboBox.SelectedItem.ToString(), out Keycode result))
            {
                Program.Settings.OnUnmuteKey = result;
                Program.Settings.Update();
            }
        }

        private void copyrightLinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "https://foxgirl.dev/");
        }
    }
}
