using System;

namespace InfoKod_HF_02._16
{
	class Program
	{
		static void LeMain(string[] args)
		{
			Console.WriteLine("Információ és Kódelmélet házi feladat!");
			Console.WriteLine("Készítette: Korcsák Gergely\n");

			CodedString coder = new();

			coder.GetCharNum();
			coder.GetStringLenght();
			coder.GenerateString();

			coder.CountNumbers();
			coder.CalculateChances();
			coder.CcalculateEntropy();

			Console.WriteLine();

			Console.WriteLine("\n\n// Huffman kódolás:");
			coder.GeneratePrefixCodeTree();
			coder.CalculateAverageCodeLength();
			coder.CompressString();
			coder.DecompressString();

			//Console.WriteLine("\n\n// Azonos hosszú kód:");
			//coder.GenerateAverageLengthCode();
			//coder.CalculateAverageCodeLength();
			//coder.CompressString();
			//coder.DecompressString();

			//Console.WriteLine("\n\n// Szinguláris kód:         -- A szingularitás menő csak néha haszontalan...");
			//coder.GenerateSingularCode();
			//coder.CalculateAverageCodeLength();
			//coder.CompressString();
			//coder.DecompressString();

			//Console.WriteLine("\n\n// Shannon-Fano kód:");
			//coder.GenerateShannonFanoCode();
			//coder.CalculateAverageCodeLength();
			//coder.CompressString();
			//coder.DecompressString();

			//Console.WriteLine("\n\n// Gillbert-Moore kód:");
			//coder.GenerateGillbertMooreCode();
			//coder.CalculateAverageCodeLength();
			//coder.CompressString();
			//coder.DecompressString();


			Console.WriteLine("\n\n\n\n\n\n\n\n\n");

		}

		
	}
}
