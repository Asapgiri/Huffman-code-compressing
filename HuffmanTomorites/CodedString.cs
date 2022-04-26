using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoKod_HF_02._16
{
	public class CodedString
	{
		private static readonly Random random = new Random();

		int charNum;
		int stringLength;
		string generatedString;
		Dictionary<char, int> charCount;
		Dictionary<char, int> unorderedCharCount;
		Dictionary<char, double> charChance;
		Dictionary<char, double> unorderedCharChance;
		double entrophy;

		Dictionary<char, string> codeWords;
		class Node<T> where T : struct
		{
			public Node(char _ch, T _freq, Node<T> _left = null, Node<T> _right = null)
			{
				ch = _ch;
				freq = _freq;
				left = _left;
				right = _right;
			}
			public char ch;
			public T freq;
			public Node<T> left, right;
		}
		string compressedString;

		public void GetCharNum()
		{
			string cread = string.Empty;
			bool parsedOnce = false;

			Console.Write("A program egy random szöveget generál\nKérem adja meg a rendelkezésre álló karakterek\nszámát: ");

			while (!int.TryParse(cread, out charNum) || charNum <= 0)
			{
				if (parsedOnce)
				{
					Console.Write("A megadott érték nem szám, vagy kisebb mint 1! kérlek add meg újra!\nKarakterek száma: ");
				}
				else
				{
					parsedOnce = true;
				}
				cread = Console.ReadLine();
			}
		}

		public void GetStringLenght()
		{
			string cread = string.Empty;
			bool parsedOnce = false;

			Console.Write("Kérem adja meg a szöveg hosszát: ");

			while (!int.TryParse(cread, out stringLength) || stringLength < 60)
			{
				if (parsedOnce)
				{
					Console.Write("A megadott érték nem szám, vagy kisebb mint 60! kérlek add meg újra!\nKarakterek száma: ");
				}
				else
				{
					parsedOnce = true;
				}
				cread = Console.ReadLine();
			}
		}

		public void GenerateString()
		{
			List<char> letters = new List<char>();
			List<char> characterList = new List<char>();
			List<int> chances = new List<int>() { 25 };
			int chanceSum = 25;

			for (int i = 97; i < 123; i++)
			{
				letters.Add((char)i);
			}

			if (charNum <= 0)
			{
				charNum = 12;
			}
			else if (charNum > letters.Count)
			{
				charNum = letters.Count;
			}
			if (stringLength < 60)
			{
				stringLength = 120;
			}
			else if (stringLength > 9999)
			{
				stringLength = 9999;
			}

			Console.Write("\n\nA gereált karakterek a következő:\n { ");
			for (int i = 0; i < charNum; i++)
			{
				// ASCII a, z
				int newCharPos = (char)random.Next(0, letters.Count);
				char newChar = letters[newCharPos];
				letters.RemoveAt(newCharPos);
				characterList.Add(newChar);

				if (i < charNum - 1)
				{
					int actualChance = random.Next(chanceSum, 100);
					chances.Insert(0, actualChance);
					chanceSum += actualChance - chanceSum;
				}

				Console.Write(newChar);
				if (i != charNum - 1) Console.Write(", ");
			}
			Console.WriteLine(" }");


			generatedString = string.Empty;
			for (int i = 0; i < stringLength; i++)
			{
				int chance = chances.FirstOrDefault(x => x < random.Next(0, 100));
				generatedString += characterList[
					(chance == 0) ? 0 : chances.IndexOf(chance)];
			}

			Console.WriteLine("\n\nA gereált szöveg a következő:\n { " + generatedString + " }");
			Console.WriteLine();
		}

		public void CountNumbers()
		{
			unorderedCharCount = new Dictionary<char, int>();

			for (int i = 0; i < generatedString.Length; i++)
			{
				if (unorderedCharCount.ContainsKey(generatedString[i]))
				{
					unorderedCharCount[generatedString[i]]++;
				}
				else
				{
					unorderedCharCount.Add(generatedString[i], 1);
				}
			}

			charCount = new Dictionary<char, int>(unorderedCharCount.OrderByDescending(x => x.Value));

			Console.Write("\nA begyűjtött rendezettlenül:\n  { ");
			foreach (KeyValuePair<char, int> character in unorderedCharCount)
			{
				Console.Write("{0} db '{1}'", character.Value, character.Key);
				if (!character.Equals(unorderedCharCount.Last())) Console.Write("; ");
			}
			Console.WriteLine(" }");

			Console.Write("\nA begyűjtött rendezve:\n  { ");
			foreach (KeyValuePair<char, int> character in charCount)
			{
				Console.Write("{0} db '{1}'", character.Value, character.Key);
				if (!character.Equals(charCount.Last())) Console.Write("; ");
			}
			Console.WriteLine(" }");
		}

		public void CalculateChances()
		{
			unorderedCharChance = new Dictionary<char, double> ();

			Console.Write("\nA begyűjtött karakterek valószínűségei rendezettlen:\n  { ");
			foreach (KeyValuePair<char, int> ci in unorderedCharCount)
			{
				double chance = (double)ci.Value / (double)generatedString.Length;
				unorderedCharChance.Add(ci.Key, chance);
				Console.Write("p({0}): {1:0.00}", ci.Key, chance);
				if (!ci.Equals(unorderedCharCount.Last())) Console.Write("; ");
			}
			Console.WriteLine(" }");

			charChance = new Dictionary<char, double>();

			Console.Write("\nA begyűjtött karakterek valószínűségei:\n  { ");
			foreach (KeyValuePair<char, int> ci in charCount)
			{
				double chance = (double)ci.Value / (double)generatedString.Length;
				charChance.Add(ci.Key, chance);
				Console.Write("p({0}): {1:0.00}", ci.Key, chance);
				if (!ci.Equals(charCount.Last())) Console.Write("; ");
			}
			Console.WriteLine(" }");
		}

		public void CcalculateEntropy()
		{
			entrophy = 0.0f;
			foreach (var cc in charChance)
			{
				entrophy += cc.Value * Math.Log2(cc.Value);
			}
			entrophy = -entrophy;
			Console.WriteLine("\n\nAz entrópia:\n  H(x) = {0:0.00}", entrophy);
			Console.WriteLine("  H(x) < log2({0})", charNum);
			double log2charNum = Math.Log2(charNum);
			Console.Write("  {0:0.00} < {1:0.00}", entrophy, log2charNum);
			Console.WriteLine(" ({0})\n", entrophy <= log2charNum ? "igaz" : "hamis");
		}

		/// <summary>
		/// Le Huffmann kód.
		/// </summary>
		public void GeneratePrefixCodeTree()
		{
			codeWords = new Dictionary<char, string>();
			List<Node<int>> nodes = new List<Node<int>>();

			foreach (var item in charCount)
			{
				nodes.Insert(0, new Node<int>(item.Key, item.Value));
			}

			while (nodes.Count != 1)
			{
				Node<int> left = nodes.First(); nodes.Remove(left);
				Node<int> right = nodes.First(); nodes.Remove(right);
				Node<int> newNode = new Node<int>('\0', left.freq + right.freq, left, right);

				if (nodes.Count == 0)
				{
					nodes.Add(newNode);
					break;
				}

				for (int i = 0; i < nodes.Count; i++)
				{
					if (nodes[i].freq > newNode.freq)
					{
						nodes.Insert(i, newNode);
						break;
					}
					else if (i == nodes.Count - 1) {
						nodes.Add(newNode);
						break;
					}
				}
			}

			Node<int> root = nodes.Last();
			Encode(root, string.Empty);

			ShowCodedWords();
		}

		private void ShowCodedWords()
		{
			Console.WriteLine("\n\nA begyűjtött karakterek számított kódjai:\n  {");
			foreach (var item in codeWords)
			{
				Console.Write("    {0}: \t'{1}'", item.Value, item.Key);
				if (!item.Equals(codeWords.Last())) Console.WriteLine(",");
			}
			Console.WriteLine("\n  }");
		}

		//Huffmann
		private void Encode(Node<int> root, string str)
		{
			if (root == null)
			{
				return;
			}

			Console.Write("\n  ");
			string lineSpaces = string.Empty;
			for (int i = 0; i < str.Length; i++)
			{
				lineSpaces += " ";
			}
			Console.Write(lineSpaces);
			if (str.Length != 0)
			{
				Console.Write("∟{0} ({1})", str.Last(), root.freq);
			}

			if (root.left == null && root.right == null)
			{
				codeWords.Add(root.ch, str != string.Empty ? str : "1");
				if (str != string.Empty) Console.Write(": '{0}'", root.ch);
				else Console.Write("{0}  ∟1 ({2}): '{1}'", lineSpaces, root.ch, root.freq);
			}

			Encode(root.left, str + "0");
			Encode(root.right, str + "1");
		}

		public void CompressString()
		{
			compressedString = string.Empty;
			string originalBinary = string.Empty;

			for (int i = 0; i < generatedString.Length; i++)
			{
				compressedString += codeWords[generatedString[i]];
				originalBinary += Convert.ToString(generatedString[i], 2).PadLeft(8, '0');
				//Console.Write(bynaryTree[generatedString[i]]);
				//if (i != generatedString.Length - 1) Console.Write(", ");
			}

			Console.Write("\nEredeti szöveg binárisan:\n { ");
			Console.Write(originalBinary);
			Console.WriteLine(" } " + originalBinary.Length + " bit\n");

			Console.Write("\nSzöveg kódolása a követhezőképpen:\n  { ");
			Console.Write(compressedString);
			Console.WriteLine(" } " + compressedString.Length + " bit\n");

			Console.WriteLine("A tömörítés arány: {0:0.00}", (float)compressedString.Length /
															 (float)originalBinary.Length);
			Console.WriteLine("Nyert bitek: {0} bit", originalBinary.Length - compressedString.Length);
		}

		public void DecompressString()
		{
			string decompressedString = string.Empty;
			string buffer = string.Empty;

			Console.Write("\nSzöveg kikódolása...");
			for (int i = 0; i < compressedString.Length; i++)
			{
				buffer += compressedString[i];
				if (codeWords.ContainsValue(buffer))
				{
					decompressedString += codeWords.FirstOrDefault(x => x.Value == buffer).Key;
					buffer = string.Empty;
				}
			}
			Console.WriteLine("  KÉSZ.\n");

			Console.WriteLine("A kikódolt szöveg:\n  { " + decompressedString + " }\n");

			if (decompressedString == generatedString)
			{
				Console.WriteLine("A két szöveg megegyezik.");
			}
			else
			{
				Console.WriteLine("A két szöveg különbözik.");
			}
		}

		public void GenerateSingularCode()
		{
			codeWords = new Dictionary<char, string>();

			int i = 0;
			foreach (var item in charChance)
			{
				codeWords.Add(item.Key, (i++ % 2).ToString());
			}

			ShowCodedWords();
		}

		public void CalculateAverageCodeLength()
		{
			int codeLengthSum = 0;

			foreach (var item in codeWords)
			{
				codeLengthSum += item.Value.Length;
			}

			float averageLength = (float)codeLengthSum / codeWords.Count;

			Console.WriteLine("Az átlagos kódszó hosszúsága:\n L(K) = {0:0.00}", averageLength);
		}

		public void GenerateShannonFanoCode()
		{
			codeWords = new Dictionary<char, string>();
			List<node> nodes = new List<node>();

			foreach (var item in charChance)
			{
				nodes.Insert(0, new node() { 
					sym = item.Key,
					pro = item.Value,
					arr = new int[20],
					top = -1
				});
			}

			shannon(0, nodes.Count - 1, nodes);
			foreach(node item in nodes)
			{
				string s = string.Empty;
				for (int i = 0; i <= item.top; i++)
				{
					s += item.arr[i].ToString();
				}
				codeWords.Add(item.sym, s);
			}

			ShowCodedWords();
		}

		// declare structure node
		class node
		{
			// for storing symbol
			public char sym;

			// for storing probability or frequency
			public double pro;
			public int[] arr;
			public int top;
		}

		private void shannon(int lowerLimit, int upperLimit, List<node> p)
		{
			double pack1 = 0, pack2 = 0, diff1 = 0, diff2 = 0;
			int i = 0, k = 0, j = 0;
			if ((lowerLimit + 1) == upperLimit || lowerLimit == upperLimit || lowerLimit > upperLimit)
			{
				if (lowerLimit == upperLimit || lowerLimit > upperLimit)
					return;
				p[upperLimit].arr[++(p[upperLimit].top)] = 0;
				p[lowerLimit].arr[++(p[lowerLimit].top)] = 1;
				return;
			}
			else
			{
				for (i = lowerLimit; i <= upperLimit - 1; i++)
					pack1 = pack1 + p[i].pro;
				pack2 = pack2 + p[upperLimit].pro;
				diff1 = pack1 - pack2;
				if (diff1 < 0)
					diff1 = diff1 * -1;
				j = 2;
				while (j != upperLimit - lowerLimit + 1)
				{
					k = upperLimit - j;
					pack1 = pack2 = 0;
					for (i = lowerLimit; i <= k; i++)
						pack1 = pack1 + p[i].pro;
					for (i = upperLimit; i > k; i--)
						pack2 = pack2 + p[i].pro;
					diff2 = pack1 - pack2;
					if (diff2 < 0)
						diff2 = diff2 * -1;
					if (diff2 >= diff1)
						break;
					diff1 = diff2;
					j++;
				}
				k++;
				for (i = lowerLimit; i <= k; i++)
					p[i].arr[++(p[i].top)] = 1;
				for (i = k + 1; i <= upperLimit; i++)
					p[i].arr[++(p[i].top)] = 0;

				// Invoke shannon function
				shannon(lowerLimit, k, p);
				shannon(k + 1, upperLimit, p);
			}
		}

		public void GenerateGillbertMooreCode()
		{
			codeWords = new Dictionary<char, string>();
			List<node> nodes = new List<node>();

			foreach (var item in unorderedCharChance)
			{
				nodes.Insert(0, new node()
				{
					sym = item.Key,
					pro = item.Value,
					arr = new int[20],
					top = -1
				});
			}

			shannon(0, nodes.Count - 1, nodes);
			foreach (node item in nodes)
			{
				string s = string.Empty;
				for (int i = 0; i <= item.top; i++)
				{
					s += item.arr[i].ToString();
				}
				codeWords.Add(item.sym, s);
			}

			ShowCodedWords();
		}
	}
}
