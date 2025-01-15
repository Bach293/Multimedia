using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms; 

namespace SearchMultiMedia
{
    internal class TextComparisonCls
    {
        public static List<(int id, double similarity)> CompareTextToDatabase(string inputText, string connectionString, ArrayList inputFeatures, double[] inputVector)
        {
            string selectQuery = "SELECT ID, TieuDeXLNNTN, NoiDungTomTatXLNNTN, NoiDungXLNNTN FROM VanBan";
            List<(int id, double similarity)> similarities = new List<(int, double)>();

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
                                string tieuDeXLNNTN = reader.GetString(1);
                                string noiDungTomTatXLNNTN = reader.GetString(2);
                                string noiDungXLNNTN = reader.GetString(3);

                                ArrayList fealstTieuDe = new ArrayList();
                                double[] tieuDeVector = DeserializeVector(tieuDeXLNNTN, ref fealstTieuDe);
                                ArrayList fealstNoiDungTomTat = new ArrayList();
                                double[] noiDungTomTatVector = DeserializeVector(noiDungTomTatXLNNTN, ref fealstNoiDungTomTat);
                                ArrayList fealstNoiDung = new ArrayList();
                                double[] noiDungVector = DeserializeVector(noiDungXLNNTN, ref fealstNoiDung);

                                ArrayList allFealst1 = SimilarWordCls.unifyFealst(inputFeatures, fealstTieuDe);
                                ArrayList allFealst2 = SimilarWordCls.unifyFealst(inputFeatures, fealstNoiDungTomTat);
                                ArrayList allFealst3 = SimilarWordCls.unifyFealst(inputFeatures, fealstNoiDung);

                                double similarityTitle = SimilarWordCls.calSimilarCosineAllFea(allFealst1, inputFeatures, fealstTieuDe, inputVector, tieuDeVector) * 100.0;
                                double similaritySummary = SimilarWordCls.calSimilarCosineAllFea(allFealst2, inputFeatures, fealstNoiDungTomTat, inputVector, noiDungTomTatVector) * 100.0;
                                double similarityContent = SimilarWordCls.calSimilarCosineAllFea(allFealst3, inputFeatures, fealstNoiDung, inputVector, noiDungVector) * 100.0;

                                double averageSimilarity = similarityTitle * 0.2 + similaritySummary * 0.3 + similarityContent * 0.5;

                                if (averageSimilarity > 0.0)
                                {
                                    similarities.Add((id, averageSimilarity));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to the database: {ex.Message}");
                }
            }
            return similarities;
        }
        public static List<(int id, double similarity)> CompareAudioTextToDatabase(string inputText, string connectionString, ArrayList inputFeatures, double[] inputVector)
        {
            string selectQuery = "SELECT ID, TieuDeXLNNTN, NoiDungTomTatXLNNTN, NoiDungXLNNTN FROM AmThanh";
            List<(int id, double similarity)> similarities = new List<(int, double)>();

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
                                string tieuDeXLNNTN = reader.GetString(1);
                                string noiDungTomTatXLNNTN = reader.GetString(2);
                                string noiDungXLNNTN = reader.GetString(3);

                                ArrayList fealstTieuDe = new ArrayList();
                                double[] tieuDeVector = DeserializeVector(tieuDeXLNNTN, ref fealstTieuDe);
                                ArrayList fealstNoiDungTomTat = new ArrayList();
                                double[] noiDungTomTatVector = DeserializeVector(noiDungTomTatXLNNTN, ref fealstNoiDungTomTat);
                                ArrayList fealstNoiDung = new ArrayList();
                                double[] noiDungVector = DeserializeVector(noiDungXLNNTN, ref fealstNoiDung);

                                ArrayList allFealst1 = SimilarWordCls.unifyFealst(inputFeatures, fealstTieuDe);
                                ArrayList allFealst2 = SimilarWordCls.unifyFealst(inputFeatures, fealstNoiDungTomTat);
                                ArrayList allFealst3 = SimilarWordCls.unifyFealst(inputFeatures, fealstNoiDung);

                                double similarityTitle = SimilarWordCls.calSimilarCosineAllFea(allFealst1, inputFeatures, fealstTieuDe, inputVector, tieuDeVector) * 100.0;
                                double similaritySummary = SimilarWordCls.calSimilarCosineAllFea(allFealst2, inputFeatures, fealstNoiDungTomTat, inputVector, noiDungTomTatVector) * 100.0;
                                double similarityContent = SimilarWordCls.calSimilarCosineAllFea(allFealst3, inputFeatures, fealstNoiDung, inputVector, noiDungVector) * 100.0;

                                double averageSimilarity = similarityTitle * 0.2 + similaritySummary * 0.3 + similarityContent * 0.5;

                                if (averageSimilarity > 0.0)
                                {
                                    similarities.Add((id, averageSimilarity));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to the database: {ex.Message}");
                }
            }
            return similarities;
        }
        public static List<(int id, double similarity)> CompareImageTextToDatabase(string inputText, string connectionString, ArrayList inputFeatures, double[] inputVector)
        {
            string selectQuery = "SELECT ID, TieuDeXLNNTN FROM HinhAnh";
            List<(int id, double similarity)> similarities = new List<(int, double)>();

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
                                string tieuDeXLNNTN = reader.GetString(1);

                                ArrayList fealstTieuDe = new ArrayList();
                                double[] tieuDeVector = DeserializeVector(tieuDeXLNNTN, ref fealstTieuDe);

                                ArrayList allFealst1 = SimilarWordCls.unifyFealst(inputFeatures, fealstTieuDe);

                                double similarityTitle = SimilarWordCls.calSimilarCosineAllFea(allFealst1, inputFeatures, fealstTieuDe, inputVector, tieuDeVector) * 100.0;

                                double averageSimilarity = similarityTitle;

                                if (averageSimilarity > 0.0)
                                {
                                    similarities.Add((id, averageSimilarity));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to the database: {ex.Message}");
                }
            }
            return similarities;
        }
        private static double[] DeserializeVector(string vectorString, ref ArrayList fealst)
        {
            string[] vectorArray = SimilarWordCls.getFeaArr(vectorString);
            SimilarWordCls.getFeature(vectorArray, ref fealst);
            double[] vector = new double[fealst.Count];

            SimilarWordCls.calVector(vectorArray, fealst, ref vector);

            return vector;
        }
    }
}
