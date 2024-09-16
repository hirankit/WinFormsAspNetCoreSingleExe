namespace WinFormsAspNetCoreSingleExe;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        aspNetCoreHostTogglerButton = new Button();
        webHostUrlLinkLabel = new LinkLabel();
        consoleWindowTogglerButton = new Button();
        SuspendLayout();
        // 
        // aspNetCoreHostTogglerButton
        // 
        aspNetCoreHostTogglerButton.Font = new Font("Consolas", 8F);
        aspNetCoreHostTogglerButton.Location = new Point(22, 119);
        aspNetCoreHostTogglerButton.Margin = new Padding(6);
        aspNetCoreHostTogglerButton.Name = "aspNetCoreHostTogglerButton";
        aspNetCoreHostTogglerButton.Size = new Size(472, 49);
        aspNetCoreHostTogglerButton.TabIndex = 0;
        aspNetCoreHostTogglerButton.Text = "Start AspNetCore Host";
        aspNetCoreHostTogglerButton.UseVisualStyleBackColor = true;
        aspNetCoreHostTogglerButton.Click += AspNetCoreHostTogglerButton_Click;
        // 
        // webHostUrlLinkLabel
        // 
        webHostUrlLinkLabel.Font = new Font("Consolas", 7.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
        webHostUrlLinkLabel.LinkArea = new LinkArea(0, 0);
        webHostUrlLinkLabel.Location = new Point(22, 23);
        webHostUrlLinkLabel.Margin = new Padding(6, 0, 6, 0);
        webHostUrlLinkLabel.Name = "webHostUrlLinkLabel";
        webHostUrlLinkLabel.Size = new Size(472, 73);
        webHostUrlLinkLabel.TabIndex = 2;
        webHostUrlLinkLabel.Text = "Web Server is not Running";
        webHostUrlLinkLabel.TextAlign = ContentAlignment.MiddleCenter;
        webHostUrlLinkLabel.LinkClicked += UrlLinkLabel_LinkClicked;
        // 
        // consoleWindowTogglerButton
        // 
        consoleWindowTogglerButton.Font = new Font("Consolas", 8F);
        consoleWindowTogglerButton.Location = new Point(22, 181);
        consoleWindowTogglerButton.Margin = new Padding(6);
        consoleWindowTogglerButton.Name = "consoleWindowTogglerButton";
        consoleWindowTogglerButton.Size = new Size(472, 49);
        consoleWindowTogglerButton.TabIndex = 3;
        consoleWindowTogglerButton.Text = "Show Console Window";
        consoleWindowTogglerButton.UseVisualStyleBackColor = true;
        consoleWindowTogglerButton.Click += ConsoleWindowTogglerButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(509, 275);
        Controls.Add(consoleWindowTogglerButton);
        Controls.Add(webHostUrlLinkLabel);
        Controls.Add(aspNetCoreHostTogglerButton);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Margin = new Padding(6);
        MaximizeBox = false;
        MaximumSize = new Size(535, 346);
        MinimizeBox = false;
        MinimumSize = new Size(535, 346);
        Name = "MainForm";
        Text = "Main";
        ResumeLayout(false);
    }

    #endregion

    private Button aspNetCoreHostTogglerButton;
    private LinkLabel webHostUrlLinkLabel;
    private Button consoleWindowTogglerButton;
}
