using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HuffmanTomorites {
    static class Coder {
        private static readonly char STR_END = '\0';
        private static readonly int BYTE_MAX = 256;
        private const string FILE_EXTENSION = "hfzip";

        public static bool CompressFiles(string[] files, string destinationFile, ProgressWindow progressWindow) {
            progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
            string[] newFileName = destinationFile.Split('.');
            if (newFileName[newFileName.Length - 1] != FILE_EXTENSION) {
                destinationFile += "." + FILE_EXTENSION;
            }
            int[,] byteCountsPerFile = CountBytesInFiles(files, progressWindow);
            int[] byteCounts = new int[BYTE_MAX];
            for (int i = 0; i < byteCountsPerFile.GetLength(0); i++) {
                for (int j = 0; j < byteCountsPerFile.GetLength(1); j++) {
                    byteCounts[j] += byteCountsPerFile[i, j];
                    progressWindow.MakeProgress();
                }
            }
            Dictionary<byte, byte[]> byteCodes = GenerateHuffmanCodeTree(byteCounts, progressWindow); // <character, code aray>

            return WriteFilesToZip(files, byteCountsPerFile, byteCodes, destinationFile, progressWindow);
        }

        #region Creating Compressed File [...]
        private static bool WriteFilesToZip(string[] files, int[,] byteCountsPerFile, Dictionary<byte, byte[]> byteCodes, string destinationFile, ProgressWindow progressWindow) {
            using (FileStream ftw = File.Open(destinationFile, FileMode.Create)) {
                List<byte> bytesToCode = new List<byte>();

                //Creating file pretag...
                for (int i = 0; i < byteCodes.Count; i++) {
                    ftw.WriteByte((byte)byteCodes[(byte)i].Length); // a byte to know the code length

                    //writing the codeword to bytes...
                    byte[] codeList = byteCodes[(byte)i];
                    int spam = codeList.Length % 8;
                    byte[] tempCode = new byte[codeList.Length / 8 + (spam > 0 ? 1 : 0)];

                    // putting the codewords after their length
                    for (int j = 0; j < tempCode.Length; j++) {
                        int l = 0;
                        for (int k = 7; k >= 0; k--) {
                            tempCode[j] |= (byte)(codeList[j * 8 + l] << k);
                            l++;
                            if (j * 8 + l >= codeList.Length) break;
                        }
                        ftw.WriteByte(tempCode[j]);
                    }
                    progressWindow.MakeProgress();
                    if (progressWindow.CancelToken.Token.IsCancellationRequested) {
                        ftw.Close();
                        File.Delete(destinationFile);
                        progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                    }
                }

                // writing filename(s) and lengths
                int newAddress = 0;
                int[] startIndexes = new int[files.Length];
                int[] fileLengths = new int[files.Length];
                ftw.Write(BitConverter.GetBytes(files.Length), 0, 4);
                for (int i = 0; i < files.Length; i++) {
                    string[] f = files[i].Split('\\', '/');
                    string fileName = f[f.Length - 1] + STR_END;

                    int fileCodeLength = 0;
                    int originalFileLength = 0;
                    for (int j = 0; j < BYTE_MAX; j++) {
                        fileCodeLength += byteCountsPerFile[i, j] * byteCodes[(byte)j].Length;
                        originalFileLength += byteCountsPerFile[i, j] * 8;
                    }

                    ftw.Write(Encoding.ASCII.GetBytes(fileName), 0, fileName.Length);
                    ftw.Write(BitConverter.GetBytes(newAddress), 0, 4);
                    ftw.Write(BitConverter.GetBytes(fileCodeLength), 0, 4);
                    fileLengths[i] = fileCodeLength;
                    startIndexes[i] = newAddress;
                    progressWindow.MakeProgress();
                    newAddress += fileCodeLength;
                }

                if (progressWindow.CancelToken.Token.IsCancellationRequested) {
                    ftw.Close();
                    File.Delete(destinationFile);
                    progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                }

                // reading files to encode them
                foreach (string path in files) {
                    int actualFileLength = 0;
                    using (FileStream fs = File.Open(path, FileMode.Open)) {
                        for (long i = 0; i < fs.Length; i++) {
                            byte[] newCode = byteCodes[(byte)fs.ReadByte()];
                            progressWindow.MakeProgress();
                            if (progressWindow.CancelToken.Token.IsCancellationRequested) {
                                ftw.Close();
                                fs.Close();
                                File.Delete(destinationFile);
                                progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                            }

                            for (int j = 0; j < newCode.Length; j++) {
                                bytesToCode.Add(newCode[j]);
                            }
                            while (bytesToCode.Count >= 8) {
                                byte temp = 0;

                                for (int k = 7; k >= 0; k--) {
                                    temp |= (byte)(bytesToCode[0] << k);
                                    bytesToCode.RemoveAt(0);
                                }
                                ftw.WriteByte(temp);
                                actualFileLength += 8;
                            }
                        }
                        if (bytesToCode.Count > 0) {
                            byte temp = 0;
                            for (int k = 7; k >= 0; k--) {
                                temp |= (byte)(bytesToCode[0] << k);
                                bytesToCode.RemoveAt(0);
                                actualFileLength++;
                                if (bytesToCode.Count <= 0) break;
                            }
                            ftw.WriteByte(temp);
                        }

                        fs.Close();
                    }

                }

                ftw.Close();
            }

            return true;
        }

        private static Dictionary<byte, byte[]> GenerateHuffmanCodeTree(int[] byteCounts, ProgressWindow progressWindow) {
            Dictionary<byte, byte[]> byteCodes = new Dictionary<byte, byte[]>();
            Dictionary<byte, int> counts = new Dictionary<byte, int>();
            List<Node<byte, int>> nodes = new List<Node<byte, int>>();

            for (int i = 0; i < byteCounts.Length; i++) {
                counts.Add((byte)i, byteCounts[i]);
            }
            counts = new Dictionary<byte, int>(counts.OrderByDescending(x => x.Value));

            foreach (var item in counts) {
                nodes.Insert(0, new Node<byte, int>(item.Key, item.Value));
            }

            while (nodes.Count != 1) {
                progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                Node<byte, int> left = nodes.First(); nodes.Remove(left);
                Node<byte, int> right = nodes.First(); nodes.Remove(right);
                Node<byte, int> newNode = new Node<byte, int>(0, left.freq + right.freq, left, right);

                if (nodes.Count == 0) {
                    nodes.Add(newNode);
                    break;
                }

                for (int i = 0; i < nodes.Count; i++) {
                    progressWindow.MakeProgress();
                    progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                    if (nodes[i].freq > newNode.freq) {
                        nodes.Insert(i, newNode);
                        break;
                    }
                    else if (i == nodes.Count - 1) {
                        nodes.Add(newNode);
                        break;
                    }
                }
            }

            Node<byte, int> root = nodes.Last();
            Encode(root, string.Empty, byteCodes, progressWindow);

            return byteCodes;
        }

        private static void Encode(Node<byte, int> root, string str, Dictionary<byte, byte[]> byteCodes, ProgressWindow progressWindow) {
            if (root == null) {
                return;
            }

            Console.Write("\n  ");
            string lineSpaces = string.Empty;
            for (int i = 0; i < str.Length; i++) {
                lineSpaces += " ";
            }
            Console.Write(lineSpaces);
            if (str.Length != 0) {
                Console.Write("∟{0} ({1})", str.Last(), root.freq);
            }

            if (root.left == null && root.right == null) {
                if (string.Empty != str) {
                    byte[] codeWord = new byte[str.Length];
                    for (int i = 0; i < str.Length; i++) {
                        if ('0' == str[i]) codeWord[i] = 0;
                        else codeWord[i] = 1;
                    }
                    byteCodes.Add(root.ch, codeWord);
                }
                else {
                    byteCodes.Add(root.ch, new byte[] { 1 });
                }
                progressWindow.MakeProgress();
                progressWindow.CancelToken.Token.ThrowIfCancellationRequested();

                if (str != string.Empty) Console.Write(": '{0}'", root.ch);
                else Console.Write("{0}  ∟1 ({2}): '{1}'", lineSpaces, root.ch, root.freq);
            }

            Encode(root.left, str + "0", byteCodes, progressWindow);
            Encode(root.right, str + "1", byteCodes, progressWindow);
        }

        private static int[,] CountBytesInFiles(string[] files, ProgressWindow progressWindow) {
            int[,] byteCounts = new int[files.Length, BYTE_MAX];

            CalculateProgressLenght(files, progressWindow);

            int fileIndex = 0;
            foreach (string path in files) {
                using (FileStream fs = File.Open(path, FileMode.Open)) {
                    for (long i = 0; i < fs.Length; i++) {
                        byteCounts[fileIndex, fs.ReadByte()]++;
                        progressWindow.MakeProgress();
                        if (progressWindow.CancelToken.Token.IsCancellationRequested) {
                            fs.Close();
                            progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                        }
                    }
                    fileIndex++;
                    fs.Close();
                }
            }

            return byteCounts;
        }

        private static void CalculateProgressLenght(string[] files, ProgressWindow progressWindow) {
            double max = files.Length * BYTE_MAX + BYTE_MAX * 3 + 2 * files.Length;

            foreach (string path in files) {
                using (FileStream fs = File.Open(path, FileMode.Open)) {
                    max += fs.Length << 1;
                    fs.Close();
                }
            }
            for (int i = 1; i < BYTE_MAX; i++) {
                max += i;
            }

            progressWindow.SetupProgressBar(max);
        }
        #endregion Creating Compressed File [...]

        public static bool DeCompressFiles(string file, string destinationFolder, ProgressWindow progressWindow) {
            progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
            using (FileStream fs = File.Open(file, FileMode.Open)) {
                progressWindow.SetupProgressBar(fs.Length);
                Dictionary<byte, byte[]> byteCodes = GetByteCodes(fs, progressWindow);
                progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                int[] startIndexes;
                int[] fileLengths;
                string[] files = GetFileNames(fs, out startIndexes, out fileLengths, progressWindow);
                progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                DecodeFilesToDestination(fs, destinationFolder, byteCodes, files, startIndexes, fileLengths, progressWindow);
                fs.Close();
            }
            return true;
        }

        #region Decompress file [...]
        private static void DecodeFilesToDestination(FileStream fs, string destinationFolder, Dictionary<byte, byte[]> byteCodes, string[] files, int[] startIndexes, int[] fileLengths, ProgressWindow progressWindow) {
            for (int i = 0; i < files.Length; i++) {
                using (FileStream ftw = File.Open(Path.Join(destinationFolder, files[i]), FileMode.Create)) {
                    int byteLen = (fileLengths[i] / 8) + ((fileLengths[i] % 8) > 0 ? 1 : 0);
                    List<byte> bytes = new List<byte>();
                    for (int j = 0; j < byteLen; j++) {
                        int temp = fs.ReadByte();
                        progressWindow.MakeProgress();
                        if (progressWindow.CancelToken.Token.IsCancellationRequested) {
                            ftw.Close();
                            fs.Close();
                            File.Delete(Path.Join(destinationFolder, files[i]));
                            progressWindow.CancelToken.Token.ThrowIfCancellationRequested();
                        }
                        for (int k = 7; k >= 0; k--) {
                            bytes.Add((byte)((temp & (1 << k)) >> k));
                            byte[] bytesArray = bytes.ToArray();
                            var element = byteCodes.FirstOrDefault(x => x.Value.SequenceEqual(bytesArray));
                            if (null != element.Value) {
                                ftw.WriteByte(element.Key);
                                bytes.Clear();
                            }
                        }
                    }
                    ftw.Close();
                }
            }
        }

        private static string[] GetFileNames(FileStream fs, out int[] startIndexes, out int[] fileLengths, ProgressWindow progressWindow) {
            byte[] length = new byte[4];
            fs.Read(length, 0, 4);
            progressWindow.MakeProgress(4);
            int filesCount = BitConverter.ToInt32(length, 0);

            string[] files = new string[filesCount];
            startIndexes = new int[filesCount];
            fileLengths = new int[filesCount];

            for (int i = 0; i < filesCount; i++) {
                char c = (char)fs.ReadByte();
                progressWindow.MakeProgress();
                while (c != STR_END) {
                    files[i] += c;
                    c = (char)fs.ReadByte();
                    progressWindow.MakeProgress();
                }
                fs.Read(length, 0, 4);
                progressWindow.MakeProgress(4);
                startIndexes[i] = BitConverter.ToInt32(length, 0);
                fs.Read(length, 0, 4);
                progressWindow.MakeProgress(4);
                fileLengths[i] = BitConverter.ToInt32(length, 0);
            }

            return files;
        }

        private static Dictionary<byte, byte[]> GetByteCodes(FileStream fs, ProgressWindow progressWindow) {
            Dictionary<byte, byte[]> byteCodes = new Dictionary<byte, byte[]>();

            int actualCode = 0;
            int actualLength = 0;
            while (actualCode < BYTE_MAX && actualCode < fs.Length) {
                actualLength = fs.ReadByte();
                progressWindow.MakeProgress();

                byte[] bytes = new byte[actualLength];
                int len = (actualLength / 8) + ((actualLength % 8) > 0 ? 1 : 0);
                byte[] read = new byte[len];

                for (int i = 0; i < len; i++) {
                    read[i] = (byte)fs.ReadByte();
                    progressWindow.MakeProgress();
                    int j = 0;
                    for (int k = 7; k >= 0; k--) {
                        if (i * 8 + j >= actualLength) break;
                        bytes[i * 8 + j] = (byte)(((1 << k) & read[i]) >> k);
                        j++;
                    }
                }

                byteCodes.Add((byte)actualCode, bytes);

                actualCode++;
            }
            if (actualCode > BYTE_MAX + 0) {
                throw new Exception("Érvénytelen file!");
            }

            return byteCodes;
        }
        #endregion Decompress file [...]
    }
}
