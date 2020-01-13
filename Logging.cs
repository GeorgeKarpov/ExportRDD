using System;
using System.IO;

public static class ErrLogger
{
    public static string filePath;
    public static bool error;

    public static void Start()
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath))
        {
            streamWriter.WriteLine("# File: " + filePath);
            streamWriter.WriteLine("# Date: " + DateTime.Now.Date.ToString("dd.MM.yyyy"));
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log begin");
            streamWriter.Close();
            error = false;
        }
    }

    public static void Stop()
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
        {
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log end");
            streamWriter.Close();
        }
    }

    public static void Log(string message)
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
        {
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " + message);
            streamWriter.Close();
        }
    }
}

public static class Logger
{
    public static string filePath;

    public static void Start()
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath))
        {
            streamWriter.WriteLine("# File: " + filePath);
            streamWriter.WriteLine("# Date: " + DateTime.Now.Date.ToString("dd.MM.yyyy"));
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log begin");
            streamWriter.Close();
        }
    }
    public static void Stop()
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
        {
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- log end");
            streamWriter.Close();
        }
    }

    public static void Log(string message)
    {
        using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
        {
            streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " -- " + message);
            streamWriter.Close();
        }
    }
}
