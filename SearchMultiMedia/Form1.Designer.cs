namespace SearchMultiMedia
{
    partial class Form1
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
            btnRecord = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnSelectImage = new Button();
            btnStopRecording = new Button();
            btnSearch = new Button();
            flpText = new FlowLayoutPanel();
            flpAudio = new FlowLayoutPanel();
            flpImage = new FlowLayoutPanel();
            btnRemove = new Button();
            flpTextPanel = new FlowLayoutPanel();
            flpAudioPanel = new FlowLayoutPanel();
            flpImagePanel = new FlowLayoutPanel();
            lblResultText = new Label();
            lblResultAudio = new Label();
            lblResultImage = new Label();
            SuspendLayout();
            // 
            // btnRecord
            // 
            btnRecord.Location = new Point(1240, 14);
            btnRecord.Name = "btnRecord";
            btnRecord.Size = new Size(130, 29);
            btnRecord.TabIndex = 2;
            btnRecord.Text = "Ghi âm";
            btnRecord.UseVisualStyleBackColor = true;
            btnRecord.Click += btnRecord_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 14);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(1000, 57);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 97);
            label1.Name = "label1";
            label1.Size = new Size(62, 20);
            label1.TabIndex = 2;
            label1.Text = "Văn bản";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(515, 97);
            label2.Name = "label2";
            label2.Size = new Size(73, 20);
            label2.TabIndex = 3;
            label2.Text = "Âm thanh";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1017, 97);
            label3.Name = "label3";
            label3.Size = new Size(68, 20);
            label3.TabIndex = 4;
            label3.Text = "Hình ảnh";
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new Point(1080, 49);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new Size(130, 29);
            btnSelectImage.TabIndex = 1;
            btnSelectImage.Text = "Chọn ảnh";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += btnSelectImage_Click;
            // 
            // btnStopRecording
            // 
            btnStopRecording.Location = new Point(1240, 49);
            btnStopRecording.Name = "btnStopRecording";
            btnStopRecording.Size = new Size(130, 29);
            btnStopRecording.TabIndex = 3;
            btnStopRecording.Text = "Dừng ghi âm";
            btnStopRecording.UseVisualStyleBackColor = true;
            btnStopRecording.Click += btnStopRecording_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(1080, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(130, 29);
            btnSearch.TabIndex = 0;
            btnSearch.Text = "Tìm kiếm";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // flpText
            // 
            flpText.AutoScroll = true;
            flpText.Location = new Point(12, 163);
            flpText.Name = "flpText";
            flpText.Size = new Size(458, 429);
            flpText.TabIndex = 11;
            // 
            // flpAudio
            // 
            flpAudio.AutoScroll = true;
            flpAudio.Location = new Point(515, 163);
            flpAudio.Name = "flpAudio";
            flpAudio.Size = new Size(458, 429);
            flpAudio.TabIndex = 12;
            // 
            // flpImage
            // 
            flpImage.AutoScroll = true;
            flpImage.Location = new Point(1017, 163);
            flpImage.Name = "flpImage";
            flpImage.Size = new Size(458, 429);
            flpImage.TabIndex = 13;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(1017, 14);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(27, 29);
            btnRemove.TabIndex = 14;
            btnRemove.Text = "X";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // flpTextPanel
            // 
            flpTextPanel.Location = new Point(12, 598);
            flpTextPanel.Name = "flpTextPanel";
            flpTextPanel.Size = new Size(458, 47);
            flpTextPanel.TabIndex = 15;
            // 
            // flpAudioPanel
            // 
            flpAudioPanel.Location = new Point(515, 598);
            flpAudioPanel.Name = "flpAudioPanel";
            flpAudioPanel.Size = new Size(458, 47);
            flpAudioPanel.TabIndex = 16;
            // 
            // flpImagePanel
            // 
            flpImagePanel.Location = new Point(1017, 598);
            flpImagePanel.Name = "flpImagePanel";
            flpImagePanel.Size = new Size(458, 47);
            flpImagePanel.TabIndex = 17;
            // 
            // lblResultText
            // 
            lblResultText.AutoSize = true;
            lblResultText.Location = new Point(12, 140);
            lblResultText.Name = "lblResultText";
            lblResultText.Size = new Size(0, 20);
            lblResultText.TabIndex = 18;
            // 
            // lblResultAudio
            // 
            lblResultAudio.AutoSize = true;
            lblResultAudio.Location = new Point(515, 140);
            lblResultAudio.Name = "lblResultAudio";
            lblResultAudio.Size = new Size(0, 20);
            lblResultAudio.TabIndex = 19;
            // 
            // lblResultImage
            // 
            lblResultImage.AutoSize = true;
            lblResultImage.Location = new Point(1017, 140);
            lblResultImage.Name = "lblResultImage";
            lblResultImage.Size = new Size(0, 20);
            lblResultImage.TabIndex = 20;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1489, 658);
            Controls.Add(lblResultImage);
            Controls.Add(lblResultAudio);
            Controls.Add(lblResultText);
            Controls.Add(flpImagePanel);
            Controls.Add(flpAudioPanel);
            Controls.Add(flpTextPanel);
            Controls.Add(btnRemove);
            Controls.Add(flpImage);
            Controls.Add(flpAudio);
            Controls.Add(flpText);
            Controls.Add(btnSearch);
            Controls.Add(btnStopRecording);
            Controls.Add(btnSelectImage);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(btnRecord);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnRecord;
        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnSelectImage;
        private Button btnStopRecording;
        private Button btnSearch;
        private FlowLayoutPanel flpText;
        private FlowLayoutPanel flpAudio;
        private FlowLayoutPanel flpImage;
        private Button btnRemove;
        private FlowLayoutPanel flpTextPanel;
        private FlowLayoutPanel flpAudioPanel;
        private FlowLayoutPanel flpImagePanel;
        private Label lblResultText;
        private Label lblResultAudio;
        private Label lblResultImage;
    }
}
