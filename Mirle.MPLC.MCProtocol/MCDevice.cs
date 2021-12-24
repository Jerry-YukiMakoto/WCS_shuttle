using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirle.MPLC.MCProtocol
{
    internal class MCDevice
    {
        private static readonly Dictionary<string, Device> _DeviceLookup = new Dictionary<string, Device>()
        {
            ["SM"] = new Device(Device.TypeBit, "SM", 0x91, Device.AddressDecimal),
            ["SD"] = new Device(Device.TypeWord, "SD", 0xA9, Device.AddressDecimal),
            ["X"] = new Device(Device.TypeBit, "X*", 0x9C, Device.AddressHexadecimal),
            ["Y"] = new Device(Device.TypeBit, "Y*", 0x9D, Device.AddressHexadecimal),
            ["M"] = new Device(Device.TypeBit, "M*", 0x90, Device.AddressDecimal),
            ["L"] = new Device(Device.TypeBit, "L*", 0x92, Device.AddressDecimal),
            ["F"] = new Device(Device.TypeBit, "F*", 0x93, Device.AddressDecimal),
            ["V"] = new Device(Device.TypeBit, "V*", 0x94, Device.AddressDecimal),
            ["B"] = new Device(Device.TypeBit, "B*", 0xA0, Device.AddressHexadecimal),
            ["D"] = new Device(Device.TypeWord, "D*", 0xA8, Device.AddressDecimal),
            ["W"] = new Device(Device.TypeWord, "W*", 0xB4, Device.AddressHexadecimal),
            ["TS"] = new Device(Device.TypeBit, "TS", 0xC1, Device.AddressDecimal),
            ["TC"] = new Device(Device.TypeBit, "TC", 0xC0, Device.AddressDecimal),
            ["TN"] = new Device(Device.TypeWord, "TN", 0xC2, Device.AddressDecimal),
            ["SS"] = new Device(Device.TypeBit, "SS", 0xC7, Device.AddressDecimal),
            ["SC"] = new Device(Device.TypeBit, "SC", 0xC6, Device.AddressDecimal),
            ["SN"] = new Device(Device.TypeWord, "SN", 0xC8, Device.AddressDecimal),
            ["CS"] = new Device(Device.TypeBit, "CS", 0xC4, Device.AddressDecimal),
            ["CC"] = new Device(Device.TypeBit, "CC", 0xC3, Device.AddressDecimal),
            ["CN"] = new Device(Device.TypeWord, "CN", 0xC5, Device.AddressDecimal),
            ["SB"] = new Device(Device.TypeBit, "SB", 0xA1, Device.AddressHexadecimal),
            ["SW"] = new Device(Device.TypeWord, "SW", 0xB5, Device.AddressHexadecimal),
            ["S"] = new Device(Device.TypeBit, "S*", 0x98, Device.AddressDecimal),
            ["DX"] = new Device(Device.TypeBit, "DX", 0xA2, Device.AddressHexadecimal),
            ["DY"] = new Device(Device.TypeBit, "DY", 0xA3, Device.AddressHexadecimal),
            ["Z"] = new Device(Device.TypeWord, "Z*", 0xCC, Device.AddressDecimal),
            ["R"] = new Device(Device.TypeWord, "R*", 0xAF, Device.AddressDecimal),
            ["ZR"] = new Device(Device.TypeWord, "ZR", 0xB0, Device.AddressHexadecimal),
        };

        public string AsciiAddress { get; private set; }
        public string AsciiDeviceCode { get; private set; }
        public byte BinaryDeviceCode { get; private set; }
        public int Address { get; private set; }
        public bool IsBit { get; private set; } = false;
        public bool IsWord => !IsBit;
        public int BitIndex { get; private set; }

        private MCDevice()
        {
        }

        public static MCDevice Parse(string deviceAddress)
        {
            return Parse(deviceAddress, 0);
        }

        public static MCDevice Parse(string deviceAddress, int offset)
        {
            try
            {
                var item = _DeviceLookup.First(i => deviceAddress.StartsWith(i.Key));
                var device = item.Value;
                string asciiAddress = deviceAddress.TrimStart(item.Key.ToCharArray()).Split('.')[0];

                int bitIndex = deviceAddress.Contains(".") ? int.Parse(deviceAddress.Split('.')[1], System.Globalization.NumberStyles.HexNumber) : 0;

                if (device.DataType == Device.TypeBit)
                {
                    bitIndex = 0;
                }

                int address = 0;

                switch (device.AddressType)
                {
                    case Device.AddressHexadecimal:
                        address = int.Parse(asciiAddress, System.Globalization.NumberStyles.HexNumber) + offset;
                        asciiAddress = address.ToString("X");
                        break;

                    case Device.AddressDecimal:
                    default:
                        address = int.Parse(asciiAddress) + offset;
                        asciiAddress = address.ToString("D");
                        break;
                }


                return new MCDevice()
                {
                    AsciiAddress = asciiAddress,
                    AsciiDeviceCode = device.AsciiCode,
                    BinaryDeviceCode = device.BinaryCode,
                    Address = address,
                    IsBit = device.DataType == Device.TypeBit,
                    BitIndex = bitIndex,
                };
            }
            catch (Exception) { }
            return null;
        }
    }
}