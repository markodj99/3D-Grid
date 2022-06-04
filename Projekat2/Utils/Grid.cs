using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Projekat.Model;

namespace Projekat2.Utils
{
    public class Grid
    {
        private readonly double _maxLat = 45.277031;
        private readonly double _minLat = 45.2325;
        private readonly double _maxLon = 19.894459;
        private readonly double _minLon = 19.793909;

        private double _noviX = 0, _noviY = 0;
        public Dictionary<string, double> Coordinates { get; } = new Dictionary<string, double>(4)
        {
            {"maxLat", double.MinValue}, {"maxLon", double.MinValue},
            {"minLat", double.MaxValue}, {"minLon", double.MaxValue}
        };

        private readonly string _path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName + "\\Resources", "Geographic.xml");

        public Dictionary<long, SubstationEntity> SubstationEntities { get; } = new Dictionary<long, SubstationEntity>(17);
        public Dictionary<long, NodeEntity> NodeEntities { get; } = new Dictionary<long, NodeEntity>(795);
        public Dictionary<long, SwitchEntity> SwitchEntities { get; } = new Dictionary<long, SwitchEntity>(1233);
        public Dictionary<long, LineEntity> LineEntities { get; } = new Dictionary<long, LineEntity>(888);

        public Grid() { }

        public void LoadModel()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(_path);

            Substations(xmlDoc);
            Nodes(xmlDoc);
            Switches(xmlDoc);
            Routes(xmlDoc);

            CalculateRelativeCoordinates();
        }

        private void Substations(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText)
                };

                ToLatLon(sub.X, sub.Y, 34, out _noviX, out _noviY);
                if (CheckBounds())
                {
                    if (_noviX > Coordinates["maxLat"]) Coordinates["maxLat"] = _noviX;
                    if (_noviX < Coordinates["minLat"]) Coordinates["minLat"] = _noviX;
                    if (_noviY > Coordinates["maxLon"]) Coordinates["maxLon"] = _noviY;
                    if (_noviY < Coordinates["minLon"]) Coordinates["minLon"] = _noviY;
                    sub.X = _noviX;
                    sub.Y = _noviY;
                    SubstationEntities.Add(sub.Id, sub);
                }
            }
        }

        private void Nodes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeEntity = new NodeEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText)
                };

                ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out _noviX, out _noviY);
                if (CheckBounds())
                {
                    if (_noviX > Coordinates["maxLat"]) Coordinates["maxLat"] = _noviX;
                    if (_noviX < Coordinates["minLat"]) Coordinates["minLat"] = _noviX;
                    if (_noviY > Coordinates["maxLon"]) Coordinates["maxLon"] = _noviY;
                    if (_noviY < Coordinates["minLon"]) Coordinates["minLon"] = _noviY;
                    nodeEntity.X = _noviX;
                    nodeEntity.Y = _noviY;
                    NodeEntities.Add(nodeEntity.Id, nodeEntity);
                }
            }
        }

        private void Switches(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    X = double.Parse(node.SelectSingleNode("X").InnerText),
                    Y = double.Parse(node.SelectSingleNode("Y").InnerText),
                    Status = node.SelectSingleNode("Status").InnerText
                };

                ToLatLon(switchobj.X, switchobj.Y, 34, out _noviX, out _noviY);
                if (CheckBounds())
                {
                    if (_noviX > Coordinates["maxLat"]) Coordinates["maxLat"] = _noviX;
                    if (_noviX < Coordinates["minLat"]) Coordinates["minLat"] = _noviX;
                    if (_noviY > Coordinates["maxLon"]) Coordinates["maxLon"] = _noviY;
                    if (_noviY < Coordinates["minLon"]) Coordinates["minLon"] = _noviY;
                    switchobj.X = _noviX;
                    switchobj.Y = _noviY;
                    SwitchEntities.Add(switchobj.Id, switchobj);
                }
            }
        }

        private void Routes(XmlDocument xmlDoc)
        {
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                bool skip = false;
                LineEntity l = new LineEntity()
                {
                    Id = long.Parse(node.SelectSingleNode("Id").InnerText),
                    Name = node.SelectSingleNode("Name").InnerText,
                    IsUnderground = node.SelectSingleNode("IsUnderground").InnerText.Equals("true"),
                    R = float.Parse(node.SelectSingleNode("R").InnerText),
                    ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText,
                    LineType = node.SelectSingleNode("LineType").InnerText,
                    ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText),
                    FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText),
                    SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText),
                    Vertices = new List<Point>()
                };

                foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes)
                {
                    Point p = new Point
                    {
                        X = double.Parse(pointNode.SelectSingleNode("X").InnerText),
                        Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText)
                    };

                    ToLatLon(p.X, p.Y, 34, out _noviX, out _noviY);

                    if (!CheckBounds())
                    {
                        skip = true;
                        break;
                    }
                    p.X = _noviX;
                    p.Y = _noviY;
                    if (_noviX > Coordinates["maxLat"]) Coordinates["maxLat"] = _noviX;
                    if (_noviX < Coordinates["minLat"]) Coordinates["minLat"] = _noviX;
                    if (_noviY > Coordinates["maxLon"]) Coordinates["maxLon"] = _noviY;
                    if (_noviY < Coordinates["minLon"]) Coordinates["minLon"] = _noviY;
                    l.Vertices.Add(p);
                }

                if (skip) continue;
                if (CheckIfEntityExists(l.FirstEnd, l.SecondEnd)) LineEntities.Add(l.Id, l);
            }
        }

        private bool CheckBounds()
        {
            if (_noviX >= _minLat && _noviX <= _maxLat) if (_noviY >= _minLon && _noviY <= _maxLon) return true;
            return false;
        }

        private bool CheckIfEntityExists(long firstEnd, long secondEnd)
        {
            bool first = false, second = false;

            if (SubstationEntities.ContainsKey(firstEnd) || NodeEntities.ContainsKey(firstEnd) || SwitchEntities.ContainsKey(firstEnd)) first = true;
            if (SubstationEntities.ContainsKey(secondEnd) || NodeEntities.ContainsKey(secondEnd) || SwitchEntities.ContainsKey(secondEnd)) second = true;

            return first && second;
        }

        private void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void CalculateRelativeCoordinates()
        {
            Coordinates["maxLat"] -= Coordinates["minLat"];
            Coordinates["maxLon"] -= Coordinates["minLon"];

            foreach (var substationEntity in SubstationEntities.Values)
            {
                substationEntity.X -= Coordinates["minLat"];
                substationEntity.Y -= Coordinates["minLon"];
            }

            foreach (var nodeEntity in NodeEntities.Values)
            {
                nodeEntity.X -= Coordinates["minLat"];
                nodeEntity.Y -= Coordinates["minLon"];
            }

            foreach (var switchEntity in SwitchEntities.Values)
            {
                switchEntity.X -= Coordinates["minLat"];
                switchEntity.Y -= Coordinates["minLon"];
            }

            foreach (var point in LineEntities.SelectMany(lineEntity => lineEntity.Value.Vertices))
            {
                point.X -= Coordinates["minLat"];
                point.Y -= Coordinates["minLon"];
            }
        }
    }
}
