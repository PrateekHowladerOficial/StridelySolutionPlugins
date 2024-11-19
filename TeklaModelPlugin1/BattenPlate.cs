using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using static Tekla.Structures.Model.Position;
using Tekla.Structures.Solid;
using Identifier = Tekla.Structures.Identifier;
using Line = Tekla.Structures.Geometry3d.Line;
using Fitting = Tekla.Structures.Model.Fitting;

using TeklaPH;

namespace TeklaModelPlugin1

{
    public class PluginData
    {
        #region Fields
        [StructuresField("Thickness")]
        public double Thickness;
        [StructuresField("Material")]
        public string Material;
        [StructuresField("Width")]
        public double Width;
        [StructuresField("DistenceBtwPlates")]
        public double DistenceBtwPlates;
        [StructuresField("NumberOfPlates")]
        public int NumberOfPlates;
        [StructuresField("Name")]
        public string Name;
        [StructuresField("Finish")]
        public string Finish;
        [StructuresField("Offset1")]
        public double Offset1;
        [StructuresField("Offset2")]
        public double Offset2;
        [StructuresField("Reverse")]
        public int Reverse;
        [StructuresField("LayoutIndex")]
        public int LayoutIndex;
        [StructuresField("PlateDepth")]
        public int PlateDepth;
        [StructuresField("Position")]
        public int Position;
        [StructuresField("offsetPosition")]
        public int offsetPosition;
        #endregion
    }

    [Plugin("Batten_Plate_1221")]
    [PluginUserInterface("TeklaModelPlugin1.MainForm")]
    
    public class TeklaModelPlugin1 : PluginBase
    {
        Model myModel = new Model();

        #region Fields
        private Model _Model;
        private PluginData _Data;
        public double _Thickness;
        public string _Material;
        public double _Width;
        public double _DistenceBtwPlates;
        public int _NumberOfPlates;
        public string _Name;
        public string _Finish;
        public double _Offset1;
        public double _Offset2;
        public int _Reverse;
        public int _LayoutIndex;
        public int _PlateDepth;
        public int _Position;
        public int _offsetPosition;
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
        public TeklaModelPlugin1(PluginData data)
        {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            List<InputDefinition> input = new List<InputDefinition>();
            try
            { //
              // This is an example for selecting two points; change this to suit your needs.
              //
                
                Picker picker = new Picker();
                var part = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick one object");
                var partno1 = part;
                input.Add(new InputDefinition(partno1.Identifier));
                Point Point = picker.PickPoint();
                input.Add(new InputDefinition(Point));
                part = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick one object");
                var partno2 = part;
                input.Add(new InputDefinition(partno2.Identifier));

                return input;
            }
            catch { }
            return input;
        }
        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                GetValuesFromDialog();

                try
                {


                    List<Point> bolt_points = new List<Point>();

                    var beam1Input = Input[0];
                    Beam beam1 = myModel.SelectModelObject(beam1Input.GetInput() as Identifier) as Beam;
                    Point pointInput = Input[1].GetInput() as Point;
                    Beam beam2 = myModel.SelectModelObject(Input[2].GetInput() as Identifier) as Beam;

                    Point origin1 = beam1.EndPoint;
                    var girtCoord = beam1.GetCoordinateSystem();
                    girtCoord.Origin = origin1;
                    //girtCoord.AxisX = girtCoord.AxisX *- 1;

                    TransformationPlane currentTransformation = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                    var newWorkPlane = new TransformationPlane(girtCoord);
                    // workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane);
                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newWorkPlane);

                    Point point = newWorkPlane.TransformationMatrixToLocal.Transform(currentTransformation.TransformationMatrixToGlobal.Transform(pointInput));

                    ArrayList beam1_centerpoints = beam1.GetCenterLine(false);
                    ArrayList beam2_centerpoints = beam2.GetCenterLine(false);
                    Point start_point = Projection.PointToLine(point,new Line (beam1_centerpoints[0] as Point, beam1_centerpoints[1] as Point));
                    Point refference = (_Reverse == 0) ? beam1_centerpoints[1] as Point : beam1_centerpoints[0] as Point;
                    List<Faces.Face_> beam1_faces = Faces.Get_faces(beam1);
                    List<Faces.Face_> beam2_faces = Faces.Get_faces(beam2);
                    Point p1 = new Point(), p2 = new Point(), p3 = new Point(), p4 = new Point(), hold = start_point;
                    List<ContourPlate> plates = new List<ContourPlate>();
                    Line line1 = new Line() , line2 = new Line();
                    switch (_offsetPosition)
                    {
                        case 0:
                            line1 = null;line2 = null;break;
                        case 1:
                            line1 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1)? 4:0].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 3 : 1].Face));
                            line2 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 3 : 1].Face), Faces.ConvertFaceToGeometricPlane(beam2_faces[(_Position == 1) ? 4 : 0].Face));
                            break;
                        case 2:
                            line1 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[ 2].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 3 : 1].Face));
                            line2 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 3 : 1].Face), Faces.ConvertFaceToGeometricPlane(beam2_faces[2].Face));
                            break ;

                    }


                    if (_LayoutIndex == 0 || _LayoutIndex == 1)
                    {
                        int positionIndex = (_Position == 0) ? 1 : 3;
                        GeometricPlane plane = Faces.ConvertFaceToGeometricPlane(beam1_faces[positionIndex].Face);

                        hold =  TeklaPH.Line.FindPointOnLine(start_point, refference, _DistenceBtwPlates);
                        for (int i = 0; i < _NumberOfPlates; i++)
                        {
                            if (Distance.PointToPoint(hold, refference) >= _Width)//doesnot alows the plates to be created out side the beam
                            {
                                p1 = hold;
                                p2 = TeklaPH.Line.FindPointOnLine(p1, refference, _Width);
                                p3 = TeklaPH.Line.FindPerpendicularIntersection(beam1_centerpoints[0] as Point, beam1_centerpoints[1] as Point, p2, beam2_centerpoints[0] as Point, beam2_centerpoints[1] as Point);
                                p4 = TeklaPH.Line.FindPerpendicularIntersection(beam1_centerpoints[0] as Point, beam1_centerpoints[1] as Point, p1, beam2_centerpoints[0] as Point, beam2_centerpoints[1] as Point);

                                plates.Add(countourPlate(Projection.PointToPlane(p1 ,plane), Projection.PointToPlane(p2 , plane), Projection.PointToPlane(p3 , plane), Projection.PointToPlane(p4, plane), beam1, beam2, true, line1, line2));
                                hold = TeklaPH.Line.FindPointOnLine(p2, refference, _DistenceBtwPlates);
                            }
                        }
                    }
                    hold = start_point;
                    switch (_offsetPosition)
                    {
                        case 0:
                            line1 = null; line2 = null; break;
                        case 1:
                            line1 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 4 : 0].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 5 : 7].Face));
                            line2 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 5 : 7].Face), Faces.ConvertFaceToGeometricPlane(beam2_faces[(_Position == 1) ? 4 : 0].Face));
                            break;
                        case 2:
                            line1 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[6].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 5 : 7].Face));
                            line2 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[(_Position == 1) ? 5 : 7].Face), Faces.ConvertFaceToGeometricPlane(beam2_faces[2].Face));
                            break;

                    }
                    if (_LayoutIndex == 0 || _LayoutIndex == 2)
                    {
                        hold = TeklaPH.Line.FindPointOnLine(start_point, refference, _DistenceBtwPlates);
                        int positionIndex = (_Position == 0) ? 7 : 5;
                        GeometricPlane plane = Faces.ConvertFaceToGeometricPlane(beam1_faces[positionIndex].Face);
                        for (int i = 0; i < _NumberOfPlates; i++)
                        {
                            if (Distance.PointToPoint(hold, refference) >= _Width)//doesnot alows the plates to be created out side the beam
                            {
                                p1 = hold;
                                p2 = TeklaPH.Line.FindPointOnLine(p1, refference, _Width);
                                p3 = TeklaPH.Line.FindPerpendicularIntersection(beam1_centerpoints[0] as Point, beam1_centerpoints[1] as Point, p2, beam2_centerpoints[0] as Point, beam2_centerpoints[1] as Point);
                                p4 = TeklaPH.Line.FindPerpendicularIntersection(beam1_centerpoints[0] as Point, beam1_centerpoints[1] as Point, p1, beam2_centerpoints[0] as Point, beam2_centerpoints[1] as Point);
                                plates.Add(countourPlate(Projection.PointToPlane(p1 , plane), Projection.PointToPlane(p2 ,plane), Projection.PointToPlane(p3 , plane), Projection.PointToPlane(p4 , plane), beam1, beam2, false, line1, line2));
                                hold = TeklaPH.Line.FindPointOnLine(p2, refference, _DistenceBtwPlates);
                            }
                        }

                    }
                    weld(beam1, beam2, plates);

                    myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTransformation);
                    return true;

                }
                catch { }
                return false;
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog()
        {

            {
                _Thickness = Data.Thickness;
                _Width = Data.Width;
                _Material = Data.Material;
                _DistenceBtwPlates = Data.DistenceBtwPlates;
                _NumberOfPlates = Data.NumberOfPlates;
                _Name = Data.Name;
                _Finish = Data.Finish;
                _Offset1 = Data.Offset1;
                _Offset2 = Data.Offset2;
                _Reverse = Data.Reverse;
                _LayoutIndex = Data.LayoutIndex;
                _PlateDepth = Data.PlateDepth;
                _Position = Data.Position;
                _offsetPosition = Data.offsetPosition;
                if (IsDefaultValue(_Thickness))
                {
                    _Thickness = 10;
                }
                if (IsDefaultValue(_Width))
                {
                    _Width = 300;
                }
                if (IsDefaultValue(_Material))
                {
                    _Material = "IS2062";
                }
                if (IsDefaultValue(_DistenceBtwPlates))
                {
                    _DistenceBtwPlates = 300;
                }
                if (IsDefaultValue(_NumberOfPlates))
                {
                    _NumberOfPlates = 5;
                }
                if (IsDefaultValue(_Name))
                {
                    _Name = "Batten plate";
                }
                if (IsDefaultValue(_Finish))
                {
                    _Finish = "foo";
                }
                if (IsDefaultValue(_Offset1))
                {
                    _Offset1 = 0;
                }
                if (IsDefaultValue(_Offset2))
                {
                    _Offset2 = 0;
                }
                if (IsDefaultValue(_Reverse))
                {
                    _Reverse = 0;

                }
                if (IsDefaultValue(_LayoutIndex))
                {
                    _LayoutIndex = 1;
                }
                if (IsDefaultValue(_PlateDepth))
                {
                    _PlateDepth = 2;
                }
                if (IsDefaultValue(_Position))
                {
                    _Position = 0;
                }

                if (IsDefaultValue(_offsetPosition))
                {
                    _offsetPosition = 0;
                }
            }
        }

        private ContourPlate countourPlate(Point p1, Point p2, Point p3, Point p4, Beam beam1, Beam beam2, bool flag, Line line1, Line line2)
        {
            if (_Reverse == 1)
                flag = !flag;
            if (_Position == 1)
                flag = !flag;
            if (line1 != null && line2 != null)//sets for the edge position
            {
                p1 = Intersection.LineToLine(line1, new Line(p1, p4)).StartPoint;
                p2 = Intersection.LineToLine(line1, new Line(p2, p3)).StartPoint;
                p3 = Intersection.LineToLine(line2, new Line(p3, p2)).StartPoint;
                p4 = Intersection.LineToLine(line2, new Line(p1, p4)).StartPoint;
            }
            p1 = TeklaPH.Line.FindPointOnLine(p1, p4, _Offset1);//sets for the offset
            p2 = TeklaPH.Line.FindPointOnLine(p2, p3, _Offset1);
            p3 = TeklaPH.Line.FindPointOnLine(p3, p2, _Offset2);
            p4 = TeklaPH.Line.FindPointOnLine(p4, p1, _Offset2);


            ArrayList countp = new ArrayList();

            ContourPoint contourPoint = new ContourPoint(p1, new Chamfer());
            countp.Add(contourPoint);
            contourPoint = new ContourPoint(p2, new Chamfer());
            countp.Add(contourPoint);
            contourPoint = new ContourPoint(p3, new Chamfer());
            countp.Add(contourPoint);
            contourPoint = new ContourPoint(p4, new Chamfer());
            countp.Add(contourPoint);

            ContourPlate cp = new ContourPlate();
            cp.Contour.ContourPoints = countp;

            cp.Profile.ProfileString = "PLT" + _Thickness;

            cp.Material.MaterialString = _Material;
            cp.Class = "4";

            cp.Position.Depth = (!flag) ? Position.DepthEnum.FRONT : DepthEnum.BEHIND;
            cp.Name = _Name;
            cp.Finish = _Finish;
            cp.Position.DepthOffset = 0;
            cp.Insert();


            return cp;

        }

        private void weld(Part part1, Part part2, List<ContourPlate> plates)
        {
            try
            {
                Weld Weld = new Weld();
                foreach (ContourPlate p in plates)
                {

                    Weld.MainObject = p;
                    Weld.SecondaryObject = part1;
                    Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                    Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                    Weld.LengthAbove = 12;
                    Weld.ConnectAssemblies = true;
                    Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_SLOT;
                    Weld.Insert();

                    Weld.SecondaryObject = part2;
                    Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                    Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                    Weld.LengthAbove = 12;
                    Weld.ConnectAssemblies = true;
                    Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_SLOT;
                    Weld.Insert();
                }

                Weld.Modify();
            }
            catch { }
        }
       

        #endregion
    }
}
