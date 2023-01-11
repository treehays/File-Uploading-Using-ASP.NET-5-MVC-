using AhmadFileUpload.ApplicationContext;
using AhmadFileUpload.Models;
using AhmadFileUpload.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AhmadFileUpload.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationContex _contex;
        public FileController(ApplicationContex contex)
        {
            _contex = contex;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}


        //index controller
        public async Task<IActionResult> Index()
        {
            var allFiles= await LoadAllFiles();
            ViewBag.Message = TempData["Message"];
            return View(allFiles);
        }

        //uploading Action

        [HttpPost]
        public async Task<IActionResult> FileOfflineAction (List<IFormFile> incomingFiles, string description, string UploadedBy)
        {
            foreach (var item in incomingFiles)
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\");
                //var basePath = Path.Combine(Directory.GetCurrentDirectory() + "C:\\Users\\Abdul\\source\\repos\\AhmadFileUpload\\AhmadFileUpload\\Controllers\\");
                bool basePathExists = Directory.Exists(basePath);
                if (!basePathExists) Directory.CreateDirectory(basePath);
                var fileName = Path.GetFileNameWithoutExtension(item.FileName);
                var filePath = Path.Combine(basePath, item.FileName);
                var extension = Path.GetExtension(item.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.CopyToAsync(stream);
                    }
                    var fileModel = new FileOffline
                    {
                        CreatedOn = DateTime.Now,
                        FileType = item.ContentType,
                        Extension = extension,
                        Name = fileName,
                        Description = description,
                        FilePath = filePath,
                        UploadedBy = UploadedBy
                    };
                    _contex.FileOfflines.Add(fileModel);
                    _contex.SaveChanges();
                }
            }
            TempData["Message"] = "File Uploaded successfully.";
            return RedirectToAction("Index");
        }

        //downloding action
        public async Task<IActionResult> DownloadOfflineFile(int id)
        {
            var file = await _contex.FileOfflines.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null) return null;
            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, file.FileType, file.Name + file.Extension);
        }

        public async Task<IActionResult> DeleteOfflineFile(int id)
        {
            var file = await _contex.FileOfflines.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null) return null;
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
            _contex.FileOfflines.Remove(file);
            _contex.SaveChanges();
            TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from File System.";
            return RedirectToAction("Index");
        }



        //This will be refactor late
        private async Task<FileUploadViewModel> LoadAllFiles()
        {
            var viewModel = new FileUploadViewModel();
            viewModel.FileOnlines = await _contex.FileOnlines.ToListAsync();
            viewModel.FileOfflines = await _contex.FileOfflines.ToListAsync();
            return viewModel;
        }


    }
}
