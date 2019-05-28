using Interop.Gsa_10_0;
using SpeckleCore;
using SpeckleElements;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SpeckleElementsGSA
{
    public class GSAInterfacer
    {
        public ComAuto GSAObject;
        
        public GSAResultMode ResultMode;

        public Indexer Indexer = new Indexer();

        public double CoincidentNodeAllowance = 0.1;

        private Dictionary<string, object> PreviousGSAGetCache = new Dictionary<string, object>();
        private Dictionary<string, object> GSAGetCache = new Dictionary<string, object>();

        private Dictionary<string, object> PreviousGSASetCache = new Dictionary<string, object>();
        private Dictionary<string, object> GSASetCache = new Dictionary<string, object>();
        
        #region GWA Command
        /// <summary>
        /// Returns a list of GWA records with the index of the record prepended.
        /// </summary>
        /// <param name="command">GET GWA command</param>
        /// <returns>Array of GWA records</returns>
        public string[] GetGWARecords(string command)
        {
            if (!command.StartsWith("GET"))
                throw new Exception("GetGWAGetCommands() only takes in GET commands");

            object result = RunGWACommand(command);
            string[] newPieces = ((string)result).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s, idx) => idx.ToString() + ":" + s).ToArray();
            return newPieces;
        }

        /// <summary>
        /// Returns a list of new GWA records with the index of the record prepended.
        /// </summary>
        /// <param name="command">GET GWA command</param>
        /// <returns>Array of GWA records</returns>
        public string[] GetNewGWARecords(string command)
        {
            if (!command.StartsWith("GET"))
                throw new Exception("GetNewGWAGetCommands() only takes in GET commands");

            object result = RunGWACommand(command);

            if (PreviousGSAGetCache.ContainsKey(command))
            {
                if ((result as string) == (PreviousGSAGetCache[command] as string))
                    return new string[0];

                string[] newPieces = ((string)result).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s,idx) => idx.ToString() + ":" + s).ToArray();
                string[] prevPieces = ((string)PreviousGSAGetCache[command]).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s, idx) => idx.ToString() + ":" + s).ToArray();

                string[] ret = newPieces.Where(n => !prevPieces.Contains(n)).ToArray();

                return ret;
            }
            else
            {
                return ((string)result).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s, idx) => idx.ToString() + ":" + s).ToArray();
            }
        }

        /// <summary>
        /// Returns a list of deleted GWA records with the index of the record prepended.
        /// </summary>
        /// <param name="command">GET GWA command</param>
        /// <returns>Array of GWA records</returns>
        public string[] GetDeletedGWARecords(string command)
        {
            if (!command.Contains("GET"))
                throw new Exception("GetDeletedGWAGetCommands() only takes in GET commands");

            object result = RunGWACommand(command);

            if (PreviousGSAGetCache.ContainsKey(command))
            {
                if ((result as string) == (PreviousGSAGetCache[command] as string))
                    return new string[0];

                string[] newPieces = ((string)result).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s, idx) => idx.ToString() + ":" + s).ToArray();
                string[] prevPieces = ((string)PreviousGSAGetCache[command]).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);//.Select((s, idx) => idx.ToString() + ":" + s).ToArray();

                string[] ret = prevPieces.Where(p => !newPieces.Contains(p)).ToArray();

                return ret;
            }
            else
                return new string[0];
        }

        /// <summary>
        /// Runs a GWA command with the option to cache GET and SET commands.
        /// </summary>
        /// <param name="command">GWA command</param>
        /// <param name="cache">Use cache</param>
        /// <returns>GWA command return object</returns>
        public object RunGWACommand(string command, bool cache = true)
        {
            if (cache)
            {
                if (command.StartsWith("GET"))
                {
                    if (!GSAGetCache.ContainsKey(command))
                    {
                        if (command.StartsWith("GET_ALL,MEMB"))
                        {
                            // TODO: Member GET_ALL work around
                            int[] memberRefs = new int[0];
                            GSAObject.EntitiesInList("all", GsaEntity.MEMBER, out memberRefs);

                            if (memberRefs == null || memberRefs.Length == 0)
                                return "";

                            List<string> result = new List<string>();

                            foreach (int r in memberRefs)
                                (result as List<string>).Add((string)RunGWACommand("GET,MEMB," + r.ToString()));

                            GSAGetCache[command] = string.Join("\n", result);
                        }
                        else if (command.StartsWith("GET_ALL,ANAL"))
                        {
                            // TODO: Anal GET_ALL work around
                            int highestRef = (int)RunGWACommand("HIGHEST,ANAL.1");

                            List<string> result = new List<string>();

                            for (int i = 1; i <= highestRef; i++)
                            {
                                string res = (string)RunGWACommand("GET,ANAL," + i.ToString());
                                if (res != null && res != "")
                                    (result as List<string>).Add(res);
                            }

                            GSAGetCache[command] = string.Join("\n", result);
                        }
                        else
                        {
                            GSAGetCache[command] = GSAObject.GwaCommand(command);
                        }
                    }

                    return GSAGetCache[command];
                }

                if (command.StartsWith("SET"))
                {
                    if (PreviousGSASetCache.ContainsKey(command))
                        GSASetCache[command] = PreviousGSASetCache[command];

                    if (!GSASetCache.ContainsKey(command))
                        GSASetCache[command] = GSAObject.GwaCommand(command);

                    return GSASetCache[command];
                }
            }

            return GSAObject.GwaCommand(command);
        }

        /// <summary>
        /// BLANKS all SET GWA records which are in the previous cache, but not the current cache.
        /// </summary>
        public void BlankDepreciatedGWASetCommands()
        {
            List<string> prevSets = PreviousGSASetCache.Keys.Where(l => l.StartsWith("SET")).ToList();

            for (int i = 0; i < prevSets.Count(); i++)
            {
                string[] split = Regex.Replace(prevSets[i], ":{speckle_app_id:.*}", "").ListSplit("\t");
                prevSets[i] = split[1] + "\t" + split[2] + "\t";
            }

            prevSets = prevSets.Where(l => !GSASetCache.Keys.Any(x => Regex.Replace(x, ":{speckle_app_id:.*}", "").Contains(l))).ToList();

            for (int i = 0; i < prevSets.Count(); i++)
            {
                string p = prevSets[i];

                string[] split = p.ListSplit("\t");

                if (split[1].IsDigits())
                {
                    // Uses SET
                    if (!Indexer.InBaseline(split[0], Convert.ToInt32(split[1])))
                        RunGWACommand("BLANK\t" + split[0] + "\t" + split[1], false);
                }
                else
                {
                    // Uses SET_AT
                    if (!Indexer.InBaseline(split[1], Convert.ToInt32(split[0])))
                    {
                        RunGWACommand("DELETE\t" + split[1] + "\t" + split[0], false);
                        int idxShifter = Convert.ToInt32(split[0]) + 1;
                        bool flag = false;
                        while (!flag)
                        {
                            flag = true;

                            prevSets = prevSets
                                .Select(line => line.Replace(
                                    idxShifter.ToString() + "\t" + split[1] + "\t",
                                    (idxShifter - 1).ToString() + "\t" + split[1] + "\t")).ToList();

                            string target = "\t" + idxShifter.ToString() + "\t" + split[1] + "\t";
                            string rep = "\t" + (idxShifter - 1).ToString() + "\t" + split[1] + "\t";

                            string prevCacheKey = PreviousGSASetCache.Keys
                                .FirstOrDefault(x => x.Contains(target));
                            if (prevCacheKey != null)
                            {
                                PreviousGSASetCache[prevCacheKey.Replace(target, rep)] = PreviousGSASetCache[prevCacheKey];
                                PreviousGSASetCache.Remove(prevCacheKey);
                                flag = false;
                            }

                            string currCacheKey = GSASetCache.Keys
                                .FirstOrDefault(x => x.Contains(target));
                            if (currCacheKey != null)
                            {
                                GSASetCache[currCacheKey.Replace(target, rep)] = GSASetCache[currCacheKey];
                                GSASetCache.Remove(currCacheKey);
                                flag = false;
                            }

                            idxShifter++;
                        }
                    }
                }
            }
        }
        #endregion

        #region Nodes
        /// <summary>
        /// Create new node at the coordinate. If a node already exists, no new nodes are created. Updates Indexer with the index.
        /// </summary>
        /// <param name="x">X coordinate of the node</param>
        /// <param name="y">Y coordinate of the node</param>
        /// <param name="z">Z coordinate of the node</param>
        /// <param name="structuralID">Structural ID of the node</param>
        /// <returns>Node index</returns>
        public int NodeAt(double x, double y, double z, string structuralID = null)
        {
            int idx = GSAObject.Gen_NodeAt(x, y, z, CoincidentNodeAllowance);

            if (structuralID != null)
                Indexer.ReserveIndicesAndMap(typeof(GSANode), new List<int>() { idx }, new List<string>() { structuralID });
            else
                Indexer.ReserveIndices(typeof(GSANode), new List<int>() { idx });

            // Add artificial cache
            string cacheKey = "SET\t" + typeof(GSANode).GetGSAKeyword() + "\t" + idx.ToString() + "\t";
            if (!GSASetCache.ContainsKey(cacheKey))
                GSASetCache[cacheKey] = 0;

            return idx;
        }
        #endregion

        #region Axis
        /// <summary>
        /// Set the structural axis in GSA and returns the index of the axis.
        /// Returns 0 for global axis.
        /// </summary>
        /// <param name="axis">Axis to set</param>
        /// <returns>Index of axis</returns>
        public int SetAxis(StructuralAxis axis)
        {
            if (axis.Xdir.Value.SequenceEqual(new double[] { 1, 0, 0 }) &&
                axis.Ydir.Value.SequenceEqual(new double[] { 0, 1, 0 }) &&
                axis.Normal.Value.SequenceEqual(new double[] { 0, 0, 1 }))
                return 0;

            List<string> ls = new List<string>();

            int res = Indexer.ResolveIndex("AXIS");

            ls.Add("SET");
            ls.Add("AXIS");
            ls.Add(res.ToString());
            ls.Add("");
            ls.Add("CART");

            ls.Add("0");
            ls.Add("0");
            ls.Add("0");

            ls.Add(axis.Xdir.Value[0].ToString());
            ls.Add(axis.Xdir.Value[1].ToString());
            ls.Add(axis.Xdir.Value[2].ToString());

            ls.Add(axis.Ydir.Value[0].ToString());
            ls.Add(axis.Ydir.Value[1].ToString());
            ls.Add(axis.Ydir.Value[2].ToString());

            RunGWACommand(string.Join("\t", ls));

            return res;
        }

        /// <summary>
        /// Calculates the local axis of a 1D entity.
        /// </summary>
        /// <param name="coor">Entity coordinates</param>
        /// <param name="zAxis">Z axis of the 1D entity</param>
        /// <returns>Axis</returns>
        public StructuralAxis LocalAxisEntity1D(double[] coor, StructuralVectorThree zAxis)
        {
            Vector3D axisX = new Vector3D(coor[3] - coor[0], coor[4] - coor[1], coor[5] - coor[2]);
            Vector3D axisZ = new Vector3D(zAxis.Value[0], zAxis.Value[1], zAxis.Value[2]);
            Vector3D axisY = Vector3D.CrossProduct(axisZ, axisX);

            StructuralAxis axis = new StructuralAxis(
                new StructuralVectorThree(new double[] { axisX.X, axisX.Y, axisX.Z }),
                new StructuralVectorThree(new double[] { axisY.X, axisY.Y, axisY.Z }),
                new StructuralVectorThree(new double[] { axisZ.X, axisZ.Y, axisZ.Z })
            );
            axis.Normalize();
            return axis;
        }

        /// <summary>
        /// Calculates the local axis of a point from a GSA node axis.
        /// </summary>
        /// <param name="axis">ID of GSA node axis</param>
        /// <param name="gwaRecord">GWA record of AXIS if used</param>
        /// <param name="evalAtCoor">Coordinates to evaluate axis at</param>
        /// <returns>Axis</returns>
        public StructuralAxis Parse0DAxis(int axis, out string gwaRecord, double[] evalAtCoor = null)
        {
            Vector3D x;
            Vector3D y;
            Vector3D z;

            gwaRecord = null;

            switch (axis)
            {
                case 0:
                    // Global
                    return new StructuralAxis(
                        new StructuralVectorThree(new double[] { 1, 0, 0 }),
                        new StructuralVectorThree(new double[] { 0, 1, 0 }),
                        new StructuralVectorThree(new double[] { 0, 0, 1 })
                    );
                case -11:
                    // X elevation
                    return new StructuralAxis(
                        new StructuralVectorThree(new double[] { 0, -1, 0 }),
                        new StructuralVectorThree(new double[] { 0, 0, 1 }),
                        new StructuralVectorThree(new double[] { -1, 0, 0 })
                    );
                case -12:
                    // Y elevation
                    return new StructuralAxis(
                        new StructuralVectorThree(new double[] { 1, 0, 0 }),
                        new StructuralVectorThree(new double[] { 0, 0, 1 }),
                        new StructuralVectorThree(new double[] { 0, -1, 0 })
                    );
                case -14:
                    // Vertical
                    return new StructuralAxis(
                        new StructuralVectorThree(new double[] { 0, 0, 1 }),
                        new StructuralVectorThree(new double[] { 1, 0, 0 }),
                        new StructuralVectorThree(new double[] { 0, 1, 0 })
                    );
                case -13:
                    // Global cylindrical
                    x = new Vector3D(evalAtCoor[0], evalAtCoor[1], 0);
                    x.Normalize();
                    z = new Vector3D(0, 0, 1);
                    y = Vector3D.CrossProduct(z, x);

                    return new StructuralAxis(
                        new StructuralVectorThree(new double[] { x.X, x.Y, x.Z }),
                        new StructuralVectorThree(new double[] { y.X, y.Y, y.Z }),
                        new StructuralVectorThree(new double[] { z.X, z.Y, z.Z })
                    );
                default:
                    string res = GetGWARecords("GET,AXIS," + axis.ToString()).FirstOrDefault();
                    gwaRecord = res;

                    string[] pieces = res.Split(new char[] { ',' });
                    if (pieces.Length < 13)
                    {
                        return new StructuralAxis(
                            new StructuralVectorThree(new double[] { 1, 0, 0 }),
                            new StructuralVectorThree(new double[] { 0, 1, 0 }),
                            new StructuralVectorThree(new double[] { 0, 0, 1 })
                        );
                    }
                    Vector3D origin = new Vector3D(Convert.ToDouble(pieces[4]), Convert.ToDouble(pieces[5]), Convert.ToDouble(pieces[6]));

                    Vector3D X = new Vector3D(Convert.ToDouble(pieces[7]), Convert.ToDouble(pieces[8]), Convert.ToDouble(pieces[9]));
                    X.Normalize();


                    Vector3D Yp = new Vector3D(Convert.ToDouble(pieces[10]), Convert.ToDouble(pieces[11]), Convert.ToDouble(pieces[12]));
                    Vector3D Z = Vector3D.CrossProduct(X, Yp);
                    Z.Normalize();

                    Vector3D Y = Vector3D.CrossProduct(Z, X);

                    Vector3D pos = new Vector3D(0, 0, 0);

                    if (evalAtCoor == null)
                        pieces[3] = "CART";
                    else
                    {
                        pos = new Vector3D(evalAtCoor[0] - origin.X, evalAtCoor[1] - origin.Y, evalAtCoor[2] - origin.Z);
                        if (pos.Length == 0)
                            pieces[3] = "CART";
                    }

                    switch (pieces[3])
                    {
                        case "CART":
                            return new StructuralAxis(
                                new StructuralVectorThree(new double[] { X.X, X.Y, X.Z }),
                                new StructuralVectorThree(new double[] { Y.X, Y.Y, Y.Z }),
                                new StructuralVectorThree(new double[] { Z.X, Z.Y, Z.Z })
                            );
                        case "CYL":
                            x = new Vector3D(pos.X, pos.Y, 0);
                            x.Normalize();
                            z = Z;
                            y = Vector3D.CrossProduct(Z, x);
                            y.Normalize();

                            return new StructuralAxis(
                                new StructuralVectorThree(new double[] { x.X, x.Y, x.Z }),
                                new StructuralVectorThree(new double[] { y.X, y.Y, y.Z }),
                                new StructuralVectorThree(new double[] { z.X, z.Y, z.Z })
                            );
                        case "SPH":
                            x = pos;
                            x.Normalize();
                            z = Vector3D.CrossProduct(Z, x);
                            z.Normalize();
                            y = Vector3D.CrossProduct(z, x);
                            z.Normalize();

                            return new StructuralAxis(
                                new StructuralVectorThree(new double[] { x.X, x.Y, x.Z }),
                                new StructuralVectorThree(new double[] { y.X, y.Y, y.Z }),
                                new StructuralVectorThree(new double[] { z.X, z.Y, z.Z })
                            );
                        default:
                            return new StructuralAxis(
                                new StructuralVectorThree(new double[] { 1, 0, 0 }),
                                new StructuralVectorThree(new double[] { 0, 1, 0 }),
                                new StructuralVectorThree(new double[] { 0, 0, 1 })
                            );
                    }
            }
        }

        /// <summary>
        /// Calculates the local axis of a 1D entity.
        /// </summary>
        /// <param name="coor">Entity coordinates</param>
        /// <param name="rotationAngle">Angle of rotation from default axis</param>
        /// <param name="orientationNode">Node to orient axis to</param>
        /// <returns>Axis</returns>
        public StructuralAxis Parse1DAxis(double[] coor, double rotationAngle = 0, double[] orientationNode = null)
        {
            Vector3D x;
            Vector3D y;
            Vector3D z;

            x = new Vector3D(coor[3] - coor[0], coor[4] - coor[1], coor[5] - coor[2]);
            x.Normalize();

            if (orientationNode == null)
            {
                if (x.X == 0 && x.Y == 0)
                {
                    //Column
                    y = new Vector3D(0, 1, 0);
                    z = Vector3D.CrossProduct(x, y);
                }
                else
                {
                    //Non-Vertical
                    Vector3D Z = new Vector3D(0, 0, 1);
                    y = Vector3D.CrossProduct(Z, x);
                    y.Normalize();
                    z = Vector3D.CrossProduct(x, y);
                    z.Normalize();
                }
            }
            else
            {
                Vector3D Yp = new Vector3D(orientationNode[0], orientationNode[1], orientationNode[2]);
                z = Vector3D.CrossProduct(x, Yp);
                z.Normalize();
                y = Vector3D.CrossProduct(z, x);
                y.Normalize();
            }

            //Rotation
            Matrix3D rotMat = HelperClass.RotationMatrix(x, rotationAngle.ToRadians());
            y = Vector3D.Multiply(y, rotMat);
            z = Vector3D.Multiply(z, rotMat);

            return new StructuralAxis(
                new StructuralVectorThree(new double[] { x.X, x.Y, x.Z }),
                new StructuralVectorThree(new double[] { y.X, y.Y, y.Z }),
                new StructuralVectorThree(new double[] { z.X, z.Y, z.Z })
            );
        }

        /// <summary>
        /// Calculates the local axis of a 2D entity.
        /// </summary>
        /// <param name="coor">Entity coordinates</param>
        /// <param name="rotationAngle">Angle of rotation from default axis</param>
        /// <param name="isLocalAxis">Is axis calculated from local coordinates?</param>
        /// <returns>Axis</returns>
        public StructuralAxis Parse2DAxis(double[] coor, double rotationAngle = 0, bool isLocalAxis = false)
        {
            Vector3D x;
            Vector3D y;
            Vector3D z;

            List<Vector3D> nodes = new List<Vector3D>();

            for (int i = 0; i < coor.Length; i += 3)
                nodes.Add(new Vector3D(coor[i], coor[i + 1], coor[i + 2]));

            if (isLocalAxis)
            {
                if (nodes.Count == 3)
                {
                    x = Vector3D.Subtract(nodes[1], nodes[0]);
                    x.Normalize();
                    z = Vector3D.CrossProduct(x, Vector3D.Subtract(nodes[2], nodes[0]));
                    z.Normalize();
                    y = Vector3D.CrossProduct(z, x);
                    y.Normalize();
                }
                else if (nodes.Count == 4)
                {
                    x = Vector3D.Subtract(nodes[2], nodes[0]);
                    x.Normalize();
                    z = Vector3D.CrossProduct(x, Vector3D.Subtract(nodes[3], nodes[1]));
                    z.Normalize();
                    y = Vector3D.CrossProduct(z, x);
                    y.Normalize();
                }
                else
                {
                    // Default to QUAD method
                    x = Vector3D.Subtract(nodes[2], nodes[0]);
                    x.Normalize();
                    z = Vector3D.CrossProduct(x, Vector3D.Subtract(nodes[3], nodes[1]));
                    z.Normalize();
                    y = Vector3D.CrossProduct(z, x);
                    y.Normalize();
                }
            }
            else
            {
                x = Vector3D.Subtract(nodes[1], nodes[0]);
                x.Normalize();
                z = Vector3D.CrossProduct(x, Vector3D.Subtract(nodes[2], nodes[0]));
                z.Normalize();

                x = new Vector3D(1, 0, 0);
                x = Vector3D.Subtract(x, Vector3D.Multiply(Vector3D.DotProduct(x, z), z));

                if (x.Length == 0)
                    x = new Vector3D(0, z.X > 0 ? -1 : 1, 0);

                y = Vector3D.CrossProduct(z, x);

                x.Normalize();
                y.Normalize();
            }

            //Rotation
            Matrix3D rotMat = HelperClass.RotationMatrix(z, rotationAngle * (Math.PI / 180));
            x = Vector3D.Multiply(x, rotMat);
            y = Vector3D.Multiply(y, rotMat);

            return new StructuralAxis(
                new StructuralVectorThree(new double[] { x.X, x.Y, x.Z }),
                new StructuralVectorThree(new double[] { y.X, y.Y, y.Z }),
                new StructuralVectorThree(new double[] { z.X, z.Y, z.Z })
            );
        }

        /// <summary>
        /// Calculates rotation angle of 1D entity to align with axis.
        /// </summary>
        /// <param name="coor">Entity coordinates</param>
        /// <param name="zAxis">Z axis of entity</param>
        /// <returns>Rotation angle</returns>
        public double Get1DAngle(double[] coor, StructuralVectorThree zAxis)
        {
            return Get1DAngle(LocalAxisEntity1D(coor, zAxis));
        }

        /// <summary>
        /// Calculates rotation angle of 1D entity to align with axis.
        /// </summary>
        /// <param name="axis">Axis of entity</param>
        /// <returns>Rotation angle</returns>
        public double Get1DAngle(StructuralAxis axis)
        {
            Vector3D axisX = new Vector3D(axis.Xdir.Value[0], axis.Xdir.Value[1], axis.Xdir.Value[2]);
            Vector3D axisY = new Vector3D(axis.Ydir.Value[0], axis.Ydir.Value[1], axis.Ydir.Value[2]);
            Vector3D axisZ = new Vector3D(axis.Normal.Value[0], axis.Normal.Value[1], axis.Normal.Value[2]);

            if (axisX.X == 0 & axisX.Y == 0)
            {
                // Column
                Vector3D Yglobal = new Vector3D(0, 1, 0);

                double angle = Math.Acos(Vector3D.DotProduct(Yglobal, axisY) / (Yglobal.Length * axisY.Length)).ToDegrees();
                if (double.IsNaN(angle)) return 0;

                Vector3D signVector = Vector3D.CrossProduct(Yglobal, axisY);
                double sign = Vector3D.DotProduct(signVector, axisX);

                return sign >= 0 ? angle : -angle;
            }
            else
            {
                Vector3D Zglobal = new Vector3D(0, 0, 1);
                Vector3D Y0 = Vector3D.CrossProduct(Zglobal, axisX);
                double angle = Math.Acos(Vector3D.DotProduct(Y0, axisY) / (Y0.Length * axisY.Length)).ToDegrees();
                if (double.IsNaN(angle)) angle = 0;

                Vector3D signVector = Vector3D.CrossProduct(Y0, axisY);
                double sign = Vector3D.DotProduct(signVector, axisX);

                return sign >= 0 ? angle : 360 - angle;
            }
        }

        /// <summary>
        /// Calculates rotation angle of 2D entity to align with axis
        /// </summary>
        /// <param name="coor">Entity coordinates</param>
        /// <param name="axis">Axis of entity</param>
        /// <returns>Rotation angle</returns>
        public double Get2DAngle(double[] coor, StructuralAxis axis)
        {
            Vector3D axisX = new Vector3D(axis.Xdir.Value[0], axis.Xdir.Value[1], axis.Xdir.Value[2]);
            Vector3D axisY = new Vector3D(axis.Ydir.Value[0], axis.Ydir.Value[1], axis.Ydir.Value[2]);
            Vector3D axisZ = new Vector3D(axis.Normal.Value[0], axis.Normal.Value[1], axis.Normal.Value[2]);

            Vector3D x0;
            Vector3D z0;

            List<Vector3D> nodes = new List<Vector3D>();

            for (int i = 0; i < coor.Length; i += 3)
                nodes.Add(new Vector3D(coor[i], coor[i + 1], coor[i + 2]));

            // Get 0 angle axis in GLOBAL coordinates
            x0 = Vector3D.Subtract(nodes[1], nodes[0]);
            x0.Normalize();
            z0 = Vector3D.CrossProduct(x0, Vector3D.Subtract(nodes[2], nodes[0]));
            z0.Normalize();

            x0 = new Vector3D(1, 0, 0);
            x0 = Vector3D.Subtract(x0, Vector3D.Multiply(Vector3D.DotProduct(x0, z0), z0));

            if (x0.Length == 0)
                x0 = new Vector3D(0, z0.X > 0 ? -1 : 1, 0);

            x0.Normalize();

            // Find angle
            double angle = Math.Acos(Vector3D.DotProduct(x0, axisX) / (x0.Length * axisX.Length)).ToDegrees();
            if (double.IsNaN(angle)) return 0;

            Vector3D signVector = Vector3D.CrossProduct(x0, axisX);
            double sign = Vector3D.DotProduct(signVector, axisZ);

            return sign >= 0 ? angle : -angle;
        }

        /// <summary>
        /// Maps a flat array of coordinates from a local coordinate system to the global Cartesian coordinate system.
        /// </summary>
        /// <param name="values">Flat array of coordinates</param>
        /// <param name="axis">Local coordinate system</param>
        /// <returns>Transformed array of coordinates</returns>
        public double[] MapPointsLocal2Global(double[] values, StructuralAxis axis)
        {
            List<double> newVals = new List<double>();

            for (int i = 0; i < values.Length; i += 3)
            {
                List<double> coor = values.Skip(i).Take(3).ToList();

                double x = 0;
                double y = 0;
                double z = 0;

                x += axis.Xdir.Value[0] * coor[0];
                y += axis.Xdir.Value[1] * coor[0];
                z += axis.Xdir.Value[2] * coor[0];

                x += axis.Ydir.Value[0] * coor[1];
                y += axis.Ydir.Value[1] * coor[1];
                z += axis.Ydir.Value[2] * coor[1];

                x += axis.Normal.Value[0] * coor[2];
                y += axis.Normal.Value[1] * coor[2];
                z += axis.Normal.Value[2] * coor[2];

                newVals.Add(x);
                newVals.Add(y);
                newVals.Add(z);
            }

            return newVals.ToArray();
        }

        /// <summary>
        /// Maps a flat array of coordinates from the global Cartesian coordinate system to a local coordinate system.
        /// </summary>
        /// <param name="values">Flat array of coordinates</param>
        /// <param name="axis">Local coordinate system</param>
        /// <returns>Transformed array of coordinates</returns>
        public double[] MapPointsGlobal2Local(double[] values, StructuralAxis axis)
        {
            List<double> newVals = new List<double>();

            for (int i = 0; i < values.Length; i += 3)
            {
                List<double> coor = values.Skip(i).Take(3).ToList();

                double x = 0;
                double y = 0;
                double z = 0;

                x += axis.Xdir.Value[0] * coor[0];
                y += axis.Ydir.Value[0] * coor[0];
                z += axis.Normal.Value[0] * coor[0];

                x += axis.Xdir.Value[1] * coor[1];
                y += axis.Ydir.Value[1] * coor[1];
                z += axis.Normal.Value[1] * coor[1];

                x += axis.Xdir.Value[2] * coor[2];
                y += axis.Ydir.Value[2] * coor[2];
                z += axis.Normal.Value[2] * coor[2];

                newVals.Add(x);
                newVals.Add(y);
                newVals.Add(z);
            }

            return newVals.ToArray();
        }
        #endregion

        #region List
        /// <summary>
        /// Converts a GSA list to a list of indices.
        /// </summary>
        /// <param name="list">GSA list</param>
        /// <param name="type">GSA entity type</param>
        /// <returns></returns>
        public int[] ConvertGSAList(string list, GSAEntity type)
        {
            if (list == null) return new int[0];

            string[] pieces = list.ListSplit(" ");
            pieces = pieces.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            List<int> items = new List<int>();
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].IsDigits())
                    items.Add(Convert.ToInt32(pieces[i]));
                else if (pieces[i].Contains('"'))
                    items.AddRange(ConvertNamedGSAList(pieces[i], type));
                else if (pieces[i] == "to")
                {
                    int lowerRange = Convert.ToInt32(pieces[i - 1]);
                    int upperRange = Convert.ToInt32(pieces[i + 1]);

                    for (int j = lowerRange + 1; j <= upperRange; j++)
                        items.Add(j);

                    i++;
                }
                else
                {
                    try
                    {
                        int[] itemTemp = new int[0];
                        GSAObject.EntitiesInList(pieces[i], (GsaEntity)type, out itemTemp);
                        items.AddRange(itemTemp);
                    }
                    catch
                    { }
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Converts a named GSA list to a list of indices.
        /// </summary>
        /// <param name="list">GSA list</param>
        /// <param name="type">GSA entity type</param>
        /// <returns></returns>
        public int[] ConvertNamedGSAList(string list, GSAEntity type)
        {
            list = list.Trim(new char[] { '"' });

            string res = GetGWARecords("GET,LIST," + list).FirstOrDefault();

            string[] pieces = res.Split(new char[] { ',' });

            return ConvertGSAList(pieces[pieces.Length - 1], type);
        }

        /// <summary>
        /// Extracts and return the group indicies in the list.
        /// </summary>
        /// <param name="list">List</param>
        /// <returns>Array of group indices</returns>
        public int[] GetGroupsFromGSAList(string list)
        {
            string[] pieces = list.ListSplit(" ");

            List<int> groups = new List<int>();

            foreach (string p in pieces)
                if (p.Length > 0 && p[0] == 'G')
                    groups.Add(Convert.ToInt32(p.Substring(1)));

            return groups.ToArray();
        }
        #endregion

        #region Cache
        /// <summary>
        /// Move current cache into previous cache.
        /// </summary>
        public void ClearCache()
        {
            PreviousGSAGetCache = new Dictionary<string, object>(GSAGetCache);
            GSAGetCache.Clear();
            PreviousGSASetCache = new Dictionary<string, object>(GSASetCache);
            GSASetCache.Clear();
        }

        /// <summary>
        /// Clear current and previous cache.
        /// </summary>
        public void FullClearCache()
        {
            PreviousGSAGetCache.Clear();
            GSAGetCache.Clear();
            PreviousGSASetCache.Clear();
            GSASetCache.Clear();
        }

        /// <summary>
        /// Blanks all records within the current and previous cache.
        /// </summary>
        public void DeleteSpeckleObjects()
        {
            BlankDepreciatedGWASetCommands();
            ClearCache();
            BlankDepreciatedGWASetCommands();
        }
        #endregion

        #region SID
        public string GenerateSID(SpeckleObject obj)
        {
            string sid = "";

            if (!string.IsNullOrEmpty(obj.ApplicationId))
                sid += "{speckle_app_id:" + obj.ApplicationId + "}";

            return sid;
        }
        #endregion

        #region Results
        /// <summary>
        /// Checks if the load case exists in the GSA file
        /// </summary>
        /// <param name="loadCase">GSA load case description</param>
        /// <returns>True if load case exists</returns>
        public bool CaseExist(string loadCase)
        {
            try
            {
                string[] pieces = loadCase.Split(new char[] { 'p' }, StringSplitOptions.RemoveEmptyEntries);

                if (pieces.Length == 1)
                    return GSAObject.CaseExist(loadCase[0].ToString(), Convert.ToInt32(loadCase.Substring(1))) == 1;
                else if (pieces.Length == 2)
                    return GSAObject.CaseExist(loadCase[0].ToString(), Convert.ToInt32(pieces[0].Substring(1))) == 1;
                else
                    return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Extracts the reactions for the given node.
        /// </summary>
        /// <param name="id">GSA node ID</param>
        /// <param name="loadCase">Load case</param>
        /// <param name="axis">Result axis</param>
        /// <returns>Dictionary of reactions with keys {x,y,z,xx,yy,zz}.</returns>
        public Dictionary<string, double> GetNodeReactions(int id, string loadCase, string axis = "local")
        {
            try
            {
                if (ResultMode != GSAResultMode.NodeReactions)
                {
                    GSAObject.Output_Init_Arr(0x0, axis, loadCase, ResHeader.REF_REAC, 1);
                    ResultMode = GSAResultMode.NodeReactions;
                }

                GsaResults[] res;
                int num;
                GSAObject.Output_Extract_Arr(id, out res, out num);

                Dictionary<string, double> ret = new Dictionary<string, double>()
                {
                    {"x", res.Last().dynaResults[0]},
                    {"y", res.Last().dynaResults[1]},
                    {"z", res.Last().dynaResults[2]},
                    {"xx", res.Last().dynaResults[4]},
                    {"yy", res.Last().dynaResults[5]},
                    {"zz", res.Last().dynaResults[6]},
                };

                //Dictionary<string, double> ret = new Dictionary<string, double>();

                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 1, 1);
                //ret["x"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 2, 1);
                //ret["y"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 3, 1);
                //ret["z"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 4, 1);
                //ret["xx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 5, 1);
                //ret["yy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_REAC + 6, 1);
                //ret["zz"] = GSAObject.Output_Extract(id, 0);

                return ret;
            }
            catch { return null; }
        }

        /// <summary>
        /// Extracts the displacements for the given node.
        /// </summary>
        /// <param name="id">GSA node ID</param>
        /// <param name="loadCase">Load case</param>
        /// <param name="axis">Result axis</param>
        /// <returns>Dictionary of displacements with keys {x,y,z,xx,yy,zz}.</returns>
        public Dictionary<string, double> GetNodeDisplacements(int id, string loadCase, string axis = "local")
        {
            try
            {
                if (ResultMode != GSAResultMode.NodeDisplacements)
                {
                    GSAObject.Output_Init_Arr(0x0, axis, loadCase, ResHeader.REF_DISP, 1);
                    ResultMode = GSAResultMode.NodeDisplacements;
                }

                GsaResults[] res;
                int num;
                GSAObject.Output_Extract_Arr(id, out res, out num);

                Dictionary<string, double> ret = new Dictionary<string, double>()
                {
                    {"x", res.Last().dynaResults[0]},
                    {"y", res.Last().dynaResults[1]},
                    {"z", res.Last().dynaResults[2]},
                    {"xx", res.Last().dynaResults[4]},
                    {"yy", res.Last().dynaResults[5]},
                    {"zz", res.Last().dynaResults[6]},
                };

                //Dictionary<string, double> ret = new Dictionary<string, double>();

                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 1, 1);
                //ret["x"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 2, 1);
                //ret["y"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 3, 1);
                //ret["z"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 4, 1);
                //ret["xx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 5, 1);
                //ret["yy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x0, axis, loadCase, (int)ResHeader.REF_DISP + 6, 1);
                //ret["zz"] = GSAObject.Output_Extract(id, 0);

                return ret;
            }
            catch { return null; }
        }

        /// <summary>
        /// Extracts the displacements for the given 2D element.
        /// </summary>
        /// <param name="id">GSA element ID</param>
        /// <param name="loadCase">Load case</param>
        /// <param name="axis">Result axis</param>
        /// <returns>Dictionary of displacements with keys {x,y,z}.</returns>
        public Dictionary<string, double> Get2DElementDisplacements(int id, string loadCase, string axis = "local")
        {
            try
            {
                if (ResultMode != GSAResultMode.Element2DDisplacements)
                {
                    GSAObject.Output_Init_Arr(0x10, axis, loadCase, ResHeader.REF_DISP_EL2D, 1);
                    ResultMode = GSAResultMode.Element2DDisplacements;
                }

                GsaResults[] res;
                int num;
                GSAObject.Output_Extract_Arr(id, out res, out num);

                Dictionary<string, double> ret = new Dictionary<string, double>()
                {
                    {"x", res.Last().dynaResults[0]},
                    {"y", res.Last().dynaResults[1]},
                    {"z", res.Last().dynaResults[2]},
                };

                //Dictionary<string, double> ret = new Dictionary<string, double>();

                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 1, 1);
                //ret["x"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 2, 1);
                //ret["y"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 3, 1);
                //ret["z"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 5, 1);
                //ret["xx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 6, 1);
                //ret["yy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_DISP_EL2D + 7, 1);
                //ret["zz"] = GSAObject.Output_Extract(id, 0);

                return ret;
            }
            catch { return null; }
        }

        /// <summary>
        /// Extracts the forces for the given 2D element.
        /// </summary>
        /// <param name="id">GSA element ID</param>
        /// <param name="loadCase">Load case</param>
        /// <param name="axis">Result axis</param>
        /// <returns>Dictionary of forces with keys {nx,ny,nxy,mx,my,mxy,vx,vy}.</returns>
        public Dictionary<string, double> Get2DElementForces(int id, string loadCase, string axis = "local")
        {
            try
            {
                GSAObject.Output_Init_Arr(0x10, axis, loadCase, ResHeader.REF_FORCE_EL2D_PRJ, 1);
                ResultMode = GSAResultMode.Element2DForces;

                GsaResults[] forceRes;
                int forceNum;
                GSAObject.Output_Extract_Arr(id, out forceRes, out forceNum);

                GSAObject.Output_Init_Arr(0x10, axis, loadCase, ResHeader.REF_MOMENT_EL2D_PRJ, 1);

                GsaResults[] momentRes;
                int momentNum;
                GSAObject.Output_Extract_Arr(id, out momentRes, out momentNum);

                Dictionary<string, double> ret = new Dictionary<string, double>()
                {
                    {"nx", forceRes.Last().dynaResults[2]},
                    {"ny", forceRes.Last().dynaResults[3]},
                    {"nxy", forceRes.Last().dynaResults[4]},
                    {"mx", momentRes.Last().dynaResults[1]},
                    {"my", momentRes.Last().dynaResults[2]},
                    {"mxy", momentRes.Last().dynaResults[3]},
                    {"vx", forceRes.Last().dynaResults[5]},
                    {"vy", forceRes.Last().dynaResults[6]},
                };

                //Dictionary<string, double> ret = new Dictionary<string, double>();

                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_FORCE_EL2D_PRJ + 3, 1);
                //ret["nx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_FORCE_EL2D_PRJ + 4, 1);
                //ret["ny"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_FORCE_EL2D_PRJ + 5, 1);
                //ret["nxy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_MOMENT_EL2D_PRJ + 2, 1);
                //ret["mx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_MOMENT_EL2D_PRJ + 3, 1);
                //ret["my"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_MOMENT_EL2D_PRJ + 4, 1);
                //ret["mxy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_FORCE_EL2D_PRJ + 6, 1);
                //ret["vx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x2, axis, loadCase, (int)ResHeader.REF_FORCE_EL2D_PRJ + 7, 1);
                //ret["vy"] = GSAObject.Output_Extract(id, 0);

                return ret;
            }
            catch { return null; }
        }

        /// <summary>
        /// Extracts the stresses for the given 2D element.
        /// </summary>
        /// <param name="id">GSA element ID</param>
        /// <param name="loadCase">Load case</param>
        /// <param name="axis">Result axis</param>
        /// <param name="layer">Layer of element to extract results from</param>
        /// <returns>Dictionary of stresses with keys {sxx,syy,tzx,tzy,txy}.</returns>
        public Dictionary<string, double> Get2DElementStresses(int id, string loadCase, string axis = "local", GSA2DElementLayer layer = GSA2DElementLayer.Middle)
        {
            try
            {
                if (ResultMode != GSAResultMode.Element2DStresses)
                {
                    GSAObject.Output_Init_Arr(0x10 | (int)layer, axis, loadCase, ResHeader.REF_STRESS_EL2D_PRJ, 1);
                    ResultMode = GSAResultMode.Element2DStresses;
                }

                GsaResults[] res;
                int num;
                GSAObject.Output_Extract_Arr(id, out res, out num);

                Dictionary<string, double> ret = new Dictionary<string, double>()
                {
                    {"sxx", res.Last().dynaResults[0]},
                    {"syy", res.Last().dynaResults[1]},
                    {"tzx", res.Last().dynaResults[5]},
                    {"tzy", res.Last().dynaResults[4]},
                    {"txy", res.Last().dynaResults[3]},
                };

                //Dictionary<string, double> ret = new Dictionary<string, double>();

                //GSAObject.Output_Init(0x10 | (int)layer, axis, loadCase, (int)ResHeader.REF_STRESS_EL2D_PRJ + 1, 1);
                //ret["sxx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x10 | (int)layer, axis, loadCase, (int)ResHeader.REF_STRESS_EL2D_PRJ + 2, 1);
                //ret["syy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x10 | (int)layer, axis, loadCase, (int)ResHeader.REF_STRESS_EL2D_PRJ + 6, 1);
                //ret["tzx"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x10 | (int)layer, axis, loadCase, (int)ResHeader.REF_STRESS_EL2D_PRJ + 5, 1);
                //ret["tzy"] = GSAObject.Output_Extract(id, 0);
                //GSAObject.Output_Init(0x10 | (int)layer, axis, loadCase, (int)ResHeader.REF_STRESS_EL2D_PRJ + 4, 1);
                //ret["txy"] = GSAObject.Output_Extract(id, 0);

                return ret;
            }
            catch { return null; }
        }
        #endregion
    }
}
