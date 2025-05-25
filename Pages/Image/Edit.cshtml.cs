using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using OpenCvSharp;

namespace ImageTextExtractor.Pages.Image
{
    public class EditModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly OcrService _ocrService;
        private readonly FtpHelper _FtpHelper;
        private readonly FileHelper _FileHelper;

                
        public EditModel(IWebHostEnvironment env,OcrService ocrService, FtpHelper FtpHelp, FileHelper FileHelp)
        {
            _env = env;
            _ocrService = ocrService;
            _FtpHelper=FtpHelp;
            _FileHelper=FileHelp;
            
        }

        [BindProperty]
        public IFormFile OriginalImage { get; set; }

        [BindProperty]
        public string CropData { get; set; }

        public IActionResult OnPost()
        {
            if (OriginalImage == null || OriginalImage.Length == 0)
            {
                return BadRequest("Image is required.");
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads/"+ (Path.GetFileNameWithoutExtension(OriginalImage.FileName)));
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var originalFileName = Path.GetFileNameWithoutExtension(OriginalImage.FileName);
            var originalPath = Path.Combine(uploadsDir, "original_" + originalFileName);
            using (var stream = new FileStream(originalPath, FileMode.Create))
            {
                OriginalImage.CopyTo(stream);
            }

            var originalUrl = uploadsDir + originalFileName;
            dynamic crop = null;
            if (!string.IsNullOrEmpty(CropData))
            {
                crop = JsonSerializer.Deserialize<dynamic>(CropData);
            }
            var cropInfo = JsonSerializer.Deserialize<CropInfo>(CropData);
            if (cropInfo == null) return BadRequest("Invalid crop data");
            using var src = Cv2.ImRead(originalPath);
            var rect = new Rect(
                (int)cropInfo.x,
                (int)cropInfo.y,
                (int)cropInfo.width,
                (int)cropInfo.height
            );
            using var cropped = new Mat(src, rect);
            var croppedPath = Path.Combine(uploadsDir, "cropped_" + originalFileName +".png");
            //Cv2.ImWrite(croppedPath, cropped);
            _ocrService.SaveMatAsImage(cropped, croppedPath);

            using var streamFile = new FileStream(croppedPath, FileMode.Open, FileAccess.Read);
            var the_ret = _ocrService.ExtractText(streamFile,uploadsDir,  originalFileName);
            _FileHelper.SaveStringToFile(the_ret, uploadsDir, originalFileName+".txt");


            string baseDirectory = _env.WebRootPath;
            string ftpServerUrl = "ftp://172.20.18.42/Gandhi";
            string ftpUsername = "Adm1nh0";
            string ftpPassword = "5p1nd0m@r3T";
            string baseFolder = uploadsDir.Replace(baseDirectory, string.Empty);
            bool success = _FtpHelper.UploadFolder(baseFolder, ftpServerUrl, ftpUsername, ftpPassword,baseDirectory );
            streamFile.Close();


            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(100);
            _FileHelper.DeleteFolderRoot(uploadsDir);
            return new JsonResult(new
            {
                FtpImage = ftpServerUrl+"/"+baseFolder,
                cropCoordinates = crop ?? null,
                text= the_ret
            });
        }

        public class CropInfo
        {
            public float x { get; set; }
            public float y { get; set; }
            public float width { get; set; }
            public float height { get; set; }
        }
    }
}
