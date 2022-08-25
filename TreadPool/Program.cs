class Program
{
    static void Main()
    {

        var thread_pool = new MyThreadPool();
        
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token = cancelTokenSource.Token;
      
        static void ExecuteMethod1()
        {
            Console.WriteLine("Hello from the thread pool 1111.");
        }
        static void ExecuteMethod2()
        {
            Console.WriteLine("Hello from the thread pool.");
        }
        var handle1 = thread_pool.QueueUserWorkItem(ExecuteMethod1, token);
        handle1.Finished += (o, a) => { Console.WriteLine($"Done 1"); };

        Thread.Sleep(5000);
        
        cancelTokenSource.Cancel();
        var handle2 = thread_pool.QueueUserWorkItem(ExecuteMethod2, token);
        handle2.Finished += (o, a) => { Console.WriteLine($"Done 2"); };
    }
}


