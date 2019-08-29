using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Agents
{
    [Serializable]
    abstract class Player
    {
        public Color playerColor;
        public Board b;
        public string name;
        protected Random rng;

        public abstract Tuple<Coord, Coord, SpecialMove> chooseMove(List<Move> moves);

        public virtual void finish()
        {

        }

        public virtual void setBoard(Board _b)
        {
            b = _b;
        }
        public virtual void startup()
        {

        }
    }
}
