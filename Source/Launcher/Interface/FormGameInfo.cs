/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Net;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Launcher
{
	public class FormGameInfo : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblPlayers;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblMaxPlayers;
		private System.Windows.Forms.Label lblMaxClients;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblClients;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblFraglimit;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblTimelimit;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label lblGameType;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label lblMap;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label lblJoinSmallest;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView lstPlayers;
		private System.Windows.Forms.ColumnHeader clmPLayerName;
		private System.Windows.Forms.ColumnHeader clmPlayerTeam;
		private System.Windows.Forms.ColumnHeader clmPlayerSpect;
		private System.Windows.Forms.ColumnHeader clmPlayerPing;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.LinkLabel lblWebsite;
		private System.Windows.Forms.Panel pnlExtended;
		private System.Windows.Forms.Panel pnlProtocol;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label lblProtocol;
		private System.Windows.Forms.Panel pnlLocked;
		private System.Windows.Forms.Label lblLocked;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.CheckBox chkRefresh;
		private System.Windows.Forms.Timer tmrRefresh;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer tmrUpdate;
		private System.Windows.Forms.Button btnJoin;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblLocation;
		public System.Windows.Forms.PictureBox picLocation;
		private int lastrevision;
		private System.Windows.Forms.Label lblBuildDescription;
		private GamesListItem item;
		
		// Constructor
		public FormGameInfo(GamesListItem item)
		{
			// Required for Windows Form Designer support
			InitializeComponent();
			
			// Keep item
			this.item = item;
			UpdateSettings();
		}
		
		// Clean up any resources being used.
		protected override void Dispose(bool disposing)
		{
			// Destroy references
			item = null;
			
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormGameInfo));
			this.lblTitle = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lblPlayers = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lblMaxPlayers = new System.Windows.Forms.Label();
			this.lblMaxClients = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.lblClients = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.btnClose = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblFraglimit = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblTimelimit = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.lblGameType = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.lblMap = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.lblJoinSmallest = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lstPlayers = new System.Windows.Forms.ListView();
			this.clmPLayerName = new System.Windows.Forms.ColumnHeader();
			this.clmPlayerTeam = new System.Windows.Forms.ColumnHeader();
			this.clmPlayerSpect = new System.Windows.Forms.ColumnHeader();
			this.clmPlayerPing = new System.Windows.Forms.ColumnHeader();
			this.label12 = new System.Windows.Forms.Label();
			this.lblWebsite = new System.Windows.Forms.LinkLabel();
			this.pnlExtended = new System.Windows.Forms.Panel();
			this.pnlProtocol = new System.Windows.Forms.Panel();
			this.lblProtocol = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.pnlLocked = new System.Windows.Forms.Panel();
			this.lblLocked = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.chkRefresh = new System.Windows.Forms.CheckBox();
			this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
			this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
			this.btnJoin = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.lblLocation = new System.Windows.Forms.Label();
			this.picLocation = new System.Windows.Forms.PictureBox();
			this.lblBuildDescription = new System.Windows.Forms.Label();
			this.pnlExtended.SuspendLayout();
			this.pnlProtocol.SuspendLayout();
			this.pnlLocked.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.lblTitle.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(12, 12);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(356, 28);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Title";
			this.lblTitle.UseMnemonic = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(180, 148);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Players:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblPlayers
			// 
			this.lblPlayers.AutoSize = true;
			this.lblPlayers.Location = new System.Drawing.Point(228, 148);
			this.lblPlayers.Name = "lblPlayers";
			this.lblPlayers.Size = new System.Drawing.Size(10, 16);
			this.lblPlayers.TabIndex = 2;
			this.lblPlayers.Text = "0";
			this.lblPlayers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(256, 148);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Maximum:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblMaxPlayers
			// 
			this.lblMaxPlayers.AutoSize = true;
			this.lblMaxPlayers.Location = new System.Drawing.Point(312, 148);
			this.lblMaxPlayers.Name = "lblMaxPlayers";
			this.lblMaxPlayers.Size = new System.Drawing.Size(10, 16);
			this.lblMaxPlayers.TabIndex = 4;
			this.lblMaxPlayers.Text = "0";
			this.lblMaxPlayers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblMaxClients
			// 
			this.lblMaxClients.AutoSize = true;
			this.lblMaxClients.Location = new System.Drawing.Point(312, 124);
			this.lblMaxClients.Name = "lblMaxClients";
			this.lblMaxClients.Size = new System.Drawing.Size(10, 16);
			this.lblMaxClients.TabIndex = 8;
			this.lblMaxClients.Text = "0";
			this.lblMaxClients.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(256, 124);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 16);
			this.label4.TabIndex = 7;
			this.label4.Text = "Maximum:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblClients
			// 
			this.lblClients.AutoSize = true;
			this.lblClients.Location = new System.Drawing.Point(228, 124);
			this.lblClients.Name = "lblClients";
			this.lblClients.Size = new System.Drawing.Size(10, 16);
			this.lblClients.TabIndex = 6;
			this.lblClients.Text = "0";
			this.lblClients.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(180, 124);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 16);
			this.label6.TabIndex = 5;
			this.label6.Text = "Clients:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnClose.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnClose.Location = new System.Drawing.Point(264, 404);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(104, 28);
			this.btnClose.TabIndex = 1;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(12, 36);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(356, 7);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			// 
			// lblFraglimit
			// 
			this.lblFraglimit.AutoSize = true;
			this.lblFraglimit.Location = new System.Drawing.Point(60, 4);
			this.lblFraglimit.Name = "lblFraglimit";
			this.lblFraglimit.Size = new System.Drawing.Size(10, 16);
			this.lblFraglimit.TabIndex = 12;
			this.lblFraglimit.Text = "0";
			this.lblFraglimit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(0, 4);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(60, 16);
			this.label5.TabIndex = 11;
			this.label5.Text = "Scorelimit:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblTimelimit
			// 
			this.lblTimelimit.AutoSize = true;
			this.lblTimelimit.Location = new System.Drawing.Point(60, 28);
			this.lblTimelimit.Name = "lblTimelimit";
			this.lblTimelimit.Size = new System.Drawing.Size(10, 16);
			this.lblTimelimit.TabIndex = 14;
			this.lblTimelimit.Text = "0";
			this.lblTimelimit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(4, 28);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 13;
			this.label7.Text = "Timelimit:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblGameType
			// 
			this.lblGameType.AutoSize = true;
			this.lblGameType.Location = new System.Drawing.Point(68, 124);
			this.lblGameType.Name = "lblGameType";
			this.lblGameType.Size = new System.Drawing.Size(65, 16);
			this.lblGameType.TabIndex = 18;
			this.lblGameType.Text = "Deathmatch";
			this.lblGameType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(24, 124);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(44, 16);
			this.label9.TabIndex = 17;
			this.label9.Text = "Game:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblMap
			// 
			this.lblMap.AutoSize = true;
			this.lblMap.Location = new System.Drawing.Point(68, 148);
			this.lblMap.Name = "lblMap";
			this.lblMap.Size = new System.Drawing.Size(26, 16);
			this.lblMap.TabIndex = 20;
			this.lblMap.Text = "Test";
			this.lblMap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(24, 148);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(44, 16);
			this.label10.TabIndex = 19;
			this.label10.Text = "Map:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblJoinSmallest
			// 
			this.lblJoinSmallest.AutoSize = true;
			this.lblJoinSmallest.Location = new System.Drawing.Point(304, 28);
			this.lblJoinSmallest.Name = "lblJoinSmallest";
			this.lblJoinSmallest.Size = new System.Drawing.Size(10, 16);
			this.lblJoinSmallest.TabIndex = 24;
			this.lblJoinSmallest.Text = "0";
			this.lblJoinSmallest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(164, 28);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(140, 16);
			this.label11.TabIndex = 23;
			this.label11.Text = "Always join smallest team:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Location = new System.Drawing.Point(4, 56);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(356, 7);
			this.groupBox2.TabIndex = 25;
			this.groupBox2.TabStop = false;
			// 
			// lstPlayers
			// 
			this.lstPlayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstPlayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						 this.clmPLayerName,
																						 this.clmPlayerTeam,
																						 this.clmPlayerSpect,
																						 this.clmPlayerPing});
			this.lstPlayers.FullRowSelect = true;
			this.lstPlayers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstPlayers.Location = new System.Drawing.Point(4, 72);
			this.lstPlayers.Name = "lstPlayers";
			this.lstPlayers.Size = new System.Drawing.Size(356, 156);
			this.lstPlayers.TabIndex = 3;
			this.lstPlayers.View = System.Windows.Forms.View.Details;
			// 
			// clmPLayerName
			// 
			this.clmPLayerName.Text = "Name";
			this.clmPLayerName.Width = 124;
			// 
			// clmPlayerTeam
			// 
			this.clmPlayerTeam.Text = "Team";
			this.clmPlayerTeam.Width = 64;
			// 
			// clmPlayerSpect
			// 
			this.clmPlayerSpect.Text = "Mode";
			this.clmPlayerSpect.Width = 86;
			// 
			// clmPlayerPing
			// 
			this.clmPlayerPing.Text = "Ping";
			this.clmPlayerPing.Width = 50;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 76);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(52, 16);
			this.label12.TabIndex = 27;
			this.label12.Text = "Website:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblWebsite
			// 
			this.lblWebsite.ActiveLinkColor = System.Drawing.Color.Blue;
			this.lblWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblWebsite.AutoSize = true;
			this.lblWebsite.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblWebsite.DisabledLinkColor = System.Drawing.Color.Gray;
			this.lblWebsite.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.lblWebsite.LinkColor = System.Drawing.Color.Blue;
			this.lblWebsite.Location = new System.Drawing.Point(68, 76);
			this.lblWebsite.Name = "lblWebsite";
			this.lblWebsite.Size = new System.Drawing.Size(106, 16);
			this.lblWebsite.TabIndex = 2;
			this.lblWebsite.TabStop = true;
			this.lblWebsite.Text = "http://www.test.com/";
			this.lblWebsite.UseMnemonic = false;
			this.lblWebsite.VisitedLinkColor = System.Drawing.Color.Purple;
			this.lblWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWebsite_LinkClicked);
			// 
			// pnlExtended
			// 
			this.pnlExtended.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlExtended.Controls.Add(this.lblJoinSmallest);
			this.pnlExtended.Controls.Add(this.label11);
			this.pnlExtended.Controls.Add(this.groupBox2);
			this.pnlExtended.Controls.Add(this.lstPlayers);
			this.pnlExtended.Controls.Add(this.lblFraglimit);
			this.pnlExtended.Controls.Add(this.label5);
			this.pnlExtended.Controls.Add(this.lblTimelimit);
			this.pnlExtended.Controls.Add(this.label7);
			this.pnlExtended.Location = new System.Drawing.Point(8, 168);
			this.pnlExtended.Name = "pnlExtended";
			this.pnlExtended.Size = new System.Drawing.Size(364, 232);
			this.pnlExtended.TabIndex = 30;
			// 
			// pnlProtocol
			// 
			this.pnlProtocol.BackColor = System.Drawing.SystemColors.Info;
			this.pnlProtocol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlProtocol.Controls.Add(this.lblProtocol);
			this.pnlProtocol.Controls.Add(this.pictureBox1);
			this.pnlProtocol.Location = new System.Drawing.Point(12, 172);
			this.pnlProtocol.Name = "pnlProtocol";
			this.pnlProtocol.Size = new System.Drawing.Size(356, 24);
			this.pnlProtocol.TabIndex = 31;
			this.pnlProtocol.Visible = false;
			// 
			// lblProtocol
			// 
			this.lblProtocol.ForeColor = System.Drawing.SystemColors.InfoText;
			this.lblProtocol.Location = new System.Drawing.Point(24, 4);
			this.lblProtocol.Name = "lblProtocol";
			this.lblProtocol.Size = new System.Drawing.Size(336, 16);
			this.lblProtocol.TabIndex = 1;
			this.lblProtocol.Text = "Protocol error";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(24, 20);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// pnlLocked
			// 
			this.pnlLocked.BackColor = System.Drawing.SystemColors.Info;
			this.pnlLocked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlLocked.Controls.Add(this.lblLocked);
			this.pnlLocked.Controls.Add(this.pictureBox2);
			this.pnlLocked.Location = new System.Drawing.Point(12, 400);
			this.pnlLocked.Name = "pnlLocked";
			this.pnlLocked.Size = new System.Drawing.Size(356, 24);
			this.pnlLocked.TabIndex = 32;
			this.pnlLocked.Visible = false;
			// 
			// lblLocked
			// 
			this.lblLocked.ForeColor = System.Drawing.SystemColors.InfoText;
			this.lblLocked.Location = new System.Drawing.Point(24, 4);
			this.lblLocked.Name = "lblLocked";
			this.lblLocked.Size = new System.Drawing.Size(336, 16);
			this.lblLocked.TabIndex = 1;
			this.lblLocked.Text = "This server is locked with a password.";
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(0, 1);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(24, 20);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox2.TabIndex = 0;
			this.pictureBox2.TabStop = false;
			// 
			// chkRefresh
			// 
			this.chkRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkRefresh.Appearance = System.Windows.Forms.Appearance.Button;
			this.chkRefresh.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkRefresh.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkRefresh.Location = new System.Drawing.Point(12, 404);
			this.chkRefresh.Name = "chkRefresh";
			this.chkRefresh.Size = new System.Drawing.Size(104, 28);
			this.chkRefresh.TabIndex = 4;
			this.chkRefresh.Text = "Auto Refresh";
			this.chkRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.chkRefresh.CheckedChanged += new System.EventHandler(this.chkRefresh_CheckedChanged);
			// 
			// tmrRefresh
			// 
			this.tmrRefresh.Interval = 3000;
			this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
			// 
			// tmrUpdate
			// 
			this.tmrUpdate.Interval = 600;
			this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
			// 
			// btnJoin
			// 
			this.btnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnJoin.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnJoin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnJoin.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnJoin.Location = new System.Drawing.Point(156, 404);
			this.btnJoin.Name = "btnJoin";
			this.btnJoin.Size = new System.Drawing.Size(104, 28);
			this.btnJoin.TabIndex = 0;
			this.btnJoin.Text = "Join";
			this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 100);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 16);
			this.label3.TabIndex = 33;
			this.label3.Text = "Location:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblLocation
			// 
			this.lblLocation.AutoSize = true;
			this.lblLocation.Location = new System.Drawing.Point(68, 100);
			this.lblLocation.Name = "lblLocation";
			this.lblLocation.Size = new System.Drawing.Size(95, 16);
			this.lblLocation.TabIndex = 34;
			this.lblLocation.Text = "Location unknown";
			this.lblLocation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// picLocation
			// 
			this.picLocation.BackColor = System.Drawing.Color.Transparent;
			this.picLocation.Location = new System.Drawing.Point(70, 100);
			this.picLocation.Name = "picLocation";
			this.picLocation.Size = new System.Drawing.Size(16, 16);
			this.picLocation.TabIndex = 35;
			this.picLocation.TabStop = false;
			// 
			// lblBuildDescription
			// 
			this.lblBuildDescription.AutoSize = true;
			this.lblBuildDescription.Location = new System.Drawing.Point(16, 48);
			this.lblBuildDescription.Name = "lblBuildDescription";
			this.lblBuildDescription.Size = new System.Drawing.Size(163, 16);
			this.lblBuildDescription.TabIndex = 36;
			this.lblBuildDescription.Text = "Unknown server type or version";
			this.lblBuildDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormGameInfo
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(378, 443);
			this.Controls.Add(this.lblBuildDescription);
			this.Controls.Add(this.pnlProtocol);
			this.Controls.Add(this.lblLocation);
			this.Controls.Add(this.lblWebsite);
			this.Controls.Add(this.lblMap);
			this.Controls.Add(this.lblGameType);
			this.Controls.Add(this.lblMaxClients);
			this.Controls.Add(this.lblClients);
			this.Controls.Add(this.lblMaxPlayers);
			this.Controls.Add(this.lblPlayers);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnJoin);
			this.Controls.Add(this.chkRefresh);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.pnlExtended);
			this.Controls.Add(this.pnlLocked);
			this.Controls.Add(this.picLocation);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormGameInfo";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Game Details";
			this.pnlExtended.ResumeLayout(false);
			this.pnlProtocol.ResumeLayout(false);
			this.pnlLocked.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		
		// This updates the components to match with the game information
		public void UpdateSettings()
		{
			// Lookup country information
			IPRangeInfo cinfo = General.ip2country.LookupIP(item.Address.Address.ToString());
			
			// Setup interface with game item information
			lastrevision = item.Revision;
			lblTitle.Text = item.Title;
			lblWebsite.Text = item.Website;
			lblMap.Text = item.MapName;
			lblGameType.Text = General.GameTypeDescription(item.GameType);
			lblPlayers.Text = item.Players.ToString();
			lblClients.Text = item.Clients.ToString();
			lblMaxPlayers.Text = item.MaxPlayers.ToString();
			lblMaxClients.Text = item.MaxClients.ToString();
			pnlLocked.Visible = item.Locked;
			if(item.Locked && (pnlLocked.Bottom > btnClose.Top)) this.Height += pnlLocked.Height;
			
			// Set flag icon and location
			//try { picLocation.Image = formmain.GetFlagIcon(cinfo.ccode1); } catch(Exception) { }
			picLocation.Image = General.mainwindow.GetFlagIcon(cinfo.ccode1);
			if(picLocation.Image != null) lblLocation.Left = picLocation.Right + 2;
			lblLocation.Text = cinfo.country;
			
			// Check if protocol matches
			if(item.Protocol == Gateway.PROTOCOL_VERSION)
			{
				// Show extended information
				lblFraglimit.Text = item.Fraglimit.ToString();
				lblTimelimit.Text = item.Timelimit.ToString();
				lblJoinSmallest.Text = YesNo(item.JoinSmallest);
				
				// Go for player information
				lstPlayers.Items.Clear();
				for(int i = 0; i < item.Clients; i++)
				{
					// Make player item
					ListViewItem itm = new ListViewItem(item.PlayerName[i]);
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(item.PlayerTeam[i].ToString());
					if(item.PlayerSpectator[i]) itm.SubItems.Add("Spectating");
					else itm.SubItems.Add("Playing");
					itm.SubItems.Add(item.PlayerPing[i] + "ms");
					itm.SubItems[3].ForeColor = GamesListItem.MakePingColor(item.PlayerPing[i]);
					
					// Add the item to list
					lstPlayers.Items.Add(itm);
				}
				
				// Show build description
				if(item.BuildDescription != null) lblBuildDescription.Text = item.BuildDescription;
			}
			// Older protocol?
			else if(item.Protocol < Gateway.PROTOCOL_VERSION)
			{
				// Show error
				pnlExtended.Visible = false;
				pnlProtocol.Visible = true;
				lblProtocol.Text = "This server is running an outdated version of Bloodmasters.";
			}
			// Nieuwer protocol?
			else
			{
				// Show error
				pnlExtended.Visible = false;
				pnlProtocol.Visible = true;
				lblProtocol.Text = "This server is running a newer version of Bloodmasters.";
			}
		}
		
		// This returns Yes for true, No for false
		private string YesNo(bool opt)
		{
			if(opt) return "Yes"; else return "No";
		}
		
		// Website link clicked
		private void lblWebsite_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// The website MUST start with http://
			if(lblWebsite.Text.ToLower().StartsWith("http://"))
			{
				// Open website
				this.Cursor = Cursors.WaitCursor;
				General.OpenWebsite(lblWebsite.Text);
				this.Cursor = Cursors.Default;
			}
		}
		
		// Auto refresh changed
		private void chkRefresh_CheckedChanged(object sender, System.EventArgs e)
		{
			// Enable/disable auto refresh
			tmrRefresh.Enabled = chkRefresh.Checked;
			tmrUpdate.Enabled = chkRefresh.Checked;
		}
		
		// Close dialog
		private void btnClose_Click(object sender, System.EventArgs e)
		{
			// Stop auto refresh
			tmrRefresh.Enabled = false;
			tmrUpdate.Enabled = false;
		}
		
		// Refresh time
		private void tmrRefresh_Tick(object sender, System.EventArgs e)
		{
			// Refresh now
			item.Refresh();
		}
		
		// Check for item updates
		private void tmrUpdate_Tick(object sender, System.EventArgs e)
		{
			// Item changed?
			if(item.Revision > lastrevision) UpdateSettings();
		}
		
		// Join clicked
		private void btnJoin_Click(object sender, System.EventArgs e)
		{
			// Stop auto refresh
			tmrRefresh.Enabled = false;
			tmrUpdate.Enabled = false;
			
			// Close with OK as dialog result
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
