﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.UtilityResource;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc4;
using Xbim.Ifc4.UtilityResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Logging;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation.ModelGeomInfo;


namespace xBIM_IFC_Trial
{
    //    class Program
    //    {
    //        static string file = "F:/TUM/STUDIES/THESIS/BIMtoUnity/xBIM/IFC_samplefiles/2011-09-14-Clinic-IFC/Clinic_A_20110906_optimized.ifc";

    //        static void Main()
    //        {
    //            using (var model = IfcStore.Open(file))
    //            {
    //                Console.WriteLine("_____________________S T A R T________________________");

    //                Console.WriteLine("MODEL WAS FOUND: " + model.FileName);
    //                Console.WriteLine(" NUMBER OF INSTANCES: " + model.Instances.Count());
    //                Console.WriteLine(" NUMBER OF site INSTANCES: " + model.Instances.CountOf<IIfcSite>());
    //                Console.WriteLine(" NUMBER OF wall INSTANCES: " + model.Instances.CountOf<IIfcWallStandardCase>());

    //                var firstelem = model.Instances.FirstOrDefault<IIfcObjectDefinition>();

    //                Console.WriteLine( firstelem.Name +"[ " + firstelem.GetType().Name +" ]");

    //                //getHierarchy(firstelem);

    //                Console.WriteLine("_____________________________________________");
    //            }
    //        }

    //        public static void getHierarchy(IfcSpatialStructureElement mainnode)
    //        {
    //            Console.WriteLine(mainnode.Name + "[ " + mainnode.GetType().Name + " ]");
    //            List<IfcProduct> elems = new List<IfcProduct>();
    //            foreach (IfcSpatialStructureElement e in mainnode.GetContainedElements())
    //            {
    //                Console.WriteLine("---" + e.Name);
    //                elems = e.GetContainedElements().ToList();
    //                getHierarchy(e);
    //            }

    //        }

    //    }
    //}

    //var spacelist = space.ToList();

    //foreach (var s in space)
    //for (int i = 0; i < spacelist.Count(); ++i)
    //{
    //    //Console.WriteLine("SPACE: " + spacelist[i].Name );
    //    Console.WriteLine(" \n" + "xxxxxxxxxxxxxxxxxxxxxxxx " + spacelist[i].Name + " || " + spacelist[i].GlobalId + " || " + spacelist[i].GetType() + " xxxxxxxxxxxxxxxxxxxxxxxx ");
    //    //var elements = spacelist[i].ContainsElements;
    //    var elements = spacelist[i].GetContainedElements();
    //    Console.WriteLine("-No. of contained elements: " + elements.Count());
    //    if (elements.Count() > 0)
    //    {
    //        Console.WriteLine("FOLLOWING ARE THE ELEMENTS FOUND:");
    //        foreach (var e in elements)
    //        {

    //            Console.WriteLine("--ELEMENT : " + e.Name + " // TYPE: " + e.GetType() + " // with GUID: " + e.GlobalId);
    //        }

    //    }

    //}


    /*******************************************************************************************************************************************************************************/

    class Program
    {
        const string file = "F:/TUM/STUDIES/THESIS/BIMtoUnity/xBIM/IFC_samplefiles/2011-09-14-Clinic-IFC/trialbim_simple.ifc";
        
        public static void Main()
        {

            
            using (var model = IfcStore.Open(file))
            {

                Console.WriteLine("\n" + "---------------------------------------S T A R T---------------------------------------" + "\n");
                Dictionary<string, IfcSpace> spaceids;
                Dictionary<string, IfcBuildingStorey> storeyids;

                var project = model.Instances.FirstOrDefault<IIfcProject>();

                var be = model.Instances.OfType<IIfcBuildingStorey>();
                Console.WriteLine("buildingelems : " + be.Count());
               


                TransformGraph tgraph = new TransformGraph(model);
                tgraph.AddProducts(be);
                var nodes = tgraph.ProductNodes;
                //nodes.Where(x => x.GetType() == )
                var rootnode = tgraph.Root;
                Console.WriteLine("ROOT : " + rootnode.ProductLabel);

                foreach (TransformNode tn in nodes.Values)
                {
                    Console.WriteLine(tn.ToString() +  tn.ProductLabel);
                    foreach (var n in tn.Children)
                    {
                        Console.WriteLine(n.ToString() + n.ProductLabel);
                    }
                }
                

                //buildhierarchy(rootnode, 1);

                IEnumerable<IfcSpace> spaces = model.Instances.OfType<IfcSpace>();
                spaceids = getspaceelementids(spaces);

                IEnumerable<IfcBuildingStorey> storeys = model.Instances.OfType<IfcBuildingStorey>();
                storeyids = getstoreyelementids(storeys);

                //PrintHierarchy(project, 0, spaceids, storeyids);

                /************************************************************************/
                
            }


        }
    
        private static void PrintHierarchy(IIfcObjectDefinition o, int level, Dictionary<string, IfcSpace> spaceidset, Dictionary<string, IfcBuildingStorey> storeyidset)
        {
            Console.WriteLine($"{GetIndent(level)}{">" + o.Name} [{o.GetType().Name}]");
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
                        Console.WriteLine($"{GetIndent(level + 3)}" + "OBJECTS FOUND ARE: ");
                        foreach (var sne in spacenodelelems)
                        {
                            Console.WriteLine($"{GetIndent(level + 4)}{"->" + sne.Name} [{sne.GetType().Name}]");
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
                        Console.WriteLine($"{GetIndent(level + 3)}" + "OTHER OBJECTS FOUND UNDER STOREY ARE: ");
                        foreach (var bsne in bsnodelelems)
                        {
                            
                            Console.WriteLine($"{GetIndent(level + 4)}{"->" + bsne.Name} [{bsne.GetType().Name}]{ " TRANSLATION: "}");
                            
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

        private static Dictionary<string,IfcSpace> getspaceelementids (IEnumerable<IfcSpace> spaces_ien)
        {
            Dictionary<string, IfcSpace> eids = new Dictionary<string, IfcSpace>();
            foreach ( IfcSpace s in spaces_ien)
            {
                eids.Add(s.GlobalId.ToString(),s);
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

        private static void buildhierarchy(TransformNode mnode, int level)
        {
            //Console.WriteLine($"{GetIndent(level)}{mnode.ProductLabel} [{mnode.GetType().Name}]");
            //Console.WriteLine("ROOT: " + mnode.ProductLabel);
            if (mnode.ChildCount > 0)
            {
                List<TransformNode> child = mnode.Children.ToList();

                foreach (TransformNode c in child)
                {
                    Console.WriteLine($"{GetIndent(level)}{mnode.ProductLabel} [{mnode.GetType().Name}]");
                    //Console.WriteLine(c.ProductLabel);
                    buildhierarchy(c, 2);
                }

            }
        }
    }
}