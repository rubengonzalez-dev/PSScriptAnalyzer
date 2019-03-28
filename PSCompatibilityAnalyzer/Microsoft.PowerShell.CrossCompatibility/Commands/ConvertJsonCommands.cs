using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsData.ConvertTo, CommandUtilities.MODULE_PREFIX + "Json")]
    public class ConvertToPSCompatibilityJsonCommand : PSCmdlet
    {
        private JsonProfileSerializer _serializer;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public object[] Item { get; set; }

        [Parameter]
        [Alias(new [] { "Compress" })]
        public SwitchParameter NoWhitespace { get; set; }

        protected override void BeginProcessing()
        {
            _serializer = JsonProfileSerializer.Create(NoWhitespace ? Formatting.None : Formatting.Indented);
        }

        protected override void ProcessRecord()
        {
            foreach (object obj in Item)
            {
                WriteObject(_serializer.Serialize(obj));
            }
            return;
        }
    }

    [Cmdlet(VerbsData.ConvertFrom, CommandUtilities.MODULE_PREFIX + "Json")]
    public class ConvertFromPSCompatibilityJsonCommand : PSCmdlet
    {
        private JsonProfileSerializer _serializer;

        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "JsonSource")]
        public object[] JsonSource { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [ValidateNotNullOrEmpty()]
        public string[] Path { get; set; }

        public ConvertFromPSCompatibilityJsonCommand()
        {
            _serializer = JsonProfileSerializer.Create();
        }

        protected override void ProcessRecord()
        {
            if (Path != null)
            {
                foreach (string filePath in Path)
                {
                    string absolutePath = this.GetNormalizedAbsolutePath(filePath);
                    WriteObject(_serializer.DeserializeFromFile(absolutePath));
                }
                return;
            }

            if (JsonSource != null)
            {
                foreach (object jsonSourceItem in JsonSource)
                {
                    switch (jsonSourceItem)
                    {
                        case string jsonString:
                            WriteObject(_serializer.Deserialize(jsonString));
                            return;

                        case FileInfo jsonFile:
                            WriteObject(_serializer.Deserialize(jsonFile));
                            return;

                        case TextReader jsonReader:
                            WriteObject(_serializer.Deserialize(jsonReader));
                            return;

                        default:
                            throw new ArgumentException($"Unsupported type for {nameof(JsonSource)} parameter. Should be a string, FileInfo or TextReader object.");
                    }
                }
            }
        }
    }
}