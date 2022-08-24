
namespace TreadPool
{
    public class HandleEvent : EventArgs
    {
        public event EventHandler? Finished;
       
        internal void onFinished()
        {
            Finished?.Invoke("st", EventArgs.Empty);
        }
    }
}
