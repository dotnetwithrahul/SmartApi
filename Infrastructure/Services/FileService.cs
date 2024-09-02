namespace FirebaseApiMain.Infrastructure.Services
{


    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions, string categoryId);
        void DeleteFile(string fileNameWithExtension);
    }


    public class FileService(IWebHostEnvironment environment) : IFileService
    {

        public async Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions, string categoryId)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile));
            }

            // Set the path to the "wwwroot/uploads/category" directory
            var path = Path.Combine(environment.WebRootPath, "uploads", "category");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Get the file extension
            var ext = Path.GetExtension(imageFile.FileName);
            if (!allowedFileExtensions.Contains(ext))
            {
                throw new ArgumentException($"Only {string.Join(",", allowedFileExtensions)} are allowed.");
            }

            // Generate the file name using the categoryId (maintain same name for replacement)
            var fileName = $"{categoryId}{ext}";
            var fileNameWithPath = Path.Combine(path, fileName);

            // Check if the file already exists and delete it before saving the new file
            if (File.Exists(fileNameWithPath))
            {
                File.Delete(fileNameWithPath);
            }

            // Save the new file
            using var stream = new FileStream(fileNameWithPath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            // Return the relative path that can be used to access the image through the API
            var relativePath = $"/uploads/category/{fileName}";
            return relativePath;
        }


        public void DeleteFile(string fileNameWithExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithExtension))
            {
                throw new ArgumentNullException(nameof(fileNameWithExtension));
            }
            var contentPath = environment.ContentRootPath;
            var path = Path.Combine(contentPath, $"Uploads", fileNameWithExtension);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Invalid file path");
            }
            File.Delete(path);
        }

    }
}
