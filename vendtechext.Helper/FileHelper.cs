using Microsoft.AspNetCore.Http;
using vendtechext.DAL.Common;

namespace vendtechext.Helper
{
    public class FileHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LogService _log;
        private readonly string _dir;
        public FileHelper(IHttpContextAccessor httpContextAccessor, LogService log)
        {
            _httpContextAccessor = httpContextAccessor;
            _log = log;
            _dir = "images";
        }
        public async Task<string> CreateFile(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    // Define the directory path where files will be saved (must be inside the wwwroot folder for accessibility)
                    var directoryPath = Path.Combine("wwwroot", _dir);

                    // Check if the directory exists; if not, create it
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Generate a unique filename using GUID and preserve the file extension
                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;

                    // Define the full file path
                    var filePath = Path.Combine(directoryPath, fileName);

                    // Save the file to the specified file path
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Return the actual URL to the uploaded file
                    var fileUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme} :// {_httpContextAccessor.HttpContext.Request.Host}/{_dir}/{fileName}";
                    return fileUrl;
                }
                return "";
            }
            catch (Exception ex)
            {
                _log.Log(LogType.Error, ex.Message, ex);
                return "";
            }
        }

        public async Task<string> UpdateFile(IFormFile file, string imgPath)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var directoryPath = Path.Combine("wwwroot", _dir);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileExtension = Path.GetExtension(file.FileName);
                    var fileName = Guid.NewGuid().ToString() + fileExtension;

                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    DeleteFile(imgPath);

                    var fileUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{_dir}/{fileName}";
                    return fileUrl;
                }
                return imgPath;
            }
            catch (Exception ex)
            {
                _log.Log(LogType.Error, ex.Message, ex);
                return "";
            }
        }

        private void DeleteFile(string fileUrl)
        {
            if (!string.IsNullOrEmpty(fileUrl))
            {
                Uri uri = new Uri(fileUrl);

                // Extract the file name
                string fileName = Path.GetFileName(uri.LocalPath);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _dir, fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
                
        }

    }
}
