#region

using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Security;
using ExtendedMongoMembership;
using MongoDB.Bson;
using MongoRepository;
using WebMatrix.WebData;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class MembershipController : CompanyControllerBase
    {
        private readonly IMembershipService _membership;
        private readonly MongoSession _session;

        public MembershipController(IRepository<Company> repository, IMembershipService membership)
            : base(repository)
        {
            _membership = membership;
            _session = new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);
        }

        public MembershipController()
            : this(new MongoRepository<Company>(), new MembershipService())
        {
        }

        public ActionResult Index()
        {
            return View(_session.Users);
        }

        public ActionResult Create()
        {
            var companyId = (string) RouteData.Values["id"];
            var model = new CreateUser();
            if (!string.IsNullOrEmpty(companyId))
            {
                var company = Repository.GetById(companyId);
                if (company == null) return HttpNotFound();

                model.CompanyId = company.Id;
                model.CompanyName = company.CompanyName;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(CreateUser model)
        {
            if (!model.IsAdmin)
            {
                var company = Repository.GetById(model.CompanyId);
                if (company == null) return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _membership.CreateUserAndAccount(model.EmailAddress,
                        model.Password,
                        new
                        {
                            Name = (BsonString) model.Name,
                            Company = model.IsAdmin
                                ? (BsonValue) BsonNull.Value
                                : (BsonString) model.CompanyId
                        });

                    _membership.AddUserToRole(model.EmailAddress, RoleName.Customer);
                    if (model.IsAdmin)
                    {
                        _membership.AddUserToRole(model.EmailAddress, RoleName.Admin);
                    }

                    bool isCompanyCreationWizard = Request.Params["cw"] != null;
                    if (isCompanyCreationWizard)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return RedirectToAction("Index");
                }
                catch (MembershipCreateUserException e)
                {
                    if (e.StatusCode == MembershipCreateStatus.DuplicateUserName)
                    {
                        ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("CustomError", "An unknown error occured while creating the user");
                }
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var user = _session.Users.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new EditUser
            {
                EmailAddress = user.UserName,
                Name = user.GetValue("Name", new BsonString("")).ToString()
            });
        }

        [HttpPost]
        public ActionResult Edit(int id, EditUser model)
        {
            if (ModelState.IsValid)
            {
                var user = _session.Users.SingleOrDefault(x => x.UserId == id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                user.UserName = model.EmailAddress;
                if (user.CatchAll == null) user.CatchAll = new BsonDocument();
                user.CatchAll["Name"] = (BsonString) model.Name;

                _session.Save(user);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult ChangePassword(int id)
        {
            var user = _session.Users.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new ChangePassword
            {
                EmailAddress = user.UserName,
            });
        }

        [HttpPost]
        public ActionResult ChangePassword(int id, ChangePassword model)
        {
            if (ModelState.IsValid)
            {
                var user = _session.Users.SingleOrDefault(x => x.UserId == id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                WebSecurity.CreateAccount(user.UserName, model.Password, false);

                return RedirectToAction("Index");
            }
            return View(model);
        }


        public ActionResult Delete(int id)
        {
            var user = _session.Users.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection model)
        {
            var user = _session.Users.SingleOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var deleted = _membership.DeleteAccount(user.UserName);
            return RedirectToAction("Index");
        }

        [ChildActionOnly]
        public ActionResult Company(string id)
        {
            return PartialView(new SelectList(Repository.ToList().OrderBy(c=>c.CompanyName), "Id", "CompanyName", id));
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User already exists. Please enter a different email address.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}