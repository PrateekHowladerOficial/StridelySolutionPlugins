using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using ModelObjectSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using Point = Tekla.Structures.Geometry3d.Point;
using Tekla.Structures.Model.UI;
using Component = Tekla.Structures.Model.Component;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Tekla.Structures.Solid;
using System.Collections;
using static Tekla.Structures.Model.Position;

namespace API__Connection
{
    public partial class Form2 : Form
    {
        Model myModel = new Model();
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = true;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = true;
            checkBox5.Checked = true;
            checkBox6.Checked = false;
        }

       
        class Face_
        {
            public Face Face { get; set; }
            public Vector Vector { get; set; }
            public void face_(Face face, Vector vector)
            {
                Face = face;
                Vector = vector;
            }
        }
        private List<Face_> get_faces(Beam beam)
        {

            Solid solid = beam.GetSolid();
            FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
            List<Face_> faces = new List<Face_>();
            while (faceEnumerator.MoveNext())
            {

                Face face = faceEnumerator.Current as Face;
                Vector vector = face.Normal;
                faces.Add(new Face_ { Face = face, Vector = vector });

            }

            return faces;
        }
        private void countourPlate(List<Face_> face_s)
        {

            foreach (Face_ face in face_s)
            {
                ArrayList points = new ArrayList();
                LoopEnumerator loopEnumerator = face.Face.GetLoopEnumerator();
                while (loopEnumerator.MoveNext())
                {

                    Loop loop = loopEnumerator.Current as Loop;
                    VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                    while (vertexEnumerator.MoveNext())
                    {
                        Point point = vertexEnumerator.Current as Point;
                        points.Add(point);
                    }
                }

                ArrayList countourPoints = new ArrayList();

                foreach (Point point in points)
                {
                    ContourPoint contourPoint = new ContourPoint(point, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                ContourPlate cp = new ContourPlate();
                cp.Contour.ContourPoints = countourPoints;

                cp.Profile.ProfileString = "PLT10";

                cp.Material.MaterialString = "IS2062";
                cp.Class = "4";
                cp.Position.Depth = Position.DepthEnum.BEHIND;
                cp.Insert();

                myModel.CommitChanges();
            }

        }
        private Point midPoint(Point point, Point point1)
        {
            Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
            return mid;
        }
        public static GeometricPlane ConvertFaceToGeometricPlane(Face face)
        {
            ArrayList points = new ArrayList();
            // Get the edges from the face (since 'Points' is not available)
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }

            Point point1 = points[0] as Point;
            Point point2 = points[1] as Point;
            Point point3 = points[2] as Point;



            if (point1 == null || point2 == null || point3 == null)
            {
                throw new ArgumentException("The face does not have sufficient points to define a plane.");
            }

            // Create vectors from the points
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of the two vectors)
            Vector normalVector = Vector.Cross(vector1, vector2);
            normalVector.Normalize();

            // Create the geometric plane using point1 and the normal vector
            GeometricPlane geometricPlane = new GeometricPlane(point1, normalVector);

            return geometricPlane;
        }

        private ArrayList get_points(Face face)
        {
            ArrayList points = new ArrayList();
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }
            return points;
        }
        private void GetFaceAxes(Face face, out Vector xAxis, out Vector yAxis)
        {
            Vector normalVector;
            // Get the loop vertices of the face to extract points
            ArrayList vertices = get_points(face);

            if (vertices == null || vertices.Count < 3)
            {
                throw new ArgumentException("The face does not have enough vertices to define axes.");
            }

            // Select three distinct points to define the plane and axes
            Point point1 = vertices[0] as Point;
            Point point2 = vertices[1] as Point;
            Point point3 = vertices[2] as Point;

            // Define the X-axis vector as the vector between point1 and point2
            xAxis = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            xAxis.Normalize();

            // Define another vector on the face
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of xAxis and vector2)
            normalVector = Vector.Cross(xAxis, vector2);
            normalVector.Normalize();

            // Define the Y-axis vector as the cross product of the normal vector and X-axis vector
            yAxis = Vector.Cross(normalVector, xAxis);
            yAxis.Normalize();
        }
        private double CalculateDistanceBetweenFaces(Face face1, Face face2)
        {
            // Get the loop vertices of both faces to extract points
            ArrayList face1Vertices = get_points(face1);
            ArrayList face2Vertices = get_points(face2);

            if (face1Vertices == null || face1Vertices.Count == 0 || face2Vertices == null || face2Vertices.Count == 0)
            {
                throw new ArgumentException("One or both faces do not have vertices.");
            }

            // Initialize the minimum distance to a large value
            double minDistance = double.MaxValue;

            // Loop through all points on face1 and face2 and calculate the distance between each pair
            foreach (Point p1 in face1Vertices)
            {
                foreach (Point p2 in face2Vertices)
                {
                    double distance = DistanceBetweenPoints(p1, p2);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
        }
        public static Vector GetNormalVector(Point point1, Point point2, Point point3)
        {
            // Step 1: Create two vectors using the points
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Step 2: Compute the cross product of the two vectors to get the normal vector
            Vector normal = Vector.Cross(vector1, vector2);

            // Step 3: Normalize the normal vector (optional)
            normal.Normalize();

            return normal;
        }
        private void weld(Part part, Part part1)
        {
            Weld Weld = new Weld();
            Weld.MainObject = part;
            Weld.SecondaryObject = part1;
            Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld.Insert();

            Weld.LengthAbove = 12;
            Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_SLOT;

            Weld.Modify();

        }
        private void cutpart(Part beam1, List<Point> points, ContourPlate cp)
        {
            double distence = DistanceBetweenPoints(points[0], points[2]);
            List<(Point, Point)> list = new List<(Point, Point)>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (DistanceBetweenPoints(points[i], points[j]) <= distence)
                    {
                        distence = DistanceBetweenPoints(points[i], points[j]);
                        list.Add((points[i], points[j]));
                    }
                }
            }
            if (list.Count > 2)
            {
                list.Remove(list[0]);
            }
            Beam Beam2 = new Beam();

            (Beam2.StartPoint, Beam2.EndPoint) = list[0];
            Beam2.Profile.ProfileString = "PLT" + (double.Parse(textBox6.Text) * 2).ToString() + "*" + (double.Parse(textBox7.Text) * 2).ToString();
            Beam2.Position.Depth = DepthEnum.MIDDLE;
            Beam2.Class = BooleanPart.BooleanOperativeClassName;
            Beam2.Position.Rotation = RotationEnum.FRONT;
            Beam2.Insert();
            Beam Beam3 = new Beam();

            (Beam3.StartPoint, Beam3.EndPoint) = list[1];
            Beam3.Profile.ProfileString = "PLT" + (double.Parse(textBox6.Text) * 2).ToString() + "*" + (double.Parse(textBox7.Text) * 2).ToString();
            Beam3.Position.Depth = DepthEnum.MIDDLE;
            Beam3.Position.Rotation = RotationEnum.FRONT;
            Beam3.Class = BooleanPart.BooleanOperativeClassName;
            Beam3.Insert();

            BooleanPart Beam = new BooleanPart();
            Beam.Father = beam1;
            Beam.SetOperativePart(Beam2);
            // Beam.Type = BooleanPart.BooleanTypeEnum.BOOLEAN_ADD; // BOOLEAN_CUT is default type.
            if (!Beam.Insert())
                Console.WriteLine("Insert failed!");

            Beam.SetOperativePart(Beam3);
            // Beam.Type = BooleanPart.BooleanTypeEnum.BOOLEAN_ADD; // BOOLEAN_CUT is default type.
            if (!Beam.Insert())
                Console.WriteLine("Insert failed!");
            Beam.Father = cp;
            Beam.SetOperativePart(Beam2);
            // Beam.Type = BooleanPart.BooleanTypeEnum.BOOLEAN_ADD; // BOOLEAN_CUT is default type.
            if (!Beam.Insert())
                Console.WriteLine("Insert failed!");

            Beam.SetOperativePart(Beam3);
            // Beam.Type = BooleanPart.BooleanTypeEnum.BOOLEAN_ADD; // BOOLEAN_CUT is default type.
            if (!Beam.Insert())
                Console.WriteLine("Insert failed!");

            Beam2.Delete(); // Not needed when using BOOLEAN_ADD, operative part is deleted automatically.
            Beam3.Delete();
        }
        public static Point FindPointOnLine(Point startPoint, Point secondPoint, double distance)
        {
            // Step 1: Calculate the direction vector from startPoint to secondPoint
            Vector direction = new Vector(
                secondPoint.X - startPoint.X,
                secondPoint.Y - startPoint.Y,
                secondPoint.Z - startPoint.Z
            );

            // Step 2: Normalize the direction vector
            direction.Normalize();

            // Step 3: Scale the direction vector by the distance
            Vector scaledVector = new Vector(
                direction.X * distance,
                direction.Y * distance,
                direction.Z * distance
            );

            // Step 4: Calculate the new point by adding the scaled vector to the start point
            Point newPoint = new Point(
                startPoint.X + scaledVector.X,
                startPoint.Y + scaledVector.Y,
                startPoint.Z + scaledVector.Z
            );

            return newPoint;
        }
        private void boltArray(ContourPlate cp, Beam beam, List<Point> points)
        {

            BoltArray bA = new BoltArray();
            bA.PartToBeBolted = cp;
            bA.PartToBoltTo = beam;
            double distencemin = 0, distancemax = 0;
            Point hold = points[0], max = new Point(), min = new Point();
            for (int i = 1; i < points.Count; i++)
            {
                if (distencemin > DistanceBetweenPoints(hold, points[i]) || distencemin == 0.0)
                {
                    min = points[i];
                    distencemin = DistanceBetweenPoints(hold, points[i]);
                }
                if (distancemax < DistanceBetweenPoints(hold, points[i]))
                {
                    max = points[i];
                    distancemax = DistanceBetweenPoints(hold, points[i]);
                }

            }
            Vector vector = GetNormalVector(min, max, hold);




            bA.BoltSize = 15.87;
            bA.Tolerance = 3.0;
            bA.BoltStandard = "8.8XOX";
            bA.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            bA.Length = 100;
            bA.ThreadInMaterial = BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES;

            bA.Position.Depth = Position.DepthEnum.FRONT;
            bA.Position.Plane = Position.PlaneEnum.MIDDLE;
            bA.Position.Rotation = (vector.X == 1 || vector.X == -1 || vector.Z == 1) ? RotationEnum.FRONT : RotationEnum.TOP;

            bA.Bolt = checkBox1.Checked;
            bA.Washer1 = checkBox2.Checked;
            bA.Washer2 = checkBox3.Checked;
            bA.Washer3 = checkBox4.Checked;
            bA.Nut1 = checkBox5.Checked;
            bA.Nut2 = checkBox6.Checked;
            double total = 0;
            for (int i = 1; i < int.Parse(textBox4.Text); i++)
            {
                bA.AddBoltDistX(double.Parse(textBox5.Text));
                total += double.Parse(textBox5.Text);
            }
            bA.StartPointOffset.Dx = 0;


            for (int i = 1; i < int.Parse(textBox3.Text); i++)
            {
                bA.AddBoltDistY(double.Parse(textBox2.Text));
            }
            bA.EndPointOffset.Dx = 0;
            bA.FirstPosition = FindPointOnLine(midPoint(hold, max), midPoint(hold, min), total / 2);
            bA.SecondPosition = midPoint(hold, max);
            if (!bA.Insert())
            {
                MessageBox.Show("BoltArray Insert failed ");
            }
        }

        // Method to calculate the distance between two points
        private static double DistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) +
                             Math.Pow(point2.Y - point1.Y, 2) +
                             Math.Pow(point2.Z - point1.Z, 2));
        }


        private void joint_click(object sender, EventArgs e)
        
        {
            if (myModel.GetConnectionStatus())
            {
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                myModel.CommitChanges();
                List<Point> bolt_points = new List<Point>();
                Picker picker = new Picker();
                Beam beam1 = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select the pimary object") as Beam;
                Beam beam2 = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Select the secondary object") as Beam;
                ArrayList centerLine1 = beam1.GetCenterLine(false), centerLine2 = beam2.GetCenterLine(false);
                List<Face_> beam1_faces = get_faces(beam1);
                Point mid = midPoint(centerLine2[0] as Point, centerLine2[1] as Point);
                double distance = 0, hold = 0;
                Face_ beam1_inter_face = null;
                bool flag_cut = false;
                foreach (Face_ face in beam1_faces)
                {
                    GeometricPlane geometricPlane = ConvertFaceToGeometricPlane(face.Face);
                    LineSegment lineSegment = new LineSegment(centerLine2[0] as Point, centerLine2[1] as Point);
                    Point intersectingPoint = Intersection.LineSegmentToPlane(lineSegment, geometricPlane);
                    if (intersectingPoint != null)
                    {
                        distance = Distance.PointToPoint(intersectingPoint, mid);
                        if ((distance < hold || hold == 0.0) && !new List<int> { 1, 3, 7, 9, 0, 10, 6, 4 }.Contains(beam1_faces.IndexOf(face)))
                        {
                            hold = distance;
                            beam1_inter_face = face;
                            if (new List<int> { 2, 8 }.Contains(beam1_faces.IndexOf(face)))
                                flag_cut = true;
                        }
                    }

                }
                Fitting fitting = new Fitting();
                fitting.Plane = new Plane();
                Vector vector = beam1_inter_face.Vector;
                Point point = get_points(beam1_inter_face.Face)[0] as Point;
                double gap = double.Parse(textBox1.Text.ToString());
                Point point1 = new Point(point.X + gap * vector.X, point.Y + gap * vector.Y, point.Z + gap * vector.Z);
                fitting.Plane.Origin = point1;
                GetFaceAxes(beam1_inter_face.Face, out Vector xAxis, out Vector yAxis);
                fitting.Plane.AxisX = xAxis;
                fitting.Plane.AxisY = yAxis;
                fitting.Father = beam2;
                fitting.Insert();
                List<Face_> beam2_faces = get_faces(beam2);
                Face face1 = null;
                if (CalculateDistanceBetweenFaces(beam1_inter_face.Face, beam2_faces[12].Face) < CalculateDistanceBetweenFaces(beam1_inter_face.Face, beam2_faces[13].Face))
                {
                    face1 = beam2_faces[12].Face;
                    ArrayList points = get_points(face1);
                    ArrayList countourPoints = new ArrayList();

                    foreach (Point p in points)
                    {
                        if (new List<int> { 8, 1, 2, 7 }.Contains(points.IndexOf(p)))
                        {
                            bolt_points.Add(p);
                            ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                            countourPoints.Add(contourPoint);
                        }
                    }
                    ContourPlate cp = new ContourPlate();
                    cp.Contour.ContourPoints = countourPoints;

                    cp.Profile.ProfileString = "PLT" + textBox1.Text;

                    cp.Material.MaterialString = "IS2062";
                    cp.Class = "4";
                    cp.Position.Depth = Position.DepthEnum.BEHIND;
                    cp.Insert();
                    boltArray(cp, beam1, bolt_points);
                    weld(beam2, cp);
                    if (flag_cut)
                        cutpart(beam2, bolt_points, cp);

                }
                else
                {
                    face1 = beam2_faces[13].Face;
                    ArrayList points = get_points(face1);
                    ArrayList countourPoints = new ArrayList();

                    foreach (Point p in points)
                    {
                        if (new List<int> { 0, 5, 6, 11 }.Contains(points.IndexOf(p)))
                        {
                            bolt_points.Add(p);
                            ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                            countourPoints.Add(contourPoint);
                        }
                    }
                    ContourPlate cp = new ContourPlate();
                    cp.Contour.ContourPoints = countourPoints;

                    cp.Profile.ProfileString = "PLT" + textBox1.Text;

                    cp.Material.MaterialString = "IS2062";
                    cp.Class = "4";
                    cp.Position.Depth = Position.DepthEnum.BEHIND;
                    cp.Insert();
                    boltArray(cp, beam1, bolt_points);
                    weld(beam2, cp);
                    if (flag_cut)
                        cutpart(beam2, bolt_points, cp);
                }

                myModel.CommitChanges();

                MessageBox.Show(beam1_faces.IndexOf(beam1_inter_face).ToString());




            }
        }
    }
}
