using MyTcpClient;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] cookies;
            // Class1.TryGetCookies(new Uri("https://ja3er.com/json"), out cookies);
            Class1.TryGetCookies(new Uri("https://www.bet365.com/"), out cookies);
            int i;
            for (i = 0; i < cookies.Length; i++)
                Console.WriteLine(cookies[i]);
        }
    }
}
