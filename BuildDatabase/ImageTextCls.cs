using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildDatabase
{
    internal class ImageTextCls
    {
        private const int MaxConcurrentTasks = 4; // Giới hạn số lượng tác vụ chạy đồng thời
        private const int TimeoutMilliseconds = 120000;

        // Hàm chạy batch file và trả về kết quả
        private static async Task<string> RunTokenizerBatchAsync(string inputText)
        {
            string inputFile = Path.GetTempFileName();
            File.WriteAllText(inputFile, inputText, Encoding.UTF8);

            string outputFile = Path.GetTempFileName();
            string arguments = $"-i {inputFile} -o {outputFile}";

            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeoutMilliseconds))
                {
                    await RunExeAsync("B:\\Git\\xlnntn\\vnTokenizer.bat", arguments, cts.Token);
                }

                await Task.Delay(100); // Nghỉ ms sau mỗi batch để giảm tải CPU
                return File.ReadAllText(outputFile, Encoding.UTF8);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Process exceeded timeout limit (30s). Skipping...");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while running tokenizer batch: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                File.Delete(inputFile);
                File.Delete(outputFile);
            }
        }

        // Hàm để chạy file thực thi
        private static async Task RunExeAsync(string fileexe, string parameters, CancellationToken cancellationToken)
        {
            if (!File.Exists(fileexe))
            {
                throw new FileNotFoundException($"Batch file does not exist: {fileexe}");
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = fileexe,
                Arguments = parameters,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.PriorityClass = ProcessPriorityClass.BelowNormal; // Giảm mức ưu tiên của tiến trình

                Task waitForExit = Task.Run(() => process.WaitForExit(), cancellationToken);
                if (await Task.WhenAny(waitForExit, Task.Delay(TimeoutMilliseconds, cancellationToken)) == waitForExit)
                {
                    // Process completed within timeout.
                    await waitForExit;
                }
                else
                {
                    // Timeout occurred.
                    try
                    {
                        process.Kill();
                    }
                    catch { }

                    throw new OperationCanceledException("Process exceeded timeout limit.", cancellationToken);
                }
            }
        }

        // Hàm để chèn kết quả vào cơ sở dữ liệu SQL Server
        private static async Task InsertResultIntoDatabaseAsync(string connectionString, int id, string tieuDeXLNNTN)
        {
            string query = "UPDATE HinhAnh SET TieuDeXLNNTN = @TieuDeXLNNTN WHERE ID = @Id";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TieuDeXLNNTN", tieuDeXLNNTN);
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

        // Hàm xử lý các bản ghi từ cơ sở dữ liệu và chạy vnTokenizer đồng thời
        public static async Task ProcessAndSaveDataAsync(string connectionString)
        {
            string selectQuery = "SELECT ID, TieuDe FROM HinhAnh";
            SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrentTasks);

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<Task> tasks = new List<Task>();
                            int recordCount = 0;

                            while (await reader.ReadAsync())
                            {
                                int id = reader.GetInt32(0);
                                string tieuDe = reader.GetString(1);

                                await semaphore.WaitAsync(); // Giới hạn số lượng tác vụ đồng thời

                                tasks.Add(Task.Run(async () =>
                                {
                                    try
                                    {
                                        string tieuDeXLNNTN = await RunTokenizerBatchAsync(tieuDe);

                                        await InsertResultIntoDatabaseAsync(connectionString, id, tieuDeXLNNTN.ToLower());
                                        Console.WriteLine($"Processed record {Interlocked.Increment(ref recordCount)} successfully.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error processing record {id}: {ex.Message}");
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }));
                            }

                            // Chờ tất cả các tác vụ hoàn thành
                            await Task.WhenAll(tasks);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in processing records: {ex.Message}");
            }
            finally
            {
                semaphore.Dispose();
            }
        }
    }
}
