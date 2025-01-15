using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BuildDatabase
{
    internal class ImageCls
    {
        private const string ImageDirectory = "B:\\DPT\\SearchForTextByVoice\\image"; 

        static ImageCls()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        // Hàm đọc giá trị biểu đồ màu và biểu đồ màu tích lũy
        private static void CalculateColorHistograms(string imagePath, int n, out int[] hr, out int[] hg, out int[] hb, out int[] tr, out int[] tg, out int[] tb)
        {
            hr = new int[n];
            hg = new int[n];
            hb = new int[n];
            tr = new int[n];
            tg = new int[n];
            tb = new int[n];
            for (int i = 0; i < n; i++)
            {
                hr[i] = 0;
                hg[i] = 0;
                hb[i] = 0;
                tr[i] = 0;
                tg[i] = 0;
                tb[i] = 0;
            }

            Bitmap bmp = new Bitmap(imagePath);
            for (int row = 0; row < bmp.Height; row++)
            {
                for (int col = 0; col < bmp.Width; col++)
                {
                    Color pixelColor = bmp.GetPixel(col, row);
                    hr[pixelColor.R]++;
                    hg[pixelColor.G]++;
                    hb[pixelColor.B]++;
                }
            }

            // Tính biểu đồ màu tích lũy
            tr[0] = hr[0];
            tg[0] = hg[0];
            tb[0] = hb[0];
            for (int i = 1; i < n; i++)
            {
                tr[i] = tr[i - 1] + hr[i];
                tg[i] = tg[i - 1] + hg[i];
                tb[i] = tb[i - 1] + hb[i];
            }

            bmp.Dispose();
        }

        // Hàm tuần tự hóa biểu đồ màu thành chuỗi
        private static string SerializeColorHistogram(int n, int[] hr, int[] hg, int[] hb)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                sb.AppendLine($"{i}: {hr[i]}, {hg[i]}, {hb[i]}");
            }
            return sb.ToString();
        }

        // Hàm tuần tự hóa biểu đồ màu tích lũy thành chuỗi
        private static string SerializeCumulativeColorHistogram(int n, int[] tr, int[] tg, int[] tb)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                sb.AppendLine($"{i}: {tr[i]}, {tg[i]}, {tb[i]}");
            }
            return sb.ToString();
        }

        // Hàm tuần tự hóa LBP thành chuỗi
        private static string SerializeLBP(Bitmap lbpImage)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < lbpImage.Height; y++)
            {
                for (int x = 0; x < lbpImage.Width; x++)
                {
                    Color pixelColor = lbpImage.GetPixel(x, y);
                    sb.Append($"{pixelColor.R} "); // Assuming grayscale LBP
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static async Task InsertDataIntoDatabaseAsync(string connectionString, int id, string colorHistogram, string cumulativeColorHistogram, string lbpData)
        {
            string query = "UPDATE HinhAnh SET BieuDoMau = @ColorHistogram, BieuDoMauTichLuy = @CumulativeColorHistogram, NhiPhanCucBo = @LbpData WHERE ID = @Id";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ColorHistogram", colorHistogram);
                        command.Parameters.AddWithValue("@CumulativeColorHistogram", cumulativeColorHistogram);
                        command.Parameters.AddWithValue("@LbpData", lbpData);
                        command.Parameters.AddWithValue("@Id", id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while inserting data into database: {ex.Message}");
            }
        }

        public static async Task ProcessAndSaveImageDataAsync(string connectionString)
        {
            string selectQuery = "SELECT ID, TenFile FROM HinhAnh";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            int recordCount = 0;

                            while (await reader.ReadAsync())
                            {
                                int id = reader.GetInt32(0);
                                string fileName = reader.GetString(1);
                                string filePath = Path.Combine(ImageDirectory, fileName);
                                Console.WriteLine($"Processing file: {filePath}");

                                try
                                {
                                    int n = 256;
                                    int[] hr, hg, hb, tr, tg, tb;
                                    CalculateColorHistograms(filePath, n, out hr, out hg, out hb, out tr, out tg, out tb);

                                    string colorHistogram = SerializeColorHistogram(n, hr, hg, hb);
                                    string cumulativeColorHistogram = SerializeCumulativeColorHistogram(n, tr, tg, tb);

                                    string lbpData = await RunPythonScriptLBP(filePath);  

                                    await InsertDataIntoDatabaseAsync(connectionString, id, colorHistogram, cumulativeColorHistogram, lbpData);
                                    recordCount++;
                                    Console.WriteLine($"Processed record {id} successfully.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error processing record {id}: {ex.Message}");
                                }
                            }

                            Console.WriteLine($"Processed {recordCount} records in total.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in processing records: {ex.Message}");
            }
        }
        static async Task<string> RunPythonScriptLBP(string imagePath)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo()
                {
                    FileName = "python",  
                    Arguments = $"\"B:\\DPT\\SearchForTextByVoice\\lbp.py\" \"{imagePath}\"", 
                    RedirectStandardOutput = true,
                    RedirectStandardError = true, 
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(start))
                {
                    using (System.IO.StreamReader reader = process.StandardOutput)
                    using (System.IO.StreamReader errorReader = process.StandardError) 
                    {
                        string result = await reader.ReadToEndAsync();
                        string error = await errorReader.ReadToEndAsync();

                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception($"Python script error: {error}"); 
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred while running the Python script: {ex.Message}"; 
            }
        }
    }
}
