using System.Diagnostics;
using TreadPool;

public class MyThreadPool
{
    private readonly Thread[] _Threads;
    private readonly Queue<(Action Work, CancellationToken cancellationToken)> _Works = new();
    private volatile bool _CanWork = true;

    private readonly AutoResetEvent _WorkingEvent = new(false);
    private readonly AutoResetEvent _ExecuteEvent = new(true);

    public MyThreadPool()
    {
        _Threads = new Thread[1];
        Initialize();

    }

    private void Initialize()
    {
        var thread = new Thread(WorkingThread)
          {
             IsBackground = true
          };
        _Threads[0] = thread;
         thread.Start();
     
    }

    public HandleEvent QueueUserWorkItem(Action Work, CancellationToken cancellationToken)
    {
        var result = new HandleEvent();
        void localExecute()
        {
            Work();
            result.onFinished();
        }
        if (!_CanWork) throw new InvalidOperationException("Попытка передать задание уничтоженному пулу потоков");

        _ExecuteEvent.WaitOne(); // запрашиваем доступ к очереди
        if (!_CanWork) throw new InvalidOperationException("Попытка передать задание уничтоженному пулу потоков");
       
        _ExecuteEvent.Set();
        _Works.Enqueue((localExecute, cancellationToken));
        _WorkingEvent.Set();

        return result;
    }
  
    private void WorkingThread()
    {
        while (_CanWork)
        {
            while (_Works.Count == 0) // если (до тех пор пока) в очереди нет заданий
            {
                _ExecuteEvent.Set(); // освобождаем очередь
                _WorkingEvent.WaitOne(); // дожидаемся разрешения на выполнение
                if (!_CanWork) break;

                _ExecuteEvent.WaitOne(); // запрашиваем доступ к очереди вновь
            }

            var (work, cancellationToken) = _Works.Dequeue();

            _ExecuteEvent.Set(); // разрешаем доступ к очереди

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    Console.WriteLine("Операция отменена");
                cancellationToken.ThrowIfCancellationRequested();

                work();
            }
            catch (ThreadInterruptedException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка выполнения задания в потоке ", e);
            }
            finally
            {
                Console.WriteLine("Поток  завершил свою работу");
                if (!_WorkingEvent.SafeWaitHandle.IsClosed)
                    _WorkingEvent.Set();
            }
        }           
    }
    
    private const int _DisposeThreadJoinTimeout = 100;
    public void Dispose()
    {
        _CanWork = false;

        _WorkingEvent.Set();
        foreach (var thread in _Threads)
            if (!thread.Join(_DisposeThreadJoinTimeout))
                thread.Interrupt();

        _ExecuteEvent.Dispose();
        _WorkingEvent.Dispose();
    }
  
    
}
