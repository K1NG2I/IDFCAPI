using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RSuite.Domain.Common.Organization;
using RSuite.Domain.Common.Security;
using RSuite.Domain.Common.Security;
using RSuite.Infrastructure.Core.Base;
using RSuite.Infrastructure.Specification.Common.Organization;
using RSuite.Infrastructure.Specification.Common.Security;
using RSuite.Web.Base;
using RSuite.Infrastructure.Specification.Common;
using RSuite.Infrastructure.Core.Context;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace RSuite.UserInterface.Web.Mvc.Controllers
{
	/// <summary>
	/// Description of UserController.
	/// </summary>
	public class UserController : BaseController, IPersist<User>
	{
		private IUserRepository _userRepository;
        private User _user;
        IProfileRightService _profileRightService;
        IOrganizationQueryService _organizationQueryService;
        ILoginContextService _loginContextService;
        ICommonQueryService _commonQueryService;
		IQueryExecutor _queryExecutor;
        public UserController(IUserRepository UserRepository, IProfileRightService ProfileRightService, IOrganizationQueryService OrganizationQueryService, ICommonQueryService CommonQueryService, ILoginContextService LoginContextService, IQueryExecutor queryExecutor)
        {
            _userRepository = UserRepository;
            _profileRightService = ProfileRightService;
            _organizationQueryService=OrganizationQueryService;
            _loginContextService = LoginContextService;
            _commonQueryService = CommonQueryService;
			_queryExecutor = queryExecutor;
        } 
		public virtual ActionResult Index()
		{
			
			ViewBag.OrganizationLocationCollection = _organizationQueryService.GetOrganizationLocationCollection();
			 _user = _userRepository.GetByKeyId(KeyId);
			string FormMode = Request.QueryString["FormMode"];
            string formModeString = Encoding.Unicode.GetString(Convert.FromBase64String(FormMode));
			int UserId = _loginContextService.GetLoginContext().UserId;
            int formType = int.Parse(formModeString);
			ViewBag.FormMode=formType;
            
             IList<Profile>	ProfileCollection=new List<Profile>();
             if (_user.OrganizationLocationId > 0) 
			 {
			 	ProfileCollection=_profileRightService.GetProfileListByLocation(_user.OrganizationLocationId).ToList();
			 }
			 ViewBag.ProfileCollection=ProfileCollection;
             ViewBag.DepartmentCollection = _commonQueryService.GetDepartmentCollection();
            return View("~/Views/Administration/Rights/UserView.cshtml", _user);
		}
        public virtual User Save(User EntityDto)
        {
            EntityDto.IsAdminUser = _queryExecutor.ExecuteQuery<User>("Uid=" + EntityDto.Uid).FirstOrDefault().IsAdminUser;
            EntityDto.Password = GetPasswordByUserId(EntityDto);
			_user = SaveEntity(_userRepository, EntityDto);
            return _user;
		}
		public virtual ActionResult GetProfileByLocationId(int OrganizationLocationId=0)
		{
			var PrifileCollection=_profileRightService.GetProfileListByLocation(OrganizationLocationId);
			  return Json(new { profileName = PrifileCollection }, JsonRequestBehavior.AllowGet);
			
		}

		public string GetPasswordByUserId(User user)
		{
			if (user.Password == null)
			{
				if (user.Uid > 0)
				{
					var existingUser = _userRepository.GetByKeyId(user.Uid);
					if (existingUser != null)
					{
						return existingUser.Password;
					}
				}
			}
			return user.Password;
		}

    }
}