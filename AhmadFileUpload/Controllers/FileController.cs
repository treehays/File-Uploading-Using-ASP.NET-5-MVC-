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
                        Name = fileName.Length > 10 ? fileName.Remove(9) + DateTime.Now.ToLongDateString().Replace(" ", "") : fileName + DateTime.Now.ToLongDateString().Replace(" ", "").Replace(",",""),
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



        [HttpPost]
        public async Task<IActionResult> FileOnlineAction(List<IFormFile> incomingFiles, string description, string UploadedBy)
        {
            foreach (var file in incomingFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileModel = new FileOnline
                {
                    CreatedOn = DateTime.UtcNow,
                    FileType = file.ContentType,
                    Extension = extension,
                    Name = fileName.Length > 10 ? fileName.Remove(9) + DateTime.Now.ToLongDateString().Replace(" ", "") : fileName + DateTime.Now.ToLongDateString().Replace(" ", "").Replace(",", ""),
                    Description = description,
                    UploadedBy = UploadedBy,
                };
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    fileModel.Data = dataStream.ToArray();
                }
                _contex.FileOnlines.Add(fileModel);
                _contex.SaveChanges();
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

        //download from database
        public async Task<IActionResult> DownloadOnlineFile(int id)
        {
            var file = await _contex.FileOnlines.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null) return null;
            return File(file.Data, file.FileType, file.Name + file.Extension);
        }


        //delete from database
        public async Task<IActionResult> DeleteFileOnline(int id)
        {
            var file = await _contex.FileOnlines.Where(x => x.Id == id).FirstOrDefaultAsync();
            _contex.FileOnlines.Remove(file);
            _contex.SaveChanges();
            TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from Database.";
            return RedirectToAction("Index");
        }

        //delete from pc
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
