using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.OptionParser.Attributes;

namespace TestUtility
{
    [TestClass]
    public class TestIniDeserialize
    {
        public class PairItem
        {
            public String Key;
            public String Value;
        }
        public class TestClass
        {
            public int intItem;
            public String stringItem;
            public int PairCount;
            [FlattenArrayLengthName("PairCount"), FlattenArrayName("pairs$INDEX$","$INDEX$")]
            public PairItem[] pairs;
        }
        [TestMethod]
        public void TestTemplateDeserialize()
        {
            String iniContent = String.Join(Environment.NewLine,
                "intItem=1",
                "stringItem=2",
                "PairCount=3",
                "pairs0.Key=K1",
                "pairs0.Value=V1",
                "pairs1.Key=K2",
                "pairs1.Value=V2",
                "pairs2.Key=K3",
                "pairs2.Value=V3");
            TestClass clz= IniReader.DeserializeString<TestClass>(iniContent);
        }
    }
}
