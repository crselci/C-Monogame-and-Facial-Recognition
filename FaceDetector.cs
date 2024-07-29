using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System;
using Project3;
using Emgu.CV.Dnn;

namespace FaceDetect
{
    public class FaceDetector
    {
        public static bool approved = false;
        public static Image<Bgr, byte> DetectAndCropFace(Image<Bgr, byte> image)
        {
            // Load the face detection HaarCascade
            CascadeClassifier faceCascade = new CascadeClassifier("C:\\Programming\\monogame practice\\Project3\\Project3\\Content\\bin\\DesktopGL\\haarcascade_frontalface_alt.xml");

            // Convert the image to grayscale
            Image<Gray, byte> grayImage = image.Convert<Gray, byte>();

            // Detect faces in the image
            Rectangle[] faces = faceCascade.DetectMultiScale(grayImage, 1.1, 10, Size.Empty, Size.Empty);

            if (faces.Length > 0)
            {
                // Crop the first face from the image
                Image<Bgr, byte> faceImage = image.Copy(faces[0]);
                approved = true;
                return faceImage;
            }
            else
            {
                // Return the original image if no faces are detected
                return image;
            }
        }
    }
}
