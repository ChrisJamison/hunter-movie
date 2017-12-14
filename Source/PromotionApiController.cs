using PAU.Core.Models;
using PAU.Core.Models.Context.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using PAU.Core.Extensions;
using Umbraco.Web;
using PAU.Core.Constants;
using PAU.Core.Models.Promotions;
using Umbraco.Core.Models;
using PAU.Core.ActionFilters;

namespace PAU.Core.Controllers.Api
{
    public class PromotionApiController : BaseController
    {
        public PromotionApiController(IContext context)
            : base(context)
        {
        }

        public PromotionCodeDTO GetPromotionCodes(int campaignId, string codes)
        {
            if (string.IsNullOrEmpty(codes) || campaignId == 0)
            {
                return new PromotionCodeDTO
                {
                    Success = true,
                    Message = ""
                };
            }

            var campaign = new UmbracoHelper(UmbracoContext.Current).TypedContent(campaignId);

            string messages = string.Empty;
            bool success = true;
            var lstCodes = string.Empty;
            if (campaign != null)
            {
                lstCodes = campaign.CoaleseField(WellknownProperties.PromotionCodes);
                if (!string.IsNullOrEmpty(lstCodes))
                {
                    var arrCodes = lstCodes.SplitIntoCollection();
                    foreach (var code in codes.SplitIntoCollection())
                    {
                        if (arrCodes.Any(code.Contains))
                        {
                            continue;
                        }
                        messages += code + ", ";
                    }
                }
                else
                {
                    success = false;
                    messages = string.Format("Code(s) {0} are invalid", codes.Replace(";", ", "));
                }
            }

            if (!string.IsNullOrEmpty(messages))
            {
                success = false;
                messages = string.Format("Code(s) {0} are invalid", messages.Substring(0, messages.Length - 2));
            }
            return new PromotionCodeDTO
            {
                Success = success,
                Message = messages
            };
        }

        public bool GetPromotionSerialExisted(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                return false;
            }

            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);
            IEnumerable<IPublishedContent> userClaimItemsNodes =
                                        helper.TypedContent(WellknownNodes.Promotions)
                                            .Descendants()
                                            .Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.UserClaimItem);

            return userClaimItemsNodes.Any(x => x.CoaleseField(WellknownProperties.SerialNumber) == serialNumber);
        }

        [HttpGet]
        public HttpResponseMessage GetFAQsItem(int? promotionId)
        {
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);

            IEnumerable<IPublishedContent> fAQsNodes =
                helper.TypedContent(WellknownNodes.Promotions)
                    .Descendants()
                    .Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.FAQsItem);

            fAQsNodes = fAQsNodes.Where(x => x.GetPropertyValue<int>(WellknownProperties.PromotionId) == promotionId);

            return Request.CreateResponse(fAQsNodes.Select(x => MapContentToFAQsItem(x)).ToList());
        }

        private FAQsItem MapContentToFAQsItem(IPublishedContent content)
        {
            return new FAQsItem
            {
                PromotionId = content.GetPropertyValue<int>(WellknownProperties.PromotionId),
                Content = content.GetPropertyValue<string>(WellknownProperties.FAQContent),
            };
        }

        #region Promotion API Integration

        

        #endregion
    }
}
