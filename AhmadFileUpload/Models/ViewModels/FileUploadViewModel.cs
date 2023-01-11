namespace AhmadFileUpload.Models.ViewModels
{
    public class FileUploadViewModel
    {
        public IList<FileOnline> FileOnlines { get; set; }
        public IList<FileOffline> FileOfflines { get; set; }
    }
}
