
using System;

namespace Mirle.IASC
{
    public class ChangeLayerEventArgs : EventArgs
    {
        public string CommandId { get; }
        public LayerStatus LayerChangeStatus { get; }
        public LayerResult LayerChangeResult { get; }

        public ChangeLayerEventArgs(string commandId, LayerStatus layerChangeStatus, LayerResult layerChangeResult)
        {
            CommandId = commandId;
            LayerChangeStatus = layerChangeStatus;
            LayerChangeResult = layerChangeResult;
        }
    }
}