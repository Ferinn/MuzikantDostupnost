namespace MuzDost
{
    using System;
    using System.Net.Http;
    public class Downloader
    {
        public async Task DownloadAll(Contractor[] contractors)
        {
            List<Task<bool>> downloadTasks = new List<Task<bool>>(contractors.Length);
            bool[] taskSuccess = new bool[contractors.Length];
            int successCount = 0;

            // starting all Downloads
            for (int i = 0; i < contractors.Length; i++)
            {
                if (contractors[i].process.UrlAdress != "" && contractors[i].process.IsXML == 1)
                {
                    Console.WriteLine($"MainForm has started downloading file from {contractors[i].process.UrlAdress}");
                }
                else if (contractors[i].process.LocalAdress != "" && contractors[i].process.IsXML == 0)
                {
                    Console.WriteLine($"MainForm has started downloading file from {contractors[i].process.LocalAdress}");
                    CSVConvertor CSVConvertor = new CSVConvertor();
                    contractors[i].xmlDataPath = "..\\Downloads\\" + contractors[i].name + ".xml";

                    taskSuccess[i] = false;
                    downloadTasks.Add(CSVConvertor.ConvertAsync(contractors[i].process.LocalAdress, 
                                                                contractors[i].xmlDataPath,
                                                                contractors[i].process));
                    continue;
                }
                else
                {
                    Console.WriteLine($"Neither UrlAdress or LocalAdress is set for {contractors[i].name}, aborting processing.");
                    taskSuccess[i] = false;
                    continue;
                }
                taskSuccess[i] = false;
                downloadTasks.Add(Download(contractors[i]));
            }
            downloadTasks.TrimExcess();

            // awaiting their completion
            try
            {
                taskSuccess = await Task.WhenAll(downloadTasks);
            }
            // executed once all tasks are completed (successfully or else)
            finally
            {
                for (int i = 0; i < taskSuccess.Length; i++)
                {
                    if (taskSuccess[i])
                    {
                        contractors[i].success = true;
                        successCount++;
                    }
                    else
                    {
                        Console.WriteLine($"Contractor {contractors[i].name}'s sourcing has failed!");
                    }
                }
            }
            Console.WriteLine($"Finished downloading provided contractor data, {successCount} out of {contractors.Length} operations have been succesfull.");
        }

        private async Task<bool> Download(Contractor contractor)
        {
            // preparing path into download dir under contractors name
            string _pathToSave = Program.downloadDir + contractor.name + ".xml";
            contractor.xmlDataPath = _pathToSave;
            
            // preparing web client
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (HttpClient httpClient = new HttpClient(clientHandler))
            {
                try
                {
                    // asking for data from URL
                    HttpResponseMessage response = await httpClient.GetAsync(contractor.process.UrlAdress);
                    response.EnsureSuccessStatusCode();
                    using Stream contentStream = await response.Content.ReadAsStreamAsync();

                    // saving data to file
                    using FileStream fileStream = File.Create(_pathToSave);
                    await contentStream.CopyToAsync(fileStream);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An Error has been caught while downloading: {contractor.name} from URL: {contractor.process.UrlAdress}.\n" +
                                        $"{ex.Message}");
                    return false;
                }
            }
        }
    }
}