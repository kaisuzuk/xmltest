using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Runtime;

namespace XMLtest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            A classA = new A();
            XDocument doc = XDocument.Load("C:\\Users\\suzuk\\OneDrive\\デスクトップ\\tmp\\test.xml");


            string str = "35.17094978061289#136.88048013429014#35.17242315212818#136.8897498482816#35.169406219913135#136.88966401759652#35.16737148149401#136.88056596497526#35.17094978061289#136.88048013429014";

            if (!classA.IsValidFmisName(doc)) return;
            if (!classA.IsValidPFD(doc)) return;
            if (!classA.IsValidPNT(doc, str)) return;
            if (!classA.IsValidGRD(doc)) return;
            if (!classA.IsValidPDV(doc)) return;

            Console.WriteLine("SUCCESS");
        }
    }

    class A
    {
        public bool IsValidFmisName(XDocument doc)
        {
            XElement rootElement = doc.XPathSelectElement("/ISO11783_TaskData");

            string FmisName = rootElement.Attribute("ManagementSoftwareManufacture").Value;

            Console.WriteLine(FmisName);

            return true;
        }

        public bool IsValidPFD(XDocument doc)
        {
            XElement PFD = doc.XPathSelectElement("/ISO11783_TaskData/PFD");
            string PFD_B = PFD.Attribute("B").Value;
            string PFD_C = PFD.Attribute("C").Value;
            double PFD_D = Double.Parse(PFD.Attribute("D").Value);

            Console.WriteLine("{0} {1} {2}", PFD_B, PFD_C, PFD_D);

            return true;
        }

        public bool IsValidPNT(XDocument doc, string latlngStr)
        {
            IEnumerable<XElement> PNT = doc.XPathSelectElements("/ISO11783_TaskData/PFD/PLN/LSG/PNT");

            if (PNT.First().ToString() != PNT.Last().ToString())
            {
                Console.WriteLine("PNTタグの最初と最後が一致しません");
                return false;
            }

            string Wkt1 = ConvertPntToWkt(PNT);
            string Wkt2 = ConvertLatlngStrToWkt(latlngStr);

            Console.WriteLine(Wkt1);
            Console.WriteLine(Wkt2);

            // PostGIS で比較する
            
            return true;
        }

        public bool IsValidGRD(XDocument doc)
        {
            XElement GRD = doc.XPathSelectElement("/ISO11783_TaskData/TSK/GRD");
            double GRD_A = Double.Parse(GRD.Attribute("A").Value);
            double GRD_B = Double.Parse(GRD.Attribute("B").Value);
            double GRD_C = Double.Parse(GRD.Attribute("C").Value);
            double GRD_D = Double.Parse(GRD.Attribute("D").Value);
            int    GRD_E = int.Parse(GRD.Attribute("E").Value);
            int    GRD_F = int.Parse(GRD.Attribute("F").Value);

            Console.WriteLine("{0} {1} {2} {3} {4} {5}", GRD_A, GRD_B, GRD_C, GRD_D, GRD_E, GRD_F);

            return true;
        }
        public bool IsValidPDV(XDocument doc)
        {
            IEnumerable<XElement> PDV = doc.XPathSelectElements("/ISO11783_TaskData/TSK/TZN/PDV");
            XElement PDV_0 = doc.XPathSelectElement("/ISO11783_TaskData/TSK/TZN[@A='0']/PDV");
            XElement PDV_3 = doc.XPathSelectElement("/ISO11783_TaskData/TSK/TZN[@A='3']/PDV");

            if (PDV_0.Attribute("B").Value != PDV_3.Attribute("B").Value) return false;

            foreach (XElement element in PDV)
            {
                int val = int.Parse(element.Attribute("B").Value);
                if (val < 0 || val > 90) return false;
            }

            return true;
        }

        public string ConvertPntToWkt(IEnumerable<XElement> PNT)
        {
            string Wkt = "";

            foreach (XElement element in PNT)
            {
                double longitude = Double.Parse(element.Attribute("D").Value);
                double latitude = Double.Parse(element.Attribute("C").Value);
                Wkt += $"{Math.Round(longitude, 8)} {Math.Round(latitude, 8)},";
            }

            Wkt = $"POLYGON(({Wkt.TrimEnd(',')}))";

            return Wkt;
        }

        public string ConvertLatlngStrToWkt(string latlng)
        {
            string Wkt = "";
            List<string> strList = latlng.Split('#').ToList();
            List<string> latList = Enumerable.Range(0, strList.Count()).Where(i => i % 2 == 0).Select(i => strList[i]).ToList();
            List<string> lngList = Enumerable.Range(0, strList.Count()).Where(i => i % 2 == 1).Select(i => strList[i]).ToList();

            for (int i = 0; i < latList.Count(); i++)
            {
                double longitude = Double.Parse(lngList[i]);
                double latitude = Double.Parse(latList[i]);
                Wkt += $"{Math.Round(longitude, 8)} {Math.Round(latitude, 8)},";
            }

            Wkt = $"POLYGON(({Wkt.TrimEnd(',')}))";

            return Wkt;
        }

    }
}
