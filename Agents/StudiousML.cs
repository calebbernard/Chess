using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    class StudiousML : ML
    {
        protected Player RealAI;
        protected int guesses;
        protected int correctGuesses;

        public StudiousML(string _name, Random _rng, Player _realAI) : base(_name, _rng)
        {
            RealAI = _realAI;
            guesses = 0;
            correctGuesses = 0;
        }

        public override Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> options)
        {
            mind.updateHits(b, playerColor);
            Tuple<Coord, Coord, SpecialMove> myChoiceTuple = base.chooseMove(options);
            Tuple<Coord, Coord, SpecialMove> realChoiceTuple = RealAI.chooseMove(options);
            guesses++;

            if (myChoiceTuple.Item1.col == realChoiceTuple.Item1.col &&
                myChoiceTuple.Item1.row == realChoiceTuple.Item1.row &&
                myChoiceTuple.Item2.col == realChoiceTuple.Item2.col &&
                myChoiceTuple.Item2.row == realChoiceTuple.Item2.row &&
                myChoiceTuple.Item3 == realChoiceTuple.Item3)
            {
                correctGuesses++;
            }
            return RealAI.chooseMove(options);
        }

        public double getGuessRatio()
        {
            return (double)correctGuesses / (double)guesses;
        }

        public override void setBoard(Board _b)
        {
            base.setBoard(_b);
            RealAI.setBoard(_b);
        }

        public override void startup()
        {
            base.startup();
            guesses = 0;
            correctGuesses = 0;
        }
    }
}
