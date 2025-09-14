namespace MuzDost
{
    using System;
    class TaskManager
    {
        // main loop, download -> match -> calculate availability type -> save result
        // temporary serial sollution
        public async Task Process(Contractor[] contractors, string muzDataPath)
        {
            MuzData muzData = new MuzData(muzDataPath);

            // downloads all contractors in array
            Downloader downloader = new Downloader();
            await downloader.DownloadAll(contractors);

            Matcher matcher = new Matcher(muzData);
            
            foreach (Contractor contractor in contractors)
            {
                if (contractor.success)
                {
                    await Task.Run(() => contractor.LoadData());
                    await Task.Run(() => matcher.Match(contractor));
                }
            }

            Saver.SaveAll();
        }
    }
}