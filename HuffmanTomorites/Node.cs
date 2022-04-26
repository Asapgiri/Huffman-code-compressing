using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuffmanTomorites {
    internal class Node<K, T> where T : struct where K : struct {
		public Node(K _ch, T _freq, Node<K, T> _left = null, Node<K, T> _right = null) {
			ch = _ch;
			freq = _freq;
			left = _left;
			right = _right;
		}
		public K ch;
		public T freq;
		public Node<K, T> left, right;
	}
}
