using PAU.Core.Constants;
using PAU.Core.Filters;
using PAU.Core.Helpers;
using PAU.Core.Models.Base;
using PAU.Core.Models.Context.Interface;
using PAU.Core.Services;
using PAU.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PAU.Core.ActionFilters;
using PAU.Core.Models;

namespace PAU.Core.Controllers.Api
{
    public class AuthenticateApiController : BaseController
    {
        private readonly ITokenServices _tokenServices;
        private readonly IUserServices _userServices;
        private readonly IEloquaService _eloquaService;

        public AuthenticateApiController(IContext context, IUserServices userServices, ITokenServices tokenServices, IEloquaService iEloquaService)
            : base(context)
        {
            _userServices = userServices;
            _tokenServices = tokenServices;
            _eloquaService = iEloquaService;
        }

        /// <summary>
        /// Authenticates user and returns token with expiry.
        /// Access: /Umbraco/Api/AuthenticateApi/Login
        /// </summary>
        /// <returns></returns>
        [ApiAuthenticationFilter]
        [HttpPost]
        public HttpResponseMessage Login()
        {
            if (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var basicAuthenticationIdentity = System.Threading.Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
                if (basicAuthenticationIdentity != null)
                {
                    return GetAuthenticatedUserInfo(basicAuthenticationIdentity);
                }
            }

            return null;
        }

        /// <summary>
        /// Access: /Umbraco/Api/AuthenticateApi/Register
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [SecurityRequired]
        public HttpResponseMessage Register([FromBody]RegisterAccountModel model)
        {
            try
            {
                if (ModelState.ContainsKey("model.DateOfBirth") && ModelState["model.DateOfBirth"].Errors.Any(x => x.Exception != null))
                {
                    ModelState.AddModelError("DateOfBirth", WellknownResponseMessages.InvalidBirthDateFormat);
                }

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.PAUMemberSource))
                    {
                        model.PAUMemberSource = PauHelper.PromotionSite;
                    }
                    var contactDetails = PauHelper.MapRegisterAccountModelToContactDetailEntity(model);
                    contactDetails.PasswordResetGuid = Guid.NewGuid().ToString();
                    _eloquaService.CreateOrUpdateContact(contactDetails);
                    return Request.CreateResponse(HttpStatusCode.OK, new ResponseDTO
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = WellknownResponseMessages.Success
                    });
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                {
                    Success = false,
                    StatusCode = 400,
                    Message = WellknownResponseMessages.Falied,
                    Errors = ModelState.AllErrors()
                });
            }

            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace }
                });
            }

        }

        /// <summary>
        /// Access: /Umbraco/Api/AuthenticateApi/CheckUserExisting
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        [HttpGet]
        [SecurityRequired]
        public HttpResponseMessage CheckUserExisting(string emailAddress)
        {
            try
            {
                var userInfo = _eloquaService.GetContactDetailsByEmail(emailAddress);
                if (userInfo == null || (userInfo != null && userInfo.MyPanasonicMember != 1))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new ResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.EmailNotExisted
                    });
                }                

                ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailExisted);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                {
                    Success = true,
                    StatusCode = 200,
                    Message = WellknownResponseMessages.EmailExisted,
                    Errors = ModelState.AllErrors()
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace }
                });
            }
        }
        /// <summary>
        /// Returns auth token for the validated user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private HttpResponseMessage GetAuthenticatedUserInfo(BasicAuthenticationIdentity basicIdentity)
        {
            try
            {
                //Delete old token by userid
                _tokenServices.DeleteByUserEmail(basicIdentity.UserName);
                //Generate new token
                var token = _tokenServices.GenerateToken(basicIdentity.UserName, basicIdentity.EloquaContactId);

                var responseDTO = new ResponseDTO
                {
                    Success = true,
                    StatusCode = 200,
                    Message = WellknownResponseMessages.Success
                };

                var response = Request.CreateResponse(HttpStatusCode.OK, responseDTO);
                response.Headers.Add("Token", token.AuthToken);
                response.Headers.Add("TokenExpiry", ConfigurationHelper.AuthTokenExpiry.ToString());
                response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");
                return response;
            }
            catch (Exception ex)
            {
                //Data.EventLog.LogAPIErrorException(userId, string.Empty, ex.ToString());

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace}
                });
            }

        }
    }
}