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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OptionParser.OptionParse
{

    /// <summary>
    /// A Reflection-Oriented TryParse bridge
    /// </summary>
    /// <example>
    /// bool boolVal = DynamicTryParse<bool>.Parse("true");
    /// int intVal = DynamicTryParse<int>.Parse("1048576");
    /// 
    /// class KVPair{
    ///    public String Key;
    ///    public String Value;
    ///    public static bool TryParse(String value,out KVPair output){
    ///       String[] splits=value.Split('=');
    ///       if(splits.Length==2) {
    ///          KVPair ret = new KVPair();
    ///          ret.Key = splits[0];
    ///          ret.Value = splits[1];
    ///          output=ret;
    ///          return true;
    ///       }
    ///       output=null;
    ///       return false;
    ///    }
    /// }
    /// KVPair kvpair = DynamicTryParse<KVPair>.Parse("A=100");
    /// 
    /// </example>
    /// <typeparam name="T">DataType which implements static method TryParse</typeparam>
    public class DynamicTryParse<T>
    {
        private delegate bool TryParseInternal(String value, out T outputValue);
        /// <summary>
        /// Use TryParseInternal to convert TryParse interface into given datatype
        /// and invoke to write parsed value.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="outputValue"></param>
        /// <returns></returns>
        public static bool TryParse(String stringValue, out T convertedValue)
        {
            var targetType = typeof(T);
            if (targetType == typeof(string))
            {
                convertedValue = (T)Convert.ChangeType(stringValue, typeof(T));
                return true;
            }
            var nullableType = targetType.IsGenericType &&
                           targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (nullableType)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    convertedValue = default(T);
                    return true;
                }
                targetType = new NullableConverter(targetType).UnderlyingType;
            }

            Type[] argTypes = { typeof(string), targetType.MakeByRefType() };
            var tryParseMethodInfo = targetType.GetMethod("TryParse", argTypes);
            if (tryParseMethodInfo == null)
            {
                convertedValue = default(T);
                return false;
            }

            object[] args = { stringValue, null };
            var successfulParse = (bool)tryParseMethodInfo.Invoke(null, args);
            if (!successfulParse)
            {
                convertedValue = default(T);
                return false;
            }

            convertedValue = (T)args[1];
            return true;
        }
        /// <summary>
        /// parse given string into specified data-typed value.
        /// </summary>
        /// <param name="s">input string to convert</param>
        /// <returns>converted value, when parse failed it will return default value of data type</returns>
        public static T Parse(String s)
        {
            T ret = default(T);
            TryParse(s, out ret);
            return ret;
        }
    }
}
