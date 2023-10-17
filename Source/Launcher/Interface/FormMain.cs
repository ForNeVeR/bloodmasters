/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.Bloodmasters.Launcher.Map;

namespace CodeImp.Bloodmasters.Launcher.Interface;

public class FormMain : System.Windows.Forms.Form
{
    private ServerBrowser browser;
    private bool updatelist = false;
    private Dictionary<string, int> flags = new();

    private System.Windows.Forms.PictureBox picLogo;
    private System.Windows.Forms.TabControl tabsMode;
    private System.Windows.Forms.TabPage tabHost;
    private System.Windows.Forms.TabPage tabFind;
    private System.Windows.Forms.Button btnExitJoinGame;
    private System.Windows.Forms.Button btnHostGame;
    private System.Windows.Forms.Button btnOptionsJoinGame;
    private System.Windows.Forms.Button btnExitHostGame;
    private System.Windows.Forms.Button btnOptionsHostGame;
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
    private Panel pnlIcon;
    private Panel pnlTabs;
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
                      ControlStyles.DoubleBuffer, true);
        this.UpdateStyles();

        // Load window state from configuration
        this.WindowState = (FormWindowState)Program.config.ReadSetting("windowstate", 0);

        // Required for Windows Form Designer support
        InitializeComponent();

        // Show version number
        Version v = Assembly.GetExecutingAssembly().GetName().Version;
        stpVersion.Text = "  version " + v.ToString(4);

        // Initialize the browser
        browser = new ServerBrowser();
        browser.OnFilteredListChanged += new ServerBrowser.FilteredListChanged(FilteredListChanged);

        // Load window size and location from configuration
        this.Location = new Point(Program.config.ReadSetting("windowleft", 200),
            Program.config.ReadSetting("windowtop", 200));
        this.Size = new Size(Program.config.ReadSetting("windowwidth", this.Size.Width),
            Program.config.ReadSetting("windowheight", this.Size.Height));

        // Load the interface settings from configuration
        maxclients = Program.config.ReadSetting("serverclients", 10);
        maxplayers = Program.config.ReadSetting("serverplayers", 10);
        if (maxclients > (int)txtServerClients.Maximum) maxclients = (int)txtServerClients.Maximum;
        if (maxplayers > (int)txtServerPlayers.Maximum) maxplayers = (int)txtServerPlayers.Maximum;
        txtFilterTitles.Text = Program.config.ReadSetting("filtertitle", "");
        chkShowFull.Checked = Program.config.ReadSetting("filterfull", true);
        chkShowEmpty.Checked = Program.config.ReadSetting("filterempty", true);
        cmbType.SelectedIndex = Program.config.ReadSetting("filtertype", 0);
        cmbMap.Text = Program.config.ReadSetting("filtermap", "");
        txtServerTitle.Text = Program.config.ReadSetting("servertitle", "");
        txtServerWebsite.Text = Program.config.ReadSetting("serverwebsite", "");
        txtServerPassword.Text = Program.config.ReadSetting("serverpassword", "");
        cmbServerType.SelectedIndex = Program.config.ReadSetting("servertype", 0);
        chkServerDedicated.Checked = Program.config.ReadSetting("serverdedicated", false);
        chkServerAddToMaster.Checked = Program.config.ReadSetting("serverpublic", true);
        txtServerClients.Value = maxclients;
        txtServerPlayers.Value = maxplayers;
        txtServerFraglimit.Value = Program.config.ReadSetting("serverscorelimit", 20);
        txtServerTimelimit.Value = Program.config.ReadSetting("servertimelimit", 15);
        chkServerJoinSmallest.Checked = Program.config.ReadSetting("serverjoinsmallest", true);
        txtServerPort.Value = Program.config.ReadSetting("serverport", Consts.DEFAULT_SERVER_PORT);
        //servermaps = General.config.ReadSetting("servermaps", new Hashtable());

        // Apply sort comparer
        ascending = Program.config.ReadSetting("sortascending", true);
        subitemindex = Program.config.ReadSetting("sortitem", 6);
        lstGames.ListViewItemSorter = new GamesListItemComparer(subitemindex, ascending);

        // DEBUG:
        //tabsMode.SelectedIndex = 1;
    }

    // Clean up any resources being used.
    protected override void Dispose(bool disposing)
    {
        // Clear map preview image
        if (picMapPreview.Image != null) picMapPreview.Image.Dispose();
        picMapPreview.Image = null;

        // Stop queries
        browser.StopQuery();

        // Check if disposing
        if (disposing)
        {
            // Dispose components, if any
            if (components != null) components.Dispose();
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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
        picLogo = new PictureBox();
        tabsMode = new TabControl();
        tabFind = new TabPage();
        btnRefreshGames = new Button();
        btnGameDetails = new Button();
        btnManualJoin = new Button();
        btnJoinGame1 = new Button();
        cmbMap = new ComboBox();
        lblMap = new Label();
        cmbType = new ComboBox();
        lblType = new Label();
        chkShowEmpty = new CheckBox();
        chkShowFull = new CheckBox();
        txtFilterTitles = new TextBox();
        lblFilterTitles = new Label();
        lstGames = new ListView();
        clmTitle = new ColumnHeader();
        clmPing = new ColumnHeader();
        clmPlayers = new ColumnHeader();
        clmClients = new ColumnHeader();
        clmType = new ColumnHeader();
        clmMap = new ColumnHeader();
        imglstFlags = new ImageList(components);
        imglstGame = new ImageList(components);
        tabHost = new TabPage();
        btnExportConfig = new Button();
        btnImportConfig = new Button();
        btnHostGame = new Button();
        pnlHostGame = new Panel();
        grpMaps = new GroupBox();
        lblMapTitle = new Label();
        label9 = new Label();
        lblMapPlayers = new Label();
        lblMapAuthor = new Label();
        picMapPreview = new PictureBox();
        label6 = new Label();
        label5 = new Label();
        lstMaps = new CheckedListBox();
        grpSettings = new GroupBox();
        txtServerPort = new NumericUpDown();
        label7 = new Label();
        chkServerJoinSmallest = new CheckBox();
        txtServerTimelimit = new NumericUpDown();
        txtServerFraglimit = new NumericUpDown();
        txtServerPlayers = new NumericUpDown();
        txtServerClients = new NumericUpDown();
        lblTimelimit = new Label();
        lblFraglimit = new Label();
        lblPlayers = new Label();
        lblClients = new Label();
        grpGeneral = new GroupBox();
        chkServerAddToMaster = new CheckBox();
        txtServerPassword = new TextBox();
        label4 = new Label();
        cmbServerType = new ComboBox();
        label3 = new Label();
        txtServerWebsite = new TextBox();
        label2 = new Label();
        txtServerTitle = new TextBox();
        label1 = new Label();
        chkServerDedicated = new CheckBox();
        btnOptionsJoinGame = new Button();
        btnExitJoinGame = new Button();
        btnOptionsHostGame = new Button();
        btnExitHostGame = new Button();
        mnuGame = new ContextMenuStrip(components);
        itmJoinGame1 = new ToolStripMenuItem();
        itmGameDetails = new ToolStripMenuItem();
        menuItem3 = new ToolStripMenuItem();
        itmGameWebsite = new ToolStripMenuItem();
        menuItem7 = new ToolStripMenuItem();
        itmCopyGameTitle = new ToolStripMenuItem();
        itmCopyGameDetails = new ToolStripMenuItem();
        itmCopyGameIP = new ToolStripMenuItem();
        tltDescription = new ToolTip(components);
        stbStatus = new StatusStrip();
        stpPanel = new ToolStripStatusLabel();
        stpVersion = new ToolStripStatusLabel();
        dlgSaveConfig = new SaveFileDialog();
        tmrUpdateList = new Timer(components);
        pnlIcon = new Panel();
        pnlTabs = new Panel();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        tabsMode.SuspendLayout();
        tabFind.SuspendLayout();
        tabHost.SuspendLayout();
        pnlHostGame.SuspendLayout();
        grpMaps.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picMapPreview).BeginInit();
        grpSettings.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)txtServerPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)txtServerTimelimit).BeginInit();
        ((System.ComponentModel.ISupportInitialize)txtServerFraglimit).BeginInit();
        ((System.ComponentModel.ISupportInitialize)txtServerPlayers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)txtServerClients).BeginInit();
        grpGeneral.SuspendLayout();
        mnuGame.SuspendLayout();
        stbStatus.SuspendLayout();
        pnlIcon.SuspendLayout();
        pnlTabs.SuspendLayout();
        SuspendLayout();
        //
        // picLogo
        //
        picLogo.BackColor = Color.Black;
        picLogo.BorderStyle = BorderStyle.Fixed3D;
        picLogo.Cursor = Cursors.Hand;
        picLogo.Dock = DockStyle.Fill;
        picLogo.Image = (Image)resources.GetObject("picLogo.Image");
        picLogo.Location = new Point(6, 6);
        picLogo.Name = "picLogo";
        picLogo.Size = new Size(939, 94);
        picLogo.SizeMode = PictureBoxSizeMode.CenterImage;
        picLogo.TabIndex = 0;
        picLogo.TabStop = false;
        picLogo.Click += picLogo_Click;
        //
        // tabsMode
        //
        tabsMode.Controls.Add(tabFind);
        tabsMode.Controls.Add(tabHost);
        tabsMode.Dock = DockStyle.Fill;
        tabsMode.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        tabsMode.Location = new Point(6, 0);
        tabsMode.Name = "tabsMode";
        tabsMode.Padding = new Point(25, 3);
        tabsMode.SelectedIndex = 0;
        tabsMode.Size = new Size(939, 563);
        tabsMode.TabIndex = 1;
        tabsMode.SelectedIndexChanged += tabsMode_SelectedIndexChanged;
        //
        // tabFind
        //
        tabFind.BackColor = SystemColors.Control;
        tabFind.Controls.Add(btnRefreshGames);
        tabFind.Controls.Add(btnGameDetails);
        tabFind.Controls.Add(btnManualJoin);
        tabFind.Controls.Add(btnJoinGame1);
        tabFind.Controls.Add(btnOptionsJoinGame);
        tabFind.Controls.Add(btnExitJoinGame);
        tabFind.Controls.Add(cmbMap);
        tabFind.Controls.Add(lblMap);
        tabFind.Controls.Add(cmbType);
        tabFind.Controls.Add(lblType);
        tabFind.Controls.Add(chkShowEmpty);
        tabFind.Controls.Add(chkShowFull);
        tabFind.Controls.Add(txtFilterTitles);
        tabFind.Controls.Add(lblFilterTitles);
        tabFind.Controls.Add(lstGames);
        tabFind.Font = new Font("Arial", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        tabFind.Location = new Point(4, 25);
        tabFind.Name = "tabFind";
        tabFind.Size = new Size(931, 534);
        tabFind.TabIndex = 3;
        tabFind.Text = "Find Game";
        //
        // btnRefreshGames
        //
        btnRefreshGames.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnRefreshGames.FlatStyle = FlatStyle.System;
        btnRefreshGames.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnRefreshGames.Location = new Point(13, 479);
        btnRefreshGames.Name = "btnRefreshGames";
        btnRefreshGames.Size = new Size(146, 34);
        btnRefreshGames.TabIndex = 14;
        btnRefreshGames.Text = "Refresh";
        tltDescription.SetToolTip(btnRefreshGames, "Refresh entire servers list");
        btnRefreshGames.Click += btnRefreshGames_Click;
        //
        // btnGameDetails
        //
        btnGameDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnGameDetails.Enabled = false;
        btnGameDetails.FlatStyle = FlatStyle.System;
        btnGameDetails.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnGameDetails.Location = new Point(164, 479);
        btnGameDetails.Name = "btnGameDetails";
        btnGameDetails.Size = new Size(146, 34);
        btnGameDetails.TabIndex = 13;
        btnGameDetails.Text = " Game Details...";
        tltDescription.SetToolTip(btnGameDetails, "Shows detailed game information of the selected server");
        btnGameDetails.Click += btnGameDetails_Click;
        //
        // btnManualJoin
        //
        btnManualJoin.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnManualJoin.FlatStyle = FlatStyle.System;
        btnManualJoin.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnManualJoin.Location = new Point(467, 479);
        btnManualJoin.Name = "btnManualJoin";
        btnManualJoin.Size = new Size(145, 34);
        btnManualJoin.TabIndex = 12;
        btnManualJoin.Text = " Specify...";
        tltDescription.SetToolTip(btnManualJoin, "Allows you to specify a server by IP address and port number");
        btnManualJoin.Click += btnManualJoin_Click;
        //
        // btnJoinGame1
        //
        btnJoinGame1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnJoinGame1.Enabled = false;
        btnJoinGame1.FlatStyle = FlatStyle.System;
        btnJoinGame1.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnJoinGame1.Location = new Point(316, 479);
        btnJoinGame1.Name = "btnJoinGame1";
        btnJoinGame1.Size = new Size(145, 34);
        btnJoinGame1.TabIndex = 11;
        btnJoinGame1.Text = "Join";
        tltDescription.SetToolTip(btnJoinGame1, "Joins the selected game");
        btnJoinGame1.Click += btnJoinGame1_Click;
        //
        // cmbMap
        //
        cmbMap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbMap.Location = new Point(730, 15);
        cmbMap.MaxDropDownItems = 10;
        cmbMap.Name = "cmbMap";
        cmbMap.Size = new Size(185, 24);
        cmbMap.Sorted = true;
        cmbMap.TabIndex = 10;
        tltDescription.SetToolTip(cmbMap, "Whole or part of the short map name you are looking for");
        cmbMap.TextChanged += cmbMap_TextChanged;
        //
        // lblMap
        //
        lblMap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblMap.BackColor = Color.Transparent;
        lblMap.Location = new Point(674, 15);
        lblMap.Name = "lblMap";
        lblMap.Size = new Size(50, 24);
        lblMap.TabIndex = 9;
        lblMap.Text = "Map:";
        lblMap.TextAlign = ContentAlignment.MiddleRight;
        lblMap.UseMnemonic = false;
        //
        // cmbType
        //
        cmbType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.Items.AddRange(new object[] { "Any", "DM", "TDM", "CTF", "SC", "TSC" });
        cmbType.Location = new Point(562, 15);
        cmbType.MaxDropDownItems = 12;
        cmbType.Name = "cmbType";
        cmbType.Size = new Size(90, 24);
        cmbType.TabIndex = 8;
        tltDescription.SetToolTip(cmbType, "Type of game you are looking for");
        cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
        //
        // lblType
        //
        lblType.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblType.Location = new Point(500, 15);
        lblType.Name = "lblType";
        lblType.Size = new Size(56, 24);
        lblType.TabIndex = 7;
        lblType.Text = "Type:";
        lblType.TextAlign = ContentAlignment.MiddleRight;
        //
        // chkShowEmpty
        //
        chkShowEmpty.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkShowEmpty.FlatStyle = FlatStyle.System;
        chkShowEmpty.Location = new Point(411, 15);
        chkShowEmpty.Name = "chkShowEmpty";
        chkShowEmpty.Size = new Size(84, 26);
        chkShowEmpty.TabIndex = 4;
        chkShowEmpty.Text = "Empty";
        tltDescription.SetToolTip(chkShowEmpty, "Shows empty servers when checked");
        chkShowEmpty.CheckedChanged += chkShowEmpty_CheckedChanged;
        //
        // chkShowFull
        //
        chkShowFull.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkShowFull.FlatStyle = FlatStyle.System;
        chkShowFull.Location = new Point(327, 15);
        chkShowFull.Name = "chkShowFull";
        chkShowFull.Size = new Size(67, 26);
        chkShowFull.TabIndex = 3;
        chkShowFull.Text = "Full";
        tltDescription.SetToolTip(chkShowFull, "Shows full servers when checked");
        chkShowFull.CheckedChanged += chkShowFull_CheckedChanged;
        //
        // txtFilterTitles
        //
        txtFilterTitles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtFilterTitles.Location = new Point(95, 15);
        txtFilterTitles.Name = "txtFilterTitles";
        txtFilterTitles.Size = new Size(193, 23);
        txtFilterTitles.TabIndex = 2;
        tltDescription.SetToolTip(txtFilterTitles, "Whole or part of the server title you are looking for");
        txtFilterTitles.TextChanged += txtFilterTitles_TextChanged;
        //
        // lblFilterTitles
        //
        lblFilterTitles.BackColor = Color.Transparent;
        lblFilterTitles.Location = new Point(6, 15);
        lblFilterTitles.Name = "lblFilterTitles";
        lblFilterTitles.Size = new Size(84, 24);
        lblFilterTitles.TabIndex = 1;
        lblFilterTitles.Text = "Filter titles:";
        lblFilterTitles.TextAlign = ContentAlignment.MiddleRight;
        lblFilterTitles.UseMnemonic = false;
        //
        // lstGames
        //
        lstGames.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstGames.Columns.AddRange(new ColumnHeader[] { clmTitle, clmPing, clmPlayers, clmClients, clmType, clmMap });
        lstGames.FullRowSelect = true;
        lstGames.LabelWrap = false;
        lstGames.Location = new Point(11, 54);
        lstGames.MultiSelect = false;
        lstGames.Name = "lstGames";
        lstGames.Size = new Size(904, 421);
        lstGames.SmallImageList = imglstFlags;
        lstGames.StateImageList = imglstGame;
        lstGames.TabIndex = 0;
        lstGames.UseCompatibleStateImageBehavior = false;
        lstGames.View = View.Details;
        lstGames.ColumnClick += lstGames_ColumnClick;
        lstGames.SelectedIndexChanged += lstGames_SelectedIndexChanged;
        lstGames.DoubleClick += lstGames_DoubleClick;
        lstGames.MouseUp += lstGames_MouseUp;
        //
        // clmTitle
        //
        clmTitle.Text = "Server";
        clmTitle.Width = 259;
        //
        // clmPing
        //
        clmPing.Text = "Ping";
        clmPing.TextAlign = HorizontalAlignment.Right;
        clmPing.Width = 52;
        //
        // clmPlayers
        //
        clmPlayers.Text = "Players";
        clmPlayers.TextAlign = HorizontalAlignment.Right;
        //
        // clmClients
        //
        clmClients.Text = "Clients";
        clmClients.TextAlign = HorizontalAlignment.Right;
        //
        // clmType
        //
        clmType.Text = "Type";
        clmType.TextAlign = HorizontalAlignment.Right;
        clmType.Width = 50;
        //
        // clmMap
        //
        clmMap.Text = "Current Map";
        clmMap.TextAlign = HorizontalAlignment.Right;
        clmMap.Width = 128;
        //
        // imglstFlags
        //
        imglstFlags.ColorDepth = ColorDepth.Depth32Bit;
        imglstFlags.ImageStream = (ImageListStreamer)resources.GetObject("imglstFlags.ImageStream");
        imglstFlags.TransparentColor = Color.Transparent;
        imglstFlags.Images.SetKeyName(0, "");
        //
        // imglstGame
        //
        imglstGame.ColorDepth = ColorDepth.Depth32Bit;
        imglstGame.ImageStream = (ImageListStreamer)resources.GetObject("imglstGame.ImageStream");
        imglstGame.TransparentColor = Color.Transparent;
        imglstGame.Images.SetKeyName(0, "");
        imglstGame.Images.SetKeyName(1, "");
        //
        // tabHost
        //
        tabHost.Controls.Add(btnExportConfig);
        tabHost.Controls.Add(btnImportConfig);
        tabHost.Controls.Add(btnHostGame);
        tabHost.Controls.Add(btnOptionsHostGame);
        tabHost.Controls.Add(btnExitHostGame);
        tabHost.Controls.Add(pnlHostGame);
        tabHost.Font = new Font("Arial", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        tabHost.Location = new Point(4, 25);
        tabHost.Name = "tabHost";
        tabHost.Size = new Size(943, 534);
        tabHost.TabIndex = 1;
        tabHost.Text = "Host Game";
        //
        // btnExportConfig
        //
        btnExportConfig.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExportConfig.FlatStyle = FlatStyle.System;
        btnExportConfig.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnExportConfig.Location = new Point(326, 479);
        btnExportConfig.Name = "btnExportConfig";
        btnExportConfig.Size = new Size(145, 34);
        btnExportConfig.TabIndex = 13;
        btnExportConfig.Text = "Export Config...";
        tltDescription.SetToolTip(btnExportConfig, "Saves the server configuration to a file");
        btnExportConfig.Click += btnExportConfig_Click;
        //
        // btnImportConfig
        //
        btnImportConfig.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnImportConfig.FlatStyle = FlatStyle.System;
        btnImportConfig.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnImportConfig.Location = new Point(174, 479);
        btnImportConfig.Name = "btnImportConfig";
        btnImportConfig.Size = new Size(146, 34);
        btnImportConfig.TabIndex = 14;
        btnImportConfig.Text = "Import Config...";
        tltDescription.SetToolTip(btnImportConfig, "Loads the server configuration from a file");
        //
        // btnHostGame
        //
        btnHostGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnHostGame.FlatStyle = FlatStyle.System;
        btnHostGame.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnHostGame.Location = new Point(477, 479);
        btnHostGame.Name = "btnHostGame";
        btnHostGame.Size = new Size(145, 34);
        btnHostGame.TabIndex = 3;
        btnHostGame.Text = "Launch";
        tltDescription.SetToolTip(btnHostGame, "Launches the server and joins the game if not dedicated");
        btnHostGame.Click += btnHostGame_Click;
        btnHostGame.Enabled = false;
        //
        // pnlHostGame
        //
        pnlHostGame.Anchor = AnchorStyles.None;
        pnlHostGame.Controls.Add(grpMaps);
        pnlHostGame.Controls.Add(grpSettings);
        pnlHostGame.Controls.Add(grpGeneral);
        pnlHostGame.Location = new Point(0, 0);
        pnlHostGame.Name = "pnlHostGame";
        pnlHostGame.Padding = new Padding(6);
        pnlHostGame.Size = new Size(943, 534);
        pnlHostGame.TabIndex = 4;
        //
        // grpMaps
        //
        grpMaps.Controls.Add(lblMapTitle);
        grpMaps.Controls.Add(label9);
        grpMaps.Controls.Add(lblMapPlayers);
        grpMaps.Controls.Add(lblMapAuthor);
        grpMaps.Controls.Add(picMapPreview);
        grpMaps.Controls.Add(label6);
        grpMaps.Controls.Add(label5);
        grpMaps.Controls.Add(lstMaps);
        grpMaps.Location = new Point(297, 128);
        grpMaps.Name = "grpMaps";
        grpMaps.Size = new Size(571, 310);
        grpMaps.TabIndex = 16;
        grpMaps.TabStop = false;
        grpMaps.Text = " Maps ";
        //
        // lblMapTitle
        //
        lblMapTitle.BackColor = Color.Transparent;
        lblMapTitle.Location = new Point(336, 226);
        lblMapTitle.Name = "lblMapTitle";
        lblMapTitle.Size = new Size(218, 25);
        lblMapTitle.TabIndex = 28;
        lblMapTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblMapTitle.UseMnemonic = false;
        //
        // label9
        //
        label9.BackColor = Color.Transparent;
        label9.Location = new Point(263, 226);
        label9.Name = "label9";
        label9.Size = new Size(67, 25);
        label9.TabIndex = 27;
        label9.Text = "Title:";
        label9.TextAlign = ContentAlignment.MiddleRight;
        label9.UseMnemonic = false;
        //
        // lblMapPlayers
        //
        lblMapPlayers.BackColor = Color.Transparent;
        lblMapPlayers.Location = new Point(336, 276);
        lblMapPlayers.Name = "lblMapPlayers";
        lblMapPlayers.Size = new Size(218, 24);
        lblMapPlayers.TabIndex = 26;
        lblMapPlayers.TextAlign = ContentAlignment.MiddleLeft;
        lblMapPlayers.UseMnemonic = false;
        //
        // lblMapAuthor
        //
        lblMapAuthor.BackColor = Color.Transparent;
        lblMapAuthor.Location = new Point(336, 251);
        lblMapAuthor.Name = "lblMapAuthor";
        lblMapAuthor.Size = new Size(218, 25);
        lblMapAuthor.TabIndex = 25;
        lblMapAuthor.TextAlign = ContentAlignment.MiddleLeft;
        lblMapAuthor.UseMnemonic = false;
        //
        // picMapPreview
        //
        picMapPreview.BackColor = SystemColors.AppWorkspace;
        picMapPreview.BorderStyle = BorderStyle.Fixed3D;
        picMapPreview.Location = new Point(269, 30);
        picMapPreview.Name = "picMapPreview";
        picMapPreview.Size = new Size(285, 189);
        picMapPreview.SizeMode = PictureBoxSizeMode.StretchImage;
        picMapPreview.TabIndex = 24;
        picMapPreview.TabStop = false;
        //
        // label6
        //
        label6.BackColor = Color.Transparent;
        label6.Location = new Point(263, 276);
        label6.Name = "label6";
        label6.Size = new Size(67, 24);
        label6.TabIndex = 23;
        label6.Text = "Players:";
        label6.TextAlign = ContentAlignment.MiddleRight;
        label6.UseMnemonic = false;
        //
        // label5
        //
        label5.BackColor = Color.Transparent;
        label5.Location = new Point(263, 251);
        label5.Name = "label5";
        label5.Size = new Size(67, 25);
        label5.TabIndex = 22;
        label5.Text = "Author:";
        label5.TextAlign = ContentAlignment.MiddleRight;
        label5.UseMnemonic = false;
        //
        // lstMaps
        //
        lstMaps.CheckOnClick = true;
        lstMaps.IntegralHeight = false;
        lstMaps.Location = new Point(17, 30);
        lstMaps.Name = "lstMaps";
        lstMaps.ScrollAlwaysVisible = true;
        lstMaps.Size = new Size(235, 265);
        lstMaps.Sorted = true;
        lstMaps.TabIndex = 0;
        lstMaps.ItemCheck += lstMaps_ItemCheck;
        lstMaps.SelectedIndexChanged += lstMaps_SelectedIndexChanged;
        lstMaps.MouseLeave += lstMaps_MouseLeave;
        lstMaps.MouseMove += lstMaps_MouseMove;
        //
        // grpSettings
        //
        grpSettings.BackColor = Color.Transparent;
        grpSettings.Controls.Add(txtServerPort);
        grpSettings.Controls.Add(label7);
        grpSettings.Controls.Add(chkServerJoinSmallest);
        grpSettings.Controls.Add(txtServerTimelimit);
        grpSettings.Controls.Add(txtServerFraglimit);
        grpSettings.Controls.Add(txtServerPlayers);
        grpSettings.Controls.Add(txtServerClients);
        grpSettings.Controls.Add(lblTimelimit);
        grpSettings.Controls.Add(lblFraglimit);
        grpSettings.Controls.Add(lblPlayers);
        grpSettings.Controls.Add(lblClients);
        grpSettings.Location = new Point(6, 128);
        grpSettings.Name = "grpSettings";
        grpSettings.Size = new Size(274, 310);
        grpSettings.TabIndex = 15;
        grpSettings.TabStop = false;
        grpSettings.Text = " Settings ";
        //
        // txtServerPort
        //
        txtServerPort.Location = new Point(134, 271);
        txtServerPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        txtServerPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        txtServerPort.Name = "txtServerPort";
        txtServerPort.Size = new Size(96, 23);
        txtServerPort.TabIndex = 23;
        txtServerPort.TextAlign = HorizontalAlignment.Center;
        tltDescription.SetToolTip(txtServerPort, "Maximum number of clients allowed on server");
        txtServerPort.Value = new decimal(new int[] { 6969, 0, 0, 0 });
        //
        // label7
        //
        label7.Location = new Point(17, 271);
        label7.Name = "label7";
        label7.Size = new Size(112, 19);
        label7.TabIndex = 22;
        label7.Text = "Server port:";
        label7.TextAlign = ContentAlignment.BottomRight;
        label7.UseMnemonic = false;
        //
        // chkServerJoinSmallest
        //
        chkServerJoinSmallest.BackColor = SystemColors.Control;
        chkServerJoinSmallest.FlatStyle = FlatStyle.System;
        chkServerJoinSmallest.Location = new Point(45, 172);
        chkServerJoinSmallest.Name = "chkServerJoinSmallest";
        chkServerJoinSmallest.Size = new Size(218, 30);
        chkServerJoinSmallest.TabIndex = 20;
        chkServerJoinSmallest.Text = "Always join smallest team";
        tltDescription.SetToolTip(chkServerJoinSmallest, "Forces joining people on the smallest team when checked");
        chkServerJoinSmallest.UseVisualStyleBackColor = false;
        //
        // txtServerTimelimit
        //
        txtServerTimelimit.Location = new Point(134, 133);
        txtServerTimelimit.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        txtServerTimelimit.Name = "txtServerTimelimit";
        txtServerTimelimit.Size = new Size(96, 23);
        txtServerTimelimit.TabIndex = 19;
        txtServerTimelimit.TextAlign = HorizontalAlignment.Center;
        tltDescription.SetToolTip(txtServerTimelimit, "Maximum duration of the game in minutes");
        txtServerTimelimit.Value = new decimal(new int[] { 15, 0, 0, 0 });
        //
        // txtServerFraglimit
        //
        txtServerFraglimit.Location = new Point(134, 98);
        txtServerFraglimit.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        txtServerFraglimit.Name = "txtServerFraglimit";
        txtServerFraglimit.Size = new Size(96, 23);
        txtServerFraglimit.TabIndex = 18;
        txtServerFraglimit.TextAlign = HorizontalAlignment.Center;
        tltDescription.SetToolTip(txtServerFraglimit, "Frags or captures required to end the game");
        txtServerFraglimit.Value = new decimal(new int[] { 20, 0, 0, 0 });
        //
        // txtServerPlayers
        //
        txtServerPlayers.Location = new Point(134, 64);
        txtServerPlayers.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        txtServerPlayers.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        txtServerPlayers.Name = "txtServerPlayers";
        txtServerPlayers.Size = new Size(96, 23);
        txtServerPlayers.TabIndex = 17;
        txtServerPlayers.TextAlign = HorizontalAlignment.Center;
        tltDescription.SetToolTip(txtServerPlayers, "Maximum number of players allowed");
        txtServerPlayers.Value = new decimal(new int[] { 10, 0, 0, 0 });
        //
        // txtServerClients
        //
        txtServerClients.Location = new Point(134, 30);
        txtServerClients.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        txtServerClients.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        txtServerClients.Name = "txtServerClients";
        txtServerClients.Size = new Size(96, 23);
        txtServerClients.TabIndex = 16;
        txtServerClients.TextAlign = HorizontalAlignment.Center;
        tltDescription.SetToolTip(txtServerClients, "Maximum number of clients allowed on server");
        txtServerClients.Value = new decimal(new int[] { 10, 0, 0, 0 });
        //
        // lblTimelimit
        //
        lblTimelimit.BackColor = Color.Transparent;
        lblTimelimit.Location = new Point(17, 133);
        lblTimelimit.Name = "lblTimelimit";
        lblTimelimit.Size = new Size(112, 20);
        lblTimelimit.TabIndex = 15;
        lblTimelimit.Text = "Time limit:";
        lblTimelimit.TextAlign = ContentAlignment.BottomRight;
        lblTimelimit.UseMnemonic = false;
        //
        // lblFraglimit
        //
        lblFraglimit.BackColor = Color.Transparent;
        lblFraglimit.Location = new Point(17, 98);
        lblFraglimit.Name = "lblFraglimit";
        lblFraglimit.Size = new Size(112, 20);
        lblFraglimit.TabIndex = 14;
        lblFraglimit.Text = "Score limit:";
        lblFraglimit.TextAlign = ContentAlignment.BottomRight;
        lblFraglimit.UseMnemonic = false;
        //
        // lblPlayers
        //
        lblPlayers.BackColor = Color.Transparent;
        lblPlayers.Location = new Point(17, 64);
        lblPlayers.Name = "lblPlayers";
        lblPlayers.Size = new Size(112, 20);
        lblPlayers.TabIndex = 13;
        lblPlayers.Text = "Max players:";
        lblPlayers.TextAlign = ContentAlignment.BottomRight;
        lblPlayers.UseMnemonic = false;
        //
        // lblClients
        //
        lblClients.BackColor = Color.Transparent;
        lblClients.Location = new Point(17, 30);
        lblClients.Name = "lblClients";
        lblClients.Size = new Size(112, 19);
        lblClients.TabIndex = 12;
        lblClients.Text = "Max clients:";
        lblClients.TextAlign = ContentAlignment.BottomRight;
        lblClients.UseMnemonic = false;
        //
        // grpGeneral
        //
        grpGeneral.Controls.Add(chkServerAddToMaster);
        grpGeneral.Controls.Add(txtServerPassword);
        grpGeneral.Controls.Add(label4);
        grpGeneral.Controls.Add(cmbServerType);
        grpGeneral.Controls.Add(label3);
        grpGeneral.Controls.Add(txtServerWebsite);
        grpGeneral.Controls.Add(label2);
        grpGeneral.Controls.Add(txtServerTitle);
        grpGeneral.Controls.Add(label1);
        grpGeneral.Controls.Add(chkServerDedicated);
        grpGeneral.Dock = DockStyle.Top;
        grpGeneral.Location = new Point(6, 6);
        grpGeneral.Name = "grpGeneral";
        grpGeneral.Size = new Size(931, 113);
        grpGeneral.TabIndex = 13;
        grpGeneral.TabStop = false;
        grpGeneral.Text = " General ";
        //
        // chkServerAddToMaster
        //
        chkServerAddToMaster.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkServerAddToMaster.FlatStyle = FlatStyle.System;
        chkServerAddToMaster.Location = new Point(713, 69);
        chkServerAddToMaster.Name = "chkServerAddToMaster";
        chkServerAddToMaster.Size = new Size(196, 29);
        chkServerAddToMaster.TabIndex = 20;
        chkServerAddToMaster.Text = "Show in public list";
        tltDescription.SetToolTip(chkServerAddToMaster, "Shows your server on the internet so that people can find it");
        //
        // txtServerPassword
        //
        txtServerPassword.Location = new Point(403, 69);
        txtServerPassword.MaxLength = 50;
        txtServerPassword.Name = "txtServerPassword";
        txtServerPassword.Size = new Size(202, 23);
        txtServerPassword.TabIndex = 18;
        tltDescription.SetToolTip(txtServerPassword, "Password to lock the server");
        //
        // label4
        //
        label4.BackColor = Color.Transparent;
        label4.Location = new Point(314, 69);
        label4.Name = "label4";
        label4.Size = new Size(84, 25);
        label4.TabIndex = 17;
        label4.Text = "Password:";
        label4.TextAlign = ContentAlignment.MiddleRight;
        label4.UseMnemonic = false;
        //
        // cmbServerType
        //
        cmbServerType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbServerType.Items.AddRange(new object[] { "Deathmatch (DM)", "Team Deathmatch (TDM)", "Capture The Flag (CTF)", "Scavenger (SC)", "Team Scavenger (TSC)" });
        cmbServerType.Location = new Point(101, 69);
        cmbServerType.MaxDropDownItems = 12;
        cmbServerType.Name = "cmbServerType";
        cmbServerType.Size = new Size(196, 24);
        cmbServerType.TabIndex = 16;
        tltDescription.SetToolTip(cmbServerType, "Type of game to play");
        cmbServerType.SelectedIndexChanged += cmbServerType_SelectedIndexChanged;
        //
        // label3
        //
        label3.BackColor = Color.Transparent;
        label3.Location = new Point(6, 69);
        label3.Name = "label3";
        label3.Size = new Size(89, 25);
        label3.TabIndex = 15;
        label3.Text = "Game type:";
        label3.TextAlign = ContentAlignment.MiddleRight;
        label3.UseMnemonic = false;
        //
        // txtServerWebsite
        //
        txtServerWebsite.Location = new Point(403, 30);
        txtServerWebsite.MaxLength = 300;
        txtServerWebsite.Name = "txtServerWebsite";
        txtServerWebsite.Size = new Size(202, 23);
        txtServerWebsite.TabIndex = 14;
        tltDescription.SetToolTip(txtServerWebsite, "Complete URL of the website related to this game server");
        //
        // label2
        //
        label2.BackColor = Color.Transparent;
        label2.Location = new Point(325, 30);
        label2.Name = "label2";
        label2.Size = new Size(73, 24);
        label2.TabIndex = 13;
        label2.Text = "Website:";
        label2.TextAlign = ContentAlignment.MiddleRight;
        label2.UseMnemonic = false;
        //
        // txtServerTitle
        //
        txtServerTitle.Location = new Point(101, 30);
        txtServerTitle.MaxLength = 200;
        txtServerTitle.Name = "txtServerTitle";
        txtServerTitle.Size = new Size(196, 23);
        txtServerTitle.TabIndex = 12;
        tltDescription.SetToolTip(txtServerTitle, "Title of the game to display in the servers list");
        //
        // label1
        //
        label1.BackColor = Color.Transparent;
        label1.Location = new Point(6, 30);
        label1.Name = "label1";
        label1.Size = new Size(89, 24);
        label1.TabIndex = 11;
        label1.Text = "Title:";
        label1.TextAlign = ContentAlignment.MiddleRight;
        label1.UseMnemonic = false;
        //
        // chkServerDedicated
        //
        chkServerDedicated.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chkServerDedicated.FlatStyle = FlatStyle.System;
        chkServerDedicated.Location = new Point(713, 30);
        chkServerDedicated.Name = "chkServerDedicated";
        chkServerDedicated.Size = new Size(174, 29);
        chkServerDedicated.TabIndex = 19;
        chkServerDedicated.Text = "Dedicated server";
        tltDescription.SetToolTip(chkServerDedicated, "Only a dedicated server will be started when checked");
        //
        // btnOptionsJoinGame
        //
        btnOptionsJoinGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnOptionsJoinGame.DialogResult = DialogResult.Cancel;
        btnOptionsJoinGame.FlatStyle = FlatStyle.System;
        btnOptionsJoinGame.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnOptionsJoinGame.Location = new Point(618, 479);
        btnOptionsJoinGame.Name = "btnOptionsJoinGame";
        btnOptionsJoinGame.Size = new Size(145, 34);
        btnOptionsJoinGame.TabIndex = 3;
        btnOptionsJoinGame.Text = "Options...";
        tltDescription.SetToolTip(btnOptionsJoinGame, "Displays the options configuration dialog");
        btnOptionsJoinGame.Click += btnOptions_Click;
        //
        // btnExitJoinGame
        //
        btnExitJoinGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExitJoinGame.DialogResult = DialogResult.Cancel;
        btnExitJoinGame.FlatStyle = FlatStyle.System;
        btnExitJoinGame.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnExitJoinGame.Location = new Point(769, 479);
        btnExitJoinGame.Name = "btnExitJoinGame";
        btnExitJoinGame.Size = new Size(146, 34);
        btnExitJoinGame.TabIndex = 2;
        btnExitJoinGame.Text = "Exit";
        tltDescription.SetToolTip(btnExitJoinGame, "Click this and the devil will take your soul");
        btnExitJoinGame.Click += btnExit_Click;
        //
        // btnOptionsHostGame
        //
        btnOptionsHostGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnOptionsHostGame.DialogResult = DialogResult.Cancel;
        btnOptionsHostGame.FlatStyle = FlatStyle.System;
        btnOptionsHostGame.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnOptionsHostGame.Location = new Point(630, 479);
        btnOptionsHostGame.Name = "btnOptionsHostGame";
        btnOptionsHostGame.Size = new Size(145, 34);
        btnOptionsHostGame.TabIndex = 3;
        btnOptionsHostGame.Text = "Options...";
        tltDescription.SetToolTip(btnOptionsHostGame, "Displays the options configuration dialog");
        btnOptionsHostGame.Click += btnOptions_Click;
        //
        // btnExitHostGame
        //
        btnExitHostGame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnExitHostGame.DialogResult = DialogResult.Cancel;
        btnExitHostGame.FlatStyle = FlatStyle.System;
        btnExitHostGame.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        btnExitHostGame.Location = new Point(781, 479);
        btnExitHostGame.Name = "btnExitHostGame";
        btnExitHostGame.Size = new Size(146, 34);
        btnExitHostGame.TabIndex = 2;
        btnExitHostGame.Text = "Exit";
        tltDescription.SetToolTip(btnExitHostGame, "Click this and the devil will take your soul");
        btnExitHostGame.Click += btnExit_Click;
        //
        // mnuGame
        //
        mnuGame.ImageScalingSize = new Size(20, 20);
        mnuGame.Items.AddRange(new ToolStripItem[] { itmJoinGame1, itmGameDetails, menuItem3, itmGameWebsite, menuItem7, itmCopyGameTitle, itmCopyGameDetails, itmCopyGameIP });
        mnuGame.Name = "mnuGame";
        mnuGame.Size = new Size(238, 196);
        //
        // itmJoinGame1
        //
        itmJoinGame1.Name = "itmJoinGame1";
        itmJoinGame1.Size = new Size(237, 24);
        itmJoinGame1.Text = "Join Game";
        itmJoinGame1.Click += btnJoinGame1_Click;
        //
        // itmGameDetails
        //
        itmGameDetails.Name = "itmGameDetails";
        itmGameDetails.Size = new Size(237, 24);
        itmGameDetails.Text = "Show Game Details...";
        itmGameDetails.Click += btnGameDetails_Click;
        //
        // menuItem3
        //
        menuItem3.Name = "menuItem3";
        menuItem3.Size = new Size(237, 24);
        menuItem3.Text = "-";
        //
        // itmGameWebsite
        //
        itmGameWebsite.Name = "itmGameWebsite";
        itmGameWebsite.Size = new Size(237, 24);
        itmGameWebsite.Text = "Browse Website";
        itmGameWebsite.Click += itmGameWebsite_Click;
        //
        // menuItem7
        //
        menuItem7.Name = "menuItem7";
        menuItem7.Size = new Size(237, 24);
        menuItem7.Text = "-";
        //
        // itmCopyGameTitle
        //
        itmCopyGameTitle.Name = "itmCopyGameTitle";
        itmCopyGameTitle.Size = new Size(237, 24);
        itmCopyGameTitle.Text = "Copy Game Title";
        itmCopyGameTitle.Click += itmCopyGameTitle_Click;
        //
        // itmCopyGameDetails
        //
        itmCopyGameDetails.Name = "itmCopyGameDetails";
        itmCopyGameDetails.Size = new Size(237, 24);
        itmCopyGameDetails.Text = "Copy Game Information";
        itmCopyGameDetails.Click += itmCopyGameDetails_Click;
        //
        // itmCopyGameIP
        //
        itmCopyGameIP.Name = "itmCopyGameIP";
        itmCopyGameIP.Size = new Size(237, 24);
        itmCopyGameIP.Text = "Copy Game Address";
        itmCopyGameIP.Click += itmCopyGameIP_Click;
        //
        // tltDescription
        //
        tltDescription.AutoPopDelay = 6000;
        tltDescription.InitialDelay = 300;
        tltDescription.ReshowDelay = 50;
        //
        // stbStatus
        //
        stbStatus.Font = new Font("Arial", 8.25F, FontStyle.Bold, GraphicsUnit.Point);
        stbStatus.ImageScalingSize = new Size(20, 20);
        stbStatus.Items.AddRange(new ToolStripItem[] { stpPanel, stpVersion });
        stbStatus.Location = new Point(0, 669);
        stbStatus.Name = "stbStatus";
        stbStatus.Size = new Size(951, 22);
        stbStatus.TabIndex = 4;
        //
        // stpPanel
        //
        stpPanel.Name = "stpPanel";
        stpPanel.Size = new Size(57, 16);
        stpPanel.Text = " Ready.";
        //
        // stpVersion
        //
        stpVersion.Name = "stpVersion";
        stpVersion.Size = new Size(125, 16);
        stpVersion.Text = " version 0.0.0000";
        //
        // dlgSaveConfig
        //
        dlgSaveConfig.DefaultExt = "cfg";
        dlgSaveConfig.Filter = "Configurations   *.cfg|*.cfg|All files|*.*";
        dlgSaveConfig.Title = "Export Configuration";
        //
        // tmrUpdateList
        //
        tmrUpdateList.Interval = 200;
        tmrUpdateList.Tick += tmrUpdateList_Tick;
        //
        // pnlIcon
        //
        pnlIcon.Controls.Add(picLogo);
        pnlIcon.Dock = DockStyle.Top;
        pnlIcon.Location = new Point(0, 0);
        pnlIcon.Name = "pnlIcon";
        pnlIcon.Padding = new Padding(6);
        pnlIcon.Size = new Size(951, 106);
        pnlIcon.TabIndex = 5;
        //
        // pnlTabs
        //
        pnlTabs.Controls.Add(tabsMode);
        pnlTabs.Dock = DockStyle.Fill;
        pnlTabs.Location = new Point(0, 106);
        pnlTabs.Name = "pnlTabs";
        pnlTabs.Padding = new Padding(6, 0, 6, 0);
        pnlTabs.Size = new Size(951, 563);
        pnlTabs.TabIndex = 6;
        //
        // FormMain
        //
        AutoScaleDimensions = new SizeF(7F, 16F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnExitJoinGame;
        ClientSize = new Size(951, 691);
        Controls.Add(pnlTabs);
        Controls.Add(pnlIcon);
        Controls.Add(stbStatus);
        Font = new Font("Arial", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        Icon = (Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        MinimumSize = new Size(969, 738);
        Name = "FormMain";
        SizeGripStyle = SizeGripStyle.Hide;
        StartPosition = FormStartPosition.Manual;
        Text = "Bloodmasters Launcher";
        Closing += FormMain_Closing;
        KeyDown += FormMain_KeyDown;
        Resize += FormMain_Resize;
        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        tabsMode.ResumeLayout(false);
        tabFind.ResumeLayout(false);
        tabFind.PerformLayout();
        tabHost.ResumeLayout(false);
        pnlHostGame.ResumeLayout(false);
        grpMaps.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picMapPreview).EndInit();
        grpSettings.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)txtServerPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)txtServerTimelimit).EndInit();
        ((System.ComponentModel.ISupportInitialize)txtServerFraglimit).EndInit();
        ((System.ComponentModel.ISupportInitialize)txtServerPlayers).EndInit();
        ((System.ComponentModel.ISupportInitialize)txtServerClients).EndInit();
        grpGeneral.ResumeLayout(false);
        grpGeneral.PerformLayout();
        mnuGame.ResumeLayout(false);
        stbStatus.ResumeLayout(false);
        stbStatus.PerformLayout();
        pnlIcon.ResumeLayout(false);
        pnlTabs.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion

    // This refreshes the maps lists
    public void RefreshMapsLists()
    {
        // Clear the combo
        cmbMap.Items.Clear();

        // Go for all .wad files
        List<string> wads = ArchiveManager.FindAllFiles(".wad");
        foreach (string wf in wads)
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
        foreach (string mn in cmbMap.Items)
        {
            // Check if the name matches
            if (string.Compare(mn, mapname, true) == 0) return true;
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
        List<string> wads = ArchiveManager.FindAllFiles(".wad");
        foreach (string wf in wads)
        {
            try
            {
                // Make short map name
                string[] wfparts = wf.Split('/');
                string mname = wfparts[1].Substring(0, wfparts[1].Length - 4);

                // Load the map information
                LevelMap.Map wadmap = new LauncherMap(mname, true, Paths.Instance.TempDir);

                // Check if game type is supported
                if (((cmbServerType.SelectedIndex == 0) && wadmap.SupportsDM) ||
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
            catch (Exception) { }
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
        if (stpPanel.Text != description)
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
            Archive flagsarchive = ArchiveManager.GetArchive("flags.zip");
            temppath = ArchiveManager.GetArchiveTempPath(flagsarchive);
            flagsarchive.ExtractAllFiles(temppath);

            // Load all flags icons
            files = Directory.GetFiles(temppath);
            foreach (string f in files)
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
        catch (Exception)
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
        if (flags.TryGetValue(filename, out int flag))
        {
            // Return country flag
            return flag;
        }
        else
        {
            // Return unknown flag
            return flags["_0.ico"];
            //return 0;
        }
    }

    // This returns a flag icon image
    public Image GetFlagIcon(char[] countrycode)
    {
        return imglstFlags.Images[GetFlagIconIndex(countrycode)];
    }

    // This refreshes the list
    public void RefreshGamesList()
    {
        // Mousecursor
        this.Cursor = Cursors.WaitCursor;

        // Start new queries
        ShowStatus("Requesting list of servers...");
        string result = browser.StartNewQuery();
        if (result != "") ShowStatus("Ready.  (" + result + ")"); else ShowStatus("Ready.");

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
        Program.SaveConfiguration();

        // Make arguments
        Configuration args = new Configuration();
        args.WriteSetting("join", addr + ":" + port);
        args.WriteSetting("password", password);

        // Start the game
        Program.LaunchBloodmasters(args, "");

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
        if (pass.ShowDialog(this) == DialogResult.OK)
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
        if (e.KeyCode == Keys.F5)
        {
            // Check if Join Game tab is open
            if (tabsMode.SelectedIndex == 0)
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
        switch (tabsMode.SelectedIndex)
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
        Program.SaveConfiguration();

        // Make the server configuration file
        string scfgfile = Program.MakeUniqueFilename(Paths.Instance.ConfigDirPath, "server_", ".cfg");
        Configuration scfg = MakeServerConfig(true);
        scfg.SaveConfiguration(scfgfile);

        // Make arguments
        Configuration args = new Configuration();
        if (chkServerDedicated.Checked == false)
            args.WriteSetting("host", scfgfile);
        else
            args.WriteSetting("dedicated", scfgfile);

        // Start the game
        Program.LaunchBloodmasters(args, scfgfile);
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
        if (includercon) cfg.WriteSetting("rconpassword", Program.RandomString(20));

        // Map names
        ListDictionary maps = new ListDictionary();
        foreach (string mname in lstMaps.CheckedItems) maps.Add(mname, null);
        cfg.WriteSetting("maps", maps);

        return cfg;
    }

    // Logo clicked
    private void picLogo_Click(object sender, System.EventArgs e)
    {
        // Open website
        this.Cursor = Cursors.WaitCursor;
        Program.OpenWebsite("http://www.bloodmasters.com/");
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
        switch ((GAMETYPE)cmbServerType.SelectedIndex)
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
        if (lstMaps.SelectedIndex > -1)
        {
            // Clear previous image
            if (picMapPreview.Image != null) picMapPreview.Image.Dispose();
            picMapPreview.Image = null;

            // Find the preview image
            string bmpfilename = lstMaps.SelectedItem + ".bmp";
            string bmparchive = ArchiveManager.FindFileArchive(bmpfilename);
            if (bmparchive != "")
            {
                try
                {
                    // Extract the .bmp file
                    string bmptempfile = ArchiveManager.ExtractFile(bmparchive + "/" + bmpfilename);

                    // Display map image
                    picMapPreview.Image = Image.FromFile(bmptempfile);
                }
                catch (Exception)
                {
                    // Unable to load map image, clear it
                    if (picMapPreview.Image != null) picMapPreview.Image.Dispose();
                    picMapPreview.Image = null;
                }
            }

            try
            {
                // Load the map information
                LevelMap.Map wadmap = new LauncherMap(lstMaps.SelectedItem.ToString(), true, Paths.Instance.TempDir);

                // Display map information
                lblMapTitle.Text = wadmap.Title;
                lblMapAuthor.Text = wadmap.Author;
                lblMapPlayers.Text = "~ " + wadmap.RecommendedPlayers + " recommended";
            }
            catch (Exception)
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
            if (picMapPreview.Image != null) picMapPreview.Image.Dispose();
            picMapPreview.Image = null;
            lblMapTitle.Text = "";
            lblMapAuthor.Text = "";
            lblMapPlayers.Text = "";
        }
    }

    private void lstMaps_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        btnHostGame.Enabled = lstMaps.CheckedItems.Count switch
        {
            // Only 1 map selected, and it is going to be unchecked
            1 when e.NewValue == CheckState.Unchecked => false,
            _ => true
        };
    }

    // Window closes
    private void FormMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Make list of maps
        ListDictionary maplist = new ListDictionary();
        foreach (string mapname in lstMaps.CheckedItems) if (!maplist.Contains(mapname)) maplist.Add(mapname, null);

        // Save the interface settings to configuration
        if (this.WindowState == FormWindowState.Normal)
        {
            Program.config.WriteSetting("windowleft", this.Location.X);
            Program.config.WriteSetting("windowtop", this.Location.Y);
        }
        Program.config.WriteSetting("servertitle", txtServerTitle.Text);
        Program.config.WriteSetting("serverwebsite", txtServerWebsite.Text);
        Program.config.WriteSetting("serverpassword", txtServerPassword.Text);
        Program.config.WriteSetting("servertype", cmbServerType.SelectedIndex);
        Program.config.WriteSetting("serverdedicated", chkServerDedicated.Checked);
        Program.config.WriteSetting("serverpublic", chkServerAddToMaster.Checked);
        Program.config.WriteSetting("serverclients", (int)txtServerClients.Value);
        Program.config.WriteSetting("serverplayers", (int)txtServerPlayers.Value);
        Program.config.WriteSetting("serverscorelimit", (int)txtServerFraglimit.Value);
        Program.config.WriteSetting("servertimelimit", (int)txtServerTimelimit.Value);
        Program.config.WriteSetting("serverjoinsmallest", chkServerJoinSmallest.Checked);
        Program.config.WriteSetting("serverport", (int)txtServerPort.Value);
        Program.config.WriteSetting("servermaps", maplist);
        Program.config.WriteSetting("filtertitle", txtFilterTitles.Text);
        Program.config.WriteSetting("filterfull", chkShowFull.Checked);
        Program.config.WriteSetting("filterempty", chkShowEmpty.Checked);
        Program.config.WriteSetting("filtertype", cmbType.SelectedIndex);
        Program.config.WriteSetting("filtermap", cmbMap.Text);
    }

    // Window is resized
    private void FormMain_Resize(object sender, System.EventArgs e)
    {
        if (!Created) return;

        // When in normal state
        if (this.WindowState == FormWindowState.Normal)
        {
            // Store window size and location
            Program.config.WriteSetting("windowleft", this.Location.X);
            Program.config.WriteSetting("windowtop", this.Location.Y);
            Program.config.WriteSetting("windowwidth", this.Size.Width);
            Program.config.WriteSetting("windowheight", this.Size.Height);
        }

        // Store window state
        if (this.WindowState != FormWindowState.Minimized)
            Program.config.WriteSetting("windowstate", (int)this.WindowState);
        else
            Program.config.WriteSetting("windowstate", (int)FormWindowState.Normal);
    }

    // Export configuration clicked
    private void btnExportConfig_Click(object sender, System.EventArgs e)
    {
        // Show export dialog
        DialogResult result = dlgSaveConfig.ShowDialog(this);
        if (result == DialogResult.OK)
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
        if (lstGames.SelectedItems.Count > 0)
        {
            // Get the selected item
            ListViewItem item = lstGames.SelectedItems[0];
            GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

            // Check if the map is missing
            if (!CheckMapExists(gitem.MapName))
            {
                // Auto-download?
                if (Program.config.ReadSetting("autodownload", true))
                {
                    // Server URL valid?
                    if (gitem.Website.ToLower().StartsWith("http://"))
                    {
                        // Show download dialog
                        FormDownload download = new FormDownload(gitem);
                        download.ShowDialog(this);
                        download.Dispose();

                        // Check if new file exists
                        filename = Path.Combine(Paths.DownloadedResourceDir, gitem.MapName + ".zip");
                        if (File.Exists(filename))
                        {
                            // Busy!
                            this.Cursor = Cursors.WaitCursor;
                            this.Update();

                            // Open the map archive
                            try { ArchiveManager.OpenArchive(filename); }
                            catch (Exception) { MessageBox.Show(this, "Unable to open the archive file " + gitem.MapName + ".zip. The file is not in the correct format.", "Downloading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

                            // Refresh maps lists
                            RefreshMapsLists();

                            // Go for all items to see if they need updating
                            for (int i = lstGames.Items.Count - 1; i >= 0; i--)
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
            if (CheckMapExists(gitem.MapName))
            {
                // Game locked?
                if (gitem.Locked)
                {
                    // Ask for password input
                    if (!AskPassword(out pass)) return;
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
        if (btnJoinGame1.Enabled) btnJoinGame1_Click(sender, e);
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
        if (updatelist)
        {
            // Dont refresh the list
            Program.LockWindowUpdate(lstGames.Handle);
            lstGames.BeginUpdate();

            // Get the whole list
            Dictionary<string, GamesListItem> gitems = browser.GetFilteredList();

            // Go for all items to see if they need updating
            for (int i = lstGames.Items.Count - 1; i >= 0; i--)
            {
                // Get the item
                ListViewItem item = lstGames.Items[i];

                // Item in the new collection?
                if (gitems.TryGetValue(item.SubItems[9].Text, out GamesListItem gi))
                {
                    // Update the item ifchanged
                    if (gi.Changed) gi.UpdateListViewItem(item);

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
            foreach (GamesListItem gi in gitems.Values)
            {
                // Add to the list
                gi.NewListViewItem(lstGames);
            }

            // Redraw the list
            lstGames.EndUpdate();
            Program.LockWindowUpdate(IntPtr.Zero);
            updatelist = false;
        }
    }

    // Game Details clicked
    private void btnGameDetails_Click(object sender, System.EventArgs e)
    {
        string pass = "";
        bool dojoin;

        // Anything selected?
        if (lstGames.SelectedItems.Count > 0)
        {
            // Get the selected item
            ListViewItem item = lstGames.SelectedItems[0];
            GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

            // Load and show details dialog
            FormGameInfo gameinfowindow = new FormGameInfo(gitem);
            if (gameinfowindow.ShowDialog(this) == DialogResult.OK)
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
        if (e.Button == MouseButtons.Right)
        {
            // Anything selected?
            if (lstGames.SelectedItems.Count > 0)
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
        if (lstGames.SelectedItems.Count > 0)
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
        if (lstGames.SelectedItems.Count > 0)
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
        if (lstGames.SelectedItems.Count > 0)
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
        if ((index > -1) && (index < lstMaps.Items.Count))
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
        if (specify.ShowDialog(this) == DialogResult.OK)
        {
            // Show status
            ShowStatus("Resolving address...");

            // Stop queries
            browser.StopQuery();

            // Try to resolve the address
            try { ip = Dns.Resolve(specify.txtJoinAddress.Text); } catch (Exception) { }
            if ((ip == null) || (ip.AddressList.Length == 0))
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
        if (lstGames.SelectedItems.Count > 0)
        {
            // Get the selected item
            ListViewItem item = lstGames.SelectedItems[0];
            GamesListItem gitem = browser.GetItemByAddress(item.SubItems[9].Text);

            // The website MUST start with http://
            if (gitem.Website.ToLower().StartsWith("http://"))
            {
                // Open website
                this.Cursor = Cursors.WaitCursor;
                Program.OpenWebsite(gitem.Website);
                this.Cursor = Cursors.Default;
            }
        }
    }

    // Column clicked in games list
    private void lstGames_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
        int subitemindex = 0;
        bool ascending = Program.config.ReadSetting("sortascending", true);

        // Determine subitem to sort by for this column
        switch (e.Column)
        {
            case 0: subitemindex = 0; break;    // Title
            case 1: subitemindex = 6; break;    // Ping
            case 2: subitemindex = 7; break;    // Players
            case 3: subitemindex = 8; break;    // Clients
            case 4: subitemindex = 4; break;    // Game Type
            case 5: subitemindex = 5; break;    // Map
        }

        // Already sorted by this subitem?
        if (Program.config.ReadSetting("sortitem", 6) == subitemindex)
        {
            // Change sort order
            ascending = !ascending;
        }

        // Make new sort comparer
        lstGames.ListViewItemSorter = new GamesListItemComparer(subitemindex, ascending);

        // Save settings
        Program.config.WriteSetting("sortitem", subitemindex);
        Program.config.WriteSetting("sortascending", ascending);
    }
}
