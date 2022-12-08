using BugTracker.Services.Interfaces;


namespace BugTracker.Services
{
    public class FileService : IFileService
    {
        
        private readonly string _defaultBTUserImageSrc = "/img/defaultblogthumb.png";
        private readonly string _defaultCompanyImageSrc = "/img/defaultcategorynoimage.jpg";
        private readonly string _defaultProjectImageSrc = "/img/defaultblogthumb.png";




        public string ConvertByteArrayToFile(byte[] fileData, string extension, int ImageType)
        {
            if (fileData == null || fileData.Length == 0)
            {
                switch (ImageType)
                {
                    case 1: return _defaultBTUserImageSrc;
                    case 2: return _defaultCompanyImageSrc;
                    case 3: return _defaultProjectImageSrc;
                }
            }

            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData!);
                string imageSrcString = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageSrcString;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
