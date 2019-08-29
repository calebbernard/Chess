using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tests
{
    class TestController
    {
        TestSuite fullSuite;

        public TestController()
        {
            fullSuite = new TestSuite();

            // Add new test sets to the suit here
            fullSuite.subTests.Add(new BoardOperationTests());
            fullSuite.subTests.Add(new PieceMovementTests());
            // MiscTests has full-game bot testing and can take a while so prefer to comment out unless specifically testing that
            //fullSuite.subTests.Add(new MiscTests());
            //fullSuite.subTests.Add(new MLAgentTests());
        }

        public bool run()
        {
            bool result = false;
            string output = fullSuite.run();
            if (output.Length == 0)
            {
                result = true;
                output = "All tests passed.";
            }
            Console.WriteLine(output);
            return result;
        }

    }

    class TestSuite
    {
        public List<TestSuite> subTests;
        public List<Test> tests;

        public TestSuite()
        {
            subTests = new List<TestSuite>();
            tests = new List<Test>();
        }

        public string run()
        {
            string output = "";
            foreach (Test t in tests)
            {
                // Console.WriteLine("Testing: " + t.description);
                if (t.run() == false)
                {
                    output += "Failed: " + t.description + "\n";
                }
            }
            foreach (TestSuite t in subTests)
            {
                output += t.run();
            }
            return output;
        }
    }

    abstract class Test
    {
        public string description;
        abstract public bool run();
    }

    class IntentionalBadTest : Test // who watches the watchers?!
    {
        public IntentionalBadTest()
        {
            description = "This test is designed to fail. Don't include it in a real test suite.";
        }

        public override bool run()
        {
            return false;
        }
    }
}
