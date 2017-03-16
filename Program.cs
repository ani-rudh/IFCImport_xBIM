﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xbim.Ifc;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using System.Text;
using Xbim.XbimExtensions;
using Xbim.Common.XbimExtensions;

namespace xBIM_IFC_Trial
{

    class Program
    {
        const string file = "D:\\THESIS_ANI\\xBIM\\2011-09-14-Duplex-IFC\\Duplex_A_20110907_optimized.ifc";
        //const string file = "D:\\THESIS_ANI\\xBIM\\trial_openings_IFC_2x3coord.ifc";
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\trialbim_simple.ifc";

        public static void Main()
        {

            using (var model = IfcStore.Open(file))
            {

                Console.WriteLine("\n" + "---------------------------------------S T A R T---------------------------------------" + "\n");
                Dictionary<string, IfcSpace> spaceids;
                Dictionary<string, IfcBuildingStorey> storeyids;

                var project = model.Instances.FirstOrDefault<IIfcProject>();

                IEnumerable<IfcSpace> spaces = model.Instances.OfType<IfcSpace>();
                spaceids = getspaceelementids(spaces);

                IEnumerable<IfcBuildingStorey> storeys = model.Instances.OfType<IfcBuildingStorey>();
                storeyids = getstoreyelementids(storeys);

                //PrintHierarchy(project, 0, spaceids, storeyids);


                /************************************************************************/

                //var context = new Xbim3DModelContext(model);
                //context.CreateContext();

                //XbimTessellator tesselator = new XbimTessellator(model, XbimGeometryType.TriangulatedMesh);

                /************************************************************************/

                var context = new Xbim3DModelContext(model);
                context.CreateContext();

                var productshape = context.ShapeInstances();

                var _productShape = productshape.Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded).ToList();

                XbimShapeTriangulation mesh = null;

                Console.WriteLine("No of shape Instances in the model is : " + _productShape.Count());
                foreach (var shapeInstance in _productShape)
                {
                    Console.WriteLine(" \n <-------------------------------shape: #" + shapeInstance.IfcProductLabel + "-------------------------------> \n");

                    var geometry = context.ShapeGeometry(shapeInstance);

                    //Console.WriteLine("====" + geometry.GetType());
                    Console.WriteLine("Geometry Type: " + geometry.Format);

                    var ms = new MemoryStream(((IXbimShapeGeometryData)geometry).ShapeData);
                    var br = new BinaryReader(ms);

                    mesh = br.ReadShapeTriangulation();
                    mesh = mesh.Transform(((XbimShapeInstance)shapeInstance).Transformation);

                    Console.WriteLine("No. of faces on this ShapeInstance: " + mesh.Faces.Count());

                    var facesfound = mesh.Faces.ToList();

                    //var triangleCount = mesh.Faces.Aggregate(face => face.TriangleCount);

                    foreach (var f in facesfound)
                    {
                        Console.WriteLine("Triangle count on face: " + f.GetType() + " :mesh is  " + f.TriangleCount);

                        foreach (var fi in f.Indices)
                        {
                            Console.WriteLine(" -> " + fi);
                        }
                        Console.WriteLine("Indices COUNT ===== " + f.Indices.Count());

                    }

                    Console.WriteLine(" \n Vertices of the Geometry: ");
                    foreach (var v in mesh.Vertices.ToList())
                    {
                        Console.WriteLine(" vertex: " + mesh.Vertices.ToList().IndexOf(v) + " " + v.X + " _ " + v.Y + " _ " + v.Z);

                    }
                }

            }

        }

        private static void PrintHierarchy(IIfcObjectDefinition o, int level, Dictionary<string, IfcSpace> spaceidset, Dictionary<string, IfcBuildingStorey> storeyidset)
        {
            Console.WriteLine($"{GetIndent(level)}{" >> " + o.Name} [{o.GetType().Name}{ " | #" + o.EntityLabel }]");
            var item = o.IsDecomposedBy.SelectMany(r => r.RelatedObjects);

            foreach (var i in item)
            {
                PrintHierarchy(i, level + 2, spaceidset, storeyidset);

                var id = i.GlobalId.ToString();

                if (spaceidset.ContainsKey(id))
                {
                    IfcSpace spacenode;
                    spaceidset.TryGetValue(id, out spacenode);
                    var spacenodelelems = spacenode.GetContainedElements();

                    if (spacenodelelems.Count() > 0)
                    {
                        Console.WriteLine($"{GetIndent(level + 4)}" + "OBJECTS FOUND UNDER SPACE ARE: ");
                        foreach (var sne in spacenodelelems)
                        {
                            var parent = sne.IsContainedIn;

                            Console.WriteLine($"{GetIndent(level + 5)}{" -> " + sne.Name} [{sne.GetType().Name}{ " | #" + sne.EntityLabel }{" | PARENT : #" + parent.EntityLabel}]");
                        }
                    }
                }

                else if (storeyidset.ContainsKey(id))
                {
                    IfcBuildingStorey bsnode;
                    storeyidset.TryGetValue(id, out bsnode);
                    var bsnodelelems = bsnode.GetContainedElements();

                    if (bsnodelelems.Count() > 0)
                    {
                        Console.WriteLine($"{GetIndent(level + 4)}" + "OTHER OBJECTS FOUND UNDER STOREY ARE: ");
                        foreach (var bsne in bsnodelelems)
                        {
                            var parent = bsne.IsContainedIn;

                            Console.WriteLine($"{GetIndent(level + 5)}{" -> " + bsne.Name} [{bsne.GetType().Name}{ " | #" + bsne.EntityLabel } {" | PARENT : #" + parent.EntityLabel}]");

                        }
                    }
                }

            }

        }

        private static string GetIndent(int level)
        {
            var indent = "";
            for (int i = 0; i < level; i++)
                indent += "  ";
            return indent;
        }

        private static Dictionary<string, IfcSpace> getspaceelementids(IEnumerable<IfcSpace> spaces_ien)
        {
            Dictionary<string, IfcSpace> eids = new Dictionary<string, IfcSpace>();
            foreach (IfcSpace s in spaces_ien)
            {
                eids.Add(s.GlobalId.ToString(), s);
                //Console.WriteLine("Gid for " + s.Name + " is: " +s.GlobalId.ToString());
            }

            return eids;
        }

        private static Dictionary<string, IfcBuildingStorey> getstoreyelementids(IEnumerable<IfcBuildingStorey> storeys_ien)
        {
            Dictionary<string, IfcBuildingStorey> eids = new Dictionary<string, IfcBuildingStorey>();
            foreach (IfcBuildingStorey s in storeys_ien)
            {
                eids.Add(s.GlobalId.ToString(), s);
                //Console.WriteLine("Gid for " + s.Name + " is: " +s.GlobalId.ToString());
            }

            return eids;
        }

    }
}