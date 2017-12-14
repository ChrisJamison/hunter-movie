using PAU.Core.Models.Context.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using PAU.Core.Helpers;
using Umbraco.Web.WebApi;
using PAU.Core.ActionFilters;

namespace PAU.Core.Controllers.Api
{
    public class TestApiController : UmbracoApiController
    {
        ///// <summary>
        /////  Access: /Umbraco/Api/TestApi/GetProducts
        ///// </summary>
        ///// <returns></returns>
        [HttpGet]
        public IEnumerable<string> GetProducts()
        {
            return new[] { "Table", "Chair", "Desk", "Computer", "Beer fridge" };
        }

        /// <summary>
        /// Access: /Umbraco/Api/TestApi/GetSecurityToken
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public HttpResponseMessage GetSecurityToken()
        {
            var sercurityScheme = ConfigurationHelper.AuthenticationScheme;
            var timespan = EncryptUtil.GetTimeStampUTC();
            var originalData = string.Format("{0}|{1}", sercurityScheme, timespan);
            var securityToken = EncryptUtil.EncryptData(originalData, ConfigurationHelper.PanasonicPublicKeyFile);
            return Request.CreateResponse(HttpStatusCode.OK, securityToken);
        }

        ///// <summary>
        /////  Access: /Umbraco/Api/PromotionApi/TestAuthorizationProducts
        ///// </summary>
        ///// <returns></returns>
        [AuthorizationRequiredAttribute]
        [HttpGet]
        public IEnumerable<string> GetAuthorizationProducts()
        {
            return new[] { "Table", "Chair", "Desk", "Computer", "Beer fridge" };
        }

        ///// <summary>
        /////  Access: /Umbraco/Api/PromotionApi/TestSecurityProducts
        ///// </summary>
        ///// <returns></returns>
        [SecurityRequiredAttribute]
        [HttpGet]
        public IEnumerable<string> GetSecurityProducts()
        {
            return new[] { "Table", "Chair", "Desk", "Computer", "Beer fridge" };
        }
    }
}