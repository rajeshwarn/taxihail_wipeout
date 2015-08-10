#region

using System;
using System.Linq;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using MongoRepository;
using CustomerPortal.Web.Services;
using System.Threading.Tasks;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class RevisionController : RevisionControllerBase
    {
        private readonly ISourceControl _sourceControl;
        public RevisionController(IRepository<Revision> repository, ISourceControl sourceControl)
            : base(repository)
        {
            _sourceControl = sourceControl;
        }

        public RevisionController()
            : this(new MongoRepository<Revision>(), SourceControlFactory.GetInstance())
        {
        }

        public ActionResult Index()
        {
            ViewBag.MaxTag = TempData["UpdateTagLimitError"];
            var repository = new MongoRepository<Revision>();
            if (!repository.Any())
            {
                return RedirectToAction("Update", "Revision");
            }
            return View(repository.Where( r=>!r.Inactive ));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Revision revision)
        {
            revision.Id = Guid.NewGuid().ToString();
            Repository.Add(revision);
            return View();
        }

        public async Task<ActionResult> Update()
        {
            var hasLimitError = await _sourceControl.UpdateVersions();
            TempData["UpdateTagLimitError"] = hasLimitError ? "There's more than 100 tags, only the first 100 have been fetched." : string.Empty;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {

            var repository = new MongoRepository<Revision>();

            foreach (var rev in repository)
            {

                if (form["ckb_" + rev.Commit] != null)
                {
                    rev.Hidden = form["ckb_" + rev.Commit].Contains("true");
                    rev.CustomerVisible = form["ckbCV_" + rev.Commit].Contains("true");
                    rev.Inactive = form["ckbInactive_" + rev.Commit].Contains("true");
                    repository.Update(rev);
                }

            }


            return View(repository.Where(r => !r.Inactive));

        }
    }
}