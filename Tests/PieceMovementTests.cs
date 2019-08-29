using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Tests
{
    class PieceMovementTests : TestSuite
    {
        public PieceMovementTests() : base()
        {
            tests.Add(new PawnStandardMove());
            tests.Add(new PawnsCantMoveBackwards());
            tests.Add(new PawnsCanDoubleHopOnStartTile());
            tests.Add(new PawnsCantDoubleHopWhenNotOnStartTile());
            tests.Add(new PawnStandardCaptures());
            tests.Add(new PawnIllegalCaptures());
            tests.Add(new KingSideCastle());
            tests.Add(new QueenSideCastle());
            tests.Add(new KingMove());
            tests.Add(new RookMove());
            tests.Add(new RookIllegalMoves());
        }
    }
    class PawnStandardMove : Test
    {
        public PawnStandardMove()
        {
            description = "Pawn can hop one square forward.";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.enforceTurns = false;
            b.clear();
            // test both white and black pawns since they move differently (consider breaking this out into two tests
            b.set("black pawn d4");
            b.set("white pawn e4");

            // Act
            // // move each tile one square forwards in their respective directions
            b.move(Coord.fromString("d4"), Coord.fromString("d3"));
            b.move(Coord.fromString("e4"), Coord.fromString("e5"));

            // Assert
            // the pawns should be on their respective destination tiles
            return (
                b.checkTile(Coord.fromString("d3")) == "Black Pawn" &&
                b.checkTile(Coord.fromString("e5")) == "White Pawn");
        }
    }

    class PawnsCantMoveBackwards : Test
    {
        public PawnsCantMoveBackwards()
        {
            description = "Pawns can't move backwards";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            // testing both colors since they move differently
            b.set("black pawn d4");
            b.set("white pawn e4");

            // Act
            // attempt to move both pieces backwards. These moves should fail
            b.move(Coord.fromString("d4"), Coord.fromString("d5"));
            b.move(Coord.fromString("e4"), Coord.fromString("e3"));

            // Assert
            return (
                b.checkTile(Coord.fromString("d4")) == "Black Pawn" &&
                b.checkTile(Coord.fromString("e4")) == "White Pawn");
        }
    }

    class PawnsCanDoubleHopOnStartTile : Test
    {
        public PawnsCanDoubleHopOnStartTile()
        {
            description = "Pawns can double hop on start tile";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black pawn a7");
            b.set("white pawn b2");

            // Act
            // hop two tiles
            b.move(Coord.fromString("a7"), Coord.fromString("a5"));
            b.move(Coord.fromString("b2"), Coord.fromString("b4"));

            // Assert
            return (b.checkTile(Coord.fromString("a5")) == "Black Pawn" &&
                b.checkTile(Coord.fromString("b4")) == "White Pawn");
        }
    }

    class PawnsCantDoubleHopWhenNotOnStartTile : Test
    {
        public PawnsCantDoubleHopWhenNotOnStartTile()
        {
            description = "Pawns can't double hop when not on start tile";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black pawn a6");
            b.set("white pawn b3");

            // Act
            // These double hop moves should fail.
            b.move(Coord.fromString("a6"), Coord.fromString("a4"));
            b.move(Coord.fromString("b3"), Coord.fromString("b5"));

            // Assert
            return (b.checkTile(Coord.fromString("a6")) == "Black Pawn" &&
                    b.checkTile(Coord.fromString("b3")) == "White Pawn");
        }
    }

    class PawnStandardCaptures : Test
    {
        public PawnStandardCaptures()
        {
            description = "Standard pawn captures";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            // black to capture white piece
            b.set("black pawn e3");
            b.set("white pawn f2");
            // white to capture that black piece (now on f2)
            b.set("white pawn g1");

            // Act
            // black, then white captures on f2
            b.move(Coord.fromString("e3"), Coord.fromString("f2"));
            b.move(Coord.fromString("g1"), Coord.fromString("f2"));

            // Assert
            // there should only be one piece on the board, the white pawn on f2.
            return (b.checkTile(Coord.fromString("f2")) == "White Pawn" &&
                    b.checkTile(Coord.fromString("e3")) == "" &&
                    b.checkTile(Coord.fromString("g1")) == "");
        }
    }

    class PawnIllegalCaptures : Test
    {
        public PawnIllegalCaptures()
        {
            description = "Illegal pawn captures";
        }

        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            // both of these can try and fail to capture each other
            b.set("black pawn d4");
            b.set("white pawn e5");
            // make sure they don't capture own color
            b.set("black pawn e3");
            b.set("white pawn d6");
            // can't capture straight ahead
            b.set("black pawn d7");

            // Act
            b.move(Coord.fromString("d4"), Coord.fromString("e5"));
            b.move(Coord.fromString("e5"), Coord.fromString("d4"));
            b.move(Coord.fromString("e5"), Coord.fromString("d6"));
            b.move(Coord.fromString("d4"), Coord.fromString("e3"));
            b.move(Coord.fromString("d6"), Coord.fromString("d7"));

            // Assert
            // All pieces should be in their original positions
            return (b.checkTile(Coord.fromString("d4")) == "Black Pawn" &&
                    b.checkTile(Coord.fromString("e5")) == "White Pawn" &&
                    b.checkTile(Coord.fromString("e3")) == "Black Pawn" &&
                    b.checkTile(Coord.fromString("d6")) == "White Pawn" &&
                    b.checkTile(Coord.fromString("d7")) == "Black Pawn");
        }
    }

    class KingSideCastle : Test
    {
        public KingSideCastle()
        {
            description = "Kingside castle";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            int pass = 0; // necessary because the first one is blocking the second one. Maybe a sign that I should break this into two tests?
            b.set("black king e8");
            b.set("black rook h8");

            // Act
            b.move(Coord.fromString("e8"), Coord.fromString("g8"));

            // Assert
            if (b.checkTile(Coord.fromString("g8")) == "Black King" &&
                    b.checkTile(Coord.fromString("f8")) == "Black Rook")
            {
                pass += 1;
            }

            // Arrange
            b.clear();
            b.set("white king e1");
            b.set("white rook h1");

            // Act
            b.move(Coord.fromString("e1"), Coord.fromString("g1"));

            // Assert
            if ( b.checkTile(Coord.fromString("g1")) == "White King" &&
                    b.checkTile(Coord.fromString("f1")) == "White Rook")
            {
                pass += 1;
            }

            return pass == 2;
        }
    }

    class QueenSideCastle : Test
    {
        public QueenSideCastle()
        {
            description = "Queenside castle";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            int pass = 0;
            b.set("black king e8");
            b.set("black rook a8");

            // Act
            b.move(Coord.fromString("e8"), Coord.fromString("c8"));

            // Assert
            if (b.checkTile(Coord.fromString("c8")) == "Black King" &&
                    b.checkTile(Coord.fromString("d8")) == "Black Rook")
            {
                pass += 1;
            }

            // Arrange
            b.clear();
            b.set("white king e1");
            b.set("white rook a1");

            // Act
            b.move(Coord.fromString("e1"), Coord.fromString("c1"));

            // Assert
            if (b.checkTile(Coord.fromString("c1")) == "White King" &&
                    b.checkTile(Coord.fromString("d1")) == "White Rook")
            {
                pass += 1;
            }

            return pass == 2;
        }
    }

    class KingMove : Test
    {
        public KingMove()
        {
            description = "King moves";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black king e4");
            int successfulMoves = 0;

            // Act
            // move in a circle, verify success after each move
            b.move(Coord.fromString("e4"), Coord.fromString("e5")); // up
            if (b.checkTile(Coord.fromString("e5")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("e5"), Coord.fromString("f6")); // up-right
            if (b.checkTile(Coord.fromString("f6")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("f6"), Coord.fromString("g6")); // right
            if (b.checkTile(Coord.fromString("g6")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("g6"), Coord.fromString("h5")); // down-right
            if (b.checkTile(Coord.fromString("h5")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("h5"), Coord.fromString("h4")); // down
            if (b.checkTile(Coord.fromString("h4")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("h4"), Coord.fromString("g3")); // down-left
            if (b.checkTile(Coord.fromString("g3")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("g3"), Coord.fromString("f3")); // left
            if (b.checkTile(Coord.fromString("f3")) == "Black King")
            {
                successfulMoves += 1;
            }
            b.move(Coord.fromString("f3"), Coord.fromString("e4")); // up-left
            if (b.checkTile(Coord.fromString("e4")) == "Black King")
            {
                successfulMoves += 1;
            }

            // Assert
            return successfulMoves == 8;
        }
    }

    /*
     * King illegal moves
     * Illegal castles
     *  Queen move
     *  Rook move
     *  Bishop move
     *  Knight move
     */

    class RookMove : Test
    {
        public RookMove()
        {
            description = "Legal rook moves and captures";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("black rook e4");
            b.set("white pawn a8");
            b.set("white pawn b8");

            // Act
            // single hop up
            b.move(Coord.fromString("e4"), Coord.fromString("e5"));
            // double hop right
            b.move(Coord.fromString("e5"), Coord.fromString("g5"));
            // triple hop down
            b.move(Coord.fromString("g5"), Coord.fromString("g2"));
            // all the way to the left
            b.move(Coord.fromString("g2"), Coord.fromString("a2"));
            // capture at long range
            b.move(Coord.fromString("a2"), Coord.fromString("a8"));
            // capture neighbor
            b.move(Coord.fromString("a8"), Coord.fromString("b8"));

            // Assert
            return (b.checkTile(Coord.fromString("b8")) == "Black Rook" &&
                    b.checkTile(Coord.fromString("a8")) == "");
        }
    }

    class RookIllegalMoves : Test
    {
        public RookIllegalMoves()
        {
            description = "Rook illegal moves";
        }
        public override bool run()
        {
            // Arrange
            var b = new Board(false);
            b.clear();
            b.set("white rook e4");
            b.set("black pawn e5");
            b.set("white pawn e3");
            int pass = 0;

            // Act
            // can't move through enemy
            b.move(Coord.fromString("e4"), Coord.fromString("e6"));
            if (b.checkTile("e4") == "White Rook")
            {
                pass += 1;
            }
            // can't move to square occupied by ally
            b.move(Coord.fromString("e4"), Coord.fromString("e3"));
            if (b.checkTile("e4") == "White Rook")
            {
                pass += 1;
            }
            // can't move through ally
            b.move(Coord.fromString("e4"), Coord.fromString("e2"));
            if (b.checkTile("e4") == "White Rook")
            {
                pass += 1;
            }

            // Assert
            return pass == 3;
        }
    }
}
