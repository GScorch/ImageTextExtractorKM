using OpenCvSharp;
using Tesseract;

public class OcrService
{
    private readonly string _tessDataPath;

    public OcrService(IWebHostEnvironment env)
    {
        _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
    }

    public string ExtractText(Stream imageStream, string dir, string filename)
    {
        imageStream.Position = 0;
        var preprocessedImage = PreprocessImage(imageStream, dir, filename);


        using var engine = new TesseractEngine(_tessDataPath, "ssd+7seg+eng", EngineMode.LstmOnly);
        engine.DefaultPageSegMode = PageSegMode.SingleLine;
        //engine.DefaultPageSegMode = PageSegMode.SingleLine;
        engine.SetVariable("tessedit_char_whitelist", "0123456789");
        using var img = Pix.LoadFromMemory(preprocessedImage);
        //using var img = Pix.LoadFromMemory(ReadFully(imageStream));
        using var page = engine.Process(img);
        return page.GetText();
    }

    private byte[] ReadFully(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
    // public byte[] PreprocessImage(Stream imageStream, string dir, string filename)
    // {
    //     using var ms = new MemoryStream();
    //     imageStream.CopyTo(ms);
    //     var bytes = ms.ToArray();
    //     var mat = Cv2.ImDecode(bytes, ImreadModes.Grayscale);
    //     Cv2.Resize(mat, mat, new Size(mat.Width + mat.Height, mat.Height));
    //     Cv2.MorphologyEx(mat, mat, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2)));
    //     Cv2.GaussianBlur(mat, mat, new Size(5, 5), 0);

    //     var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new Size(8, 8));
    //     clahe.Apply(mat, mat);

    //     // Adaptive Threshold to binarize
    //     //Cv2.AdaptiveThreshold(mat, mat, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

    //     // Invert so text is black on white (Tesseract prefers this)
    //     //Cv2.BitwiseNot(mat, mat);

    //     // Find contours to detect content area
    //     // Cv2.FindContours(mat.Clone(), out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
    //     // if (contours.Count() > 0)  // Check if any contours are found
    //     // {
    //     //     var biggest = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
    //     //     var rect = Cv2.BoundingRect(biggest);
    //     //     mat = new Mat(mat, rect);  // Crop the image to the largest contour
    //     // }

    //     // Save to root folder for debugging
    //     var savePath = Path.Combine(dir, "preprocessed_"+filename+".png");
    //     Cv2.ImWrite(savePath, mat);
    //     return mat.ToBytes(".png");
    // }
    public byte[] PreprocessImage(Stream imageStream, string dir, string filename)
    {
        // Read image from stream into grayscale Mat
        using var ms = new MemoryStream();
        imageStream.CopyTo(ms);
        var bytes = ms.ToArray();
        var mat = Cv2.ImDecode(bytes, ImreadModes.Grayscale);

        // Resize to improve OCR (horizontal scaling mostly helps with narrow fonts)
        Cv2.Resize(mat, mat, new Size(mat.Width + mat.Height, mat.Height));

        // Morphological close to connect characters
        Cv2.MorphologyEx(mat, mat, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2)));

        // Apply Gaussian blur to reduce noise
        Cv2.GaussianBlur(mat, mat, new Size(3, 3), 0);

        // Apply CLAHE (Contrast Limited Adaptive Histogram Equalization)
        var clahe = Cv2.CreateCLAHE(clipLimit: 4.0, tileGridSize: new Size(8, 8));
        clahe.Apply(mat, mat);

        Mat kernel = new Mat(3, 3, MatType.CV_32FC1); // 3x3 kernel, float type

        // Manually set values row-by-row
        kernel.Set(0, 0, 0f);  kernel.Set(0, 1, -1f); kernel.Set(0, 2, 0f);
        kernel.Set(1, 0, -1f); kernel.Set(1, 1, 5f);  kernel.Set(1, 2, -1f);
        kernel.Set(2, 0, 0f);  kernel.Set(2, 1, -1f); kernel.Set(2, 2, 0f);

        Mat floatMat = new Mat();
        mat.ConvertTo(floatMat, MatType.CV_32FC1);

        Mat filtered = new Mat();
        Cv2.Filter2D(floatMat, filtered, -1, kernel);

        filtered.ConvertTo(mat, MatType.CV_8UC1); // convert back to 8-bit grayscale

        // ➕ Threshold (Otsu's method to binarize)
        Cv2.Threshold(mat, mat, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        // ➕ Invert to make digits black on white (Tesseract prefers it)
        Cv2.BitwiseNot(mat, mat);

        // Optional: Resize again if digits are still too small
        // Cv2.Resize(mat, mat, new Size(mat.Width * 2, mat.Height * 2), interpolation: InterpolationFlags.Linear);

        // Save for debugging
        var savePath = Path.Combine(dir, "preprocessed_" + filename );
        SaveMatAsImage(mat, savePath);
        

        return mat.ToBytes(".png");
    }
    public void SaveMatAsImage(Mat mat, string savePath, string extension = ".png")
    {
        byte[] encodedBytes = mat.ToBytes(extension);
        File.WriteAllBytes(savePath, encodedBytes);
    }
}
