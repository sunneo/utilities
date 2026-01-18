/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
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
        Dictionary<key, LinkedListNode<key>> dictionary = new Dictionary<key, LinkedListNode<key>>();
        Dictionary<key, type> valueDict = new Dictionary<key, type>();
        LinkedList<key> LRUList = new LinkedList<key>();
        
        public void Clear()
        {
            dictionary.Clear();
            valueDict.Clear();
            LRUList.Clear();
        }
        public bool ContainsKey(key word)
        {
            lock (locker)
            {
                return dictionary.ContainsKey(word);
            }
        }
        public type Get(key word)
        {
            lock (locker)
            {
                if (dictionary.ContainsKey(word))
                {
                    // Move to front (most recently used)
                    LinkedListNode<key> node = dictionary[word];
                    if (node != LRUList.First)
                    {
                        LRUList.Remove(node);
                        LRUList.AddFirst(node);
                    }
                    return valueDict[word];
                }
                return default(type);
            }
        }
        public void Put(key word, type lstSuggestions) // add or replace
        {
            lock (locker)
            {
                if (dictionary.ContainsKey(word))
                {
                    // Update existing entry
                    LinkedListNode<key> node = dictionary[word];
                    LRUList.Remove(node);
                    LRUList.AddFirst(node);
                    valueDict[word] = lstSuggestions;
                }
                else
                {
                    // Check if we need to evict
                    if (LRUList.Count >= bucketSize)
                    {
                        key victim = LRUList.Last.Value;
                        LRUList.RemoveLast();
                        dictionary.Remove(victim);
                        
                        if (victim != null)
                        {
                            type victimVal = valueDict[victim];
                            valueDict.Remove(victim);
                            if (OnErasing != null)
                            {
                                OnErasing(this, victimVal);
                            }
                        }
                    }
                    // Add new entry
                    LinkedListNode<key> newNode = new LinkedListNode<key>(word);
                    LRUList.AddFirst(newNode);
                    dictionary[word] = newNode;
                    valueDict[word] = lstSuggestions;
                }
            }
        }
    }
}
