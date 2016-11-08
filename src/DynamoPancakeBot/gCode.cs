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
    public class drawColor : List<Curve>
    {
        //**FIELD

        //**PROPERTIES** //**QUERY**
        /// <summary>
        /// speed of movement (ips)
        /// </summary>
        public double moveSpeed { get; set; }

        //**CONSTRUCTOR**
        internal drawColor(int color, gCodePancakeBot gCode) : base()
        {
            if (gCode.useColorSpeed) this.moveSpeed = gCode.botColorSpeed[color];
            else this.moveSpeed = gCode.botSpeed;
        }

        //**STATIC METHOD**
        /// <summary>
        /// create a listDraw object;
        /// extends list class with added tooling properties, double feedSpeed, double toolDiameter, double moveSpeed, double jogSpeed, double plungeDepth
        /// </summary>
        /// <returns></returns>
        public static drawColor BySettings(int color, gCodePancakeBot gCode)
        {
            return new drawColor(color, gCode);
        }
    }

    public class gCodePancakeBot
    {
        //**FIELD
        internal int intColor = 4;

        //**PROPERTIES** //**STRING**
        public string mainHeader { get; set; }
        public string mainFooter { get; set; }

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
        internal gCodePancakeBot(drawColor[] orderedDrawList)
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

        public void editSettings(int botSpeed = 4620, int lineEndPreShutoff = 20, int startWait = 450, int endWait = 250, int shadeChangeWait = 35,
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

        public string changeColor(int color)
        {
            string[] Color = {"Light", "Medium", "Medium Dark", "Dark"}
            string changeColor = @";Switching Color to: " + Color[color] + "\r\n" +
                            "G4 P1000; Pause for 1000 milliseconds" + "\r\n" +
                            "G00 X1 Y1; Help homing" + "\r\n" +
                            "G28 X0 Y0 ; Home All Axis" + "\r\n" +
                            "M84; Motors off" + "\r\n" +
                            "M142; Bottle change" + "\r\n" +
                            "G4 P35000; Pause for 35000 milliseconds";
            return changeColor;
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
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            
        }

        internal void generateHeader()
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
            mainHeader = @";PancakePainter v1.2.0-beta GCODE header start" + "\r\n" +
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
        }

        internal void generateFooter()
        {
            mainFooter = @";PancakePainter Footer Start" + "\r\n" +
                            "G4 P1000 ;Pause for 1000 milliseconds" + "\r\n" +
                            "G00 X1 Y1 ;Help homing" + "\r\n" +
                            "G28 X0 Y0 ;Home All Axis" + "\r\n" +
                            "M84 ;Motors off" + "\r\n" +
                            ";PancakePainter Footer Complete";
        }
    }
}
