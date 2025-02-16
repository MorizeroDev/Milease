using System;
using UnityEngine;

namespace Milease.Utils
{
    public static class LogUtils
    {
        private const string PREFIX =
            "<b><color=white>[</color>" +
            "<color=#3dd5e6>M</color>" +
            "<color=#7ec3ee>i</color>" +
            "<color=#92baf1>l</color>" +
            "<color=#a9aaf3>e</color>" +
            "<color=#cf86f9>a</color>" +
            "<color=#e75ffc>s</color>" +
            "<color=#f533fe>e</color>" +
            "<color=white>]</color></b> ";
        
        public static void Error(string content)
        {
            Debug.LogError(PREFIX + content);
        }
        
        public static void Warning(string content)
        {
            Debug.LogWarning(PREFIX + content);
        }
        
        public static void Info(string content)
        {
            Debug.Log(PREFIX + content);
        }
        
        public static void Throw(string content)
        {
            Debug.LogError(PREFIX + content);
            throw new Exception(content);
        }
    }
}
