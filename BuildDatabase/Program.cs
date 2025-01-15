using BuildDatabase;
using System;
class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "Data Source=LAPTOP-79T4Q5ET\\BACH;Initial Catalog=DPT;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        //await TextCls.ProcessAndSaveDataAsync(connectionString);

        //await ImageTextCls.ProcessAndSaveDataAsync(connectionString);

        //await AudioTextCls.ProcessAndSaveDataAsync(connectionString);

        //await Audio_MFCC_Cls.ProcessAndSaveMFCCDataAsync(connectionString);

        //await Audio_SpectralFeatures_Cls.ProcessAndSaveSpectralFeaturesAsync(connectionString);

        //await ImageCls.ProcessAndSaveImageDataAsync(connectionString);

        //await Image_Features_Cls.ProcessAndSaveImageFeaturesAsync(connectionString);

        await Audio_ChromaFeature_Cls.ProcessAndSaveChromaFeaturesAsync(connectionString);
        Console.WriteLine("Processing completed successfully!");
    }
}
