namespace Digger.Interface
{
    public interface ITimer
    {
        int Time { get; }
        void Start();
        void SyncFrame(int fps);
    }
}
