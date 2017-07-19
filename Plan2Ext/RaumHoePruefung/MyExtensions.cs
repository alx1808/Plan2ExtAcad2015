//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
  using Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.RaumHoePruefung
{
    internal static class MyExtensions
    {
        public static double GetElevation(this Entity ent)
        {
            throw new InvalidOperationException("\nElement is no Polyline!");
        }
        public static double GetElevation(this Polyline pl)
        {
            return pl.Elevation;
        }
        public static double GetElevation(this Polyline2d pl)
        {
            double? height = null;
            using (var myTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                foreach (ObjectId oid in pl)
                {
                    Vertex2d v2d = myTrans.GetObject(oid, OpenMode.ForRead) as Vertex2d;
                    if (v2d != null)
                    {
                        height = v2d.Position.Z;
                    }
                    break;
                }

                myTrans.Commit();
            }
            if (height.HasValue) return height.Value;

            throw new InvalidOperationException("\nUngültige 2d-Polylinie!");
        }

        public static double GetElevation(this Polyline3d pl)
        {
            double? height = null;
            using (var myTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                foreach (ObjectId oid in pl)
                {
                    PolylineVertex3d v2d = myTrans.GetObject(oid, OpenMode.ForRead) as PolylineVertex3d;
                    if (v2d != null)
                    {
                        height = v2d.Position.Z;
                    }
                    break;
                }

                myTrans.Commit();
            }
            if (height.HasValue) return height.Value;

            throw new InvalidOperationException("\nUngültige 3d-Polylinie!");
        }

    }
}
