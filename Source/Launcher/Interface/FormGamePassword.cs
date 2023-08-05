/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace CodeImp.Bloodmasters.Launcher
{
	public class FormGamePassword : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox2;
		public System.Windows.Forms.TextBox txtJoinPassword;
		private System.Windows.Forms.Label lblJoinPassword;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnJoin;
		private System.ComponentModel.Container components = null;

		// Constructor
		public FormGamePassword()
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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtJoinPassword = new System.Windows.Forms.TextBox();
			this.lblJoinPassword = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnJoin = new System.Windows.Forms.Button();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			//
			// groupBox2
			//
			this.groupBox2.Controls.Add(this.txtJoinPassword);
			this.groupBox2.Controls.Add(this.lblJoinPassword);
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(324, 68);
			this.groupBox2.TabIndex = 10;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = " Password ";
			//
			// txtJoinPassword
			//
			this.txtJoinPassword.AutoSize = false;
			this.txtJoinPassword.Location = new System.Drawing.Point(92, 27);
			this.txtJoinPassword.MaxLength = 50;
			this.txtJoinPassword.Name = "txtJoinPassword";
			this.txtJoinPassword.PasswordChar = '●';
			this.txtJoinPassword.Size = new System.Drawing.Size(192, 22);
			this.txtJoinPassword.TabIndex = 7;
			this.txtJoinPassword.Text = "";
			//
			// lblJoinPassword
			//
			this.lblJoinPassword.Location = new System.Drawing.Point(4, 27);
			this.lblJoinPassword.Name = "lblJoinPassword";
			this.lblJoinPassword.Size = new System.Drawing.Size(84, 20);
			this.lblJoinPassword.TabIndex = 6;
			this.lblJoinPassword.Text = "Password:";
			this.lblJoinPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// btnCancel
			//
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(228, 92);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 27);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "Cancel";
			//
			// btnJoin
			//
			this.btnJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnJoin.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnJoin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnJoin.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnJoin.Location = new System.Drawing.Point(120, 92);
			this.btnJoin.Name = "btnJoin";
			this.btnJoin.Size = new System.Drawing.Size(104, 27);
			this.btnJoin.TabIndex = 12;
			this.btnJoin.Text = "Join";
			//
			// FormGamePassword
			//
			this.AcceptButton = this.btnJoin;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(339, 127);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnJoin);
			this.Controls.Add(this.groupBox2);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormGamePassword";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Game Password";
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
