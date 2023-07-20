/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class FormServer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.ComponentModel.Container components = null;
		
		// Constructor
		public FormServer()
		{
			// Required for Windows Form Designer support
			InitializeComponent();
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
			this.lblInfo = new System.Windows.Forms.Label();
			this.rtbConsole = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// lblInfo
			// 
			this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblInfo.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblInfo.Location = new System.Drawing.Point(4, 4);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(552, 32);
			this.lblInfo.TabIndex = 0;
			this.lblInfo.Text = "Bloodmasters dedicated server is running.  Close this window to stop the server.";
			this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// rtbConsole
			// 
			this.rtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConsole.BackColor = System.Drawing.Color.Black;
			this.rtbConsole.DetectUrls = false;
			this.rtbConsole.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.rtbConsole.ForeColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.rtbConsole.HideSelection = false;
			this.rtbConsole.Location = new System.Drawing.Point(2, 36);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.ReadOnly = true;
			this.rtbConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtbConsole.Size = new System.Drawing.Size(554, 234);
			this.rtbConsole.TabIndex = 1;
			this.rtbConsole.Text = "";
			// 
			// FormServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(558, 271);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.lblInfo);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.MinimumSize = new System.Drawing.Size(416, 188);
			this.Name = "FormServer";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Bloodmasters Dedicated Server";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormServer_Closing);
			this.ResumeLayout(false);

		}
		#endregion
		
		// Window closing
		private void FormServer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Stop the server
			General.serverrunning = false;
		}
		
		// This updates the console
		public void WriteLine(string text)
		{
			Write(text + "\n");
		}
		
		// This updates the console
		public void Write(string text)
		{
			// Check if not disposed
			if(!IsDisposed)
			{
				// Jump to the end
				rtbConsole.SelectionStart = int.MaxValue;
				rtbConsole.SelectionLength = 0;
				
				// Insert text
				rtbConsole.AppendText(text);
				
				// Jump to the end again
				rtbConsole.SelectionStart = int.MaxValue;
				rtbConsole.SelectionLength = 0;
				rtbConsole.ScrollToCaret();
				
				// Update window
				this.Update();
			}
		}
	}
}
