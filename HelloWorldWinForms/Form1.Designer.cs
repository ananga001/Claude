namespace HelloWorldWinForms;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        labelHello = new Label();
        SuspendLayout();

        labelHello.AutoSize = true;
        labelHello.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        labelHello.Location = new Point(80, 80);
        labelHello.Text = "Hello, World!";

        ClientSize = new Size(320, 200);
        Controls.Add(labelHello);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Hello World";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label labelHello;
}
