using System;
using System.Collections.Generic;

public class Constants
{
	public class PieceCodes
	{
		// The order in which pieces are stored in the prefab array

		public static int BlackPawn = 0;
		public static int WhitePawn = 1;
		public static int BlackRook = 2;
		public static int WhiteRook = 3;
		public static int BlackBishop = 4;
		public static int WhiteBishop = 5;
		public static int BlackKnight = 6;
		public static int WhiteKnight = 7;
		public static int BlackQueen = 8;
		public static int WhiteQueen = 9;
		public static int BlackKing = 10;
		public static int WhiteKing = 11;

		public static Dictionary<int, Type> pieceCodeToPieceTypeMapping = new Dictionary<int, Type> {
			{BlackPawn, typeof(Pawn)},
			{WhitePawn, typeof(Pawn)},
			{BlackRook, typeof(Rook)},
			{WhiteRook, typeof(Rook)},
			{BlackBishop, typeof(Bishop)},
			{WhiteBishop, typeof(Bishop)},
			{BlackKnight, typeof(Knight)},
			{WhiteKnight, typeof(Knight)},
			{BlackQueen, typeof(Queen)},
			{WhiteQueen, typeof(Queen)},
			{BlackKing, typeof(King)},
			{WhiteKing, typeof(King)}
		};
	}

	public class PieceNames
	{
		public static string BlackPawn = "BlackPawn";
		public static string WhitePawn = "WhitePawn";
		public static string BlackRook = "BlackRook";
		public static string WhiteRook = "WhiteRook";
		public static string BlackBishop = "BlackBishop";
		public static string WhiteBishop = "WhiteBishop";
		public static string BlackKnight = "BlackKnight";
		public static string WhiteKnight = "WhiteKnight";
		public static string BlackQueen = "BlackQueen";
		public static string WhiteQueen = "WhiteQueen";
		public static string BlackKing = "BlackKing";
		public static string WhiteKing = "WhiteKing";
		public static string ChessBoard = "ChessBoard";
		public static string Clone = "(Clone)";
	}

	public class PieceClassNames
	{
		public static string Pawn = "Pawn";
		public static string Rook = "Rook";
		public static string Bishop = "Bishop";
		public static string Knight = "Knight";
		public static string Queen = "Queen";
		public static string King = "King";
	}

	public class EventNames
	{
		public static string PawnsLanded = "pawnslanded";

		public static string RooksLanded = "rookslanded";

		public static string KnightsLanded = "knightslanded";

		public static string BishopsLanded = "bishopslanded";

		public static string PieceSpawned = "piecespawned";

		public static string NewPlayerTurn = "newplayerturn";

		public static string AIComputationDone = "aicomputationdone";
	}

	public class TextGameObjectNames
	{
		public static string ALongTimeAgo = "A long time ago...";

		public static string StarWarsLogo = "Star Wars Logo";

		public static string OpeningCrawl = "Opening Crawl";

		public static string GameModeSelection = "Game Mode Selection";

		public static string SinglePlayerMode = "Single";

		public static string MultiPlayerMode = "Multi";
	}

	public class AudioClipNames
	{
		public static string AudioDirectory = "Audio/";

		public static string OpeningCrawl = "Opening_Crawl";

		public static string RevealBoard = "Reveal_Board";

		public static string GameLoop = "Game_Loop";

		public static string RebelAttack = "Rebel_Attack";

		public static string ImperialMarch = "Imperial_March";

		public static string Explosion = "Explosion";
	}

	public class AI
	{
		public static int maxAllowedDepth = 2;

		public static int maxAllowedMovesAtHighestLevel = 10;
	}

	public static string MainCameraObject = "Main Camera";
	public static string ActionCameraObject = "Action Camera";
	public static string AuxiliaryAudioObject = "Auxiliary Audio";
	public static string PieceTag = "Piece";
	public static string TileTag = "Tile";
	public static string PieceHolderTag = "PieceHolder";
	public static string VictoryText = "victory";

}

public enum Side
{
	Black,
	White
}