
using System;

namespace Mirle.IASC
{
    public class CommandReceiveEventArgs : EventArgs
    {
        public string CommandId { get; }

        public CommandReceiveEventArgs(string commandId)
        {
            CommandId = commandId;
        }
    }
}