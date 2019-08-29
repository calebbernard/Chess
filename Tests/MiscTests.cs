using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tests
{
    class MiscTests : TestSuite
    {
        public MiscTests() : base()
        {
            tests.Add(new RandomMatch());
            tests.Add(new GreedyMatch());
        }
    }

    class RandomMatch : Test
    {
        public RandomMatch()
        {
            // idea is to shake out any end-to-end bugs or exception throwing
            description = "Play full game between two random agents";
        }
        public override bool run()
        {
            Random rng = new Random();
            // Arrange
            var e = new Engine(new Agents.RandomPlayer(rng), new Agents.RandomPlayer(rng));

            // Act
            e.play();

            // Assert
            // As this test is meant to look for crashes, simply finishing a match without crashing is good enough.
            return true;
        }
    }

    class GreedyMatch : Test
    {
        public GreedyMatch()
        {
            description = "Play full game between two Greedy bots";
        }
        public override bool run()
        {
            Random rng = new Random();
            // Arrange
            var e = new Engine(new Agents.Greedy(rng), new Agents.Greedy(rng));

            // Act
            e.play();

            // Assert
            // As this test is meant to look for crashes, simply finishing a match without crashing is good enough.
            return true;
        }
    }
}
