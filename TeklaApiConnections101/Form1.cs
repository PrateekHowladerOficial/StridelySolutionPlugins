using System;
using System.Collections;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using static Tekla.Structures.Model.Position;
using Color = Tekla.Structures.Model.UI.Color;
using System.Collections.Generic;
using TeklaPH;
using Line = Tekla.Structures.Geometry3d.Line;
using Fitting = Tekla.Structures.Model.Fitting;

namespace TeklaApiConnections101
{
    public partial class Form1 : Form
    {
        //  GLOBAL VARIABLES
        #region GLOBAL VARIABLES
        Model myModel = new Model();
        Picker _picker = new Picker();
        private Position.DepthEnum depthEnum;
        private Position.PlaneEnum planeEnum;
        private Position.RotationEnum rotationEnum;
        Beam myColumn = null;
        Beam mybeam = null;
        string colProfile;
        string colMaterial;
        string colClass;
        string beamProfile;
        string beamMaterial;
        string beamClass;
        string platethickness;
        int weldSize;
        const double inch = 25.4;
        double plateLength = 6 * inch;  //  plate length will be 6 * inch >> 6 * 25.4   ==    152.4
        double plateThickness = 0.5 * inch;   //  plate thickness will be 0.5 * inch >> 0.5 * 25.4  ==  12.7
        double beamDepth = 0.0;
        double boltSize = 12.7;
        double tolerance = 2.00;
        string boltStd = "A325N";
        double cutLength = 80;
        double length = 80;
        double extraLength = 0;
        private static ArrayList ObjectListw = new ArrayList(); // weld
        private static ArrayList ObjectList = new ArrayList(); // girt conn
        ControlPoint cp = new ControlPoint(new Point(0, 0, 0));//cp.Insert();
        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!myModel.GetConnectionStatus())
            {
                MessageBox.Show("Tekla is NOT Connected");
                return;
            }
            else
            {
                MessageBox.Show("Tekla is Connected");
                return;
            }
        }

        //  END PLATE
        private void btn_EndPlate_Click(object sender, EventArgs e)
        {
            #region Pick 2 Parts
            ModelObject colObj = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick primary part");
            ModelObject beamObj = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick secondary part");
            #endregion

            Beam beam = beamObj as Beam;
            Beam column = colObj as Beam;

            #region CoordinateSystem
            var beamPart = beam as Beam;
            Point origin11;
            if (Distance.PointToPoint(beam.StartPoint, column.EndPoint) < Distance.PointToPoint(beam.EndPoint, column.EndPoint))
            {
                origin11 = beam.StartPoint;
                try
                {
                    if (origin11 == null)
                    {
                        MessageBox.Show("No point selected. Coordinate System creation aborted");
                        return;
                    }

                    var beamCoord = (beam as Part).GetCoordinateSystem();
                    var workPlaneHandler = new Model().GetWorkPlaneHandler();
                    var newWorkPlane = new TransformationPlane(beamCoord);

                    new Model().CommitChanges();

                    if (beamCoord == null)
                        throw new ArgumentNullException(nameof(beamCoord));

                    var gd = new GraphicsDrawer();

                    var origin = beamCoord.Origin;
                    var xPoint = origin + 400 * beamCoord.AxisX.GetNormal();
                    var yPoint = origin + 400 * beamCoord.AxisX.Cross(beamCoord.AxisY).GetNormal();
                    var zPoint = origin + 400 * beamCoord.AxisY.GetNormal();

                    var xColor = new Color(1, 0, 0);
                    var yColor = new Color(0, 1, 0);
                    var zColor = new Color(0, 0, 1);

                    gd.DrawLineSegment(origin, xPoint, xColor);
                    gd.DrawText(xPoint, "X", xColor);
                    gd.DrawLineSegment(origin, yPoint, yColor);
                    gd.DrawText(yPoint, "Y", yColor);
                    gd.DrawLineSegment(origin, zPoint, zColor);
                    gd.DrawText(zPoint, "Z", zColor);

                    if (!string.IsNullOrWhiteSpace("ORIGIN"))
                        gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                    if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                    {
                        MessageBox.Show("Coordinate System failed to insert");
                        myModel.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred");
                }
                #endregion

                #region CUT
                CutPlane cutPlane = new CutPlane();
                Beam beamStart = beam as Beam;
                cutPlane.Plane = new Plane();
                cutPlane.Plane.Origin = new Point(mm2Inch(0.5), 0, 0);
                cutPlane.Plane.AxisX = new Vector(0, mm2Inch(40), 0);
                cutPlane.Plane.AxisY = new Vector(0, 0, mm2Inch(-40));
                cutPlane.Father = beamPart;
                cutPlane.Insert();
                #endregion

                #region PLATE
                Point plStartPt = new Point(0, mm2Inch(8), 0);
                Point plEndPoint = new Point(0, mm2Inch(-8), 0);
                Beam vPlate = new Beam(plStartPt, plEndPoint);
                vPlate.Profile.ProfileString = "PL 177.8*12.7";
                double plateThickness = mm2Inch(0.5); ; //  plate
                vPlate.Material.MaterialString = "43A";
                vPlate.Class = "1";
                vPlate.Position.Plane = PlaneEnum.LEFT;
                vPlate.Position.Rotation = RotationEnum.TOP;
                vPlate.Position.Depth = DepthEnum.MIDDLE;
                vPlate.Insert();
                #endregion

                #region WELD
                Weld weld = new Weld();
                ObjectListw.Add(beamPart);
                ObjectListw.Add(vPlate);
                weld.Delete();
                weld.MainObject = beamPart;
                weld.SecondaryObject = vPlate;
                weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld.SizeAbove = 10;
                weld.SizeBelow = 10;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.AroundWeld = false;
                weld.ShopWeld = false;
                if (!weld.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region BoltArray
                Part primaryCol = column as Part;
                Beam beam_Part = beam as Beam;
                //Point beamSrtPt = beam_Part.StartPoint;
                Part secondaryPlate = vPlate;
                BoltArray bArr = new BoltArray();
                bArr.Delete();
                bArr.PartToBoltTo = primaryCol;
                bArr.PartToBeBolted = secondaryPlate;
                bArr.FirstPosition = vPlate.StartPoint;
                bArr.SecondPosition = vPlate.EndPoint;
                bArr.BoltSize = mm2Inch(1);
                bArr.Tolerance = 2.00;
                bArr.BoltStandard = "A325N";
                bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr.Length = mm2Inch(3.15);
                bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr.Position.Rotation = Position.RotationEnum.BELOW;

                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.StartPointOffset.Dx = mm2Inch(2.5);
                bArr.AddBoltDistY(mm2Inch(3.5));
                if (!bArr.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
            else if (Distance.PointToPoint(beam.StartPoint, column.EndPoint) > Distance.PointToPoint(beam.EndPoint, column.EndPoint))
            {
                #region CoordinateSystem
                origin11 = beam.EndPoint;
                //ControlPoint CP = new ControlPoint(origin11);
                //CP.Insert();

                try
                {
                    if (origin11 == null)
                    {
                        MessageBox.Show("No point selected. Coordinate System creation aborted");
                        return;
                    }

                    var beamCoord = (beam as Part).GetCoordinateSystem();
                    var workPlaneHandler = new Model().GetWorkPlaneHandler();
                    beamCoord.Origin = new Point(beam.EndPoint);
                    beamCoord.AxisX *= -1;
                    var newWorkPlane = new TransformationPlane(beamCoord);

                    new Model().CommitChanges();

                    if (beamCoord == null)
                        throw new ArgumentNullException(nameof(beamCoord));

                    var gd = new GraphicsDrawer();

                    var origin = beam.EndPoint as Point;
                    Point xPoint = origin + 400 * beamCoord.AxisX.GetNormal();

                    var yPoint = origin + 400 * beamCoord.AxisY.Cross(beamCoord.AxisX).GetNormal();

                    var zPoint = origin + 400 * beamCoord.AxisY.GetNormal();

                    var xColor = new Color(1, 0, 0);
                    var yColor = new Color(0, 1, 0);
                    var zColor = new Color(0, 0, 1);

                    gd.DrawLineSegment(origin, xPoint, xColor);
                    gd.DrawText(xPoint, "X", xColor);
                    gd.DrawLineSegment(origin, yPoint, yColor);
                    gd.DrawText(yPoint, "Z", yColor);
                    gd.DrawLineSegment(origin, zPoint, zColor);
                    gd.DrawText(zPoint, "Y", zColor);

                    if (!string.IsNullOrWhiteSpace("ORIGIN"))
                        gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                    if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                    {
                        MessageBox.Show("Coordinate System failed to insert");
                        myModel.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred");
                }

                //ControlPoint cp = new ControlPoint(new Point(origin11));
                //cp.Insert();
                #endregion

                #region CUT
                CutPlane cutPlane = new CutPlane();
                Beam beamStart = beam as Beam;
                cutPlane.Plane = new Plane();
                cutPlane.Plane.Origin = new Point(mm2Inch(0.5), 0, 0);
                cutPlane.Plane.AxisX = new Vector(0, mm2Inch(40), 0);
                cutPlane.Plane.AxisY = new Vector(0, 0, mm2Inch(-40));
                cutPlane.Father = beam;
                cutPlane.Insert();
                #endregion

                #region PLATE
                Point plStartPt = new Point(0, 0, 0);
                Point plEndPoint = new Point(0, mm2Inch(-16), 0);
                Beam vPlate = new Beam(plStartPt, plEndPoint);
                vPlate.Profile.ProfileString = "PL 177.8*12.7";
                double plateThickness = mm2Inch(0.5); ; //  plate
                vPlate.Material.MaterialString = "43A";
                vPlate.Class = "1";
                vPlate.Position.Plane = PlaneEnum.LEFT;
                vPlate.Position.Rotation = RotationEnum.TOP;
                vPlate.Position.Depth = DepthEnum.MIDDLE;
                vPlate.Insert();
                #endregion

                #region WELD
                Weld weld = new Weld();
                ObjectListw.Add(beamPart);
                ObjectListw.Add(vPlate);
                weld.Delete();
                weld.MainObject = beamPart;
                weld.SecondaryObject = vPlate;
                weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld.SizeAbove = 10;
                weld.SizeBelow = 10;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld.AroundWeld = false;
                weld.ShopWeld = false;
                if (!weld.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region BoltArray
                Part primaryCol = column as Part;
                Beam beam_Part = beam as Beam;
                Point beamSrtPt = beam_Part.EndPoint;
                Part secondaryPlate = vPlate;
                BoltArray bArr = new BoltArray();
                bArr.Delete();
                bArr.PartToBoltTo = primaryCol;
                bArr.PartToBeBolted = secondaryPlate;
                //bArr.FirstPosition = origin11;
                bArr.FirstPosition = vPlate.StartPoint;
                bArr.SecondPosition = vPlate.EndPoint;
                bArr.BoltSize = mm2Inch(1);
                bArr.Tolerance = 2.00;
                bArr.BoltStandard = "A325N";
                bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr.Length = mm2Inch(3.15);
                bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr.Position.Rotation = Position.RotationEnum.BELOW;

                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.AddBoltDistX(mm2Inch(3.5));
                bArr.StartPointOffset.Dx = mm2Inch(2.5);
                bArr.AddBoltDistY(mm2Inch(3.5));
                //bArr.Insert();
                if (!bArr.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
            SetGlobalandStoreOriginal();
            myModel.CommitChanges();
        }

        //  SINGLE GIRT
        private void btn_SingleGirt_Click(object sender, EventArgs e)
        {
            #region Pick Parts
            ModelObject columnObject = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Primary Part");
            ModelObject girtObject = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Secondary Part");
            #endregion

            #region Coordinate System & WorkPlane
            var girt = girtObject as Beam;
            var column = columnObject as Beam;

            Point origin1;

            if (Distance.PointToPoint(girt.StartPoint, column.EndPoint) < Distance.PointToPoint(girt.EndPoint, column.EndPoint))
            {
                origin1 = girt.StartPoint;
                try
                {
                    if (origin1 == null)
                    {
                        MessageBox.Show("No point selected for origin, cannot create Coordinate System.");
                        return;
                    }
                    var girtCoord = girt.GetCoordinateSystem();
                    girtCoord.Origin = origin1;
                    var workPlaneHandler = new Model().GetWorkPlaneHandler();
                    var newWorkPlane = new TransformationPlane(girtCoord);

                    new Model().CommitChanges();

                    if (girtCoord == null)
                        throw new ArgumentNullException(nameof(girtCoord));

                    var gd = new GraphicsDrawer();
                    var origin = girtCoord.Origin;

                    var xPoint = origin + 400 * girtCoord.AxisX.GetNormal();
                    var yPoint = origin + 400 * girtCoord.AxisY.GetNormal();
                    var zPoint = origin + 400 * girtCoord.AxisX.Cross(girtCoord.AxisY).GetNormal();

                    var xColor = new Color(1, 0, 0);
                    var yColor = new Color(0, 1, 0);
                    var zColor = new Color(0, 0, 1);

                    gd.DrawLineSegment(origin, xPoint, xColor);
                    gd.DrawText(xPoint, "X", xColor);
                    gd.DrawLineSegment(origin, yPoint, yColor);
                    gd.DrawText(yPoint, "Y", yColor);
                    gd.DrawLineSegment(origin, zPoint, zColor);
                    gd.DrawText(zPoint, "Z", zColor);

                    if (!string.IsNullOrWhiteSpace("ORIGIN"))
                        gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                    if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                    {
                        MessageBox.Show("Coordinate System failed to insert");
                        myModel.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred.");
                }
                #endregion
                //cp.Insert();

                List<Faces.Face_> faces_ = Faces.Get_faces(column, true);   // faces retireved
                List<GeometricPlane> colFlangeSurface = SurfaceFinder.GetFlangeOutterSurfacePlane(column); // plane of outer flange

                // check to see intersection 
                var girtCenterLine = girt.GetCenterLine(false);
                Point p1 = Intersection.LineToPlane(new Line(girtCenterLine[0] as Point, girtCenterLine[1] as Point), colFlangeSurface[0]);
                Point p2 = Intersection.LineToPlane(new Line(girtCenterLine[0] as Point, girtCenterLine[1] as Point), colFlangeSurface[1]);
                GeometricPlane gp = new GeometricPlane();

                if (p1 != null)
                {
                    Point midPoint = TeklaPH.Line.MidPoint(girtCenterLine[0] as Point, girtCenterLine[1] as Point);
                    if (Distance.PointToPoint(midPoint, p1) > Distance.PointToPoint(midPoint, p2))
                        gp = colFlangeSurface[1];
                    else
                        gp = colFlangeSurface[0];

                    Point originPt = gp.Origin - gp.GetNormal() * mm2Inch(4);

                    GeometricPlane newGeoPlane = new GeometricPlane(originPt, gp.GetNormal());

                    Plane plane = GeoPlane.ConvertGeometricPlaneToPlane(newGeoPlane);

                    Fitting fit = new Fitting();
                    fit.Plane = plane;
                    fit.Father = girt;
                    fit.Insert();
                }
                else

                    TeklaPH.Fitting.Web_Fitting(column, girt, mm2Inch(4));
                #region Plate
                Point plStartPoint = new Point(mm2Inch(0.125), 0, mm2Inch(-0.125));
                Point plEndPoint = new Point(mm2Inch(7), 0, mm2Inch(-0.125));
                Beam cPlate = new Beam(plStartPoint, plEndPoint);
                string plateProfile = "PL";
                string plateWidth = "76.2";     // 3 inches
                string plateThickness = "12.7"; // 1/2 inch
                cPlate.Profile.ProfileString = $"{plateProfile} {plateWidth}*{plateThickness}";
                cPlate.Material.MaterialString = "A153";
                cPlate.Class = "4";
                cPlate.Position.Plane = Position.PlaneEnum.MIDDLE;
                cPlate.Position.Rotation = Position.RotationEnum.FRONT;
                cPlate.Insert();
                #endregion

                #region Weld
                Weld weld1 = new Weld();
                Weld weld2 = new Weld();
                ObjectList.Add(girt);
                ObjectList.Add(cPlate);
                ObjectList.Add(column);
                weld1.Delete();
                weld1.MainObject = column;
                weld1.SecondaryObject = cPlate;
                weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.SizeAbove = mm2Inch(0.5);
                weld1.SizeBelow = mm2Inch(0.5);
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld1.AroundWeld = false;
                weld1.ShopWeld = false;
                if (!weld1.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }

                weld2.Delete();
                weld2.MainObject = cPlate;
                weld2.SecondaryObject = girt;
                //weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                //weld2.SizeAbove = inch2MM(0.5);
                weld2.SizeBelow = mm2Inch(0.5);
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
                weld2.AroundWeld = false;
                weld2.ShopWeld = false;
                if (!weld2.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region Bolt Array
                Part secondaryPart = cPlate;
                Part primaryPart = girt;
                BoltArray bArr = new BoltArray();
                bArr.Delete();
                bArr.PartToBoltTo = primaryPart;
                bArr.PartToBeBolted = secondaryPart;
                bArr.FirstPosition = cPlate.StartPoint;
                bArr.SecondPosition = cPlate.EndPoint;
                bArr.BoltSize = mm2Inch(0.5);
                bArr.Tolerance = mm2Inch(0.079);
                bArr.BoltStandard = "A325N";
                bArr.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr.Length = 80;
                bArr.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr.Position.Rotation = Position.RotationEnum.FRONT;

                bArr.AddBoltDistX(0);
                bArr.StartPointOffset.Dx = mm2Inch(5.375);
                bArr.AddBoltDistY(mm2Inch(1.5));
                //bArr.Insert();
                if (!bArr.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
        }

        //  DOUBLE GIRT
        private void btn_DoubleGirt_Click(object sender, EventArgs e)
        {
            #region PICK Parts
            ModelObject column = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Primary Part");
            ModelObject girt1 = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Secondary Part 1");
            ModelObject girt2 = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick Secondary Part 2");
            #endregion

            #region Error Check (Girt Elevation Level)
            Beam girtPart1 = girt1 as Beam;
            Beam girtPart2 = girt2 as Beam;
            Beam col = column as Beam;

            Point girt1Point;
            Point girt2Point;

            // Try Center Lines
            if (Distance.PointToPoint(girtPart1.StartPoint, col.EndPoint) < Distance.PointToPoint(girtPart1.EndPoint, col.EndPoint))
            {
                girt1Point = girtPart1.StartPoint;
            }
            else
            {
                girt1Point = girtPart1.EndPoint;
            }

            if (Distance.PointToPoint(girtPart2.StartPoint, col.EndPoint) < Distance.PointToPoint(girtPart2.EndPoint, col.EndPoint))
            {
                girt2Point = girtPart2.StartPoint;
            }
            else
            {
                girt2Point = girtPart2.EndPoint;
            }

            if (girt1Point.Z == girt2Point.Z)
            {
                MessageBox.Show("The elevation of the two girts is same");
                #endregion

                #region Coordinate System & WorkPlane
                var colPart = column as Beam;
                var g1 = girt1 as Beam;
                var g2 = girt2 as Beam;

                Point origin1 = girt1Point;

                try
                {
                    if (origin1 == null)
                    {
                        MessageBox.Show("No point selected. Coordinate System creation aborted");
                        return;
                    }
                    var girtCoord = (girt1 as Part).GetCoordinateSystem();
                    girtCoord.Origin = origin1;
                    //girtCoord.AxisX = girtCoord.AxisX *- 1;
                    var workPlaneHandler = new Model().GetWorkPlaneHandler();
                    var newWorkPlane = new TransformationPlane(girtCoord);

                    new Model().CommitChanges();

                    if (girtCoord == null)
                        throw new ArgumentNullException(nameof(girtCoord));

                    var gd = new GraphicsDrawer();
                    var origin = girtCoord.Origin;

                    var xPoint = origin + 400 * girtCoord.AxisX.GetNormal();
                    var yPoint = origin + 400 * girtCoord.AxisY.GetNormal();
                    var zPoint = origin + 400 * girtCoord.AxisX.Cross(girtCoord.AxisY).GetNormal();

                    var xColor = new Color(1, 0, 0);
                    var yColor = new Color(0, 1, 0);
                    var zColor = new Color(0, 0, 1);

                    gd.DrawLineSegment(origin, xPoint, xColor);
                    gd.DrawText(xPoint, "X", xColor);
                    gd.DrawLineSegment(origin, yPoint, yColor);
                    gd.DrawText(yPoint, "Y", yColor);
                    gd.DrawLineSegment(origin, zPoint, zColor);
                    gd.DrawText(zPoint, "Z", zColor);

                    if (!string.IsNullOrWhiteSpace("ORIGIN"))
                        gd.DrawText(origin, "ORIGIN", new Color(0, 0, 0));

                    if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                    {
                        MessageBox.Show("Coordinate System failed to insert");
                        myModel.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred");
                }
                #endregion

                #region Girt CUTs
                CutPlane cutPlane1 = new CutPlane();
                CutPlane cutPlane2 = new CutPlane();
                Beam girt1Part = girt1 as Beam;
                Beam girt2Part = girt2 as Beam;

                cutPlane1.Plane = new Plane();
                cutPlane1.Plane.Origin = new Point(mm2Inch(4), 0, 0);
                cutPlane1.Plane.AxisX = new Vector(0, mm2Inch(4), 0);
                cutPlane1.Plane.AxisY = new Vector(0, 0, mm2Inch(-4));
                cutPlane1.Father = girtPart2;
                cutPlane1.Insert();

                cutPlane2.Plane = new Plane();
                cutPlane2.Plane.Origin = new Point(mm2Inch(-4), 0, 0);
                cutPlane2.Plane.AxisX = new Vector(0, mm2Inch(-4), mm2Inch(-4));
                cutPlane2.Plane.AxisY = new Vector(0, 0, mm2Inch(-4));
                cutPlane2.Father = girtPart1;
                cutPlane2.Insert();
                #endregion

                #region PLATEs
                Point plStartPoint1 = new Point(mm2Inch(0.125), 0, mm2Inch(-0.125));
                Point plEndPoint1 = new Point(mm2Inch(7), 0, mm2Inch(-0.125));
                Beam plate1 = new Beam(plStartPoint1, plEndPoint1);
                string plateProfile = "PL";
                string plateWidth = "76.2";     // 3 inches
                string plateThickness = "12.7"; // 1/2 inch
                plate1.Profile.ProfileString = $"{plateProfile} {plateWidth}*{plateThickness}";
                plate1.Material.MaterialString = "A153";
                plate1.Class = "4";
                plate1.Position.Plane = Position.PlaneEnum.MIDDLE;
                plate1.Position.Rotation = Position.RotationEnum.FRONT;
                plate1.Insert();

                Point plStartPoint2 = new Point(mm2Inch(-0.125), 0, mm2Inch(-0.125));
                Point plEndPoint2 = new Point(mm2Inch(-7), 0, mm2Inch(-0.125));
                Beam plate2 = new Beam(plStartPoint2, plEndPoint2);
                plate2.Profile.ProfileString = $"{plateProfile} {plateWidth}*{plateThickness}";
                plate2.Material.MaterialString = "A153";
                plate2.Class = "4";
                plate2.Position.Plane = Position.PlaneEnum.MIDDLE;
                plate2.Position.Rotation = Position.RotationEnum.FRONT;
                plate2.Insert();
                #endregion

                #region Plates WELDs to Column
                Weld weld1 = new Weld();
                ObjectList.Add(colPart);
                ObjectList.Add(plate1);
                weld1.Delete();
                weld1.MainObject = colPart;
                weld1.SecondaryObject = plate1;
                weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld1.SizeAbove = mm2Inch(0.5);
                weld1.SizeBelow = mm2Inch(0.5);
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld1.AroundWeld = false;
                weld1.ShopWeld = false;
                if (!weld1.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }

                Weld weld2 = new Weld();
                ObjectList.Add(colPart);
                ObjectList.Add(plate1);
                weld2.Delete();
                weld2.MainObject = colPart;
                weld2.SecondaryObject = plate2;
                weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                weld2.SizeAbove = mm2Inch(0.5);
                weld2.SizeBelow = mm2Inch(0.5);
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Z;
                weld2.AroundWeld = false;
                weld2.ShopWeld = false;
                if (!weld2.Insert())
                {
                    MessageBox.Show("Weld insert failed");
                }
                #endregion

                #region Plates BOLTING to Girts
                //Beam plate1_Part = plate1 as Beam;
                //Part secondaryPart1 = girtPart1;
                BoltArray bArr1 = new BoltArray();
                bArr1.PartToBoltTo = plate1;
                bArr1.PartToBeBolted = girtPart1;
                bArr1.FirstPosition = plate1.StartPoint;
                bArr1.SecondPosition = plate1.EndPoint;
                bArr1.BoltSize = mm2Inch(0.5);
                bArr1.Tolerance = 2.00;
                bArr1.BoltStandard = "A325N";
                bArr1.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr1.Length = mm2Inch(3.15);
                bArr1.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr1.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr1.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr1.Position.Rotation = Position.RotationEnum.FRONT;
                bArr1.AddBoltDistX(0);
                bArr1.StartPointOffset.Dx = mm2Inch(5.375);
                bArr1.AddBoltDistY(mm2Inch(1.5));
                if (!bArr1.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }

                //Beam plate2_Part = plate2 as Beam;
                //Part secondaryPart2 = girtPart2;
                BoltArray bArr2 = new BoltArray();
                bArr2.PartToBoltTo = plate2;
                bArr2.PartToBeBolted = girtPart2;
                bArr2.FirstPosition = plate2.StartPoint;
                bArr2.SecondPosition = plate2.EndPoint;
                bArr2.BoltSize = mm2Inch(0.5);
                bArr2.Tolerance = 2.00;
                bArr2.BoltStandard = "A325N";
                bArr2.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
                bArr2.Length = mm2Inch(3.15);
                bArr2.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
                bArr2.Position.Depth = Position.DepthEnum.MIDDLE;
                bArr2.Position.Plane = Position.PlaneEnum.MIDDLE;
                bArr2.Position.Rotation = Position.RotationEnum.FRONT;
                bArr2.AddBoltDistX(0);
                bArr2.StartPointOffset.Dx = mm2Inch(5.375);
                bArr2.AddBoltDistY(mm2Inch(1.5));
                if (!bArr2.Insert())
                {
                    MessageBox.Show("BoltArray Insert Failed");
                }
                #endregion
            }
            else
            {
                MessageBox.Show("The elevation of the two girts is NOT same");
                return;
            }
            SetGlobalandStoreOriginal();
            myModel.CommitChanges();
        }

        //  KNEE CONNECTION
        private void btn_KneeConn_Click(object sender, EventArgs e)
        {
            #region PICK Parts
            ModelObject columnObj = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Primary Part i.e. Column");
            ModelObject beamObj = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Secondary Part i.e. Beam");
            #endregion

            #region Error Check

            Beam column = columnObj as Beam;
            Beam beam = beamObj as Beam;

            List<System.Drawing.Point> intersectionPoints = new List<System.Drawing.Point>();

            var colCL = (column as Part).GetCenterLine(false);
            var beamCL = (beam as Part).GetCenterLine(false);
            Beam colBeam = column as Beam;

            Point intersection = Intersection.LineToPlane((new Line(beamCL[0] as Point, beamCL[1] as Point)), new GeometricPlane(colBeam.StartPoint, colBeam.GetCoordinateSystem().AxisY));

            if (intersection.Z == colBeam.StartPoint.Z)
            {
                Point origin = intersection;
            }
            #endregion

            #region Coordinate System & WorkPlane
            var beamPart = beam as Beam;
            var c = column as Beam;
            //            Point origin = intersection;
            try
            {
                if (intersection == null)
                {
                    MessageBox.Show("No Point selected. Coordinate System creation aborted");
                    return;
                }
                var colCoord = (column as Part).GetCoordinateSystem();
                colCoord.Origin = intersection;

                var axisX = new Vector(GetAxisX(beam, column)).GetNormal();
                var axisY = new Vector(column.GetCoordinateSystem().AxisX).GetNormal();

                var cs = new CoordinateSystem(intersection, axisX, axisY);

                DrawCoordinateSystem(cs);

                var workPlaneHandler = new Model().GetWorkPlaneHandler();
                //var newWorkPlane = new TransformationPlane(colCoord);
                var newWorkPlane = new TransformationPlane(cs);

                new Model().CommitChanges();

                //if (colCoord == null)
                //throw new ArgumentNullException(nameof(colCoord));
                if (cs == null)
                    throw new ArgumentNullException(nameof(cs));

                /*                //var gd = new GraphicsDrawer();
                                //var origin1 = colCoord.Origin;

                                //var xPoint = origin1 + 400 * colCoord.AxisX.GetNormal();
                                //var yPoint = origin1 + 400 * colCoord.AxisY.GetNormal();
                                //var zPoint = origin1 + 400 * colCoord.AxisY.Cross(colCoord.AxisX).GetNormal();

                                //var xColor = new Color(1,0,0);
                                //var yColor = new Color(0,1,0);
                                //var zColor = new Color(0,0,1);

                                //gd.DrawLineSegment(origin1, xPoint, xColor);
                                //gd.DrawText(xPoint, "X", xColor);
                                //gd.DrawLineSegment(origin1, yPoint, yColor);
                                //gd.DrawText(yPoint, "Y", yColor);
                                //gd.DrawLineSegment(origin1, zPoint, zColor);
                                //gd.DrawText(zPoint, "Z", zColor);

                                //if (!string.IsNullOrWhiteSpace("ORIGIN"))
                                //    gd.DrawText(origin1, "ORIGIN", new Color(0, 0, 0));*/

                if (!workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane))
                {
                    MessageBox.Show("Coordinate System failed to insert");
                    myModel.CommitChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred");
            }
            #endregion

            #region STIFFENER PLATES
            #region Plate1 Column_FlangeFACE
            //Point startPoint1 = new Point(310, 206.365, 0);
            //Point endPoint1 = new Point(-767.2, 206.365, 0);
            Point startPoint1 = new Point(mm2Inch(11.4375), mm2Inch(8.625), mm2Inch(0));
            Point endPoint1 = new Point(mm2Inch(-2.186), mm2Inch(-32.25), mm2Inch(0));
            Beam cp1 = new Beam(startPoint1, endPoint1);
            cp1.Profile.ProfileString = "PL25.4*180.98 ";
            cp1.Material.MaterialString = "A36";
            cp1.Class = "99";
            cp1.Position.Plane = Position.PlaneEnum.LEFT;
            cp1.Position.Depth = Position.DepthEnum.MIDDLE;
            cp1.Position.Rotation = Position.RotationEnum.TOP;
            cp1.Insert();
            #endregion

            #region Plate3 Horizontal Column Web SideL
            Point startPoint3 = new Point(mm2Inch(-2.3125), mm2Inch(-30.625), mm2Inch(-0.1875));
            Point endPoint3 = new Point(mm2Inch(-16.535), mm2Inch(-25.875), mm2Inch(-0.1875));
            Beam cp3 = new Beam(startPoint3, endPoint3);
            cp3.Profile.ProfileString = "PL9.53*76.2";
            cp3.Material.MaterialString = "A36";
            cp3.Class = "99";
            cp3.Position.Plane = Position.PlaneEnum.LEFT;
            cp3.Position.Rotation = Position.RotationEnum.TOP;
            cp3.Position.Depth = Position.DepthEnum.BEHIND;
            cp3.Insert();
            #endregion

            #region Plate2 Horizontal Column Web SideR
            Point startPoint2 = new Point(mm2Inch(-2.3125), mm2Inch(-30.625), mm2Inch(0.1875));
            Point endPoint2 = new Point(mm2Inch(-16.535), mm2Inch(-25.875), mm2Inch(0.1875));
            Beam cp2 = new Beam(startPoint2, endPoint2);
            cp2.Profile.ProfileString = "PL9.53*76.2";
            cp2.Material.MaterialString = "A36";
            cp2.Class = "99";
            cp2.Position.Plane = Position.PlaneEnum.LEFT;
            cp2.Position.Rotation = Position.RotationEnum.TOP;
            cp2.Position.Depth = Position.DepthEnum.FRONT;
            cp2.Insert();
            #endregion

            #region Plate3 Diagonal Column Web SideL
            Point startPoint4 = new Point(mm2Inch(5), mm2Inch(-7.6875), mm2Inch(-0.1875));
            Point endPoint4 = new Point(mm2Inch(-16.5725), mm2Inch(-25.8375), mm2Inch(-0.1875));
            Beam cp4 = new Beam(startPoint4, endPoint4);
            cp4.Profile.ProfileString = "PL9.53*76.2";
            cp4.Material.MaterialString = "A36";
            cp4.Class = "99";
            cp4.Position.Plane = Position.PlaneEnum.LEFT;
            cp4.Position.Rotation = Position.RotationEnum.TOP;
            cp4.Position.Depth = Position.DepthEnum.BEHIND;
            cp4.Insert();
            #endregion

            #region Plate2 Diagonal Column Web SideR
            Point startPoint5 = new Point(mm2Inch(5), mm2Inch(-7.6875), mm2Inch(0.1875));
            Point endPoint5 = new Point(mm2Inch(-16.5725), mm2Inch(-25.8375), mm2Inch(0.1875));
            Beam cp5 = new Beam(startPoint5, endPoint5);
            cp5.Profile.ProfileString = "PL9.53*76.2";
            cp5.Material.MaterialString = "A36";
            cp5.Class = "99";
            cp5.Position.Plane = Position.PlaneEnum.LEFT;
            cp5.Position.Rotation = Position.RotationEnum.TOP;
            cp5.Position.Depth = Position.DepthEnum.FRONT;
            cp5.Insert();
            #endregion

            #region Plate4 Diagonal Column Flange Beam Bottom SideBottom
            Point startPoint6 = new Point(mm2Inch(35.9375), mm2Inch(-8), mm2Inch(0));
            Point endPoint6 = new Point(mm2Inch(-0.75), mm2Inch(-31.125), mm2Inch(0));
            Beam cp6 = new Beam(startPoint6, endPoint6);
            cp6.Profile.ProfileString = "PL9.53*177.8";
            cp6.Material.MaterialString = "A36";
            cp6.Class = "99";
            cp6.Position.Plane = Position.PlaneEnum.LEFT;
            cp6.Position.Rotation = Position.RotationEnum.TOP;
            cp6.Position.Depth = Position.DepthEnum.MIDDLE;
            cp6.Insert();
            #endregion

            #region Plate5 Perpendicular Diagonal Column Flange Beam Bottom SideBottom
            //Point startPoint7 = new Point(mm2Inch(), mm2Inch(), mm2Inch());
            //Point endPoint7 = new Point(mm2Inch(), mm2Inch(), mm2Inch());
            Point startPoint7 = cp6.StartPoint;
            Point endPoint7 = cp6.EndPoint;
            Beam cp7 = new Beam(startPoint7, endPoint7);
            cp7.Profile.ProfileString = "PL9.53*392.08";
            cp7.Material.MaterialString = "A36";
            cp7.Class = "99";
            cp7.Position.Plane = Position.PlaneEnum.RIGHT;
            cp7.Position.Rotation = Position.RotationEnum.FRONT;
            cp7.Position.Depth = Position.DepthEnum.MIDDLE;
            cp7.Insert();
            #endregion

            #endregion

            #region Rafter CUT
            CutPlane cutPlane1 = new CutPlane();
            //Beam beamPart1 = beam as Beam;
            cutPlane1.Plane = new Plane();
            //cutPlane1.Plane.Origin = new Point() + (mm2Inch(9.125) * column.GetCoordinateSystem().AxisY.GetNormal());
            cutPlane1.Plane.Origin = cp1.StartPoint;
            cutPlane1.Plane.AxisX = new Vector(mm2Inch(0), mm2Inch(0), mm2Inch(20));
            cutPlane1.Plane.AxisY = column.GetCoordinateSystem().AxisX;
            //cutPlane1.Father = beamPart1;
            cutPlane1.Father = beam;
            cutPlane1.Insert();

            #region Plate CUT
            CutPlane cutPlane2 = new CutPlane();
            cutPlane2.Plane = new Plane();
            cutPlane2.Plane.Origin = cp6.EndPoint;
            cutPlane2.Plane.AxisX = new Vector(mm2Inch(0), mm2Inch(0), mm2Inch(20));
            cutPlane2.Plane.AxisY = column.GetCoordinateSystem().AxisX;
            cutPlane2.Father = cp7;
            cutPlane2.Insert();
            #endregion

            #region Plate CUT
            CutPlane cutPlane3 = new CutPlane();
            cutPlane3.Plane = new Plane();
            cutPlane3.Plane.Origin = cp6.StartPoint;
            cutPlane3.Plane.AxisX = new Vector(mm2Inch(0), mm2Inch(0), mm2Inch(20));
            cutPlane3.Plane.AxisY = beam.GetCoordinateSystem().AxisX;
            cutPlane3.Father = cp7;
            cutPlane3.Insert();
            #endregion
            #endregion

            #region WELDs
            var colPart = column as Beam;

            #region weld cp1 to beam
            Weld weld = new Weld();
            ObjectList.Add(beam);
            ObjectList.Add(cp1);
            weld.Delete();
            weld.MainObject = beam;
            weld.SecondaryObject = cp1;
            weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld.SizeAbove = mm2Inch(0.5);
            weld.SizeBelow = mm2Inch(0.5);
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_MINUS_X;
            weld.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld.AroundWeld = false;
            weld.ShopWeld = false;
            if (!weld.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 1 cp1
            Weld weld1 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp1);
            weld1.Delete();
            weld1.MainObject = colPart;
            weld1.SecondaryObject = cp1;
            weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld1.SizeAbove = mm2Inch(0.5);
            weld1.SizeBelow = mm2Inch(0.5);
            weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld1.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld1.AroundWeld = false;
            weld1.ShopWeld = false;
            if (!weld1.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 2 cp2
            Weld weld2 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp2);
            weld2.Delete();
            weld2.MainObject = colPart;
            weld2.SecondaryObject = cp2;
            weld2.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld2.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld2.SizeAbove = mm2Inch(0.5);
            weld2.SizeBelow = mm2Inch(0.5);
            weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld2.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld2.AroundWeld = false;
            weld2.ShopWeld = false;
            if (!weld2.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 3 cp3
            Weld weld3 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp3);
            weld3.Delete();
            weld3.MainObject = colPart;
            weld3.SecondaryObject = cp3;
            weld3.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld3.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld3.SizeAbove = mm2Inch(0.5);
            weld3.SizeBelow = mm2Inch(0.5);
            weld3.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld3.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld3.AroundWeld = false;
            weld3.ShopWeld = false;
            if (!weld3.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 4 cp4
            Weld weld4 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp4);
            weld4.Delete();
            weld4.MainObject = colPart;
            weld4.SecondaryObject = cp4;
            weld4.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld4.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld4.SizeAbove = mm2Inch(0.5);
            weld4.SizeBelow = mm2Inch(0.5);
            weld4.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld4.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld4.AroundWeld = false;
            weld4.ShopWeld = false;
            if (!weld4.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 5 cp5
            Weld weld5 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp5);
            weld5.Delete();
            weld5.MainObject = colPart;
            weld5.SecondaryObject = cp5;
            weld5.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld5.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld5.SizeAbove = mm2Inch(0.5);
            weld5.SizeBelow = mm2Inch(0.5);
            weld5.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld5.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld5.AroundWeld = false;
            weld5.ShopWeld = false;
            if (!weld5.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 6 cp6
            Weld weld6 = new Weld();
            ObjectList.Add(cp6);
            ObjectList.Add(cp7);
            weld6.Delete();
            weld6.MainObject = cp6;
            weld6.SecondaryObject = cp7;
            weld6.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld6.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld6.SizeAbove = mm2Inch(0.5);
            weld6.SizeBelow = mm2Inch(0.5);
            weld6.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld6.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_Y;
            weld6.AroundWeld = false;
            weld6.ShopWeld = false;
            if (!weld6.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 7 cp7
            Weld weld7 = new Weld();
            ObjectList.Add(colPart);
            ObjectList.Add(cp7);
            weld7.Delete();
            weld7.MainObject = colPart;
            weld7.SecondaryObject = cp7;
            weld7.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld7.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld7.SizeAbove = mm2Inch(0.5);
            weld7.SizeBelow = mm2Inch(0.5);
            weld7.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld7.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld7.AroundWeld = false;
            weld7.ShopWeld = false;
            if (!weld7.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #region weld 8 cp6
            Weld weld8 = new Weld();
            ObjectList.Add(beam);
            ObjectList.Add(cp6);
            weld8.Delete();
            weld8.MainObject = beam;
            weld8.SecondaryObject = cp6;
            weld8.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld8.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            weld8.SizeAbove = mm2Inch(0.3);
            weld8.SizeBelow = mm2Inch(0.3);
            weld8.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld8.Position = Weld.WeldPositionEnum.WELD_POSITION_PLUS_X;
            weld8.AroundWeld = false;
            weld8.ShopWeld = false;
            if (!weld8.Insert())
            {
                MessageBox.Show("Weld insert failed");
            }
            #endregion

            #endregion

            #region BOLTs
            Beam col_Part = column as Beam;
            Part secondaryPart = cp1;
            #region Bolt Array 1
            BoltArray bArr1 = new BoltArray();
            bArr1.Delete();
            bArr1.PartToBoltTo = col_Part;
            bArr1.PartToBeBolted = secondaryPart;
            bArr1.FirstPosition = cp1.StartPoint;
            bArr1.SecondPosition = cp1.EndPoint;
            bArr1.BoltSize = mm2Inch(0.75);
            bArr1.Tolerance = 2.00;
            bArr1.BoltStandard = "A325N";
            bArr1.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr1.Length = mm2Inch(3.150);
            bArr1.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr1.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr1.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr1.Position.Rotation = Position.RotationEnum.TOP;
            bArr1.AddBoltDistX(4 * mm2Inch(0.7));
            bArr1.AddBoltDistX(4 * mm2Inch(0.7));
            bArr1.StartPointOffset.Dx = mm2Inch(7);
            bArr1.AddBoltDistY(mm2Inch(3));
            if (!bArr1.Insert())
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            #endregion

            #region Bolt Array 2
            Point beamStartPoint = col_Part.StartPoint;
            Part secondaryPart2 = cp1;
            BoltArray bArr2 = new BoltArray();
            bArr2.Delete();
            bArr2.PartToBoltTo = col_Part;
            bArr2.PartToBeBolted = secondaryPart2;
            bArr2.FirstPosition = cp1.EndPoint;
            bArr2.SecondPosition = cp1.StartPoint;
            bArr2.BoltSize = mm2Inch(0.75);
            bArr2.Tolerance = 2.00;
            bArr2.BoltStandard = "A325N";
            bArr2.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_SITE;
            bArr2.Length = mm2Inch(3.150);
            bArr2.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;
            bArr2.Position.Depth = Position.DepthEnum.MIDDLE;
            bArr2.Position.Plane = Position.PlaneEnum.MIDDLE;
            bArr2.Position.Rotation = Position.RotationEnum.BELOW;
            bArr2.AddBoltDistX(4 * mm2Inch(0.7));
            bArr2.AddBoltDistX(4 * mm2Inch(0.7));
            bArr2.StartPointOffset.Dx = mm2Inch(10);
            bArr2.AddBoltDistY(mm2Inch(3));
            if (!bArr2.Insert())
            {
                MessageBox.Show("BoltArray Insert Failed");
            }
            #endregion
            #endregion

            SetGlobalandStoreOriginal();
        }

        //  SPLICE CONNECTION
        private void btn_SpliceConn_Click(object sender, EventArgs e)
        {
            TransformationPlane orginalTP = SetGlobalAndStoreOriginal();    //saving current TransformationPlane and setting new    // variable 'originalTP' of type 'TransformationPlane'

            CoordinateSystem cso = new CoordinateSystem();      //  creates a new coordinate system
            (Beam beam1, Beam beam2) pick = Pick2Beams();       //  pick two beams from the model and returning them as a Tuple

            GeometricPlane beam1GP = GetGeometricPlane(pick.beam1);  //  get the Geometric Planes of both beams
            GeometricPlane beam2GP = GetGeometricPlane(pick.beam2);  //  GetGeometryPlane retrieves/calculates a GeometricPlane associated with the Beam object provided

            bool isParrallel = Tekla.Structures.Geometry3d.Parallel.PlaneToPlane(beam1GP, beam2GP); //check if the planes of the two beams are parallel or not
            if (isParrallel)
            {
                try
                {
                    ArrayList beam1CL = pick.beam1.GetCenterLine(false);    // get the centre line of beam 1
                    ArrayList beam2CL = pick.beam2.GetCenterLine(false);    // get the centre line of beam 2

                    Point midBeam1 = GetMidPoint(beam1CL[0] as Point, beam1CL[1] as Point); //calculate the midpoints of the center lines of both beams
                    Point midBeam2 = GetMidPoint(beam2CL[0] as Point, beam2CL[1] as Point); // and store in Point midBeam1 & midBeam2

                    Point originPoint = GetMidPoint(midBeam1, midBeam2);    //calculate the midpoint between two midpoints to use as the origin

                    //define a new coordinate system at the origin point with X & Y axes from first beam
                    CoordinateSystem cs = new CoordinateSystem(originPoint, pick.beam1.GetCoordinateSystem().AxisX.GetNormal(), pick.beam1.GetCoordinateSystem().AxisY.GetNormal());

                    //create & set a new transformation plane using the defined coordinate system
                    TransformationPlane transformationPlane = new TransformationPlane(cs);
                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(transformationPlane);

                    CoordinateSystem setCoor = new CoordinateSystem();

                    //draw the coordinate system for debugging process
                    ViewHandler.RedrawWorkplane();
                    DrawCoordinateSystem(new CoordinateSystem(), "Org");

                    //inserting the fittings on both beams at the defined coordinate system
                    InsertFitting(pick.beam1, setCoor);
                    InsertFitting(pick.beam2, setCoor);

                    //inserting web plates and flange plates on the beams
                    Beam webPlate = InsertWebPlate(pick.beam1, setCoor);
                    (Beam beamTP, Beam beamBP) flangePlate = InsertFlangePlates(pick.beam1, setCoor);

                    //transform the midpoints into the local coordinate system of the new transformation plane
                    TransformationPlane transformationPlane1 = new TransformationPlane(setCoor);
                    Matrix matrix = transformationPlane1.TransformationMatrixToLocal;

                    var trMidBeam1 = matrix.Transform(new Point(midBeam1));
                    var trMidBeam2 = matrix.Transform(new Point(midBeam2));

                    //draw text at the transformed midpoints for debugging purpose
                    new GraphicsDrawer().DrawText(trMidBeam1, "b1p", new Color());
                    new GraphicsDrawer().DrawText(trMidBeam2, "b2p", new Color());

                    //calculcate the direction vectors for placing bolts on the web plate
                    var directionVectorToMovePointToDesiredLocation = new Vector(trMidBeam1 - setCoor.Origin).GetNormal();
                    var directionVectorToMovePointToDesiredLocation1 = new Vector(trMidBeam2 - setCoor.Origin).GetNormal();

                    //insert bolts on the web plate at the specified positions 
                    InsertBolts(webPlate, pick.beam1, setCoor.Origin, new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation), Position.RotationEnum.FRONT);
                    InsertBolts(webPlate, pick.beam2, setCoor.Origin, new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1), Position.RotationEnum.BACK);

                    InsertBolts(flangePlate.beamTP, pick.beam1, new Point(beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation) + beamDepth * 0.5 * setCoor.AxisY.GetNormal(), Position.RotationEnum.BELOW);
                    InsertBolts(flangePlate.beamTP, pick.beam2, new Point(beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1) + beamDepth * 0.5 * setCoor.AxisY.GetNormal(), Position.RotationEnum.TOP);

                    InsertBolts(flangePlate.beamBP, pick.beam1, new Point(-beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation - beamDepth * 0.5 * setCoor.AxisY.GetNormal()), Position.RotationEnum.BELOW);
                    InsertBolts(flangePlate.beamBP, pick.beam2, new Point(-beamDepth * 0.5 * setCoor.AxisY.GetNormal()), new Point((plateLength * 0.5) * directionVectorToMovePointToDesiredLocation1 - beamDepth * 0.5 * setCoor.AxisY.GetNormal()), Position.RotationEnum.TOP);
                }
                catch
                {
                    throw new Exception();  //if an exception occurs throw a new generic exception
                }

                finally
                {
                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(orginalTP); // restore the original transformation plane
                }
            }
            else
            {
                MessageBox.Show("Invalid option");  //  display msg if the beams are not parallel
            }
            SetGlobalandStoreOriginal();
        }

        #region SUPPORT METHODS
        private double mm2Inch(double inch)
        {
            return (inch * 25.4);
        }
        private GeometricPlane GetGeometricPlane(Beam b1)   //  geometric plane
        {
            GeometricPlane geoPlane = new GeometricPlane(b1.GetCoordinateSystem());
            return geoPlane;
        }
        private (Beam b1, Beam b2) Pick2Beams() //  pick 2 beams as Tuple
        {
            Beam pick1Beam = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "pick first beam") as Beam;
            Beam pick2Beam = _picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "pick second beam") as Beam;
            return (pick1Beam, pick2Beam);
        }
        private TransformationPlane SetGlobalAndStoreOriginal() //  Transformation Plane
        {
            WorkPlaneHandler myWorkPlaneHandler = myModel.GetWorkPlaneHandler();
            TransformationPlane original = myWorkPlaneHandler.GetCurrentTransformationPlane();
            TransformationPlane newTSPlane = new TransformationPlane();
            myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newTSPlane);
            myModel.CommitChanges();
            return original;
        }
        private Point GetMidPoint(Beam b)   //  get Mid Point
        {
            Point startPoint = b.StartPoint;
            Point endPoint = b.EndPoint;
            Point midPoint = new Point((startPoint.X + endPoint.X) * 0.5, (startPoint.Y + endPoint.Y) * 0.5, (startPoint.Z + endPoint.Z) * 0.5);
            return midPoint;
        }
        private Point GetMidPoint(Point p1, Point p2) //  midpoint by passsing two points
        {
            Point midPoint = new Point((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5, (p1.Z + p2.Z) * 0.5);
            return midPoint;
        }
        public void DrawCoordinateSystem(CoordinateSystem cs, string textAtOrigin)  //  Coordinate System
        {
            double lengthFoot = 2.0;
            GraphicsDrawer graphicDrawer = new GraphicsDrawer();

            Point X = cs.Origin + cs.AxisX.GetNormal() * lengthFoot * 12 * 25.4;
            Point Y = cs.Origin + cs.AxisY.GetNormal() * lengthFoot * 12 * 25.4;
            Point Z = cs.Origin + cs.AxisX.Cross(cs.AxisY).GetNormal() * lengthFoot * 12 * 25.4;

            graphicDrawer.DrawText(cs.Origin, textAtOrigin, new Tekla.Structures.Model.UI.Color());
            graphicDrawer.DrawText(X, "X", new Color(1, 0, 0));
            graphicDrawer.DrawText(Y, "Y", new Color(0, 1, 0));
            graphicDrawer.DrawText(Z, "Z", new Color(0, 0, 1));

            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, X), new Color(1, 0, 0));
            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, Y), new Color(0, 1, 0));
            graphicDrawer.DrawLineSegment(new LineSegment(cs.Origin, Z), new Color(0, 0, 1));
        }
        private void InsertFitting(Beam b, CoordinateSystem cs) //  Fitting
        {
            Fitting fitplane = new Fitting();
            fitplane.Plane = new Plane();
            fitplane.Plane.Origin = new Point();
            fitplane.Plane.AxisX = cs.AxisX.Cross(cs.AxisY).GetNormal();
            fitplane.Plane.AxisY = cs.AxisY.GetNormal();
            fitplane.Father = b;
            fitplane.Insert();
            myModel.CommitChanges();
        }
        private Beam InsertWebPlate(Beam b, CoordinateSystem cs)    //  Web plates
        {
            double WEB_THICKNESS = 0.0;
            b.GetReportProperty("WEB_THICKNESS", ref WEB_THICKNESS);
            Point p1 = new Point(plateLength * cs.AxisX.GetNormal() + (WEB_THICKNESS * 0.5) * cs.AxisX.Cross(cs.AxisY).GetNormal());
            Point p2 = new Point(plateLength * -1 * (cs.AxisX.GetNormal()) + (WEB_THICKNESS * 0.5) * cs.AxisX.Cross(cs.AxisY).GetNormal());
            string profile = $"PL{plateThickness}*{plateLength}";
            string material = "A36";
            Beam webPlate = InsertBeam(profile, material, p1, p2, Position.PlaneEnum.MIDDLE, Position.RotationEnum.FRONT, Position.DepthEnum.FRONT);
            myModel.CommitChanges();
            return webPlate;
        }
        private (Beam beamTP, Beam beamBP) InsertFlangePlates(Beam b, CoordinateSystem cs)  //  Flange Plates
        {
            b.GetReportProperty("HEIGHT", ref beamDepth);
            Point point1TP = new Point(plateLength * cs.AxisX.GetNormal() + (beamDepth * 0.5) * cs.AxisY.GetNormal());
            Point point2TP = new Point(-point1TP.X, point1TP.Y, point1TP.Z);
            string profile = $"PL{plateThickness}*{plateLength}";
            string material = "A36";
            Beam beamTP = InsertBeam(profile, material, point1TP, point2TP, Position.PlaneEnum.RIGHT, Position.RotationEnum.BELOW, Position.DepthEnum.MIDDLE);

            Point point1BP = new Point(plateLength * cs.AxisX.GetNormal() + -(beamDepth * 0.5) * cs.AxisY.GetNormal());
            Point point2BP = new Point(-point1BP.X, point1BP.Y, point1BP.Z);
            string profile1 = $"PL{plateThickness}*{plateLength}";
            string material1 = "A36";
            Beam beamBP = InsertBeam(profile1, material1, point1BP, point2BP, Position.PlaneEnum.LEFT, Position.RotationEnum.BELOW, Position.DepthEnum.MIDDLE);

            myModel.CommitChanges();
            return (beamTP, beamBP);
        }
        private Beam InsertBeam(string profile, string material, Point startPoint, Point endPoint, Position.PlaneEnum planeEnum, Position.RotationEnum rotationEnum, Position.DepthEnum depthEnum)
        {
            Beam Plate = new Beam(startPoint, endPoint)
            {
                Material = { MaterialString = material },
                Profile = { ProfileString = profile },
                Position =
                {
                    Plane = planeEnum,
                    Rotation = rotationEnum,
                    Depth = depthEnum
                }
            };
            Plate.Insert();
            return Plate;
        }
        private void InsertBolts(Beam webPlate, Beam beam, Point origin, Point point, RotationEnum rotation)
        // you can acess BoltSize ,Tolerance,  BoltStandard in method here by using (double boltsize , double tolarance, string Boltstandard)
        {
            BoltArray MPB = new BoltArray();

            MPB.PartToBeBolted = beam;
            MPB.PartToBoltTo = webPlate;

            MPB.FirstPosition = origin;
            MPB.SecondPosition = point;

            MPB.BoltSize = boltSize;  // this boltsize assign in form1 : form 
            MPB.Tolerance = tolerance;
            MPB.BoltStandard = boltStd;
            MPB.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            MPB.CutLength = cutLength;
            MPB.Length = length;
            MPB.ExtraLength = extraLength;
            MPB.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

            MPB.Position.Rotation = rotation;
            MPB.StartPointOffset.Dx = 2 * 25.4;

            MPB.Bolt = true;
            MPB.Washer1 = true;
            MPB.Washer2 = false;
            MPB.Nut1 = true;
            MPB.Nut2 = false;

            MPB.Hole1 = true;
            MPB.Hole2 = true;
            MPB.Hole3 = true;
            MPB.Hole4 = true;
            MPB.Hole5 = true;

            MPB.AddBoltDistX(50.8);
            MPB.AddBoltDistY(50.8);

            bool isInsert = MPB.Insert();
            myModel.CommitChanges();
        }
        private void DrawCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            double lengthFoot = 2.0;
            Tekla.Structures.Model.UI.GraphicsDrawer graphicsDrawer = new GraphicsDrawer();

            Point X = coordinateSystem.Origin + coordinateSystem.AxisX.GetNormal() * lengthFoot * 12 * 25.4;
            Point Y = coordinateSystem.Origin + coordinateSystem.AxisY.GetNormal() * lengthFoot * 12 * 25.4;
            Point Z = coordinateSystem.Origin + coordinateSystem.AxisX.Cross(coordinateSystem.AxisY).GetNormal() * lengthFoot * 12 * 25.4;

            graphicsDrawer.DrawText(coordinateSystem.Origin, "Org", new Color());

            ControlPoint org = new ControlPoint(coordinateSystem.Origin);
            org.Insert();

            graphicsDrawer.DrawText(X, "X", new Color(1, 0, 0));
            graphicsDrawer.DrawText(Y, "Y", new Color(0, 1, 0));
            graphicsDrawer.DrawText(Z, "Z", new Color(0, 0, 1));

            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, X), new Color(1, 0, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, Y), new Color(0, 1, 0));
            graphicsDrawer.DrawLineSegment(new LineSegment(coordinateSystem.Origin, Z), new Color(0, 0, 1));
        }
        private Vector GetAxisX(Beam beam, Beam column)
        {
            if (column != null && beam != null)
            {
                Vector axisX = new Vector();
                var distance1 = Distance.PointToPoint(column.EndPoint, beam.StartPoint);
                var distance2 = Distance.PointToPoint(column.EndPoint, beam.EndPoint);

                if (distance1 < distance2)
                {
                    var lineSegment = new LineSegment(column.EndPoint, beam.EndPoint);
                    axisX = new Vector(lineSegment.EndPoint - lineSegment.StartPoint).GetNormal();
                }
                else
                {
                    var lineSegment = new LineSegment(column.EndPoint, beam.StartPoint);
                    axisX = new Vector(lineSegment.EndPoint - lineSegment.StartPoint).GetNormal();
                }
                return axisX;
            }
            return null;
        }
        private TransformationPlane SetGlobalandStoreOriginal()
        {
            WorkPlaneHandler myWorkPlaneHandler = myModel.GetWorkPlaneHandler();
            TransformationPlane myTransformationPlane = myWorkPlaneHandler.GetCurrentTransformationPlane();

            // Create a new trasnformation plane based on the part coordinate system
            TransformationPlane newTSPlane = new TransformationPlane();

            // Set current transformation plane to the part plane
            myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newTSPlane);

            myModel.CommitChanges();
            return myTransformationPlane;
        }
        #endregion

    }
}
