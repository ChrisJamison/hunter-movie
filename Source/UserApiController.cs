using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Http;
using PAU.Core.ActionFilters;
using PAU.Core.Constants;
using PAU.Core.Helpers;
using PAU.Core.Models;
using PAU.Core.Models.Base;
using PAU.Core.Models.Context.Interface;
using PAU.Core.Models.LumixPromotions;
using PAU.Core.Services;
using PAU.Eloqua.Business;

namespace PAU.Core.Controllers.Api
{
    public class UserApiController : BaseController
    {
        private readonly IEloquaService _eloquaService;
        public UserApiController(IContext context, IEloquaService iEloquaService) : base(context)
        {
            _eloquaService = iEloquaService;
        }

        /// <summary>
        /// Access: /Umbraco/Api/UserApi/GetUserProfile
        /// </summary>
        /// <returns></returns>
        [AuthorizationRequired]
        public HttpResponseMessage GetUserProfile(string emailAddress)
        {
            try
            {
                var userInfo = _eloquaService.GetContactDetailsByEmail(emailAddress);
                if (userInfo == null)
                {
                    ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailNotExisted);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new UserProfileResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.Falied,
                        Errors = ModelState.AllErrors()
                    });
                }
                if (!ValidateIdentity(userInfo.EloquaContactId))
                {
                    ModelState.AddModelError("InvalidRequest", WellknownResponseMessages.InvalidRequest);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new UserProfileResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.Falied,
                        Errors = ModelState.AllErrors()
                    });
                }
                var response = PauHelper.MapContactDetailToRegisterAccountModel(userInfo);
                return Request.CreateResponse(HttpStatusCode.OK, new UserProfileResponseDTO
                {
                    Success = true,
                    StatusCode = 200,
                    Message = WellknownResponseMessages.Success,
                    UserInfo = response
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new UserProfileResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace }
                });
            }
        }

        /// <summary>
        /// Access: /Umbraco/Api/UserApi/UpdateUserAddress
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationRequired]
        public HttpResponseMessage UpdateUserAddress(AddressModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userInfo = _eloquaService.GetContactDetailsByEmail(model.EmailAddress);
                    if (userInfo == null)
                    {
                        ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailNotExisted);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }
                    if (!ValidateIdentity(userInfo.EloquaContactId))
                    {
                        ModelState.AddModelError("InvalidRequest", WellknownResponseMessages.InvalidRequest);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }
                    var contactDetails = PauHelper.MapAddressModelToContactDetails(userInfo, model);
                    var success = _eloquaService.UpdateContactDetails(contactDetails);
                    if (success)
                    {
                        var emailHelper = new EmailHelper();
                        emailHelper.SendUpdateProfile(contactDetails.id);
                        return Request.CreateResponse(HttpStatusCode.OK, new ResponseDTO
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = WellknownResponseMessages.Success
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = WellknownResponseMessages.Falied,
                        Errors = new List<string> { WellknownResponseMessages.ServerError }
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
        /// Access: /Umbraco/Api/UserApi/ForgotPassword
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SecurityRequired]
        public HttpResponseMessage ForgotPassword(PromotionForgetPassword model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userInfo = _eloquaService.GetContactDetailsByEmail(model.EmailAddress);
                    if (userInfo == null)
                    {
                        ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailNotExisted);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }
                    if (!_eloquaService.PromotionForgotPassword(userInfo))
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 500,
                            Message = WellknownResponseMessages.Falied,
                            Errors = new List<string> { WellknownResponseMessages.ServerError }
                        });
                    }
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
        /// Access: /Umbraco/Api/UserApi/UpdateResetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [SecurityRequired]
        public HttpResponseMessage UpdateResetPassword(PromotionResetPassword model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userInfo = _eloquaService.GetContactDetailsByEmail(model.EmailAddress);
                    if (userInfo == null)
                    {
                        ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailNotExisted);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }

                    if (!_eloquaService.UpdateResetPassword(model.EmailAddress, model.Password, model.ResetUID))
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 500,
                            Message = WellknownResponseMessages.Falied,
                            Errors = new List<string> { WellknownResponseMessages.ServerError }
                        });
                    }

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
        /// Access: /Umbraco/Api/UserApi/RegisterProduct
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationRequired]
        public HttpResponseMessage RegisterProduct(PromotionRegisteredProductModel model)
        {
            try
            {
                if (ModelState.ContainsKey("model.PurchaseDate") && ModelState["model.PurchaseDate"].Errors.Any(x => x.Exception != null))
                {
                    ModelState.AddModelError("PurchaseDate", WellknownResponseMessages.InvalidPurchaseDateFormat);
                }

                if (ModelState.IsValid)
                {
                    var userInfo = _eloquaService.GetContactDetailsByEmail(model.EmailAddress);
                    if (userInfo == null)
                    {
                        ModelState.AddModelError("EmailAddress", WellknownResponseMessages.EmailNotExisted);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }
                    if (!ValidateIdentity(userInfo.EloquaContactId))
                    {
                        ModelState.AddModelError("InvalidRequest", WellknownResponseMessages.InvalidRequest);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = WellknownResponseMessages.Falied,
                            Errors = ModelState.AllErrors()
                        });
                    }

                    // If exsited Registed Product, do nothing and return failed 
                    var isRegisteredProduct = _eloquaService.IsRegistedProduct(model.ProductCategory,
                        model.ProductSubCategory, model.ProductModelNumber, userInfo.EloquaContactId);
                    if (isRegisteredProduct)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseDTO
                        {
                            Success = false,
                            StatusCode = 400,                           
                            Message = WellknownResponseMessages.Falied,
                            Errors = new List<string> { WellknownResponseMessages.RegisteredProductExisted }
                        });
                    }
                    var registedProduct = PauHelper.MapPromotionRegistedProductModelToRegistedProductPost(model,
                        userInfo.EloquaContactId);
                    var success = _eloquaService.CreateRegisteredProduct(registedProduct);
                    if (success)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new ResponseDTO
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = WellknownResponseMessages.Success
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseDTO
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = WellknownResponseMessages.Falied,
                        Errors = new List<string> { WellknownResponseMessages.ServerError }
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
        private bool ValidateIdentity(string eloquaContactId)
        {
            return eloquaContactId == Thread.CurrentPrincipal.Identity.Name;
        }

        #region Lumix Promotion

        [HttpPost]
        [SecurityRequired]
        public HttpResponseMessage CreateOrUpdateLumixUserProfile([FromBody]LumixPromotionUserProfileModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var arr = model.Email.Split('@');
                    string email = string.Empty;
                    if (arr.Length == 2)
                    {
                        email = string.Format("{0}_{1}@{2}", arr[0], model.Id, arr[1]);
                        model.Email = email;
                    }

                    var userInfo = _eloquaService.GetLumixContactProfiles(model.Id);
                    if (userInfo != null && userInfo.Email != model.Email)
                    {
                        _eloquaService.ChangeEmailAddress(userInfo.EloquaContactId, userInfo.Email, model.Email);
                    }

                    var contactDetails = PauHelper.MapLumixAccountModelToContactDetailEntity(model);
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
        /// Access: /Umbraco/Api/UserApi/GetUserProfile
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SecurityRequired]
        public HttpResponseMessage GetLumixUserProfile(string id)
        {
            Regex regex = new Regex("^[0-9]{1,11}$");
            if (string.IsNullOrEmpty(id) || !regex.IsMatch(id))
            {
                ModelState.AddModelError("LumixUserProfile", WellknownResponseMessages.ContactIdRequired);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new LumixUserProfileResponseDTO
                {
                    Success = false,
                    StatusCode = 400,
                    Message = WellknownResponseMessages.Falied,
                    Errors = ModelState.AllErrors()
                });
            }
            try
            {
                var userInfo = _eloquaService.GetLumixContactProfiles(id);
                if (userInfo == null)
                {
                    ModelState.AddModelError("LumixUserProfile", string.Format(WellknownResponseMessages.AccountNotValid, id));
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new LumixUserProfileResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.Falied,
                        Errors = ModelState.AllErrors()
                    });
                }
                var response = PauHelper.MapContactDetailEntityToLumixAccountModel(userInfo);
                return Request.CreateResponse(HttpStatusCode.OK, new LumixUserProfileResponseDTO
                {
                    Success = true,
                    StatusCode = 200,
                    Message = WellknownResponseMessages.Success,
                    UserInfo = response
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new LumixUserProfileResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace }
                });
            }
        }


        /// <summary>
        /// Access: /Umbraco/Api/UserApi/SearchLumixUserProfile
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SecurityRequired]
        public HttpResponseMessage SearchLumixUserProfile(string terms)
        {
           
            try
            {
                       
                if (string.IsNullOrEmpty(terms) || string.IsNullOrWhiteSpace(terms))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new LumixListUserProfileResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.Falied,
                        Errors = ModelState.AllErrors()
                    });
                }
                if (terms.Equals("\"\""))
                {
                    ModelState.AddModelError("LumixUserProfile", WellknownResponseMessages.ValueCanNotBeNull);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new LumixListUserProfileResponseDTO
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = WellknownResponseMessages.Falied,
                        Errors = ModelState.AllErrors()
                    });
                }
                if (terms.StartsWith("\"") && terms.EndsWith("\""))
                {
                    terms = terms.Replace("\"", "");
                }
                var contactDetailses = _eloquaService.SearchLumixUserProfiles(terms.ToLower());
                return Request.CreateResponse(HttpStatusCode.OK, new LumixListUserProfileResponseDTO
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = WellknownResponseMessages.Success,
                        ListUserInfo = contactDetailses
                    });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new LumixListUserProfileResponseDTO
                {
                    Success = false,
                    StatusCode = 500,
                    Message = WellknownResponseMessages.Falied,
                    Errors = new List<string> { ex.Message + "\r\n" + ex.StackTrace }
                });
            }
        }

        #endregion
    }
}
