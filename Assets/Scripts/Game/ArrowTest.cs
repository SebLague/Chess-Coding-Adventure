using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
	public class ArrowTest : MonoBehaviour
	{
		public float thickness;
		public float headLength;
		public float headAngle;
		public Color col;
		public Color colTemp;

		public Chess.UI.BoardUI boardUI;
		bool isDrawing;
		Vector2 startPos;
		Vector2 endPos;

		public List<(Vector2, Vector2)> arrows = new();

		void Update()
		{
			Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			if (Mouse.current.rightButton.wasPressedThisFrame)
			{
				if (boardUI.TryGetCoordFromPosition(mouseWorld, out var coord))
				{
					isDrawing = true;
					startPos = boardUI.PositionFromCoord(coord);
				}
			}
			if (isDrawing)
			{
				if (Mouse.current.rightButton.isPressed)
				{
					if (boardUI.TryGetCoordFromPosition(mouseWorld, out var coord))
					{
						endPos = boardUI.PositionFromCoord(coord);
					}
					if ((startPos - endPos).magnitude > 0)
					{
						DrawArrow(startPos, endPos, colTemp);
					}
				}
				if (Mouse.current.rightButton.wasReleasedThisFrame)
				{
					if ((startPos - endPos).magnitude > 0)
					{
						arrows.Add((startPos, endPos));
					}
					isDrawing = false;
				}
			}

			if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)
			{
				arrows.Clear();
			}

			foreach (var a in arrows)
			{
				DrawArrow(a.Item1, a.Item2, col);
			}
		}

		void DrawArrow(Vector2 start, Vector2 end, Color col)
		{
			Seb.Vis.Draw.Arrow(start, end, thickness, headLength, headAngle, col);
		}
	}
}