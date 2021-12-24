using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Mirle.MPLC.DataType
{
    public static class DataTypeExtensions
    {
        public static bool IsBit(this IDataType dataType)
        {
            return dataType is Bit;
        }
        public static bool IsWord(this IDataType dataType)
        {
            return dataType is Word;
        }
        public static bool IsDWord(this IDataType dataType)
        {
            return dataType is DWord;
        }
        public static bool IsWordBlock(this IDataType dataType)
        {
            return dataType is WordBlock;
        }

        public static Bit AsBit(this IDataType dataType)
        {
            if (dataType is Bit bit)
            {
                return bit;
            }
            else
            {
                return new Bit();
            }
        }
        public static Word AsWord(this IDataType dataType)
        {
            if (dataType is Word word)
            {
                return word;
            }
            else
            {
                return new Word();
            }
        }
        public static DWord AsDWord(this IDataType dataType)
        {
            if (dataType is DWord dWord)
            {
                return dWord;
            }
            else
            {
                return new DWord();
            }
        }
        public static WordBlock AsWordBlock(this IDataType dataType)
        {
            if (dataType is WordBlock wordBlock)
            {
                return wordBlock;
            }
            else
            {
                return new WordBlock();
            }
        }

        public static Color ToColor(this Bit bit)
        {
            return bit.ToColor(Color.Yellow, Color.White);
        }
        public static Color ToColor(this Bit bit, Color trueColoer, Color falseColoer)
        {
            return bit.IsOn() ? trueColoer : falseColoer;
        }
        public static Color ToColor(this bool bit)
        {
            return bit ? Color.Yellow : Color.White;
        }
        public static Color ToColor(this bool bit, Color trueColoer, Color falseColoer)
        {
            return bit ? trueColoer : falseColoer;
        }

        public static int ToBase10(this string base16)
        {
            if (string.IsNullOrWhiteSpace(base16))
            {
                return 0;
            }

            return Convert.ToInt32(base16, 16);
        }

        public static string ToBase16(this int base10)
        {
            return base10.ToString("X");
        }
        public static string ToBase16(this int base10, int length)
        {
            return base10.ToString($"X{length}");
        }

        public static string ToASCII(this ushort value)
        {
            return Encoding.ASCII.GetString(BitConverter.GetBytes(value));
        }
        public static string ToASCII(this int[] value)
        {
            string ascii = string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                ascii += Convert.ToUInt16(value[i]).ToASCII();
            }

            return new string(ascii.Where(c => !char.IsControl(c)).ToArray()).Trim();
        }

        public static byte[] ToBytes(this int[] value)
        {
            byte[] result = new byte[value.Length * 2];
            for (int i = 0; i < value.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(value[i]);
                result[i * 2] = bytes[0];
                result[(i * 2) + 1] = bytes[1];
            }

            return result;
        }

        public static int[] To16BitInteger(this string ascii)
        {
            byte[] tmpData = Encoding.ASCII.GetBytes(ascii);
            byte[] bytes = new byte[tmpData.Length];

            if (tmpData.Length % 2 != 0)
            {
                bytes = new byte[tmpData.Length + (tmpData.Length % 2)];
            }
            Array.Copy(tmpData, bytes, tmpData.Length);

            int[] result = new int[bytes.Length / 2];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                result[i / 2] = (bytes[i + 1] * 256) + bytes[i];
            }

            return result;
        }

        public static int ToBCD(this int base10)
        {
            int st = 0;
            if (base10 != 0)
            {
                st = ((base10 / 10).ToBCD() * 16) + (base10 % 10);
            }

            return st;
        }
        public static int ToBase10(this int bcd)
        {
            int st = 0;
            if (bcd != 0)
            {
                st = ((bcd / 16).ToBase10() * 10) + (bcd % 16);
            }

            return st;
        }
    }
}
