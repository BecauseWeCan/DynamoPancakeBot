using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;



/// <summary>
/// code based on PancakePainter by techninja
/// https://github.com/PancakeBot/PancakePainter
/// </summary>
namespace DynamoPancakeBot
{
    /// <summary>
    /// list of autodesk curve objects with added gcode properties
    /// </summary>
    public class drawColor : List<Curve>, IDisposable
    {
        //**FIELD
        private bool disposedValue = false; // To detect redundant calls

        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// speed of movement (ips)
        /// </summary>
        public int color { get; private set; }

        //**CONSTRUCTOR**
        internal drawColor(int color) : base() { this.color = color; }

        //**STATIC METHOD**
        /// <summary>
        /// create a listDraw object;
        /// extends list class with added tooling properties, double feedSpeed, double toolDiameter, double moveSpeed, double jogSpeed, double plungeDepth
        /// </summary>
        /// <returns></returns>
        public static drawColor BySettings(int color)
        {
            return new drawColor(color);
        }

        public static drawColor BySettings(int color, IEnumerable<Curve> list)
        {
            drawColor output = new drawColor(color);
            output.AddRange(list);
            return output;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~drawColor() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// gcode node for PancakeBot
    /// </summary>
    public class gCodePancakeBot: List<drawColor>, IDisposable
    {
        //**FIELD
        internal int intColor = 4;
        private bool disposedValue = false; // To detect redundant calls

        //**PROPERTIES** //**GEOMETRY**
        public drawColor[] geomerty { get; set; }

        //**PROPERTIES** //**SETTINGS**
        public int botSpeed { get; set; }
        public int lineEndPreShutoff { get; set; }
        public int startWait { get; set; }
        public int endWait { get; set; }
        public int shadeChangeWait { get; set; }
        public bool useLineFill { get; set; }
        public int shapeFillWidth { get; set; }
        public int fillSpacing { get; set; }
        public int fillAngle { get; set; }
        public int fillGroupThreshold { get; set; }
        public bool useColorSpeed { get; set; }
        public int[] botColorSpeed { get; set; }


        //**CONSTRUCTOR**
        #region Constructors
        internal gCodePancakeBot()
        {
            botSpeed = 4620;
            lineEndPreShutoff = 20;
            startWait = 450;
            endWait = 250;
            shadeChangeWait = 35;
            useLineFill = false;
            shapeFillWidth = 4;
            fillSpacing = 15;
            fillAngle = 23;
            fillGroupThreshold = 27;
            useColorSpeed = false;
            botColorSpeed = new int[] { 4620, 4620, 4620, 4620 };

            generateHeader();
            generateFooter();
        }
        internal gCodePancakeBot(drawColor[] orderedDrawList)
        {
            this.AddRange(orderedDrawList);
            botSpeed = 4620;
            lineEndPreShutoff = 20;
            startWait = 450;
            endWait = 250;
            shadeChangeWait = 35;
            useLineFill = false;
            shapeFillWidth = 4;
            fillSpacing = 15;
            fillAngle = 23;
            fillGroupThreshold = 27;
            useColorSpeed = false;
            botColorSpeed = new int[] { 4620, 4620, 4620, 4620 };

            generateHeader();
            generateFooter();

        }
        #endregion


        //**METHODS**CREATE
        public static gCodePancakeBot ByDefault() { return new gCodePancakeBot(); }
        public static gCodePancakeBot ByGeometry(drawColor[] geometry) { return new gCodePancakeBot(geometry); }

        //**METHODS**ACTIONS

        public bool addSurface(int color, Surface surface)
        {
            PolyCurve border = PolyCurve.ByJoinedCurves(surface.PerimeterCurves());
            if (!border.IsPlanar)
            {
                int n = border.NumberOfCurves;
                Point[] points = border.PointsAtEqualSegmentLength(3 * n);
                Plane plane = Plane.ByBestFitThroughPoints(points);
                border = (PolyCurve)border.PullOntoPlane(plane);
                //dispose
                points.ForEach(p => p.Dispose());
                plane.Dispose();
            }

            ///TODO: implemnt space filling curve based on offset
            List<Curve> fillCurves = new List<Curve>();
            PolyCurve fillPath = PolyCurve.ByJoinedCurves(fillCurves);
            this[color].Add(fillPath);
            fillCurves.ForEach(c => c.Dispose());

            border.Dispose();
            return true;
        }
        public bool addCurve(int color, Curve curve)
        {
            Curve planarCurve = curve;
            if (!curve.IsPlanar)
            {
                int n = ((PolyCurve)curve).NumberOfCurves;
                Point[] points = curve.PointsAtEqualSegmentLength(3 * n);
                Plane plane = Plane.ByBestFitThroughPoints(points);
                planarCurve = (PolyCurve)curve.PullOntoPlane(plane);
                //dispose
                points.ForEach(p => p.Dispose());
                plane.Dispose();
            }

            ///TODO: implemnt space filling curve based on offset
            this[color].Add(planarCurve);
            return true;
        }

        #region PancakeBot Settings
        public void editAllSettings(int botSpeed = 4620, int lineEndPreShutoff = 20, int startWait = 450, int endWait = 250, int shadeChangeWait = 35,
            bool useLineFill = false, int shapeFillWidth = 4, int fillSpacing = 15, int fillAngle = 23, int fillGroupThreshold = 27,
            bool useColorSpeed = false, int botColorSpeed0 = 4620, int botColorSpeed1 = 4620, int botColorSpeed2 = 4620, int botColorSpeed3 = 4620)
        {
            this.botSpeed = botSpeed;
            this.lineEndPreShutoff = lineEndPreShutoff;
            this.startWait = startWait;
            this.endWait = endWait;
            this.shadeChangeWait = shadeChangeWait;
            this.useLineFill = useLineFill;
            this.shapeFillWidth = shapeFillWidth;
            this.fillSpacing = fillSpacing;
            this.fillAngle = fillAngle;
            this.fillGroupThreshold = fillGroupThreshold;
            this.useColorSpeed = useColorSpeed;
            this.botColorSpeed = new int[] { botColorSpeed0, botColorSpeed1, botColorSpeed2, botColorSpeed3 };
        }

        public void editSettings(string name, int value)
        {
            if (name == "botSpeed") this.botSpeed = value;
            else if (name == "lineEndPreShutoff") this.lineEndPreShutoff = value;
            else if (name == "startWait") this.startWait = value;
            else if (name == "endWait") this.endWait = value;
            else if (name == "shadeChangeWait") this.shadeChangeWait = value;
            else if (name == "useLineFill") this.useLineFill = (value == 1);
            else if (name == "shapeFillWidth") this.shapeFillWidth = value;
            else if (name == "fillSpacing") this.fillSpacing = value;
            else if (name == "fillAngle") this.fillAngle = value;
            else if (name == "fillGroupThreshold") this.fillGroupThreshold = value;
            else if (name == "useColorSpeed") this.useColorSpeed = (value == 1);
            else if (name == "botColorSpeed0") this.botColorSpeed[0] = value;
            else if (name == "botColorSpeed1") this.botColorSpeed[1] = value;
            else if (name == "botColorSpeed2") this.botColorSpeed[2] = value;
            else if (name == "botColorSpeed3") this.botColorSpeed[3] = value;
        }
        #endregion

        #region gCode Text
        internal string generateHeader()
        {
            DateTime fileCreated = DateTime.Now;
            DateTime fileCreated_GMT = fileCreated.ToUniversalTime();
            string GMTOffset;
            if (fileCreated.Hour - fileCreated_GMT.Hour < 0) GMTOffset = String.Format("{0:00}", fileCreated.Hour - fileCreated_GMT.Hour) + "00";
            else GMTOffset = String.Format("{0:+00}", fileCreated.Hour - fileCreated_GMT.Hour) + "00";
            TimeZone localZone = TimeZone.CurrentTimeZone;
            string timeZone;
            if (localZone.IsDaylightSavingTime(fileCreated)) timeZone = localZone.DaylightName;
            else timeZone = localZone.StandardName;
            string mainHeader = @";DynamoPancakeBot GCODE header start" + "\r\n" +
                            ";Originally generated @ " + DateTime.Now.ToString("ddd MMM dd yyyy HH:mm:ss") + " GMT" + GMTOffset + " (" + timeZone + ")" + "\r\n" +
                            ";Settings used to generate this file:" + "\r\n" +
                            ";----------------------------------------\r\n" +
                            ";botSpeed: " + botSpeed + "\r\n" +
                            ";lineEndPreShutoff: " + lineEndPreShutoff + "\r\n" +
                            ";startWait: " + startWait + "\r\n" +
                            ";endWait: " + endWait + "\r\n" +
                            ";shadeChangeWait: " + shadeChangeWait + "\r\n" +
                            ";useLineFill: " + useLineFill + "\r\n" +
                            ";shapeFillWidth: " + shapeFillWidth + "\r\n" +
                            ";fillSpacing: " + fillSpacing + "\r\n" +
                            ";fillAngle: " + fillAngle + "\r\n" +
                            ";fillGroupThreshold: " + fillGroupThreshold + "\r\n" +
                            ";useColorSpeed: " + useColorSpeed + "\r\n" +
                            ";botColorSpeed: " + botColorSpeed[0] + "," + botColorSpeed[1] + "," + botColorSpeed[2] + "," + botColorSpeed[3] + "\r\n" +
                            ";----------------------------------------" + "\r\n" +
                            "W1 X42 Y210 L485 T0 ;Define Workspace of this file" + "\r\n" +
                            "G21 ;Set units to MM" + "\r\n" +
                            "G1 F4620 ;Set Speed" + "\r\n" +
                            "M107 ;Pump off" + "\r\n" +
                            "G4 P1000 ;Pause for 1000 milliseconds" + "\r\n" +
                            "M84 ;Motors off" + "\r\n" +
                            "G00 X1 Y1 ;Help homing" + "\r\n" +
                            "G28 X0 Y0 ;Home All Axis" + "\r\n" +
                            ";PancakePainter header complete";
            return mainHeader;
        }

        internal string generateFooter()
        {
            string mainFooter = @";PancakePainter Footer Start" + "\r\n" +
                            "G4 P1000 ;Pause for 1000 milliseconds" + "\r\n" +
                            "G00 X1 Y1 ;Help homing" + "\r\n" +
                            "G28 X0 Y0 ;Home All Axis" + "\r\n" +
                            "M84 ;Motors off" + "\r\n" +
                            ";PancakePainter Footer Complete";
            return mainFooter;
        }

        internal string generateGeometry(int color)
        {
            string geometry = "";
            // TODO: generate gCode from geometry using existing javascript
            return geometry;
        }

        internal string changeColor(int color)
        {
            string[] Color = { "Light", "Medium", "Medium Dark", "Dark" };
            string changeColor = @";Switching Color to: " + Color[color] + "\r\n" +
                            "G4 P1000; Pause for 1000 milliseconds" + "\r\n" +
                            "G00 X1 Y1; Help homing" + "\r\n" +
                            "G28 X0 Y0 ; Home All Axis" + "\r\n" +
                            "M84; Motors off" + "\r\n" +
                            "M142; Bottle change" + "\r\n" +
                            "G4 P35000; Pause for 35000 milliseconds";
            return changeColor;
        }

        internal string generateMove(double x, double y)
        {
            string move = "G00 X" + x + " Y" + y + ";";
            return move;
        }

        public bool loadHeaderFooter(string filename)
        {
            List<string> lines = new List<string>();
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(filename);

                //Read the first line of text
                string line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the lie to console window
                    lines.Add(line);
                    //Read the next line
                    line = sr.ReadLine();
                }

                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return true;
            
        }

        public string generateCode()
        {
            string gCode = generateHeader() + generateGeometry(0) + changeColor(1) + generateGeometry(0) + changeColor(1) + generateGeometry(1) + changeColor(2) + generateGeometry(2) + changeColor(3) + generateGeometry(3) + generateFooter();
            return gCode;
        }
        #endregion

        #region Javascript
        public TextReader loadJS(string filename)
        {
            List<string> lines = new List<string>();
            string code = "";
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(filename);

                //Read the first line of text
                string line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the lie to console window
                    lines.Add(line);
                    code = code + line;
                    //Read the next line
                    line = sr.ReadLine();
                }

                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            Jurassic.StringScriptSource script = new Jurassic.StringScriptSource(code);
            return script.GetReader();          
        }
        #endregion

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~gCodePancakeBot() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
