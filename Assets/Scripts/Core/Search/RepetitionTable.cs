namespace Chess.Core
{
	public class RepetitionTable
	{
		ulong[] hashes;
		int[] startIndices;
		int count;

		public RepetitionTable()
		{
			hashes = new ulong[128];
			startIndices = new int[hashes.Length];
		}

		public void Init(ulong[] initialHashes)
		{
			count = initialHashes.Length;
			for (int i = 0; i < initialHashes.Length; i++)
			{
				hashes[i] = initialHashes[i];
				startIndices[i] = 0;
			}
			startIndices[count] = 0;
		}


		public void Push(ulong hash, bool reset)
		{

			hashes[count] = hash;
			count++;
			startIndices[count] = reset ? count - 1 : startIndices[count - 1];
		}

		public void TryPop()
		{
			count = System.Math.Max(0, count - 1);
		}

		public bool Contains(ulong h)
		{
			int s = startIndices[count];
			for (int i = s; i < count; i++)
			{
				if (hashes[i] == h)
				{
					return true;
				}
			}
			return false;
		}
	}
}