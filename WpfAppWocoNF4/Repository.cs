using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using WpfAppWocoNF4;
using WpfAppWocoNF4.Models;

namespace WpfAppWoCoNF4
{
    public class Repository
    {

        public Location Location { get; private set; }

        private List<Space> _spaces;
        private CultureInfo culture = new CultureInfo("en-US");
        private XNamespace gbxmlNamespace = "http://www.gbxml.org/schema";

        public void LoadFromXml(string filePath)
        {
            try
            {
                XDocument doc = XDocument.Load(filePath);

                Location = doc.Descendants(gbxmlNamespace + "Location")
                    .Select(element => new Location
                    {
                        Name = element.Element(gbxmlNamespace + "Name")?.Value
                    })
                    .FirstOrDefault();

                _spaces = doc.Descendants(gbxmlNamespace + "Space")
                    .Select(element => ParseSpace(element, gbxmlNamespace))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading XML file.", ex);
            }
        }

        private Space ParseSpace(XElement element, XNamespace ns)
        {
            return new Space
            {
                Id = element.Attribute("id")?.Value,
                ZoneIdRef = element.Attribute("zoneIdRef")?.Value,
                Name = element.Element(ns + "Name")?.Value,
                Area = Convert.ToDouble(element.Element(ns + "Area")?.Value, culture),
                Volume = Convert.ToDouble(element.Element(ns + "Volume")?.Value, culture),
                ShellGeometry = element.Element(ns + "ShellGeometry") != null ? ParseShellGeometry(element.Element(ns + "ShellGeometry"), ns) : null,
            };
        }

        private ShellGeometry ParseShellGeometry(XElement xElement, XNamespace xNamespace)
        {
            return new ShellGeometry
            {
                Id = xElement.Attribute("id")?.Value,
                Unit = xElement.Attribute(xNamespace + "Unit")?.Value,
                ClosedShell = xElement.Element(xNamespace + "ClosedShell") != null ? ParseClosedShell(xElement.Element(xNamespace + "ClosedShell"), xNamespace) : null,
            };
        }

        private ClosedShell ParseClosedShell(XElement element, XNamespace xNamespace)
        {
            return new ClosedShell
            {
                PolyLoops = element.Descendants(xNamespace + "PolyLoop")
                    .Select(e => ParsePolyLoop(e, xNamespace))
                    .ToList()
            };
        }

        private PolyLoop ParsePolyLoop(XElement element, XNamespace xNamespace1)
        {
            return new PolyLoop()
            {
                CartesianPoints = element.Descendants(xNamespace1 + "CartesianPoint")
                    .Select(e => ParseCartesianPoint(e, xNamespace1))
                    .ToList()
            };
        }

        private CartesianPoint ParseCartesianPoint(XElement arg1, XNamespace arg2)
        {
            return new CartesianPoint()
            {
                Coordinates = arg1.Descendants(arg2 + "Coordinate")
                    .Select(element => ParseCoordinate(element))
                    .ToList()
            };
        }

        private Coordinate ParseCoordinate(XElement element)
        {
            return new Coordinate()
            {
                Value = Convert.ToDouble(element.Value, culture)
            };
        }

        public IEnumerable<Space> GetAll()
        {
            return _spaces;
        }

        public IEnumerable<SpaceViewModel> GetAllSummerized()
        {
            return _spaces
                .Select(s =>
                {
                    var polyLoops = s.ShellGeometry.ClosedShell?.PolyLoops;
                    var cartesianPoints = polyLoops?.SelectMany(pl => pl.CartesianPoints).ToList();
                    var zCoordinate = cartesianPoints.Select(c => c.Z).FirstOrDefault();
                    var areaPolyloops = polyLoops.Where(p => p.CartesianPoints.All(c => c.Z == zCoordinate)).ToList();    
                    var xyCoordinates = areaPolyloops.SelectMany(pl => pl.CartesianPoints).Select(c => (c.X, c.Y)).ToList();
                    var calculatedArea = Calculator.CalculateArea(xyCoordinates);
                    var height = Math.Abs(cartesianPoints.Max(c => c.Z) - cartesianPoints.Min(c => c.Z));
                    return new SpaceViewModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Area = s.Area,
                        Volume = s.Volume,
                        ClosedShell = s.ShellGeometry.ClosedShell,
                        CartesianPoints = cartesianPoints,
                        CalculatedArea = calculatedArea,
                        CalculatedVolume = Calculator.CalculateVolume(xyCoordinates, height)
                    };
                }).ToList();
        }
    }
}
