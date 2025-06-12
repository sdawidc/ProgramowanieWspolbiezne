using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Data
{
    internal class Logger : IDisposable
    {
        private readonly string logFolder;
        private readonly Thread loggingThread;
        private readonly ConcurrentQueue<string> logQueue = new();
        private readonly AutoResetEvent logSignal = new(false);
        private bool isRunning = false;

        private string currentFileName;

        private DateTime sessionTimestamp;

        public Logger(string logFolder)
        {
            this.logFolder = logFolder;

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);


            sessionTimestamp = DateTime.Now;
            loggingThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true
            };
            loggingThread.Start();
            isRunning = true;
        }

        private void LogToFile(string fileName, string log)
        {
            currentFileName = Path.Combine(logFolder, getTimestampedFilename(fileName));
            logQueue.Enqueue(DateTime.Now+" | "+log);
            logSignal.Set();
        }

        public void Log(IVector collision)
        {
            LogToFile("collisions", "Collision position: x - " + collision.x + " y - " + collision.y);
        }

        public void Log(string creationLog)
        {
            LogToFile("creation", creationLog);
        }

        public void resetSessionTimestamp()
        {
            sessionTimestamp = DateTime.Now;
        }

        private string getTimestampedFilename(string fileName)
        {
            return fileName + ", " + sessionTimestamp.ToString().Replace(':','-') + ".txt";
        }
        private void ProcessLogQueue()
        {
            while (isRunning)
            {
                logSignal.WaitOne();

                while (logQueue.TryDequeue(out var log))
                {
                    try
                    {
                        File.AppendAllText(currentFileName, log + Environment.NewLine);
                    }
                    catch
                    {
                        throw new IOException("Error writing to file: '"+ getTimestampedFilename(currentFileName) + "'");
                    }
                }
            }
        }

        public void Dispose()
        {
            isRunning = false;
            logSignal.Set();
            loggingThread.Join();
            logSignal.Dispose();
        }
    }
}

