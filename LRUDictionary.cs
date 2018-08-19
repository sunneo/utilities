using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class LRUDictionary<key, type>
    {
        int bucketSize = 16;
        object locker = new object();
        public event EventHandler<type> OnErasing;
        public LRUDictionary(int bucketSize = 16)
        {
            this.bucketSize = bucketSize;
        }
        Dictionary<key, type> suggestList = new Dictionary<key, type>();
        List<key> LRUStringList = new List<key>();
        public void Clear()
        {
            suggestList.Clear();
            LRUStringList.Clear();
        }
        public bool ContainsKey(key word)
        {
            lock (locker)
            {
                if (suggestList.ContainsKey(word))
                {
                    int foundIdx = -1;
                    for (int i = 0; i < LRUStringList.Count; ++i)
                    {
                        if (LRUStringList[i].Equals(word))
                        {
                            foundIdx = i;
                            break;
                        }
                    }
                    return foundIdx != -1;
                }
                else
                {
                    return false;
                }
            }
        }
        public type Get(key word)
        {
            lock (locker)
            {
                if (suggestList.ContainsKey(word))
                {
                    int foundIdx = -1;
                    for (int i = 0; i < LRUStringList.Count; ++i)
                    {
                        if (LRUStringList[i].Equals(word))
                        {
                            foundIdx = i;
                            break;
                        }
                    }
                    if (foundIdx != -1 && foundIdx > 0)
                    {
                        LRUStringList.RemoveAt(foundIdx);
                        LRUStringList.Insert(0, word);
                    }
                    return suggestList[word];
                }
                return default(type);
            }
        }
        public void Put(key word, type lstSuggestions) // add or replace
        {
            lock (locker)
            {
                if (LRUStringList.Count >= bucketSize)
                {
                    key victim = LRUStringList[LRUStringList.Count - 1];

                    LRUStringList.RemoveAt(LRUStringList.Count - 1);
                    LRUStringList.Insert(0, word);
                    if (victim != null)
                    {
                        type victimVal = suggestList[victim];
                        suggestList.Remove(victim);
                        if (OnErasing != null)
                        {
                            OnErasing(this, victimVal);
                        }
                    }
                }
                if (suggestList.ContainsKey(word))
                {
                    int foundIdx = -1;
                    for (int i = 0; i < LRUStringList.Count; ++i)
                    {
                        if (LRUStringList[i].Equals(word))
                        {
                            foundIdx = i;
                            break;
                        }
                    }
                    if (foundIdx != -1 && foundIdx > 0)
                    {
                        LRUStringList.RemoveAt(foundIdx);
                        suggestList.Remove(word);
                    }
                }
                LRUStringList.Insert(0, word);
                suggestList[word] = lstSuggestions;
            }
        }
    }
}
