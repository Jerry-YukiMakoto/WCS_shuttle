namespace Mirle.MPLC.MCProtocol
{
    internal class Device
    {
        public const string TypeBit = "BIT";
        public const string TypeWord = "WORD";
        public const string AddressHexadecimal = "HEX";
        public const string AddressDecimal = "DEC";

        public string DataType { get; }
        public string AsciiCode { get; }
        public byte BinaryCode { get; }
        public string AddressType { get; }

        public Device(string dataType, string asciiCode, byte binaryCode, string addressType)
        {
            DataType = dataType;
            AsciiCode = asciiCode;
            BinaryCode = binaryCode;
            AddressType = addressType;
        }
    }
}