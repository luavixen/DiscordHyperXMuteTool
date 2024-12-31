namespace DiscordHyperXMuteTool
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.titleLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.ngenuityStatusPictureBox = new System.Windows.Forms.PictureBox();
            this.microphoneStatusPictureBox = new System.Windows.Forms.PictureBox();
            this.discordStatusPictureBox = new System.Windows.Forms.PictureBox();
            this.ngenuityStatusLabel = new System.Windows.Forms.Label();
            this.microphoneStatusLabel = new System.Windows.Forms.Label();
            this.discordStatusLabel = new System.Windows.Forms.Label();
            this.enabledCheckBox = new System.Windows.Forms.CheckBox();
            this.runOnStartupCheckBox = new System.Windows.Forms.CheckBox();
            this.syncWithDiscordCheckBox = new System.Windows.Forms.CheckBox();
            this.enabledLabel = new System.Windows.Forms.Label();
            this.runOnStartupLabel = new System.Windows.Forms.Label();
            this.syncWithDiscordLabel = new System.Windows.Forms.Label();
            this.onMuteKeyComboBox = new System.Windows.Forms.ComboBox();
            this.onUnmuteKeyComboBox = new System.Windows.Forms.ComboBox();
            this.onMuteKeyLabel = new System.Windows.Forms.Label();
            this.onUnmuteKeyLabel = new System.Windows.Forms.Label();
            this.toggleHintLabel = new System.Windows.Forms.Label();
            this.copyrightLabel1 = new System.Windows.Forms.Label();
            this.copyrightLabel4 = new System.Windows.Forms.Label();
            this.copyrightLabel3 = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.copyrightLinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.copyrightLabel2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ngenuityStatusPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.microphoneStatusPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.discordStatusPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
            this.pictureBox.Location = new System.Drawing.Point(12, 12);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(132, 132);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(150, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(246, 25);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "DiscordHyperXMuteTool";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.Location = new System.Drawing.Point(152, 37);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(385, 50);
            this.descriptionLabel.TabIndex = 2;
            this.descriptionLabel.Text = "Synchronizes your HyperX microphone\'s hardware mute state with Discord and other " +
    "applications by simulating customizable keypresses when you tap your microphone\'" +
    "s mute button.";
            // 
            // ngenuityStatusPictureBox
            // 
            this.ngenuityStatusPictureBox.Location = new System.Drawing.Point(155, 90);
            this.ngenuityStatusPictureBox.Name = "ngenuityStatusPictureBox";
            this.ngenuityStatusPictureBox.Size = new System.Drawing.Size(24, 24);
            this.ngenuityStatusPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ngenuityStatusPictureBox.TabIndex = 3;
            this.ngenuityStatusPictureBox.TabStop = false;
            // 
            // microphoneStatusPictureBox
            // 
            this.microphoneStatusPictureBox.Location = new System.Drawing.Point(155, 120);
            this.microphoneStatusPictureBox.Name = "microphoneStatusPictureBox";
            this.microphoneStatusPictureBox.Size = new System.Drawing.Size(24, 24);
            this.microphoneStatusPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.microphoneStatusPictureBox.TabIndex = 4;
            this.microphoneStatusPictureBox.TabStop = false;
            // 
            // discordStatusPictureBox
            // 
            this.discordStatusPictureBox.Location = new System.Drawing.Point(335, 90);
            this.discordStatusPictureBox.Name = "discordStatusPictureBox";
            this.discordStatusPictureBox.Size = new System.Drawing.Size(24, 24);
            this.discordStatusPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.discordStatusPictureBox.TabIndex = 5;
            this.discordStatusPictureBox.TabStop = false;
            // 
            // ngenuityStatusLabel
            // 
            this.ngenuityStatusLabel.Location = new System.Drawing.Point(185, 90);
            this.ngenuityStatusLabel.Name = "ngenuityStatusLabel";
            this.ngenuityStatusLabel.Size = new System.Drawing.Size(144, 24);
            this.ngenuityStatusLabel.TabIndex = 6;
            this.ngenuityStatusLabel.Text = "Label";
            this.ngenuityStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // microphoneStatusLabel
            // 
            this.microphoneStatusLabel.Location = new System.Drawing.Point(185, 120);
            this.microphoneStatusLabel.Name = "microphoneStatusLabel";
            this.microphoneStatusLabel.Size = new System.Drawing.Size(144, 24);
            this.microphoneStatusLabel.TabIndex = 7;
            this.microphoneStatusLabel.Text = "Label";
            this.microphoneStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // discordStatusLabel
            // 
            this.discordStatusLabel.Location = new System.Drawing.Point(365, 90);
            this.discordStatusLabel.Name = "discordStatusLabel";
            this.discordStatusLabel.Size = new System.Drawing.Size(144, 24);
            this.discordStatusLabel.TabIndex = 8;
            this.discordStatusLabel.Text = "Label";
            this.discordStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // enabledCheckBox
            // 
            this.enabledCheckBox.AutoSize = true;
            this.enabledCheckBox.Location = new System.Drawing.Point(12, 150);
            this.enabledCheckBox.Name = "enabledCheckBox";
            this.enabledCheckBox.Size = new System.Drawing.Size(65, 17);
            this.enabledCheckBox.TabIndex = 9;
            this.enabledCheckBox.Text = "Enabled";
            this.enabledCheckBox.UseVisualStyleBackColor = true;
            this.enabledCheckBox.CheckedChanged += new System.EventHandler(this.enabledCheckBox_CheckedChanged);
            // 
            // runOnStartupCheckBox
            // 
            this.runOnStartupCheckBox.AutoSize = true;
            this.runOnStartupCheckBox.Location = new System.Drawing.Point(12, 190);
            this.runOnStartupCheckBox.Name = "runOnStartupCheckBox";
            this.runOnStartupCheckBox.Size = new System.Drawing.Size(96, 17);
            this.runOnStartupCheckBox.TabIndex = 10;
            this.runOnStartupCheckBox.Text = "Run on startup";
            this.runOnStartupCheckBox.UseVisualStyleBackColor = true;
            this.runOnStartupCheckBox.CheckedChanged += new System.EventHandler(this.runOnStartupCheckBox_CheckedChanged);
            // 
            // syncWithDiscordCheckBox
            // 
            this.syncWithDiscordCheckBox.AutoSize = true;
            this.syncWithDiscordCheckBox.Location = new System.Drawing.Point(12, 230);
            this.syncWithDiscordCheckBox.Name = "syncWithDiscordCheckBox";
            this.syncWithDiscordCheckBox.Size = new System.Drawing.Size(111, 17);
            this.syncWithDiscordCheckBox.TabIndex = 11;
            this.syncWithDiscordCheckBox.Text = "Sync with Discord";
            this.syncWithDiscordCheckBox.UseVisualStyleBackColor = true;
            this.syncWithDiscordCheckBox.CheckedChanged += new System.EventHandler(this.syncWithDiscordCheckBox_CheckedChanged);
            // 
            // enabledLabel
            // 
            this.enabledLabel.AutoSize = true;
            this.enabledLabel.Location = new System.Drawing.Point(12, 170);
            this.enabledLabel.Name = "enabledLabel";
            this.enabledLabel.Size = new System.Drawing.Size(293, 13);
            this.enabledLabel.TabIndex = 12;
            this.enabledLabel.Text = "Should DiscordHyperXMuteTool be pressing keys right now?";
            // 
            // runOnStartupLabel
            // 
            this.runOnStartupLabel.AutoSize = true;
            this.runOnStartupLabel.Location = new System.Drawing.Point(12, 210);
            this.runOnStartupLabel.Name = "runOnStartupLabel";
            this.runOnStartupLabel.Size = new System.Drawing.Size(289, 13);
            this.runOnStartupLabel.TabIndex = 13;
            this.runOnStartupLabel.Text = "Should DiscordHyperXMuteTool start itself when you log in?";
            // 
            // syncWithDiscordLabel
            // 
            this.syncWithDiscordLabel.AutoSize = true;
            this.syncWithDiscordLabel.Location = new System.Drawing.Point(12, 250);
            this.syncWithDiscordLabel.Name = "syncWithDiscordLabel";
            this.syncWithDiscordLabel.Size = new System.Drawing.Size(406, 13);
            this.syncWithDiscordLabel.TabIndex = 14;
            this.syncWithDiscordLabel.Text = "Should DiscordHyperXMuteTool consider Discord\'s mute state before pressing keys?";
            // 
            // onMuteKeyComboBox
            // 
            this.onMuteKeyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.onMuteKeyComboBox.FormattingEnabled = true;
            this.onMuteKeyComboBox.Location = new System.Drawing.Point(150, 276);
            this.onMuteKeyComboBox.Name = "onMuteKeyComboBox";
            this.onMuteKeyComboBox.Size = new System.Drawing.Size(121, 21);
            this.onMuteKeyComboBox.TabIndex = 15;
            this.onMuteKeyComboBox.SelectedIndexChanged += new System.EventHandler(this.onMuteKeyComboBox_SelectedIndexChanged);
            // 
            // onUnmuteKeyComboBox
            // 
            this.onUnmuteKeyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.onUnmuteKeyComboBox.FormattingEnabled = true;
            this.onUnmuteKeyComboBox.Location = new System.Drawing.Point(150, 303);
            this.onUnmuteKeyComboBox.Name = "onUnmuteKeyComboBox";
            this.onUnmuteKeyComboBox.Size = new System.Drawing.Size(121, 21);
            this.onUnmuteKeyComboBox.TabIndex = 16;
            this.onUnmuteKeyComboBox.SelectedIndexChanged += new System.EventHandler(this.onUnmuteKeyComboBox_SelectedIndexChanged);
            // 
            // onMuteKeyLabel
            // 
            this.onMuteKeyLabel.Location = new System.Drawing.Point(12, 276);
            this.onMuteKeyLabel.Name = "onMuteKeyLabel";
            this.onMuteKeyLabel.Size = new System.Drawing.Size(132, 21);
            this.onMuteKeyLabel.TabIndex = 17;
            this.onMuteKeyLabel.Text = "Key to press on mute:";
            this.onMuteKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // onUnmuteKeyLabel
            // 
            this.onUnmuteKeyLabel.Location = new System.Drawing.Point(12, 303);
            this.onUnmuteKeyLabel.Name = "onUnmuteKeyLabel";
            this.onUnmuteKeyLabel.Size = new System.Drawing.Size(132, 21);
            this.onUnmuteKeyLabel.TabIndex = 18;
            this.onUnmuteKeyLabel.Text = "Key to press on (un)mute:";
            this.onUnmuteKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toggleHintLabel
            // 
            this.toggleHintLabel.Location = new System.Drawing.Point(277, 276);
            this.toggleHintLabel.Name = "toggleHintLabel";
            this.toggleHintLabel.Size = new System.Drawing.Size(260, 48);
            this.toggleHintLabel.TabIndex = 19;
            this.toggleHintLabel.Text = "For Discord, set both of these to the same key and create a \"Toggle Mute\" keybind" +
    " in Discord\'s settings.";
            this.toggleHintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // copyrightLabel1
            // 
            this.copyrightLabel1.AutoSize = true;
            this.copyrightLabel1.Location = new System.Drawing.Point(12, 340);
            this.copyrightLabel1.Name = "copyrightLabel1";
            this.copyrightLabel1.Size = new System.Drawing.Size(275, 13);
            this.copyrightLabel1.TabIndex = 20;
            this.copyrightLabel1.Text = "DiscordHyperXMuteTool Copyright © 2025 Lua Software";
            // 
            // copyrightLabel4
            // 
            this.copyrightLabel4.Location = new System.Drawing.Point(12, 405);
            this.copyrightLabel4.Name = "copyrightLabel4";
            this.copyrightLabel4.Size = new System.Drawing.Size(525, 45);
            this.copyrightLabel4.TabIndex = 22;
            this.copyrightLabel4.Text = resources.GetString("copyrightLabel4.Text");
            // 
            // copyrightLabel3
            // 
            this.copyrightLabel3.Location = new System.Drawing.Point(12, 360);
            this.copyrightLabel3.Name = "copyrightLabel3";
            this.copyrightLabel3.Size = new System.Drawing.Size(525, 45);
            this.copyrightLabel3.TabIndex = 23;
            this.copyrightLabel3.Text = resources.GetString("copyrightLabel3.Text");
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(402, 20);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(28, 13);
            this.versionLabel.TabIndex = 24;
            this.versionLabel.Text = "v1.0";
            // 
            // copyrightLinkLabel1
            // 
            this.copyrightLinkLabel1.AutoSize = true;
            this.copyrightLinkLabel1.Location = new System.Drawing.Point(285, 340);
            this.copyrightLinkLabel1.Name = "copyrightLinkLabel1";
            this.copyrightLinkLabel1.Size = new System.Drawing.Size(55, 13);
            this.copyrightLinkLabel1.TabIndex = 25;
            this.copyrightLinkLabel1.TabStop = true;
            this.copyrightLinkLabel1.Text = "foxgirl.dev";
            this.copyrightLinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.copyrightLinkLabel1_LinkClicked);
            // 
            // copyrightLabel2
            // 
            this.copyrightLabel2.AutoSize = true;
            this.copyrightLabel2.Location = new System.Drawing.Point(346, 340);
            this.copyrightLabel2.Name = "copyrightLabel2";
            this.copyrightLabel2.Size = new System.Drawing.Size(133, 13);
            this.copyrightLabel2.TabIndex = 21;
            this.copyrightLabel2.Text = "Iconography by CAGspecs";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 456);
            this.Controls.Add(this.copyrightLinkLabel1);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.copyrightLabel3);
            this.Controls.Add(this.copyrightLabel4);
            this.Controls.Add(this.copyrightLabel2);
            this.Controls.Add(this.copyrightLabel1);
            this.Controls.Add(this.toggleHintLabel);
            this.Controls.Add(this.onUnmuteKeyLabel);
            this.Controls.Add(this.onMuteKeyLabel);
            this.Controls.Add(this.onUnmuteKeyComboBox);
            this.Controls.Add(this.onMuteKeyComboBox);
            this.Controls.Add(this.syncWithDiscordLabel);
            this.Controls.Add(this.runOnStartupLabel);
            this.Controls.Add(this.enabledLabel);
            this.Controls.Add(this.syncWithDiscordCheckBox);
            this.Controls.Add(this.runOnStartupCheckBox);
            this.Controls.Add(this.enabledCheckBox);
            this.Controls.Add(this.discordStatusLabel);
            this.Controls.Add(this.microphoneStatusLabel);
            this.Controls.Add(this.ngenuityStatusLabel);
            this.Controls.Add(this.discordStatusPictureBox);
            this.Controls.Add(this.microphoneStatusPictureBox);
            this.Controls.Add(this.ngenuityStatusPictureBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.pictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "DiscordHyperXMuteTool";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ngenuityStatusPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.microphoneStatusPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.discordStatusPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.PictureBox ngenuityStatusPictureBox;
        private System.Windows.Forms.PictureBox microphoneStatusPictureBox;
        private System.Windows.Forms.PictureBox discordStatusPictureBox;
        private System.Windows.Forms.Label ngenuityStatusLabel;
        private System.Windows.Forms.Label microphoneStatusLabel;
        private System.Windows.Forms.Label discordStatusLabel;
        private System.Windows.Forms.CheckBox enabledCheckBox;
        private System.Windows.Forms.CheckBox runOnStartupCheckBox;
        private System.Windows.Forms.CheckBox syncWithDiscordCheckBox;
        private System.Windows.Forms.Label enabledLabel;
        private System.Windows.Forms.Label runOnStartupLabel;
        private System.Windows.Forms.Label syncWithDiscordLabel;
        private System.Windows.Forms.ComboBox onMuteKeyComboBox;
        private System.Windows.Forms.ComboBox onUnmuteKeyComboBox;
        private System.Windows.Forms.Label onMuteKeyLabel;
        private System.Windows.Forms.Label onUnmuteKeyLabel;
        private System.Windows.Forms.Label toggleHintLabel;
        private System.Windows.Forms.Label copyrightLabel1;
        private System.Windows.Forms.Label copyrightLabel4;
        private System.Windows.Forms.Label copyrightLabel3;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.LinkLabel copyrightLinkLabel1;
        private System.Windows.Forms.Label copyrightLabel2;
    }
}