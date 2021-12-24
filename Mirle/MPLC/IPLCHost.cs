namespace Mirle.MPLC
{
    public interface IPLCHost
    {
        bool EnableAutoReconnect { get; set; }
        bool EnableWriteRawData { get; set; }

        int Interval { get; set; }
        int MPLCTimeout { get; set; }

        string LogBaseDirectory { get; set; }

        IMPLCProvider GetMPLCProvider();

        void Start();

        void Stop();
    }
}
