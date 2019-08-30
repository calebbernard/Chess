using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Chess.Agents
{

    class ML : Player {
        public RuleSet mind;
        int rulesCapacity;
        int cullFrequency;
        public string fileName;
        public Random rng;
        List<Tuple<int, int>> rulesCapacityScaling;
        List<Tuple<int, int>> cullFrequencyScaling;

        public ML(string _name, Random _rng) : base()
        {
            name = _name;
            rng = _rng;
            mind = new RuleSet();
            fileName = "./" + name + ".txt";
            Console.WriteLine(name + " - initializing...");
            if (File.Exists(fileName))
            {
                Console.WriteLine(name + " - loading save data...");
                load();
            }
            scale();
            fillRules();
            save();
            Console.WriteLine(name + " -  initialization complete.");
        }

        protected void setupScale()
        {
            if (rulesCapacityScaling == null)
            {
                rulesCapacityScaling = new List<Tuple<int, int>>();
                rulesCapacityScaling.Add(Tuple.Create(0, 100));
                rulesCapacityScaling.Add(Tuple.Create(10, 500));
                rulesCapacityScaling.Add(Tuple.Create(100, 1000));
                rulesCapacityScaling.Add(Tuple.Create(250, 5000));
                rulesCapacityScaling.Add(Tuple.Create(500, 10000));
            }
            if (cullFrequencyScaling == null)
            {
                cullFrequencyScaling = new List<Tuple<int, int>>();
                cullFrequencyScaling.Add(Tuple.Create(0, 10));
            }
        }

        protected void scale()
        {
            setupScale();

            // DRY this (include variable reference in tuples?)
            // alternatively leave it broken out for now in case i want to scale on several different criteria
            // (I can eventually DRY that too of course)

            foreach (var entry in rulesCapacityScaling)
            {
                if (mind.experience >= entry.Item1)
                {
                    rulesCapacity = entry.Item2;
                } else
                {
                    break;
                }
            }

            foreach (var entry in cullFrequencyScaling)
            {
                if (mind.experience >= entry.Item1)
                {
                    cullFrequency = entry.Item2;
                } else
                {
                    break;
                }
            }
        }

        protected double hardScoreEndings(double score, Board b)
        {
            if (b.state != GameState.Active)
            {
                if (Operations.winner(playerColor, b) == 0)
                {
                    // avoid draw at all costs. prefer loss to draw. only draw if there is no non-drawing move available.
                    // undone a year later, moving away from this
                    score = 0;
                }
                else if (Operations.winner(playerColor, b) == 1)
                {
                    score = 1;
                }
                else if (Operations.winner(playerColor, b) == -1)
                {
                    score = -1;
                }
            }
            return score;
        }

        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> options)
        {
            mind.updateHits(b, playerColor);
            Move bestMove = null;
            Double bestScore = int.MinValue;
            foreach(Move m in options){
                Board dream = b.clone();
                dream.move(m.from.pos, m.to.pos, m.special);
                dream.afterTurn();
                Double score = hardScoreEndings(mind.score(dream, playerColor), dream);
                if (score > bestScore)
                {
                    bestMove = m;
                    bestScore = score;
                }
            }
            return Tuple.Create(bestMove.from.pos, bestMove.to.pos, bestMove.special);
        }

        public override void finish()
        {
            mind.updateHits(b, playerColor);
            int result = Operations.winner(playerColor, b);
            mind.experience++;
            mind.finish(result);
            scale();
            if (mind.experience % cullFrequency == 0)
            {
                cull();
            }
            if (mind.rules.Count < rulesCapacity)
            {
                fillRules();
            }
            if (result != 0)
            {
                Console.WriteLine(name + " - saving");
                save();
                Console.WriteLine("Done saving");
            }
        }

        public void save()
        {
            Console.WriteLine(name + " - saving");
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, mind);
            }
        }

        public void load()
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                mind = (RuleSet)bformatter.Deserialize(stream);
            }
            foreach (Rule r in mind.rules)
            {
                r.hit = false;
            }
        }

        public void fillRules()
        {
            if (mind.rules.Count >= rulesCapacity)
            {
                return;
            }
            Console.WriteLine(name + " - filling to capacity...");
            var basics = RuleSet.basicStatements();
            var universe = basics.Union(mind.rules.Where(x => x.proposition.output().Length <= 15));
            while (mind.rules.Count < rulesCapacity)
            {
                var a = universe.ElementAt(rng.Next(universe.Count())).proposition;
                var b = universe.ElementAt(rng.Next(universe.Count())).proposition;
                var newRule = new Rule(new TwoTermExpression(a, b, rng));
                // Prevent rule duplication
                bool already = false;
                foreach (Rule r in mind.rules)
                {
                    if (r.proposition == newRule.proposition)
                    {
                        already = true;
                    }
                }
                if (!already)
                {
                    mind.rules.Add(new Rule(new TwoTermExpression(a, b, rng)));
                    universe = basics.Union(mind.rules.Where(x => x.proposition.output().Length <= 15));
                }
            }
        }

        public double howOpinionated()
        {
            double opinion = 0;
            foreach(Rule r in mind.rules)
            {
                opinion += Math.Abs(r.score() - 0.5);
            }
            return opinion / mind.rules.Count;
        }

        public double rulesTouched()
        {
            int rulesTouched = mind.rules.Where(x => x.score() != 0.5).Count();
            return (double)rulesTouched / (double)(rulesTouched + mind.rules.Count);
        }
        
        protected int sortCompare(Rule a, Rule b)
        {
            Double aScore = Math.Abs(a.score() - 0.5);
            Double bScore = Math.Abs(b.score() - 0.5);
            if (aScore > bScore)
            {
                return 1;
            } else if (bScore > aScore)
            {
                return -1;
            } else { return 0; }
        }

        public void cull()
        {
            var nextMind = new RuleSet();
            foreach (Rule r in mind.rules)
            {
                if (r.score() != 0.5)
                {
                    nextMind.rules.Add(r);
                }
            }
            nextMind.rules.Sort(sortCompare);
            // wipe the bottom 10%
            nextMind.rules.RemoveRange(nextMind.rules.Count - nextMind.rules.Count/10, nextMind.rules.Count/10);
            mind = nextMind;
            fillRules();
        }
    }

    [Serializable]
    class RuleSet
    {
        bool frozen;
        public List<Rule> rules;
        public int experience;

        public RuleSet()
        {
            rules = new List<Rule>();
            frozen = false;
            experience = 0;
        }

        public double score(Board b, Color pc){
            Double score = 0;
            var trueRules = rules.Where(x => x.proposition.resolve(b, pc) == true);
            foreach (Rule r in trueRules)
            {
                score += r.score();
            }
            int size = trueRules.Count();
            return size == 0 ? 0.5 : score / Math.Max(1, size); ;
        }

        public void updateHits(Board b, Color pc){
            if (frozen)
            {
                return;
            }
            foreach(Rule r in rules){
                if(!r.hit && r.proposition.resolve(b, pc) == true) {
                    r.hit = true;
                }
            }
        }

        public void finish (int result) {
            foreach(Rule r in rules.Where(x => x.hit == true)){
                if(result == 1) {
                    r.winCount += 1;
                } else if (result == -1){
                    r.lossCount += 1;
                }
                r.hit = false;
            }
        }

        public static List<Rule> basicStatements()
        {
            var result = new List<Rule>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int z = 0; z < 7; z++)
                    {
                        result.Add(new Rule(new PieceStatement(true, (Pieces)z, new Coord(x, y))));
                        if ((Pieces)z != Pieces.None)
                        {
                            result.Add(new Rule(new PieceStatement(false, (Pieces)z, new Coord(x, y))));
                        }
                    }
                }
            }
            return result;
        }
    }

    [Serializable]
    class Rule
    {
        public IBooleanResolvable proposition;
        public int winCount;
        public int lossCount;
        public bool hit;

        public Rule(IBooleanResolvable _proposition, int _winCount = 0, int _lossCount = 0, int _drawCount = 0)
        {
            proposition = _proposition;
            winCount = _winCount;
            lossCount = _lossCount;
            hit = false;
        }

        public string output(){
            return proposition.output();
        }

        public double score(){
            if (winCount + lossCount == 0)
            {
                // return 0.5; // prevent div by 0
                return 0.76; // new idea: prefer the unknown to a mediocre known
            }
            return ((double)winCount / (double)(winCount + lossCount));
        }
    }

    enum Operators
    {
        T,
        OR,
        ThenIf,
        P,
        IfThen,
        Q,
        XNOR,
        AND,
        NAND,
        XOR,
        NotQ,
        MaterialNonimplication,
        NotP,
        ConverseNonimplication,
        NOR,
        F
    }

    [Serializable]
    abstract class IBooleanResolvable
    {
        public abstract bool resolve(Board b, Color pc);
        public abstract string output();
        public abstract void increaseOperatorPriority();

        public abstract string humanOutput();
    }

    [Serializable]
    class TwoTermExpression : IBooleanResolvable {
        IBooleanResolvable termA;

        IBooleanResolvable termB;

        int boolOperator;

        public TwoTermExpression(string input) {
            int lowestPriorityOperator = 257;
            int lowestPriorityOperatorIndex = -1;
            for (int i = 2; i < input.Length; i += 3)
            {
                if ((int)input[i] < lowestPriorityOperator)
                {
                    lowestPriorityOperator = (int)input[i];
                    lowestPriorityOperatorIndex = i;
                }
            }
            string partA = input.Substring(0, lowestPriorityOperatorIndex);
            string partB = input.Substring(lowestPriorityOperatorIndex + 1);
            if (partA.Length == 2)
            {
                termA = new PieceStatement(partA);
            } else
            {
                termA = new TwoTermExpression(partA);
            }
            if (partB.Length == 2)
            {
                termB = new PieceStatement(partB);
            }
            else
            {
                termB = new TwoTermExpression(partB);
            }
            boolOperator = lowestPriorityOperator;
        }

        public TwoTermExpression(IBooleanResolvable _a, IBooleanResolvable _b, Random rng, int _operator = -1)
        {
            termA = _a;
            termB = _b;
            termA.increaseOperatorPriority();
            termB.increaseOperatorPriority();
            if (_operator < 0)
            {
                boolOperator = rng.Next(0, 15); // random number min 0 max 15
                // consider reorganizing the operators to cut down on fill time
                while (boolOperator == (int)Operators.T ||
                        boolOperator == (int)Operators.F ||
                        boolOperator == (int)Operators.P ||
                        boolOperator == (int)Operators.Q ||
                        boolOperator == (int)Operators.T ||
                        boolOperator == (int)Operators.NotP ||
                        boolOperator == (int)Operators.NotQ)
                {
                    boolOperator = rng.Next(0, 15); // I think those operators are logically worthless here
                }
               
            }
            else
            {
                boolOperator = _operator;
            }
        }

        private bool binaryLogic(bool termA, bool termB, int binaryOperator)
        {
            int a = termA == true ? 0 : 1;
            int b = termB == true ? 0 : 1;
            // document out what operatorid = what operation (enum?)
            return (new bool[4, 16] { { true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false },
                { true, true, true, true, false, false, false, false, true, true, true, true, false, false, false, false },
                { true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false },
                { true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false } })[(a * 2) + b, binaryOperator];
        }

        public override bool resolve(Board b, Color pc)
        {
            bool _a = termA.resolve(b, pc);

            bool _b = termB.resolve(b, pc);
            int operatorType = boolOperator % 16;
            return binaryLogic(_a, _b, operatorType);
            
        }
        public override string output()
        {
            return termA.output() + ((char)boolOperator).ToString() + termB.output();
        }

        public override void increaseOperatorPriority()
        {
            boolOperator += 16;
            termA.increaseOperatorPriority();
            termB.increaseOperatorPriority();
        }

        public override string humanOutput()
        {
            return "( " + termA.humanOutput() + " ) " + ((Operators)boolOperator).ToString() + " ( " + termB.humanOutput() + " )";
        }
    }

    [Serializable]
    class PieceStatement : IBooleanResolvable {
        bool ally;
        Pieces type;
        int pos;

        public PieceStatement(Random rng)
        {
            ally = rng.Next(2) == 0 ? true : false;
            type = (Pieces)rng.Next(7);
            pos = new Coord(rng.Next(8), rng.Next(8)).toInt();
        }

        public PieceStatement(bool _ally, Pieces _type, Coord _pos)
        {
            ally = _ally;
            type = _type;
            pos = _pos.toInt();
        }

        public PieceStatement(string input)
        {
            int basicType = input[0];
            int basicPos = input[1];
            ally = true;
            if (basicType >= 7)
            {
                ally = false;
                basicType -= 7;
            }
            type = (Pieces)basicType;
            pos = new Coord(basicPos / 8, basicPos % 8).toInt();
        }

        public override bool resolve(Board b, Color pc)
        {
            Color c = ally ? pc : Operations.oppositeColor(pc);
            Coord cPos = Coord.fromInt(pos);
            Coord actualPos = pc == Color.White ? cPos : new Coord(7 - cPos.row, 7 - cPos.col);
            Piece p = b.getTile(actualPos).piece;
            if (p == null)
            {
                return type == Pieces.None;
            } else
            {
                return p.color == c && p.kind == type;
            }
        }

        public override string output()
        {
            int basicType = (int)type;
            int advancedType = ally == true ? basicType : basicType + 7;
            Coord cPos = Coord.fromInt(pos);
            return ((char)advancedType).ToString() + ((char)(cPos.row * 8 + cPos.col)).ToString();
        }

        public override string humanOutput()
        {
            string result = "";
            result += ally ? "ally " : "enemy ";
            result += type.ToString() + " ";
            result += "at " + pos.ToString();
            return result;
        }

        public override void increaseOperatorPriority()
        {

        }
    }
}