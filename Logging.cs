using Autodesk.AutoCAD.DatabaseServices;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

public static class ErrLogger
{
    private static string Dir;
    public static bool error;
    private static string warnFileName;
    private static string infoFileName;
    private static readonly object lockObj = new object();

    static ErrLogger()
    {
        warnFileName = "errors.log";
        infoFileName = "info.log";
    }

    public static string GetWarnFileName()
    {
        return Dir + "\\" + warnFileName;
    }

    public static void Start(string dir)
    {
        Dir = dir;
        File.Delete(Dir + "\\" + warnFileName);
        File.Delete(Dir + "\\" + infoFileName);
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + warnFileName))
            {
                streamWriter.WriteLine("# File: " + Dir + "\\" + warnFileName);
                streamWriter.WriteLine("# Date: " + DateTime.Now.Date.ToString("dd.MM.yyyy"));
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log begin");
                streamWriter.Close();
                error = false;
            }
        }
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + infoFileName))
            {
                streamWriter.WriteLine("# File: " + Dir + "\\" + infoFileName);
                streamWriter.WriteLine("# Date: " + DateTime.Now.Date.ToString("dd.MM.yyyy"));
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log begin");
                streamWriter.Close();
            }
        }
    }

    public static void Stop()
    {
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + warnFileName, append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log end");
                streamWriter.Close();
            }
        }
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + infoFileName, append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log end");
                streamWriter.Close();
            }
        }
    }

    //public static void Log(string message)
    //{
    //    using (StreamWriter streamWriter = new StreamWriter(Dir, append: true))
    //    {
    //        streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " +  message);
    //        streamWriter.Close();
    //    }
    //}

    public static void Warning<T>(string message, T Par1, T Par2)
    {
        lock(lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + warnFileName, append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " + 
                                       string.Format("{1} | {0} {2} | {3}", message, Par1, Par2, GetCaller()));
                streamWriter.Close();
            }
        }       
    }

    public static void Information<T>(string message, T Par1)
    {
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(Dir + "\\" + infoFileName, append: true))
            {
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " +
                                       string.Format("{1} | {0}", message, Par1));
                streamWriter.Close();
            }
        }
    }

    private static string GetCaller(int framesToSkip = 0)
    {
        string result = string.Empty;

        int i = 1;

        while (true)
        {
            // Walk up the stack trace ...
            var stackFrame = new StackFrame(i++, true);
            MethodBase methodBase = stackFrame.GetMethod();
            if (methodBase == null)
                break;

            // Here we're at the end - nomally we should never get that far 
            Type declaringType = methodBase.DeclaringType;
            if (declaringType == null)
                break;

            // Get class name and method of the current stack frame
            result = string.Format("{0}.{1} Line {2}", declaringType.FullName, methodBase.Name, stackFrame.GetFileLineNumber());

            // Here, we're at the first method outside of SimpleLog class. 
            // This is the method that called the log method. We're done unless it is 
            // specified to skip additional frames and go further up the stack trace.
            if (declaringType != typeof(ErrLogger) && --framesToSkip < 0)
                break;
        }

        return result;
    }
}

//public static class Logger
//{
//    public static string filePath;

//    public static void Start(string filePath)
//    {
//        Logger.filePath = filePath;
//        using (StreamWriter streamWriter = new StreamWriter(filePath))
//        {
//            streamWriter.WriteLine("# File: " + filePath);
//            streamWriter.WriteLine("# Date: " + DateTime.Now.Date.ToString("dd.MM.yyyy"));
//            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log begin");
//            streamWriter.Close();
//        }
//    }
//    public static void Stop()
//    {
//        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
//        {
//            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log end");
//            streamWriter.Close();
//        }
//    }

//    public static void Log(string message)
//    {
//        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
//        {
//            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " + message);
//            streamWriter.Close();
//        }
//    }
//}
