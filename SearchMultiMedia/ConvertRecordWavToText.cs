
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMultiMedia
{
    internal class ConvertRecordWavToText
    {
        public static string GetTextFromRecordWav(string fileAudio)
        {
            string para = $"ConvertAudioToText.py {fileAudio}";
            string text = RunExe("python", para);

            if (string.IsNullOrEmpty(text))
            {
                throw new Exception("No voice recognition or no results from Python script.");
            }

            return text;
        }


        private static string RunExe(string fileExe, string para)
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

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception($"Python script error: {error}");
                    }

                    return output.Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Python script failed: {ex.Message}");
                throw new Exception("Error running Python script: " + ex.Message, ex);
            }
        }

    }
}
