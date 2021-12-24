using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Mirle.Extensions
{
    [Obsolete]
    public static class XmlExtensions
    {
        [Obsolete]
        public static void SerializeToXmlFile<T>(this T mObject, string fileName)
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
                Debug.WriteLine($"{ex.Message}-{ex.StackTrace}");
                throw;
            }
        }

        [Obsolete]
        public static T DeserializeFromXmlFile<T>(string fileName)
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
                Debug.WriteLine($"{ex.Message}-{ex.StackTrace}");
            }
            return mObject;
        }
    }
}
