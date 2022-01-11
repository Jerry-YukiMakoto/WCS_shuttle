using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.WriLog;

namespace Mirle.DB.Proc
{
    public class clsWriLog
    {
        public static clsLog Log = new clsLog("DB_Proc", true);

        public static void StoreInLogTrace(int BufferIndex, string BufferName, string Msg)
        {
            Log.FunWriTraceLog_CV($"StoreIn => {BufferIndex} | {BufferName}: {Msg}");
        }

        public static void StoreOutLogTrace(int BufferIndex, string BufferName, string Msg)
        {
            Log.FunWriTraceLog_CV($"StoreOut => {BufferIndex} | {BufferName}: {Msg}");
        }
        
        public static void L2LLogTrace(int CmdType, string IoType, string Msg)
        {
            Log.FunWriTraceLog_CV($"L2L => {CmdType} | {IoType}: {Msg}");
        }

        public static void EmptyStoreInLogTrace(int BufferIndex, string BufferName, string Msg)
        {
            Log.FunWriTraceLog_CV($"EmptyStoreIn => {BufferIndex} | {BufferName}: {Msg}");
        }

        public static void EmptyStoreOutLogTrace(int BufferIndex, string BufferName, string Msg)
        {
            Log.FunWriTraceLog_CV($"EmptyStoreOut => {BufferIndex} | {BufferName}: {Msg}");
        }

    }
}
