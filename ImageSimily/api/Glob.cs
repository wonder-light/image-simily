using System.Collections.Concurrent;
using OpenCvSharp;

namespace ImageSimily.api;

public class Glob
{
    /// <summary>
    ///     相似图片的路径组合列表
    ///     使用线程安全的、无序的对象集合 ConcurrentBag
    /// </summary>
    private static readonly ConcurrentBag<(string, string)> Results = [];

    /// <summary>
    ///     记录执行次数的队列
    ///     使用线程安全的、无序的对象集合 ConcurrentBag
    /// </summary>
    private static readonly ConcurrentBag<int> Queues = [];

    /// <summary>
    ///     开始方法
    /// </summary>
    public static void Start()
    {
        // 目录
        const string dir = @"C:\Users\wonder\Pictures\Camera Roll\崩坏3";
        // 图片路径列表
        var images = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                              .Where(S => !S.Contains(".ini"))
                              .ToList();
        // 并行计算
        ParallelCompute(images);
    }

    /// <summary>
    ///     并行计算
    /// </summary>
    public static void ParallelCompute(List<string> images)
    {
        // 已执行的次数
        var numb1 = 0;
        // 图片数量
        var imageCount = images.Count;
        // 需要进行比较的总次数 + 图片数量(计算特征值的次数)
        var imageSum = imageCount * (imageCount - 1) / 2;
        // 直方图特征列表
        // 使用线程安全的、无序的对象集合 ConcurrentBag
        ConcurrentBag<Mat> hists = [];
        // SURF 特征列表
        // 使用线程安全的、无序的对象集合 ConcurrentBag
        ConcurrentBag<Mat> surfs = [];

        // 创建控制台线程
        var consoleThread1 = ConsoleUpdate.Create(numb1, imageCount, "计算特征值");

        // 并行选项
        var options = new ParallelOptions
        {
            //MaxDegreeOfParallelism = 20 // 最大并行数量
        };

        // 并行计算
        Parallel.ForEach(images,  (image, _) =>
        {
            hists.Add(Compare.Hist.Compute(image));
            surfs.Add(Compare.Surf.Compute(image));

            // 使用线程安全的、由多个线程共享的变量提供原子操作 Interlocked
            var value = Interlocked.Increment(ref numb1);
            Queues.Add(value);
            consoleThread1.Update(value);

            /*Queues.Add(++numb1);
            consoleThread1.Update(numb1);*/
        });

        // 已执行的次数
        var numb2 = 0;
        // 结束控制台更新线程
        consoleThread1.End();
        // 创建控制台线程
        var consoleThread2 = ConsoleUpdate.Create(numb2, imageSum, "比较特征值");

        // 并行比较图像是否相似
        Parallel.For(0, imageCount, options, (i, _) =>
        {
            Parallel.For(i + 1, imageCount, options, (j, _) =>
            {
                var histValue = Compare.Hist.Compare(hists.ElementAt(i), hists.ElementAt(j));
                var surfValue = Compare.Surf.Compare(surfs.ElementAt(i), surfs.ElementAt(j));
                if(histValue >= 85 && surfValue >= 85)
                {
                    Results.Add((images[i], images[j]));
                }

                var value = Interlocked.Increment(ref numb2);
                Queues.Add(value);
                consoleThread2.Update(value, Results.ToList());

                /*Queues.Add(++numb2);
                consoleThread2.Update(numb2, Results.ToList());*/
            });
        });

        consoleThread2.End();
    }
}