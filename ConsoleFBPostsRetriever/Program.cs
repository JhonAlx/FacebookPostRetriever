using JackLeitch.RateGate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFBPostsRetriever
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var rg = new RateGate(1600, TimeSpan.FromHours(1)))
            {

            }
        }
    }
}
