namespace ImageSimily.api;

public class ConsoleUpdate
{
    /// <summary>
    ///     已执行的次数
    /// </summary>
    public int Numb { get; protected set; }

    /// <summary>
    ///     需要执行的总数
    /// </summary>
    public int Sum { get; protected set; } = 1;

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; protected set; } = "";

    /// <summary>
    ///     进度条的位置
    /// </summary>
    public (int Left, int Top) BarPosition { get; protected set; }

    /// <summary>
    ///     光标最后的位置
    /// </summary>
    public (int Left, int Top) CursorPosition { get; set; }

    /// <summary>
    ///     进度
    /// </summary>
    public double Progress { get; protected set; }

    /// <summary>
    ///     需要打印的其它结果
    /// </summary>
    public List<(string, string)> Results { get; protected set; } = [];

    /// <summary>
    ///     控制台背景色
    /// </summary>
    public ConsoleColor ColorBack { get; protected set; } = Console.BackgroundColor;

    /// <summary>
    ///     控制台前景色
    /// </summary>
    public ConsoleColor ColorFore { get; protected set; } = Console.ForegroundColor;

    /// <summary>
    ///     用于管理线程是否取消
    /// </summary>
    public CancellationTokenSource TokenSource { get; protected set; } = new();

    /// <summary>
    ///     控制台线程
    /// </summary>
    public Thread? ConsoleThread { get; protected set; }

    /// <summary>
    ///     创建控制台更新线程
    /// </summary>
    /// <param name="numb">已执行的次数</param>
    /// <param name="sum">需要执行的总数</param>
    /// <param name="name">名称</param>
    /// <returns>返回一个 <see cref="ConsoleUpdate" /> 实例</returns>
    public static ConsoleUpdate Create(int numb, int sum, string? name = "")
    {
        var bar = new ConsoleUpdate();
        bar.Start(numb, sum, name);
        return bar;
    }

    /// <summary>
    ///     开始控制台更新线程
    /// </summary>
    /// <param name="numb">已执行的次数</param>
    /// <param name="sum">需要执行的总数</param>
    /// <param name="name">名称</param>
    /// <returns></returns>
    protected void Start(int numb, int sum, string? name = "")
    {
        Numb = numb;
        Sum = sum;
        Name = name ?? "加载中...";
        //第一行信息
        Console.WriteLine(name);
        //第二行信息, 进度条的位置
        BarPosition = Console.GetCursorPosition();
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.WriteLine(new string(' ', 100));

        //第三行信息
        Console.BackgroundColor = ColorBack;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("{0:f8}% ---- {1} ---- {2}", Progress, Numb, Sum);
        Console.ForegroundColor = ColorFore;
        Console.WriteLine(" ");
        // 记录光标当前位置
        CursorPosition = Console.GetCursorPosition();

        // 控制台线程
        ConsoleThread = new Thread(_Update);
        ConsoleThread.Start();
    }

    /// <summary>
    ///     更新数据
    /// </summary>
    /// <param name="numb">已执行的次数</param>
    /// <param name="results">需要打印的其它结果</param>
    public void Update(int numb, List<(string, string)>? results = null)
    {
        Numb = numb;
        Progress = Math.Max(Progress, Math.Clamp(Numb / (double) Sum * 100, 0, 100));
        if(results != null)
        {
            Results = results;
        }
    }

    /// <summary>
    ///     结束控制台线程
    /// </summary>
    public void End()
    {
        // 通知取消请求
        TokenSource.Cancel();
        TokenSource.Dispose();
        // 结束线程
        ConsoleThread?.Join();
    }

    /// <summary>
    ///     更新控制台内容
    /// </summary>
    private void _Update()
    {
        while(!TokenSource.IsCancellationRequested || Progress < 100)
        {
            // 记录光标当前位置
            CursorPosition = Console.GetCursorPosition();
            // 更新进度
            //绘制进度条进度
            Console.BackgroundColor = ConsoleColor.DarkCyan; //设置进度条颜色
            //设置光标位置,参数为第几列和第几行
            Console.SetCursorPosition(0, BarPosition.Top);
            //移动进度条
            var SpaceNumb = Convert.ToInt32(Progress);
            Console.Write(new string(' ', SpaceNumb));
            //恢复输出颜色
            Console.BackgroundColor = ColorBack;

            // 更新进度百分比
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, BarPosition.Top + 1);
            Console.WriteLine("{0:f8}% ---- {1} ---- {2}", Progress, Numb, Sum);
            Console.ForegroundColor = ColorFore;
            // 恢复光标位置
            Console.SetCursorPosition(CursorPosition.Left, CursorPosition.Top);
            // 更新结果
            for(var i = 0; i < Results.Count;)
            {
                Console.WriteLine("{0}", Results[i]);
                Results.RemoveAt(0);
            }
        }
    }
}