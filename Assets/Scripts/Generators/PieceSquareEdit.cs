using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Generation
{
	[SelectionBase]
	public class PieceSquareEdit : MonoBehaviour
	{
		public event System.Action<PieceSquareEdit> ValueEdited;
		public int value;
		public Coord coord;
		int valueOld;

		TMPro.TMP_Text text;

		public void SetValueWithoutNotify(int value)
		{
			this.value = value;
			valueOld = value;
			text.text = value + "";
		}

		public void Init(TMPro.TMP_Text text, Coord coord)
		{
			this.text = text;
			this.coord = coord;
		}

		void Update()
		{
			if (valueOld != value)
			{
				valueOld = value;
				text.text = value + "";
				ValueEdited?.Invoke(this);
			}
		}


	}
}