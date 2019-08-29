using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tests
{
    class BoardOperationTests : TestSuite
    {
        public BoardOperationTests() : base()
        {
            tests.Add(new BoardClear());
            tests.Add(new BoardGet());
            tests.Add(new BoardSet());
            tests.Add(new Checkmate());
        }
    }
    class BoardClear : Test
    {
        public BoardClear()
        {
            description = "Board.Clear()";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board();

            // Act
            b.clear();

            // Assert
            return b.getTile(0, 0).piece == null;
        }
    }

    class BoardGet : Test
    {
        public BoardGet()
        {
            description = "Board.Get()";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board();

            // Act
            Piece p = b.get("A8");

            // Assert
            return p.GetType() == typeof(Rook);
        }
    }

    class BoardSet : Test
    {
        public BoardSet()
        {
            description = "Board.Set()";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board();
            b.clear();

            // Act
            b.set("white rook d3");

            // Assert
            return (b.get("d3").GetType() == typeof(Rook) && b.get("d3").color == Color.White);
        }
    }

    class Checkmate : Test
    {
        public Checkmate()
        {
            description = "Checkmate";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board();
            b.clear();
            b.set("black rook a1");
            b.set("black rook a2");
            b.set("white king h1");
            b.afterTurn();

            // Assert
            return b.state == GameState.BlackVictory;
        }
    }
}
