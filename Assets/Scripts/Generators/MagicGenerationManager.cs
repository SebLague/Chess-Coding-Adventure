using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using System.IO;

namespace Chess.Generation
{
	public class MagicGenerationManager : MonoBehaviour
	{
		[SerializeField] int numIterationsPerFrame;
		[SerializeField] TMPro.TMP_Text infoDisplay;
		[SerializeField] UnityEngine.UI.Button saveButton;

		MagicGenerator.MagicResult[] bestRookMagics;
		MagicGenerator.MagicResult[] bestBishopMagics;

		System.Random rng;

		int iterationIndex;

		MagicGenerator magicGenerator;

		void Start()
		{
			magicGenerator = new MagicGenerator();
			bestRookMagics = new MagicGenerator.MagicResult[64];
			bestBishopMagics = new MagicGenerator.MagicResult[64];
			rng = new System.Random();
			saveButton.onClick.AddListener(SaveMagics);
		}

		void Update()
		{
			UpdateSearch();
			UpdateInfoDisplay();
		}

		void SaveMagics()
		{

			string path = Path.Combine(Application.persistentDataPath, "Magics.txt");
			using (StreamWriter writer = new(File.Open(path, FileMode.Create)))
			{
				for (int i = 0; i < 64; i++)
				{
					writer.WriteLine(bestRookMagics[i].MagicValue);
					writer.WriteLine(bestRookMagics[i].NumBits);
				}
				for (int i = 0; i < 64; i++)
				{
					writer.WriteLine(bestBishopMagics[i].MagicValue);
					writer.WriteLine(bestBishopMagics[i].NumBits);
				}
			}

			Debug.Log("Saved to " + path);

		}

		void UpdateInfoDisplay()
		{
			string rookInfo = CreateInfoString(true);
			string bishopInfo = CreateInfoString(false);
			infoDisplay.text = rookInfo + "\n\n" + bishopInfo;

			string CreateInfoString(bool rook)
			{
				int numMagicsFound = 0;
				int lowestNumBits = int.MaxValue;
				int highestNumBits = int.MinValue;
				float totalKBSize = 0;

				MagicGenerator.MagicResult[] bests = rook ? bestRookMagics : bestBishopMagics;

				for (int squareIndex = 0; squareIndex < 64; squareIndex++)
				{
					if (bests[squareIndex].IsValid)
					{
						numMagicsFound++;
						lowestNumBits = Mathf.Min(lowestNumBits, bests[squareIndex].NumBits);
						highestNumBits = Mathf.Max(highestNumBits, bests[squareIndex].NumBits);
						float sizeKilobytes = (Mathf.Pow(2, bests[squareIndex].NumBits) * 8) / 1000f;
						totalKBSize += sizeKilobytes;
					}

				}

				string startCol = numMagicsFound == 64 ? "<color=#7AFF60>" : "<color=#ff4a4a>";
				string endCol = "</color>";

				string info = "<color=#ffffff>" + (rook ? "<b>Rook Magics:</b>" : "<b>Bishop Magics:</b>") + "</color>";
				info += "<color=#B3B3B3>";
				info += $"\nNum Found: {startCol}{numMagicsFound} / 64{endCol}";
				info += $"\nLowest Required Bit Count: {startCol}{lowestNumBits}{endCol}";
				info += $"\nHighest Required Bit Count:{startCol} {highestNumBits}{endCol}";
				info += $"\nAvg Size Per Square: {startCol}{totalKBSize / Mathf.Max(numMagicsFound, 1):0.00} kb{endCol}";
				info += $"\nTotal Size: {startCol}{totalKBSize:0.00} kb{endCol}";
				info += "</color>";

				return info;
			}
		}

		void UpdateSearch()
		{
			bool searchWorst = rng.NextDouble() < 0.8;
			if (searchWorst)
			{
				bool searchingRooks = iterationIndex % 2 == 0;

				var bests = searchingRooks ? bestRookMagics : bestBishopMagics;

				MagicGenerator.MagicResult worstMagicSoFar = bests[0];
				int worstSquare = 0;

				int[] shuffledIndices = Seb.ArrayHelper.CreateShuffledIndexArray(64, rng);

				for (int i = 0; i < 64; i++)
				{
					int squareIndex = shuffledIndices[i];

					if (!bests[squareIndex].IsValid || (bests[squareIndex].NumBits > worstMagicSoFar.NumBits && worstMagicSoFar.IsValid))
					{

						worstMagicSoFar = bests[squareIndex];
						worstSquare = squareIndex;
					}
				}
				RunSearchIteration(worstSquare, numIterationsPerFrame, searchingRooks);
			}
			else
			{
				RunSearchIteration(Time.frameCount % 64, numIterationsPerFrame, true);
				RunSearchIteration(Time.frameCount % 64, numIterationsPerFrame, false);
			}

			iterationIndex++;

		}

		void RunSearchIteration(int squareIndex, int numIterations, bool rook)
		{
			UpdateSquare(squareIndex, rook);

			void UpdateSquare(int squareIndex, bool rook)
			{
				MagicGenerator.MagicResult[] bests = rook ? bestRookMagics : bestBishopMagics;

				int maxBitCount = bests[squareIndex].IsValid ? bests[squareIndex].NumBits - 1 : 13;
				MagicGenerator.MagicResult newMagic = magicGenerator.GenerateMagic(rng, squareIndex, rook, numIterations, maxBitCount);

				if (IsBetter(newMagic, bests[squareIndex]))
				{
					bests[squareIndex] = newMagic;
				}
			}


		}

		bool IsBetter(MagicGenerator.MagicResult newMagic, MagicGenerator.MagicResult oldMagic)
		{
			return newMagic.NumBits < oldMagic.NumBits || (!oldMagic.IsValid && newMagic.IsValid);
		}


	}
}