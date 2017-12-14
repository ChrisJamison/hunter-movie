using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using Dapper;
using PAU.Core.Helpers;
using PAU.Core.Models;
using PAU.Core.Models.Context.Interface;
using Umbraco.Core;

namespace PAU.Core.Controllers.Api
{
    public class ProductController : BaseController
    {
        private IDbConnection dbCon =
            new SqlConnection(ConfigurationManager.ConnectionStrings["crmCache"].ConnectionString);
        public ProductController(IContext context)
            : base(context)
        {
        }

        [HttpGet]
        public IEnumerable<ProductCategory> GetProductCategories()
        {
            var result = dbCon.Query<ProductCategory>("WebsiteProductCategoryLoad_Categories", new { Type = 1 }, commandType: CommandType.StoredProcedure);
            result = result.Where(x => x.Status == "Active");
            return result;
        }

        [HttpGet]
        public IEnumerable<ASCProductCategory> GetASCProductCategories()
        {
            var listLimmitedUser = ConfigurationHelper.ASCLocatorUserLimmitedAccess.ToLower().Trim().Split(';').ForEach(x => x.Split('|'));
            var result = listLimmitedUser.Any(x => x[0].Equals(User.Identity.Name.ToLower()))
                ? dbCon.Query<ASCProductCategory>("ParCategory5Load_Sorted",
                    commandType: CommandType.StoredProcedure).Where(x => IsLimmitUserCategories(x.Category5, listLimmitedUser.First(y => y[0].Equals(User.Identity.Name.ToLower()))[1]))
                : dbCon.Query<ASCProductCategory>("ParCategory5Load_Sorted",
                    commandType: CommandType.StoredProcedure);
            return result;
        }

        [HttpGet]
        public IEnumerable<ProductCategory> GetProductSubCategories(int productCategoryId)
        {
            if (productCategoryId != 0)
            {
                var result = dbCon.Query<ProductCategory>("WebsiteProductCategoryLoad_SubcategoriesByCategoryId", new { CategoryId = productCategoryId, IncludeEmptyCats = false }, commandType: CommandType.StoredProcedure);
                result = result.Where(x => x.Status == "Active").Select(ChangeSubCategoryName).OrderBy(p => p.Name);
                return result;
            }
            return null;
        }

        [HttpGet]
        public IEnumerable<ProductModel> GetProductModels(int productCategoryId, int productSubCategoryId = 0)
        {
            IEnumerable<ProductModel> result = null;
            if (productSubCategoryId != 0)
            {
                result = dbCon.Query<ProductModel>("ProductLoad_BySubcategoryId",
                    new { SubcategoryId = productSubCategoryId }, commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = dbCon.Query<ProductModel>("ProductLoad_ByCategoryId",
                    new { CategoryId = productCategoryId }, commandType: CommandType.StoredProcedure);
            }
            result = result.Where(x => x.Status == "Active").OrderBy(x => x.ProductCode);
            return result;
        }

        private static ProductCategory ChangeSubCategoryName(ProductCategory productCategory)
        {
            productCategory.Name = productCategory.Name.ToLower() == "kitchen appliances" ? "Small Kitchen Appliances" : productCategory.Name;
            return productCategory;
        }

        private static bool IsLimmitUserCategories(string productCategoryId, string productCategoryIdList)
        {
            return productCategoryIdList.Split(',').Contains(productCategoryId.ToLower());
        }
    }
}