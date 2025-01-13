using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMultiMedia
{
    internal class ImageComparisonCls
    {
        public static List<(int id, double distance_gabor, double distance_huMoment)> CompareImageToDatabase(string fileImage, string connectionString)
        {
            var (gabor, huMoments) = ExtractImageFeaturesAsync(fileImage, 0).Result;
            string selectQuery = "SELECT ID, Gabor, HuMoment FROM HinhAnh";
            List<(int id, double distance_gabor, double distance_huMoment)> similarities = new List<(int, double, double)>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string gaborFeatures = reader.GetString(1);
                                string huMomentsFeatures = reader.GetString(2);

                                // Chuyển chuỗi Gabor và HuMoment từ cơ sở dữ liệu thành danh sách các giá trị số
                                var gaborDb = gaborFeatures.Split(',').Select(double.Parse).ToList();
                                var huMomentsDb = huMomentsFeatures.Split(',').Select(double.Parse).ToList();

                                double distanceGabor = CalculateEuclideanDistance(gabor, gaborDb);
                                double distanceHuMoment = CalculateEuclideanDistance(huMoments, huMomentsDb);

                                //double cosineGabor = CalculateCosineSimilarity(gabor, gaborDb);
                                //double cosineHuMoment = CalculateCosineSimilarity(huMoments, huMomentsDb);

                                similarities.Add((id, distanceGabor, distanceHuMoment));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error comparing image to database: {ex.Message}");
                }
                return similarities;
            }
        }

        private static async Task<(List<double> gabor, List<double> huMoments)> ExtractImageFeaturesAsync(string fileImage, int id)
        {
            string gaborFile = "gabor_features.txt";
            string huMomentFile = "hu_moment_features.txt";

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

        private static double CalculateEuclideanDistance(List<double> vector1, List<double> vector2)
        {
            if (vector1.Count != vector2.Count)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            double sumOfSquares = 0;

            for (int i = 0; i < vector1.Count; i++)
            {
                sumOfSquares += Math.Pow(vector1[i] - vector2[i], 2);
            }

            return Math.Sqrt(sumOfSquares);
        }

        private static double CalculateCosineSimilarity(List<double> vector1, List<double> vector2)
        {
            if (vector1.Count != vector2.Count)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Count; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0; 
            }

            return dotProduct / (magnitude1 * magnitude2);
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
    }
}
