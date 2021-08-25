using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace se_remote {
    [Serializable]
    public class XObj {
        public string code { get; set; }
        public string[] refs { get; set; }
    }

    class X {
        public static byte[] ToXml(string code, string[] refs) {
            var obj = new XObj() { code = code, refs = refs };

            using (var tw = new System.IO.StringWriter()) {
                var xml = new System.Xml.Serialization.XmlSerializer(typeof(XObj));
                xml.Serialize(tw, obj);

                var xtxt = tw.ToString();
                var buf = Encoding.UTF8.GetBytes(xtxt);
                return BitConverter.GetBytes((int)buf.Length).Concat(buf).ToArray();
            }
        }
    }
}
