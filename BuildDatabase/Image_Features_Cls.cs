using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BuildDatabase
{
    internal class Image_Features_Cls
    {
        private const string ImageDirectory = "B:\\DPT\\SearchForTextByVoice\\image\\";

        static Image_Features_Cls()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        private static async Task<(List<double> gabor, List<double> huMoments)> ExtractImageFeaturesAsync(string fileImage, int id)
        {
            string gaborFile = Path.Combine(@"C:\\Users\\Admin\\source\\repos\\BuildDatabase\\BuildDatabase\\bin\\Debug\\net8.0", $"gabor_features.txt");
            string huMomentFile = Path.Combine(@"C:\\Users\\Admin\\source\\repos\\BuildDatabase\\BuildDatabase\\bin\\Debug\\net8.0", $"hu_moment_features.txt");

            string para = $"image_features.py \"{fileImage}\"";
            RunExe("python", para);

            if (File.Exists(gaborFile) && File.Exists(huMomentFile))
            {
                var gabor = GetFeaturesFromTextFile(gaborFile);
                var huMoments = GetFeaturesFromTextFile(huMomentFile);
                File.Delete(gaborFile);
                File.Delete(huMomentFile);
                return (gabor, huMoments);
            }
            else
            {
                throw new FileNotFoundException("Image feature files not found.");
            }
        }

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

                    if (!process.WaitForExit(120000))
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

        private static List<double> GetFeaturesFromTextFile(string filePath)
        {
            var features = new List<double>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (double.TryParse(line, out double feature))
                        {
                            features.Add(feature);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Skipping invalid feature value in line: {line}");
                        }
                    }
                }

                if (features.Count == 0)
                {
                    Console.WriteLine("No valid feature data found in the file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading features from file: {ex.Message}");
            }

            return features;
        }

        private static async Task InsertImageFeaturesIntoDatabaseAsync(string connectionString, int id, List<double> gabor, List<double> huMoments)
        {
            string query = "UPDATE HinhAnh SET Gabor = @Gabor, HuMoment = @HuMoment WHERE ID = @Id";
            string serializedGabor = SerializeFeatures(gabor);
            string serializedHuMoments = SerializeFeatures(huMoments);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Gabor", serializedGabor);
                        command.Parameters.AddWithValue("@HuMoment", serializedHuMoments);
                        command.Parameters.AddWithValue("@Id", id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while inserting image data into database: {ex.Message}");
            }
        }

        private static string SerializeFeatures(List<double> features)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(",", features));
            return sb.ToString();
        }

        public static async Task ProcessAndSaveImageFeaturesAsync(string connectionString)
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
                                    var (gabor, huMoments) = await ExtractImageFeaturesAsync(filePath, id);
                                    await InsertImageFeaturesIntoDatabaseAsync(connectionString, id, gabor, huMoments);
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
}
