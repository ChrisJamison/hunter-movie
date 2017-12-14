using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Dapper;
using PAU.Core.Constants;
using PAU.Core.Extensions;
using PAU.Core.Models;
using PAU.Core.Models.Context.Interface;
using Umbraco.Core;
using Umbraco.Web;

namespace PAU.Core.Controllers.Api
{
    public class PauAccountController : BaseController
    {
        private IDbConnection dbCon = new SqlConnection(ConfigurationManager.ConnectionStrings["crmCache"].ConnectionString);
        public PauAccountController(IContext context)
            : base(context)
        {
        }

        [HttpGet]
        public IEnumerable<SparePartDistributor> GetSparePartDistributors(double lat, double lng, string type = "SPD", int rowCount = 10)
        {
            var spds = dbCon.Query<SparePartDistributor>("AccountLoad_ByProximity",
                new {Latitude = lat, Longitude = lng, Type = type, RowCount = rowCount},
                commandType: CommandType.StoredProcedure).OrderBy(c => c.Created);               
            spds.ForEach(s =>
            {
                s.OpeningHours = dbCon.Query<SpdOpeningHours>("OpeningHrsLoad_EffectiveByAccountId",
                    new {AccountId = s.AccountId},
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
                s.Phone = s.Phone.FormatPhone();
                s.Fax = s.Fax.FormatPhone();
                s.OtherPhone = s.OtherPhone.FormatPhone();
            });
            return spds;
        }


        [HttpGet]
        public ASCLocatorList GetASCLocator(double lat, double lng, string type = "ASC", int rowCount = 10, string productCategoryId = "")
        {
            var ascs =  dbCon.Query<SparePartDistributor>("AccountLoad_ByProximityCompetency",
                    new { Latitude = lat, Longitude = lng, Type = type, Cat5 = productCategoryId, WebsiteProductCategoryId = "", RowCount = rowCount },
                    commandType: CommandType.StoredProcedure);
            ascs.ForEach(s =>
            {
                s.OpeningHours = dbCon.Query<SpdOpeningHours>("OpeningHrsLoad_EffectiveByAccountId",
                    new { AccountId = s.AccountId },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
                s.Phone = s.Phone.FormatPhone();
                s.Fax = s.Fax.FormatPhone();
                s.OtherPhone = s.OtherPhone.FormatPhone();
            });

            var ascList = new ASCLocatorList {AscLocators = new List<SparePartDistributor>()};
            ascList.AscLocators.AddRange(ascs);

            if (type == "ASC")
            {
                var splash = dbCon.Query<InteruptionZone>("InterruptionZoneLoad_ActiveByLocationCat5",
                    new { Latitude = lat, Longitude = lng, Category5 = productCategoryId },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
                if (splash != null && !splash.SplashUrl.IsNullOrWhiteSpace())
                {
                    var splashRootPage = Umbraco.GetTypedContent(WellknownNodes.SplashPages);
                    var splashPagesContent = splashRootPage.Children().FirstOrDefault(x => x.GetPropertyValue<string>(WellknownProperties.SplashUrl).Equals(splash.SplashUrl)).CoaleseField(WellknownProperties.Content);
                    if (!string.IsNullOrEmpty(splashPagesContent))
                    {
                        splash.SplashContent = splashPagesContent;
                        ascList.RptResults = false;
                        ascList.LnkShowResults = bool.Parse(splash.ShowLink) && ascs.Any();
                    }
                    else
                    {
                        ascList.RptResults = true;
                    }
                }
                else
                {
                    ascList.RptResults = true;
                }
                ascList.InteruptionZone = splash;
            }
            return ascList;
        }
    }
}