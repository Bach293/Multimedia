using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildDatabase
{
    internal class Audio_ChromaFeature_Cls
    {
        private const string AudioDirectory = "B:\\DPT\\SearchForTextByVoice\\audio\\";
        static Audio_ChromaFeature_Cls()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        private static async Task<List<double>> ExtractChromaFeaturesAsync(string fileAudio, int id)
        {
            string chromaFeatureFile = "chroma_features.txt";

            string para = $"chroma_features.py \"{fileAudio}\"";
            RunExe("python", para);

            if (File.Exists(chromaFeatureFile))
            {
                var chromas = GetFeaturesFromTextFile(chromaFeatureFile);
                File.Delete(chromaFeatureFile);  // Xoá tệp sau khi đọc
                return chromas;
            }
            else
            {
                throw new FileNotFoundException("Chroma feature file not found.");
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
        private static async Task InsertChromaFeaturesIntoDatabaseAsync(string connectionString, int id, List<double> chromas)
        {
            string query = "UPDATE AmThanh SET ChromaFeature = @ChromaFeature WHERE ID = @Id";
            string serializedChroma = SerializeFeatures(chromas);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ChromaFeature", serializedChroma);
                        command.Parameters.AddWithValue("@Id", id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while inserting chroma data into database: {ex.Message}");
            }
        }
        private static string SerializeFeatures(List<double> features)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(",", features));
            return sb.ToString();
        }
        public static async Task ProcessAndSaveChromaFeaturesAsync(string connectionString)
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
                                    var chromas = await ExtractChromaFeaturesAsync(filePath, id);
                                    await InsertChromaFeaturesIntoDatabaseAsync(connectionString, id, chromas);
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
