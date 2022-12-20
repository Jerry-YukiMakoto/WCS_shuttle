using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HslCommunicationPLC
{
    public class clsTool
    {
        public static bool GetPlcBit(short iData, int iBit)
        {
            bool bFlag = false;
            bFlag = (iData & ((short)(Math.Pow(2, iBit)))) == 0 ? false : true;
            return bFlag;
        }

        public static string GetAscii_LowByte(short iData)
        {
            int iTmp = int.Parse(Convert.ToString((iData % 256), 10));
            if (Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }

        public static string GetAscii_HiByte(short iData)
        {
            int iTmp = int.Parse(Convert.ToString((iData / 256), 10));
            if (Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }

        public static bool ArrayTransfer_S2I(string[] strInput, ref short[] intOutput)
        {
            try
            {
                intOutput = Array.ConvertAll(strInput, new Converter<string, short>(StringToShort));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static short StringToShort(string s)
        {
            return Convert.ToInt16(s);
        }
    }
}
