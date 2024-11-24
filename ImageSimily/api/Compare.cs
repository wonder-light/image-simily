using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;

namespace ImageSimily.api;

public abstract class Compare
{
    /// <summary>
    ///     SIFT 算法
    /// </summary>
    public abstract class Sift
    {
        /// <summary>
        ///     创建 SIFT 对象
        /// </summary>
        protected static SIFT Instance { get; } = SIFT.Create();

        /// <summary>
        ///     创建 FlannBasedMatcher 对象
        /// </summary>
        private static FlannBasedMatcher FlannMatcher { get; } = new();

        /// <summary>
        ///     计算图片的 SIFT 算法相关性
        /// </summary>
        /// <param name="imagePath">图片的路径</param>
        /// <returns></returns>
        public static Mat Compute(string imagePath)
        {
            // 加载图片
            var image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

            // 提取特征点和描述符
            var descriptors = new Mat();
            Instance.DetectAndCompute(image, null, out _, descriptors);

            // 释放图片资源
            image.Dispose();

            // 返回特征描述
            return descriptors;
        }

        /// <summary>
        ///     比较两个图片的 SIFT 值是否相似
        /// </summary>
        /// <param name="descriptors1">图片1的 SIFT 值</param>
        /// <param name="descriptors2">图片2的 SIFT 值</param>
        /// <returns></returns>
        public static double Compare(Mat descriptors1, Mat descriptors2)
        {
            // 相关性比较
            // 进行匹配
            // flannMatches 是一个数组，每个元素是一个 DMatch 对象，表示一对匹配结果
            var flannMatches = FlannMatcher.Match(descriptors1, descriptors2);

            // 百分比
            var value = 100 - (flannMatches.Average(m => m.Distance) - 100) / 5;

            // 是否数组
            Array.Clear(flannMatches);

            return value;
        }

        /// <summary>
        ///     SIFT 算法相关性
        ///     结果越接近 100 则越相似
        ///     图片相似度识别（精度不高，速度较快，可用于以图搜图）
        /// </summary>
        /// <param name="imgPath1">图片1的路径</param>
        /// <param name="imgPath2">图片2的路径</param>
        public static double CompareSift(string imgPath1, string imgPath2)
        {
            // 图片的 SURF 值
            var descriptors1 = Compute(imgPath1);
            var descriptors2 = Compute(imgPath2);

            //相关性比较
            return Compare(descriptors1, descriptors2);
        }
    }

    /// <summary>
    ///     SURF 算法
    /// </summary>
    public abstract class Surf
    {
        /// <summary>
        ///     创建 SURF 对象
        ///     500 是阈值参数，表示特征点的最小响应值
        /// </summary>
        protected static Feature2D Instance { get; } = SURF.Create(500);

        /// <summary>
        ///     创建 FlannBasedMatcher 对象
        /// </summary>
        private static FlannBasedMatcher FlannMatcher { get; } = new();

        /// <summary>
        ///     计算图片的 SURF 算法相关性
        /// </summary>
        /// <param name="imagePath">图片的路径</param>
        /// <returns></returns>
        public static Mat Compute(string imagePath)
        {
            // 加载图片
            var image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);

            // 提取特征点和描述符
            var descriptors = new Mat();
            Instance.DetectAndCompute(image, null, out _, descriptors);

            // 释放图片资源
            image.Dispose();

            // 返回特征描述
            return descriptors;
        }

        /// <summary>
        ///     比较两个图片的 SURF 值是否相似
        /// </summary>
        /// <param name="descriptors1">图片1的 SURF 值</param>
        /// <param name="descriptors2">图片2的 SURF 值</param>
        /// <returns></returns>
        public static double Compare(Mat descriptors1, Mat descriptors2)
        {
            // 相关性比较
            // 进行匹配
            // flannMatches 是一个数组，每个元素是一个 DMatch 对象，表示一对匹配结果
            var flannMatches = FlannMatcher.Match(descriptors1, descriptors2);
            // 百分比
            var value = (1 - flannMatches.Average(m => m.Distance)) * 100;
            // 清理数组
            Array.Clear(flannMatches);
            return value;
        }

        /// <summary>
        ///     SURF 算法相关性
        ///     结果越接近 100 则越相似
        ///     图片相似度识别（精度不高，速度较快，可用于以图搜图）
        /// </summary>
        /// <param name="imgPath1">图片1的路径</param>
        /// <param name="imgPath2">图片2的路径</param>
        public static double CompareSurf(string imgPath1, string imgPath2)
        {
            // 图片的 SURF 值
            var descriptors1 = Compute(imgPath1);
            var descriptors2 = Compute(imgPath2);

            //相关性比较
            return Compare(descriptors1, descriptors2);
        }
    }

    /// <summary>
    ///     直方图算法
    /// </summary>
    public abstract class Hist
    {
        /// <summary>
        ///     直方图数组大小
        /// </summary>
        private static int[] HistSize { get; } = { 256 };

        /// <summary>
        ///     直方图的像素范围
        /// </summary>
        private static Rangef[] HistRange { get; } = { new(0, 256) };

        /// <summary>
        ///     计算图片的直方图数组
        /// </summary>
        /// <param name="imagePath">图片的路径</param>
        /// <returns></returns>
        public static Mat Compute(string imagePath)
        {
            // 加载图片
            var image = Cv2.ImRead(imagePath);
            // 拆分通道
            Cv2.Split(image, out var imageMats);

            //直方图输出数组
            var imageHist = new Mat();
            // 计算一组图像的联合密集直方图
            Cv2.CalcHist(imageMats, [0, 1, 2], null, imageHist, 1, HistSize, HistRange);
            //归一化，排除图像分辨率不一致的影响
            Cv2.Normalize(imageHist, imageHist, 0, 1, NormTypes.MinMax);

            // 释放图片资源
            image.Dispose();

            // 返回图片的直方图数组
            return imageHist;
        }

        /// <summary>
        ///     比较两个图片的直方图数组是否相似
        /// </summary>
        /// <param name="image1">图片1的直方图数组</param>
        /// <param name="image2">图片2的直方图数组</param>
        /// <returns></returns>
        public static double Compare(Mat image1, Mat image2)
        {
            //相关性比较
            var value = Cv2.CompareHist(image1, image2, HistCompMethods.Correl);
            return value * 100;
        }

        /// <summary>
        ///     直方图相关性
        ///     结果越接近 100 则越相似
        ///     图片相似度识别（精度不高，速度较快，可用于以图搜图）
        /// </summary>
        /// <param name="imgPath1">图片1的路径</param>
        /// <param name="imgPath2">图片2的路径</param>
        public static double CompareHist(string imgPath1, string imgPath2)
        {
            //直方图输出数组
            var histA = Compute(imgPath1);
            var histB = Compute(imgPath2);

            //相关性比较
            return Compare(histA, histB);
        }
    }
}