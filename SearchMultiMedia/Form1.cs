using Microsoft.Data.SqlClient;
using NAudio.Wave;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace SearchMultiMedia
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=LAPTOP-79T4Q5ET\\BACH;Initial Catalog=DPT;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
        private WaveInEvent waveIn;
        private WaveFileWriter waveFileWriter;
        private string filePath;
        private List<(int id, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> textResultsForTextSearch;
        private List<(int id, string tenFile, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> audioResultsForTextSearch;
        private List<(int id, string tenFile, string tieuDe, double similarity)> imageResultsForTextSearch;
        private List<(int id, string tenFile, string tieuDe, double distance)> imageResultsForImageSearch;
        private List<(int id, string tenFile, string tieuDe, string noiDungTomTat, double distance)> audioResultsForAudioSearch;
        private int currentPageText = 1;
        private int currentPageTextAudio = 1;
        private int currentPageTextImage = 1;
        private int currentPageAudio = 1;
        private int currentPageImage = 1;
        private const int itemsPerPage = 5;
        private System.Windows.Forms.Timer timerRecording = new System.Windows.Forms.Timer { Interval = 1000 };
        Stopwatch stopwatch = new Stopwatch();
        private bool isBlinking = false;
        private System.Windows.Forms.Timer blinkTimer = new System.Windows.Forms.Timer { Interval = 500 };
        public Form1()
        {
            InitializeComponent();
            CustomizeUI();
            CustomizeLabels();
            CustomizeButtonUI();
            InitializeTimer();
            InitializeBlinkTimer();

            // Đặt tiêu đề cho Form
            this.Text = "Nhóm 9 Đa phương tiện";

            // Đặt màu nền cho Form
            this.BackColor = Color.LightBlue; // Thay đổi màu nền thành LightBlue (hoặc bất kỳ màu nào bạn muốn)
        }


        private void CustomizeButtonUI()
        {
            // Hàm tùy chỉnh giao diện nút (không thay đổi vị trí)
            void CustomizeButtonStyle(Button button, Color normalColor, Color hoverColor)
            {
                button.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Font chữ
                button.ForeColor = Color.White;                        // Màu chữ
                button.BackColor = normalColor;                        // Màu nền mặc định
                button.FlatStyle = FlatStyle.Flat;                     // Không viền nổi
                button.FlatAppearance.BorderSize = 0;                  // Không viền
                button.Cursor = Cursors.Hand;                         // Con trỏ chuột khi hover

                // Hiệu ứng hover
                button.MouseEnter += (s, e) => button.BackColor = hoverColor;
                button.MouseLeave += (s, e) => button.BackColor = normalColor;
            }

            // Cài đặt giao diện cho từng nút
            CustomizeButtonStyle(btnSearch, Color.DodgerBlue, Color.MediumBlue);       // Nút Tìm kiếm
            CustomizeButtonStyle(btnRecord, Color.SeaGreen, Color.DarkGreen);         // Nút Ghi âm
            CustomizeButtonStyle(btnSelectImage, Color.Orange, Color.DarkOrange);     // Nút Chọn ảnh
            CustomizeButtonStyle(btnStopRecording, Color.Crimson, Color.DarkRed);     // Nút Dừng ghi âm
        }


        private void CustomizeLabels()
        {
            void CustomizeLabel(Label label, string text, int fontSize, Color textColor, ContentAlignment alignment)
            {
                label.Text = text; // Nội dung Label
                label.Font = new Font("Segoe UI", fontSize, FontStyle.Bold); // Phông chữ đẹp
                label.ForeColor = textColor; // Màu chữ
                label.TextAlign = alignment; // Căn chỉnh
                label.BackColor = Color.Transparent; // Nền trong suốt
            }

            // Tùy chỉnh từng Label
            CustomizeLabel(label1, "Văn bản", 12, Color.DodgerBlue, ContentAlignment.MiddleCenter);
            CustomizeLabel(label2, "Âm thanh", 12, Color.SeaGreen, ContentAlignment.MiddleCenter);
            CustomizeLabel(label3, "Hình ảnh", 12, Color.DarkOrange, ContentAlignment.MiddleCenter);
        }


        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn file ảnh",
                Filter = "File ảnh|*.jpg;*.png;*.jpeg",
                FilterIndex = 1,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] selectedFiles = openFileDialog.FileNames;
                foreach (string file in selectedFiles)
                {
                    textBox1.Text = file;
                }

                MessageBox.Show($"{selectedFiles.Length} file hình ảnh đã được chọn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnRecord_Click(object sender, EventArgs e)
        {
            try
            {
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = 0;
                waveIn.WaveFormat = new WaveFormat(44100, 1); // Cấu hình định dạng ghi âm (44100 Hz, mono)
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                string directory = Directory.GetCurrentDirectory();
                filePath = Path.Combine(directory, "recording.wav");

                waveFileWriter = new WaveFileWriter(filePath, waveIn.WaveFormat);

                waveIn.StartRecording();
                //MessageBox.Show("Đang ghi âm...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                textBox1.Text = filePath;
                lblRecordingIndicator.Visible = true;
                lblRecordingTime.Visible = true;
                lblRecordingTime.Text = "Thời gian: 00:00";
                isBlinking = true;
                blinkTimer.Start();
                stopwatch.Restart();
                timerRecording.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            waveFileWriter?.Dispose();
            waveIn?.Dispose();
        }
        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            try
            {
                waveIn.StopRecording();
                isBlinking = false;
                blinkTimer.Stop();
                // Ẩn chấm đỏ và dừng đếm thời gian
                lblRecordingIndicator.Visible = false;
                lblRecordingTime.Visible = false;
                timerRecording.Stop();
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi dừng ghi âm: " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string input = textBox1.Text;
            lblResultText.Text = "";
            lblResultAudio.Text = "";
            lblResultImage.Text = "";

            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Vui lòng nhập văn bản hoặc chọn tệp để tìm kiếm.");
                return;
            }

            stopwatch.Start();

            if (IsImageFile(input))
            {
                imageResultsForImageSearch = ControlCls.GetTopImageDistanceRecords(input, connectionString);
                lblResultImage.Text = $"Kết quả: {imageResultsForImageSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";
                DisplayResultsImageWhenSearchForImage();
            }
            else if (IsAudioFile(input))
            {
                audioResultsForAudioSearch = ControlCls.GetTopAudioDistanceRecords(input, connectionString);

                string textFromAudio = ConvertRecordWavToText.GetTextFromRecordWav(input);
                textBox1.Text = $"{input}{Environment.NewLine}Chuyển đổi giọng nói thành văn bản: {textFromAudio}";

                var preparedData = ControlCls.PrepareTextInputData(textFromAudio);
                textResultsForTextSearch = ControlCls.GetTopTextSimilarRecords(preparedData, textFromAudio, connectionString);
                imageResultsForTextSearch = ControlCls.GetTopImageSimilarRecords(preparedData, textFromAudio, connectionString);

                lblResultAudio.Text = $"Kết quả: {audioResultsForAudioSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";
                lblResultText.Text = $"Kết quả: {textResultsForTextSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";
                lblResultImage.Text = $"Kết quả: {imageResultsForTextSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";

                DisplayResultsAudioWhenSearchForAudio();
                DisplayResultsTextWhenSearchForText();
                DisplayResultsImageWhenSearchForText();
            }
            else
            {
                var preparedData = ControlCls.PrepareTextInputData(input);
                textResultsForTextSearch = ControlCls.GetTopTextSimilarRecords(preparedData, input, connectionString);
                audioResultsForTextSearch = ControlCls.GetTopAudioSimilarRecords(preparedData, input, connectionString);
                imageResultsForTextSearch = ControlCls.GetTopImageSimilarRecords(preparedData, input, connectionString);

                lblResultText.Text = $"Kết quả: {textResultsForTextSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";
                lblResultAudio.Text = $"Kết quả: {audioResultsForTextSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";
                lblResultImage.Text = $"Kết quả: {imageResultsForTextSearch.Count} bản ghi, Thời gian tìm kiếm: {stopwatch.ElapsedMilliseconds} ms";

                DisplayResultsTextWhenSearchForText();
                DisplayResultsAudioWhenSearchForText();
                DisplayResultsImageWhenSearchForText();
            }

            stopwatch.Stop();

        }
        private bool IsImageFile(string path)
        {
            string pathUpdate = path.Replace("\"", "");
            string[] imageExtensions = { ".jpg", ".jpeg", ".png" };
            string extension = Path.GetExtension(pathUpdate).ToLower();
            return Array.Exists(imageExtensions, ext => ext == extension);
        }
        private bool IsAudioFile(string path)
        {
            string pathUpdate = path.Replace("\"", "");
            string[] audioExtensions = { ".wav" };
            string extension = Path.GetExtension(pathUpdate).ToLower();
            return Array.Exists(audioExtensions, ext => ext == extension);
        }
        private void DisplayResultsTextWhenSearchForText()
        {
            flpText.Controls.Clear();

            int startIndex = (currentPageText - 1) * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            var pagedRecords = textResultsForTextSearch.Skip(startIndex).Take(itemsPerPage).ToList();
            bool hasResults = false;

            foreach (var record in pagedRecords)
            {
                if (record.similarity > 0)
                {
                    hasResults = true;

                    LinkLabel linkLabel = new LinkLabel
                    {
                        Text = $"{Environment.NewLine}Tiêu đề: {record.tieuDe}",
                        Tag = record.id,
                        AutoSize = true,
                        Width = flpText.Width - 20
                    };
                    linkLabel.LinkClicked += (sender, e) => ShowTextRecordDetails(sender);

                    Label summaryLabel = new Label
                    {
                        Text = $"Nội dung tóm tắt: {record.noiDungTomTat}",
                        AutoSize = true,
                        Width = flpText.Width - 20
                    };

                    Label similarityLabel = new Label
                    {
                        Text = $"Độ tương đồng: {record.similarity.ToString("F2")}%",
                        AutoSize = true,
                        Width = flpText.Width - 20
                    };

                    flpText.Controls.Add(linkLabel);
                    flpText.Controls.Add(summaryLabel);
                    flpText.Controls.Add(similarityLabel);

                    flpText.FlowDirection = FlowDirection.TopDown;
                    flpText.WrapContents = true;
                    flpText.AutoScroll = true;
                }
            }

            if (!hasResults)
            {
                flpText.Controls.Add(new Label { Text = $"{Environment.NewLine}Không có bản ghi có độ tương tự > 0%.", AutoSize = true });
            }

            DisplayPaginationText();
        }
        private void DisplayResultsAudioWhenSearchForText()
        {
            flpAudio.Controls.Clear();

            int startIndex = (currentPageTextAudio - 1) * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            var pagedRecords = audioResultsForTextSearch.Skip(startIndex).Take(itemsPerPage).ToList();
            bool hasResults = false;

            foreach (var record in pagedRecords)
            {
                if (record.similarity > 0)
                {
                    hasResults = true;

                    LinkLabel linkLabel = new LinkLabel
                    {
                        Text = $"{Environment.NewLine}Tiêu đề: {record.tieuDe}",
                        Tag = record.id,
                        AutoSize = true,
                        Width = flpAudio.Width - 20
                    };

                    linkLabel.LinkClicked += (sender, e) =>
                    {
                        try
                        {
                            string baseDirectory = ConfigurationManager.AppSettings["AudioDirectory"];
                            string fileName = record.tenFile;
                            string filePath = Path.Combine(baseDirectory, fileName);

                            if (!File.Exists(filePath))
                            {
                                MessageBox.Show($"File không tồn tại: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath)
                            {
                                UseShellExecute = true
                            };
                            Process.Start(processStartInfo);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi mở file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    Label summaryLabel = new Label
                    {
                        Text = $"Nội dung tóm tắt: {record.noiDungTomTat}",
                        AutoSize = true,
                        Width = flpAudio.Width - 20
                    };

                    Label similarityLabel = new Label
                    {
                        Text = $"Độ tương đồng: {record.similarity.ToString("F2")}%",
                        AutoSize = true,
                        Width = flpAudio.Width - 20
                    };

                    flpAudio.Controls.Add(linkLabel);
                    flpAudio.Controls.Add(summaryLabel);
                    flpAudio.Controls.Add(similarityLabel);

                    flpAudio.FlowDirection = FlowDirection.TopDown;
                    flpAudio.WrapContents = true;
                    flpAudio.AutoScroll = true;
                }
            }

            if (!hasResults)
            {
                flpAudio.Controls.Add(new Label { Text = $"{Environment.NewLine}Không có bản ghi có độ tương tự > 0%.", AutoSize = true });
            }

            DisplayPaginationTextAudio();
        }
        private void DisplayResultsImageWhenSearchForText()
        {
            flpImage.Controls.Clear();

            int startIndex = (currentPageTextImage - 1) * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            var pagedRecords = imageResultsForTextSearch.Skip(startIndex).Take(itemsPerPage).ToList();
            bool hasResults = false;

            foreach (var record in pagedRecords)
            {
                if (record.similarity > 0)
                {
                    hasResults = true;

                    LinkLabel linkLabel = new LinkLabel
                    {
                        Text = $"{Environment.NewLine}Tiêu đề: {record.tieuDe}",
                        Tag = record.id,
                        AutoSize = true,
                        Width = flpImage.Width - 20
                    };
                    linkLabel.LinkClicked += (sender, e) =>
                    {
                        try
                        {
                            string baseDirectory = ConfigurationManager.AppSettings["ImageDirectory"];
                            string fileName = record.tenFile;
                            string filePath = Path.Combine(baseDirectory, fileName);

                            if (!File.Exists(filePath))
                            {
                                MessageBox.Show($"File không tồn tại: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath)
                            {
                                UseShellExecute = true
                            };
                            Process.Start(processStartInfo);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi mở file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    Label similarityLabel = new Label
                    {
                        Text = $"Độ tương tự: {record.similarity.ToString("F2")}%",
                        AutoSize = true,
                        Width = flpImage.Width - 20
                    };

                    flpImage.Controls.Add(linkLabel);
                    flpImage.Controls.Add(similarityLabel);

                    flpImage.FlowDirection = FlowDirection.TopDown;
                    flpImage.WrapContents = true;
                    flpImage.AutoScroll = true;
                }
            }

            if (!hasResults)
            {
                flpImage.Controls.Add(new Label { Text = $"{Environment.NewLine}Không có bản ghi có độ tương tự > 0%.", AutoSize = true });
            }

            DisplayPaginationTextImage();
        }
        private void DisplayResultsImageWhenSearchForImage()
        {
            flpText.Controls.Clear();
            flpAudio.Controls.Clear();
            flpImage.Controls.Clear();

            int startIndex = (currentPageImage - 1) * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            var pagedRecords = imageResultsForImageSearch.Skip(startIndex).Take(itemsPerPage).ToList();
            bool hasResults = false;

            foreach (var record in pagedRecords)
            {
                hasResults = true;

                LinkLabel linkLabel = new LinkLabel
                {
                    Text = $"{Environment.NewLine}Tiêu đề: {record.tieuDe}",
                    Tag = record.id,
                    AutoSize = true,
                    Width = flpImage.Width - 20
                };
                linkLabel.LinkClicked += (sender, e) =>
                {
                    try
                    {
                        string baseDirectory = ConfigurationManager.AppSettings["ImageDirectory"];
                        string fileName = record.tenFile;
                        string filePath = Path.Combine(baseDirectory, fileName);

                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show($"File không tồn tại: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(processStartInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi mở file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                Label distanceLabel = new Label
                {
                    Text = $"Khoảng cách: {record.distance.ToString("F2")}",
                    AutoSize = true,
                    Width = flpImage.Width - 20
                };

                flpImage.Controls.Add(linkLabel);
                flpImage.Controls.Add(distanceLabel);

                flpImage.FlowDirection = FlowDirection.TopDown;
                flpImage.WrapContents = true;
                flpImage.AutoScroll = true;
            }

            if (!hasResults)
            {
                flpImage.Controls.Add(new Label { Text = $"{Environment.NewLine}Không có bản ghi", AutoSize = true });
            }

            DisplayPaginationImage();
        }
        private void DisplayResultsAudioWhenSearchForAudio()
        {
            flpAudio.Controls.Clear();

            int startIndex = (currentPageAudio - 1) * itemsPerPage;
            int endIndex = startIndex + itemsPerPage;

            var pagedRecords = audioResultsForAudioSearch.Skip(startIndex).Take(itemsPerPage).ToList();
            bool hasResults = false;

            foreach (var record in pagedRecords)
            {
                hasResults = true;

                LinkLabel linkLabel = new LinkLabel
                {
                    Text = $"{Environment.NewLine}Tiêu đề: {record.tieuDe}",
                    Tag = record.id,
                    AutoSize = true,
                    Width = flpAudio.Width - 20
                };
                linkLabel.LinkClicked += (sender, e) =>
                {
                    try
                    {
                        string baseDirectory = ConfigurationManager.AppSettings["AudioDirectory"];
                        string fileName = record.tenFile;
                        string filePath = Path.Combine(baseDirectory, fileName);

                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show($"File không tồn tại: {filePath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        ProcessStartInfo processStartInfo = new ProcessStartInfo(filePath)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(processStartInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi mở file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                Label summaryLabel = new Label
                {
                    Text = $"Nội dung tóm tắt: {record.noiDungTomTat}",
                    AutoSize = true,
                    Width = flpAudio.Width - 20
                };

                Label distanceLabel = new Label
                {
                    Text = $"Khoảng cách: {record.distance.ToString("F2")}",
                    AutoSize = true,
                    Width = flpAudio.Width - 20
                };

                flpAudio.Controls.Add(linkLabel);
                flpAudio.Controls.Add(summaryLabel);
                flpAudio.Controls.Add(distanceLabel);

                flpAudio.FlowDirection = FlowDirection.TopDown;
                flpAudio.WrapContents = true;
                flpAudio.AutoScroll = true;
            }

            if (!hasResults)
            {
                flpAudio.Controls.Add(new Label { Text = "Không có bản ghi có độ tương tự > 0%.", AutoSize = true });
            }

            DisplayPaginationAudio();
        }
        private void DisplayPaginationText()
        {
            int totalPages = (int)Math.Ceiling((double)textResultsForTextSearch.Count / itemsPerPage);

            flpTextPanel.Controls.Clear();

            Button firstPageButton = new Button
            {
                Text = "Trang đầu",
                Enabled = currentPageText > 1,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            firstPageButton.Click += (sender, e) =>
                {
                    currentPageText = 1;
                    flpTextPanel.Controls.Clear();
                    DisplayResultsTextWhenSearchForText();
                    DisplayPaginationText();
                };

            Button prevButton = new Button
            {
                Text = "Trước",
                Enabled = currentPageText > 1,
                Width = 80,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            prevButton.Click += (sender, e) =>
                    {
                        currentPageText--;
                        flpTextPanel.Controls.Clear();
                        DisplayResultsTextWhenSearchForText();
                        DisplayPaginationText();
                    };

            Button nextButton = new Button
            {
                Text = "Tiếp theo",
                Enabled = currentPageText < totalPages,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            nextButton.Click += (sender, e) =>
            {
                currentPageText++;
                flpTextPanel.Controls.Clear();
                DisplayResultsTextWhenSearchForText();
                DisplayPaginationText();
            };

            Button lastPageButton = new Button
            {
                Text = "Trang cuối",
                Enabled = currentPageText < totalPages,
                Width = 120,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lastPageButton.Click += (sender, e) =>
            {
                currentPageText = totalPages;
                flpTextPanel.Controls.Clear();
                DisplayResultsTextWhenSearchForText();
                DisplayPaginationText();
            };

            flpTextPanel.Controls.Add(firstPageButton);
            flpTextPanel.Controls.Add(prevButton);
            flpTextPanel.Controls.Add(nextButton);
            flpTextPanel.Controls.Add(lastPageButton);

            flpTextPanel.FlowDirection = FlowDirection.LeftToRight;
            flpTextPanel.Padding = new Padding(10);
            flpTextPanel.WrapContents = false;
        }
        private void DisplayPaginationTextAudio()
        {
            int totalPages = (int)Math.Ceiling((double)audioResultsForTextSearch.Count / itemsPerPage);

            flpAudioPanel.Controls.Clear();

            Button firstPageButton = new Button
            {
                Text = "Trang đầu",
                Enabled = currentPageTextAudio > 1,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            firstPageButton.Click += (sender, e) =>
            {
                currentPageTextAudio = 1;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForText();
                DisplayPaginationTextAudio();
            };

            Button prevButton = new Button
            {
                Text = "Trước",
                Enabled = currentPageTextAudio > 1,
                Width = 80,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            prevButton.Click += (sender, e) =>
            {
                currentPageTextAudio--;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForText();
                DisplayPaginationTextAudio();
            };

            Button nextButton = new Button
            {
                Text = "Tiếp theo",
                Enabled = currentPageTextAudio < totalPages,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            nextButton.Click += (sender, e) =>
            {
                currentPageTextAudio++;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForText();
                DisplayPaginationTextAudio();
            };

            Button lastPageButton = new Button
            {
                Text = "Trang cuối",
                Enabled = currentPageTextAudio < totalPages,
                Width = 120,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lastPageButton.Click += (sender, e) =>
            {
                currentPageTextAudio = totalPages;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForText();
                DisplayPaginationTextAudio();
            };

            flpAudioPanel.Controls.Add(firstPageButton);
            flpAudioPanel.Controls.Add(prevButton);
            flpAudioPanel.Controls.Add(nextButton);
            flpAudioPanel.Controls.Add(lastPageButton);

            flpAudioPanel.FlowDirection = FlowDirection.LeftToRight;
            flpAudioPanel.Padding = new Padding(10);
            flpAudioPanel.WrapContents = false;
        }
        private void DisplayPaginationTextImage()
        {
            int totalPages = (int)Math.Ceiling((double)imageResultsForTextSearch.Count / itemsPerPage);

            flpImagePanel.Controls.Clear();

            Button firstPageButton = new Button
            {
                Text = "Trang đầu",
                Enabled = currentPageTextImage > 1,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            firstPageButton.Click += (sender, e) =>
            {
                currentPageTextImage = 1;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForText();
                DisplayPaginationTextImage();
            };

            Button prevButton = new Button
            {
                Text = "Trước",
                Enabled = currentPageTextImage > 1,
                Width = 80,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            prevButton.Click += (sender, e) =>
            {
                currentPageTextImage--;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForText();
                DisplayPaginationTextImage();
            };

            Button nextButton = new Button
            {
                Text = "Tiếp theo",
                Enabled = currentPageTextImage < totalPages,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            nextButton.Click += (sender, e) =>
            {
                currentPageTextImage++;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForText();
                DisplayPaginationTextImage();
            };

            Button lastPageButton = new Button
            {
                Text = "Trang cuối",
                Enabled = currentPageTextImage < totalPages,
                Width = 120,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lastPageButton.Click += (sender, e) =>
            {
                currentPageTextImage = totalPages;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForText();
                DisplayPaginationTextImage();
            };

            flpImagePanel.Controls.Add(firstPageButton);
            flpImagePanel.Controls.Add(prevButton);
            flpImagePanel.Controls.Add(nextButton);
            flpImagePanel.Controls.Add(lastPageButton);

            flpImagePanel.FlowDirection = FlowDirection.LeftToRight;
            flpImagePanel.Padding = new Padding(10);
            flpImagePanel.WrapContents = false;
        }
        private void DisplayPaginationAudio()
        {
            int totalPages = (int)Math.Ceiling((double)audioResultsForAudioSearch.Count / itemsPerPage);

            flpAudioPanel.Controls.Clear();

            Button firstPageButton = new Button
            {
                Text = "Trang đầu",
                Enabled = currentPageAudio > 1,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            firstPageButton.Click += (sender, e) =>
            {
                currentPageAudio = 1;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForAudio();
                DisplayPaginationAudio();
            };

            Button prevButton = new Button
            {
                Text = "Trước",
                Enabled = currentPageAudio > 1,
                Width = 80,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            prevButton.Click += (sender, e) =>
            {
                currentPageAudio--;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForAudio();
                DisplayPaginationAudio();
            };

            Button nextButton = new Button
            {
                Text = "Tiếp theo",
                Enabled = currentPageAudio < totalPages,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            nextButton.Click += (sender, e) =>
            {
                currentPageAudio++;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForAudio();
                DisplayPaginationAudio();
            };

            Button lastPageButton = new Button
            {
                Text = "Trang cuối",
                Enabled = currentPageAudio < totalPages,
                Width = 120,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lastPageButton.Click += (sender, e) =>
            {
                currentPageAudio = totalPages;
                flpAudioPanel.Controls.Clear();
                DisplayResultsAudioWhenSearchForAudio();
                DisplayPaginationAudio();
            };

            flpAudioPanel.Controls.Add(firstPageButton);
            flpAudioPanel.Controls.Add(prevButton);
            flpAudioPanel.Controls.Add(nextButton);
            flpAudioPanel.Controls.Add(lastPageButton);

            flpAudioPanel.FlowDirection = FlowDirection.LeftToRight;
            flpAudioPanel.Padding = new Padding(10);
            flpAudioPanel.WrapContents = false;
        }
        private void DisplayPaginationImage()
        {
            int totalPages = (int)Math.Ceiling((double)imageResultsForImageSearch.Count / itemsPerPage);

            flpImagePanel.Controls.Clear();

            Button firstPageButton = new Button
            {
                Text = "Trang đầu",
                Enabled = currentPageImage > 1,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            firstPageButton.Click += (sender, e) =>
            {
                currentPageImage = 1;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForImage();
                DisplayPaginationImage();
            };

            Button prevButton = new Button
            {
                Text = "Trước",
                Enabled = currentPageImage > 1,
                Width = 80,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            prevButton.Click += (sender, e) =>
            {
                currentPageImage--;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForImage();
                DisplayPaginationImage();
            };

            Button nextButton = new Button
            {
                Text = "Tiếp theo",
                Enabled = currentPageImage < totalPages,
                Width = 100,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            nextButton.Click += (sender, e) =>
            {
                currentPageImage++;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForImage();
                DisplayPaginationImage();
            };

            Button lastPageButton = new Button
            {
                Text = "Trang cuối",
                Enabled = currentPageImage < totalPages,
                Width = 120,
                Height = 35,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lastPageButton.Click += (sender, e) =>
            {
                currentPageImage = totalPages;
                flpImagePanel.Controls.Clear();
                DisplayResultsImageWhenSearchForImage();
                DisplayPaginationImage();
            };

            flpImagePanel.Controls.Add(firstPageButton);
            flpImagePanel.Controls.Add(prevButton);
            flpImagePanel.Controls.Add(nextButton);
            flpImagePanel.Controls.Add(lastPageButton);

            flpImagePanel.FlowDirection = FlowDirection.LeftToRight;
            flpImagePanel.Padding = new Padding(10);
            flpImagePanel.WrapContents = false;
        }
        private void ShowTextRecordDetails(object sender)
        {
            LinkLabel linkLabel = sender as LinkLabel;
            if (linkLabel != null)
            {
                int recordId = (int)linkLabel.Tag;

                string query = "SELECT TieuDe, NoiDungTomTat, NoiDung FROM VanBan WHERE ID = @ID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", recordId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tieuDe = reader.GetString(0);
                                string noiDungTomTat = reader.GetString(1);
                                string noiDung = reader.GetString(2);

                                ShowTextDetailForm(tieuDe, noiDungTomTat, noiDung);
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy bản ghi.", "Lỗi");
                            }
                        }
                    }
                }
            }
        }
        private void ShowTextDetailForm(string tieuDe, string noiDungTomTat, string noiDung)
        {
            Form detailForm = new Form
            {
                Text = "Chi tiết bản ghi",
                Size = new Size(1400, 800),
                StartPosition = FormStartPosition.CenterScreen,
                AutoScroll = true
            };

            FlowLayoutPanel flowPanel = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(10)
            };

            Label titleLabel = new Label
            {
                Text = tieuDe,
                Font = new Font("Arial", 20, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.TopLeft,
                Width = flowPanel.Width - 20
            };

            Label summaryLabel = new Label
            {
                Text = $"{Environment.NewLine}{noiDungTomTat}",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.TopLeft,
                Width = flowPanel.Width - 20
            };

            Label contentLabel = new Label
            {
                Text = $"{Environment.NewLine}{noiDung}",
                Font = new Font("Arial", 12, FontStyle.Regular),
                AutoSize = true,
                TextAlign = ContentAlignment.TopLeft,
                Width = flowPanel.Width - 20
            };

            flowPanel.Controls.Add(titleLabel);
            flowPanel.Controls.Add(summaryLabel);
            flowPanel.Controls.Add(contentLabel);

            detailForm.Controls.Add(flowPanel);

            detailForm.ShowDialog();
        }
        // Cập nhật sự kiện xóa TextBox
        private void btnRemove_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        // Hàm thiết kế giao diện
        private void CustomizeUI()
        {
            // Tùy chỉnh TextBox
            textBox1.Font = new Font("Segoe UI", 12);
            textBox1.ForeColor = Color.Gray;
            textBox1.BackColor = Color.White;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Text = "Nhập nội dung...";
            textBox1.Enter += (s, e) => {
                if (textBox1.Text == "Nhập nội dung...")
                {
                    textBox1.Text = "";
                    textBox1.ForeColor = Color.Black;
                }
            };
            textBox1.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    textBox1.Text = "Nhập nội dung...";
                    textBox1.ForeColor = Color.Gray;
                }
            };

            // Tùy chỉnh Button Xóa
            btnRemove.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnRemove.BackColor = Color.LightCoral;
            btnRemove.ForeColor = Color.White;
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Cursor = Cursors.Hand;
            btnRemove.MouseEnter += (s, e) => btnRemove.BackColor = Color.Red;
            btnRemove.MouseLeave += (s, e) => btnRemove.BackColor = Color.LightCoral;
        }

        private void InitializeTimer()
        {
            timerRecording.Tick += (s, e) =>
            {
                TimeSpan elapsed = stopwatch.Elapsed;
                lblRecordingTime.Text = $"Thời gian: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            };
        }

        private void InitializeBlinkTimer()
        {
            blinkTimer.Tick += (s, e) =>
            {
                if (isBlinking)
                {
                    lblRecordingIndicator.Visible = !lblRecordingIndicator.Visible;
                }
            };
        }
    }
}
