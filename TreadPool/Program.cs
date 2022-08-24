class Program
{
    static void Main()
    {

        var thread_pool = new MyThreadPool();
      
        static void ExecuteMethod1()
        {
            Console.WriteLine("Hello from the thread pool 1111.");
        }
        static void ExecuteMethod2()
        {
            Console.WriteLine("Hello from the thread pool.");
        }
        var handle1 = thread_pool.QueueUserWorkItem(ExecuteMethod1);
        handle1.Finished += (o, a) => { Console.WriteLine($"Done 1"); };

        Thread.Sleep(5000);

        var handle2 = thread_pool.QueueUserWorkItem(ExecuteMethod2);
        handle2.Finished += (o, a) => { Console.WriteLine($"Done 2"); };
    }
}


