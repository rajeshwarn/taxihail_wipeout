using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MK.ConfigurationManager.Entities;
using Newtonsoft.Json;

namespace MK.ConfigurationManager
{
    class RepositoryHelper
    {


        public static void FetchRepoTags()
        {
            var bitbucketTags = GetTags().Select(x => new
            {
                Display = string.Format("[Bitbucket] {0}", x.Key),
                Revision = x.Value.node
            }).ToList();

            var dbVersions = ConfigurationDatabase.Current.Versions.ToList();

            foreach (var tag in bitbucketTags)
            {
                var correspondingVersion = dbVersions.FirstOrDefault(x => x.Display == tag.Display);
                if (correspondingVersion != null)
                {
                    correspondingVersion.Revision = tag.Revision;
                }
                else
                {
                    ConfigurationDatabase.Current.AddVersion(new AppVersion
                        {
                            Id = Guid.NewGuid(),
                            Display = tag.Display,
                            Revision = tag.Revision
                        });
                }
            }

        }

        static Dictionary<string, BitbucketTagsResponse> GetTags()
        {
            var req = WebRequest.Create("https://bitbucket.org/api/1.0/repositories/apcurium/mk-taxi/tags") as HttpWebRequest;

            var authInfo = "buildapcurium:apcurium5200!";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            string result = null;
            try
            {
                using (var resp = req.GetResponse() as HttpWebResponse)
                {
                    var reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch
            {
                return new Dictionary<string, BitbucketTagsResponse>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, BitbucketTagsResponse>>(result);
        }

    }
}
