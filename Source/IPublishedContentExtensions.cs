using PAU.Core.Constants;
using PAU.Core.Helpers;
using PAU.Core.Models.Context;
using PAU.Core.Models.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace PAU.Core.Extensions
{
    public static class IPublishedContentExtensions
    {
        public static bool Is(this IPublishedContent node, params int[] ids)
        {
            return ids.Contains(node.Id);
        }

        public static string GetDropDownPropertyText(this IPublishedContent content, string property, string language)
        {
            return umbraco.library.GetPreValueAsString(Convert.ToInt32(content.CoaleseField(property, language)));
        }

        public static List<IPublishedContent> GetRelatedTypedContents(this IPublishedContent content, string property, string language)
        {
            if (!content.HasProperty(property))
                return null;

            if (content.HasProperty(property + language))
                property = property + language;

            var ids = content.CoaleseField(property, language);
            if (ids == null || ids == string.Empty)
                return new List<IPublishedContent>();
            return
                ids.Split(',').Select(id => AppContext.Current.UmbracoHelper.TypedContent(Convert.ToInt32(id))).Where(c => c != null).ToList();

        }

        public static string CoaleseField(this IPublishedContent currentNode, string property, string language = "")
        {
            if (currentNode == null)
                return "";

            if (currentNode.HasProperty(property + language))
                property = property + language;

            var content = currentNode.GetPropertyValue<string>(property);

            //make image full path
            return PauHelper.MakeContentImageFullPath(content);
        }

        public static string CoaleseFieldRecursively(this IPublishedContent currentNode, string property, bool recursive = false)
        {
            if (currentNode == null)
                return "";
            var content = currentNode.GetPropertyValue<string>(property, recursive);
            return PauHelper.MakeContentImageFullPath(content);
        }

        public static string CoaleseRelatedField(this IPublishedContent currentNode, string property, string relatedProperty, string language = "")
        {
            if (currentNode == null)
                return "";

            if (currentNode.HasProperty(property + language))
                property = property + language;

            var propValue = currentNode.GetPropertyValue<string>(property);
            if (String.IsNullOrEmpty(propValue))
                return "";

            return
                AppContext.Current.UmbracoHelper.TypedContent(Convert.ToInt32(propValue))
                    .CoaleseField(relatedProperty, language);
        }

        // Related Links
        public static Newtonsoft.Json.Linq.JArray CoaleseRelatedLinks(this IPublishedContent currentNode, string property, string language)
        {
            if (currentNode == null)
                return null;

            if (currentNode.HasProperty(property + language))
                property = property + language;

            return currentNode.GetPropertyValue<Newtonsoft.Json.Linq.JArray>(property);
        }

        public static string GetMediaUrl(this IPublishedContent content, string property, string language)
        {
            if (!content.HasProperty(property))
                return "";

            if (content.HasProperty(property + language))
                property = property + language;

            var media = AppContext.Current.MediaService.GetById(content.GetPropertyValue<int>(property));
            if (media == null)
                return "";

            var mediaUrl = media.GetValue(WellknownProperties.UmbracoFile).ToString();

            //make image full path
            return PauHelper.MakeImageFullPath(mediaUrl);
        }

        public static string GetMediaImageUrl(this IPublishedContent content, string property)
        {
            var helper = new UmbracoHelper(UmbracoContext.Current);
            var url = content.GetPropertyValue<int>(property) != 0 ? helper.TypedMedia(content.GetPropertyValue<int>(property)).Url : string.Empty;
            return PauHelper.MakeImageFullPath(url);
        }

        public static List<RelatedLinksItem> GetLinksInRelatedLinkField(this IPublishedContent content, string property, string language)
        {
            var lst = new List<RelatedLinksItem>();

            if (!content.HasProperty(property))
                return lst;

            if (content.HasProperty(property + language))
                property = property + language;

            var relatedLinks = content.GetPropertyValue<DynamicXml>(property);
            if (relatedLinks.Any())
            {
                foreach (dynamic item in relatedLinks)
                {
                    lst.Add(new RelatedLinksItem
                    {
                        LinkUrl = item.link,
                        IsNewNindow = (item.newwindow.Equals("1"))
                    });
                }
            }

            return lst;
        }

        
        public static Dictionary<string, string> GetKeyValuePairs(this IPublishedContent content, string property, string currentLanguage)
        {
            if (content.HasProperty(property + currentLanguage))
                property = property + currentLanguage;
            var keyValuePairs = content.GetPropertyValue<Dictionary<string, string>>(property);
            if (keyValuePairs != null)
                return keyValuePairs;
            return new Dictionary<string, string>();
        }
    }
}