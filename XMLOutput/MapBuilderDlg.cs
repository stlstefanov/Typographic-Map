namespace XMLOutputTester
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using Svg;
    using DotSpatial.Data;
    using System.Linq;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using DotSpatial.Projections;
    using DotSpatial.Topology;

    public partial class MapBuilderDlg : Form
    {
        #region Fields

        private SvgDocument svgDoc;
        private SvgDefinitionList svgDefs;
        private double worldMinX;
        private double worldMinY;

        #endregion


        #region Constructor

        public MapBuilderDlg()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        #endregion


        #region Enum

        private enum AreaType
        {
            Water,
            Land,
            Admin
        }

        #endregion


        #region Properties

        private float Factor { get { return float.Parse(txtFactor.Text, CultureInfo.InvariantCulture); } }

        private float PathFontSize { get { return 2.0f * Factor; } }

        private float NarrowRoadFontSize { get { return 2.5f * Factor; } }

        private float MediumWideRoadFontSize { get { return 3.0f * Factor; } }

        private float WideRoadFontSize { get { return 3.0f * Factor; } }

        private float RailFontSize { get { return 2.5f * Factor; } }

        private float RiverFontSize { get { return 5.5f * Factor; } }

        private float AreaFontSize { get { return 2.5f * Factor; } }

        #endregion


        #region Methods

        private void Execute()
        {
            // Init
            var extent = new Extent(197342, 4731653, 199677, 4733278);

            var roadsShp = ReadShpFile(@"..\..\..\Data\{0}.imposm-shapefiles\{0}_osm_roads.shp").Features.Where(ExtentsIntersect(extent)).OrderBy(f => f.DataRow[8]).ToList();
            var waterwaysShp = ReadShpFile(@"..\..\..\Data\{0}.imposm-shapefiles\{0}_osm_waterways.shp").Features.Where(ExtentsIntersect(extent)).ToList();
            var waterareasShp = ReadShpFile(@"..\..\..\Data\{0}.imposm-shapefiles\{0}_osm_waterareas.shp").Features.Where(ExtentsIntersect(extent)).ToList();
            var landusagesShp = ReadShpFile(@"..\..\..\Data\{0}.imposm-shapefiles\{0}_osm_landusages.shp").Features.Where(ExtentsIntersect(extent)).OrderBy(f => f.DataRow[5]).ToList();
            var adminShp = ReadShpFile(@"..\..\..\Data\{0}.imposm-shapefiles\{0}_osm_admin.shp").Features
                .Where(ExtentsIntersect(extent))
                .Where(f => f.DataRow[4] is long && (long)f.DataRow[4] == 8)
                .Select(f => Intersection(f, extent))
                .Where(f => f != null)
                .ToList();

            InitDocument(extent);

            // Draw
            DrawAreas(adminShp, AreaType.Admin);
            DrawAreas(landusagesShp, AreaType.Land);
            DrawWaterways(waterwaysShp);
            DrawAreas(waterareasShp, AreaType.Water);
            DrawRoads(roadsShp);

            // Make .svg file & Render
            //var stream = new MemoryStream();
            //svgDoc.Write(stream);
            //textBox1.Text = Encoding.UTF8.GetString(stream.GetBuffer());

            if (!Directory.Exists(@"C:\temp\"))
            {
                Directory.CreateDirectory(@"C:\temp\");
            }

            svgDoc.Write(@"C:\temp\render.svg");

            // Done
            progressBar.Value = 0;

            var url = @"file:///C:/temp/render.svg";
            Process.Start("chrome.exe", url);
        }

        private static Shapefile ReadShpFile(string path)
        {
            var extractName = string.Format(path, "sofia_bulgaria"); //ex_ApZcSStidYenm2QoFXYnh5c9k5DGv
            if (!File.Exists(extractName))
            {
                return null;
            }

            var file = Shapefile.OpenFile(extractName);
            file.Reproject(KnownCoordinateSystems.Projected.UtmWgs1984.WGS1984UTMZone35N);

            return file;
        }

        private void InitDocument(Extent extent)
        {
            var shapeWidth = (int)(extent.MaxX - extent.MinX);
            var shapeHeight = (int)(extent.MaxY - extent.MinY);

            worldMinX = extent.MinX;
            worldMinY = extent.MinY;

            svgDoc = new SvgDocument
            {
                //Width = new SvgUnit(SvgUnitType.Millimeter, 1189),
                //Height = new SvgUnit(SvgUnitType.Millimeter, 841),

                Width = shapeWidth,
                Height = shapeHeight,

                ViewBox = new SvgViewBox(0, 0, shapeWidth, shapeHeight)
            };

            svgDefs = new SvgDefinitionList();
            svgDoc.Children.Add(svgDefs);

            //svgDoc.Children.Add(new SvgRectangle { X = 0, Y = 0, Height = paperHeight, Width = paperWidth });
        }

        private void DrawAreas(IList<IFeature> features, AreaType areaType)
        {
            // Iterate features
            var areaId = 0;

            progressBar.Value = 0;
            progressBar.Maximum = features.Count - 1;

            foreach (var feature in features)
            {
                var type = feature.DataRow[3].ToString();
                var defaultTitle = type;
                SvgColourServer fontColor = null;

                switch (type)
                {
                    case "park":
                        defaultTitle = "парк";
                        fontColor = new SvgColourServer(Color.LightGreen);
                        break;

                    case "grass":
                        defaultTitle = "тревна площ";
                        fontColor = new SvgColourServer(Color.LightGreen);
                        break;

                    case "garden":
                        defaultTitle = "градина";
                        fontColor = new SvgColourServer(Color.LightGreen);
                        break;

                    case "forest":
                    case "wood":
                        defaultTitle = "дървесна растителност";
                        fontColor = new SvgColourServer(Color.LightGreen);
                        break;

                    case "swimming_pool":
                        defaultTitle = "плувен басейн";
                        fontColor = new SvgColourServer(Color.LightBlue);
                        break;

                    case "residential":
                        continue;
                        defaultTitle = "жилищен квартал";
                        fontColor = new SvgColourServer(Color.LightGray);
                        break;

                    case "footway":
                        defaultTitle = "площадка";
                        fontColor = new SvgColourServer(Color.LightGray);
                        break;

                    case "parking":
                        defaultTitle = "паркинг";
                        fontColor = new SvgColourServer(Color.LightGray);
                        break;

                    case "pitch":
                        defaultTitle = "игрище";
                        fontColor = new SvgColourServer(Color.LightGray);
                        break;

                    default:
                        switch (areaType)
                        {
                            case AreaType.Admin:
                            case AreaType.Land:
                                fontColor = new SvgColourServer(Color.LightGray);
                                break;

                            case AreaType.Water:
                                fontColor = new SvgColourServer(Color.LightBlue);
                                break;

                            default:
                                throw new Exception();
                        }

                        break;
                }

                var s = feature.DataRow[2].ToString();

                var str = ChangeEncoding(feature.DataRow[2].ToString());
                var titleSample = (str == string.Empty ? defaultTitle : str.ToString()) + "●";

                // Clip path
                areaId++;
                var clipPath = new SvgClipPath { ID = string.Format("luses{0}{1}", areaId, areaType) };
                clipPath.Children.Add(MakeSvgPath(feature));

                // Background color path
                var bgPath = MakeSvgPath(feature);
                bgPath.Fill = new SvgColourServer(Color.White);
                
                if (cbDebug.Checked)
                {
                    bgPath.Stroke = new SvgColourServer(Color.Red);
                    bgPath.StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 0.5f);
                }

                // Add paths to document
                svgDoc.Children.Add(bgPath);
                svgDoc.Children.Add(clipPath);

                // SvgText
                var txt = new SvgText
                {
                    Font = "PT Serif",
                    FontSize = new SvgUnit(SvgUnitType.Millimeter, AreaFontSize),
                    Fill = fontColor,
                    LengthAdjust = SvgTextLengthAdjust.Spacing
                };

                txt.CustomAttributes.Add("alignment-baseline", "middle");

                var pathMeasure = MeasureSvgElement(MakeSvgPath(feature));
                var txtMeasure = MeasureSvgText(titleSample, txt.FontSize.Value, txt.Font, false, false);

                var title = new StringBuilder();

                var rowWidth = txtMeasure.Item1;
                var w = 0f;
                while (w <= pathMeasure.Item1 * 1.1) // add 10% width to compensate for inaccurate element measure
                {
                    title.Append(titleSample);
                    w += rowWidth;
                }

                var rowHeight = txtMeasure.Item2;
                var h = 0f;
                var shiftedTitle = title.ToString();
                while (h <= pathMeasure.Item2)
                {
                    var uc_x = new SvgUnitCollection { new SvgUnit((float)(feature.Envelope.X - worldMinX)) };
                    var uc_dy = new SvgUnitCollection { new SvgUnit(SvgUnitType.Millimeter, AreaFontSize) };
                    var span = new SvgTextSpan { X = uc_x, Dy = uc_dy };

                    span.Nodes.Add(new SvgContentNode { Content = shiftedTitle });
                    txt.Children.Add(span);

                    var lettersToTrim = (int)(titleSample.Length / 2);
                    var trim = shiftedTitle.Substring(0, lettersToTrim);
                    shiftedTitle = shiftedTitle.Substring(lettersToTrim) + trim;

                    h += rowHeight;
                }

                txt.X = new SvgUnitCollection { new SvgUnit((float)(feature.Envelope.X - worldMinX)) };
                txt.Y = new SvgUnitCollection { new SvgUnit(svgDoc.Height - (float)(feature.Envelope.Y - worldMinY)) };

                txt.CustomAttributes.Add("clip-path", string.Format("url(#{0})", clipPath.ID));

                svgDoc.Children.Add(txt);
                
                progressBar.Value++;
            }
        }

        private void DrawRoads(IList<IFeature> features)
        {
            // Iterate features
            var pathId = 0;

            progressBar.Value = 0;
            progressBar.Maximum = features.Count - 1;

            foreach (var feature in features)
            {
                var roadClass = feature.DataRow[11].ToString();
                var roadType = feature.DataRow[2].ToString();

                SvgColourServer color;
                SvgUnit size;

                var defText = roadType;
                if (roadClass != "railway")
                {
                    // reference http://wiki.openstreetmap.org/wiki/Key:highway

                    if (roadType.Contains("_link"))
                    {
                        defText = "естакада";
                    }

                    switch (roadType)
                    {
                        //---- Wide roads ----

                        case "motorway":
                            color = new SvgColourServer(Color.Crimson);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        case "motorway_link":
                            color = new SvgColourServer(Color.Crimson);
                            size = new SvgUnit(SvgUnitType.Millimeter, MediumWideRoadFontSize);
                            break;

                        case "trunk":
                        case "trunk_link":
                            color = new SvgColourServer(Color.Crimson);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        case "primary":
                        case "primary_link":
                            color = new SvgColourServer(Color.OrangeRed);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        case "secondary":
                            defText = "подлез";
                            color = new SvgColourServer(Color.DarkOrange);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        case "secondary_link":
                            color = new SvgColourServer(Color.DarkOrange);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        case "tertiary":
                        case "tertiary_link":
                            color = new SvgColourServer(Color.Orange);
                            size = new SvgUnit(SvgUnitType.Millimeter, WideRoadFontSize);
                            break;

                        //---- Medium wide roads ----

                        case "unclassified":
                            color = new SvgColourServer(Color.DarkCyan);
                            size = new SvgUnit(SvgUnitType.Millimeter, MediumWideRoadFontSize);
                            break;

                        case "residential":
                            color = new SvgColourServer(Color.DarkCyan);
                            size = new SvgUnit(SvgUnitType.Millimeter, MediumWideRoadFontSize);
                            break;

                        //---- Narrow roads ----

                        case "service":
                            color = new SvgColourServer(Color.DarkCyan);
                            size = new SvgUnit(SvgUnitType.Millimeter, NarrowRoadFontSize);
                            break;

                        //---- Special road types ----

                        case "living_street":
                            defText = "пешеходна улица";
                            color = new SvgColourServer(Color.DodgerBlue);
                            size = new SvgUnit(SvgUnitType.Millimeter, MediumWideRoadFontSize);
                            break;

                        case "pedestrian":
                            defText = "пешеходна улица";
                            color = new SvgColourServer(Color.DodgerBlue);
                            size = new SvgUnit(SvgUnitType.Millimeter, MediumWideRoadFontSize);
                            break;

                        case "road":
                            defText = "път";
                            color = new SvgColourServer(Color.YellowGreen);
                            size = new SvgUnit(SvgUnitType.Millimeter, NarrowRoadFontSize);
                            break;

                        case "raceway":
                            defText = "писта";
                            color = new SvgColourServer(Color.Crimson);
                            size = new SvgUnit(SvgUnitType.Millimeter, NarrowRoadFontSize);
                            break;

                        case "track":
                            color = new SvgColourServer(Color.YellowGreen);
                            size = new SvgUnit(SvgUnitType.Millimeter, PathFontSize);
                            break;

                        //---- Paths ----

                        case "footway":
                        case "path":
                            defText = "алея";
                            color = new SvgColourServer(Color.BurlyWood);
                            size = new SvgUnit(SvgUnitType.Millimeter, PathFontSize);
                            break;

                        case "steps":
                            defText = "стъпала";
                            color = new SvgColourServer(Color.BurlyWood);
                            size = new SvgUnit(SvgUnitType.Millimeter, PathFontSize);
                            break;

                        case "cycleway":
                            defText = "велоалея";
                            color = new SvgColourServer(Color.SteelBlue);
                            size = new SvgUnit(SvgUnitType.Millimeter, PathFontSize);
                            break;

                        case "bridleway":
                            defText = "пътека за езда";
                            color = new SvgColourServer(Color.SaddleBrown);
                            size = new SvgUnit(SvgUnitType.Millimeter, PathFontSize);
                            break;

                        default:
                            continue;

                            //case "proposed":
                            //    color = new SvgColourServer(Color.SteelBlue);
                            //    size = new SvgUnit(SvgUnitType.Millimeter, 4);
                            //    break;

                            //case "construction":
                            //    color = new SvgColourServer(Color.SteelBlue);
                            //    size = new SvgUnit(SvgUnitType.Millimeter, 4);
                            //    break;

                            //case "rest_area":
                            //    color = new SvgColourServer(Color.SteelBlue);
                            //    size = new SvgUnit(SvgUnitType.Millimeter, 4);
                            //    break;
                    }
                }
                else
                {
                    switch (roadType)
                    {
                        case "rail":
                            defText = "железопътна линия";
                            color = new SvgColourServer(Color.Black);
                            size = new SvgUnit(SvgUnitType.Millimeter, RailFontSize);
                            break;

                        //case "tram":
                        //    color = new SvgColourServer(Color.Black);
                        //    size = new SvgUnit(SvgUnitType.Millimeter, Path);
                        //    break;

                        default:
                            continue;
                    }
                }

                // Make the path
                pathId++;
                var path = MakeSvgPath(feature);
                path.ID = string.Format("road{0}", pathId);

                svgDefs.Children.Add(path);

                // Make the text & outline
                var title = feature.DataRow[3] == null || string.IsNullOrWhiteSpace(feature.DataRow[3].ToString())
                    ? defText
                    : ChangeEncoding(feature.DataRow[3].ToString());

                if (size.Value == PathFontSize)
                {
                    title = title.ToLower();
                }
                else
                {
                    title = title.ToUpper();
                }

                if (cbDebug.Checked)
                {
                    // Debug paths
                    var pathDebug = (SvgPath) path.Clone();
                    pathDebug.ID += "Debug";
                    pathDebug.Fill = SvgPaintServer.None;
                    pathDebug.Stroke = new SvgColourServer(Color.Red);
                    pathDebug.StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 0.5f);
                    svgDoc.Children.Add(pathDebug);
                }

                var font = "Roboto Medium";
                svgDoc.Children.Add(MakePathText(path.ID, feature, color, size, title, true, font, false, false));
                svgDoc.Children.Add(MakePathText(path.ID, feature, color, size, title, false, font, false, false));

                progressBar.Value++;
            }
        }

        private void DrawWaterways(IList<IFeature> features)
        {
            // Iterate features
            var pathId = 0;

            progressBar.Value = 0;
            progressBar.Maximum = features.Count - 1;

            foreach (var feature in features)
            {
                string defText;

                SvgColourServer color;
                SvgUnit size;

                var waterwayType = feature.DataRow[3].ToString();

                switch (waterwayType)
                {
                    //---- Rivers ----

                    case "river":
                        color = new SvgColourServer(Color.Navy);
                        size = new SvgUnit(SvgUnitType.Millimeter, RiverFontSize);
                        defText = "река";
                        break;

                    case "stream":
                        color = new SvgColourServer(Color.Navy);
                        size = new SvgUnit(SvgUnitType.Millimeter, RiverFontSize);
                        defText = "поток";
                        break;

                    case "canal":
                        color = new SvgColourServer(Color.Navy);
                        size = new SvgUnit(SvgUnitType.Millimeter, RiverFontSize);
                        defText = "канал";
                        break;

                    case "drain":
                        color = new SvgColourServer(Color.Navy);
                        size = new SvgUnit(SvgUnitType.Millimeter, RiverFontSize);
                        defText = "отводнителен канал";
                        break;

                    case "ditch":
                        color = new SvgColourServer(Color.Blue);
                        size = new SvgUnit(SvgUnitType.Millimeter, RiverFontSize);
                        defText = "канавка";
                        break;

                    default:
                        continue;
                }

                // Make the path
                pathId++;
                var path = MakeSvgPath(feature);
                path.ID = string.Format("waterway{0}", pathId);

                svgDefs.Children.Add(path);

                // Make the text & outline
                var title = feature.DataRow[2] == null || string.IsNullOrWhiteSpace(feature.DataRow[2].ToString())
                    ? defText
                    : ChangeEncoding(feature.DataRow[2].ToString());

                //title = title.ToUpper();

                var font = "PT Serif";
                svgDoc.Children.Add(MakePathText(path.ID, feature, color, size, title, true, font, true, true));
                svgDoc.Children.Add(MakePathText(path.ID, feature, color, size, title, false, font, true, true));

                progressBar.Value++;
            }
        }

        private static string ChangeEncoding(string s)
        {
            byte[] bytes = Encoding.Default.GetBytes(s);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion


        #region Geometry related methods

        private static Func<IFeature, bool> ExtentsIntersect(Extent extent)
        {
            return f => new Extent(f.Envelope.Minimum.X, f.Envelope.Minimum.Y, f.Envelope.Maximum.X, f.Envelope.Maximum.Y).Intersects(extent);
        }

        private static IFeature Intersection(IFeature f, Extent extent)
        {
            var p = new Polygon(
                new List<Coordinate>
                {
                    new Coordinate(extent.MinX, extent.MinY),
                    new Coordinate(extent.MinX + extent.Width, extent.MinY),
                    new Coordinate(extent.MinX + extent.Width, extent.MinY + extent.Height),
                    new Coordinate(extent.MinX, extent.MinY + extent.Height),
                });

            var res = f;
            res = res.Intersection(p);
            if (res == null)
            {
                return null;
            }

            res.DataRow = f.DataRow;
            return res;
        }

        #endregion


        #region Svg releted methods

        private SvgPath MakeSvgPath(IFeature feature)
        {
            var path = new SvgPath();
            for (var i = 1; i < feature.Coordinates.Count; i++)
            {
                var p1 = new PointF((float)(feature.Coordinates[i - 1].X - worldMinX), svgDoc.Height - (float)(feature.Coordinates[i - 1].Y - worldMinY));
                var p2 = new PointF((float)(feature.Coordinates[i].X - worldMinX), svgDoc.Height - (float)(feature.Coordinates[i].Y - worldMinY));

                var shotenPathBy = 0;

                if (!path.PathData.Any())
                {
                    // Shorten the path
                    var d = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)); //distance
                    var r = shotenPathBy / d; //segment ratio

                    var x3 = r * p2.X + (1 - r) * p1.X; //find point that divides the segment
                    var y3 = r * p2.Y + (1 - r) * p1.Y; //into the ratio (1-r):r

                    var p3 = new PointF((float)x3, (float)y3);

                    path.PathData.Add(new Svg.Pathing.SvgMoveToSegment(p3));
                    path.PathData.Add(new Svg.Pathing.SvgLineSegment(p3, p2));
                }
                else if (i == feature.Coordinates.Count - 1)
                {
                    // Shorten the path
                    var d = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)); //distance
                    var r = shotenPathBy / d; //segment ratio

                    var x3 = r * p1.X + (1 - r) * p2.X; //find point that divides the segment
                    var y3 = r * p1.Y + (1 - r) * p2.Y; //into the ratio (1-r):r

                    var p3 = new PointF((float)x3, (float)y3);

                    path.PathData.Add(new Svg.Pathing.SvgLineSegment(p1, p3));
                }
                else
                {
                    path.PathData.Add(new Svg.Pathing.SvgLineSegment(p1, p2));
                }
            }

            return path;
        }

        private SvgText MakePathText(string fId, IFeature feature, SvgColourServer color, SvgUnit size, string title, bool isHalo, string font, bool bold, bool italic)
        {
            var streetNameStr = ConstructTitleString(feature, size, title, font, bold, italic);

            var node = new SvgContentNode { Content = streetNameStr };

            var txtPath = new SvgTextPath
            {
                ReferencedPath = new Uri(string.Format("#{0}", fId), UriKind.Relative),
                //LengthAdjust = SvgTextLengthAdjust.Spacing,
                Method = SvgTextPathMethod.Stretch,
                StartOffset = new SvgUnit(0f),
                //TextLength = new SvgUnit(SvgUnitType.Millimeter, featureLen)
            };

            txtPath.CustomAttributes.Add("alignment-baseline", "middle");

            txtPath.Nodes.Add(node);

            var txt = new SvgText
            {
                Font = font,
                FontSize = size,
                Fill = color,
                //LengthAdjust = SvgTextLengthAdjust.Spacing
            };

            if (bold)
            {
                txt.FontWeight = SvgFontWeight.Bold;
            }

            if (italic)
            {
                txt.FontStyle = SvgFontStyle.Italic;
            }

            if (isHalo)
            {
                txt.CustomAttributes.Add("paint-order", "stroke");
                txt.Fill = new SvgColourServer(Color.White);
                txt.Stroke = new SvgColourServer(Color.White);
                txt.StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 0.5f);
                txt.Stroke.StrokeLineCap = SvgStrokeLineCap.Round;
                txt.Stroke.StrokeLineJoin = SvgStrokeLineJoin.Bevel;
            }

            txt.Children.Add(txtPath);

            return txt;
        }

        private string ConstructTitleString(IFeature feature, SvgUnit size, string title, string font, bool bold, bool italic)
        {
            var res = new StringBuilder();
            var featureLen = feature.ToShape().ToGeometry().Length;

            // Expand
            var lineWidth = MeasureSvgText(title + "●", size, font, bold, italic).Item2;
            var w = 0f;
            while (w <= featureLen)
            {
                res.Append(title + "●");
                w += lineWidth;
            }

            return res.ToString();
        }

        private Tuple<float, float> MeasureSvgText(string content, SvgUnit size, string font, bool bold, bool italic)
        {
            var txt = new SvgText { Font = font, FontSize = size };
            var node = new SvgContentNode { Content = content };
            txt.Nodes.Add(node);

            if (bold)
            {
                txt.FontWeight = SvgFontWeight.Bold;
            }

            if (italic)
            {
                txt.FontStyle = SvgFontStyle.Italic;
            }

            return MeasureSvgElement(txt);
        }

        private Tuple<float, float> MeasureSvgElement(SvgVisualElement svgElement)
        {
            svgDoc.Children.Add(svgElement);
            var width = svgElement.Bounds.Width;
            var height = svgElement.Bounds.Height;
            svgDoc.Children.Remove(svgElement);

            return new Tuple<float, float>(width, height);
        }

        #endregion


        #region Event handlers

        private void BtnRun_Click(object sender, EventArgs e)
        {
            Execute();
        }

        private void BtnSaveSvg_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = @"(SVG)|*.svg";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                svgDoc.Write(saveFileDialog1.FileName);
            }
        }

        #endregion
    }
}
