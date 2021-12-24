namespace Mirle.MPLC
{
    public interface IConnectable
    {
        void Close();

        bool Connect();

        bool ReConnect();

        bool TestConnection();
    }
}
