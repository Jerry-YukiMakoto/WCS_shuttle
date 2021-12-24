using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Mirle
{
    public static class XmlFile
    {
        public static void Serialize<T>(this T mObject, string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    var formatter = new XmlSerializer(typeof(T));
                    var writer = new StreamWriter(fs, Encoding.UTF8);
                    formatter.Serialize(writer, mObject);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        public static T Deserialize<T>(string fileName)
        {
            var mObject = default(T);
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(T));
                    mObject = (T)formatter.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            return mObject;
        }
    }
}
