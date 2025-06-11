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

        public Logger(string logFolder)
        {
            this.logFolder = logFolder;

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            loggingThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true
            };
            loggingThread.Start();
            isRunning = true;
        }

        public void Log(string fileName, string log)
        {
            currentFileName = Path.Combine(logFolder, fileName);
            logQueue.Enqueue(DateTime.Now+" | "+log);
            logSignal.Set();
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
                        throw new IOException("Error writing to file: '"+currentFileName+"'");
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

