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
using System.Threading.Tasks;

namespace OptionParser.OptionParse
{

    /// <summary>
    /// OptionParserUnit template
    /// an option value parser which can be implicitly converted to a value.
    /// 
    /// </summary>
    /// <typeparam name="T">data type</typeparam>
    public class OptionParserUnit<T> : OptionParserUnitBase
    {
        public Func<String, T> ParseDelegator;
        public T Value;
        public String RawValue;
        public static implicit operator T(OptionParserUnit<T> pthis)
        {
            return pthis.Value;
        }
        /// <summary>
        /// override implemetation which invokes ParseDelegator to assign parsed value to Value field.
        /// while keep original string input to RawValue.
        /// </summary>
        /// <param name="Value"></param>
        public override void Parse(String Value)
        {
            base.Parse(Value);
            this.RawValue = Value;

            if (ParseDelegator != null)
            {
                this.Value = ParseDelegator(Value);
            }
        }
        /// <summary>
        /// constructor for option parser
        /// </summary>
        /// <param name="key">option name</param>
        /// <param name="ParseDelegator">Converter to translate string to value</param>
        public OptionParserUnit(String key, Func<String, T> ParseDelegator = null)
        {
            this.Key = key;
            this.ParseDelegator = ParseDelegator;
            Value = default(T);
        }

    }
}
