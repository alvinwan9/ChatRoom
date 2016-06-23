namespace Chat_Client_V1
{
    partial class Talking
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tb_SendMsg = new System.Windows.Forms.TextBox();
            this.btn_Send = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.rtb_ShowMsg = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // tb_SendMsg
            // 
            this.tb_SendMsg.Location = new System.Drawing.Point(37, 242);
            this.tb_SendMsg.Multiline = true;
            this.tb_SendMsg.Name = "tb_SendMsg";
            this.tb_SendMsg.Size = new System.Drawing.Size(435, 109);
            this.tb_SendMsg.TabIndex = 1;
            // 
            // btn_Send
            // 
            this.btn_Send.Location = new System.Drawing.Point(290, 357);
            this.btn_Send.Name = "btn_Send";
            this.btn_Send.Size = new System.Drawing.Size(75, 23);
            this.btn_Send.TabIndex = 2;
            this.btn_Send.Text = "发送";
            this.btn_Send.UseVisualStyleBackColor = true;
            this.btn_Send.Click += new System.EventHandler(this.btn_Send_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(397, 357);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(75, 23);
            this.btn_Close.TabIndex = 3;
            this.btn_Close.Text = "关闭";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // rtb_ShowMsg
            // 
            this.rtb_ShowMsg.Location = new System.Drawing.Point(37, 13);
            this.rtb_ShowMsg.Name = "rtb_ShowMsg";
            this.rtb_ShowMsg.ReadOnly = true;
            this.rtb_ShowMsg.Size = new System.Drawing.Size(435, 214);
            this.rtb_ShowMsg.TabIndex = 4;
            this.rtb_ShowMsg.Text = "";
            // 
            // Talking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 405);
            this.Controls.Add(this.rtb_ShowMsg);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Send);
            this.Controls.Add(this.tb_SendMsg);
            this.Name = "Talking";
            this.Text = "Talking";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Talking_FormClosed);
            this.Load += new System.EventHandler(this.Talking_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_SendMsg;
        private System.Windows.Forms.Button btn_Send;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.RichTextBox rtb_ShowMsg;
    }
}