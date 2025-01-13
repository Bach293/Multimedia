using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchMultiMedia
{
    internal class ControlCls
    {
        public static (ArrayList inputFeatures, double[] inputVector) PrepareTextInputData(string inputText)
        {
            ArrayList inputFeatures = new ArrayList();
            double[] inputVector = SimilarWordCls.getFeaVector2(inputText, ref inputFeatures);
            return (inputFeatures, inputVector);
        }
        public static List<(int id, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> GetTopTextSimilarRecords(
            (ArrayList inputFeatures, double[] inputVector) preparedData,
            string inputText,
            string connectionString)
        {
            var (inputFeatures, inputVector) = preparedData;
            var allTextSimilarRecords = TextComparisonCls.CompareTextToDatabase(inputText, connectionString, inputFeatures, inputVector);

            if (allTextSimilarRecords.Count == 0)
            {
                Console.WriteLine("No similar records found.");
            }
            else
            {
                Console.WriteLine($"Found {allTextSimilarRecords.Count} similar records.");
            }

            var topRecords = allTextSimilarRecords.OrderByDescending(record => record.similarity).ToList();

            List<(int id, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> result = new List<(int, string, string, string, double)>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var record in topRecords)
                {
                    Console.WriteLine($"Fetching details for ID {record.id}...");

                    string selectQuery = "SELECT TieuDe, NoiDungTomTat, NoiDung FROM VanBan WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", record.id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tieuDe = reader.GetString(0);
                                string noiDungTomTat = reader.GetString(1);
                                string noiDung = reader.GetString(2);

                                result.Add((record.id, tieuDe, noiDungTomTat, noiDung, record.similarity));
                            }
                        }
                    }
                }
            }

            return result;
        }
        public static List<(int id, string tenFile, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> GetTopAudioSimilarRecords(
            (ArrayList inputFeatures, double[] inputVector) preparedData,
            string inputText,
            string connectionString)
        {
            var (inputFeatures, inputVector) = preparedData;

            var allAudioTextSimilarRecords = TextComparisonCls.CompareAudioTextToDatabase(inputText, connectionString, inputFeatures, inputVector);

            if (allAudioTextSimilarRecords.Count == 0)
            {
                Console.WriteLine("No similar audio records found.");
                return new List<(int id, string tenFile, string tieuDe, string noiDungTomTat, string noiDung, double similarity)>();
            }

            Console.WriteLine($"Found {allAudioTextSimilarRecords.Count} similar audio records.");

            var topRecords = allAudioTextSimilarRecords.OrderByDescending(record => record.similarity).ToList();

            List<(int id, string tenFile, string tieuDe, string noiDungTomTat, string noiDung, double similarity)> result = new();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var record in topRecords)
                {
                    Console.WriteLine($"Fetching details for ID {record.id}...");

                    string selectQuery = "SELECT TenFile, TieuDe, NoiDungTomTat, NoiDung FROM AmThanh WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", record.id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tenFile = reader.GetString(0);
                                string tieuDe = reader.GetString(1);
                                string noiDungTomTat = reader.GetString(2);
                                string noiDung = reader.GetString(3);

                                result.Add((record.id, tenFile, tieuDe, noiDungTomTat, noiDung, record.similarity));
                            }
                        }
                    }
                }
            }

            return result;
        }
        public static List<(int id, string tenFile, string tieuDe, double similarity)> GetTopImageSimilarRecords(
            (ArrayList inputFeatures, double[] inputVector) preparedData,
            string inputText,
            string connectionString)
        {
            var (inputFeatures, inputVector) = preparedData;

            var allImageTextSimilarRecords = TextComparisonCls.CompareImageTextToDatabase(inputText, connectionString, inputFeatures, inputVector);

            if (allImageTextSimilarRecords.Count == 0)
            {
                Console.WriteLine("No similar image records found.");
                return new List<(int id, string tenFile, string tieuDe, double similarity)>();
            }

            Console.WriteLine($"Found {allImageTextSimilarRecords.Count} similar image records.");

            var topRecords = allImageTextSimilarRecords.OrderByDescending(record => record.similarity).ToList();

            List<(int id, string tenFile, string tieuDe, double similarity)> result = new();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var record in topRecords)
                {
                    Console.WriteLine($"Fetching details for ID {record.id}...");

                    string selectQuery = "SELECT TenFile, TieuDe FROM HinhAnh WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", record.id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tenFile = reader.GetString(0);
                                string tieuDe = reader.GetString(1);

                                result.Add((record.id, tenFile, tieuDe, record.similarity));
                            }
                        }
                    }
                }
            }

            return result;
        }
        public static List<(int id, string tenFile, string tieuDe, double distance)> GetTopImageDistanceRecords(
            string fileImage,
            string connectionString)
        {
            var imageDistanceRecords = ImageComparisonCls.CompareImageToDatabase(fileImage, connectionString);
            if (imageDistanceRecords.Count == 0)
            {
                Console.WriteLine("No similar image records found.");
                return new List<(int id, string tenFile, string tieuDe, double distance)>();
            }
            Console.WriteLine($"Found {imageDistanceRecords.Count} similar image records.");
            var topRecords = imageDistanceRecords
                .OrderBy(record => 1 * record.distance_gabor + 1 * record.distance_huMoment)
                .ToList();
            List<(int id, string tenFile, string tieuDe, double distance)> result = new();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var record in topRecords)
                {
                    Console.WriteLine($"Fetching details for ID {record.id}...");
                    string selectQuery = "SELECT TenFile, TieuDe FROM HinhAnh WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", record.id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tenFile = reader.GetString(0);
                                string tieuDe = reader.GetString(1);
                                double distance = 1 * record.distance_gabor + 1 * record.distance_huMoment;
                                result.Add((record.id, tenFile, tieuDe, distance));
                            }
                        }
                    }
                }
            }
            return result;
        }
        public static List<(int id, string tenFile, string tieuDe, string noiDungTomTat, double distance)> GetTopAudioDistanceRecords(
            string fileAudio,
            string connectionString)
        {
            var audioDistanceRecords = AudioComparisonCls.CompareAudioToDatabase(fileAudio, connectionString);
            if (audioDistanceRecords.Count == 0)
            {
                MessageBox.Show("No similar audio records found.");
                return new List<(int id, string tenFile, string tieuDe, string noiDungTomTat, double distance)>();
            }
            Console.WriteLine($"Found {audioDistanceRecords.Count} similar audio records.");
            var topRecords = audioDistanceRecords
                .OrderBy(record => 0.5 * record.distance_spectral_bandwidths + 0.5 * record.distance_spectral_centroid)
                .ToList();
            List<(int id, string tenFile, string tieuDe, string noiDungTomTat, double distance)> result = new();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var record in topRecords)
                {
                    Console.WriteLine($"Fetching details for ID {record.id}...");
                    string selectQuery = "SELECT TenFile, TieuDe, NoiDungTomTat FROM AmThanh WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", record.id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string tenFile = reader.GetString(0);
                                string tieuDe = reader.GetString(1);
                                string noiDungTomTat = reader.GetString(2);
                                double distance = 0.5 * record.distance_spectral_bandwidths + 0.5 * record.distance_spectral_centroid;
                                result.Add((record.id, tenFile, tieuDe, noiDungTomTat, distance));
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
