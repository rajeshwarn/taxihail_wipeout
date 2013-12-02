using System;
using System.Collections.Generic;
using System.IO;
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
            //Todo : Here _filePathToPatch should be the equal to temp folder containing all the assets
            _filePathToPatch = Path.Combine(Parent.ConfigDirectoryPath, PathConverter.Convert(filePathToPatch));
            _outputFolder = Path.Combine(Parent.SrcDirectoryPath, PathConverter.Convert(outputFolder));

            _filename = filename;
        }

        public override void Apply()
        {
            NinePatchMaker.Generate(_filePathToPatch, _outputFolder, _filename);
        }
    }
}