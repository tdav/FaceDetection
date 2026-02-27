using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Asbt.FaceDetection
{
    public class MyFaceDetection
    {
        private Image image;

        public Image Find(Image myImage)
        {
            if (myImage == null)
                throw new Exception("Изображение не загружено.");

            try
            {
                // Convert Image to Mat
                Bitmap bitmap = new Bitmap(myImage);
                Mat src = BitmapConverter.ToMat(bitmap);
                Mat gray = new Mat();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                // Load Cascade Classifier
                string cascadePath = System.IO.Path.Combine(Application.StartupPath, "haarcascade_frontalface_default.xml");
                if (!System.IO.File.Exists(cascadePath))
                {
                    cascadePath = "haarcascade_frontalface_default.xml";
                }

                using (var cascade = new CascadeClassifier(cascadePath))
                {
                    // Detect faces
                    Rect[] faces = cascade.DetectMultiScale(
                        gray,
                        scaleFactor: 1.1,
                        minNeighbors: 3,
                        flags: HaarDetectionTypes.ScaleImage,
                        minSize: new OpenCvSharp.Size(30, 30));

                    if (faces.Length > 0)
                    {
                        // Get the largest face
                        Rect face = faces[0];
                        int maxArea = face.Width * face.Height;
                        for (int i = 1; i < faces.Length; i++)
                        {
                            int area = faces[i].Width * faces[i].Height;
                            if (area > maxArea)
                            {
                                maxArea = area;
                                face = faces[i];
                            }
                        }

                        // Draw rectangle on original image
                        Cv2.Rectangle(src, face, Scalar.Red, 2);
                        var image1 = BitmapConverter.ToBitmap(src);

                        // --- Logic for Cropping (ISO/IEC 29794-5: 7.4.9 and 7.4.10) ---
                        // Haar cascade typically detects from eyebrows/eyes to mouth/chin.
                        // We need to approximate the full head (chin to crown) which is usually ~1.2 - 1.3 times the Haar face box.
                        int crownY = face.Y - (int)(face.Height * 0.2);
                        int chinY = face.Y + (int)(face.Height * 1.1); // Slightly below Haar rect
                        int headHeight = chinY - crownY;

                        // 7.4.9 Head Size: Digital image head height should be 50-69% of image height. Let's target 60%.
                        int targetH = (int)(headHeight / 0.60);

                        // Aspect ratio typically 35x45 mm (width/height)
                        int targetW = (int)(targetH * (35.0 / 45.0));

                        // Center horizontally
                        int targetX = face.X + face.Width / 2 - targetW / 2;

                        // Calculate Top Margin (approx 10-15% of frame height according to standard passport crops)
                        int targetY = crownY - (int)(targetH * 0.12);

                        // Adjust crop area to not exceed image boundaries
                        targetX = Math.Max(0, targetX);
                        targetY = Math.Max(0, targetY);
                        if (targetX + targetW > src.Width) targetW = src.Width - targetX;
                        if (targetY + targetH > src.Height) targetH = src.Height - targetY;

                        // Crop and display in pictureBox2
                        if (targetW > 0 && targetH > 0)
                        {
                            Rect cropRect = new Rect(targetX, targetY, targetW, targetH);
                            Mat originalSrc = BitmapConverter.ToMat(bitmap); // Use original image without red rectangle
                            Mat croppedMat = new Mat(originalSrc, cropRect);
                            image = BitmapConverter.ToBitmap(croppedMat);
                        }

                        return image;
                    }
                    else
                    {
                        throw new Exception("Лица не найдены.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при поиске лиц: {ex.Message}");                
            }
        }

        public void Save(string filename, string myFormat = ".png")
        {
            if (image == null)
                throw new Exception("Нет изображения для сохранения. Сначала найдите лицо.");


            try
            {
                // Создаем новое изображение с требуемыми параметрами (ISO/IEC 29794-5)
                // Требование: 24 бита глубина цвета (8 бит на канал)
                using (Bitmap originalImage = new Bitmap(image))
                using (Bitmap saveImage = new Bitmap(originalImage.Width, originalImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                {
                    // Требование: Разрешение печати >= 300 DPI
                    saveImage.SetResolution(300, 300);

                    using (Graphics g = Graphics.FromImage(saveImage))
                    {
                        // Очищаем фон
                        g.Clear(Color.White);
                        // Настраиваем высокое качество интерполяции (отключаем сглаживание, чтобы пиксель-в-пиксель, или ставим HighQualityBicubic)
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        // Перерисовываем пиксели строго по размеру, чтобы избежать автоматического масштабирования из-за разницы DPI (96 vs 300)
                        g.DrawImage(originalImage, new Rectangle(0, 0, saveImage.Width, saveImage.Height));
                    }

                    // Сохраняем в выбранном формате (JPEG/PNG)
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    if (myFormat.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        format = System.Drawing.Imaging.ImageFormat.Png;
                    }

                    saveImage.Save(filename, format);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Произошла ошибка при сохранении.");
            }
        }
    }
}
