﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.Tools.Localization;
using apcurium.Tools.Localization.iOS;
using apcurium.Tools.Localization.Android;

namespace apcurium.Tools.Localization
{
    public class ResourceManager
    {
        private readonly IList<ResourceFileHandlerBase> _sourceFileHandlers = new List<ResourceFileHandlerBase>();
        private readonly IList<ResourceFileHandlerBase> _destinationFileHandlers = new List<ResourceFileHandlerBase>();

       public void AddSource(ResourceFileHandlerBase sourceFileHandler)
        {
           if(sourceFileHandler == null) throw new ArgumentNullException();
            _sourceFileHandlers.Add(sourceFileHandler);
        }

        public void AddDestination(ResourceFileHandlerBase destinationFileHandler)
        {
            if(destinationFileHandler == null) throw new ArgumentNullException();
            _destinationFileHandlers.Add(destinationFileHandler);
        }




        public ResourceFileHandlerBase[] Files { get; private set; }

        public void Update()
        {
            foreach (var dest in _destinationFileHandlers)
            foreach (var src in _sourceFileHandlers)
            foreach (var key in src.Keys)
            {
                dest[key] = src[key];
            }
        }
    }
}
