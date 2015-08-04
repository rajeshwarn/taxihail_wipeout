using CustomerPortal.Web.Areas.Admin.Models.RequestResponse;
using CustomerPortal.Web.Entities;
using MongoRepository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomerPortal.Web.Services.Impl
{
    public class BitBucketVersionUpdater : ISourceControl
    {
        public Task<bool> UpdateVersions()
        {
            var repository = new MongoRepository<Revision>();
            var req = WebRequest.Create("https://bitbucket.org/api/1.0/repositories/apcurium/mk-taxi/tags") as HttpWebRequest;
            var authInfo = "buildapcurium:apcurium5200!";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
            string result = null;

            using (var resp = req.GetResponse() as HttpWebResponse)
            {
                var reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }

            var revisionsFromBitBucket =
                JsonConvert.DeserializeObject<Dictionary<string, BitBucketRepoResponse>>(result)
                    .Select(x => new Revision
                    {
                        Id = Guid.NewGuid().ToString(),
                        Commit = x.Value.node,
                        Tag = x.Key
                    }).ToList();



            revisionsFromBitBucket.Add(GetDefaultRevision());

            //Existing Revisions, we ignore version number tags to prevent building a version from an alternate commit.
            //existing revisions => update
            var updatesRevisions = repository
                .AsEnumerable()
                .Where(revision => !IsVersionNumber(revision))
                .Join(revisionsFromBitBucket, rev => rev.Tag, revBitbucket => revBitbucket.Tag, (rev, revBitbucket) => new Revision
                {
                    Id = rev.Id,
                    Tag = rev.Tag,
                    Commit = revBitbucket.Commit,
                    Hidden = rev.Hidden,
                    CustomerVisible = rev.CustomerVisible,
                    Inactive = rev.Inactive,
                })
                .ToArray();


            if (updatesRevisions.Any())
            {
                repository.Update(updatesRevisions);
            }

            var tagsFromRepository = repository.Select(x => x.Tag).ToList();
            //new revisions in bitbucket => add to repository
            var newRevisions = revisionsFromBitBucket.Where(x => !tagsFromRepository.Contains(x.Tag)).ToList();
            if (newRevisions.Any())
            {
                repository.Add(newRevisions);
            }

            return new Task<bool>(() => { return false; });

        }

        public bool IsVersionNumber(Revision revision)
        {
            //Regex pattern to validate if revision is a version number.
            return Regex.IsMatch(revision.Tag, "^([0-9][0-9]*.[0-9][0-9]*.[0-9][0-9]*(.[0-9][0-9]*)?)$");
        }

        private static Revision GetDefaultRevision()
        {
            var repository = new MongoRepository<Revision>();
            var req =
                WebRequest.Create("https://bitbucket.org/api/1.0/repositories/apcurium/mk-taxi/changesets/default/") as
                    HttpWebRequest;
            var authInfo = "buildapcurium:apcurium5200!";
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
            string result = null;

            using (var resp = req.GetResponse() as HttpWebResponse)
            {
                var reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }

            var revisionsFromBitBucket =
                JsonConvert.DeserializeObject<BitBucketRepoResponse>(result);


            return new Revision
            {
                Id = Guid.NewGuid().ToString(),
                Commit = revisionsFromBitBucket.node,
                Tag = "default.tip"
            };
        }
    }
}
