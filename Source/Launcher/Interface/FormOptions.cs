/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;
using Vortice.Direct3D9;

namespace CodeImp.Bloodmasters.Launcher
{
	public class FormOptions : System.Windows.Forms.Form
	{
		private int last_fsaa;
		private DisplayMode last_mode;
		private ListDictionary controlkeys = new ListDictionary();
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabsOptions;
		private System.Windows.Forms.TabPage tabGraphics;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbResolution;
		private System.Windows.Forms.ComboBox cmbAdapter;
		private System.Windows.Forms.CheckBox chkWindowed;
		private System.Windows.Forms.CheckBox chkSyncRate;
		private System.Windows.Forms.CheckBox chkShowFPS;
		private System.Windows.Forms.CheckBox chkShowDecals;
		private System.Windows.Forms.TabPage tabControls;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtPlayerName;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.NumericUpDown txtClientPort;
		private System.Windows.Forms.CheckBox chkFixedPort;
		private System.Windows.Forms.ListView lstControls;
		private System.Windows.Forms.ColumnHeader clmControl;
		private System.Windows.Forms.ComboBox cmbControl;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblControl;
		private System.Windows.Forms.Label lblAction;
		private System.Windows.Forms.TabPage tabSound;
		private System.Windows.Forms.CheckBox chkPlayMusic;
		private System.Windows.Forms.CheckBox chkRandomMusic;
		private System.Windows.Forms.ColumnHeader clmAction;
		private System.Windows.Forms.TrackBar trkMusicVolume;
		private System.Windows.Forms.CheckBox chkPlaySounds;
		private System.Windows.Forms.CheckBox chkPlayChatBeep;
		private System.Windows.Forms.CheckBox chkPlayTeamBeep;
		private System.Windows.Forms.TrackBar trkSoundVolume;
		private System.Windows.Forms.Label lblMusicVolumeLabel;
		private System.Windows.Forms.Label lblMusicVolume;
		private System.Windows.Forms.Label lblSoundVolumeLabel;
		private System.Windows.Forms.Label lblSoundVolume;
		private System.Windows.Forms.Label lblQuerySpeedLabel;
		private System.Windows.Forms.Label lblQuerySpeed;
		private System.Windows.Forms.TrackBar trkQuerySpeed;
		private System.Windows.Forms.Label lblSnapsSpeedLabel;
		private System.Windows.Forms.Label lblSnapsSpeed;
		private System.Windows.Forms.TrackBar trkSnapsSpeed;
		private System.Windows.Forms.TrackBar trkGamma;
		private System.Windows.Forms.Label lblGammaValue;
		private System.Windows.Forms.Label lblGamma;
		private System.Windows.Forms.CheckBox chkStartRefresh;
		private System.Windows.Forms.CheckBox chkDynamicLights;
		private System.Windows.Forms.CheckBox chkHighTextures;
		private System.Windows.Forms.Label lblFSAA;
		private System.Windows.Forms.ComboBox cmbFSAA;
		private System.Windows.Forms.CheckBox chkShowGibs;
		private System.Windows.Forms.CheckBox chkAutoScreenshot;
		private System.Windows.Forms.CheckBox chkAutoDownload;
		private System.Windows.Forms.CheckBox chkLaserIntensity;
		private System.Windows.Forms.CheckBox chkTeamColoredNames;
		private System.Windows.Forms.GroupBox grpControlOptions;
		private System.Windows.Forms.CheckBox chkScrollWeapons;
		private System.Windows.Forms.CheckBox chkAutoSwitchWeapon;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cmbMoveMethod;
		private System.Windows.Forms.CheckBox chkScreenFlashes;
		private System.Windows.Forms.Label lblControlDesc;

		// Constructor
		public FormOptions()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Setup General
			txtPlayerName.Text = General.playername;
			chkFixedPort.Checked = General.config.ReadSetting("fixedclientport", false);
			chkStartRefresh.Checked = General.config.ReadSetting("startuprefresh", true);
			txtClientPort.Value = General.config.ReadSetting("clientport", 7777);
			trkQuerySpeed.Value = General.config.ReadSetting("queryspeed", 50);
			trkSnapsSpeed.Value = General.config.ReadSetting("snapsspeed", 20);
			chkAutoScreenshot.Checked = General.config.ReadSetting("autoscreenshot", false);
			chkAutoDownload.Checked = General.config.ReadSetting("autodownload", true);
			chkAutoSwitchWeapon.Checked = General.config.ReadSetting("autoswitchweapon", true);
			chkLaserIntensity.Checked = (General.config.ReadSetting("laserintensity", 2) > 0);
			if(chkLaserIntensity.Checked) chkLaserIntensity.Tag = General.config.ReadSetting("laserintensity", 2); else chkLaserIntensity.Tag = 2;
			chkTeamColoredNames.Checked = General.config.ReadSetting("teamcolorednames", false);

			// Fill the controls combobox with some special keys
			cmbControl.Items.Add(Keys.None);
			cmbControl.Items.Add(Keys.LButton);
			cmbControl.Items.Add(Keys.RButton);
			cmbControl.Items.Add(Keys.MButton);
			cmbControl.Items.Add(Keys.XButton1);
			cmbControl.Items.Add(Keys.XButton2);
			cmbControl.Items.Add(EXTRAKEYS.MScrollUp);
			cmbControl.Items.Add(EXTRAKEYS.MScrollDown);

			// Go for all controls
			foreach(ListViewItem c in lstControls.Items)
			{
				// Get the control key
				int keycode = General.config.ReadSetting("controls/" + c.Tag, (int)Keys.None);
				if(c.SubItems.Count < 2) c.SubItems.Add("");
				c.SubItems[1].Text = InputKey.GetKeyName(keycode);
				controlkeys.Add(c.Tag, keycode);
			}

			// Other options in control
			chkScrollWeapons.Checked = General.config.ReadSetting("scrollweapons", true);
			cmbMoveMethod.SelectedIndex = General.config.ReadSetting("movemethod", 0);

			// Setup Graphics
			last_mode = Direct3D.DisplayMode;
			last_fsaa = Direct3D.DisplayFSAA;
			Direct3D.FillAdaptersList(cmbAdapter);
			chkWindowed.Checked = Direct3D.DisplayWindowed;
			chkSyncRate.Checked = Direct3D.DisplaySyncRefresh;
			chkShowFPS.Checked = General.config.ReadSetting("showfps", false);
			chkDynamicLights.Checked = General.config.ReadSetting("dynamiclights", true);
			chkShowDecals.Checked = General.config.ReadSetting("showdecals", true);
			chkShowGibs.Checked = General.config.ReadSetting("showgibs", true);
			chkScreenFlashes.Checked = General.config.ReadSetting("screenflashes", true);
			chkHighTextures.Checked = General.config.ReadSetting("hightextures", true);
			trkGamma.Value = Direct3D.DisplayGamma;

			// Setup Sound
			chkPlaySounds.Checked = General.config.ReadSetting("sounds", true);
			chkPlayChatBeep.Checked = General.config.ReadSetting("soundchatbeep", true);
			chkPlayTeamBeep.Checked = General.config.ReadSetting("soundteambeep", true);
			trkSoundVolume.Value = General.config.ReadSetting("soundsvolume", 100);
			chkPlayMusic.Checked = General.config.ReadSetting("music", true);
			chkRandomMusic.Checked = General.config.ReadSetting("musicrandom", true);
			trkMusicVolume.Value = General.config.ReadSetting("musicvolume", 50);
		}

		// Clean up any resources being used.
		protected override void Dispose( bool disposing )
		{
			// Check if disposing
			if(disposing)
			{
				// Dispose components, if any
				if(components != null) components.Dispose();
			}

			// Let superior class know about the dispose
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Walk Up");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Walk Down");
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Walk Left");
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Walk Right");
			System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Next Weapon");
			System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Previous Weapon");
			System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Fire Weapon");
			System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Fire Powerup");
			System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Respawn");
			System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("Show Scores");
			System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("Console");
			System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("Chat");
			System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("Team Chat");
			System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem("Use SMG");
			System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem("Use Minigun");
			System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem("Use Plasma Cannon");
			System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem("Use Rocket Launcher");
			System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem("Use Grenade Launcher");
			System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem("Use Phoenix");
			System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem("Use Ion Cannon");
			System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem("Join Spectators");
			System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem("Join Game");
			System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem("Join Red Team");
			System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem("Join Blue Team");
			System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem("Vote");
			System.Windows.Forms.ListViewItem listViewItem26 = new System.Windows.Forms.ListViewItem("Screenshot");
			System.Windows.Forms.ListViewItem listViewItem27 = new System.Windows.Forms.ListViewItem("Commit Suicide");
			System.Windows.Forms.ListViewItem listViewItem28 = new System.Windows.Forms.ListViewItem("Game Menu");
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabsOptions = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.chkAutoSwitchWeapon = new System.Windows.Forms.CheckBox();
			this.chkLaserIntensity = new System.Windows.Forms.CheckBox();
			this.chkTeamColoredNames = new System.Windows.Forms.CheckBox();
			this.chkAutoDownload = new System.Windows.Forms.CheckBox();
			this.chkAutoScreenshot = new System.Windows.Forms.CheckBox();
			this.lblSnapsSpeedLabel = new System.Windows.Forms.Label();
			this.lblSnapsSpeed = new System.Windows.Forms.Label();
			this.trkSnapsSpeed = new System.Windows.Forms.TrackBar();
			this.lblQuerySpeedLabel = new System.Windows.Forms.Label();
			this.lblQuerySpeed = new System.Windows.Forms.Label();
			this.trkQuerySpeed = new System.Windows.Forms.TrackBar();
			this.txtClientPort = new System.Windows.Forms.NumericUpDown();
			this.chkFixedPort = new System.Windows.Forms.CheckBox();
			this.txtPlayerName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.chkStartRefresh = new System.Windows.Forms.CheckBox();
			this.tabGraphics = new System.Windows.Forms.TabPage();
			this.chkShowGibs = new System.Windows.Forms.CheckBox();
			this.chkHighTextures = new System.Windows.Forms.CheckBox();
			this.lblGammaValue = new System.Windows.Forms.Label();
			this.trkGamma = new System.Windows.Forms.TrackBar();
			this.chkDynamicLights = new System.Windows.Forms.CheckBox();
			this.chkShowDecals = new System.Windows.Forms.CheckBox();
			this.chkShowFPS = new System.Windows.Forms.CheckBox();
			this.cmbFSAA = new System.Windows.Forms.ComboBox();
			this.lblFSAA = new System.Windows.Forms.Label();
			this.chkSyncRate = new System.Windows.Forms.CheckBox();
			this.chkWindowed = new System.Windows.Forms.CheckBox();
			this.cmbResolution = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbAdapter = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.lblGamma = new System.Windows.Forms.Label();
			this.tabControls = new System.Windows.Forms.TabPage();
			this.grpControlOptions = new System.Windows.Forms.GroupBox();
			this.cmbMoveMethod = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.chkScrollWeapons = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cmbControl = new System.Windows.Forms.ComboBox();
			this.lblControl = new System.Windows.Forms.Label();
			this.lblControlDesc = new System.Windows.Forms.Label();
			this.lblAction = new System.Windows.Forms.Label();
			this.lstControls = new System.Windows.Forms.ListView();
			this.clmAction = new System.Windows.Forms.ColumnHeader();
			this.clmControl = new System.Windows.Forms.ColumnHeader();
			this.tabSound = new System.Windows.Forms.TabPage();
			this.lblMusicVolume = new System.Windows.Forms.Label();
			this.lblSoundVolume = new System.Windows.Forms.Label();
			this.chkPlayTeamBeep = new System.Windows.Forms.CheckBox();
			this.chkPlayChatBeep = new System.Windows.Forms.CheckBox();
			this.chkPlaySounds = new System.Windows.Forms.CheckBox();
			this.trkSoundVolume = new System.Windows.Forms.TrackBar();
			this.lblSoundVolumeLabel = new System.Windows.Forms.Label();
			this.trkMusicVolume = new System.Windows.Forms.TrackBar();
			this.lblMusicVolumeLabel = new System.Windows.Forms.Label();
			this.chkRandomMusic = new System.Windows.Forms.CheckBox();
			this.chkPlayMusic = new System.Windows.Forms.CheckBox();
			this.chkScreenFlashes = new System.Windows.Forms.CheckBox();
			this.tabsOptions.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkSnapsSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trkQuerySpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtClientPort)).BeginInit();
			this.tabGraphics.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkGamma)).BeginInit();
			this.tabControls.SuspendLayout();
			this.grpControlOptions.SuspendLayout();
			this.tabSound.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkSoundVolume)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trkMusicVolume)).BeginInit();
			this.SuspendLayout();
			//
			// btnOK
			//
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnOK.Location = new System.Drawing.Point(304, 292);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(104, 27);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			//
			// btnCancel
			//
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(412, 292);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 27);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			//
			// tabsOptions
			//
			this.tabsOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabsOptions.Controls.Add(this.tabGeneral);
			this.tabsOptions.Controls.Add(this.tabGraphics);
			this.tabsOptions.Controls.Add(this.tabControls);
			this.tabsOptions.Controls.Add(this.tabSound);
			this.tabsOptions.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabsOptions.Location = new System.Drawing.Point(8, 8);
			this.tabsOptions.Name = "tabsOptions";
			this.tabsOptions.SelectedIndex = 0;
			this.tabsOptions.Size = new System.Drawing.Size(508, 272);
			this.tabsOptions.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabsOptions.TabIndex = 5;
			//
			// tabGeneral
			//
			this.tabGeneral.Controls.Add(this.chkAutoSwitchWeapon);
			this.tabGeneral.Controls.Add(this.chkLaserIntensity);
			this.tabGeneral.Controls.Add(this.chkTeamColoredNames);
			this.tabGeneral.Controls.Add(this.chkAutoDownload);
			this.tabGeneral.Controls.Add(this.chkAutoScreenshot);
			this.tabGeneral.Controls.Add(this.lblSnapsSpeedLabel);
			this.tabGeneral.Controls.Add(this.lblSnapsSpeed);
			this.tabGeneral.Controls.Add(this.trkSnapsSpeed);
			this.tabGeneral.Controls.Add(this.lblQuerySpeedLabel);
			this.tabGeneral.Controls.Add(this.lblQuerySpeed);
			this.tabGeneral.Controls.Add(this.trkQuerySpeed);
			this.tabGeneral.Controls.Add(this.txtClientPort);
			this.tabGeneral.Controls.Add(this.chkFixedPort);
			this.tabGeneral.Controls.Add(this.txtPlayerName);
			this.tabGeneral.Controls.Add(this.label3);
			this.tabGeneral.Controls.Add(this.chkStartRefresh);
			this.tabGeneral.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabGeneral.Location = new System.Drawing.Point(4, 23);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(500, 245);
			this.tabGeneral.TabIndex = 2;
			this.tabGeneral.Text = "General";
			//
			// chkAutoSwitchWeapon
			//
			this.chkAutoSwitchWeapon.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAutoSwitchWeapon.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkAutoSwitchWeapon.Location = new System.Drawing.Point(100, 176);
			this.chkAutoSwitchWeapon.Name = "chkAutoSwitchWeapon";
			this.chkAutoSwitchWeapon.Size = new System.Drawing.Size(168, 22);
			this.chkAutoSwitchWeapon.TabIndex = 38;
			this.chkAutoSwitchWeapon.Text = "Switch weapon on pickup";
			//
			// chkLaserIntensity
			//
			this.chkLaserIntensity.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkLaserIntensity.Location = new System.Drawing.Point(100, 152);
			this.chkLaserIntensity.Name = "chkLaserIntensity";
			this.chkLaserIntensity.Size = new System.Drawing.Size(168, 22);
			this.chkLaserIntensity.TabIndex = 37;
			this.chkLaserIntensity.Text = "Show guidance laser";
			//
			// chkTeamColoredNames
			//
			this.chkTeamColoredNames.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkTeamColoredNames.Location = new System.Drawing.Point(100, 128);
			this.chkTeamColoredNames.Name = "chkTeamColoredNames";
			this.chkTeamColoredNames.Size = new System.Drawing.Size(168, 22);
			this.chkTeamColoredNames.TabIndex = 36;
			this.chkTeamColoredNames.Text = "Show names with team color";
			//
			// chkAutoDownload
			//
			this.chkAutoDownload.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAutoDownload.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkAutoDownload.Location = new System.Drawing.Point(100, 80);
			this.chkAutoDownload.Name = "chkAutoDownload";
			this.chkAutoDownload.Size = new System.Drawing.Size(188, 22);
			this.chkAutoDownload.TabIndex = 35;
			this.chkAutoDownload.Text = "Auto-download maps";
			//
			// chkAutoScreenshot
			//
			this.chkAutoScreenshot.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAutoScreenshot.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkAutoScreenshot.Location = new System.Drawing.Point(100, 104);
			this.chkAutoScreenshot.Name = "chkAutoScreenshot";
			this.chkAutoScreenshot.Size = new System.Drawing.Size(196, 22);
			this.chkAutoScreenshot.TabIndex = 34;
			this.chkAutoScreenshot.Text = "Auto-screenshot when game ends";
			//
			// lblSnapsSpeedLabel
			//
			this.lblSnapsSpeedLabel.BackColor = System.Drawing.Color.Transparent;
			this.lblSnapsSpeedLabel.Location = new System.Drawing.Point(404, 16);
			this.lblSnapsSpeedLabel.Name = "lblSnapsSpeedLabel";
			this.lblSnapsSpeedLabel.Size = new System.Drawing.Size(76, 36);
			this.lblSnapsSpeedLabel.TabIndex = 31;
			this.lblSnapsSpeedLabel.Text = "Snapshots resolution:";
			this.lblSnapsSpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblSnapsSpeedLabel.UseMnemonic = false;
			//
			// lblSnapsSpeed
			//
			this.lblSnapsSpeed.BackColor = System.Drawing.Color.Transparent;
			this.lblSnapsSpeed.Location = new System.Drawing.Point(404, 188);
			this.lblSnapsSpeed.Name = "lblSnapsSpeed";
			this.lblSnapsSpeed.Size = new System.Drawing.Size(76, 28);
			this.lblSnapsSpeed.TabIndex = 33;
			this.lblSnapsSpeed.Text = "10 snapshots per sec.";
			this.lblSnapsSpeed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblSnapsSpeed.UseMnemonic = false;
			//
			// trkSnapsSpeed
			//
			this.trkSnapsSpeed.LargeChange = 10;
			this.trkSnapsSpeed.Location = new System.Drawing.Point(420, 44);
			this.trkSnapsSpeed.Maximum = 40;
			this.trkSnapsSpeed.Minimum = 10;
			this.trkSnapsSpeed.Name = "trkSnapsSpeed";
			this.trkSnapsSpeed.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trkSnapsSpeed.Size = new System.Drawing.Size(42, 148);
			this.trkSnapsSpeed.SmallChange = 5;
			this.trkSnapsSpeed.TabIndex = 32;
			this.trkSnapsSpeed.TickFrequency = 5;
			this.trkSnapsSpeed.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trkSnapsSpeed.Value = 10;
			this.trkSnapsSpeed.ValueChanged += new System.EventHandler(this.trkSnapsSpeed_ValueChanged);
			//
			// lblQuerySpeedLabel
			//
			this.lblQuerySpeedLabel.BackColor = System.Drawing.Color.Transparent;
			this.lblQuerySpeedLabel.Location = new System.Drawing.Point(316, 16);
			this.lblQuerySpeedLabel.Name = "lblQuerySpeedLabel";
			this.lblQuerySpeedLabel.Size = new System.Drawing.Size(72, 36);
			this.lblQuerySpeedLabel.TabIndex = 27;
			this.lblQuerySpeedLabel.Text = "Servers query speed:";
			this.lblQuerySpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblQuerySpeedLabel.UseMnemonic = false;
			//
			// lblQuerySpeed
			//
			this.lblQuerySpeed.BackColor = System.Drawing.Color.Transparent;
			this.lblQuerySpeed.Location = new System.Drawing.Point(320, 188);
			this.lblQuerySpeed.Name = "lblQuerySpeed";
			this.lblQuerySpeed.Size = new System.Drawing.Size(68, 28);
			this.lblQuerySpeed.TabIndex = 29;
			this.lblQuerySpeed.Text = "2 servers per sec.";
			this.lblQuerySpeed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblQuerySpeed.UseMnemonic = false;
			//
			// trkQuerySpeed
			//
			this.trkQuerySpeed.LargeChange = 20;
			this.trkQuerySpeed.Location = new System.Drawing.Point(332, 44);
			this.trkQuerySpeed.Maximum = 100;
			this.trkQuerySpeed.Minimum = 2;
			this.trkQuerySpeed.Name = "trkQuerySpeed";
			this.trkQuerySpeed.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trkQuerySpeed.Size = new System.Drawing.Size(42, 148);
			this.trkQuerySpeed.SmallChange = 10;
			this.trkQuerySpeed.TabIndex = 28;
			this.trkQuerySpeed.TickFrequency = 10;
			this.trkQuerySpeed.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trkQuerySpeed.Value = 2;
			this.trkQuerySpeed.ValueChanged += new System.EventHandler(this.trkQuerySpeed_ValueChanged);
			//
			// txtClientPort
			//
			this.txtClientPort.Location = new System.Drawing.Point(204, 201);
			this.txtClientPort.Maximum = new System.Decimal(new int[] {
																		  65535,
																		  0,
																		  0,
																		  0});
			this.txtClientPort.Minimum = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  0});
			this.txtClientPort.Name = "txtClientPort";
			this.txtClientPort.Size = new System.Drawing.Size(64, 20);
			this.txtClientPort.TabIndex = 25;
			this.txtClientPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.txtClientPort.Value = new System.Decimal(new int[] {
																		7777,
																		0,
																		0,
																		0});
			//
			// chkFixedPort
			//
			this.chkFixedPort.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkFixedPort.Location = new System.Drawing.Point(100, 200);
			this.chkFixedPort.Name = "chkFixedPort";
			this.chkFixedPort.Size = new System.Drawing.Size(108, 22);
			this.chkFixedPort.TabIndex = 26;
			this.chkFixedPort.Text = "Fixed client port:";
			//
			// txtPlayerName
			//
			this.txtPlayerName.AutoSize = false;
			this.txtPlayerName.Location = new System.Drawing.Point(100, 24);
			this.txtPlayerName.MaxLength = 40;
			this.txtPlayerName.Name = "txtPlayerName";
			this.txtPlayerName.Size = new System.Drawing.Size(176, 22);
			this.txtPlayerName.TabIndex = 3;
			this.txtPlayerName.Text = "";
			this.txtPlayerName.Validating += new System.ComponentModel.CancelEventHandler(this.txtPlayerName_Validating);
			//
			// label3
			//
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(24, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 20);
			this.label3.TabIndex = 1;
			this.label3.Text = "Player name:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label3.UseMnemonic = false;
			//
			// chkStartRefresh
			//
			this.chkStartRefresh.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkStartRefresh.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkStartRefresh.Location = new System.Drawing.Point(100, 56);
			this.chkStartRefresh.Name = "chkStartRefresh";
			this.chkStartRefresh.Size = new System.Drawing.Size(188, 22);
			this.chkStartRefresh.TabIndex = 30;
			this.chkStartRefresh.Text = "Refresh servers list on startup";
			//
			// tabGraphics
			//
			this.tabGraphics.Controls.Add(this.chkScreenFlashes);
			this.tabGraphics.Controls.Add(this.chkShowGibs);
			this.tabGraphics.Controls.Add(this.chkHighTextures);
			this.tabGraphics.Controls.Add(this.lblGammaValue);
			this.tabGraphics.Controls.Add(this.trkGamma);
			this.tabGraphics.Controls.Add(this.chkDynamicLights);
			this.tabGraphics.Controls.Add(this.chkShowDecals);
			this.tabGraphics.Controls.Add(this.chkShowFPS);
			this.tabGraphics.Controls.Add(this.cmbFSAA);
			this.tabGraphics.Controls.Add(this.lblFSAA);
			this.tabGraphics.Controls.Add(this.chkSyncRate);
			this.tabGraphics.Controls.Add(this.chkWindowed);
			this.tabGraphics.Controls.Add(this.cmbResolution);
			this.tabGraphics.Controls.Add(this.label2);
			this.tabGraphics.Controls.Add(this.cmbAdapter);
			this.tabGraphics.Controls.Add(this.label1);
			this.tabGraphics.Controls.Add(this.lblGamma);
			this.tabGraphics.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabGraphics.Location = new System.Drawing.Point(4, 23);
			this.tabGraphics.Name = "tabGraphics";
			this.tabGraphics.Size = new System.Drawing.Size(500, 245);
			this.tabGraphics.TabIndex = 0;
			this.tabGraphics.Text = "Graphics";
			//
			// chkShowGibs
			//
			this.chkShowGibs.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkShowGibs.Location = new System.Drawing.Point(324, 80);
			this.chkShowGibs.Name = "chkShowGibs";
			this.chkShowGibs.Size = new System.Drawing.Size(164, 22);
			this.chkShowGibs.TabIndex = 23;
			this.chkShowGibs.Text = "Show gibbing";
			//
			// chkHighTextures
			//
			this.chkHighTextures.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkHighTextures.Location = new System.Drawing.Point(324, 164);
			this.chkHighTextures.Name = "chkHighTextures";
			this.chkHighTextures.Size = new System.Drawing.Size(164, 22);
			this.chkHighTextures.TabIndex = 21;
			this.chkHighTextures.Text = "High detail textures";
			//
			// lblGammaValue
			//
			this.lblGammaValue.BackColor = System.Drawing.Color.Transparent;
			this.lblGammaValue.Location = new System.Drawing.Point(276, 152);
			this.lblGammaValue.Name = "lblGammaValue";
			this.lblGammaValue.Size = new System.Drawing.Size(24, 20);
			this.lblGammaValue.TabIndex = 20;
			this.lblGammaValue.Text = "0";
			this.lblGammaValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblGammaValue.UseMnemonic = false;
			//
			// trkGamma
			//
			this.trkGamma.LargeChange = 2;
			this.trkGamma.Location = new System.Drawing.Point(96, 144);
			this.trkGamma.Name = "trkGamma";
			this.trkGamma.Size = new System.Drawing.Size(176, 42);
			this.trkGamma.TabIndex = 19;
			this.trkGamma.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trkGamma.ValueChanged += new System.EventHandler(this.trkGamma_ValueChanged);
			//
			// chkDynamicLights
			//
			this.chkDynamicLights.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkDynamicLights.Location = new System.Drawing.Point(324, 136);
			this.chkDynamicLights.Name = "chkDynamicLights";
			this.chkDynamicLights.Size = new System.Drawing.Size(164, 22);
			this.chkDynamicLights.TabIndex = 16;
			this.chkDynamicLights.Text = "Dynamic lighting";
			//
			// chkShowDecals
			//
			this.chkShowDecals.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkShowDecals.Location = new System.Drawing.Point(324, 52);
			this.chkShowDecals.Name = "chkShowDecals";
			this.chkShowDecals.Size = new System.Drawing.Size(164, 22);
			this.chkShowDecals.TabIndex = 15;
			this.chkShowDecals.Text = "Show decals";
			//
			// chkShowFPS
			//
			this.chkShowFPS.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkShowFPS.Location = new System.Drawing.Point(324, 24);
			this.chkShowFPS.Name = "chkShowFPS";
			this.chkShowFPS.Size = new System.Drawing.Size(164, 22);
			this.chkShowFPS.TabIndex = 14;
			this.chkShowFPS.Text = "Show FPS and MSPF";
			//
			// cmbFSAA
			//
			this.cmbFSAA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFSAA.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbFSAA.ItemHeight = 14;
			this.cmbFSAA.Location = new System.Drawing.Point(308, 208);
			this.cmbFSAA.MaxDropDownItems = 10;
			this.cmbFSAA.Name = "cmbFSAA";
			this.cmbFSAA.Size = new System.Drawing.Size(188, 22);
			this.cmbFSAA.TabIndex = 13;
			this.cmbFSAA.Visible = false;
			this.cmbFSAA.SelectedIndexChanged += new System.EventHandler(this.cmbFSAA_SelectedIndexChanged);
			//
			// lblFSAA
			//
			this.lblFSAA.BackColor = System.Drawing.Color.Transparent;
			this.lblFSAA.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblFSAA.Location = new System.Drawing.Point(220, 208);
			this.lblFSAA.Name = "lblFSAA";
			this.lblFSAA.Size = new System.Drawing.Size(84, 20);
			this.lblFSAA.TabIndex = 12;
			this.lblFSAA.Text = "Antialiasing:";
			this.lblFSAA.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblFSAA.UseMnemonic = false;
			this.lblFSAA.Visible = false;
			//
			// chkSyncRate
			//
			this.chkSyncRate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkSyncRate.Location = new System.Drawing.Point(100, 112);
			this.chkSyncRate.Name = "chkSyncRate";
			this.chkSyncRate.Size = new System.Drawing.Size(188, 22);
			this.chkSyncRate.TabIndex = 11;
			this.chkSyncRate.Text = "Synchronize with refresh rate";
			//
			// chkWindowed
			//
			this.chkWindowed.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkWindowed.Location = new System.Drawing.Point(100, 88);
			this.chkWindowed.Name = "chkWindowed";
			this.chkWindowed.Size = new System.Drawing.Size(188, 22);
			this.chkWindowed.TabIndex = 10;
			this.chkWindowed.Text = "Windowed";
			this.chkWindowed.CheckedChanged += new System.EventHandler(this.chkWindowed_CheckedChanged);
			//
			// cmbResolution
			//
			this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbResolution.Location = new System.Drawing.Point(100, 56);
			this.cmbResolution.MaxDropDownItems = 12;
			this.cmbResolution.Name = "cmbResolution";
			this.cmbResolution.Size = new System.Drawing.Size(188, 22);
			this.cmbResolution.TabIndex = 9;
			this.cmbResolution.SelectedIndexChanged += new System.EventHandler(this.cmbResolution_SelectedIndexChanged);
			//
			// label2
			//
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(12, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 20);
			this.label2.TabIndex = 8;
			this.label2.Text = "Resolution:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.UseMnemonic = false;
			//
			// cmbAdapter
			//
			this.cmbAdapter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAdapter.Location = new System.Drawing.Point(100, 24);
			this.cmbAdapter.MaxDropDownItems = 10;
			this.cmbAdapter.Name = "cmbAdapter";
			this.cmbAdapter.Size = new System.Drawing.Size(188, 22);
			this.cmbAdapter.TabIndex = 7;
			this.cmbAdapter.SelectedIndexChanged += new System.EventHandler(this.cmbAdapter_SelectedIndexChanged);
			//
			// label1
			//
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(12, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Display driver:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.UseMnemonic = false;
			//
			// lblGamma
			//
			this.lblGamma.BackColor = System.Drawing.Color.Transparent;
			this.lblGamma.Location = new System.Drawing.Point(12, 152);
			this.lblGamma.Name = "lblGamma";
			this.lblGamma.Size = new System.Drawing.Size(84, 20);
			this.lblGamma.TabIndex = 18;
			this.lblGamma.Text = "Gamma:";
			this.lblGamma.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblGamma.UseMnemonic = false;
			//
			// tabControls
			//
			this.tabControls.Controls.Add(this.grpControlOptions);
			this.tabControls.Controls.Add(this.label5);
			this.tabControls.Controls.Add(this.cmbControl);
			this.tabControls.Controls.Add(this.lblControl);
			this.tabControls.Controls.Add(this.lblControlDesc);
			this.tabControls.Controls.Add(this.lblAction);
			this.tabControls.Controls.Add(this.lstControls);
			this.tabControls.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabControls.Location = new System.Drawing.Point(4, 23);
			this.tabControls.Name = "tabControls";
			this.tabControls.Size = new System.Drawing.Size(500, 245);
			this.tabControls.TabIndex = 1;
			this.tabControls.Text = "Controls";
			//
			// grpControlOptions
			//
			this.grpControlOptions.Controls.Add(this.cmbMoveMethod);
			this.grpControlOptions.Controls.Add(this.label4);
			this.grpControlOptions.Controls.Add(this.chkScrollWeapons);
			this.grpControlOptions.Location = new System.Drawing.Point(236, 144);
			this.grpControlOptions.Name = "grpControlOptions";
			this.grpControlOptions.Size = new System.Drawing.Size(252, 84);
			this.grpControlOptions.TabIndex = 33;
			this.grpControlOptions.TabStop = false;
			this.grpControlOptions.Text = " Extra Options ";
			//
			// cmbMoveMethod
			//
			this.cmbMoveMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMoveMethod.IntegralHeight = false;
			this.cmbMoveMethod.Items.AddRange(new object[] {
															   "Relative to screen",
															   "Relative to map",
															   "Relative to player"});
			this.cmbMoveMethod.Location = new System.Drawing.Point(80, 24);
			this.cmbMoveMethod.Name = "cmbMoveMethod";
			this.cmbMoveMethod.Size = new System.Drawing.Size(152, 22);
			this.cmbMoveMethod.TabIndex = 35;
			this.cmbMoveMethod.TabStop = false;
			//
			// label4
			//
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(8, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 20);
			this.label4.TabIndex = 34;
			this.label4.Text = "Movement:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label4.UseMnemonic = false;
			//
			// chkScrollWeapons
			//
			this.chkScrollWeapons.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkScrollWeapons.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkScrollWeapons.Location = new System.Drawing.Point(20, 52);
			this.chkScrollWeapons.Name = "chkScrollWeapons";
			this.chkScrollWeapons.Size = new System.Drawing.Size(216, 22);
			this.chkScrollWeapons.TabIndex = 33;
			this.chkScrollWeapons.Text = "Use scrollwheel to switch weapons";
			//
			// label5
			//
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Location = new System.Drawing.Point(240, 16);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(252, 56);
			this.label5.TabIndex = 11;
			this.label5.Text = "Select an action from the list at the left and press the key you would like to us" +
				"e in the control box below. You can also select a special control such as a mous" +
				"e button.";
			this.label5.UseMnemonic = false;
			//
			// cmbControl
			//
			this.cmbControl.Enabled = false;
			this.cmbControl.IntegralHeight = false;
			this.cmbControl.Location = new System.Drawing.Point(288, 100);
			this.cmbControl.Name = "cmbControl";
			this.cmbControl.Size = new System.Drawing.Size(156, 22);
			this.cmbControl.TabIndex = 10;
			this.cmbControl.TabStop = false;
			this.cmbControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cmbControl_KeyDown);
			this.cmbControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbControl_KeyPress);
			this.cmbControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cmbControl_KeyUp);
			this.cmbControl.SelectedIndexChanged += new System.EventHandler(this.cmbControl_SelectedIndexChanged);
			this.cmbControl.Leave += new System.EventHandler(this.cmbControl_Leave);
			this.cmbControl.Enter += new System.EventHandler(this.cmbControl_Enter);
			//
			// lblControl
			//
			this.lblControl.BackColor = System.Drawing.Color.Transparent;
			this.lblControl.Enabled = false;
			this.lblControl.Location = new System.Drawing.Point(240, 100);
			this.lblControl.Name = "lblControl";
			this.lblControl.Size = new System.Drawing.Size(44, 20);
			this.lblControl.TabIndex = 9;
			this.lblControl.Text = "Control:";
			this.lblControl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblControl.UseMnemonic = false;
			//
			// lblControlDesc
			//
			this.lblControlDesc.BackColor = System.Drawing.Color.Transparent;
			this.lblControlDesc.Enabled = false;
			this.lblControlDesc.Location = new System.Drawing.Point(288, 76);
			this.lblControlDesc.Name = "lblControlDesc";
			this.lblControlDesc.Size = new System.Drawing.Size(152, 20);
			this.lblControlDesc.TabIndex = 8;
			this.lblControlDesc.Text = "<select an action>";
			this.lblControlDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblControlDesc.UseMnemonic = false;
			//
			// lblAction
			//
			this.lblAction.BackColor = System.Drawing.Color.Transparent;
			this.lblAction.Enabled = false;
			this.lblAction.Location = new System.Drawing.Point(240, 76);
			this.lblAction.Name = "lblAction";
			this.lblAction.Size = new System.Drawing.Size(44, 20);
			this.lblAction.TabIndex = 4;
			this.lblAction.Text = "Action:";
			this.lblAction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblAction.UseMnemonic = false;
			//
			// lstControls
			//
			this.lstControls.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.clmAction,
																						  this.clmControl});
			this.lstControls.FullRowSelect = true;
			this.lstControls.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstControls.HideSelection = false;
			listViewItem1.Tag = "walkup";
			listViewItem2.Tag = "walkdown";
			listViewItem3.Tag = "walkleft";
			listViewItem4.Tag = "walkright";
			listViewItem5.Tag = "weaponnext";
			listViewItem6.Tag = "weaponprev";
			listViewItem7.Tag = "fireweapon";
			listViewItem8.Tag = "firepowerup";
			listViewItem9.Tag = "respawn";
			listViewItem10.Tag = "showscores";
			listViewItem11.Tag = "console";
			listViewItem12.Tag = "say";
			listViewItem13.Tag = "sayteam";
			listViewItem14.Tag = "usesmg";
			listViewItem15.Tag = "useminigun";
			listViewItem16.Tag = "useplasma";
			listViewItem17.Tag = "userocket";
			listViewItem18.Tag = "usegrenades";
			listViewItem19.Tag = "usephoenix";
			listViewItem20.Tag = "useion";
			listViewItem21.Tag = "joinspectators";
			listViewItem22.Tag = "joingame";
			listViewItem23.Tag = "joinred";
			listViewItem24.Tag = "joinblue";
			listViewItem25.Tag = "voteyes";
			listViewItem26.Tag = "screenshot";
			listViewItem27.Tag = "suicide";
			listViewItem28.Tag = "exitgame";
			this.lstControls.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																						listViewItem1,
																						listViewItem2,
																						listViewItem3,
																						listViewItem4,
																						listViewItem5,
																						listViewItem6,
																						listViewItem7,
																						listViewItem8,
																						listViewItem9,
																						listViewItem10,
																						listViewItem11,
																						listViewItem12,
																						listViewItem13,
																						listViewItem14,
																						listViewItem15,
																						listViewItem16,
																						listViewItem17,
																						listViewItem18,
																						listViewItem19,
																						listViewItem20,
																						listViewItem21,
																						listViewItem22,
																						listViewItem23,
																						listViewItem24,
																						listViewItem25,
																						listViewItem26,
																						listViewItem27,
																						listViewItem28});
			this.lstControls.Location = new System.Drawing.Point(16, 16);
			this.lstControls.MultiSelect = false;
			this.lstControls.Name = "lstControls";
			this.lstControls.Size = new System.Drawing.Size(212, 212);
			this.lstControls.TabIndex = 0;
			this.lstControls.TabStop = false;
			this.lstControls.View = System.Windows.Forms.View.Details;
			this.lstControls.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lstControls_MouseUp);
			this.lstControls.SelectedIndexChanged += new System.EventHandler(this.lstControls_SelectedIndexChanged);
			//
			// clmAction
			//
			this.clmAction.Text = "Action";
			this.clmAction.Width = 118;
			//
			// clmControl
			//
			this.clmControl.Text = "Control";
			this.clmControl.Width = 70;
			//
			// tabSound
			//
			this.tabSound.Controls.Add(this.lblMusicVolume);
			this.tabSound.Controls.Add(this.lblSoundVolume);
			this.tabSound.Controls.Add(this.chkPlayTeamBeep);
			this.tabSound.Controls.Add(this.chkPlayChatBeep);
			this.tabSound.Controls.Add(this.chkPlaySounds);
			this.tabSound.Controls.Add(this.trkSoundVolume);
			this.tabSound.Controls.Add(this.lblSoundVolumeLabel);
			this.tabSound.Controls.Add(this.trkMusicVolume);
			this.tabSound.Controls.Add(this.lblMusicVolumeLabel);
			this.tabSound.Controls.Add(this.chkRandomMusic);
			this.tabSound.Controls.Add(this.chkPlayMusic);
			this.tabSound.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabSound.Location = new System.Drawing.Point(4, 23);
			this.tabSound.Name = "tabSound";
			this.tabSound.Size = new System.Drawing.Size(500, 245);
			this.tabSound.TabIndex = 3;
			this.tabSound.Text = "Sound";
			//
			// lblMusicVolume
			//
			this.lblMusicVolume.BackColor = System.Drawing.Color.Transparent;
			this.lblMusicVolume.Enabled = false;
			this.lblMusicVolume.Location = new System.Drawing.Point(116, 196);
			this.lblMusicVolume.Name = "lblMusicVolume";
			this.lblMusicVolume.Size = new System.Drawing.Size(52, 16);
			this.lblMusicVolume.TabIndex = 21;
			this.lblMusicVolume.Text = "0%";
			this.lblMusicVolume.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblMusicVolume.UseMnemonic = false;
			//
			// lblSoundVolume
			//
			this.lblSoundVolume.BackColor = System.Drawing.Color.Transparent;
			this.lblSoundVolume.Enabled = false;
			this.lblSoundVolume.Location = new System.Drawing.Point(40, 196);
			this.lblSoundVolume.Name = "lblSoundVolume";
			this.lblSoundVolume.Size = new System.Drawing.Size(52, 16);
			this.lblSoundVolume.TabIndex = 20;
			this.lblSoundVolume.Text = "0%";
			this.lblSoundVolume.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.lblSoundVolume.UseMnemonic = false;
			//
			// chkPlayTeamBeep
			//
			this.chkPlayTeamBeep.Enabled = false;
			this.chkPlayTeamBeep.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPlayTeamBeep.Location = new System.Drawing.Point(208, 192);
			this.chkPlayTeamBeep.Name = "chkPlayTeamBeep";
			this.chkPlayTeamBeep.Size = new System.Drawing.Size(188, 22);
			this.chkPlayTeamBeep.TabIndex = 19;
			this.chkPlayTeamBeep.Text = "Beep on team chat message";
			this.chkPlayTeamBeep.Visible = false;
			//
			// chkPlayChatBeep
			//
			this.chkPlayChatBeep.Enabled = false;
			this.chkPlayChatBeep.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPlayChatBeep.Location = new System.Drawing.Point(208, 168);
			this.chkPlayChatBeep.Name = "chkPlayChatBeep";
			this.chkPlayChatBeep.Size = new System.Drawing.Size(188, 22);
			this.chkPlayChatBeep.TabIndex = 18;
			this.chkPlayChatBeep.Text = "Beep on chat message";
			this.chkPlayChatBeep.Visible = false;
			//
			// chkPlaySounds
			//
			this.chkPlaySounds.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPlaySounds.Location = new System.Drawing.Point(208, 24);
			this.chkPlaySounds.Name = "chkPlaySounds";
			this.chkPlaySounds.Size = new System.Drawing.Size(188, 22);
			this.chkPlaySounds.TabIndex = 17;
			this.chkPlaySounds.Text = "Play sound effects";
			this.chkPlaySounds.CheckedChanged += new System.EventHandler(this.chkPlaySounds_CheckedChanged);
			//
			// trkSoundVolume
			//
			this.trkSoundVolume.Enabled = false;
			this.trkSoundVolume.LargeChange = 20;
			this.trkSoundVolume.Location = new System.Drawing.Point(44, 44);
			this.trkSoundVolume.Maximum = 100;
			this.trkSoundVolume.Name = "trkSoundVolume";
			this.trkSoundVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trkSoundVolume.Size = new System.Drawing.Size(42, 152);
			this.trkSoundVolume.SmallChange = 10;
			this.trkSoundVolume.TabIndex = 16;
			this.trkSoundVolume.TickFrequency = 10;
			this.trkSoundVolume.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trkSoundVolume.ValueChanged += new System.EventHandler(this.trkSoundVolume_ValueChanged);
			//
			// lblSoundVolumeLabel
			//
			this.lblSoundVolumeLabel.BackColor = System.Drawing.Color.Transparent;
			this.lblSoundVolumeLabel.Enabled = false;
			this.lblSoundVolumeLabel.Location = new System.Drawing.Point(36, 20);
			this.lblSoundVolumeLabel.Name = "lblSoundVolumeLabel";
			this.lblSoundVolumeLabel.Size = new System.Drawing.Size(56, 28);
			this.lblSoundVolumeLabel.TabIndex = 15;
			this.lblSoundVolumeLabel.Text = "Effects volume";
			this.lblSoundVolumeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblSoundVolumeLabel.UseMnemonic = false;
			//
			// trkMusicVolume
			//
			this.trkMusicVolume.Enabled = false;
			this.trkMusicVolume.LargeChange = 20;
			this.trkMusicVolume.Location = new System.Drawing.Point(120, 44);
			this.trkMusicVolume.Maximum = 100;
			this.trkMusicVolume.Name = "trkMusicVolume";
			this.trkMusicVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trkMusicVolume.Size = new System.Drawing.Size(42, 152);
			this.trkMusicVolume.SmallChange = 10;
			this.trkMusicVolume.TabIndex = 14;
			this.trkMusicVolume.TickFrequency = 10;
			this.trkMusicVolume.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trkMusicVolume.ValueChanged += new System.EventHandler(this.trkMusicVolume_ValueChanged);
			//
			// lblMusicVolumeLabel
			//
			this.lblMusicVolumeLabel.BackColor = System.Drawing.Color.Transparent;
			this.lblMusicVolumeLabel.Enabled = false;
			this.lblMusicVolumeLabel.Location = new System.Drawing.Point(112, 20);
			this.lblMusicVolumeLabel.Name = "lblMusicVolumeLabel";
			this.lblMusicVolumeLabel.Size = new System.Drawing.Size(56, 28);
			this.lblMusicVolumeLabel.TabIndex = 13;
			this.lblMusicVolumeLabel.Text = "Music volume";
			this.lblMusicVolumeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblMusicVolumeLabel.UseMnemonic = false;
			//
			// chkRandomMusic
			//
			this.chkRandomMusic.Enabled = false;
			this.chkRandomMusic.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkRandomMusic.Location = new System.Drawing.Point(208, 88);
			this.chkRandomMusic.Name = "chkRandomMusic";
			this.chkRandomMusic.Size = new System.Drawing.Size(188, 22);
			this.chkRandomMusic.TabIndex = 12;
			this.chkRandomMusic.Text = "Randomize music tracks";
			//
			// chkPlayMusic
			//
			this.chkPlayMusic.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPlayMusic.Location = new System.Drawing.Point(208, 64);
			this.chkPlayMusic.Name = "chkPlayMusic";
			this.chkPlayMusic.Size = new System.Drawing.Size(188, 22);
			this.chkPlayMusic.TabIndex = 11;
			this.chkPlayMusic.Text = "Play music tracks";
			this.chkPlayMusic.CheckedChanged += new System.EventHandler(this.chkPlayMusic_CheckedChanged);
			//
			// chkScreenFlashes
			//
			this.chkScreenFlashes.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkScreenFlashes.Location = new System.Drawing.Point(324, 108);
			this.chkScreenFlashes.Name = "chkScreenFlashes";
			this.chkScreenFlashes.Size = new System.Drawing.Size(164, 22);
			this.chkScreenFlashes.TabIndex = 24;
			this.chkScreenFlashes.Text = "Show screen flashes";
			//
			// FormOptions
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(522, 327);
			this.Controls.Add(this.tabsOptions);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormOptions";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Bloodmasters Options";
			this.tabsOptions.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trkSnapsSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trkQuerySpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtClientPort)).EndInit();
			this.tabGraphics.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trkGamma)).EndInit();
			this.tabControls.ResumeLayout(false);
			this.grpControlOptions.ResumeLayout(false);
			this.tabSound.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trkSoundVolume)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trkMusicVolume)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		// This clears a control on other actions than the specified action
		private void ClearExistingControl(string newaction, int control)
		{
			bool changed;

			do
			{
				// Presume no changes made
				changed = false;

				// Go for all in controlkeys
				foreach(DictionaryEntry de in controlkeys)
				{
					// Not the same action?
					if((string)de.Key != newaction)
					{
						// Same control?
						if((int)de.Value == control)
						{
							// Set control to none
							controlkeys[(string)de.Key] = (int)Keys.None;

							// This needs an update in the list
							// so go for all items in the list
							foreach(ListViewItem li in lstControls.Items)
							{
								// Check if this is the matching item
								if((string)li.Tag == (string)de.Key)
								{
									// Update the control displayed on this item
									li.SubItems[1].Text = InputKey.GetKeyName((int)Keys.None);
								}
							}

							// Collection changed, must leave now, I'll be back!
							changed = true;
							break;
						}
					}
				}
			} while(changed);
		}

		// Cancel clicked
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			// Close window
			this.Close();
		}

		// OK clicked
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			// Apply General
			General.playername = txtPlayerName.Text;
			General.config.WriteSetting("fixedclientport", chkFixedPort.Checked);
			General.config.WriteSetting("startuprefresh", chkStartRefresh.Checked);
			General.config.WriteSetting("clientport", (int)txtClientPort.Value);
			General.config.WriteSetting("queryspeed", (int)trkQuerySpeed.Value);
			General.config.WriteSetting("snapsspeed", (int)trkSnapsSpeed.Value);
			General.config.WriteSetting("autoscreenshot", chkAutoScreenshot.Checked);
			General.config.WriteSetting("autodownload", chkAutoDownload.Checked);
			General.config.WriteSetting("autoswitchweapon", chkAutoSwitchWeapon.Checked);
			General.config.WriteSetting("teamcolorednames", chkTeamColoredNames.Checked);

			// Apply controls
			foreach(DictionaryEntry de in controlkeys)
			{
				General.config.WriteSetting("controls/" + de.Key, (int)de.Value);
			}

			// Apply other options in controls
			General.config.WriteSetting("scrollweapons", chkScrollWeapons.Checked);
			General.config.WriteSetting("movemethod", cmbMoveMethod.SelectedIndex);

			// Apply Graphics
			Direct3D.SelectAdapter(((DisplayAdapterItem)cmbAdapter.SelectedItem).ordinal);
			Direct3D.DisplayMode = ((DisplayModeItem)cmbResolution.SelectedItem).mode;
			Direct3D.DisplayWindowed = chkWindowed.Checked;
			Direct3D.DisplaySyncRefresh = chkSyncRate.Checked;
			Direct3D.DisplayFSAA = cmbFSAA.SelectedIndex - 1;
			Direct3D.DisplayGamma = trkGamma.Value;
			General.config.WriteSetting("dynamiclights", chkDynamicLights.Checked);
			General.config.WriteSetting("showdecals", chkShowDecals.Checked);
			General.config.WriteSetting("showgibs", chkShowGibs.Checked);
			General.config.WriteSetting("showfps", chkShowFPS.Checked);
			General.config.WriteSetting("screenflashes", chkScreenFlashes.Checked);
			General.config.WriteSetting("hightextures", chkHighTextures.Checked);

			// Laser intensity
			if(chkLaserIntensity.Checked)
				General.config.WriteSetting("laserintensity", (int)chkLaserIntensity.Tag);
			else
				General.config.WriteSetting("laserintensity", 0);

			// Apply Sound
			General.config.WriteSetting("sounds", chkPlaySounds.Checked);
			General.config.WriteSetting("soundchatbeep", chkPlayChatBeep.Checked);
			General.config.WriteSetting("soundteambeep", chkPlayTeamBeep.Checked);
			General.config.WriteSetting("soundsvolume", trkSoundVolume.Value);
			General.config.WriteSetting("music", chkPlayMusic.Checked);
			General.config.WriteSetting("musicrandom", chkRandomMusic.Checked);
			General.config.WriteSetting("musicvolume", trkMusicVolume.Value);
		}

		// Display Driver selected
		private void cmbAdapter_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(cmbAdapter.SelectedIndex > -1)
			{
				// Fill resolutions list
				Direct3D.FillResolutionsList(cmbResolution, ((DisplayAdapterItem)cmbAdapter.SelectedItem).ordinal,
								last_mode.Width, last_mode.Height, (int)last_mode.Format, last_mode.RefreshRate);
			}
		}

		// Display Mode changed
		private void cmbResolution_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(cmbResolution.SelectedIndex > -1)
			{
				// Check if the selected resolution is supported windowed and fullscreen
				bool supwindowed = Direct3D.ValidateDisplayMode(((DisplayModeItem)cmbResolution.SelectedItem).mode, true);
				bool supfullscreen = Direct3D.ValidateDisplayMode(((DisplayModeItem)cmbResolution.SelectedItem).mode, false);
				if(supwindowed && supfullscreen)
				{
					// Optional
					chkWindowed.Enabled = true;
				}
				else
				{
					// Forced
					chkWindowed.Enabled = false;
					chkWindowed.Checked = supwindowed;
				}

				// Fill antialiasing list
				last_mode = ((DisplayModeItem)cmbResolution.SelectedItem).mode;
				Direct3D.FillAntialiasingList(cmbFSAA, ((DisplayAdapterItem)cmbAdapter.SelectedItem).ordinal,
								last_mode.Format, chkWindowed.Checked, last_fsaa);
				cmbFSAA.Enabled = (cmbFSAA.Items.Count > 1);
				lblFSAA.Enabled = cmbFSAA.Enabled;
			}
		}

		// Windowed changed
		private void chkWindowed_CheckedChanged(object sender, System.EventArgs e)
		{
			// Same as resolution changing
			cmbResolution_SelectedIndexChanged(sender, e);

			// No gamma in windowed mode
			lblGamma.Enabled = !chkWindowed.Checked;
			trkGamma.Enabled = !chkWindowed.Checked;
			lblGammaValue.Enabled = !chkWindowed.Checked;
		}

		// Antialias changed
		private void cmbFSAA_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(cmbFSAA.SelectedIndex > -1)
			{
				// Keep selection
				last_fsaa = cmbFSAA.SelectedIndex - 1;
			}
		}

		// Control selected
		private void lstControls_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstControls.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstControls.SelectedItems[0];

				// Get the key associated with the control
				int keycode = (int)controlkeys[item.Tag];

				// Enable controls
				lblAction.Enabled = true;
				lblControl.Enabled = true;
				lblControlDesc.Enabled = true;
				cmbControl.Enabled = true;

				// Show the control
				lblControlDesc.Text = item.Text;
				cmbControl.Text = InputKey.GetKeyName(keycode);
				cmbControl.SelectionLength = 0;
				cmbControl.SelectionStart = cmbControl.Text.Length;
			}
			else
			{
				// Disable controls
				lblAction.Enabled = false;
				lblControl.Enabled = false;
				lblControlDesc.Enabled = false;
				lblControlDesc.Text = "<select an action>";
				cmbControl.Enabled = false;
				cmbControl.Text = "";
			}
		}

		// Mouse released from controls list
		private void lstControls_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Focus to control input
			cmbControl.Focus();
			cmbControl.SelectionLength = 0;
			cmbControl.SelectionStart = cmbControl.Text.Length;
		}

		// Control key selected
		private void cmbControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstControls.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstControls.SelectedItems[0];

				// Anything in combo selected?
				if(cmbControl.SelectedItem != null)
				{
					// Check if actually changing
					if((int)controlkeys[item.Tag] != (int)cmbControl.SelectedItem)
					{
						// Change the control
						item.SubItems[1].Text = cmbControl.SelectedItem.ToString();
						controlkeys[item.Tag] = (int)cmbControl.SelectedItem;
						cmbControl.SelectionLength = 0;
						cmbControl.SelectionStart = cmbControl.Text.Length;

						// Clear this control from any other actions
						ClearExistingControl((string)item.Tag, (int)cmbControl.SelectedItem);
					}
				}
			}
		}

		// Focus enters control key
		private void cmbControl_Enter(object sender, System.EventArgs e)
		{
			// No default buttons or tab order
			this.AcceptButton = null;
			this.CancelButton = null;
			tabsOptions.TabStop = false;
			btnOK.TabStop = false;
			btnCancel.TabStop = false;

			// Select nothing
			cmbControl.SelectionLength = 0;
			cmbControl.SelectionStart = cmbControl.Text.Length;
		}

		// Focus leaves control key
		private void cmbControl_Leave(object sender, System.EventArgs e)
		{
			// Allow default buttons and tab order
			this.AcceptButton = btnOK;
			this.CancelButton = btnCancel;
			tabsOptions.TabStop = true;
			btnOK.TabStop = true;
			btnCancel.TabStop = true;
		}

		// Control key is pressed
		private void cmbControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Anything selected?
			if(lstControls.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstControls.SelectedItems[0];

				// Check if actually changing
				if((int)controlkeys[item.Tag] != (int)e.KeyCode)
				{
					// Change the control
					item.SubItems[1].Text = e.KeyCode.ToString();
					controlkeys[item.Tag] = (int)e.KeyCode;
					cmbControl.Text = e.KeyCode.ToString();
					cmbControl.SelectionLength = 0;
					cmbControl.SelectionStart = cmbControl.Text.Length;

					// Clear this control from any other actions
					ClearExistingControl((string)item.Tag, (int)e.KeyCode);
				}
			}

			// Key handled
			e.Handled = true;
		}

		// Control key is pressed
		private void cmbControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			// Key handled
			e.Handled = true;
		}

		// Control key is released
		private void cmbControl_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Key handled
			e.Handled = true;
		}

		// Play Music checkbox clicked
		private void chkPlayMusic_CheckedChanged(object sender, System.EventArgs e)
		{
			// Enable/Disable controls
			chkRandomMusic.Enabled = chkPlayMusic.Checked;
			lblMusicVolume.Enabled = chkPlayMusic.Checked;
			lblMusicVolumeLabel.Enabled = chkPlayMusic.Checked;
			trkMusicVolume.Enabled = chkPlayMusic.Checked;
		}

		// Play Sounds checkbox clicked
		private void chkPlaySounds_CheckedChanged(object sender, System.EventArgs e)
		{
			// Enable/Disable controls
			chkPlayChatBeep.Enabled = chkPlaySounds.Checked;
			chkPlayTeamBeep.Enabled = chkPlaySounds.Checked;
			lblSoundVolumeLabel.Enabled = chkPlaySounds.Checked;
			lblSoundVolume.Enabled = chkPlaySounds.Checked;
			trkSoundVolume.Enabled = chkPlaySounds.Checked;
		}

		// Validate player name
		private void txtPlayerName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string playernameerror;

			// Check player name
			playernameerror = General.ValidatePlayerName(txtPlayerName.Text);
			if(playernameerror != null)
			{
				// Invalid player name
				MessageBox.Show(this, playernameerror, "Bloodmasters",
							MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
							MessageBoxDefaultButton.Button1);

				// Cancel validation
				e.Cancel = true;
			}
		}

		// Music Volume changed
		private void trkMusicVolume_ValueChanged(object sender, System.EventArgs e)
		{
			// Update label
			lblMusicVolume.Text = trkMusicVolume.Value + "%";
		}

		// Sound Volume changed
		private void trkSoundVolume_ValueChanged(object sender, System.EventArgs e)
		{
			// Update label
			lblSoundVolume.Text = trkSoundVolume.Value + "%";
		}

		// Server query speed changed
		private void trkQuerySpeed_ValueChanged(object sender, System.EventArgs e)
		{
			// Update label
			lblQuerySpeed.Text = trkQuerySpeed.Value + " servers per sec.";
		}

		// Snapshots speed changed
		private void trkSnapsSpeed_ValueChanged(object sender, System.EventArgs e)
		{
			// Update label
			lblSnapsSpeed.Text = trkSnapsSpeed.Value + " snapshots per sec.";
		}

		// Gamma changed
		private void trkGamma_ValueChanged(object sender, System.EventArgs e)
		{
			// Update label
			lblGammaValue.Text = trkGamma.Value.ToString();
		}
	}
}
