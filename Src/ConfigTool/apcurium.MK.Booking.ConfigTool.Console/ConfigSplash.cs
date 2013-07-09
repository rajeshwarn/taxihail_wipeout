using System;
using System.Collections.Generic;
using System.Linq;
using ninePatchMaker;


namespace apcurium.MK.Booking.ConfigTool
{
    public class ConfigSplash : Config
    {
        private readonly string _filePathToPatch;
        private readonly string _outputFolder;
        private readonly string _filename;

        public ConfigSplash(AppConfig parent, string filePathToPatch, string outputFolder, string filename)
            : base(parent)
        {
            _filePathToPatch = filePathToPatch;
            _outputFolder = outputFolder;
            _filename = filename;
        }

        public override void Apply()
        {
            NinePatchMaker.Generate(_filePathToPatch, _outputFolder, _filename);
        }
    }
}