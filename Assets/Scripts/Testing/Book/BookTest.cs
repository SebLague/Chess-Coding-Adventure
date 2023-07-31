using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Testing
{
	public class BookTest : MonoBehaviour
	{
		public TextAsset file;

		// Start is called before the first frame update
		void Start()
		{
			OpeningBook book = new OpeningBook(file.text);

			for (int i = 0; i < 1000; i++)
			{
				book.TryGetBookMove(FenUtility.StartPositionFEN, out string moveString);
				Debug.Log(moveString);
			}
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}