using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Chess.Library;

namespace Chess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running tests...");
            if (!new Tests.TestController().run())
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }
            Random rng = new Random();
            Stopwatch sw = new Stopwatch();
            

            var Player1 = new Agents.StudiousML("StudiousML-Claustrophilia", rng, new Agents.Claustrophilia());
            var Player2 = new Agents.RandomPlayer(rng);
            
            // eventually, config this
            Console.WriteLine("spacebar to visualize, else just the digest");
            var k = Console.ReadKey();
            bool visualize = false;
            if (k.Key == ConsoleKey.Spacebar)
            {
                visualize = true;
            }

            Engine e = new Engine(Player1, Player2, rng);
            e.observeDisplay = visualize;
            e.style = ObserveStyle.ScrubToInteresting;
            int games = 0;
            
            while (true)
            {
                games++;
                if (!visualize)
                {
                    sw.Reset();
                    sw.Start();
                }
                var result = e.play();
                if (!visualize)
                {
                    sw.Stop();
                    Console.WriteLine("Game {0}: {1} - Time: {2}, MLRatio: {3}", games, result, sw.Elapsed, Player1.getGuessRatio());
                }
            }
        }
    }
}
