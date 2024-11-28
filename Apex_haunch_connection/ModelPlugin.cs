using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Tekla.Structures.Solid;
using static Tekla.Structures.Model.Position;
using Identifier = Tekla.Structures.Identifier;
using Line = Tekla.Structures.Geometry3d.Line;
using Fitting = Tekla.Structures.Model.Fitting;
using TeklaPH;


namespace Apex_haunch_connection

{
    public class PluginData
    {
        #region Fields

        [StructuresField("PlateThickness1")]
        public double PlateThickness1;

        [StructuresField("PlateThickness2")]
        public double PlateThickness2;

        [StructuresField("PlateHightTop")]
        public double PlateHightTop;

        [StructuresField("PlateHightMid")]
        public double PlateHightMid;

        [StructuresField("PlateHightBottom")]
        public double PlateHightBottom;

        [StructuresField("PlateWidth")]
        public double PlateWidth;

        [StructuresField("FlagBolt")]
        public int FlagBolt;

        [StructuresField("FlagWasher1")]
        public int FlagWasher1;

        [StructuresField("FlagWasher2")]
        public int FlagWasher2;

        [StructuresField("FlagWasher3")]
        public int FlagWasher3;

        [StructuresField("FlagNut1")]
        public int FlagNut1;

        [StructuresField("FlagNut2")]
        public int FlagNut2;

        [StructuresField("BoltSize")]
        public int BoltSize;

        [StructuresField("BoltStandard")]
        public int BoltStandard;

        [StructuresField("BoltToletance")]
        public double BoltToletance;

        [StructuresField("BoltThreadMat")]
        public int BoltThreadMat;

        [StructuresField("BA1yCount")]
        public int BA1yCount;

        [StructuresField("BA1yText")]
        public string BA1yText;

        [StructuresField("BA1xCount")]
        public int BA1xCount;

        [StructuresField("BA1xText")]
        public string BA1xText;

        [StructuresField("BA1OffsetX")]
        public double BA1OffsetX;

        [StructuresField("BA1OffsetY")]
        public double BA1OffsetY;

        [StructuresField("TopBoltOffset")]
        public double TopBoltOffset;

        [StructuresField("HaunchWebThickness")]
        public double HaunchWebThickness;

        [StructuresField("FlangeThickness")]
        public double FlangeThickness;

        [StructuresField("HaunchWidth")]
        public double HaunchWidth;

        [StructuresField("Material")]
        public string Material;

        [StructuresField("HaunchLength1")]
        public double HaunchLength1;

        [StructuresField("HaunchLength2")]
        public double HaunchLength2;

        [StructuresField("LayoutFlag")]
        public int LayoutFlag;

        #endregion
    }

    [Plugin("Apex_haunch_connection")]
    [PluginUserInterface("Apex_haunch_connection.MainForm")]
    public class Apex_haunch_connection : PluginBase
    {
        Model myModel = new Model();
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private double _PlateThickness1;
        private double _PlateThickness2;
        private double _PlateHightTop;
        private double _PlateHightMid;
        private double _PlateHightBottom;
        private double _PlateWidth;
        private int _BoltSize;
        private int _BoltStandard;
        private double _BoltToletance;
        private int _BoltThreadMat;
        private int _BA1yCount;
        private string _BA1yText;
        private int _BA1xCount;
        private string _BA1xText;
        private int _FlagBolt;
        private int _FlagWasher1;
        private int _FlagWasher2;
        private int _FlagWasher3;
        private int _FlagNut1;
        private int _FlagNut2;
        private double _BA1OffsetX;
        private double _BA1OffsetY;
        private double _TopBoltOffset;

        private double _HaunchWebThickness;
        private double _FlangeThickness;
        private double _HaunchWidth;

        private double _HaunchLength1;
        private double _HaunchLength2;

        private string _Material;

        private int _LayoutFlag;

        private List<string> _BoltStandardEnum = new List<string>
        {
            "8.8XOX",
            "4.6CSK",
            "4.6CUP",
            "4.6FIRE",
            "4.6XOX",
            "8.8CSK",
            "8.8CUP",
            "8.8FIRE",
            "8.8XOX",
            "E.B",
            "HSFG-XOX",
            "UNDEFINED_BOLT",
            "UNDEFINED_STUD"
        };

        private List<double> _BoltSizeEnum = new List<double>
        {
            10.00,
            12.00,
            16.00,
            20.00,
            24.00,
            30.00
        };

        #endregion

        #region Properties
        private Model Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data
        {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public Apex_haunch_connection(PluginData data)
        {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            List<InputDefinition> RafterList = new List<InputDefinition>();

            try
            {
                Picker Picker = new Picker();
                var rafter1 = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick the 1st rafter");
                RafterList.Add(new InputDefinition(rafter1.Identifier));

                var rafter2 = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick the 2nd rafter");
                RafterList.Add(new InputDefinition(rafter2.Identifier));
            }
            catch (Exception)
            {
                //throw;
            }

            return RafterList;
        }

        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                GetValuesFromDialog();

                Beam beam1 = myModel.SelectModelObject(Input[0].GetInput() as Identifier) as Beam;
                Beam beam2 = myModel.SelectModelObject(Input[1].GetInput() as Identifier) as Beam;


                Point origin1 = beam1.EndPoint;
                var girtCoord = beam1.GetCoordinateSystem();
                girtCoord.Origin = origin1;
                //girtCoord.AxisX = girtCoord.AxisX *- 1;

                TransformationPlane currentTransformation = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                var newWorkPlane = new TransformationPlane(girtCoord);
                // workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane);
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newWorkPlane);
                GeometricPlane geometricPlane = Fitparts(beam1 as Part, beam2 as Part, _PlateThickness1, _PlateThickness2);
                ArrayList plates = Plates(beam1, beam2, _PlateHightTop, _PlateHightMid, _PlateHightBottom, _PlateWidth, _PlateThickness1, _PlateThickness2, geometricPlane);

                ArrayList parts = new ArrayList();
                if (_HaunchLength1 != 0 && _HaunchLength2 != 0)
                    parts = Hunch(beam1, beam2, plates, _PlateHightBottom, _HaunchWebThickness, _FlangeThickness, _HaunchWidth, _HaunchLength1, _HaunchLength2);

                Assembly assembly = beam1.GetAssembly();

                assembly.Add(parts[1] as Part);
                assembly.Add(parts[0] as Part);
                assembly.Add(plates[0] as Part);
                if (!assembly.Modify())
                    Console.WriteLine("Assembly Modify Failed!");
                Assembly assembly1 = beam2.GetAssembly();
                assembly1.Add(plates[1] as Part);
                assembly1.Add(parts[2] as Part);
                assembly1.Add(parts[3] as Part);
                if (!assembly1.Modify())
                    Console.WriteLine("Assembly Modify Failed!");

                boltArray(plates, beam1, beam2);

                //workPlaneHandler.SetCurrentTransformationPlane(currentTransformation);
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTransformation);
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods

        private void GetValuesFromDialog()
        {
            _PlateThickness1 = Data.PlateThickness1;
            _PlateThickness2 = Data.PlateThickness2;
            _PlateHightTop = Data.PlateHightTop;
            _PlateHightMid = Data.PlateHightMid;
            _PlateHightBottom = Data.PlateHightBottom;
            _PlateWidth = Data.PlateWidth;
            _FlagBolt = Data.FlagBolt;
            _FlagWasher1 = Data.FlagWasher1;
            _FlagWasher2 = Data.FlagWasher2;
            _FlagWasher3 = Data.FlagWasher3;
            _FlagNut1 = Data.FlagNut1;
            _FlagNut2 = Data.FlagNut2;
            _BoltSize = Data.BoltSize;
            _BoltStandard = Data.BoltStandard;
            _BoltToletance = Data.BoltToletance;
            _BoltThreadMat = Data.BoltThreadMat;
            _BA1yCount = Data.BA1yCount;
            _BA1yText = Data.BA1yText;
            _BA1xCount = Data.BA1xCount;
            _BA1xText = Data.BA1xText;
            _BA1OffsetX = Data.BA1OffsetX;
            _BA1OffsetY = Data.BA1OffsetY;
            _TopBoltOffset = Data.TopBoltOffset;

            _FlangeThickness = Data.FlangeThickness;
            _HaunchWebThickness = Data.HaunchWebThickness;
            _HaunchWidth = Data.HaunchWidth;
            _HaunchLength1 = Data.HaunchLength1;
            _HaunchLength2 = Data.HaunchLength2;

            _Material = Data.Material;

            _LayoutFlag = Data.LayoutFlag;

            if (IsDefaultValue(_PlateThickness1))
                _PlateThickness1 = 10;
            if (IsDefaultValue(_PlateThickness2))
                _PlateThickness2 = 10;
            if (IsDefaultValue(_PlateHightTop))
                _PlateHightTop = 10;
            if (IsDefaultValue(_PlateHightMid))
                _PlateHightMid = 300;
            if (IsDefaultValue(_PlateHightBottom))
                _PlateHightBottom = 10;
            if (IsDefaultValue(_PlateWidth))
                _PlateWidth = 200;
            if (IsDefaultValue(_BoltSize))
            {
                _BoltSize = 0;
            }
            if (IsDefaultValue(_BoltStandard))
            {
                _BoltStandard = 0;
            }
            if (IsDefaultValue(_BoltThreadMat))
            {
                _BoltThreadMat = 0;
            }
            if (IsDefaultValue(_BoltToletance))
            {
                _BoltToletance = 3;
            }
            if (IsDefaultValue(_FlagBolt))
            {
                _FlagBolt = 0;
            }
            if (IsDefaultValue(_FlagWasher1))
            {
                _FlagWasher1 = 0;
            }
            if (IsDefaultValue(_FlagWasher2))
            {
                _FlagWasher2 = 1;
            }
            if (IsDefaultValue(_FlagWasher3))
            {
                _FlagWasher3 = 1;
            }
            if (IsDefaultValue(_FlagNut1))
            {
                _FlagNut1 = 0;
            }
            if (IsDefaultValue(_FlagNut2))
            {
                _FlagNut2 = 1;
            }
            if (IsDefaultValue(_BA1xCount))
            {
                _BA1xCount = 3;
            }
            if (IsDefaultValue(_BA1xText))
            {
                _BA1xText = "50";
            }
            if (IsDefaultValue(_BA1yCount))
            {
                _BA1yCount = 2;
            }
            if (IsDefaultValue(_BA1yText))
            {
                _BA1yText = "60";
            }

            if (IsDefaultValue(_BA1OffsetX))
            { _BA1OffsetX = 0; }

            if (IsDefaultValue(_BA1OffsetY))
            { _BA1OffsetY = 0; }

            if (IsDefaultValue(_TopBoltOffset))
            { _TopBoltOffset = -1; }

            if (IsDefaultValue(_FlangeThickness))
                _FlangeThickness = 10;

            if (IsDefaultValue(_HaunchWebThickness))
            { _HaunchWebThickness = 10; }
            if (IsDefaultValue(_HaunchLength1))
                _HaunchLength1 = -1;
            if (IsDefaultValue(_HaunchLength2))
                _HaunchLength2 = -1;

            if (IsDefaultValue(_HaunchWidth))
                _HaunchWidth = 150;

            if (IsDefaultValue(_Material))
                _Material = "IS2062";

            if (IsDefaultValue(_LayoutFlag))
                _LayoutFlag = 0;
        }
        public void blablabla(string s)
        {

        }
        private GeometricPlane Fitparts(Part part1, Part part2, double thickness1, double thickness2)
        {
            GeoPlane geoPlane = new GeoPlane();
            Faces _Faces = new Faces();
            List<Faces.Face_> part1Faces = Faces.Get_faces(part1);
            List<Faces.Face_> part2Faces = Faces.Get_faces(part2);
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            Point part1mid = TeklaPH.Line.MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            Point part2mid = TeklaPH.Line.MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point);

            LineSegment intersectLineSegment = Intersection.LineToLine(new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point));
            Point intersectionMidPoint = TeklaPH.Line.MidPoint(intersectLineSegment.StartPoint, intersectLineSegment.EndPoint);
            Point holdPoint1 = intersectionMidPoint, holdPoint2;
            double d1 = Distance.PointToPoint(intersectionMidPoint, part1mid),
                d2 = Distance.PointToPoint(intersectionMidPoint, part2mid);
            Point p1, p2;
            if (d1 > d2)
            {
                p2 = part2mid;

                p1 = TeklaPH.Line.FindPointOnLine(intersectionMidPoint, part1mid, d2);


            }
            else
            {
                p1 = part1mid;
                p2 = TeklaPH.Line.FindPointOnLine(intersectionMidPoint, part2mid, d1);

            }
            GeometricPlane newplain = GeoPlane.CreatePlaneFromThreePoints(intersectionMidPoint, p1, p2);
            Point mid = TeklaPH.Line.MidPoint(p1, p2);
            Point point3 = mid + newplain.GetNormal() * 50;
            GeometricPlane fittingPlain = GeoPlane.CreatePlaneFromThreePoints(intersectionMidPoint, mid, point3);
            Point point1 = intersectionMidPoint + thickness1 * fittingPlain.GetNormal();
            Point point2 = intersectionMidPoint - thickness2 * fittingPlain.GetNormal();
            var plaine = GeoPlane.ConvertGeometricPlaneToPlane(fittingPlain);
            if (_LayoutFlag == 0)
            {
                GeometricPlane planeA1 = Faces.ConvertFaceToGeometricPlane(part1Faces[5].Face),
              planeA2 = Faces.ConvertFaceToGeometricPlane(part1Faces[11].Face),
              planeB1 = Faces.ConvertFaceToGeometricPlane(part2Faces[5].Face),
              planeB2 = Faces.ConvertFaceToGeometricPlane(part2Faces[11].Face);


                Line line1 = Intersection.PlaneToPlane(planeA1, planeB1),
                line2 = Intersection.PlaneToPlane(planeA2, planeB1),
                line3 = Intersection.PlaneToPlane(planeA1, planeB2),
                line4 = Intersection.PlaneToPlane(planeA2, planeB2);
                Line holdLine = null;
                double d = -1;
                foreach (var line in new List<Line> { line1, line2, line3, line4 })
                {
                    if (d < Distance.PointToLine(mid, line))
                    {
                        d = Distance.PointToLine(mid, line);
                        holdLine = line;
                    }
                }
                holdPoint1 = Intersection.LineToPlane(holdLine, newplain);
                holdPoint2 = Projection.PointToPlane(holdPoint1, fittingPlain);
                Line  l1 = new Line(holdPoint2, holdPoint1);
                Vector vector = new Vector(l1.Direction);
                point1 = point1 + vector.GetNormal() * Distance.PointToPoint(holdPoint1, holdPoint2);
                point2 = point2 + vector.GetNormal() * Distance.PointToPoint(holdPoint1, holdPoint2);
            }

            Fitting fitting1 = new Fitting();
            fitting1.Plane.AxisX = plaine.AxisX;
            fitting1.Plane.AxisY = plaine.AxisY;
            fitting1.Father = part1;


            Fitting fitting2 = new Fitting();
            fitting2.Plane.AxisX = plaine.AxisX;
            fitting2.Plane.AxisY = plaine.AxisY;
            fitting2.Father = part2;
            if (Distance.PointToPoint(point1, part1mid) < Distance.PointToPoint(point2, part1mid))
            {
                fitting1.Plane.Origin = point1;
                fitting2.Plane.Origin = point2;
            }
            else
            {
                fitting1.Plane.Origin = point2;
                fitting2.Plane.Origin = point1;
            }
            fitting1.Insert();
            fitting2.Insert();

            return new GeometricPlane(holdPoint1, fittingPlain.GetNormal());
        }
        private ArrayList Plates(Part part1, Part part2, double topHight, double middleHight, double bottomHight, double width, double thickness1, double thickness2, GeometricPlane geometricPlane)
        {
            Faces _Faces = new Faces();
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            List<Faces.Face_> part1Faces =Faces.Get_faces(part1),
                part2Faces = Faces.Get_faces(part2);
            Point p1 = Intersection.LineToPlane(new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), geometricPlane),
                p2 = Intersection.LineToPlane(new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point), geometricPlane);
            Point intersectionMidPoint = TeklaPH.Line.MidPoint(p1, p2);
            GeometricPlane gp = new GeometricPlane(intersectionMidPoint, part1Faces[2].Vector);
            Line line = Intersection.PlaneToPlane(gp, geometricPlane);
            Point refference = Projection.PointToLine(TeklaPH.Line.MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), line);

            GeometricPlane planeA1 = Faces.ConvertFaceToGeometricPlane(part1Faces[5].Face),
               planeA2 = Faces.ConvertFaceToGeometricPlane(part1Faces[11].Face),
               planeB1 = Faces.ConvertFaceToGeometricPlane(part2Faces[5].Face),
               planeB2 = Faces.ConvertFaceToGeometricPlane(part2Faces[11].Face),
               g1 = Faces.ConvertFaceToGeometricPlane(part1Faces[0].Face),
               g2 = Faces.ConvertFaceToGeometricPlane(part1Faces[10].Face);


            Line line1 = Intersection.PlaneToPlane(planeA1, geometricPlane),
            line2 = Intersection.PlaneToPlane(planeA2, geometricPlane),
            line3 = Intersection.PlaneToPlane(planeB1, geometricPlane),
            line4 = Intersection.PlaneToPlane(planeB2, geometricPlane);
            Point top = Projection.PointToLine(refference, new Line(Intersection.LineToPlane(line1, g1), Intersection.LineToPlane(line1, g2)));
            double distance = Distance.PointToLine(refference, line1);

            foreach (Line l in new List<Line> { line2, line3, line4 })
            {


                if (Distance.PointToLine(refference, l) > distance)
                {
                    top = Projection.PointToLine(refference, new Line(Intersection.LineToPlane(l, g1), Intersection.LineToPlane(l, g2)));
                    distance = Distance.PointToLine(refference, l);
                }
            }

            Vector vector = geometricPlane.GetNormal();

            Point startPoint = TeklaPH.Line.FindPointOnLine(top, refference, topHight * -1);
            double totalBottomdistance = middleHight + bottomHight;
            Point endPoint = TeklaPH.Line.FindPointOnLine(intersectionMidPoint, refference, totalBottomdistance);
            Beam beam1 = new Beam();
            beam1.StartPoint = startPoint;
            beam1.EndPoint = endPoint;
            beam1.Profile.ProfileString = "PLT" + thickness1 + "*" + width;
            beam1.Position.Depth = Position.DepthEnum.MIDDLE;
            beam1.Position.Plane = ((vector.X < 0 && vector.Y > 0) || (vector.X > 0 && vector.Y < 0)) ?Position.PlaneEnum.RIGHT: Position.PlaneEnum.LEFT;
            beam1.Position.Rotation = Position.RotationEnum.TOP;
            beam1.Material.MaterialString = _Material;
            beam1.Class = "1";
            beam1.Insert();
            Beam beam2 = new Beam();
            beam2.StartPoint = startPoint;
            beam2.EndPoint = endPoint;
            beam2.Profile.ProfileString = "PLT" + thickness2 + "*" + width;
            beam2.Position.Depth = Position.DepthEnum.MIDDLE;
            beam2.Position.Plane =((vector.X < 0 && vector.Y > 0) || (vector.X > 0 && vector.Y < 0)) ? Position.PlaneEnum.LEFT : Position.PlaneEnum.RIGHT;
            beam2.Position.Rotation = Position.RotationEnum.TOP;
            beam2.Material.MaterialString = _Material;
            beam2.Class = "1";
            beam2.Insert();
            return new ArrayList { beam1, beam2 };



        }
        private void boltArray(ArrayList parts, Part beam1, Part beam2)
        {
            Faces _Faces = new Faces();
            BoltArray bA = new BoltArray();
            bA.PartToBeBolted = parts[0] as Beam;
            bA.PartToBoltTo = parts[1] as Beam;

            ArrayList beam1Centerline = beam1.GetCenterLine(false),
                beam2Centerline = beam2.GetCenterLine(false);
            LineSegment intersection_CenterLine = Intersection.LineToLine(new Line(beam1Centerline[0] as Point, beam1Centerline[1] as Point), new Line(beam2Centerline[0] as Point, beam2Centerline[1] as Point));

            List<Faces.Face_> face_s = Faces.Get_faces(parts[0] as Beam);
            List<Faces.Face_> cp_faces = face_s.OrderByDescending(fa => Faces.CalculateFaceArea(fa)).ToList();

            bA.BoltSize = _BoltSizeEnum[_BoltSize];
            bA.Tolerance = _BoltToletance;
            bA.BoltStandard = _BoltStandardEnum[_BoltStandard];
            bA.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;

            bA.ThreadInMaterial = (_BoltThreadMat == 0) ? BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES : BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

            bA.Position.Depth = Position.DepthEnum.MIDDLE;
            bA.Position.Plane = Position.PlaneEnum.MIDDLE;
            Vector vector = cp_faces[0].Vector;
            bA.Position.Rotation = (vector.X == 1 || vector.X == -1 || vector.Z == 1) ? RotationEnum.FRONT : RotationEnum.TOP;

            bA.Bolt = (_FlagBolt == 0) ? true : false;
            bA.Washer1 = (_FlagWasher1 == 0) ? true : false;
            bA.Washer2 = (_FlagWasher2 == 0) ? true : false;
            bA.Washer3 = (_FlagWasher3 == 0) ? true : false;
            bA.Nut1 = (_FlagNut1 == 0) ? true : false;
            bA.Nut2 = (_FlagNut2 == 0) ? true : false;


            double total = 0;
            List<double> doubles = Input.InputConverter(_BA1xText);
            bool flag = false;
            double hold = 0;

            if (doubles == null)
                bA.AddBoltDistX(0);
            if (_BA1xCount > 0 && doubles != null)
            {
                if (doubles[0] != 0)
                    bA.AddBoltDistX(0);
                if (doubles.Count == 1)
                    flag = true;
                for (int i = 0; i < _BA1xCount - 1; i++)
                {
                    if (i == doubles.Count - 1)
                    {
                        hold = doubles[i];
                    }
                    if (i >= doubles.Count)
                    {
                        bA.AddBoltDistX(hold);
                        total += hold;
                    }
                    else
                    {
                        bA.AddBoltDistX((flag) ? doubles[0] : doubles[i]);
                        total += (flag) ? doubles[0] : doubles[i];

                    }
                }
            }
            bA.StartPointOffset.Dx = 0;
            if (doubles != null)
                doubles.Clear();
            doubles = Input.InputConverter(_BA1yText);

            if (doubles == null)
                bA.AddBoltDistY(0);
            if (_BA1yCount > 0 && doubles != null)
            {
                if (doubles[0] != 0)
                    bA.AddBoltDistY(0);
                if (doubles.Count == 1)
                    flag = true;
                for (int i = 0; i < _BA1yCount - 1; i++)
                {
                    if (i == doubles.Count - 1)
                    {
                        hold = doubles[i];
                    }
                    if (i >= doubles.Count)
                    {
                        bA.AddBoltDistY(hold);

                    }
                    else
                    {
                        bA.AddBoltDistY((flag) ? doubles[0] : doubles[i]);
                    }
                }
            }

            bA.StartPointOffset.Dz = _BA1OffsetY;
            bA.EndPointOffset.Dz = _BA1OffsetY;

            GeometricPlane gp1 = Faces.ConvertFaceToGeometricPlane(cp_faces[0].Face),
               gp2 = Faces.ConvertFaceToGeometricPlane(cp_faces[1].Face);
            GeometricPlane geometricPlane = new GeometricPlane();
            if (Distance.PointToPlane(intersection_CenterLine.StartPoint, gp1) > Distance.PointToPlane(intersection_CenterLine.StartPoint, gp2))
                geometricPlane = gp1;
            else
                geometricPlane = gp2;
            Point mid = TeklaPH.Line.MidPoint(intersection_CenterLine.StartPoint, intersection_CenterLine.EndPoint);
            Beam beam = parts[0] as Beam;
            Point point1 = TeklaPH.Line.FindPointOnLine(mid, beam.StartPoint, total / 2 + _BA1OffsetX);
            if (_TopBoltOffset > 0)
                point1 = TeklaPH.Line.FindPointOnLine(beam.StartPoint, beam.EndPoint, _TopBoltOffset);
            bA.FirstPosition = Projection.PointToPlane(point1, geometricPlane);
            bA.SecondPosition = Projection.PointToPlane(beam.EndPoint, geometricPlane);
            bA.Insert();
        }
        private ArrayList Hunch(Part part1, Part part2, ArrayList plates, double bottom_length, double webThickness, double flangeThickness, double width, double length1, double length2)
        {
            
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            List<Faces.Face_> part1Faces = Faces.Get_faces(part1),
               part2Faces = Faces.Get_faces(part2);
            Beam plate1 = plates[0] as Beam,
                plate2 = plates[1] as Beam;

            List<Faces.Face_> face_s = Faces.Get_faces(plates[0] as Beam);
            List<Faces.Face_> plate1_faces = face_s.OrderByDescending(fa => Faces.CalculateFaceArea(fa)).ToList();
            face_s = Faces.Get_faces(plates[1] as Beam);
            List<Faces.Face_> plate2_faces = face_s.OrderByDescending(fa => Faces.CalculateFaceArea(fa)).ToList();
            GeometricPlane plate1Closest = null, plate2Closest = null;

            GeometricPlane plA1 = Faces.ConvertFaceToGeometricPlane(plate1_faces[0].Face),
                plA2 = Faces.ConvertFaceToGeometricPlane(plate1_faces[1].Face),
                plB1 = Faces.ConvertFaceToGeometricPlane(plate2_faces[0].Face),
                plB2 = Faces.ConvertFaceToGeometricPlane(plate2_faces[1].Face);
            double d = 0;
            foreach (GeometricPlane gp in new List<GeometricPlane> { plA1, plA2, plB1, plB2 })
            {
                if (d < Distance.PointToPlane(TeklaPH.Line.MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), gp))
                {
                    d = Distance.PointToPlane(TeklaPH.Line.MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), gp);
                    plate2Closest = gp;
                }
            }
            d = 0;
            foreach (GeometricPlane gp in new List<GeometricPlane> { plA1, plA2, plB1, plB2 })
            {
                if (d < Distance.PointToPlane(TeklaPH.Line.MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point), gp))
                {
                    d = Distance.PointToPlane(TeklaPH.Line.MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point), gp);
                    plate1Closest = gp;
                }
            }

            double hight1 = 0, hight2 = 0;


            GeometricPlane part1FaceColsest = null, part2FaceClosest = null;
            if (Distance.PointToPlane(plate1.EndPoint, Faces.ConvertFaceToGeometricPlane(part1Faces[5].Face)) < Distance.PointToPlane(plate1.EndPoint, Faces.ConvertFaceToGeometricPlane(part1Faces[11].Face)))
                part1FaceColsest = Faces.ConvertFaceToGeometricPlane(part1Faces[5].Face);
            else
                part1FaceColsest = Faces.ConvertFaceToGeometricPlane(part1Faces[11].Face);

            if (Distance.PointToPlane(plate1.EndPoint, Faces.ConvertFaceToGeometricPlane(part2Faces[5].Face)) < Distance.PointToPlane(plate1.EndPoint, Faces.ConvertFaceToGeometricPlane(part2Faces[11].Face)))
                part2FaceClosest = Faces.ConvertFaceToGeometricPlane(part2Faces[5].Face);
            else
                part2FaceClosest = Faces.ConvertFaceToGeometricPlane(part2Faces[11].Face);

            Point holdStart = Projection.PointToPlane(plate1.StartPoint, plate1Closest), holdEnd = Projection.PointToPlane(plate1.EndPoint, plate1Closest);
            Point pA1 = Intersection.LineToPlane(new Line(holdStart, holdEnd), part1FaceColsest);
            Point pA2 = TeklaPH.Line.FindPointOnLine(holdEnd, holdStart, bottom_length + flangeThickness);
            if (length1 >= 0)
            {
                Line line = new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point);
                double angle = FindShortestAngleBetweenLines(new Line(Intersection.LineToPlane(line, plate1Closest), plate1Closest.GetNormal()), line);
                hight1 = CalculateOtherSideUsingTan(angle, length1);
                pA2 = TeklaPH.Line.FindPointOnLine(pA1, pA2, hight1);
            }
            Line holdLine = new Line(pA2, plate1Closest.GetNormal());
            Point pA3 = Intersection.LineToPlane(holdLine, part1FaceColsest);

            ContourPlate cp1 = new ContourPlate();
            ArrayList countourPoints = new ArrayList();
            GeometricPlane part1CenterPlain = new GeometricPlane(part1_centerLine[0] as Point, part1Faces[2].Vector); 
            foreach (Point point in new List<Point> { pA1, pA2, pA3 })
            {
                Point p = Projection.PointToPlane(point, part1CenterPlain);
                ContourPoint contourPoint = new ContourPoint(p, new Chamfer(10, 10, Chamfer.ChamferTypeEnum.CHAMFER_LINE));
                countourPoints.Add(contourPoint);
            }

            cp1.Contour.ContourPoints = countourPoints;

            cp1.Profile.ProfileString = "PLT" + webThickness;

            cp1.Material.MaterialString = "IS2062";
            cp1.Class = "4";
            cp1.Position.Depth = Position.DepthEnum.MIDDLE;
            cp1.Insert();

            holdStart = Projection.PointToPlane(plate2.StartPoint, plate2Closest); holdEnd = Projection.PointToPlane(plate2.EndPoint, plate2Closest);
            Point pB1 = Intersection.LineToPlane(new Line(holdStart, holdEnd), part2FaceClosest);
            Point pB2 = TeklaPH.Line.FindPointOnLine(holdEnd, holdStart, bottom_length + flangeThickness);
            if (length2 >= 0)
            {
                Line line = new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point);
                double angle = FindShortestAngleBetweenLines(new Line(Intersection.LineToPlane(line, plate2Closest), plate2Closest.GetNormal()), line);
                hight2 = CalculateOtherSideUsingTan(angle, length2);
                pB2 = TeklaPH.Line.FindPointOnLine(pB1, pB2, hight2);
            }
            holdLine = new Line(pB2, plate2Closest.GetNormal());
            Point pB3 = Intersection.LineToPlane(holdLine, part2FaceClosest);

            ContourPlate cp2 = new ContourPlate();
            ArrayList countourPoints1 = new ArrayList();
            GeometricPlane part2CenterPlain = new GeometricPlane(part2_centerLine[0] as Point, part2Faces[2].Vector);
            foreach (Point point in new List<Point> { pB1, pB2, pB3 })
            {
                Point p = Projection.PointToPlane(point, part2CenterPlain);
                ContourPoint contourPoint = new ContourPoint(p, new Chamfer(10, 10, Chamfer.ChamferTypeEnum.CHAMFER_LINE));
                countourPoints1.Add(contourPoint);
            }

            cp2.Contour.ContourPoints = countourPoints1;

            cp2.Profile.ProfileString = "PLT" + webThickness;

            cp2.Material.MaterialString = "IS2062";
            cp2.Class = "4";
            cp2.Position.Depth = Position.DepthEnum.MIDDLE;
            cp2.Insert();

            Beam flange1 = new Beam();
            flange1.StartPoint = pA2;
            flange1.EndPoint = pA3;
            Vector vector1 = new Line(pA2, pA3).Direction;
            vector1.Normalize();
            flange1.Profile.ProfileString = "PLT" + flangeThickness + "*" + width;
            flange1.Position.Depth = Position.DepthEnum.MIDDLE;
            flange1.Position.Plane = (vector1.X > 0 && vector1.Y > 0) ? PlaneEnum.RIGHT : PlaneEnum.LEFT;
            flange1.Position.Rotation = Position.RotationEnum.TOP;
            flange1.Material.MaterialString = _Material;
            flange1.Class = "1";
            flange1.Insert();

            Beam flange2 = new Beam();
            flange2.StartPoint = pB2;
            flange2.EndPoint = pB3;
            Vector vector2 = new Line(pB2, pB3).Direction;
            vector2.Normalize();
            flange2.Profile.ProfileString = "PLT" + flangeThickness + "*" + width;
            flange2.Position.Depth = Position.DepthEnum.MIDDLE;
            flange2.Position.Plane = (vector1.X > 0 && vector1.Y > 0) ? PlaneEnum.LEFT : PlaneEnum.RIGHT;
            flange2.Position.Rotation = Position.RotationEnum.TOP;
            flange2.Material.MaterialString = _Material;
            flange2.Class = "1";
            flange2.Insert();

            WeldArray(cp1, new ArrayList { part1, plates[0], flange1 });
            WeldArray(cp2, new ArrayList { part2, plates[1], flange2 });
            return new ArrayList { cp1, flange1, cp2, flange2 };
        }
        private void WeldArray(Part part1, ArrayList arrayList)
        {
            foreach (Part part2 in arrayList)
            {
                Weld Weld = new Weld();
                Weld.MainObject = part1;
                Weld.SecondaryObject = part2;
                Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.SizeAbove = 5;
                Weld.SizeBelow = 5;

                Weld.LengthAbove = 12;
                Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.Insert();


            }
            Weld Weld1 = new Weld();
            Weld1.MainObject = arrayList[0] as Part;
            Weld1.SecondaryObject = arrayList[1] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

            Weld1 = new Weld();
            Weld1.MainObject = arrayList[2] as Part;
            Weld1.SecondaryObject = arrayList[1] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

            Weld1 = new Weld();
            Weld1.MainObject = arrayList[0] as Part;
            Weld1.SecondaryObject = arrayList[2] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

        }
        public static double FindShortestAngleBetweenLines(Line line1, Line line2)
        {
            // Get the direction vectors of the lines
            Vector v1 = line1.Direction;
            Vector v2 = line2.Direction;

            // Calculate the dot product between the vectors
            double dotProduct = v1.Dot(v2);

            // Calculate the magnitudes of the vectors
            double magnitude1 = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            double magnitude2 = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);

            // Calculate the cosine of the angle
            double cosTheta = dotProduct / (magnitude1 * magnitude2);

            // Find the angle in radians (shortest angle)
            double angleInRadians = Math.Acos(cosTheta);

            // Convert to degrees for better understanding
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            return angleInDegrees;
        }
        public static double CalculateOtherSideUsingTan(double angleInDegrees, double knownSide)
        {
            if (angleInDegrees > 90)
                angleInDegrees = 180 - angleInDegrees;
            // Convert the angle to radians
            double angleInRadians = angleInDegrees * (Math.PI / 180.0);

            // Use the tangent to calculate the unknown side (opposite or adjacent)
            double otherSide = Math.Tan(angleInRadians) * knownSide;

            return otherSide;
        }
        #endregion
    }
}
