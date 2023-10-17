using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace CodeImp.Bloodmasters.Launcher.Interface;

public class FormDownload : System.Windows.Forms.Form
{
    private const int READ_BLOCK = 1024;

    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.ProgressBar prgStatus;
    private System.Windows.Forms.Button btnCancel;
    private System.ComponentModel.IContainer components;
    private GamesListItem server;
    private System.Windows.Forms.Timer tmrStart;
    private bool cancelled;
    private string fileurl;

    // Constructor
    public FormDownload(GamesListItem server)
    {
        // Required for Windows Form Designer support
        InitializeComponent();

        // Initialize
        this.server = server;
    }

    // Clean up any resources used.
    protected override void Dispose( bool disposing )
    {
        if(disposing)
        {
            if(components != null) components.Dispose();
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
        this.components = new System.ComponentModel.Container();
        this.lblStatus = new System.Windows.Forms.Label();
        this.prgStatus = new System.Windows.Forms.ProgressBar();
        this.btnCancel = new System.Windows.Forms.Button();
        this.tmrStart = new System.Windows.Forms.Timer(this.components);
        this.SuspendLayout();
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(12, 16);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(308, 16);
        this.lblStatus.TabIndex = 0;
        this.lblStatus.Text = "Searching server website for map download...";
        //
        // prgStatus
        //
        this.prgStatus.Location = new System.Drawing.Point(12, 36);
        this.prgStatus.Name = "prgStatus";
        this.prgStatus.Size = new System.Drawing.Size(360, 20);
        this.prgStatus.TabIndex = 1;
        //
        // btnCancel
        //
        this.btnCancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.btnCancel.Location = new System.Drawing.Point(256, 68);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(116, 28);
        this.btnCancel.TabIndex = 2;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // tmrStart
        //
        this.tmrStart.Interval = 200;
        this.tmrStart.Tick += new System.EventHandler(this.tmrStart_Tick);
        //
        // FormDownload
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(382, 107);
        this.ControlBox = false;
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.prgStatus);
        this.Controls.Add(this.lblStatus);
        this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormDownload";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Downloading...";
        this.VisibleChanged += new System.EventHandler(this.FormDownload_VisibleChanged);
        this.ResumeLayout(false);

    }
    #endregion

    // Cancel clicked
    private void btnCancel_Click(object sender, System.EventArgs e)
    {
        cancelled = true;
    }

    // This searches
    public bool Search()
    {
        HttpWebRequest req = null;
        HttpWebResponse resp = null;
        Stream download = null;
        MemoryStream html = null;
        byte[] data = new byte[READ_BLOCK];
        int contentlength, numread, newpos, endpos, lastpos = 0;
        string htmltext, htmltextlower;

        // Display status
        lblStatus.Text = "Searching server website for " + server.MapName + ".zip...";
        lblStatus.Update();

        // Server URL is what we are looking for?
        if(server.Website.ToLower().EndsWith(server.MapName.ToLower() + ".zip"))
        {
            if(server.Website.ToLower().StartsWith("http://"))
            {
                // Done already :)
                fileurl = server.Website;
                return true;
            }
        }

        try
        {
            // Download the website HTML
            req = HttpWebRequest.Create(server.Website) as HttpWebRequest;
            req.Timeout = 3000;
            resp = req.GetResponse() as HttpWebResponse;
        }
        catch(Exception)
        {
            // Clean up
            if(resp != null) resp.Close();

            // Cant download
            MessageBox.Show(this, "Unable to contact the server website. The website URL may be invalid or the website is experiencing technical difficulties.", "Downloading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        // Set progress bar
        contentlength = (int)resp.ContentLength;
        if(contentlength > 0) prgStatus.Maximum = contentlength;

        // Make a memory stream for the download
        html = new MemoryStream();

        // Get the http download
        download = resp.GetResponseStream();

        // Read the whole stream
        do
        {
            // Try reading 1 KB
            numread = -1;
            try { numread = download.Read(data, 0, READ_BLOCK); } catch(Exception e) { }

            // Anything new?
            if(numread > 0)
            {
                // Append the bytes
                html.Write(data, 0, numread);
                if((int)html.Length <= prgStatus.Maximum) prgStatus.Value = (int)html.Length;
            }

            // Allow cancel
            Application.DoEvents();
            if(cancelled)
            {
                // Clean up and leave
                html.Close();
                download.Close();
                resp.Close();
                return false;
            }
        }
        while(html.Length < resp.ContentLength);

        // Make text string
        htmltext = Encoding.ASCII.GetString(html.ToArray());
        htmltextlower = htmltext.ToLower();

        // Clean up
        html.Close();
        download.Close();
        resp.Close();

        do
        {
            // Find a "<a" tag
            newpos = htmltextlower.IndexOf("<a", lastpos);

            // Found anything?
            if(newpos > -1)
            {
                // Find the end of the tag ">"
                lastpos = htmltextlower.IndexOf(">", newpos);

                // If there is no end, use the end of the file
                if(lastpos == -1) lastpos = htmltext.Length - 1;

                // Find the "href" part
                endpos = htmltextlower.IndexOf("href", newpos);
                if(endpos > lastpos) continue;

                // Find the =
                endpos = htmltext.IndexOf("=", endpos);
                if(endpos > lastpos) continue;

                // Skip any whitespace
                endpos++;
                while((htmltext[endpos] == ' ') || (htmltext[endpos] == '\t') ||
                      (htmltext[endpos] == '\n') || (htmltext[endpos] == '\r')) endpos++;
                if(endpos > lastpos) continue;

                // Set position where URL starts
                newpos = endpos;

                // Find the next space
                endpos = htmltext.IndexOfAny(new char[]{' ', '\t', '\r', '\n' }, newpos);
                if(endpos > lastpos) endpos = lastpos;

                // Remove any quotes
                fileurl = htmltext.Substring(newpos, endpos - newpos);
                fileurl = fileurl.Replace("\"", "");
                fileurl = fileurl.Replace("\'", "");

                // URL we are looking for?
                if(fileurl.ToLower().EndsWith(server.MapName.ToLower() + ".zip"))
                {
                    // Make the url absolute
                    if(!fileurl.ToLower().StartsWith("http://"))
                        fileurl = Path.Combine(server.Website, fileurl);

                    // Found!
                    return true;
                }
            }
        }
        while(newpos > -1);

        // Nothing found
        MessageBox.Show(this, "The download link for the map " + server.MapName + ".zip could not be found on the website provided by the server.", "Downloading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return false;
    }

    // This downloads
    private bool Download()
    {
        HttpWebRequest req = null;
        HttpWebResponse resp = null;
        Stream download = null;
        FileStream file;
        byte[] data = new byte[READ_BLOCK];
        string filename;
        int contentlength, numread;

        // Display status
        lblStatus.Text = "Downloading map " + server.MapName + ".zip...";
        lblStatus.Update();

        // Set progress bar
        prgStatus.Value = 0;
        prgStatus.Maximum = 1;

        try
        {
            // Download the website HTML
            req = HttpWebRequest.Create(fileurl) as HttpWebRequest;
            req.Timeout = 3000;
            resp = req.GetResponse() as HttpWebResponse;
        }
        catch(Exception)
        {
            // Clean up
            if(resp != null) resp.Close();

            // Cant download
            MessageBox.Show(this, "Unable to contact the download website. The file URL may be invalid or the website is experiencing technical difficulties.", "Downloading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        // Set progress bar
        contentlength = (int)resp.ContentLength;
        if(contentlength > 0) prgStatus.Maximum = contentlength;

        // Make the file
        filename = Path.Combine(Paths.Instance.DownloadedResourceDir, server.MapName + ".zip");
        file = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        // Get the http download
        download = resp.GetResponseStream();

        // Read the whole stream
        do
        {
            // Try reading 1 KB
            numread = -1;
            try { numread = download.Read(data, 0, READ_BLOCK); } catch(Exception e) { }

            // Anything new?
            if(numread > 0)
            {
                // Write the bytes to file
                file.Write(data, 0, numread);
                if((int)file.Length <= prgStatus.Maximum) prgStatus.Value = (int)file.Length;
            }

            // Allow cancel
            Application.DoEvents();
            if(cancelled)
            {
                // Clean up and leave
                file.Close();
                download.Close();
                resp.Close();
                File.Delete(filename);
                return false;
            }
        }
        while(file.Length < resp.ContentLength);

        // Clean up
        file.Flush();
        file.Close();
        download.Close();
        resp.Close();

        // Success!
        return true;
    }

    // When appearing
    private void FormDownload_VisibleChanged(object sender, System.EventArgs e)
    {
        // Start timer
        tmrStart.Enabled = true;
    }

    private void tmrStart_Tick(object sender, System.EventArgs e)
    {
        // Stop timer
        tmrStart.Enabled = false;

        // Make sure window is refreshed
        this.Update();
        if(this.Search())
        {
            // Download
            if(this.Download())
            {
                // Success
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Failed
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
        else
        {
            // Failed
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
