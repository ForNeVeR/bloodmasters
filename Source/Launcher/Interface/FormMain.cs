/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace CodeImp.Bloodmasters.Launcher
{
	public class FormMain : System.Windows.Forms.Form
	{
		private ServerBrowser browser;
		private bool updatelist = false;
		private Hashtable flags = new Hashtable();

		private System.Windows.Forms.PictureBox picLogo;
		private System.Windows.Forms.TabControl tabsMode;
		private System.Windows.Forms.TabPage tabHost;
		private System.Windows.Forms.TabPage tabFind;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnHostGame;
		private System.Windows.Forms.Button btnOptions;
		private System.Windows.Forms.ListView lstGames;
		private System.Windows.Forms.Label lblFilterTitles;
		private System.Windows.Forms.TextBox txtFilterTitles;
		private System.Windows.Forms.CheckBox chkShowFull;
		private System.Windows.Forms.Label lblMap;
		private System.Windows.Forms.ToolStripMenuItem menuItem3;
		private System.Windows.Forms.ToolStripMenuItem menuItem7;
		private System.Windows.Forms.Button btnJoinGame1;
		private System.Windows.Forms.Button btnGameDetails;
		private System.Windows.Forms.ToolStripMenuItem itmJoinGame1;
		private System.Windows.Forms.ToolStripMenuItem itmGameDetails;
		private System.Windows.Forms.ToolStripMenuItem itmCopyGameTitle;
		private System.Windows.Forms.ToolStripMenuItem itmCopyGameDetails;
		private System.Windows.Forms.ToolStripMenuItem itmCopyGameIP;
		private System.Windows.Forms.ToolStripMenuItem itmGameWebsite;
		private System.Windows.Forms.ColumnHeader clmTitle;
		private System.Windows.Forms.ColumnHeader clmPing;
		private System.Windows.Forms.ColumnHeader clmPlayers;
		private System.Windows.Forms.ColumnHeader clmMap;
		private System.Windows.Forms.ContextMenuStrip mnuGame;
		private System.Windows.Forms.ImageList imglstGame;
		private System.Windows.Forms.ColumnHeader clmClients;
		private System.Windows.Forms.Button btnRefreshGames;
		private System.Windows.Forms.Panel pnlHostGame;
		private System.Windows.Forms.ComboBox cmbMap;
		private System.Windows.Forms.ComboBox cmbType;
		private System.Windows.Forms.Label lblType;
		private System.Windows.Forms.CheckBox chkShowEmpty;
		private System.Windows.Forms.ColumnHeader clmType;
		private System.Windows.Forms.GroupBox grpGeneral;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolTip tltDescription;
		private System.Windows.Forms.StatusStrip stbStatus;
		private System.Windows.Forms.ToolStripStatusLabel stpPanel;
		private System.Windows.Forms.Label lblTimelimit;
		private System.Windows.Forms.Label lblFraglimit;
		private System.Windows.Forms.Label lblPlayers;
		private System.Windows.Forms.Label lblClients;
		private System.Windows.Forms.GroupBox grpMaps;
		private System.Windows.Forms.CheckedListBox lstMaps;
		private System.Windows.Forms.ComboBox cmbServerType;
		private System.Windows.Forms.TextBox txtServerWebsite;
		private System.Windows.Forms.TextBox txtServerTitle;
		private System.Windows.Forms.TextBox txtServerPassword;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown txtServerTimelimit;
		private System.Windows.Forms.NumericUpDown txtServerFraglimit;
		private System.Windows.Forms.NumericUpDown txtServerPlayers;
		private System.Windows.Forms.NumericUpDown txtServerClients;
		private System.Windows.Forms.GroupBox grpSettings;
		private System.Windows.Forms.CheckBox chkServerDedicated;
		private System.Windows.Forms.CheckBox chkServerAddToMaster;
		private System.Windows.Forms.CheckBox chkServerJoinSmallest;
		private System.Windows.Forms.Label lblMapTitle;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label lblMapPlayers;
		private System.Windows.Forms.Label lblMapAuthor;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.PictureBox picMapPreview;
		private System.Windows.Forms.NumericUpDown txtServerPort;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnExportConfig;
		private System.Windows.Forms.Button btnImportConfig;
		private System.Windows.Forms.SaveFileDialog dlgSaveConfig;
		public System.Windows.Forms.Timer tmrUpdateList;
		private System.Windows.Forms.Button btnManualJoin;
		private System.Windows.Forms.ToolStripStatusLabel stpVersion;
		private System.Windows.Forms.ImageList imglstFlags;
		private System.ComponentModel.IContainer components;


		// Constructor
		public FormMain()
		{
			//IDictionary servermaps;
			int subitemindex;
			bool ascending;
			int maxplayers, maxclients;

			// Enable double buffering
			this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.UserPaint |
						  ControlStyles.DoubleBuffer , true);
			this.UpdateStyles();

			// Load window state from configuration
			this.WindowState = (FormWindowState)General.config.ReadSetting("windowstate", 0);

			// Required for Windows Form Designer support
			InitializeComponent();

			// Show version number
			Version v = Assembly.GetExecutingAssembly().GetName().Version;
			stpVersion.Text = "  version " + v.ToString(4);

			// Initialize the browser
			browser = new ServerBrowser();
			browser.OnFilteredListChanged += new ServerBrowser.FilteredListChanged(FilteredListChanged);

			// Load window size and location from configuration
			this.Location = new Point(General.config.ReadSetting("windowleft", 200),
									  General.config.ReadSetting("windowtop", 200));
			this.Size = new Size(General.config.ReadSetting("windowwidth", this.Size.Width),
								 General.config.ReadSetting("windowheight", this.Size.Height));

			// Load the interface settings from configuration
			maxclients = General.config.ReadSetting("serverclients", 10);
			maxplayers = General.config.ReadSetting("serverplayers", 10);
			if(maxclients > (int)txtServerClients.Maximum) maxclients = (int)txtServerClients.Maximum;
			if(maxplayers > (int)txtServerPlayers.Maximum) maxplayers = (int)txtServerPlayers.Maximum;
			txtFilterTitles.Text = General.config.ReadSetting("filtertitle", "");
			chkShowFull.Checked = General.config.ReadSetting("filterfull", true);
			chkShowEmpty.Checked = General.config.ReadSetting("filterempty", true);
			cmbType.SelectedIndex = General.config.ReadSetting("filtertype", 0);
			cmbMap.Text = General.config.ReadSetting("filtermap", "");
			txtServerTitle.Text = General.config.ReadSetting("servertitle", "");
			txtServerWebsite.Text = General.config.ReadSetting("serverwebsite", "");
			txtServerPassword.Text = General.config.ReadSetting("serverpassword", "");
			cmbServerType.SelectedIndex = General.config.ReadSetting("servertype", 0);
			chkServerDedicated.Checked = General.config.ReadSetting("serverdedicated", false);
			chkServerAddToMaster.Checked = General.config.ReadSetting("serverpublic", true);
			txtServerClients.Value = maxclients;
			txtServerPlayers.Value = maxplayers;
			txtServerFraglimit.Value = General.config.ReadSetting("serverscorelimit", 20);
			txtServerTimelimit.Value = General.config.ReadSetting("servertimelimit", 15);
			chkServerJoinSmallest.Checked = General.config.ReadSetting("serverjoinsmallest", true);
			txtServerPort.Value = General.config.ReadSetting("serverport", Consts.DEFAULT_SERVER_PORT);
			//servermaps = General.config.ReadSetting("servermaps", new Hashtable());

			// Apply sort comparer
			ascending = General.config.ReadSetting("sortascending", true);
			subitemindex = General.config.ReadSetting("sortitem", 6);
			lstGames.ListViewItemSorter = new GamesListItemComparer(subitemindex, ascending);

			// DEBUG:
			//tabsMode.SelectedIndex = 1;
		}

		// Clean up any resources being used.
		protected override void Dispose( bool disposing )
		{
			// Clear map preview image
			if(picMapPreview.Image != null) picMapPreview.Image.Dispose();
			picMapPreview.Image = null;

			// Stop queries
			browser.StopQuery();

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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormMain));
			this.picLogo = new System.Windows.Forms.PictureBox();
			this.tabsMode = new System.Windows.Forms.TabControl();
			this.tabFind = new System.Windows.Forms.TabPage();
			this.btnRefreshGames = new System.Windows.Forms.Button();
			this.btnGameDetails = new System.Windows.Forms.Button();
			this.btnManualJoin = new System.Windows.Forms.Button();
			this.btnJoinGame1 = new System.Windows.Forms.Button();
			this.cmbMap = new System.Windows.Forms.ComboBox();
			this.lblMap = new System.Windows.Forms.Label();
			this.cmbType = new System.Windows.Forms.ComboBox();
			this.lblType = new System.Windows.Forms.Label();
			this.chkShowEmpty = new System.Windows.Forms.CheckBox();
			this.chkShowFull = new System.Windows.Forms.CheckBox();
			this.txtFilterTitles = new System.Windows.Forms.TextBox();
			this.lblFilterTitles = new System.Windows.Forms.Label();
			this.lstGames = new System.Windows.Forms.ListView();
			this.clmTitle = new System.Windows.Forms.ColumnHeader();
			this.clmPing = new System.Windows.Forms.ColumnHeader();
			this.clmPlayers = new System.Windows.Forms.ColumnHeader();
			this.clmClients = new System.Windows.Forms.ColumnHeader();
			this.clmType = new System.Windows.Forms.ColumnHeader();
			this.clmMap = new System.Windows.Forms.ColumnHeader();
			this.imglstFlags = new System.Windows.Forms.ImageList(this.components);
			this.imglstGame = new System.Windows.Forms.ImageList(this.components);
			this.tabHost = new System.Windows.Forms.TabPage();
			this.btnImportConfig = new System.Windows.Forms.Button();
			this.btnExportConfig = new System.Windows.Forms.Button();
			this.pnlHostGame = new System.Windows.Forms.Panel();
			this.grpMaps = new System.Windows.Forms.GroupBox();
			this.lblMapTitle = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.lblMapPlayers = new System.Windows.Forms.Label();
			this.lblMapAuthor = new System.Windows.Forms.Label();
			this.picMapPreview = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lstMaps = new System.Windows.Forms.CheckedListBox();
			this.grpSettings = new System.Windows.Forms.GroupBox();
			this.txtServerPort = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.chkServerJoinSmallest = new System.Windows.Forms.CheckBox();
			this.txtServerTimelimit = new System.Windows.Forms.NumericUpDown();
			this.txtServerFraglimit = new System.Windows.Forms.NumericUpDown();
			this.txtServerPlayers = new System.Windows.Forms.NumericUpDown();
			this.txtServerClients = new System.Windows.Forms.NumericUpDown();
			this.lblTimelimit = new System.Windows.Forms.Label();
			this.lblFraglimit = new System.Windows.Forms.Label();
			this.lblPlayers = new System.Windows.Forms.Label();
			this.lblClients = new System.Windows.Forms.Label();
			this.grpGeneral = new System.Windows.Forms.GroupBox();
			this.chkServerAddToMaster = new System.Windows.Forms.CheckBox();
			this.txtServerPassword = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cmbServerType = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtServerWebsite = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtServerTitle = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.chkServerDedicated = new System.Windows.Forms.CheckBox();
			this.btnHostGame = new System.Windows.Forms.Button();
			this.mnuGame = new();
			this.itmJoinGame1 = new();
			this.itmGameDetails = new();
			this.menuItem3 = new();
			this.itmGameWebsite = new();
			this.menuItem7 = new();
			this.itmCopyGameTitle = new();
			this.itmCopyGameDetails = new();
			this.itmCopyGameIP = new();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnOptions = new System.Windows.Forms.Button();
			this.tltDescription = new System.Windows.Forms.ToolTip(this.components);
			this.stbStatus = new();
			this.stpPanel = new();
			this.stpVersion = new();
			this.dlgSaveConfig = new System.Windows.Forms.SaveFileDialog();
			this.tmrUpdateList = new System.Windows.Forms.Timer(this.components);
			this.tabsMode.SuspendLayout();
			this.tabFind.SuspendLayout();
			this.tabHost.SuspendLayout();
			this.pnlHostGame.SuspendLayout();
			this.grpMaps.SuspendLayout();
			this.grpSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtServerPort)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerTimelimit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerFraglimit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerPlayers)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerClients)).BeginInit();
			this.grpGeneral.SuspendLayout();
			this.SuspendLayout();
			//
			// picLogo
			//
			this.picLogo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.picLogo.BackColor = System.Drawing.Color.Black;
			this.picLogo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picLogo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
			this.picLogo.Location = new System.Drawing.Point(8, 8);
			this.picLogo.Name = "picLogo";
			this.picLogo.Size = new System.Drawing.Size(669, 84);
			this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picLogo.TabIndex = 0;
			this.picLogo.TabStop = false;
			this.picLogo.Click += new System.EventHandler(this.picLogo_Click);
			//
			// tabsMode
			//
			this.tabsMode.Controls.Add(this.tabFind);
			this.tabsMode.Controls.Add(this.tabHost);
			this.tabsMode.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabsMode.Location = new System.Drawing.Point(8, 100);
			this.tabsMode.Name = "tabsMode";
			this.tabsMode.Padding = new System.Drawing.Point(25, 3);
			this.tabsMode.SelectedIndex = 0;
			this.tabsMode.Size = new System.Drawing.Size(669, 448);
			this.tabsMode.TabIndex = 1;
			this.tabsMode.SelectedIndexChanged += new System.EventHandler(this.tabsMode_SelectedIndexChanged);
			//
			// tabFind
			//
			this.tabFind.BackColor = System.Drawing.SystemColors.Control;
			this.tabFind.Controls.Add(this.btnRefreshGames);
			this.tabFind.Controls.Add(this.btnGameDetails);
			this.tabFind.Controls.Add(this.btnManualJoin);
			this.tabFind.Controls.Add(this.btnJoinGame1);
			this.tabFind.Controls.Add(this.cmbMap);
			this.tabFind.Controls.Add(this.lblMap);
			this.tabFind.Controls.Add(this.cmbType);
			this.tabFind.Controls.Add(this.lblType);
			this.tabFind.Controls.Add(this.chkShowEmpty);
			this.tabFind.Controls.Add(this.chkShowFull);
			this.tabFind.Controls.Add(this.txtFilterTitles);
			this.tabFind.Controls.Add(this.lblFilterTitles);
			this.tabFind.Controls.Add(this.lstGames);
			this.tabFind.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabFind.Location = new System.Drawing.Point(4, 23);
			this.tabFind.Name = "tabFind";
			this.tabFind.Size = new System.Drawing.Size(661, 421);
			this.tabFind.TabIndex = 3;
			this.tabFind.Text = "Find Game";
			//
			// btnRefreshGames
			//
			this.btnRefreshGames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefreshGames.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRefreshGames.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnRefreshGames.Location = new System.Drawing.Point(8, 381);
			this.btnRefreshGames.Name = "btnRefreshGames";
			this.btnRefreshGames.Size = new System.Drawing.Size(104, 28);
			this.btnRefreshGames.TabIndex = 14;
			this.btnRefreshGames.Text = "Refresh";
			this.tltDescription.SetToolTip(this.btnRefreshGames, "Refresh entire servers list");
			this.btnRefreshGames.Click += new System.EventHandler(this.btnRefreshGames_Click);
			//
			// btnGameDetails
			//
			this.btnGameDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGameDetails.Enabled = false;
			this.btnGameDetails.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnGameDetails.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnGameDetails.Location = new System.Drawing.Point(116, 381);
			this.btnGameDetails.Name = "btnGameDetails";
			this.btnGameDetails.Size = new System.Drawing.Size(104, 28);
			this.btnGameDetails.TabIndex = 13;
			this.btnGameDetails.Text = " Game Details...";
			this.tltDescription.SetToolTip(this.btnGameDetails, "Shows detailed game information of the selected server");
			this.btnGameDetails.Click += new System.EventHandler(this.btnGameDetails_Click);
			//
			// btnManualJoin
			//
			this.btnManualJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnManualJoin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnManualJoin.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnManualJoin.Location = new System.Drawing.Point(332, 381);
			this.btnManualJoin.Name = "btnManualJoin";
			this.btnManualJoin.Size = new System.Drawing.Size(104, 28);
			this.btnManualJoin.TabIndex = 12;
			this.btnManualJoin.Text = " Specify...";
			this.tltDescription.SetToolTip(this.btnManualJoin, "Allows you to specify a server by IP address and port number");
			this.btnManualJoin.Click += new System.EventHandler(this.btnManualJoin_Click);
			//
			// btnJoinGame1
			//
			this.btnJoinGame1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnJoinGame1.Enabled = false;
			this.btnJoinGame1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnJoinGame1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnJoinGame1.Location = new System.Drawing.Point(224, 381);
			this.btnJoinGame1.Name = "btnJoinGame1";
			this.btnJoinGame1.Size = new System.Drawing.Size(104, 28);
			this.btnJoinGame1.TabIndex = 11;
			this.btnJoinGame1.Text = "Join";
			this.tltDescription.SetToolTip(this.btnJoinGame1, "Joins the selected game");
			this.btnJoinGame1.Click += new System.EventHandler(this.btnJoinGame1_Click);
			//
			// cmbMap
			//
			this.cmbMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbMap.Location = new System.Drawing.Point(520, 12);
			this.cmbMap.MaxDropDownItems = 10;
			this.cmbMap.Name = "cmbMap";
			this.cmbMap.Size = new System.Drawing.Size(132, 22);
			this.cmbMap.Sorted = true;
			this.cmbMap.TabIndex = 10;
			this.tltDescription.SetToolTip(this.cmbMap, "Whole or part of the short map name you are looking for");
			this.cmbMap.TextChanged += new System.EventHandler(this.cmbMap_TextChanged);
			//
			// lblMap
			//
			this.lblMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMap.BackColor = System.Drawing.Color.Transparent;
			this.lblMap.Location = new System.Drawing.Point(480, 12);
			this.lblMap.Name = "lblMap";
			this.lblMap.Size = new System.Drawing.Size(36, 20);
			this.lblMap.TabIndex = 9;
			this.lblMap.Text = "Map:";
			this.lblMap.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblMap.UseMnemonic = false;
			//
			// cmbType
			//
			this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbType.Items.AddRange(new object[] {
														 "Any",
														 "DM",
														 "TDM",
														 "CTF",
														 "SC",
														 "TSC"});
			this.cmbType.Location = new System.Drawing.Point(400, 12);
			this.cmbType.MaxDropDownItems = 12;
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new System.Drawing.Size(64, 22);
			this.cmbType.TabIndex = 8;
			this.tltDescription.SetToolTip(this.cmbType, "Type of game you are looking for");
			this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
			//
			// lblType
			//
			this.lblType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblType.Location = new System.Drawing.Point(356, 12);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(40, 20);
			this.lblType.TabIndex = 7;
			this.lblType.Text = "Type:";
			this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// chkShowEmpty
			//
			this.chkShowEmpty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkShowEmpty.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkShowEmpty.Location = new System.Drawing.Point(292, 12);
			this.chkShowEmpty.Name = "chkShowEmpty";
			this.chkShowEmpty.Size = new System.Drawing.Size(60, 21);
			this.chkShowEmpty.TabIndex = 4;
			this.chkShowEmpty.Text = "Empty";
			this.tltDescription.SetToolTip(this.chkShowEmpty, "Shows empty servers when checked");
			this.chkShowEmpty.CheckedChanged += new System.EventHandler(this.chkShowEmpty_CheckedChanged);
			//
			// chkShowFull
			//
			this.chkShowFull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkShowFull.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkShowFull.Location = new System.Drawing.Point(232, 12);
			this.chkShowFull.Name = "chkShowFull";
			this.chkShowFull.Size = new System.Drawing.Size(48, 21);
			this.chkShowFull.TabIndex = 3;
			this.chkShowFull.Text = "Full";
			this.tltDescription.SetToolTip(this.chkShowFull, "Shows full servers when checked");
			this.chkShowFull.CheckedChanged += new System.EventHandler(this.chkShowFull_CheckedChanged);
			//
			// txtFilterTitles
			//
			this.txtFilterTitles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilterTitles.AutoSize = false;
			this.txtFilterTitles.Location = new System.Drawing.Point(68, 12);
			this.txtFilterTitles.Name = "txtFilterTitles";
			this.txtFilterTitles.Size = new System.Drawing.Size(136, 22);
			this.txtFilterTitles.TabIndex = 2;
			this.txtFilterTitles.Text = "";
			this.tltDescription.SetToolTip(this.txtFilterTitles, "Whole or part of the server title you are looking for");
			this.txtFilterTitles.TextChanged += new System.EventHandler(this.txtFilterTitles_TextChanged);
			//
			// lblFilterTitles
			//
			this.lblFilterTitles.BackColor = System.Drawing.Color.Transparent;
			this.lblFilterTitles.Location = new System.Drawing.Point(4, 12);
			this.lblFilterTitles.Name = "lblFilterTitles";
			this.lblFilterTitles.Size = new System.Drawing.Size(60, 20);
			this.lblFilterTitles.TabIndex = 1;
			this.lblFilterTitles.Text = "Filter titles:";
			this.lblFilterTitles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblFilterTitles.UseMnemonic = false;
			//
			// lstGames
			//
			this.lstGames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstGames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.clmTitle,
																					   this.clmPing,
																					   this.clmPlayers,
																					   this.clmClients,
																					   this.clmType,
																					   this.clmMap});
			this.lstGames.FullRowSelect = true;
			this.lstGames.HideSelection = false;
			this.lstGames.LabelWrap = false;
			this.lstGames.Location = new System.Drawing.Point(8, 44);
			this.lstGames.MultiSelect = false;
			this.lstGames.Name = "lstGames";
			this.lstGames.Size = new System.Drawing.Size(644, 332);
			this.lstGames.SmallImageList = this.imglstFlags;
			this.lstGames.StateImageList = this.imglstGame;
			this.lstGames.TabIndex = 0;
			this.lstGames.View = System.Windows.Forms.View.Details;
			this.lstGames.DoubleClick += new System.EventHandler(this.lstGames_DoubleClick);
			this.lstGames.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lstGames_MouseUp);
			this.lstGames.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstGames_ColumnClick);
			this.lstGames.SelectedIndexChanged += new System.EventHandler(this.lstGames_SelectedIndexChanged);
			//
			// clmTitle
			//
			this.clmTitle.Text = "Server";
			this.clmTitle.Width = 259;
			//
			// clmPing
			//
			this.clmPing.Text = "Ping";
			this.clmPing.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.clmPing.Width = 52;
			//
			// clmPlayers
			//
			this.clmPlayers.Text = "Players";
			this.clmPlayers.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// clmClients
			//
			this.clmClients.Text = "Clients";
			this.clmClients.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// clmType
			//
			this.clmType.Text = "Type";
			this.clmType.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.clmType.Width = 50;
			//
			// clmMap
			//
			this.clmMap.Text = "Current Map";
			this.clmMap.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.clmMap.Width = 128;
			//
			// imglstFlags
			//
			this.imglstFlags.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imglstFlags.ImageSize = new System.Drawing.Size(16, 16);
			this.imglstFlags.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstFlags.ImageStream")));
			this.imglstFlags.TransparentColor = System.Drawing.Color.Transparent;
			//
			// imglstGame
			//
			this.imglstGame.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imglstGame.ImageSize = new System.Drawing.Size(16, 16);
			this.imglstGame.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imglstGame.ImageStream")));
			this.imglstGame.TransparentColor = System.Drawing.Color.Transparent;
			//
			// tabHost
			//
			this.tabHost.Controls.Add(this.btnImportConfig);
			this.tabHost.Controls.Add(this.btnExportConfig);
			this.tabHost.Controls.Add(this.pnlHostGame);
			this.tabHost.Controls.Add(this.btnHostGame);
			this.tabHost.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabHost.Location = new System.Drawing.Point(4, 23);
			this.tabHost.Name = "tabHost";
			this.tabHost.Size = new System.Drawing.Size(661, 421);
			this.tabHost.TabIndex = 1;
			this.tabHost.Text = "Host Game";
			//
			// btnImportConfig
			//
			this.btnImportConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnImportConfig.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnImportConfig.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnImportConfig.Location = new System.Drawing.Point(116, 381);
			this.btnImportConfig.Name = "btnImportConfig";
			this.btnImportConfig.Size = new System.Drawing.Size(104, 28);
			this.btnImportConfig.TabIndex = 14;
			this.btnImportConfig.Text = "Import Config...";
			this.tltDescription.SetToolTip(this.btnImportConfig, "Loads the server configuration from a file");
			//
			// btnExportConfig
			//
			this.btnExportConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExportConfig.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnExportConfig.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnExportConfig.Location = new System.Drawing.Point(224, 381);
			this.btnExportConfig.Name = "btnExportConfig";
			this.btnExportConfig.Size = new System.Drawing.Size(104, 28);
			this.btnExportConfig.TabIndex = 13;
			this.btnExportConfig.Text = "Export Config...";
			this.tltDescription.SetToolTip(this.btnExportConfig, "Saves the server configuration to a file");
			this.btnExportConfig.Click += new System.EventHandler(this.btnExportConfig_Click);
			//
			// pnlHostGame
			//
			this.pnlHostGame.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.pnlHostGame.Controls.Add(this.grpMaps);
			this.pnlHostGame.Controls.Add(this.grpSettings);
			this.pnlHostGame.Controls.Add(this.grpGeneral);
			this.pnlHostGame.Location = new System.Drawing.Point(18, 8);
			this.pnlHostGame.Name = "pnlHostGame";
			this.pnlHostGame.Size = new System.Drawing.Size(624, 360);
			this.pnlHostGame.TabIndex = 4;
			//
			// grpMaps
			//
			this.grpMaps.Controls.Add(this.lblMapTitle);
			this.grpMaps.Controls.Add(this.label9);
			this.grpMaps.Controls.Add(this.lblMapPlayers);
			this.grpMaps.Controls.Add(this.lblMapAuthor);
			this.grpMaps.Controls.Add(this.picMapPreview);
			this.grpMaps.Controls.Add(this.label6);
			this.grpMaps.Controls.Add(this.label5);
			this.grpMaps.Controls.Add(this.lstMaps);
			this.grpMaps.Location = new System.Drawing.Point(212, 104);
			this.grpMaps.Name = "grpMaps";
			this.grpMaps.Size = new System.Drawing.Size(408, 252);
			this.grpMaps.TabIndex = 16;
			this.grpMaps.TabStop = false;
			this.grpMaps.Text = " Maps ";
			//
			// lblMapTitle
			//
			this.lblMapTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblMapTitle.Location = new System.Drawing.Point(240, 184);
			this.lblMapTitle.Name = "lblMapTitle";
			this.lblMapTitle.Size = new System.Drawing.Size(156, 20);
			this.lblMapTitle.TabIndex = 28;
			this.lblMapTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblMapTitle.UseMnemonic = false;
			//
			// label9
			//
			this.label9.BackColor = System.Drawing.Color.Transparent;
			this.label9.Location = new System.Drawing.Point(188, 184);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(48, 20);
			this.label9.TabIndex = 27;
			this.label9.Text = "Title:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label9.UseMnemonic = false;
			//
			// lblMapPlayers
			//
			this.lblMapPlayers.BackColor = System.Drawing.Color.Transparent;
			this.lblMapPlayers.Location = new System.Drawing.Point(240, 224);
			this.lblMapPlayers.Name = "lblMapPlayers";
			this.lblMapPlayers.Size = new System.Drawing.Size(156, 20);
			this.lblMapPlayers.TabIndex = 26;
			this.lblMapPlayers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblMapPlayers.UseMnemonic = false;
			//
			// lblMapAuthor
			//
			this.lblMapAuthor.BackColor = System.Drawing.Color.Transparent;
			this.lblMapAuthor.Location = new System.Drawing.Point(240, 204);
			this.lblMapAuthor.Name = "lblMapAuthor";
			this.lblMapAuthor.Size = new System.Drawing.Size(156, 20);
			this.lblMapAuthor.TabIndex = 25;
			this.lblMapAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblMapAuthor.UseMnemonic = false;
			//
			// picMapPreview
			//
			this.picMapPreview.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.picMapPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picMapPreview.Location = new System.Drawing.Point(192, 24);
			this.picMapPreview.Name = "picMapPreview";
			this.picMapPreview.Size = new System.Drawing.Size(204, 154);
			this.picMapPreview.TabIndex = 24;
			this.picMapPreview.TabStop = false;
			//
			// label6
			//
			this.label6.BackColor = System.Drawing.Color.Transparent;
			this.label6.Location = new System.Drawing.Point(188, 224);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 20);
			this.label6.TabIndex = 23;
			this.label6.Text = "Players:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label6.UseMnemonic = false;
			//
			// label5
			//
			this.label5.BackColor = System.Drawing.Color.Transparent;
			this.label5.Location = new System.Drawing.Point(188, 204);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 20);
			this.label5.TabIndex = 22;
			this.label5.Text = "Author:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label5.UseMnemonic = false;
			//
			// lstMaps
			//
			this.lstMaps.CheckOnClick = true;
			this.lstMaps.IntegralHeight = false;
			this.lstMaps.Location = new System.Drawing.Point(12, 24);
			this.lstMaps.Name = "lstMaps";
			this.lstMaps.ScrollAlwaysVisible = true;
			this.lstMaps.Size = new System.Drawing.Size(168, 216);
			this.lstMaps.Sorted = true;
			this.lstMaps.TabIndex = 0;
			this.lstMaps.SelectedIndexChanged += new System.EventHandler(this.lstMaps_SelectedIndexChanged);
			this.lstMaps.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstMaps_MouseMove);
			this.lstMaps.MouseLeave += new System.EventHandler(this.lstMaps_MouseLeave);
			//
			// grpSettings
			//
			this.grpSettings.BackColor = System.Drawing.Color.Transparent;
			this.grpSettings.Controls.Add(this.txtServerPort);
			this.grpSettings.Controls.Add(this.label7);
			this.grpSettings.Controls.Add(this.chkServerJoinSmallest);
			this.grpSettings.Controls.Add(this.txtServerTimelimit);
			this.grpSettings.Controls.Add(this.txtServerFraglimit);
			this.grpSettings.Controls.Add(this.txtServerPlayers);
			this.grpSettings.Controls.Add(this.txtServerClients);
			this.grpSettings.Controls.Add(this.lblTimelimit);
			this.grpSettings.Controls.Add(this.lblFraglimit);
			this.grpSettings.Controls.Add(this.lblPlayers);
			this.grpSettings.Controls.Add(this.lblClients);
			this.grpSettings.Location = new System.Drawing.Point(4, 104);
			this.grpSettings.Name = "grpSettings";
			this.grpSettings.Size = new System.Drawing.Size(196, 252);
			this.grpSettings.TabIndex = 15;
			this.grpSettings.TabStop = false;
			this.grpSettings.Text = " Settings ";
			//
			// txtServerPort
			//
			this.txtServerPort.Location = new System.Drawing.Point(96, 220);
			this.txtServerPort.Maximum = new System.Decimal(new int[] {
																		  65535,
																		  0,
																		  0,
																		  0});
			this.txtServerPort.Minimum = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  0});
			this.txtServerPort.Name = "txtServerPort";
			this.txtServerPort.Size = new System.Drawing.Size(68, 20);
			this.txtServerPort.TabIndex = 23;
			this.txtServerPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tltDescription.SetToolTip(this.txtServerPort, "Maximum number of clients allowed on server");
			this.txtServerPort.Value = new System.Decimal(new int[] {
																		6969,
																		0,
																		0,
																		0});
			//
			// label7
			//
			this.label7.Location = new System.Drawing.Point(12, 220);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(80, 16);
			this.label7.TabIndex = 22;
			this.label7.Text = "Server port:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.label7.UseMnemonic = false;
			//
			// chkServerJoinSmallest
			//
			this.chkServerJoinSmallest.BackColor = System.Drawing.SystemColors.Control;
			this.chkServerJoinSmallest.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkServerJoinSmallest.Location = new System.Drawing.Point(32, 140);
			this.chkServerJoinSmallest.Name = "chkServerJoinSmallest";
			this.chkServerJoinSmallest.Size = new System.Drawing.Size(156, 24);
			this.chkServerJoinSmallest.TabIndex = 20;
			this.chkServerJoinSmallest.Text = "Always join smallest team";
			this.tltDescription.SetToolTip(this.chkServerJoinSmallest, "Forces joining people on the smallest team when checked");
			//
			// txtServerTimelimit
			//
			this.txtServerTimelimit.Location = new System.Drawing.Point(96, 108);
			this.txtServerTimelimit.Maximum = new System.Decimal(new int[] {
																			   1000,
																			   0,
																			   0,
																			   0});
			this.txtServerTimelimit.Name = "txtServerTimelimit";
			this.txtServerTimelimit.Size = new System.Drawing.Size(68, 20);
			this.txtServerTimelimit.TabIndex = 19;
			this.txtServerTimelimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tltDescription.SetToolTip(this.txtServerTimelimit, "Maximum duration of the game in minutes");
			this.txtServerTimelimit.Value = new System.Decimal(new int[] {
																			 15,
																			 0,
																			 0,
																			 0});
			//
			// txtServerFraglimit
			//
			this.txtServerFraglimit.Location = new System.Drawing.Point(96, 80);
			this.txtServerFraglimit.Maximum = new System.Decimal(new int[] {
																			   10000,
																			   0,
																			   0,
																			   0});
			this.txtServerFraglimit.Name = "txtServerFraglimit";
			this.txtServerFraglimit.Size = new System.Drawing.Size(68, 20);
			this.txtServerFraglimit.TabIndex = 18;
			this.txtServerFraglimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tltDescription.SetToolTip(this.txtServerFraglimit, "Frags or captures required to end the game");
			this.txtServerFraglimit.Value = new System.Decimal(new int[] {
																			 20,
																			 0,
																			 0,
																			 0});
			//
			// txtServerPlayers
			//
			this.txtServerPlayers.Location = new System.Drawing.Point(96, 52);
			this.txtServerPlayers.Maximum = new System.Decimal(new int[] {
																			 10,
																			 0,
																			 0,
																			 0});
			this.txtServerPlayers.Minimum = new System.Decimal(new int[] {
																			 2,
																			 0,
																			 0,
																			 0});
			this.txtServerPlayers.Name = "txtServerPlayers";
			this.txtServerPlayers.Size = new System.Drawing.Size(68, 20);
			this.txtServerPlayers.TabIndex = 17;
			this.txtServerPlayers.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tltDescription.SetToolTip(this.txtServerPlayers, "Maximum number of players allowed");
			this.txtServerPlayers.Value = new System.Decimal(new int[] {
																		   10,
																		   0,
																		   0,
																		   0});
			//
			// txtServerClients
			//
			this.txtServerClients.Location = new System.Drawing.Point(96, 24);
			this.txtServerClients.Maximum = new System.Decimal(new int[] {
																			 10,
																			 0,
																			 0,
																			 0});
			this.txtServerClients.Minimum = new System.Decimal(new int[] {
																			 2,
																			 0,
																			 0,
																			 0});
			this.txtServerClients.Name = "txtServerClients";
			this.txtServerClients.Size = new System.Drawing.Size(68, 20);
			this.txtServerClients.TabIndex = 16;
			this.txtServerClients.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tltDescription.SetToolTip(this.txtServerClients, "Maximum number of clients allowed on server");
			this.txtServerClients.Value = new System.Decimal(new int[] {
																		   10,
																		   0,
																		   0,
																		   0});
			//
			// lblTimelimit
			//
			this.lblTimelimit.BackColor = System.Drawing.Color.Transparent;
			this.lblTimelimit.Location = new System.Drawing.Point(12, 108);
			this.lblTimelimit.Name = "lblTimelimit";
			this.lblTimelimit.Size = new System.Drawing.Size(80, 16);
			this.lblTimelimit.TabIndex = 15;
			this.lblTimelimit.Text = "Time limit:";
			this.lblTimelimit.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.lblTimelimit.UseMnemonic = false;
			//
			// lblFraglimit
			//
			this.lblFraglimit.BackColor = System.Drawing.Color.Transparent;
			this.lblFraglimit.Location = new System.Drawing.Point(12, 80);
			this.lblFraglimit.Name = "lblFraglimit";
			this.lblFraglimit.Size = new System.Drawing.Size(80, 16);
			this.lblFraglimit.TabIndex = 14;
			this.lblFraglimit.Text = "Score limit:";
			this.lblFraglimit.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.lblFraglimit.UseMnemonic = false;
			//
			// lblPlayers
			//
			this.lblPlayers.BackColor = System.Drawing.Color.Transparent;
			this.lblPlayers.Location = new System.Drawing.Point(12, 52);
			this.lblPlayers.Name = "lblPlayers";
			this.lblPlayers.Size = new System.Drawing.Size(80, 16);
			this.lblPlayers.TabIndex = 13;
			this.lblPlayers.Text = "Max players:";
			this.lblPlayers.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.lblPlayers.UseMnemonic = false;
			//
			// lblClients
			//
			this.lblClients.BackColor = System.Drawing.Color.Transparent;
			this.lblClients.Location = new System.Drawing.Point(12, 24);
			this.lblClients.Name = "lblClients";
			this.lblClients.Size = new System.Drawing.Size(80, 16);
			this.lblClients.TabIndex = 12;
			this.lblClients.Text = "Max clients:";
			this.lblClients.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.lblClients.UseMnemonic = false;
			//
			// grpGeneral
			//
			this.grpGeneral.Controls.Add(this.chkServerAddToMaster);
			this.grpGeneral.Controls.Add(this.txtServerPassword);
			this.grpGeneral.Controls.Add(this.label4);
			this.grpGeneral.Controls.Add(this.cmbServerType);
			this.grpGeneral.Controls.Add(this.label3);
			this.grpGeneral.Controls.Add(this.txtServerWebsite);
			this.grpGeneral.Controls.Add(this.label2);
			this.grpGeneral.Controls.Add(this.txtServerTitle);
			this.grpGeneral.Controls.Add(this.label1);
			this.grpGeneral.Controls.Add(this.chkServerDedicated);
			this.grpGeneral.Location = new System.Drawing.Point(4, 4);
			this.grpGeneral.Name = "grpGeneral";
			this.grpGeneral.Size = new System.Drawing.Size(616, 92);
			this.grpGeneral.TabIndex = 13;
			this.grpGeneral.TabStop = false;
			this.grpGeneral.Text = " General ";
			//
			// chkServerAddToMaster
			//
			this.chkServerAddToMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkServerAddToMaster.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkServerAddToMaster.Location = new System.Drawing.Point(460, 56);
			this.chkServerAddToMaster.Name = "chkServerAddToMaster";
			this.chkServerAddToMaster.Size = new System.Drawing.Size(140, 24);
			this.chkServerAddToMaster.TabIndex = 20;
			this.chkServerAddToMaster.Text = "Show in public list";
			this.tltDescription.SetToolTip(this.chkServerAddToMaster, "Shows your server on the internet so that people can find it");
			//
			// txtServerPassword
			//
			this.txtServerPassword.AutoSize = false;
			this.txtServerPassword.Location = new System.Drawing.Point(288, 56);
			this.txtServerPassword.MaxLength = 50;
			this.txtServerPassword.Name = "txtServerPassword";
			this.txtServerPassword.Size = new System.Drawing.Size(144, 22);
			this.txtServerPassword.TabIndex = 18;
			this.txtServerPassword.Text = "";
			this.tltDescription.SetToolTip(this.txtServerPassword, "Password to lock the server");
			//
			// label4
			//
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(224, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 20);
			this.label4.TabIndex = 17;
			this.label4.Text = "Password:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label4.UseMnemonic = false;
			//
			// cmbServerType
			//
			this.cmbServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbServerType.Items.AddRange(new object[] {
															   "Deathmatch (DM)",
															   "Team Deathmatch (TDM)",
															   "Capture The Flag (CTF)",
															   "Scavenger (SC)",
															   "Team Scavenger (TSC)"});
			this.cmbServerType.Location = new System.Drawing.Point(72, 56);
			this.cmbServerType.MaxDropDownItems = 12;
			this.cmbServerType.Name = "cmbServerType";
			this.cmbServerType.Size = new System.Drawing.Size(140, 22);
			this.cmbServerType.TabIndex = 16;
			this.tltDescription.SetToolTip(this.cmbServerType, "Type of game to play");
			this.cmbServerType.SelectedIndexChanged += new System.EventHandler(this.cmbServerType_SelectedIndexChanged);
			//
			// label3
			//
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(4, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 20);
			this.label3.TabIndex = 15;
			this.label3.Text = "Game type:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label3.UseMnemonic = false;
			//
			// txtServerWebsite
			//
			this.txtServerWebsite.AutoSize = false;
			this.txtServerWebsite.Location = new System.Drawing.Point(288, 24);
			this.txtServerWebsite.MaxLength = 300;
			this.txtServerWebsite.Name = "txtServerWebsite";
			this.txtServerWebsite.Size = new System.Drawing.Size(144, 22);
			this.txtServerWebsite.TabIndex = 14;
			this.txtServerWebsite.Text = "";
			this.tltDescription.SetToolTip(this.txtServerWebsite, "Complete URL of the website related to this game server");
			//
			// label2
			//
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(232, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 20);
			this.label2.TabIndex = 13;
			this.label2.Text = "Website:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.UseMnemonic = false;
			//
			// txtServerTitle
			//
			this.txtServerTitle.AutoSize = false;
			this.txtServerTitle.Location = new System.Drawing.Point(72, 24);
			this.txtServerTitle.MaxLength = 200;
			this.txtServerTitle.Name = "txtServerTitle";
			this.txtServerTitle.Size = new System.Drawing.Size(140, 22);
			this.txtServerTitle.TabIndex = 12;
			this.txtServerTitle.Text = "";
			this.tltDescription.SetToolTip(this.txtServerTitle, "Title of the game to display in the servers list");
			//
			// label1
			//
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(4, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.TabIndex = 11;
			this.label1.Text = "Title:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.UseMnemonic = false;
			//
			// chkServerDedicated
			//
			this.chkServerDedicated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkServerDedicated.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkServerDedicated.Location = new System.Drawing.Point(460, 24);
			this.chkServerDedicated.Name = "chkServerDedicated";
			this.chkServerDedicated.Size = new System.Drawing.Size(124, 24);
			this.chkServerDedicated.TabIndex = 19;
			this.chkServerDedicated.Text = "Dedicated server";
			this.tltDescription.SetToolTip(this.chkServerDedicated, "Only a dedicated server will be started when checked");
			//
			// btnHostGame
			//
			this.btnHostGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnHostGame.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnHostGame.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnHostGame.Location = new System.Drawing.Point(332, 381);
			this.btnHostGame.Name = "btnHostGame";
			this.btnHostGame.Size = new System.Drawing.Size(104, 28);
			this.btnHostGame.TabIndex = 3;
			this.btnHostGame.Text = "Launch";
			this.tltDescription.SetToolTip(this.btnHostGame, "Launches the server and joins the game if not dedicated");
			this.btnHostGame.Click += new System.EventHandler(this.btnHostGame_Click);
			//
			// mnuGame
			//
			this.mnuGame.Items.AddRange(new[] {
																					this.itmJoinGame1,
																					this.itmGameDetails,
																					this.menuItem3,
																					this.itmGameWebsite,
																					this.menuItem7,
																					this.itmCopyGameTitle,
																					this.itmCopyGameDetails,
																					this.itmCopyGameIP});
			//
			// itmJoinGame1
			//
			this.itmJoinGame1.Text = "Join Game";
			this.itmJoinGame1.Click += new System.EventHandler(this.btnJoinGame1_Click);
			//
			// itmGameDetails
			//
			this.itmGameDetails.Text = "Show Game Details...";
			this.itmGameDetails.Click += new System.EventHandler(this.btnGameDetails_Click);
			//
			// menuItem3
			//
			this.menuItem3.Text = "-";
			//
			// itmGameWebsite
			//
			this.itmGameWebsite.Text = "Browse Website";
			this.itmGameWebsite.Click += new System.EventHandler(this.itmGameWebsite_Click);
			//
			// menuItem7
			//
			this.menuItem7.Text = "-";
			//
			// itmCopyGameTitle
			//
			this.itmCopyGameTitle.Text = "Copy Game Title";
			this.itmCopyGameTitle.Click += new System.EventHandler(this.itmCopyGameTitle_Click);
			//
			// itmCopyGameDetails
			//
			this.itmCopyGameDetails.Text = "Copy Game Information";
			this.itmCopyGameDetails.Click += new System.EventHandler(this.itmCopyGameDetails_Click);
			//
			// itmCopyGameIP
			//
			this.itmCopyGameIP.Text = "Copy Game Address";
			this.itmCopyGameIP.Click += new System.EventHandler(this.itmCopyGameIP_Click);
			//
			// btnExit
			//
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnExit.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnExit.Location = new System.Drawing.Point(560, 504);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(104, 28);
			this.btnExit.TabIndex = 2;
			this.btnExit.Text = "Exit";
			this.tltDescription.SetToolTip(this.btnExit, "Click this and the devil will take your soul");
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			//
			// btnOptions
			//
			this.btnOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOptions.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOptions.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnOptions.Location = new System.Drawing.Point(452, 504);
			this.btnOptions.Name = "btnOptions";
			this.btnOptions.Size = new System.Drawing.Size(104, 28);
			this.btnOptions.TabIndex = 3;
			this.btnOptions.Text = "Options...";
			this.tltDescription.SetToolTip(this.btnOptions, "Displays the options configuration dialog");
			this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
			//
			// tltDescription
			//
			this.tltDescription.AutoPopDelay = 6000;
			this.tltDescription.InitialDelay = 300;
			this.tltDescription.ReshowDelay = 50;
			//
			// stbStatus
			//
			this.stbStatus.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.stbStatus.Name = "stbStatus";
			this.stbStatus.Items.AddRange(new [] {
																						 this.stpPanel,
																						 this.stpVersion});
			this.stbStatus.Size = new System.Drawing.Size(684, 20);
			this.stbStatus.TabIndex = 4;
			//
			// stpPanel
			//
			this.stpPanel.AutoSize = true;
			this.stpPanel.Text = " Ready.";
			this.stpPanel.Width = 563;
			//
			// stpVersion
			//
			this.stpVersion.AutoSize = true;
			this.stpVersion.BorderStyle = Border3DStyle.Flat;
			this.stpVersion.Text = " version 0.0.0000";
			this.stpVersion.Width = 105;
			//
			// dlgSaveConfig
			//
			this.dlgSaveConfig.DefaultExt = "cfg";
			this.dlgSaveConfig.Filter = "Configurations   *.cfg|*.cfg|All files|*.*";
			this.dlgSaveConfig.Title = "Export Configuration";
			//
			// tmrUpdateList
			//
			this.tmrUpdateList.Interval = 200;
			this.tmrUpdateList.Tick += new System.EventHandler(this.tmrUpdateList_Tick);
			//
			// FormMain
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnExit;
			this.ClientSize = new System.Drawing.Size(684, 573);
			this.Controls.Add(this.stbStatus);
			this.Controls.Add(this.btnOptions);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.tabsMode);
			this.Controls.Add(this.picLogo);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(692, 600);
			this.Name = "FormMain";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Bloodmasters Launcher";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
			this.Resize += new System.EventHandler(this.FormMain_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormMain_Closing);
			this.tabsMode.ResumeLayout(false);
			this.tabFind.ResumeLayout(false);
			this.tabHost.ResumeLayout(false);
			this.pnlHostGame.ResumeLayout(false);
			this.grpMaps.ResumeLayout(false);
			this.grpSettings.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.txtServerPort)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerTimelimit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerFraglimit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerPlayers)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtServerClients)).EndInit();
			this.grpGeneral.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		// This refreshes the maps lists
		public void RefreshMapsLists()
		{
			// Clear the combo
			cmbMap.Items.Clear();

			// Go for all .wad files
			ArrayList wads = ArchiveManager.FindAllFiles(".wad");
			foreach(string wf in wads)
			{
				// Make short map name
				string[] wfparts = wf.Split('/');
				string mname = wfparts[1].Substring(0, wfparts[1].Length - 4);

				// Add to list
				cmbMap.Items.Add(mname);
			}

			// Fill the maps list
			RefillMapsList();
		}

		// This checks if a given map name exists
		public bool CheckMapExists(string mapname)
		{
			// Go for all items in the combo
			foreach(string mn in cmbMap.Items)
			{
				// Check if the name matches
				if(string.Compare(mn, mapname, true) == 0) return true;
			}

			// Not found
			return false;
		}

		// This fills the maps list
		public void RefillMapsList()
		{
			// Clear the list
			lstMaps.Items.Clear();

			// Go for all .wad files
			ArrayList wads = ArchiveManager.FindAllFiles(".wad");
			foreach(string wf in wads)
			{
				try
				{
					// Make short map name
					string[] wfparts = wf.Split('/');
					string mname = wfparts[1].Substring(0, wfparts[1].Length - 4);

					// Load the map information
					Map wadmap = new Map(mname, true, General.temppath);

					// Check if game type is supported
					if(((cmbServerType.SelectedIndex == 0) && wadmap.SupportsDM) ||
					   ((cmbServerType.SelectedIndex == 1) && wadmap.SupportsTDM) ||
					   ((cmbServerType.SelectedIndex == 2) && wadmap.SupportsCTF) ||
					   ((cmbServerType.SelectedIndex == 3) && wadmap.SupportsSC) ||
					   ((cmbServerType.SelectedIndex == 4) && wadmap.SupportsTSC) ||
					   ((cmbServerType.SelectedIndex == 5) && wadmap.SupportsST) ||
					   ((cmbServerType.SelectedIndex == 6) && wadmap.SupportsTST))
					{
						// Add to list
						//lstMaps.Items.Add(mname, selection.Contains(mname));
						lstMaps.Items.Add(mname);
					}
				}
				catch(Exception) { }
			}
		}

		// Exit
		private void btnExit_Click(object sender, System.EventArgs e)
		{
			// Close window
			this.Close();
		}

		// This shows a status
		public void ShowStatus(string description)
		{
			// Different text?
			if(stpPanel.Text != description)
			{
				// Update panel
				stpPanel.Text = " " + description;
				this.Update();
			}
		}

		// This loads the flag images
		public bool LoadFlagImages()
		{
			string[] files;
			string temppath;
			int index;
			bool success;

			// Show status
			ShowStatus("Loading country flag images...");

			try
			{
				// Extract flag icons
				Archive flagsarchive = ArchiveManager.GetArchive("flags.rar");
				temppath = ArchiveManager.GetArchiveTempPath(flagsarchive);
				flagsarchive.ExtractAllFiles(temppath);

				// Load all flags icons
				files = Directory.GetFiles(temppath);
				foreach(string f in files)
				{
					// Load flag icon
					Image flag = Image.FromFile(f);

					// Add to image list
					index = imglstFlags.Images.Add(flag, Color.Transparent);
					flags.Add(Path.GetFileName(f).ToLower(), index);
				}

				// Worked
				success = true;
			}
			catch(Exception)
			{
				// Only the unknown flag in there
				flags.Add("_0.ico", 0);
				success = false;
			}

			// Done loading images
			ShowStatus("Ready.");

			// Return success
			return success;
		}

		// This inserts only the empty flag icon
		public void LoadNoFlagImages()
		{
			// Only the unknown flag in there
			flags.Add("_0.ico", 0);
		}

		// This returns a flag icon image
		public int GetFlagIconIndex(char[] countrycode)
		{
			// Make the filename
			string filename = (new string(countrycode)).ToLower() + ".ico";

			// Does this flag exist?
			if(flags.Contains(filename))
			{
				// Return country flag
				return (int)flags[filename];
			}
			else
			{
				// Return unknown flag
				return (int)flags["_0.ico"];
				//return 0;
			}
		}

		// This returns a flag icon image
		public Image GetFlagIcon(char[] countrycode)
		{
			int index;

			// Make the filename
			string filename = (new string(countrycode)).ToLower() + ".ico";

			// Does this flag exist?
			if(flags.Contains(filename))
			{
				// Return country flag
				index = (int)flags[filename];
				return imglstFlags.Images[index];
			}
			else
			{
				// Return unknown flag
				index = (int)flags["_0.ico"];
				return imglstFlags.Images[index];
				//return null;
			}
		}

		// This refreshes the list
		public void RefreshGamesList()
		{
			// Mousecursor
			this.Cursor = Cursors.WaitCursor;

			// Start new queries
			ShowStatus("Requesting list of servers...");
			string result = browser.StartNewQuery();
			if(result != "") ShowStatus("Ready.  (" + result + ")"); else ShowStatus("Ready.");

			// Mousecursor
			this.Cursor = Cursors.Default;
		}

		// This joins the specified game
		private void JoinGame(IPAddress addr, int port, string password)
		{
			// Disable the games list
			// because a doubleclick somehow keeps
			// the focus there after the game started
			lstGames.Enabled = false;

			// Stop queries
			browser.StopQuery();

			// Save the settings
			General.SaveConfiguration();

			// Make arguments
			Configuration args = new Configuration();
			args.WriteSetting("join", addr + ":" + port);
			args.WriteSetting("password", password);

			// Start the game
			General.LaunchBloodmasters(args, "");

			// Re-enable games list
			lstGames.Enabled = true;
			lstGames.Focus();
		}

		// This asks for password input
		private bool AskPassword(out string password)
		{
			bool join;

			// Show dialog
			FormGamePassword pass = new FormGamePassword();
			if(pass.ShowDialog(this) == DialogResult.OK)
			{
				// Get result
				password = pass.txtJoinPassword.Text;
				join = true;
			}
			else
			{
				password = "";
				join = false;
			}

			// Clean up
			pass.Dispose();
			pass = null;

			// Return result
			return join;
		}

		// Key pressed
		private void FormMain_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Check if F5 is pressed
			if(e.KeyCode == Keys.F5)
			{
				// Check if Join Game tab is open
				if(tabsMode.SelectedIndex == 0)
				{
					// Refresh the list
					RefreshGamesList();
				}
			}
		}

		// Tab changes
		private void tabsMode_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Check what tab is open now
			// And change default button
			switch(tabsMode.SelectedIndex)
			{
				// Find Game
				case 0: this.AcceptButton = btnJoinGame1; break;

				// Host Game
				case 1: this.AcceptButton = btnHostGame; break;
			}
		}

		// Launch Host Game
		private void btnHostGame_Click(object sender, System.EventArgs e)
		{
			// Stop queries
			browser.StopQuery();

			// Save the settings
			General.SaveConfiguration();

			// Make the server configuration file
			string scfgfile = General.MakeUniqueFilename(General.apppath, "server_", ".cfg");
			Configuration scfg = MakeServerConfig(true);
			scfg.SaveConfiguration(scfgfile);

			// Make arguments
			Configuration args = new Configuration();
			if(chkServerDedicated.Checked == false)
				args.WriteSetting("host", scfgfile);
			else
				args.WriteSetting("dedicated", scfgfile);

			// Start the game
			General.LaunchBloodmasters(args, scfgfile);
		}

		// This makes a server configuration
		private Configuration MakeServerConfig(bool includercon)
		{
			// Make the server conifg
			Configuration cfg = new Configuration(true);

			// Basic settings
			cfg.WriteSetting("title", txtServerTitle.Text);
			cfg.WriteSetting("password", txtServerPassword.Text);
			cfg.WriteSetting("website", txtServerWebsite.Text);
			cfg.WriteSetting("port", (int)txtServerPort.Value);
			cfg.WriteSetting("gametype", cmbServerType.SelectedIndex);
			cfg.WriteSetting("scorelimit", (int)txtServerFraglimit.Value);
			cfg.WriteSetting("joinsmallest", chkServerJoinSmallest.Checked);
			cfg.WriteSetting("maxclients", (int)txtServerClients.Value);
			cfg.WriteSetting("maxplayers", (int)txtServerPlayers.Value);
			cfg.WriteSetting("timelimit", (int)txtServerTimelimit.Value);
			cfg.WriteSetting("public", chkServerAddToMaster.Checked);
			if(includercon) cfg.WriteSetting("rconpassword", General.RandomString(20));

			// Map names
			ListDictionary maps = new ListDictionary();
			foreach(string mname in lstMaps.CheckedItems) maps.Add(mname, null);
			cfg.WriteSetting("maps", maps);

			return cfg;
		}

		// Logo clicked
		private void picLogo_Click(object sender, System.EventArgs e)
		{
			// Open website
			this.Cursor = Cursors.WaitCursor;
			General.OpenWebsite("http://www.bloodmasters.com/");
			this.Cursor = Cursors.Default;
		}

		// Options clicked
		public void btnOptions_Click(object sender, System.EventArgs e)
		{
			// Load and show options dialog
			FormOptions options = new FormOptions();
			options.ShowDialog(this);
			options.Dispose();
		}

		// Server game type changed
		private void cmbServerType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// What game type is selected?
			switch((GAMETYPE)cmbServerType.SelectedIndex)
			{
				// Deathmatch
				case GAMETYPE.DM:
					lblFraglimit.Enabled = true;
					txtServerFraglimit.Enabled = true;
					chkServerJoinSmallest.Enabled = false;
					break;

				// Team Deathmatch
				case GAMETYPE.TDM:
					lblFraglimit.Enabled = true;
					txtServerFraglimit.Enabled = true;
					chkServerJoinSmallest.Enabled = true;
					break;

				// Capture The Flag
				case GAMETYPE.CTF:
					lblFraglimit.Enabled = true;
					txtServerFraglimit.Enabled = true;
					chkServerJoinSmallest.Enabled = true;
					break;

				// Scavenger
				case GAMETYPE.SC:
					lblFraglimit.Enabled = true;
					txtServerFraglimit.Enabled = true;
					chkServerJoinSmallest.Enabled = false;
					break;

				// Team Scavenger
				case GAMETYPE.TSC:
					lblFraglimit.Enabled = true;
					txtServerFraglimit.Enabled = true;
					chkServerJoinSmallest.Enabled = true;
					break;
			}

			// Refill maps list
			//RefillMapsList(new ListDictionary());
			RefillMapsList();
		}

		// Map in list selected
		private void lstMaps_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstMaps.SelectedIndex > -1)
			{
				// Clear previous image
				if(picMapPreview.Image != null) picMapPreview.Image.Dispose();
				picMapPreview.Image = null;

				// Find the preview image
				string bmpfilename = lstMaps.SelectedItem + ".bmp";
				string bmparchive = ArchiveManager.FindFileArchive(bmpfilename);
				if(bmparchive != "")
				{
					try
					{
						// Extract the .bmp file
						string bmptempfile = ArchiveManager.ExtractFile(bmparchive + "/" + bmpfilename);

						// Display map image
						picMapPreview.Image = Image.FromFile(bmptempfile);
					}
					catch(Exception)
					{
						// Unable to load map image, clear it
						if(picMapPreview.Image != null) picMapPreview.Image.Dispose();
						picMapPreview.Image = null;
					}
				}

				try
				{
					// Load the map information
					Map wadmap = new Map(lstMaps.SelectedItem.ToString(), true, General.temppath);

					// Display map information
					lblMapTitle.Text = wadmap.Title;
					lblMapAuthor.Text = wadmap.Author;
					lblMapPlayers.Text = "~ " + wadmap.RecommendedPlayers + " recommended";
				}
				catch(Exception)
				{
					// Display map information
					lblMapTitle.Text = "(invalid map)";
					lblMapAuthor.Text = "";
					lblMapPlayers.Text = "";
				}
			}
			else
			{
				// Clear
				if(picMapPreview.Image != null) picMapPreview.Image.Dispose();
				picMapPreview.Image = null;
				lblMapTitle.Text = "";
				lblMapAuthor.Text = "";
				lblMapPlayers.Text = "";
			}
		}

		// Window closes
		private void FormMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Make list of maps
			ListDictionary maplist = new ListDictionary();
			foreach(string mapname in lstMaps.CheckedItems) if(!maplist.Contains(mapname)) maplist.Add(mapname, null);

			// Save the interface settings to configuration
			if(this.WindowState == FormWindowState.Normal)
			{
				General.config.WriteSetting("windowleft", this.Location.X);
				General.config.WriteSetting("windowtop", this.Location.Y);
			}
			General.config.WriteSetting("servertitle", txtServerTitle.Text);
			General.config.WriteSetting("serverwebsite", txtServerWebsite.Text);
			General.config.WriteSetting("serverpassword", txtServerPassword.Text);
			General.config.WriteSetting("servertype", cmbServerType.SelectedIndex);
			General.config.WriteSetting("serverdedicated", chkServerDedicated.Checked);
			General.config.WriteSetting("serverpublic", chkServerAddToMaster.Checked);
			General.config.WriteSetting("serverclients", (int)txtServerClients.Value);
			General.config.WriteSetting("serverplayers", (int)txtServerPlayers.Value);
			General.config.WriteSetting("serverscorelimit", (int)txtServerFraglimit.Value);
			General.config.WriteSetting("servertimelimit", (int)txtServerTimelimit.Value);
			General.config.WriteSetting("serverjoinsmallest", chkServerJoinSmallest.Checked);
			General.config.WriteSetting("serverport", (int)txtServerPort.Value);
			General.config.WriteSetting("servermaps", maplist);
			General.config.WriteSetting("filtertitle", txtFilterTitles.Text);
			General.config.WriteSetting("filterfull", chkShowFull.Checked);
			General.config.WriteSetting("filterempty", chkShowEmpty.Checked);
			General.config.WriteSetting("filtertype", cmbType.SelectedIndex);
			General.config.WriteSetting("filtermap", cmbMap.Text);
		}

		// Window is resized
		private void FormMain_Resize(object sender, System.EventArgs e)
		{
			// Resize controls?
			if(this.WindowState != FormWindowState.Minimized)
			{
				// Lock window update
				General.LockWindowUpdate(this.Handle);

				// Perform resizing
				tabsMode.Width = this.ClientSize.Width - tabsMode.Left * 2;
				tabsMode.Height = (int)(this.ClientSize.Height - tabsMode.Top - stbStatus.Height - tabsMode.Left / 1.8f);
				tabsMode.PerformLayout();

				// Release window
				General.LockWindowUpdate(IntPtr.Zero);
			}

			// When in normal state
			if(this.WindowState == FormWindowState.Normal)
			{
				// Store window size and location
				General.config.WriteSetting("windowleft", this.Location.X);
				General.config.WriteSetting("windowtop", this.Location.Y);
				General.config.WriteSetting("windowwidth", this.Size.Width);
				General.config.WriteSetting("windowheight", this.Size.Height);
			}

			// Store window state
			if(this.WindowState != FormWindowState.Minimized)
				General.config.WriteSetting("windowstate", (int)this.WindowState);
			else
				General.config.WriteSetting("windowstate", (int)FormWindowState.Normal);
		}

		// Export configuration clicked
		private void btnExportConfig_Click(object sender, System.EventArgs e)
		{
			// Show export dialog
			DialogResult result = dlgSaveConfig.ShowDialog(this);
			if(result == DialogResult.OK)
			{
				// Make server configuration
				Configuration cfg = MakeServerConfig(false);

				// Write to file
				cfg.SaveConfiguration(dlgSaveConfig.FileName);
			}
		}

		// Refresh clicked
		private void btnRefreshGames_Click(object sender, System.EventArgs e)
		{
			// Refresh the list
			RefreshGamesList();
		}

		// Join Game clicked
		private void btnJoinGame1_Click(object sender, System.EventArgs e)
		{
			string pass = "";
			string filename;

			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// Check if the map is missing
				if(!CheckMapExists(gitem.MapName))
				{
					// Auto-download?
					if(General.config.ReadSetting("autodownload", true))
					{
						// Server URL valid?
						if(gitem.Website.ToLower().StartsWith("http://"))
						{
							// Show download dialog
							FormDownload download = new FormDownload(gitem);
							download.ShowDialog(this);
							download.Dispose();

							// Check if new file exists
							filename = Path.Combine(General.apppath, gitem.MapName + ".rar");
							if(File.Exists(filename))
							{
								// Busy!
								this.Cursor = Cursors.WaitCursor;
								this.Update();

								// Open the map archive
								try { ArchiveManager.OpenArchive(filename); }
								catch(Exception) { MessageBox.Show(this, "Unable to open the archive file " + gitem.MapName + ".rar. The file is not in the correct format.", "Downloading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

								// Refresh maps lists
								RefreshMapsLists();

								// Go for all items to see if they need updating
								for(int i = lstGames.Items.Count - 1; i >= 0; i--)
								{
									// Get the item
									ListViewItem aitem = lstGames.Items[i];

									// Get the server item
									GamesListItem gi = browser.GetItemByAddress(aitem.SubItems[9].Text);

									// Update the item
									gi.UpdateListViewItem(aitem);
								}

								// Done
								this.Cursor = Cursors.Default;
							}
							else
							{
								// Cancelled
								return;
							}
						}
						else
						{
							// Dont have the map and server wbesite is not given
							MessageBox.Show(this, "You do not have this map and the download website information is not available.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return;
						}
					}
					else
					{
						// Dont have the map
						MessageBox.Show(this, "You cannot join this game, because you do not have this map.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}

				// Check if map exists
				if(CheckMapExists(gitem.MapName))
				{
					// Game locked?
					if(gitem.Locked)
					{
						// Ask for password input
						if(!AskPassword(out pass)) return;
					}

					// Join the game
					JoinGame(gitem.Address.Address, gitem.Address.Port, pass);
				}
			}
		}

		// Item in games list double clicked
		private void lstGames_DoubleClick(object sender, System.EventArgs e)
		{
			// Click the join button
			if(btnJoinGame1.Enabled) btnJoinGame1_Click(sender, e);
		}

		// Filter title changed
		private void txtFilterTitles_TextChanged(object sender, System.EventArgs e)
		{
			// Set the filter
			browser.FilterTitle = txtFilterTitles.Text;
		}

		// Show Full changed
		private void chkShowFull_CheckedChanged(object sender, System.EventArgs e)
		{
			// Set the filter
			browser.FilterFull = chkShowFull.Checked;
		}

		// Show Empty changed
		private void chkShowEmpty_CheckedChanged(object sender, System.EventArgs e)
		{
			// Set the filter
			browser.FilterEmpty = chkShowEmpty.Checked;
		}

		// Filter Game Type changed
		private void cmbType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Set the filter
			browser.FilterType = cmbType.SelectedIndex;
		}

		// Filter Map changed
		private void cmbMap_TextChanged(object sender, System.EventArgs e)
		{
			// Set the filter
			browser.FilterMap = cmbMap.Text;
		}

		// This is called when the filtered list is changed
		private void FilteredListChanged()
		{
			// Schedule a list update
			updatelist = true;
		}

		// When the list must be updated
		private void tmrUpdateList_Tick(object sender, System.EventArgs e)
		{
			// Update needed?
			if(updatelist)
			{
				// Dont refresh the list
				General.LockWindowUpdate(lstGames.Handle);
				lstGames.BeginUpdate();

				// Get the whole list
				Hashtable gitems = browser.GetFilteredList();

				// Go for all items to see if they need updating
				for(int i = lstGames.Items.Count - 1; i >= 0; i--)
				{
					// Get the item
					ListViewItem item = lstGames.Items[i];

					// Item in the new collection?
					if(gitems.Contains(item.SubItems[9].Text))
					{
						// Get the server item
						GamesListItem gi = (GamesListItem)gitems[item.SubItems[9].Text];

						// Update the item ifchanged
						if(gi.Changed) gi.UpdateListViewItem(item);

						// Remove from server items
						gitems.Remove(item.SubItems[9].Text);
					}
					else
					{
						// Remove item from list
						lstGames.Items.RemoveAt(i);
					}
				}

				// Go for all remaining items to add to the list
				foreach(DictionaryEntry de in gitems)
				{
					// Get the server item
					GamesListItem gi = (GamesListItem)de.Value;

					// Add to the list
					gi.NewListViewItem(lstGames);
				}

				// Redraw the list
				lstGames.EndUpdate();
				General.LockWindowUpdate(IntPtr.Zero);
				updatelist = false;
			}
		}

		// Game Details clicked
		private void btnGameDetails_Click(object sender, System.EventArgs e)
		{
			string pass = "";
			bool dojoin;

			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// Load and show details dialog
				FormGameInfo gameinfowindow = new FormGameInfo(gitem);
				if(gameinfowindow.ShowDialog(this) == DialogResult.OK)
				{
					// Join this game
					btnJoinGame1_Click(sender, e);
				}

				// Clean up
				gameinfowindow.Dispose();
				gameinfowindow = null;
			}
		}

		// Game item (de)selected
		private void lstGames_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Enable/disable controls
			btnJoinGame1.Enabled = (lstGames.SelectedItems.Count > 0);
			btnGameDetails.Enabled = (lstGames.SelectedItems.Count > 0);
		}

		// Mouse button released
		private void lstGames_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Right mouse button?
			if(e.Button == MouseButtons.Right)
			{
				// Anything selected?
				if(lstGames.SelectedItems.Count > 0)
				{
					// Get the selected item
					ListViewItem item = lstGames.SelectedItems[0];
					GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

					// Setup popup menu
					itmGameWebsite.Enabled = gitem.Website.ToLower().StartsWith("http://");

					// Show popup menu
					mnuGame.Show(lstGames, new Point(e.X, e.Y));
				}
			}
		}

		// Copy game title
		private void itmCopyGameTitle_Click(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// Copy information
				Clipboard.SetDataObject(gitem.Title, true);
			}
		}

		// Copy game details
		private void itmCopyGameDetails_Click(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// Copy information
				Clipboard.SetDataObject("Server: \"" + gitem.Title + "\"" +
										" Address: " + gitem.Address +
										" Ping: " + gitem.Ping + "ms" +
										" Players: " + gitem.Players + "/" + gitem.MaxPlayers +
										" Clients: " + gitem.Clients + "/" + gitem.MaxClients +
										" Type: " + gitem.GameType +
										" Map: " + gitem.MapName, true);
			}
		}

		// Copy game address
		private void itmCopyGameIP_Click(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// Copy information
				Clipboard.SetDataObject(gitem.Address.ToString(), true);
			}
		}

		// Mouse moves over map list
		private void lstMaps_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Get the pointed item
			int index = lstMaps.IndexFromPoint(e.X, e.Y);
			if((index > -1) && (index < lstMaps.Items.Count))
			{
				// Highlight the item
				lstMaps.SelectedIndex = index;
			}
			else
			{
				// Dehighlight or unhighlight whatever you call it
				lstMaps.SelectedIndex = -1;
			}
		}

		// Mouse leaves maps list
		private void lstMaps_MouseLeave(object sender, System.EventArgs e)
		{
			// Dehighlight
			lstMaps.SelectedIndex = -1;
		}

		// Specify clicked
		private void btnManualJoin_Click(object sender, System.EventArgs e)
		{
			IPHostEntry ip = null;

			// Show the specify dialog
			FormGameSpecify specify = new FormGameSpecify();
			if(specify.ShowDialog(this) == DialogResult.OK)
			{
				// Show status
				ShowStatus("Resolving address...");

				// Stop queries
				browser.StopQuery();

				// Try to resolve the address
				try { ip = Dns.Resolve(specify.txtJoinAddress.Text); } catch (Exception) { }
				if((ip == null) || (ip.AddressList.Length == 0))
				{
					// No result
					ShowStatus("Ready.");
					MessageBox.Show(this, "Unable to resolve the specified server address.",
							"Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else
				{
					// Join the game
					JoinGame(ip.AddressList[0], int.Parse(specify.txtJoinPort.Text),
													specify.txtJoinPassword.Text);
				}
			}

			// Clean up
			specify.Dispose();
			specify = null;
		}

		// Browse website
		private void itmGameWebsite_Click(object sender, System.EventArgs e)
		{
			// Anything selected?
			if(lstGames.SelectedItems.Count > 0)
			{
				// Get the selected item
				ListViewItem item = lstGames.SelectedItems[0];
				GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

				// The website MUST start with http://
				if(gitem.Website.ToLower().StartsWith("http://"))
				{
					// Open website
					this.Cursor = Cursors.WaitCursor;
					General.OpenWebsite(gitem.Website);
					this.Cursor = Cursors.Default;
				}
			}
		}

		// Column clicked in games list
		private void lstGames_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			int subitemindex = 0;
			bool ascending = General.config.ReadSetting("sortascending", true);

			// Determine subitem to sort by for this column
			switch(e.Column)
			{
				case 0: subitemindex = 0; break;	// Title
				case 1: subitemindex = 6; break;	// Ping
				case 2: subitemindex = 7; break;	// Players
				case 3: subitemindex = 8; break;	// Clients
				case 4: subitemindex = 4; break;	// Game Type
				case 5: subitemindex = 5; break;	// Map
			}

			// Already sorted by this subitem?
			if(General.config.ReadSetting("sortitem", 6) == subitemindex)
			{
				// Change sort order
				ascending = !ascending;
			}

			// Make new sort comparer
			lstGames.ListViewItemSorter = new GamesListItemComparer(subitemindex, ascending);

			// Save settings
			General.config.WriteSetting("sortitem", subitemindex);
			General.config.WriteSetting("sortascending", ascending);
		}
	}
}
