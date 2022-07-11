using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SideBySide
{
    public class DVHData
    {
        public DVHData()
        {
            this.StartCalculations();
        }

        private void StartCalculations()
        {
            /*var random = new Random();
            List<double> dose_a = new List<double> { 0, 1, 2, 3, 4 };
            List<double> volume_a = new List<double> { 0, 1, 2, 3, 4 };

            List<double> dose_b = new List<double> { 0, 1, 2, 3, 4 };
            List<double> volume_b = new List<double> { 1, 2, 3, 4, 5 };

            List<double> dose_c = new List<double> { 0, 1, 2, 3, 4 };
            List<double> volume_c = new List<double> { 3, 4, 5, 6, 7 };

            List<string> names = new List<string> { "Heart", "Gurda", "Naak", "Dimagh", "Suman" };

            var ListCombination = new Dictionary<List<double>, List<double>>();

            ListCombination.Add(dose_a, volume_a);
            ListCombination.Add(dose_b, volume_b);
            ListCombination.Add(dose_c, volume_c);*/

            List<StructureData> Data = new List<StructureData>()
            {
                new StructureData()
                {
                    Dose = new List<double>{0,1,2,3,4},
                    Volume = new List<double> { 0, 1, 2, 3, 4 },
                    MeanDose = 2,
                    MaxDose = 3,
                    AlphaBetaValue = 4,
                    StructureName = "Heart",
                },
                new StructureData()
                {
                    Dose = new List<double> { 0, 1, 2, 3, 4 },
                    Volume = new List<double> { 1, 2, 3, 4, 5 },
                    MeanDose = 2,
                    MaxDose = 3,
                    AlphaBetaValue = 4,
                    StructureName = "Gurda",
                },
                new StructureData()
                {
                    Dose = new List<double>{ 0, 1, 2, 3, 4 },
                    Volume = new List<double> { 3, 4, 5, 6, 7},
                    MeanDose = 2,
                    MaxDose = 3,
                    AlphaBetaValue = 4,
                    StructureName = "Dimagh",
                },
            };


            string htmlBuilder = HTMLBuilder.StaticText(Data, "Basit", "26.01.1995", "1351");

            var runner = new HTMLRunner(htmlBuilder);
            runner.Launch("Test");
        }


    }

    public class HTMLRunner // Create HTML File
    {
        public string Text { get; set; }
        public string TempFolder { get; set; }


        public HTMLRunner(string text)
        {
            TempFolder = Path.GetTempPath();
            Text += text.ToString();
        }


        public void Launch(string title)
        {

            var fileName = Path.Combine(TempFolder, title + ".html");
            Debug.WriteLine($"FileName >\n\n {fileName} \n\n");
            File.WriteAllText(fileName, Text);
        }
    }

    public class HTMLBuilder //Generate the text for the plot to be passed to HTMLRunner class
    {
        public static string StaticText(List<StructureData> Data, string name, string dob, string id)
        {
            // displays the mean, max and alphaBeta Values
            string LastHeading = "";

            // contains the lines data for x-axis and y-axis
            string lines = "";

            // patients information
            string information = " | Patient Name: " + name + " - DOB: " + dob + " - ID: " + id + " |";

            // loops over every structure object to extract values
            foreach (var singleItem in Data)
            {
                // creating x,y axis plot line for each structure
                string x = "x:[" + string.Join(",", singleItem.Dose) + "]";
                string y = "y:[" + string.Join(",", singleItem.Volume) + "]";

                // combining x and y axis plot line for each structure
                lines += "{" + x + "," + y + "," + "name: '" + singleItem.StructureName + "',mode: 'lines+markers', type: 'scatter'},";

                // creates an HTML string to display name, mean, max and alphaBeta values
                LastHeading += "<h3>" + singleItem.StructureName + "</h3> " +
                    "<div> MeanDose = " + singleItem.MeanDose.ToString() + " Gy | " + "MaxDose = " + singleItem.MaxDose.ToString()
                    + " Gy | AlphaBetaRatio = " + singleItem.AlphaBetaValue.ToString()
                    + " Gy </div><br>";

            }

            // start of HTML script
            string startScript = @"
                             <!DOCTYPE html>
                             <html lang='en'>
                             <head>
                             <meta charset=utf-8>
                             <title> EQD2 DVH </title>
                             <script src='https://cdn.plot.ly/plotly-1.2.0.min.js'></script>
                               </head>
                               <body style='text-align: center; font-family:Helvetica;'>
                                        <h1> " + information + @"</h1>
                                        <h2> EQD2 DVH </h2>         
                                        <div id='chart'> </div>
                                        <div style='font-size:1em;'> " + LastHeading + @"</div>
                               </body>
                               <script>
                                    var chart = document.getElementById('chart');
                                    var data = [";

            Debug.WriteLine($"WHAT:> {startScript} ");

            // contains end of HTML script with PLOTLY for charts compilation
            string endScript = @" ]; 
                       Plotly.newPlot(chart, data, {
                                  xaxis: {
                                    title: 'Dose in Gy',
                                    showgrid: false,
                                  },
                                  yaxis: {
                                    title: 'Volume in cm3',
                                  }
                                }); 
                    </script></html>";

            // combining the entire script
            string collectedgraph = startScript + lines + endScript;

            Debug.WriteLine($"HTML:> {collectedgraph} ");

            return collectedgraph;
        }
    }

    /// <summary>
    /// Class for Data collection of a single structure
    /// </summary>
    public class StructureData
    {
        public List<double> Dose { get; set; }
        public List<double> Volume { get; set; }
        public string StructureName { get; set; }
        public double MeanDose { get; set; }
        public double AlphaBetaValue { get; set; }
        public double MaxDose { get; set; }
    }
}


// ONLINE EXECUTABLE PROGRAM WEBSITE :> https://www.tutorialspoint.com/compile_csharp_online.php
//Microsoft (R) Visual C# Compiler version 3.4.0-beta4-19562-05 (ff930dec)
//Copyright (C) Microsoft Corporation. All rights reserved.

/* Start from below 
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        //Your code goes here
        Console.WriteLine("Hello, world!");

        DVHData data = new DVHData();
    }
}

public class DVHData
{
    public DVHData()
    {
        this.StartCalculations();
    }

    private void StartCalculations()
    {
        *//*var random = new Random();
        List<double> dose_a = new List<double> { 0, 1, 2, 3, 4 };
        List<double> volume_a = new List<double> { 0, 1, 2, 3, 4 };

        List<double> dose_b = new List<double> { 0, 1, 2, 3, 4 };
        List<double> volume_b = new List<double> { 1, 2, 3, 4, 5 };

        List<double> dose_c = new List<double> { 0, 1, 2, 3, 4 };
        List<double> volume_c = new List<double> { 3, 4, 5, 6, 7 };

        List<string> names = new List<string> { "Heart", "Gurda", "Naak", "Dimagh", "Suman" };

        var ListCombination = new Dictionary<List<double>, List<double>>();

        ListCombination.Add(dose_a, volume_a);
        ListCombination.Add(dose_b, volume_b);
        ListCombination.Add(dose_c, volume_c);*//*

        List<StructureData> Data = new List<StructureData>() {
            new StructureData() {
                Dose = new List<double> {0,1,2,3,4},
                Volume = new List<double> { 0, 1, 2, 3, 4 },
                MeanDose = 2,
                MaxDose = 3,
                AlphaBetaValue = 4,
                StructureName = "Heart",
            },
            new StructureData() {
                Dose = new List<double> { 0, 1, 2, 3, 4 },
                Volume = new List<double> { 1, 2, 3, 4, 5 },
                MeanDose = 2,
                MaxDose = 3,
                AlphaBetaValue = 4,
                StructureName = "Gurda",
            },
            new StructureData() {
                Dose = new List<double> { 0, 1, 2, 3, 4 },
                Volume = new List<double> { 3, 4, 5, 6, 7},
                MeanDose = 2,
                MaxDose = 3,
                AlphaBetaValue = 4,
                StructureName = "Dimagh",
            },
        };


        string htmlBuilder = HTMLBuilder.StaticText(Data, "Basit", "26.01.1995", "1351");

        var runner = new HTMLRunner(htmlBuilder);
        runner.Launch("Test");
    }


}

public class HTMLRunner // Create HTML File
{
    public string Text { get; set; }
    public string TempFolder { get; set; }


    public HTMLRunner(string text)
    {
        TempFolder = Path.GetTempPath();
        Text += text.ToString();
    }


    public void Launch(string title)
    {

        var fileName = Path.Combine(TempFolder, title + ".html");
        Debug.WriteLine($"FileName >\n\n {fileName} \n\n");
        File.WriteAllText(fileName, Text);
    }
}

public class HTMLBuilder //Generate the text for the plot to be passed to HTMLRunner class
{
    public static string StaticText(List<StructureData> Data, string name, string dob, string id)
    {
        // displays the mean, max and alphaBeta Values
        string LastHeading = "";

        // contains the lines data for x-axis and y-axis
        string lines = "";

        // patients information
        string information = " | Patient Name: " + name + " - DOB: " + dob + " - ID: " + id + " |";

        // loops over every structure object to extract values
        foreach (var singleItem in Data)
        {
            // creating x,y axis plot line for each structure
            string x = "x:[" + string.Join(",", singleItem.Dose) + "]";
            string y = "y:[" + string.Join(",", singleItem.Volume) + "]";

            // combining x and y axis plot line for each structure
            lines += "{" + x + "," + y + "," + "name: '" + singleItem.StructureName + "',mode: 'lines+markers', type: 'scatter'},";

            // creates an HTML string to display name, mean, max and alphaBeta values
            LastHeading += "<h3>" + singleItem.StructureName + "</h3> " +
                           "<div> MeanDose = " + singleItem.MeanDose.ToString() + " Gy | " + "MaxDose = " + singleItem.MaxDose.ToString()
                           + " Gy | AlphaBetaRatio = " + singleItem.AlphaBetaValue.ToString()
                           + " Gy </div><br>";

        }

        // start of HTML script
        string startScript = @"
                             <!DOCTYPE html>
                             <html lang='en'>
                             <head>
                             <meta charset=utf-8>
                             <title> EQD2 DVH </title>
                             <script src='https://cdn.plot.ly/plotly-1.2.0.min.js'></script>
                             </head>
                             <body style='text-align: center; font-family:Helvetica;'>
                             <h1> " + information + @"</h1>
                             <h2> EQD2 DVH </h2>
                             <div id='chart'> </div>
                             <div style='font-size:1em;'> " + LastHeading + @"</div>
                             </body>
                             <script>
                             var chart = document.getElementById('chart');
                             var data = [";

        // contains end of HTML script with PLOTLY for charts compilation
        string endScript = @" ];
                           Plotly.newPlot(chart, data, {
                           xaxis: {
                           title: 'Dose in Gy',
                           showgrid: false,
                       },
                           yaxis: {
                           title: 'Volume in cm3',
                       }
                       });
                           </script></html>";

        // combining the entire script
        string collectedgraph = startScript + lines + endScript;

        Console.WriteLine($"HTML:> {collectedgraph} ");

        return collectedgraph;
    }
}

/// <summary>
/// Class for Data collection of a single structure
/// </summary>
public class StructureData
{
    public List<double> Dose { get; set; }
    public List<double> Volume { get; set; }
    public string StructureName { get; set; }
    public double MeanDose { get; set; }
    public double AlphaBetaValue { get; set; }
    public double MaxDose { get; set; }
}
*/
