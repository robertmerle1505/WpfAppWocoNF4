using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WpfAppWocoNF4;
using WpfAppWocoNF4.Models;

namespace WpfAppWoCoNF4
{
    public class Repository
    {
        public Location Location { get; private set; }

        private XNamespace gbxmlNamespace = "http://www.gbxml.org/schema";
        private List<Space> _spaces;
        
        private CultureInfo culture = new CultureInfo("en-US");
        
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
            catch (System.Exception ex)
            {
                throw new System.Exception("Error loading XML file.", ex);
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
                AnalyticalShell = xElement.Element(xNamespace + "AnalyticalShell") != null ? ParseAnalyticalShell(xElement.Element(xNamespace + "AnalyticalShell"), xNamespace) : null,
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

        private AnalyticalShell ParseAnalyticalShell(XElement element1, XNamespace xNamespace1)
                {
                    return new AnalyticalShell
                    {
                        ShellSurface = element1.Element(xNamespace1 + "ShellSurface") != null ? ParseShellSurface(element1.Element(xNamespace1 + "ShellSurface"), xNamespace1) : null,
                    };

                }
            
        

        private ShellSurface ParseShellSurface(XElement element, XNamespace xNamespace1)
        {
            return new ShellSurface
            {
                SurfaceType = element.Attribute("surfaceType")?.Value,
                PolyLoop = element.Element(xNamespace1 + "PolyLoop") != null ? ParsePolyLoop(element.Element(xNamespace1 + "PolyLoop"), xNamespace1) : null,

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
                    var closedShellPolyLoops = s.ShellGeometry.ClosedShell?.PolyLoops;
                    var cartesianPoints = closedShellPolyLoops?.SelectMany(pl => pl.CartesianPoints).ToList();
                    var Z = cartesianPoints.Select(c => c.Z).FirstOrDefault();
                    var horizontalPolyloops = closedShellPolyLoops.Where(p => p.CartesianPoints.All(c => c.Z == Z)).ToList();    
                    var valueTuplesXY = horizontalPolyloops.SelectMany(pl => pl.CartesianPoints).Select(c => (c.X, c.Y)).ToList();
                    var calculatedArea = Calculator.CalculateArea(valueTuplesXY);
                    var diffZ = Math.Abs(cartesianPoints.Max(c => c.Z) - cartesianPoints.Min(c => c.Z));
                    return new SpaceViewModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Area = s.Area,
                        Volume = s.Volume,
                        ClosedShell = s.ShellGeometry.ClosedShell,
                        CartesianPoints = cartesianPoints,
                        CalculatedArea = calculatedArea,
                        CalculatedVolume = Calculator.CalculateVolume(valueTuplesXY, diffZ)
                    };
                }).ToList();
        }


    }

    public class SpaceViewModel
    {
        public string Id { get; internal set; }
        public double Area { get; internal set; }
        public double Volume { get; internal set; }
        public string Name { get; internal set; }
        public List<CartesianPoint> CartesianPoints { get; set; }
        public double CalculatedArea { get; set; }
        public double CalculatedVolume { get; set; }
        public ClosedShell ClosedShell { get; set; }
    }
}
