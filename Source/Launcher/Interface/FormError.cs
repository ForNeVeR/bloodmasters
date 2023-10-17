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
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Launcher.Interface;

internal sealed class FormError : System.Windows.Forms.Form
{
    // Controls
    private System.Windows.Forms.PictureBox picIcon;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnClose;
    private System.ComponentModel.Container components = null;
    public System.Windows.Forms.Label lblTitle;
    public System.Windows.Forms.Label lblMessage;
    public System.Windows.Forms.TextBox txtCallStack;

    // Constructor
    public FormError()
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
    private void InitializeComponent()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormError));
        this.picIcon = new System.Windows.Forms.PictureBox();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblMessage = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.txtCallStack = new System.Windows.Forms.TextBox();
        this.btnClose = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        // picIcon
        //
        this.picIcon.Image = ((System.Drawing.Image)(resources.GetObject("picIcon.Image")));
        this.picIcon.Location = new System.Drawing.Point(16, 12);
        this.picIcon.Name = "picIcon";
        this.picIcon.Size = new System.Drawing.Size(32, 32);
        this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.picIcon.TabIndex = 0;
        this.picIcon.TabStop = false;
        //
        // lblTitle
        //
        this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                     | System.Windows.Forms.AnchorStyles.Right)));
        this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.lblTitle.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.lblTitle.Location = new System.Drawing.Point(56, 12);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(504, 16);
        this.lblTitle.TabIndex = 1;
        this.lblTitle.Text = "Bloodmasters engine throws the following ";
        this.lblTitle.UseMnemonic = false;
        //
        // lblMessage
        //
        this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                       | System.Windows.Forms.AnchorStyles.Right)));
        this.lblMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.lblMessage.Location = new System.Drawing.Point(56, 28);
        this.lblMessage.Name = "lblMessage";
        this.lblMessage.Size = new System.Drawing.Size(504, 36);
        this.lblMessage.TabIndex = 2;
        this.lblMessage.Text = "BlaException bladieblaap cannot be found and such.";
        this.lblMessage.UseMnemonic = false;
        //
        // label1
        //
        this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                   | System.Windows.Forms.AnchorStyles.Right)));
        this.label1.AutoSize = true;
        this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.label1.Location = new System.Drawing.Point(56, 64);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(137, 16);
        this.label1.TabIndex = 3;
        this.label1.Text = "Error report information:";
        this.label1.UseMnemonic = false;
        //
        // txtCallStack
        //
        this.txtCallStack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                          | System.Windows.Forms.AnchorStyles.Left)
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
        this.txtCallStack.Location = new System.Drawing.Point(56, 80);
        this.txtCallStack.Multiline = true;
        this.txtCallStack.Name = "txtCallStack";
        this.txtCallStack.ReadOnly = true;
        this.txtCallStack.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.txtCallStack.Size = new System.Drawing.Size(504, 184);
        this.txtCallStack.TabIndex = 0;
        this.txtCallStack.Text = "Calls";
        this.txtCallStack.WordWrap = false;
        //
        // btnClose
        //
        this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnClose.Location = new System.Drawing.Point(444, 272);
        this.btnClose.Name = "btnClose";
        this.btnClose.Size = new System.Drawing.Size(116, 24);
        this.btnClose.TabIndex = 2;
        this.btnClose.Text = "Close";
        this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
        //
        // FormError
        //
        this.AcceptButton = this.btnClose;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btnClose;
        this.ClientSize = new System.Drawing.Size(568, 305);
        this.Controls.Add(this.btnClose);
        this.Controls.Add(this.txtCallStack);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblMessage);
        this.Controls.Add(this.picIcon);
        this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "FormError";
        this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Bloodmasters engine Exception";
        this.ResumeLayout(false);

    }
    #endregion

    // Close clicked
    private void btnClose_Click(object sender, System.EventArgs e)
    {
        // Close window
        this.Hide();
    }
}
