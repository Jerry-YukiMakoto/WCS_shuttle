using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.ASRS.AWCS.Model.LogTrace
{
    public abstract class LogTraceBase
    {
        public int BufferIndex { get; }
        public string BufferName { get; }

        public string OtherMessage { get; }

        protected LogTraceBase(int bufferIndex, string bufferName, string otherMessage)
        {
            BufferIndex = bufferIndex;
            BufferName = bufferName;
            OtherMessage = otherMessage;
        }

        protected abstract string DefaultLogTrace();

        public string GetMessage()
        {
            return $"{BufferIndex}|{BufferName}|{DefaultLogTrace()}|{OtherMessage}";
        }
    }
}
