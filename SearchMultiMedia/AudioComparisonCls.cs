using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMultiMedia
{
    internal class AudioComparisonCls
    {
        public static List<(int id, double distance_spectral_centroid, double distance_spectral_bandwidths, double distance_mfcc)> CompareAudioToDatabase(string fileAudio, string connectionString)
        {
            var (centroids, bandwidths) = ExtractSpectralFeatures(fileAudio, 0);
            //var mfccFeatures = ExtractMFCCFeatures(fileAudio, 0);
            //var sequenceToCompareMfcc = MFCC.ExtractSequenceFromMFCC(mfccFeatures);

            string selectQuery = "SELECT ID, SpectralCentroid, SpectralBandwidth, MFCC FROM AmThanh";
            List<(int id, double distance_spectral_centroid, double distance_spectral_bandwidths, double distance_mfcc)> similarities = new List<(int, double, double, double)>();
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

                                string centroidFeatures = reader.GetString(1);
                                string bandwidthFeatures = reader.GetString(2);
                                string mfccFeaturesDb = reader.GetString(3);

                                var centroidsDb = centroidFeatures.Split(',').Select(double.Parse).ToList();
                                var bandwidthsDb = bandwidthFeatures.Split(',').Select(double.Parse).ToList();

                                //var mfccFeaturesDbList = GetMFCCFromDB(mfccFeaturesDb);
                                //var sequenceDbMfcc = MFCC.ExtractSequenceFromMFCC(mfccFeaturesDbList);

                                //double distanceMfcc = DTW_Distance_Standard(sequenceToCompareMfcc, sequenceDbMfcc);
                                double distanceSpectralCentroid = CalculateDTW(centroids.ToArray(), centroidsDb.ToArray());
                                double distanceSpectralBandwidths = CalculateDTW(bandwidths.ToArray(), bandwidthsDb.ToArray());

                                double distanceMfcc = 0.0;

                                similarities.Add((id, distanceSpectralCentroid, distanceSpectralBandwidths, distanceMfcc));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error comparing audio to database: {ex.Message}");
                }
            }

            return similarities;
        }
        private static (List<double> centroids, List<double> bandwidths) ExtractSpectralFeatures(string fileAudio, int id)
        {
            string centroidFile = Path.Combine(@"C:\\Users\\Admin\\source\\repos\\SearchMultiMedia\\SearchMultiMedia\\bin\\Debug\\net8.0-windows", $"spectral_features_centroid.txt");
            string bandwidthFile = Path.Combine(@"C:\\Users\\Admin\\source\\repos\\SearchMultiMedia\\SearchMultiMedia\\bin\\Debug\\net8.0-windows", $"spectral_features_bandwidths.txt");

            string para = $"spectral_features.py \"{fileAudio}\"";
            RunExe("python", para);  // RunExe cần được sửa để chạy đồng bộ

            if (File.Exists(centroidFile) && File.Exists(bandwidthFile))
            {
                var centroids = GetFeaturesFromTextFile(centroidFile);
                var bandwidths = GetFeaturesFromTextFile(bandwidthFile);
                File.Delete(centroidFile);
                File.Delete(bandwidthFile);
                return (centroids, bandwidths);
            }
            else
            {
                throw new FileNotFoundException("Spectral feature files not found.");
            }
        }
        private static List<List<double>> ExtractMFCCFeatures(string fileAudio, int id)
        {
            string resultFile = Path.Combine(@"C:\\Users\\Admin\\source\\repos\\SearchMultiMedia\\SearchMultiMedia\\bin\\Debug\\net8.0-windows", $"mfcc.txt");

            string para = $"mfcc.py \"{fileAudio}\"";
            RunExe("python", para);  // RunExe cần được sửa để chạy đồng bộ

            if (File.Exists(resultFile))
            {
                var mfccFeatures = GetMFCCFromTextFile(resultFile);
                File.Delete(resultFile);
                return mfccFeatures;
            }
            else
            {
                throw new FileNotFoundException("MFCC result file not found.");
            }
        }
        public class MFCCFrame
        {
            public double[] Features = new double[13];
        }
        public class Sequence
        {
            public MFCCFrame[] Frames { get; set; }
        }
        public class MFCC
        {
            public static Sequence ExtractSequenceFromMFCC(List<List<double>> mfccFeatures)
            {
                Sequence sequence = new Sequence();
                int numOfFrames = mfccFeatures.Count;
                sequence.Frames = new MFCCFrame[numOfFrames];

                for (int i = 0; i < numOfFrames; i++)
                {
                    sequence.Frames[i] = new MFCCFrame();
                    int numOfCoefficients = mfccFeatures[i].Count;
                    Debug.Assert(numOfCoefficients == 13);
                    for (int j = 0; j < numOfCoefficients; j++)
                    {
                        sequence.Frames[i].Features[j] = mfccFeatures[i][j];
                    }
                }

                return sequence;
            }
        }
        public static double DTW_Distance_Standard(Sequence sequence1, Sequence sequence2)
        {
            int numberOfFrames_Sequence1 = sequence1.Frames.Length;
            int numberOfFrames_Sequence2 = sequence2.Frames.Length;

            double[,] DTW = new double[numberOfFrames_Sequence1, numberOfFrames_Sequence2];

            DTW[0, 0] = distance(sequence1.Frames[0], sequence2.Frames[0]);

            for (int i = 1; i < numberOfFrames_Sequence1; i++)
            {
                DTW[i, 0] = DTW[i - 1, 0] + distance(sequence1.Frames[i], sequence2.Frames[0]);
            }
            for (int j = 1; j < numberOfFrames_Sequence2; j++)
            {
                DTW[0, j] = DTW[0, j - 1] + distance(sequence1.Frames[0], sequence2.Frames[j]);
            }

            double d;
            for (int i = 1; i < numberOfFrames_Sequence1; i++)
            {
                for (int j = 1; j < numberOfFrames_Sequence2; j++)
                {
                    d = distance(sequence1.Frames[i], sequence2.Frames[j]);
                    DTW[i, j] = Math.Min(DTW[i - 1, j - 1], DTW[i, j - 1]);
                    DTW[i, j] = Math.Min(DTW[i, j], DTW[i - 1, j]);
                    DTW[i, j] += d;
                }
            }

            d = DTW[numberOfFrames_Sequence1 - 1, numberOfFrames_Sequence2 - 1];
            Array.Clear(DTW, 0, DTW.Length);

            return d;
        }
        private static double distance(MFCCFrame frame1, MFCCFrame frame2)
        {
            double difference_distance = 0;
            for (int i = 0; i < 13; i++)
            {
                difference_distance += Math.Pow(frame1.Features[i] - frame2.Features[i], 2);
            }
            return Math.Sqrt(difference_distance);
        }
        public static double CalculateDTW(double[] vector1, double[] vector2)
        {
            int n = vector1.Length;
            int m = vector2.Length;

            double[,] dtw = new double[n + 1, m + 1];

            for (int i = 0; i <= n; i++)
                dtw[i, 0] = double.PositiveInfinity;
            for (int j = 0; j <= m; j++)
                dtw[0, j] = double.PositiveInfinity;

            dtw[0, 0] = 0;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    double cost = Math.Abs(vector1[i - 1] - vector2[j - 1]);

                    dtw[i, j] = cost + Math.Min(
                        Math.Min(dtw[i - 1, j], dtw[i, j - 1]),
                        dtw[i - 1, j - 1]
                    );
                }
            }

            return dtw[n, m];
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
        public static List<List<double>> GetMFCCFromDB(string fileMFCC)
        {
            List<List<double>> lstLstMFCC = new List<List<double>>();

            var lines = fileMFCC.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string[] strings = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (strings.Length == 13)
                {
                    List<double> lstMFCC = new List<double>();

                    foreach (string s in strings)
                    {
                        lstMFCC.Add(double.Parse(s.Trim()));
                    }

                    lstLstMFCC.Add(lstMFCC);
                }
                else
                {
                    Console.WriteLine($"Warning: Skipping invalid MFCC data in line: {line}");
                }
            }

            return lstLstMFCC;
        }
    }
}
