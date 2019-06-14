using System;
using System.Diagnostics;
using System.Threading;

namespace OpenFAST.TCPClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(1000);
            try
            {
                var client = new FastClient("127.0.0.1", 16121);
                client.Connect();
                Thread.Sleep(1000);
                Stopwatch sw = new Stopwatch();
                while (!FastClient.Closed)
                {
                    sw.Start();
                    for (int i = 0; i < 64000; i++)
                    {
                        if (FastClient.Closed)
                            break;
                        client.SendMessage("GOOG");
                    }
                    sw.Stop();
                    if (!FastClient.Closed)
                    {
                        Console.WriteLine(sw.Elapsed.TotalSeconds);
                        Console.WriteLine("MSG/S:" + (64000 / sw.Elapsed.TotalSeconds).ToString("0"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }
    }
}