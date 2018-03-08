using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ResXcueTask
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    public class ResXcueTask : Task
    {
        [Required]
        public ITaskItem[] Files { get; set; }

        public bool Indent { get; set; }

        public bool RemoveSchema { get; set; }

        public ResXcueTask()
        {
            Indent = true;
            RemoveSchema = true;
        }

        public override bool Execute()
        {
            foreach (var file in Files)
            {
                var fileName = file.GetMetadata("FullPath");
                ReformatFile(fileName);
            }

            return true;
        }

        private void ReformatFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.IsReadOnly)
            {
                Log.LogMessage($"File {fileName} is readonly, skipping");
                return;
            }

            var xml = XDocument.Load(fileName);

            if (xml.Root?.FirstNode == null)
            {
                Log.LogMessage($"File {fileName} has no first node, skipping");
                return;
            }

            if (IsAlreadyReformatted(fileName))
            {
                Log.LogMessage($"File {fileName} was already processed, skipping");
                return;
            }

            var reformatted = ReformatDocument(xml);
            File.WriteAllText(fileName, reformatted);
        }

        private string ReformatDocument(XDocument xml)
        {
            var reformatted = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            reformatted.AppendLine("<root xml:space=\"preserve\">");

            var dataElements = new List<XElement>();

            if (xml?.Root == null)
            {
                throw new InvalidDataException();
            }

            foreach (var element in xml.Root.Elements())
            {
                var elementLocalName = element.Name.LocalName;

                if (elementLocalName == "data" || elementLocalName == "metadata")
                {
                    dataElements.Add(element);
                    var value = element.Element("value");
                    element.RemoveNodes();

                    var spacePreserveAttribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName == "space");
                    spacePreserveAttribute?.Remove();

                    element.AddFirst(value);
                }
                else if (elementLocalName != "schema" || !RemoveSchema)
                {
                    reformatted.AppendLine($"\t{element.ToString(SaveOptions.DisableFormatting)}");
                }
            }

            reformatted.AppendLine();

            var maxNameLength = dataElements.Any() ? dataElements.Max(d => d.Attribute("name")?.Value.Length ?? 0) : 0;

            foreach (var dataElement in dataElements.OrderBy(d => d.Attribute("name")?.Value))
            {
                if (Indent)
                {
                    var name = dataElement.Attribute("name")?.Value ?? string.Empty;
                    dataElement.AddFirst(new string(' ', Math.Max(maxNameLength - name.Length, 0)));
                }

                reformatted.AppendLine($"\t{dataElement.ToString(SaveOptions.DisableFormatting)}");
            }

            reformatted.AppendLine("</root>");

            return reformatted.ToString();
        }

        private static bool IsAlreadyReformatted(string fileName)
        {
            var lines = File.ReadLines(fileName).ToList();
            //Check whether the last line contains only the closing root element. If not, formatting is needed
            if (lines.Last() != "</root>")
                return false;

            //Check whether all comments are removed. If not, formatting is needed
            return !lines.Any(l => l.Contains("<!--"));
        }
    }
}
