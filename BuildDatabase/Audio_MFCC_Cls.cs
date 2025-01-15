using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BuildDatabase
{
    internal class Audio_MFCC_Cls
    {
        private const string AudioDirectory = "B:\\DPT\\SearchForTextByVoice\\audio\\"; // Thư mục chứa file âm thanh

        static Audio_MFCC_Cls()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        // Hàm đọc đặc trưng MFCC từ tệp âm thanh và trả về danh sách đặc trưng
        private static async Task<List<List<double>>> ExtractMFCCFeaturesAsync(string fileAudio, int id)
        {
            string resultFile = Path.Combine(@"C:\Users\Admin\source\repos\BuildDatabase\BuildDatabase\bin\Debug\net8.0", $"mfcc.txt");

            string para = $"mfcc.py \"{fileAudio}\"";
            RunExe("python", para);

            if (File.Exists(resultFile))
            {
                var mfccFeatures = MFCC.GetMFCCFromTextFile(resultFile);
                File.Delete(resultFile); 
                return mfccFeatures;
            }
            else
            {
                throw new FileNotFoundException("MFCC result file not found.");
            }
        }

        // Hàm để chạy file thực thi
        private static void RunExe(string fileExe, string para)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = fileExe,
                    Arguments = para,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    if (!process.WaitForExit(120000)) // Thời gian chờ tối đa 2 phút
                    {
                        process.Kill();
                        throw new TimeoutException("Process execution timed out.");
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error:");
                        Console.WriteLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running external process: " + ex.Message);
            }
        }

        // Hàm để chèn kết quả MFCC vào cơ sở dữ liệu SQL Server
        private static async Task InsertMFCCIntoDatabaseAsync(string connectionString, int id, List<List<double>> mfccFeatures)
        {
            string query = "UPDATE AmThanh SET MFCC = @MFCCFeatures WHERE ID = @Id";
            string serializedFeatures = SerializeMFCCFeatures(mfccFeatures);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MFCCFeatures", serializedFeatures);
                        command.Parameters.AddWithValue("@Id", id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while inserting MFCC data into database: {ex.Message}");
            }
        }

        // Hàm tuần tự hóa danh sách MFCC thành chuỗi
        private static string SerializeMFCCFeatures(List<List<double>> mfccFeatures)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var frame in mfccFeatures)
            {
                sb.AppendLine(string.Join(",", frame));
            }
            return sb.ToString();
        }

        // Hàm xử lý các bản ghi từ cơ sở dữ liệu và tính toán MFCC
        public static async Task ProcessAndSaveMFCCDataAsync(string connectionString)
        {
            string selectQuery = "SELECT ID, TenFile FROM AmThanh";

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
                                string filePath = Path.Combine(AudioDirectory, fileName);
                                Console.WriteLine($"Processing file: {filePath}");

                                try
                                {
                                    var mfccFeatures = await ExtractMFCCFeaturesAsync(filePath, id);
                                    await InsertMFCCIntoDatabaseAsync(connectionString, id, mfccFeatures);
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
    }

    public class MFCC
    {
        public static List<List<double>> GetMFCCFromTextFile(string fileMFCC)
        {
            StreamReader sr = new StreamReader(fileMFCC);
            string line = sr.ReadLine();
            List<List<double>> lstLstMFCC = new List<List<double>>();
            while (line != null)
            {
                string[] strings = line.Split(new char[] { ' ' });
                if (strings.Length == 13)
                {
                    List<double> lstMFCC = new List<double>();
                    foreach (string s in strings)
                    {
                        lstMFCC.Add(double.Parse(s));
                    }
                    lstLstMFCC.Add(lstMFCC);
                }
                line = sr.ReadLine();
            }
            sr.Close();
            return lstLstMFCC;
        }
    }
}
