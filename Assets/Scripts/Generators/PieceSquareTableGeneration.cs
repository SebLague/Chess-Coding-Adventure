using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.UI;

namespace Chess.Generation
{
	public class PieceSquareTableGeneration : MonoBehaviour
	{
		public enum TableType
		{
			None,
			Pawn,
			PawnEnd,
			Rook,
			Knight,
			Bishop,
			Queen,
			KingStart,
			KingEnd,
			KingTransition
		}
		public TableType tableType;
		public Color minScoreColour;
		public Color maxScoreColour;
		public float tileScale;
		public TMPro.TMP_Text valueTextPrefab;
		public Transform boardHolder;
		public string output;
		public bool horizontalEditSymmetry;

		[Range(0, 1)] public float endgameTransitionT;
		float endgameTransitionTOld;
		Material[,] squareMaterials;
		PieceSquareEdit[,] editSquares;
		BoardUI boardUI;
		TableType tableTypeOld;

		void Start()
		{
			CreateBoardUI();
			SetOutput();
		}

		void Update()
		{
			if (tableType != tableTypeOld)
			{
				tableTypeOld = tableType;
				SetValues();
				UpdateColours();
				SetOutput();
			}
		}

		void SetValues()
		{
			int[] values = tableType switch
			{
				TableType.Pawn => PieceSquareTable.Pawns,
				TableType.PawnEnd => PieceSquareTable.PawnsEnd,
				TableType.Rook => PieceSquareTable.Rooks,
				TableType.Knight => PieceSquareTable.Knights,
				TableType.Bishop => PieceSquareTable.Bishops,
				TableType.Queen => PieceSquareTable.Queens,
				TableType.KingStart => PieceSquareTable.KingStart,
				TableType.KingEnd => PieceSquareTable.KingEnd,
				TableType.KingTransition => PieceSquareTable.KingStart,
				_ => null
			};

			for (int i = 0; i < values.Length; i++)
			{
				Coord coord = new Coord(i);
				Coord flippedCoord = new Coord(coord.fileIndex, 7-coord.rankIndex);
				int value = values[flippedCoord.SquareIndex];
				if (tableType == TableType.KingTransition)
				{
					value = (int)Mathf.Lerp(value, PieceSquareTable.KingEnd[i], endgameTransitionT);
				}
				editSquares[coord.fileIndex, coord.rankIndex].SetValueWithoutNotify(value);
			}
		}

		void UpdateColours()
		{
			int minValue = int.MaxValue;
			int maxValue = int.MinValue;

			for (int i = 0; i < 64; i++)
			{
				Coord coord = new Coord(i);
				minValue = Mathf.Min(minValue, editSquares[coord.fileIndex, coord.rankIndex].value);
				maxValue = Mathf.Max(maxValue, editSquares[coord.fileIndex, coord.rankIndex].value);
			}

			for (int i = 0; i < 64; i++)
			{
				Coord coord = new Coord(i);
				int value = editSquares[coord.fileIndex, coord.rankIndex].value;
				float t = 0.5f;
				if (maxValue > minValue)
				{
					t = (value - minValue) / (float)(maxValue - minValue);
				}
				squareMaterials[coord.fileIndex, coord.rankIndex].color = Color.Lerp(minScoreColour, maxScoreColour, t);
			}
		}

		void OnValueEdited(PieceSquareEdit editedSquare)
		{
			if (horizontalEditSymmetry)
			{
				Coord mirrorCoord = new Coord(7 - editedSquare.coord.fileIndex, editedSquare.coord.rankIndex);
				editSquares[mirrorCoord.fileIndex, mirrorCoord.rankIndex].SetValueWithoutNotify(editedSquare.value);
			}
			UpdateColours();
			SetOutput();
		}

		void CreateBoardUI()
		{
			Shader squareShader = Shader.Find("Unlit/ColorAlpha");
			squareMaterials = new Material[8, 8];
			editSquares = new PieceSquareEdit[8, 8];

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					// Create square
					string squareName = BoardHelper.SquareNameFromCoordinate(file, rank);
					PieceSquareEdit squareEdit = new GameObject(squareName).AddComponent<PieceSquareEdit>();
					squareEdit.ValueEdited += OnValueEdited;
					editSquares[file, rank] = squareEdit;
					Transform holder = squareEdit.transform;

					holder.parent = boardHolder;
					holder.position = BoardUI.PositionFromCoord(file, rank, -1);
					TMPro.TMP_Text valueText = Instantiate(valueTextPrefab, holder.position, Quaternion.identity, holder);
					squareEdit.Init(valueText, new Coord(file, rank));

					Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
					square.parent = holder;
					square.localPosition = Vector3.forward * 0.01f;
					square.localScale = Vector3.one * tileScale;
					Material squareMaterial = new Material(squareShader);

					square.GetComponent<MeshRenderer>().material = squareMaterial;
					squareMaterials[file, rank] = squareMaterial;

				}
			}
		}

		void SetOutput()
		{
			output = "{ ";
			for (int i = 0; i < 64; i++)
			{
				Coord coord = new Coord(i);
				Coord flippedCoord = new Coord(coord.fileIndex, 7-coord.rankIndex);
				int value = editSquares[flippedCoord.fileIndex, flippedCoord.rankIndex].value;
				output += value;

				if (i < 63)
				{
					output += ", ";
				}
			}
			output += " };";
		}

		void OnValidate()
		{
			if (Application.isPlaying && editSquares != null)
			{
				if (endgameTransitionT != endgameTransitionTOld && tableType == TableType.KingTransition)
				{
					endgameTransitionTOld = endgameTransitionT;
					SetValues();
				}
				UpdateColours();
			}
		}
	}
}