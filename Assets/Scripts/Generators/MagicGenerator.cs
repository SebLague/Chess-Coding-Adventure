using System.Collections.Generic;
using System;
using Chess.Core;

namespace Chess.Generation
{
	public class MagicGenerator
	{
		SquarePatterns[] rookLookups;
		SquarePatterns[] bishopLookups;

		public MagicGenerator()
		{
			rookLookups = new SquarePatterns[64];
			bishopLookups = new SquarePatterns[64];

			for (int i = 0; i < 64; i++)
			{
				rookLookups[i] = GenerateSquarePatterns(i, true);
				bishopLookups[i] = GenerateSquarePatterns(i, false);
			}
		}

		public MagicResult GenerateMagic(System.Random rng, int originSquare, bool rook, int numSearchIterations, int maxBitCount = 12)
		{
			SquarePatterns lookup = rook ? rookLookups[originSquare] : bishopLookups[originSquare];
			ulong bestMagicSoFar = 0;
			int fewestBitsSoFar = maxBitCount + 1;



			int numEntries = (int)System.Math.Pow(2, maxBitCount);
			ulong[] test = new ulong[numEntries];

			
			for (int i = 0; i < numSearchIterations; i++)
			{
				ulong magic = GenerateRandomMagic(rng);


				for (int numBits = fewestBitsSoFar - 1; numBits > 0; numBits--)
				{
					numEntries = (int)System.Math.Pow(2, fewestBitsSoFar - 1);
					var span = test.AsSpan(0, numEntries);
					span.Clear();

					if (TestMagic(lookup, magic, numBits, span))
					{
						bestMagicSoFar = magic;
						fewestBitsSoFar = numBits;
					}
				}
			}

			return new MagicResult(bestMagicSoFar != 0, bestMagicSoFar, fewestBitsSoFar);

		}

		static bool TestMagic(SquarePatterns lookup, ulong magic, int numBits, System.Span<ulong> test)
		{
			int numEntries = (int)System.Math.Pow(2, numBits);

			for (int i = 0; i < lookup.allBlockerPatterns.Length; i++)
			{
				ulong blockers = lookup.allBlockerPatterns[i];
				ulong moves = lookup.allMoves[i];

				int magicKey = (int)((blockers * magic) >> (64 - numBits));
				if (test[magicKey] == 0 || test[magicKey] == moves)
				{
					test[magicKey] = moves;
				}
				else
				{
					// This index is already occupied by a different move, so magic has failed
					return false;
				}
			}
			return true;
		}

		// Generate random 64bit number lots of bits set
		static ulong GenerateRandomMagic(System.Random rng)
		{
			ulong magic = 0;
			// Chose a probability for bit being set to 1
			double probMin = 0.25;
			double probMax = 0.95;
			double prob = probMin + (probMax - probMin) * rng.NextDouble();
			// Randomly set bits
			for (int i = 0; i < 64; i++)
			{
				bool setBit = rng.NextDouble() < prob;
				if (setBit)
				{
					magic |= 1ul << i;
				}
			}
			return magic;
		}


		public static SquarePatterns GenerateSquarePatterns(int originSquare, bool rook)
		{
			ulong movementMask = MagicHelper.CreateMovementMask(originSquare, rook);
			ulong[] allPatterns = MagicHelper.CreateAllBlockerBitboards(movementMask);

			ulong[] moves = new ulong[allPatterns.Length];

			for (int i = 0; i < allPatterns.Length; i++)
			{
				moves[i] = MagicHelper.LegalMoveBitboardFromBlockers(originSquare, allPatterns[i], rook);
			}

			return new SquarePatterns(allPatterns, moves);

		}

		public readonly struct SquarePatterns
		{
			public readonly ulong[] allBlockerPatterns;
			public readonly ulong[] allMoves;

			public SquarePatterns(ulong[] blockerPatterns, ulong[] moves)
			{
				this.allBlockerPatterns = blockerPatterns;
				this.allMoves = moves;
			}
		}

		public readonly struct MagicResult
		{
			public readonly bool IsValid;
			public readonly ulong MagicValue;
			public readonly int NumBits;

			public MagicResult(bool isValid, ulong magicValue, int numBits)
			{
				IsValid = isValid;
				MagicValue = magicValue;
				NumBits = numBits;
			}
		}
	}
}