﻿using System.Diagnostics;
using TreadPool;

public class MyThreadPool
{
    private readonly Thread[] _Threads;
    private readonly Queue<(Action Work, object? Parameter)> _Works = new();
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

    public HandleEvent QueueUserWorkItem(Action Work) => QueueUserWorkItem(new HandleEvent(), Work);

    public HandleEvent QueueUserWorkItem(object? Parameter, Action Work)
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
        _Works.Enqueue((localExecute, result));
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

            var (work, parameter) = _Works.Dequeue();

            _ExecuteEvent.Set(); // разрешаем доступ к очереди

            var timer = Stopwatch.StartNew();
            work();
            timer.Stop();

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