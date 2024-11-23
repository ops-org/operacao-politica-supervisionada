﻿/*
 * Computer Vision SDK QuickStart
 *
 * Examples included:
 *  - Authenticate
 *  - OCR (Read API): Read file from URL
 #  - OCR (Read API): Read file from local
 *
 *  Prerequisites:
 *   - Visual Studio 2019 (or 2017, but note this is a .Net Core console app, not .Net Framework)
 *   - NuGet library: Microsoft.Azure.CognitiveServices.Vision.ComputerVision
 *   - Azure Computer Vision resource from https://ms.portal.azure.com
 *   - Create a .Net Core console app, then copy/paste this Program.cs file into it. Be sure to update the namespace if it's different.
 *   - Download local images (celebrities.jpg, objects.jpg, handwritten_text.jpg, and printed_text.jpg)
 *     from the link below then add to your bin/Debug/netcoreapp2.2 folder.
 *     https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/ComputerVision/Images
 *
 *   How to run:
 *    - Once your prerequisites are complete, press the Start button in Visual Studio.
 *    - Each example displays a printout of its results.
 *
 *   References:
 *    - .NET SDK: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/cognitiveservices/client/computervision?view=azure-dotnet
 *    - API (testing console): https://westus.dev.cognitive.microsoft.com/docs/services/computer-vision-v3-2/operations/5d986960601faab4bf452005
 *    - Computer Vision documentation: https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/
 */

using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System.Text;

namespace OPS.Importador.Utilities
{
    public class ComputerVisionOcr
    {
        // Add your Computer Vision key and endpoint
        static string key = "52a29fecf4e44d428fd0f9cbb3d2f370";
        static string endpoint = "https://ops-ocr.cognitiveservices.azure.com/";

        private readonly ComputerVisionClient client;

        public ComputerVisionOcr()
        {
            client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        public async Task<string> ReadFileLocal(string localFile)
        {
            // Read text from URL
            var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile));
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            //Console.WriteLine($"Reading text from local file {Path.GetFileName(localFile)}...");
            //Console.WriteLine();
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted));

            // Display the found text.
            var textUrlFileResulsts = results.AnalyzeResult.ReadResults;
            var sb = new StringBuilder();
            foreach (ReadResult page in textUrlFileResulsts)
            {
                foreach (Line line in page.Lines)
                {
                    sb.AppendLine(line.Text);
                }
            }
            return sb.ToString();
        }
    }
}