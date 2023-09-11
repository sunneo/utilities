using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SequentialDictionary<K,V>:Dictionary<K,V>, IDictionary<K, V>
    {
        LinkedList<K> seqKey = new LinkedList<K>();
        Dictionary<K, LinkedListNode<K>> keyMap = new Dictionary<K, LinkedListNode<K>>();
        
        
        public new void Add(K key, V value)
        {
            Add(key, value, false);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="origKey"></param>
		/// <param name="newKey"></param>
		/// <param name="value"></param>
		protected void ReplaceOrAdd(K origKey, K newKey, V value)
		{
			LinkedListNode<K> origKeyNode = null;
			// Find original key node from key map.
			if (keyMap.ContainsKey(origKey))
			{
				origKeyNode = keyMap[origKey];
			}
			// Replace key from key map and add new value.
			if (origKeyNode != null)
			{
				base.Remove(origKey);              // Remove data by key from dictionary.
				keyMap.Remove(origKey);             // Remove node by key from key map.
				origKeyNode.Value = newKey;         // Update key value from node.
				keyMap.Add(newKey, origKeyNode);    // Add key and node to  key map
				base.Add(newKey, value);           // Put new key and value.
			}
			else
			{
				this.Add(newKey, value);            // Add new one if unable to find key.
			}
		}


		/**
		 * Replaces the element at the specified position in this map with
		 * the specified element.
		 *
		 * @param index - Index of the element to replace
		 * @param key
		 * @param value
		 */
		public void Add(int index, K key, V value)
		{
			if (index < 0 || index - 1 > keyMap.Count)
			{
				Add(key, value);
			}
			else
			{
				// Find key by index.
				int i = 0;
				K target = default(K);
				foreach (K k in GetSequentialKey())
				{
					if (i == index)
					{
						target = k;
						break;
					}
					i++;
				}
				ReplaceOrAdd(target, key, value);
			}
		}

		public void Add(K key, V value, bool reorderOnConflict)
		{
			base.Add(key, value);
			// reorder
			if (reorderOnConflict)
			{
				if (keyMap.ContainsKey(key))
				{
					LinkedListNode<K> node = keyMap[key];
					node.List.Remove(node);
				}
				keyMap.Add(key, seqKey.AddLast(key));
			}
			else
			{
				if (!keyMap.ContainsKey(key))
				{
					keyMap.Add(key, seqKey.AddLast(key));
				}

			}
		}
		public new bool Remove(K key)
		{
			bool ret = base.Remove(key);
			if (ret)
			{
				LinkedListNode<K> node = keyMap[key];
				node.List.Remove(node);
				keyMap.Remove(key);
			}
			return ret;
		}

		public new void Clear()
		{
			base.Clear();
			seqKey.Clear();
			this.keyMap.Clear();
		}
		public List<V> GetSequentialValues()
		{
			List<V> ret = new List<V>();
			foreach(K k in GetSequentialKey())
            {
				ret.Add(this[k]);
            }
			return ret;
		}

		public List<KeyValuePair<K, V>> GetSequentialEntrySet()
		{
			List<KeyValuePair<K,V>> ret = new List<KeyValuePair<K, V>>();
			foreach (K k in GetSequentialKey())
			{
				ret.Add(new KeyValuePair<K,V>(k,this[k]));
			}
			return ret;
		}
		public IEnumerable<K> GetSequentialKey()
		{
			return this.seqKey;

		}
	}
}
