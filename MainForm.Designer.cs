namespace DesktopPictureFrame
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing )
    {
      if( disposing && ( components != null ) )
      {
        components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
      this.uiImage = new System.Windows.Forms.PictureBox();
      this.uiSystemTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.uiImage)).BeginInit();
      this.SuspendLayout();
      // 
      // uiImage
      // 
      this.uiImage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.uiImage.ErrorImage = ((System.Drawing.Image)(resources.GetObject("uiImage.ErrorImage")));
      this.uiImage.Location = new System.Drawing.Point(0, 0);
      this.uiImage.Name = "uiImage";
      this.uiImage.Size = new System.Drawing.Size(284, 261);
      this.uiImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.uiImage.TabIndex = 0;
      this.uiImage.TabStop = false;
      this.uiImage.LoadCompleted += new System.ComponentModel.AsyncCompletedEventHandler(this.uiImage_LoadCompleted);
      this.uiImage.Click += new System.EventHandler(this.uiImage_Click);
      this.uiImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
      // 
      // uiSystemTrayIcon
      // 
      this.uiSystemTrayIcon.Text = "notifyIcon";
      this.uiSystemTrayIcon.Visible = true;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Fuchsia;
      this.ClientSize = new System.Drawing.Size(284, 261);
      this.Controls.Add(this.uiImage);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "MainForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.TransparencyKey = System.Drawing.Color.Fuchsia;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.Load += new System.EventHandler(this.MainForm_Load);
      ((System.ComponentModel.ISupportInitialize)(this.uiImage)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox uiImage;
    private System.Windows.Forms.NotifyIcon uiSystemTrayIcon;
  }
}

