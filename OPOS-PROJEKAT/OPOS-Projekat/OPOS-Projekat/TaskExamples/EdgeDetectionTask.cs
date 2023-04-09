using OPOS_Projekat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json.Serialization;

namespace OPOS_Projekat.TaskExamples
{
    [Serializable]
    public class EdgeDetectionTask : IAction
    {
        public List<string> inputs { get; set; } = new();
        public List<string> outputs { get; set; } = new();
        private List<int> processed { get; set; } = new();
        public int ID { get; set; } = 0;
        public int degreeOfParallelism { get; set; } = 1;
        public int degreeOfParallelismPerTask { get; set; } = 1;
        static int[,] kernel = new int[,] { { 0, -1, 0 }, { -1, 4, -1 }, { 0, -1, 0 }, };
        public EdgeDetectionTask() { }
        public EdgeDetectionTask(int id, List<string> inputs, List<string> outputs, int degreeOfParallelism, int degreeOfParallelismPerTask)
        {
            this.ID = id;
            this.inputs = inputs;
            this.outputs = outputs;
            this.degreeOfParallelism = degreeOfParallelism;
            this.degreeOfParallelismPerTask = degreeOfParallelismPerTask;
        }
        public void Run(ICoopApi coopApi)
        {
            int it = 0;
            object lock1 = new();
            if (inputs == null || outputs == null || inputs.Count() != outputs.Count())
                throw new Exception();
            if (inputs.Count() > 1)
            {
                //MaxDegreeOfParallelism properties is an integer number indicating the number of threads to execute the code.
                Parallel.For(0, inputs.Count(), new ParallelOptions() { MaxDegreeOfParallelism = this.degreeOfParallelism }, num =>
                {
                    if (!processed.Contains(num))
                    {
                        lock (lock1)
                        {
                            it++;
                            double progress = (double)it / inputs.Count();
                            coopApi.SetProgress(progress);
                        }
                        string inputPath = inputs[num];
                        string outputPath = outputs[num];
                        Bitmap inputImage = (Bitmap)System.Drawing.Image.FromFile(inputPath);
                        Bitmap outputImage = this.DetectEdgesNoParallel(inputImage, coopApi);
                        outputImage.Save(outputPath, ImageFormat.Jpeg);
                        lock (processed)
                        {
                            processed.Add(num);
                        }
                    }
                });
            }
            else
            {
                string inputPath = inputs[0];
                string outputPath = outputs[0];
                Bitmap inputImage = (Bitmap)System.Drawing.Image.FromFile(inputPath);
                Bitmap outputImage = this.DetectEdges(inputImage, coopApi);
                outputImage.Save(outputPath, ImageFormat.Jpeg);
            }
            /* foreach (var input in inputs)
             {
                 string inputPath = input.Item1;
                 string outputPath = input.Item2;
                 Bitmap inputImage = (Bitmap)System.Drawing.Image.FromFile(inputPath);
                 Bitmap outputImage = this.DetectEdges(inputImage, coopApi);
                 outputImage.Save(outputPath, ImageFormat.Jpeg);
             }*/

        }

        public Bitmap DetectEdges(Bitmap image, ICoopApi coopApi)
        {
            //result bitmap image
            Bitmap outputImage = (Bitmap)image.Clone();

            //Kernel dimensions
            int kernelWidth = 3;
            int kernelHeight = 3;

            //input picture dimensions
            int width = image.Width;
            int height = image.Height;

            int it = 0;
            object lock1 = new();

            //result rgb color pixels
            Color[,] result = new Color[image.Width, image.Height];

            //Locks a Bitmap into system memory and returns a Bitmap Data
            BitmapData bitmapData = outputImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // The stride is the width of a single row of pixels (a scan line), rounded up to a four-byte boundary.
            // If the stride is positive, the bitmap is top-down. If the stride is negative, the bitmap is bottom-up.
            int bytes = bitmapData.Stride * height; //calculating how many bytes does the pixel array actually take
            byte[] rgbValues = new byte[bytes];

            // Copies data from a managed array to an unmanaged memory pointer,
            // or from an unmanaged memory pointer to a managed array.
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);


            // Fill the color array with the new sharpened color values.
            //using parrlel foreach for parallelism
            //List<int> integerList = Enumerable.Range(0, width).ToList();
            //Parallel.ForEach(integerList, options, i => {
            Parallel.For(0, width, new ParallelOptions() { MaxDegreeOfParallelism = degreeOfParallelismPerTask }, i =>
            {
                lock (lock1)
                {
                    it++;
                    double progress = (double)it / (double)width;
                    coopApi.SetProgress(progress);
                }
                for (int j = 0; j < height; j++)
                {

                    double gR = 0, gG = 0, gB = 0;
                    for (int filterX = 0; filterX < kernelWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < kernelHeight; filterY++)
                        {

                            //finding coords (starting from 1 pixel left from current and adding kernel index to it)
                            int imageX = (i - kernelWidth / 2 + filterX + width) % width;
                            int imageY = (j - kernelHeight / 2 + filterY + height) % height;
                            //finding current pixel coords
                            int rgb = imageY * bitmapData.Stride + 3 * imageX;

                            //applying kernel
                            gR += rgbValues[rgb + 2] * kernel[filterX, filterY];
                            gG += rgbValues[rgb + 1] * kernel[filterX, filterY];
                            gB += rgbValues[rgb + 0] * kernel[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)gR, 0), 255);
                        int g = Math.Min(Math.Max((int)gG, 0), 255);
                        int b = Math.Min(Math.Max((int)gB, 0), 255);

                        //setting result pixel
                        result[i, j] = Color.FromArgb(r, g, b);
                    }
                }
            });


            //Parallel.ForEach(integerList, options, i => {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int rgb = y * bitmapData.Stride + 3 * x;
                    //jobApi.checkForInterrupts();
                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }
            //);

            //copy to new picture
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bitmapData.Scan0, bytes);
            // bitmapdata release
            outputImage.UnlockBits(bitmapData);
            //returning edge detected image
            return outputImage;
        }
        public Bitmap DetectEdgesNoParallel(Bitmap image, ICoopApi coopApi)
        {
            //result bitmap image
            Bitmap outputImage = (Bitmap)image.Clone();

            //Kernel dimensions
            int kernelWidth = 3;
            int kernelHeight = 3;

            //input picture dimensions
            int width = image.Width;
            int height = image.Height;


            //result rgb color pixels
            Color[,] result = new Color[image.Width, image.Height];

            //Locks a Bitmap into system memory and returns a Bitmap Data
            BitmapData bitmapData = outputImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // The stride is the width of a single row of pixels (a scan line), rounded up to a four-byte boundary.
            // If the stride is positive, the bitmap is top-down. If the stride is negative, the bitmap is bottom-up.
            int bytes = bitmapData.Stride * height; //calculating how many bytes does the pixel array actually take
            byte[] rgbValues = new byte[bytes];

            // Copies data from a managed array to an unmanaged memory pointer,
            // or from an unmanaged memory pointer to a managed array.
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);


            // Fill the color array with the new sharpened color values.
            //using parrlel foreach for parallelism
            //List<int> integerList = Enumerable.Range(0, width).ToList();
            //Parallel.ForEach(integerList, options, i => {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double gR = 0, gG = 0, gB = 0;
                    for (int filterX = 0; filterX < kernelWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < kernelHeight; filterY++)
                        {

                            //finding coords (starting from 1 pixel left from current and adding kernel index to it)
                            int imageX = (i - kernelWidth / 2 + filterX + width) % width;
                            int imageY = (j - kernelHeight / 2 + filterY + height) % height;
                            //finding current pixel coords
                            int rgb = imageY * bitmapData.Stride + 3 * imageX;

                            //applying kernel
                            gR += rgbValues[rgb + 2] * kernel[filterX, filterY];
                            gG += rgbValues[rgb + 1] * kernel[filterX, filterY];
                            gB += rgbValues[rgb + 0] * kernel[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)gR, 0), 255);
                        int g = Math.Min(Math.Max((int)gG, 0), 255);
                        int b = Math.Min(Math.Max((int)gB, 0), 255);

                        //setting result pixel
                        result[i, j] = Color.FromArgb(r, g, b);
                    }
                }
            }



            //Parallel.ForEach(integerList, options, i => {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int rgb = y * bitmapData.Stride + 3 * x;
                    //jobApi.checkForInterrupts();
                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }
            //);

            //copy to new picture
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bitmapData.Scan0, bytes);
            // bitmapdata release
            outputImage.UnlockBits(bitmapData);
            //returning edge detected image
            return outputImage;
        }

        public int getID()
        {
            return this.ID;
        }
    }
}




