using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.Tools.Localization;

namespace apcurium.Tools.Localization.MergeTool.ViewModel
{
    public class MainViewModel
    {

        public MainViewModel()
        {

            Initialize();

        }


        private void Initialize()
        {
            var manager = new ResourceManager();
            foreach (var file in manager.Files )
            {
                
            }
        }

    }
}
