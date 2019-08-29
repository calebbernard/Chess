using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tests
{
    class MLAgentTests : TestSuite
    {
        public MLAgentTests() : base()
        {
            tests.Add(new PieceStatementTest());
            tests.Add(new CompoundStatementTest());
            tests.Add(new OperatorTest());
            tests.Add(new GeneratePieceStatementsTest());
            tests.Add(new AcquireKnowledgeTest());
            tests.Add(new KnowledgePersistsAcrossColorTest());
        }
    }

    class PieceStatementTest : Test
    {
        public PieceStatementTest()
        {
            description = "Check that a piece statement correctly resolves";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board();
            var l = new List<Agents.PieceStatement>();
            foreach (Piece p in b.pieces)
            {
                bool ally = p.color == Color.Black ? true : false;
                l.Add(new Agents.PieceStatement(ally, p.kind, p.tile.pos.flip()));
            }
            l.Add(new Agents.PieceStatement(true, Pieces.None, new Coord(4, 4)));

            // Act
            bool allTrue = true;
            foreach (Agents.PieceStatement s in l)
            {
                if (!s.resolve(b, Color.Black))
                {
                    allTrue = false;
                }
            }

            // Assert
            return allTrue;

        }
    }

    class CompoundStatementTest : Test
    {
        public CompoundStatementTest()
        {
            description = "ML Compound Statement";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board();
            var and1 = new Agents.PieceStatement(true, Pieces.Rook, Coord.fromString("A1"));
            var and2 = new Agents.PieceStatement(true, Pieces.Knight, Coord.fromString("B1"));
            var proposition = new Agents.TwoTermExpression(and1.output() + (char)Agents.Operators.AND + and2.output());
            var part2 = new Agents.PieceStatement(true, Pieces.Rook, Coord.fromString("B1"));
            var nextProposition = new Agents.TwoTermExpression(proposition.output() + (char)Agents.Operators.XOR + part2.output());

            // Assert
            return (proposition.resolve(b, Color.Black) && nextProposition.resolve(b, Color.Black));
        }
    }

    class OperatorTest : Test
    {
        public OperatorTest()
        {
            description = "Test boolean operators";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board();
            var trueProp = new Agents.PieceStatement(true, Pieces.Rook, Coord.fromString("A1"));
            var falseProp = new Agents.PieceStatement(true, Pieces.Rook, Coord.fromString("A2"));
            String[] results = { "TTTT", "TTTF", "TTFT", "TTFF", "TFTT", "TFTF", "TFFT", "TFFF", "FTTT", "FTTF", "FTFT", "FTFF", "FFTT", "FFTF", "FFFT", "FFFF" };
            int successes = 0;

            // Act
            for (int x = 0; x < 16; x++)
            {
                string outcome = "";
                var props = new List<Agents.TwoTermExpression>();
                props.Add(new Agents.TwoTermExpression(trueProp.output() + (char)x + trueProp.output()));
                props.Add(new Agents.TwoTermExpression(trueProp.output() + (char)x + falseProp.output()));
                props.Add(new Agents.TwoTermExpression(falseProp.output() + (char)x + trueProp.output()));
                props.Add(new Agents.TwoTermExpression(falseProp.output() + (char)x + falseProp.output()));
                foreach(Agents.TwoTermExpression expression in props)
                {
                    outcome += expression.resolve(b, Color.Black) ? "T" : "F";
                }
                if (outcome == results[x])
                {
                    successes += 1;
                }
            }

            // Assert
            return successes == 16;
        }
    }

    class GeneratePieceStatementsTest : Test
    {
        public GeneratePieceStatementsTest()
        {
            description = "Generating all basic piece statements";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board();
            var r = new Agents.RuleSet();
            r.rules = Agents.RuleSet.basicStatements();
            int trueRules = 0;

            // Act
            foreach (Agents.Rule rule in r.rules)
            {
                trueRules += rule.proposition.resolve(b, Color.Black) ? 1 : 0;
            }

            // Assert
            return (r.rules.Count == 832 && trueRules == 64);
        }
    }

    class AcquireKnowledgeTest : Test
    {
        public AcquireKnowledgeTest()
        {
            description = "Acquire knowledge";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black king e4");
            var ML = new Agents.ML("placeholder", new Random());
            ML.b = b;
            ML.playerColor = Color.Black;
            var p = new Agents.PieceStatement(true, Pieces.King, Coord.fromString("e5").flip());
            var r = new Agents.Rule(p, 0, 0);
            ML.mind.rules.Add(r);
            bool foundMove = false;
            while (!foundMove)
            {
                var m = ML.chooseMove(b.availableMoves(Color.Black));
                if (m.Item2.row == Coord.fromString("e5").row &&
                    m.Item2.col == Coord.fromString("e5").col)
                {
                    foundMove = true;
                }
                //b.display();
                //Console.ReadKey();
            }
            b.clear();
            b.set("black king e5");
            b.state = GameState.BlackVictory;
            ML.finish();
            b.state = GameState.Active;
            b.clear();
            b.set("black king e4");
            // Act
            int count = 0;
            int target = 10;
            for (int x = 0; x < target; x++)
            {
                var m = ML.chooseMove(b.availableMoves(Color.Black));
                if (m.Item2.row == Coord.fromString("e5").row &&
                    m.Item2.col == Coord.fromString("e5").col)
                {
                    count++;
                }
            }

            // Assert
            return count == target;
        }
    }

    class KnowledgePersistsAcrossColorTest : Test
    {
        public KnowledgePersistsAcrossColorTest()
        {
            description = "Knowledge persists across colors";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black king h8");
            var ML = new Agents.ML("placeholder", new Random());
            ML.b = b;
            ML.rng = new Random();
            // moving to the right = win
            ML.mind.rules.Add(new Agents.Rule(new Agents.PieceStatement(true, Pieces.King, Coord.fromString("b1")), 1, 0, 0));
            // moving up-right = loss
            ML.mind.rules.Add(new Agents.Rule(new Agents.PieceStatement(true, Pieces.King, Coord.fromString("b2")), 0, 1, 0));
            // moving down = tie
            ML.mind.rules.Add(new Agents.Rule(new Agents.PieceStatement(true, Pieces.King, Coord.fromString("a2")), 0, 0, 1));

            // Act
            ML.playerColor = Color.Black;
            var m1 = ML.chooseMove(b.availableMoves(Color.Black));
            b.clear();
            b.set("white king a1");

            ML.playerColor = Color.White;
            var m2 = ML.chooseMove(b.availableMoves(Color.White));

            // Assert
            return (m1.Item2.row == Coord.fromString("g8").row && m1.Item2.col == Coord.fromString("g8").col &&
                    m2.Item2.row == Coord.fromString("b1").row && m2.Item2.col == Coord.fromString("b1").col);
        }
    }
}


