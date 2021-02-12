using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Chess {
	public static class Zobrist {
		const int seed = 2361912;
		const string randomNumbersFileName = "RandomNumbers.txt";

		/// piece type, colour, square index
		public static readonly ulong[, , ] piecesArray = new ulong[8, 2, 64];
		public static readonly ulong[] castlingRights = new ulong[16];
		/// ep file (0 = no ep).
		public static readonly ulong[] enPassantFile = new ulong[9]; // no need for rank info as side to move is included in key
		public static readonly ulong sideToMove;

		static System.Random prng = new System.Random (seed);

		static void WriteRandomNumbers () {
			prng = new System.Random (seed);
			string randomNumberString = "";
			int numRandomNumbers = 64 * 8 * 2 + castlingRights.Length + 9 + 1;

			for (int i = 0; i < numRandomNumbers; i++) {
				randomNumberString += RandomUnsigned64BitNumber ();
				if (i != numRandomNumbers - 1) {
					randomNumberString += ',';
				}
			}
			var writer = new StreamWriter (randomNumbersPath);
			writer.Write (randomNumberString);
			writer.Close ();
		}

		static Queue<ulong> ReadRandomNumbers () {
			if (!File.Exists (randomNumbersPath)) {
				Debug.Log ("Create");
				WriteRandomNumbers ();
			}
			Queue<ulong> randomNumbers = new Queue<ulong> ();

			var reader = new StreamReader (randomNumbersPath);
			string numbersString = reader.ReadToEnd ();
			reader.Close ();

			string[] numberStrings = numbersString.Split (',');
			for (int i = 0; i < numberStrings.Length; i++) {
				ulong number = ulong.Parse (numberStrings[i]);
				randomNumbers.Enqueue (number);
			}
			return randomNumbers;
		}

		static Zobrist () {

			var randomNumbers = ReadRandomNumbers ();

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				for (int pieceIndex = 0; pieceIndex < 8; pieceIndex++) {
					piecesArray[pieceIndex, Board.WhiteIndex, squareIndex] = randomNumbers.Dequeue ();
					piecesArray[pieceIndex, Board.BlackIndex, squareIndex] = randomNumbers.Dequeue ();
				}
			}

			for (int i = 0; i < 16; i++) {
				castlingRights[i] = randomNumbers.Dequeue ();
			}

			for (int i = 0; i < enPassantFile.Length; i++) {
				enPassantFile[i] = randomNumbers.Dequeue ();
			}

			sideToMove = randomNumbers.Dequeue ();
		}

		/// Calculate zobrist key from current board position. This should only be used after setting board from fen; during search the key should be updated incrementally.
		public static ulong CalculateZobristKey (Board board) {
			ulong zobristKey = 0;

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				if (board.Square[squareIndex] != 0) {
					int pieceType = Piece.PieceType (board.Square[squareIndex]);
					int pieceColour = Piece.Colour (board.Square[squareIndex]);

					zobristKey ^= piecesArray[pieceType, (pieceColour == Piece.White) ? Board.WhiteIndex : Board.BlackIndex, squareIndex];
				}
			}

			int epIndex = (int) (board.currentGameState >> 4) & 15;
			if (epIndex != -1) {
				zobristKey ^= enPassantFile[epIndex];
			}

			if (board.ColourToMove == Piece.Black) {
				zobristKey ^= sideToMove;
			}

			zobristKey ^= castlingRights[board.currentGameState & 0b1111];

			return zobristKey;
		}

		static string randomNumbersPath {
			get {
				return Path.Combine (Application.streamingAssetsPath, randomNumbersFileName);
			}
		}

		static ulong RandomUnsigned64BitNumber () {
			byte[] buffer = new byte[8];
			prng.NextBytes (buffer);
			return BitConverter.ToUInt64 (buffer, 0);
		}
	}
}