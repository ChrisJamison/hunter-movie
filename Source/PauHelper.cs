using PAU.Core.ConfigSections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PAU.Business.Annotations;
using PAU.Business.BusinessProcesses;
using PAU.Core.Constants;
using PAU.Core.Extensions;
using PAU.Core.Models;
using Umbraco.Web;
using PAU.Core.Models.Promotions;
using Umbraco.Core.Models;
using Newtonsoft.Json;
using PAU.Business.Extensions;
using PAU.Core.Models.LumixPromotions;

namespace PAU.Core.Helpers
{
    public static class PauHelper
    {
        private static UmbracoHelper helper = new UmbracoHelper(UmbracoContext.Current);
        public const string DefaultLabel = "Please select";
        public const string EmptyStr = "Empty String";
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string PromotionSite = "Promotion Site";

        //Meta Tags
        const string Description = "Panasonic Promotions";
        const string Title = "Panasonic Promotions";
        const string OGImage = "http://au.panasonic.com.au/images/panasonic_logo.png";
        private const string EloquaBlankData = " ";
        public static string MakeContentImageFullPath(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                return "";
            }

            var imageDomain = ConfigurationHelper.ImageDomain;
            if (!string.IsNullOrEmpty(imageDomain))
            {
                // Image source
                var replaceString = string.Empty;
                if (source.Contains("src"))
                {
                    replaceString = string.Format("src=\"//{0}/media", imageDomain);
                    source = source.Replace("src=\"/media", replaceString);
                }
                else
                {
                    replaceString = string.Format("//{0}/media/", imageDomain);
                    source = source.Replace("/media/", replaceString);
                }

                // Link href
                replaceString = string.Format("href=\"//{0}/media", imageDomain);
                source = source.Replace("href=\"/media", replaceString);
            }

            return source;
        }

        public static string MakeImageFullPath(string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                return "";
            }

            var imageDomain = ConfigurationHelper.ImageDomain;
            if (!string.IsNullOrEmpty(imageDomain))
            {
                var replaceString = string.Format("//{0}/media", imageDomain);
                source = source.Replace("/media", replaceString);
            }

            return source;
        }

        public static MatchCollection CollectContentBlocks(string text)
        {
            var regParagraph = new Regex("(<p(.*?)>(.*?)</p>)|(<h[1-6](.*?)>(.*?)</h[1-6]>)|(<ul(.*?)>(.*?)</ul>)|(<div(.*?)>(.*?)</div>)");
            return regParagraph.Matches(text.Replace("\n", String.Empty));
        }

        public static string[] GetNatureOfContact()
        {
            return new string[] { "Feedback or suggestion", "Compliment", "Product information", "Complaint", "Product fault", "Information about a promotion" };
        }

        public static int GetWellknownNodeId(string nodeName)
        {
            var nodeConfig = ConfigurationManager.GetSection("nodeConfig") as NodeConfigSection;
            if (nodeConfig == null)
            {
                throw new NullReferenceException("Configuration of Node is not set in the Web.config");
            }

            var nodeId = 0;
            var nodeItems = from WellknownNodesElement s in nodeConfig.WellknownNodes
                            where s.Name == nodeName
                            select s;

            if (nodeItems != null)
            {
                var item = nodeItems.FirstOrDefault();

                if (item.Name.ToLower() == nodeName.ToLower())
                {
                    switch (nodeConfig.Environment)
                    {
                        case "local":
                            nodeId = Convert.ToInt32(item.Local);
                            break;
                        case "staging":
                            nodeId = Convert.ToInt32(item.Staging);
                            break;
                        case "live":
                            nodeId = Convert.ToInt32(item.Live);
                            break;
                        default:
                            nodeId = Convert.ToInt32(item.Local);
                            break;
                    }
                }
            }

            return nodeId;
        }

        public static IEnumerable<SelectListItem> GetGenders(Gender value)
        {
            return new List<SelectListItem>
            {
                new SelectListItem() {Text = DefaultLabel, Value = ""},
                new SelectListItem() {Text = "Male", Value = "Male", Selected = value == Gender.Male},
                new SelectListItem() {Text = "Female", Value = "Female", Selected = value == Gender.Female}
            };
        }

        public static ContactDetails MapModelToContactDetailEntity(MembershipModel model, string password)
        {
            var result = new ContactDetails()
            {
                id = model.Id,
                FirstName = model.FirstName,
                Surname = model.Surname,
                Email = model.EmailAddress,
                Address = new AddressDetails()
                {
                    AddressLine1 = !string.IsNullOrEmpty(model.AddressLine1) ? model.AddressLine1 : EloquaBlankData,
                    AddressLine2 = !string.IsNullOrEmpty(model.AddressLine2) ? model.AddressLine2 : EloquaBlankData,
                    Country = !string.IsNullOrEmpty(model.Country) ? model.Country : EloquaBlankData,
                    Postcode = !string.IsNullOrEmpty(model.Postcode) ? model.Postcode : EloquaBlankData,
                    State = !string.IsNullOrEmpty(model.State) ? model.State : EloquaBlankData,
                    Suburb = !string.IsNullOrEmpty(model.Suburb) ? model.Suburb : EloquaBlankData
                },
                AgeGroup = model.AgeGroup,
                PGUnder18 = model.PgUnder18,
                Title = model.Title,
                Gender = model.Gender,
                MemberFacebook = model.MemberFacebook ? "1" : "0",
                MemberTwitter = model.MemberTwitter ? "1" : "0",
                MemberFlickr = model.MemberFlickr ? "1" : "0",
                MemberLinkedIn = model.MemberLinkedIn ? "1" : "0",
                MemberYouTube = model.MemberYoutube ? "1" : "0",
                Mobile = model.Mobile,
                PhoneAH = !string.IsNullOrEmpty(model.PhoneAH) ? model.PhoneAH : EloquaBlankData,
                PhoneBH = model.PhoneBH,
                SubscribeToNewsletter = model.SubscribeToNewsletter ? 1 : 0,
                MyPanasonicMember = 1,
                DateOfBirth = model.DateOfBirth.ToString("MM/dd/yyyy"),
                MemberStatus = model.MemberStatus ? Active : Inactive,
                PAU = "1"
            };
            if (!string.IsNullOrEmpty(password))
            {
                result.Password = PasswordEncryptor.Encrypt(password);
            }
            return result;
        }

        public static ContactDetails MapRegisterAccountModelToContactDetailEntity(RegisterAccountModel model)
        {
            var result = new ContactDetails()
            {
                FirstName = model.FirstName,
                Surname = model.LastName,
                Email = model.EmailAddress,
                Address = new AddressDetails()
                {
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    Postcode = model.Postcode != EmptyStr ? model.Postcode : string.Empty,
                    State = model.State,
                    Suburb = model.Suburb
                },
                PGUnder18 = model.Under18Years ? 1 : 0,
                PhoneAH = model.Phone,
                SubscribeToNewsletter = model.Newsletter ? 1 : 0,
                MyPanasonicMember = 1,
                DateOfBirth = model.DateOfBirth.ToString("MM/dd/yyyy"),
                MemberStatus = Active,
                PAU = "1",
                PAUMemberSource = model.PAUMemberSource
            };
            if (!string.IsNullOrEmpty(model.Password))
            {
                result.Password = PasswordEncryptor.Encrypt(model.Password);
            }
            return result;
        }

        public static RegisterAccountResponseModel MapContactDetailToRegisterAccountModel(ContactDetails model)
        {
            var date = Convert.ToDouble(model.DateOfBirth).UnixTimeStampToDateTime();
            var result = new RegisterAccountResponseModel()
            {
                FirstName = model.FirstName,
                LastName = model.Surname,
                EmailAddress = model.Email,
                Phone = model.PhoneAH,
                Under18Years = model.PGUnder18 == 1,
                Newsletter = model.SubscribeToNewsletter == 1,
                DateOfBirth = string.Format("{0:dd/MM/yyyy}", date),
                AddressLine1 = model.Address.AddressLine1,
                AddressLine2 = model.Address.AddressLine2,
                Postcode = model.Address.Postcode,
                Suburb = model.Address.Suburb,
                State = model.Address.State
            };
            return result;
        }

        public static RegisteredProductPost MapPromotionRegistedProductModelToRegistedProductPost(
            PromotionRegisteredProductModel model, string eloquaContactId)
        {
            var result = new RegisteredProductPost
            {
                ProductCategory = model.ProductCategory,
                ProductSubcategory = model.ProductSubCategory,
                ModelName = model.ProductModelNumber,
                ProductSerialNumber = model.ProductSerialNumber,
                PurchaseDate = model.PurchaseDate.ToEloquaUnixTime().ToString(),
                DealersName = model.DealerName,
                CampaignCode = model.CampaignCode,
                CampaignDescription = model.CampaignDescription,
                EloquaContactID = eloquaContactId,
                EmailAddress = model.EmailAddress
            };
            return result;
        }
        public static ContactDetails MapAddressModelToContactDetails(ContactDetails contactDetails, AddressModel model)
        {
            contactDetails.Address = new AddressDetails
            {
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                Postcode = model.Postcode != EmptyStr ? model.Postcode : string.Empty,
                State = model.State,
                Suburb = model.Suburb
            };
            return contactDetails;
        }

        public static ContactDetails CreateContactDetails(ContactInfo model)
        {
            return new ContactDetails()
            {
                FirstName = model.FirstName,
                Surname = model.LastName,
                Email = model.EmailAddress,
                Password = string.Empty,
                PhoneAH = model.PhoneNumber,
                PAU = "1",
                MyPanasonicMember = 0,
                PasswordResetGuid = Guid.NewGuid().ToString(),
                Address = new AddressDetails()
                {
                    Postcode = model.Postcode
                }
            };
        }
        public static ContactDetails UpdateContactDetailsFromContactInfo(ContactDetails contactDetails, ContactInfo model)
        {
            contactDetails.FirstName = model.FirstName;
            contactDetails.Surname = model.LastName;
            contactDetails.PhoneAH = model.PhoneNumber;
            contactDetails.Address.Postcode = model.Postcode;
            contactDetails.PasswordResetGuid = Guid.NewGuid().ToString();
            return contactDetails;
        }

        public static ContactDetails MapNewsletterModelToContactDetailEntity(NewsletterSignUpModel model)
        {
            var guid = Guid.NewGuid().ToString();

            var result = new ContactDetails
            {
                FirstName = model.FirstName,
                Surname = model.Surname,
                Email = model.EmailAddress,
                Password = string.Empty,
                PasswordResetGuid = guid,
                Title = model.Title,
                SubscribeToNewsletter = 0,
                PAU = "1",
                MyPanasonicMember = 0,
                Address = new AddressDetails()
            };
            return result;
        }

        public static ContactProfiles MapModelToContactProfileEntity(MembershipModel model, string eloquaContactId)
        {
            var productInterests = model.ProductInterests ?? new ProductInterest();
            var profileOptions = model.ProfileOptions ?? new ProfileOption();
            return new ContactProfiles()
            {
                EmailAddress = model.EmailAddress,
                EloquaContactId = eloquaContactId,
                Accessories = productInterests.Accessories.ToString(),
                AirConditioner = productInterests.AirConditioner.ToString(),
                AudioHiFi = productInterests.AudioHifi.ToString(),
                Camcorder = productInterests.Camcorder.ToString(),
                LumixCamera = productInterests.LumixCamera.ToString(),
                ToughbookComputer = productInterests.ToughbookComputer.ToString(),
                PowerTools = productInterests.PowerTools.ToString(),
                ProfessionalVideo = productInterests.ProfessinalVideo.ToString(),
                HomeEntertainment = productInterests.HomeEntertainment.ToString(),
                HouseholdElectronics = productInterests.HouseholdElectronics.ToString(),
                VIErATV = productInterests.VIErATV.ToString(),
                Officesolutions = productInterests.Officesolutions.ToString(),
                Securitysolutions = productInterests.Securitysolutions.ToString(),
                Educationsolutions = productInterests.Educationsolutions.ToString(),
                Displaysolutions = productInterests.Displaysolutions.ToString(),
                ReadInstructions = profileOptions.ReadInstructions,
                StartWithBasicFunctions = profileOptions.StartWithBasicFunctions,
                CustomiseToSuit = profileOptions.CustomiseToSuit,
                FigureOutFunctions = profileOptions.FigureOutFunctions,
                UseTechnologyAsMuchAsPossible = profileOptions.UseTechnologyAsMuchAsPossible,
                ExictedToTryOutItem = profileOptions.ExcitedToTryOutItem,
                PhotographyAndFilm = productInterests.PhotographyAndFilm.ToString(),
                Telecommunications = productInterests.Telecommunications.ToString()
            };
        }

        public static NameValueCollection BuildParamenters<T>(T content) where T : class
        {
            var vals = new NameValueCollection();

            var properties = content.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var propertyInfo = Attribute.GetCustomAttribute(property, typeof(CustomObjectFieldAttribute)) as CustomObjectFieldAttribute;

                if (propertyInfo != null)
                {
                    var value = content.GetType().GetProperty(property.Name).GetValue(content, null);

                    vals.Add(propertyInfo.Name, value == null ? "" : value.ToString());
                }
            }

            return vals;
        }

        public static MembershipModel MapContactDetailAndContactProfileToModel(ContactDetails contactDetail, ContactProfiles contactProfiles)
        {
            var trueStr = bool.TrueString;
            var one = "1";
            var model = new MembershipModel()
            {
                Id = contactDetail.id,
                Title = contactDetail.Title,
                FirstName = contactDetail.FirstName,
                Surname = contactDetail.Surname,
                Gender = contactDetail.Gender,
                AddressLine1 = contactDetail.Address.AddressLine1,
                AddressLine2 = contactDetail.Address.AddressLine2,
                AgeGroup = contactDetail.AgeGroup,
                Country = contactDetail.Address.Country,
                Suburb = contactDetail.Address.Suburb,
                State = contactDetail.Address.State,
                Postcode = contactDetail.Address.Postcode,
                EmailAddress = contactDetail.Email,
                EmailConfirmation = contactDetail.Email,
                PhoneAH = contactDetail.PhoneAH,
                PhoneBH = contactDetail.PhoneBH,
                Mobile = contactDetail.Mobile,
                MemberFacebook = contactDetail.MemberFacebook == one,
                MemberLinkedIn = contactDetail.MemberLinkedIn == one,
                MemberYoutube = contactDetail.MemberYouTube == one,
                MemberTwitter = contactDetail.MemberTwitter == one,
                MemberFlickr = contactDetail.MemberFlickr == one,
                SubscribeToNewsletter = contactDetail.SubscribeToNewsletter == 1 ? true : false,
                IsPanasonicMember = contactDetail.MyPanasonicMember,
                MemberStatus = (!string.IsNullOrEmpty(contactDetail.MemberStatus) && contactDetail.MemberStatus == PauHelper.Inactive) ? false : true,
                PgUnder18 = contactDetail.PGUnder18
            };

            if (!string.IsNullOrEmpty(contactDetail.DateOfBirth))
            {
                model.DateOfBirth = Convert.ToDouble(contactDetail.DateOfBirth).UnixTimeStampToDateTime();
            }

            if (contactProfiles == null)
            {
                contactProfiles = new ContactProfiles();
            }
            model.ProductInterests = new ProductInterest()
            {
                Accessories = contactProfiles.Accessories == trueStr,
                LumixCamera = contactProfiles.LumixCamera == trueStr,
                AirConditioner = contactProfiles.AirConditioner == trueStr,
                AudioHifi = contactProfiles.AudioHiFi == trueStr,
                Camcorder = contactProfiles.Camcorder == trueStr,
                HomeEntertainment = contactProfiles.HomeEntertainment == trueStr,
                ToughbookComputer = contactProfiles.ToughbookComputer == trueStr,
                PowerTools = contactProfiles.PowerTools == trueStr,
                Securitysolutions = contactProfiles.Securitysolutions == trueStr,
                HouseholdElectronics = contactProfiles.HouseholdElectronics == trueStr,
                ProfessinalVideo = contactProfiles.ProfessionalVideo == trueStr,
                VIErATV = contactProfiles.VIErATV == trueStr,
                Officesolutions = contactProfiles.Officesolutions == trueStr,
                Educationsolutions = contactProfiles.Educationsolutions == trueStr,
                Displaysolutions = contactProfiles.Displaysolutions == trueStr,
                PhotographyAndFilm = contactProfiles.PhotographyAndFilm == trueStr,
                Telecommunications = contactProfiles.Telecommunications == trueStr
            };
            model.ProfileOptions = new ProfileOption()
            {
                StartWithBasicFunctions = contactProfiles.StartWithBasicFunctions,
                ReadInstructions = contactProfiles.ReadInstructions,
                FigureOutFunctions = contactProfiles.FigureOutFunctions,
                ExcitedToTryOutItem = contactProfiles.ExictedToTryOutItem,
                UseTechnologyAsMuchAsPossible = contactProfiles.UseTechnologyAsMuchAsPossible,
                CustomiseToSuit = contactProfiles.CustomiseToSuit
            };
            return model;
        }

        public static IEnumerable<SelectListItem> GetAgeGroups()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() { Value="Under 18", Text = "Under 18"},
                new SelectListItem() { Value="18 - 24", Text = "18 - 24"},
                new SelectListItem() { Value="25 - 34", Text = "25 - 34"},
                new SelectListItem() { Value="35 - 44", Text = "35 - 44"},
                new SelectListItem() { Value="45 - 55", Text = "45 - 55"},
                new SelectListItem() { Value="Over 55", Text = "Over 55"}
            };
        }

        public static IEnumerable<SelectListItem> GetDealers()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() { Value="", Text = DefaultLabel},
                new SelectListItem() { Value="In-Store", Text = "In-Store"},
                new SelectListItem() { Value="Online", Text = "Online"},
                new SelectListItem() { Value="Other", Text = "Other"}
            };
        }

        public static IEnumerable<SelectListItem> GetCountries()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() {Value = "Afghanistan", Text = "Afghanistan"},
                new SelectListItem() {Value = "Åland Islands", Text = "Åland Islands"},
                new SelectListItem() {Value = "Albania", Text = "Albania"},
                new SelectListItem() {Value = "Algeria", Text = "Algeria"},
                new SelectListItem() {Value = "American Samoa", Text = "American Samoa"},
                new SelectListItem() {Value = "Andorra", Text = "Andorra"},
                new SelectListItem() {Value = "Angola", Text = "Angola"},
                new SelectListItem() {Value = "Anguilla", Text = "Anguilla"},
                new SelectListItem() {Value = "Antarctica", Text = "Antarctica"},
                new SelectListItem() {Value = "Antigua and Barbuda", Text = "Antigua and Barbuda"},
                new SelectListItem() {Value = "Argentina", Text = "Argentina"},
                new SelectListItem() {Value = "Armenia", Text = "Armenia"},
                new SelectListItem() {Value = "Aruba", Text = "Aruba"},
                new SelectListItem() {Value = "Australia", Text = "Australia"},
                new SelectListItem() {Value = "Austria", Text = "Austria"},
                new SelectListItem() {Value = "Azerbaijan", Text = "Azerbaijan"},
                new SelectListItem() {Value = "Bahamas", Text = "Bahamas"},
                new SelectListItem() {Value = "Bahrain", Text = "Bahrain"},
                new SelectListItem() {Value = "Bangladesh", Text = "Bangladesh"},
                new SelectListItem() {Value = "Barbados", Text = "Barbados"},
                new SelectListItem() {Value = "Belarus", Text = "Belarus"},
                new SelectListItem() {Value = "Belgium", Text = "Belgium"},
                new SelectListItem() {Value = "Belize", Text = "Belize"},
                new SelectListItem() {Value = "Benin", Text = "Benin"},
                new SelectListItem() {Value = "Bermuda", Text = "Bermuda"},
                new SelectListItem() {Value = "Bhutan", Text = "Bhutan"},
                new SelectListItem() {Value = "Bolivia", Text = "Bolivia"},
                new SelectListItem() {Value = "Bosnia and Herzegovina", Text = "Bosnia and Herzegovina"},
                new SelectListItem() {Value = "Botswana", Text = "Botswana"},
                new SelectListItem() {Value = "Bouvet Island", Text = "Bouvet Island"},
                new SelectListItem() {Value = "Brazil", Text = "Brazil"},
                new SelectListItem() {Value = "Brit/Indian Ocean Terr.", Text = "Brit/Indian Ocean Terr."},
                new SelectListItem() {Value = "Brunei Darussalam", Text = "Brunei Darussalam"},
                new SelectListItem() {Value = "Bulgaria", Text = "Bulgaria"},
                new SelectListItem() {Value = "Burkina Faso", Text = "Burkina Faso"},
                new SelectListItem() {Value = "Burundi", Text = "Burundi"},
                new SelectListItem() {Value = "Cambodia", Text = "Cambodia"},
                new SelectListItem() {Value = "Cameroon", Text = "Cameroon"},
                new SelectListItem() {Value = "Canada", Text = "Canada"},
                new SelectListItem() {Value = "Cape Verde", Text = "Cape Verde"},
                new SelectListItem() {Value = "Cayman Islands", Text = "Cayman Islands"},
                new SelectListItem() {Value = "Central African Republic", Text = "Central African Republic"},
                new SelectListItem() {Value = "Chad", Text = "Chad"},
                new SelectListItem() {Value = "Chile", Text = "Chile"},
                new SelectListItem() {Value = "China", Text = "China"},
                new SelectListItem() {Value = "Christmas Island", Text = "Christmas Island"},
                new SelectListItem() {Value = "Cocos (Keeling) Islands", Text = "Cocos (Keeling) Islands"},
                new SelectListItem() {Value = "Colombia", Text = "Colombia"},
                new SelectListItem() {Value = "Comoros", Text = "Comoros"},
                new SelectListItem() {Value = "Congo", Text = "Congo"},
                new SelectListItem() {Value = "Congo, The Dem. Republic Of ", Text = "Congo, The Dem. Republic Of "},
                new SelectListItem() {Value = "Cook Islands", Text = "Cook Islands"},
                new SelectListItem() {Value = "Costa Rica", Text = "Costa Rica"},
                new SelectListItem() {Value = "Côte D'Ivore", Text = "Côte D'Ivore"},
                new SelectListItem() {Value = "Croatia", Text = "Croatia"},
                new SelectListItem() {Value = "Cuba", Text = "Cuba"},
                new SelectListItem() {Value = "Cyprus", Text = "Cyprus"},
                new SelectListItem() {Value = "Czech Republic", Text = "Czech Republic"},
                new SelectListItem() {Value = "Denmark", Text = "Denmark"},
                new SelectListItem() {Value = "Djibouti", Text = "Djibouti"},
                new SelectListItem() {Value = "Dominica", Text = "Dominica"},
                new SelectListItem() {Value = "Dominican Republic", Text = "Dominican Republic"},
                new SelectListItem() {Value = "Ecuador", Text = "Ecuador"},
                new SelectListItem() {Value = "Egypt", Text = "Egypt"},
                new SelectListItem() {Value = "El Salvador", Text = "El Salvador"},
                new SelectListItem() {Value = "Equatorial Guinea", Text = "Equatorial Guinea"},
                new SelectListItem() {Value = "Eritrea", Text = "Eritrea"},
                new SelectListItem() {Value = "Estonia", Text = "Estonia"},
                new SelectListItem() {Value = "Ethiopia", Text = "Ethiopia"},
                new SelectListItem() {Value = "Falkland Islands", Text = "Falkland Islands"},
                new SelectListItem() {Value = "Faroe Islands", Text = "Faroe Islands"},
                new SelectListItem() {Value = "Fiji", Text = "Fiji"},
                new SelectListItem() {Value = "Finland", Text = "Finland"},
                new SelectListItem() {Value = "France", Text = "France"},
                new SelectListItem() {Value = "French Guiana", Text = "French Guiana"},
                new SelectListItem() {Value = "French Polynesia", Text = "French Polynesia"},
                new SelectListItem() {Value = "French Southern Terr.", Text = "French Southern Terr."},
                new SelectListItem() {Value = "Gabon", Text = "Gabon"},
                new SelectListItem() {Value = "Gambia", Text = "Gambia"},
                new SelectListItem() {Value = "Georgia", Text = "Georgia"},
                new SelectListItem() {Value = "Germany", Text = "Germany"},
                new SelectListItem() {Value = "Ghana", Text = "Ghana"},
                new SelectListItem() {Value = "Gibraltar", Text = "Gibraltar"},
                new SelectListItem() {Value = "Greece", Text = "Greece"},
                new SelectListItem() {Value = "Greenland", Text = "Greenland"},
                new SelectListItem() {Value = "Grenada", Text = "Grenada"},
                new SelectListItem() {Value = "Guadeloupe", Text = "Guadeloupe"},
                new SelectListItem() {Value = "Guam", Text = "Guam"},
                new SelectListItem() {Value = "Guatemala", Text = "Guatemala"},
                new SelectListItem() {Value = "Guinea", Text = "Guinea"},
                new SelectListItem() {Value = "Guinea-Bissau", Text = "Guinea-Bissau"},
                new SelectListItem() {Value = "Guyana", Text = "Guyana"},
                new SelectListItem() {Value = "Haiti", Text = "Haiti"},
                new SelectListItem() {Value = "Heard/McDonald Isls.", Text = "Heard/McDonald Isls."},
                new SelectListItem() {Value = "Honduras", Text = "Honduras"},
                new SelectListItem() {Value = "Hong Kong", Text = "Hong Kong"},
                new SelectListItem() {Value = "Hungary", Text = "Hungary"},
                new SelectListItem() {Value = "Iceland", Text = "Iceland"},
                new SelectListItem() {Value = "India", Text = "India"},
                new SelectListItem() {Value = "Indonesia", Text = "Indonesia"},
                new SelectListItem() {Value = "Iran", Text = "Iran"},
                new SelectListItem() {Value = "Iraq", Text = "Iraq"},
                new SelectListItem() {Value = "Ireland", Text = "Ireland"},
                new SelectListItem() {Value = "Israel", Text = "Israel"},
                new SelectListItem() {Value = "Italy", Text = "Italy"},
                new SelectListItem() {Value = "Jamaica", Text = "Jamaica"},
                new SelectListItem() {Value = "Japan", Text = "Japan"},
                new SelectListItem() {Value = "Jordan", Text = "Jordan"},
                new SelectListItem() {Value = "Kazakhstan", Text = "Kazakhstan"},
                new SelectListItem() {Value = "Kenya", Text = "Kenya"},
                new SelectListItem() {Value = "Kiribati", Text = "Kiribati"},
                new SelectListItem() {Value = "Korea (North)", Text = "Korea (North)"},
                new SelectListItem() {Value = "Korea (South)", Text = "Korea (South)"},
                new SelectListItem() {Value = "Kuwait", Text = "Kuwait"},
                new SelectListItem() {Value = "Kyrgyzstan", Text = "Kyrgyzstan"},
                new SelectListItem() {Value = "Laos", Text = "Laos"},
                new SelectListItem() {Value = "Latvia", Text = "Latvia"},
                new SelectListItem() {Value = "Lebanon", Text = "Lebanon"},
                new SelectListItem() {Value = "Lesotho", Text = "Lesotho"},
                new SelectListItem() {Value = "Liberia", Text = "Liberia"},
                new SelectListItem() {Value = "Libya", Text = "Libya"},
                new SelectListItem() {Value = "Liechtenstein", Text = "Liechtenstein"},
                new SelectListItem() {Value = "Lithuania", Text = "Lithuania"},
                new SelectListItem() {Value = "Luxembourg", Text = "Luxembourg"},
                new SelectListItem() {Value = "Macau", Text = "Macau"},
                new SelectListItem() {Value = "Macedonia", Text = "Macedonia"},
                new SelectListItem() {Value = "Madagascar", Text = "Madagascar"},
                new SelectListItem() {Value = "Malawi", Text = "Malawi"},
                new SelectListItem() {Value = "Malaysia", Text = "Malaysia"},
                new SelectListItem() {Value = "Maldives", Text = "Maldives"},
                new SelectListItem() {Value = "Mali", Text = "Mali"},
                new SelectListItem() {Value = "Malta", Text = "Malta"},
                new SelectListItem() {Value = "Marshall Islands", Text = "Marshall Islands"},
                new SelectListItem() {Value = "Martinique", Text = "Martinique"},
                new SelectListItem() {Value = "Mauritania", Text = "Mauritania"},
                new SelectListItem() {Value = "Mauritius", Text = "Mauritius"},
                new SelectListItem() {Value = "Mayotte", Text = "Mayotte"},
                new SelectListItem() {Value = "Mexico", Text = "Mexico"},
                new SelectListItem() {Value = "Micronesia", Text = "Micronesia"},
                new SelectListItem() {Value = "Moldova", Text = "Moldova"},
                new SelectListItem() {Value = "Monaco", Text = "Monaco"},
                new SelectListItem() {Value = "Mongolia", Text = "Mongolia"},
                new SelectListItem() {Value = "Montserrat", Text = "Montserrat"},
                new SelectListItem() {Value = "Morocco", Text = "Morocco"},
                new SelectListItem() {Value = "Mozambique", Text = "Mozambique"},
                new SelectListItem() {Value = "Myanmar", Text = "Myanmar"},
                new SelectListItem() {Value = "N. Mariana Isls.", Text = "N. Mariana Isls."},
                new SelectListItem() {Value = "Namibia", Text = "Namibia"},
                new SelectListItem() {Value = "Nauru", Text = "Nauru"},
                new SelectListItem() {Value = "Nepal", Text = "Nepal"},
                new SelectListItem() {Value = "Netherlands", Text = "Netherlands"},
                new SelectListItem() {Value = "Netherlands Antilles", Text = "Netherlands Antilles"},
                new SelectListItem() {Value = "New Caledonia", Text = "New Caledonia"},
                new SelectListItem() {Value = "New Zealand", Text = "New Zealand"},
                new SelectListItem() {Value = "Nicaragua", Text = "Nicaragua"},
                new SelectListItem() {Value = "Niger", Text = "Niger"},
                new SelectListItem() {Value = "Nigeria", Text = "Nigeria"},
                new SelectListItem() {Value = "Niue", Text = "Niue"},
                new SelectListItem() {Value = "Norfolk Island", Text = "Norfolk Island"},
                new SelectListItem() {Value = "Norway", Text = "Norway"},
                new SelectListItem() {Value = "Oman", Text = "Oman"},
                new SelectListItem() {Value = "Pakistan", Text = "Pakistan"},
                new SelectListItem() {Value = "Palau", Text = "Palau"},
                new SelectListItem()
                {
                    Value = "Palestinian Territory, Occupied",
                    Text = "Palestinian Territory, Occupied"
                },
                new SelectListItem() {Value = "Panama", Text = "Panama"},
                new SelectListItem() {Value = "Papua New Guinea", Text = "Papua New Guinea"},
                new SelectListItem() {Value = "Paraguay", Text = "Paraguay"},
                new SelectListItem() {Value = "Peru", Text = "Peru"},
                new SelectListItem() {Value = "Philippines", Text = "Philippines"},
                new SelectListItem() {Value = "Pitcairn", Text = "Pitcairn"},
                new SelectListItem() {Value = "Poland", Text = "Poland"},
                new SelectListItem() {Value = "Portugal", Text = "Portugal"},
                new SelectListItem() {Value = "Puerto Rico", Text = "Puerto Rico"},
                new SelectListItem() {Value = "Qatar", Text = "Qatar"},
                new SelectListItem() {Value = "Reunion", Text = "Reunion"},
                new SelectListItem() {Value = "Romania", Text = "Romania"},
                new SelectListItem() {Value = "Russian Federation", Text = "Russian Federation"},
                new SelectListItem() {Value = "Rwanda", Text = "Rwanda"},
                new SelectListItem() {Value = "Saint Kitts and Nevis", Text = "Saint Kitts and Nevis"},
                new SelectListItem() {Value = "Saint Lucia", Text = "Saint Lucia"},
                new SelectListItem() {Value = "Samoa", Text = "Samoa"},
                new SelectListItem() {Value = "San Marino", Text = "San Marino"},
                new SelectListItem() {Value = "Sao Tome/Principe", Text = "Sao Tome/Principe"},
                new SelectListItem() {Value = "Saudi Arabia", Text = "Saudi Arabia"},
                new SelectListItem() {Value = "Senegal", Text = "Senegal"},
                new SelectListItem() {Value = "Serbia and Montenegro", Text = "Serbia and Montenegro"},
                new SelectListItem() {Value = "Seychelles", Text = "Seychelles"},
                new SelectListItem() {Value = "Sierra Leone", Text = "Sierra Leone"},
                new SelectListItem() {Value = "Singapore", Text = "Singapore"},
                new SelectListItem() {Value = "Slovak Republic", Text = "Slovak Republic"},
                new SelectListItem() {Value = "Slovenia", Text = "Slovenia"},
                new SelectListItem() {Value = "Solomon Islands", Text = "Solomon Islands"},
                new SelectListItem() {Value = "Somalia", Text = "Somalia"},
                new SelectListItem() {Value = "South Africa", Text = "South Africa"},
                new SelectListItem() {Value = "Spain", Text = "Spain"},
                new SelectListItem() {Value = "Sri Lanka", Text = "Sri Lanka"},
                new SelectListItem() {Value = "St. Helena", Text = "St. Helena"},
                new SelectListItem() {Value = "St. Pierre and Miquelon", Text = "St. Pierre and Miquelon"},
                new SelectListItem() {Value = "St. Vincent and Grenadines", Text = "St. Vincent and Grenadines"},
                new SelectListItem() {Value = "Sudan", Text = "Sudan"},
                new SelectListItem() {Value = "Suriname", Text = "Suriname"},
                new SelectListItem() {Value = "Svalbard/Jan Mayen Isls.", Text = "Svalbard/Jan Mayen Isls."},
                new SelectListItem() {Value = "Swaziland", Text = "Swaziland"},
                new SelectListItem() {Value = "Sweden", Text = "Sweden"},
                new SelectListItem() {Value = "Switzerland", Text = "Switzerland"},
                new SelectListItem() {Value = "Syria", Text = "Syria"},
                new SelectListItem() {Value = "Taiwan", Text = "Taiwan"},
                new SelectListItem() {Value = "Tajikistan", Text = "Tajikistan"},
                new SelectListItem() {Value = "Tanzania", Text = "Tanzania"},
                new SelectListItem() {Value = "Thailand", Text = "Thailand"},
                new SelectListItem() {Value = "Timor-Leste", Text = "Timor-Leste"},
                new SelectListItem() {Value = "Togo", Text = "Togo"},
                new SelectListItem() {Value = "Tokelau", Text = "Tokelau"},
                new SelectListItem() {Value = "Tonga", Text = "Tonga"},
                new SelectListItem() {Value = "Trinidad and Tobago", Text = "Trinidad and Tobago"},
                new SelectListItem() {Value = "Tunisia", Text = "Tunisia"},
                new SelectListItem() {Value = "Turkey", Text = "Turkey"},
                new SelectListItem() {Value = "Turkmenistan", Text = "Turkmenistan"},
                new SelectListItem() {Value = "Turks/Caicos Isls.", Text = "Turks/Caicos Isls."},
                new SelectListItem() {Value = "Tuvalu", Text = "Tuvalu"},
                new SelectListItem() {Value = "Uganda", Text = "Uganda"},
                new SelectListItem() {Value = "Ukraine", Text = "Ukraine"},
                new SelectListItem() {Value = "United Arab Emirates", Text = "United Arab Emirates"},
                new SelectListItem() {Value = "United Kingdom", Text = "United Kingdom"},
                new SelectListItem() {Value = "United States", Text = "United States"},
                new SelectListItem() {Value = "Uruguay", Text = "Uruguay"},
                new SelectListItem() {Value = "US Minor Outlying Is.", Text = "US Minor Outlying Is."},
                new SelectListItem() {Value = "Uzbekistan", Text = "Uzbekistan"},
                new SelectListItem() {Value = "Vanuatu", Text = "Vanuatu"},
                new SelectListItem() {Value = "Vatican City", Text = "Vatican City"},
                new SelectListItem() {Value = "Venezuela", Text = "Venezuela"},
                new SelectListItem() {Value = "Viet Nam", Text = "Viet Nam"},
                new SelectListItem() {Value = "Virgin Islands (British)", Text = "Virgin Islands (British)"},
                new SelectListItem() {Value = "Virgin Islands (U.S.)", Text = "Virgin Islands (U.S.)"},
                new SelectListItem() {Value = "Wallis/Futuna Isls.", Text = "Wallis/Futuna Isls."},
                new SelectListItem() {Value = "Western Sahara", Text = "Western Sahara"},
                new SelectListItem() {Value = "Yemen", Text = "Yemen"},
                new SelectListItem() {Value = "Zambia", Text = "Zambia"},
                new SelectListItem() {Value = "Zimbabwe", Text = "Zimbabwe"}
            };
        }

        public static IEnumerable<SelectListItem> GetStates()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() { Value="", Text = DefaultLabel},
                new SelectListItem() {Value="ACT", Text = "ACT"},
                new SelectListItem() {Value="NSW", Text = "NSW"},
                new SelectListItem() {Value="NT", Text = "NT"},
                new SelectListItem() {Value="QLD", Text = "QLD"},
                new SelectListItem() {Value="SA", Text = "SA"},
                new SelectListItem() {Value="TAS", Text = "TAS"},
                new SelectListItem() {Value="VIC", Text = "VIC"},
                new SelectListItem() {Value="WA", Text = "WA"}
            };
        }

        public static IEnumerable<SelectListItem> GetTitles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem() {Value = "Mr.", Text = "Mr."},
                new SelectListItem() {Value = "Mrs.", Text = "Mrs."},
                new SelectListItem() {Value = "Ms.", Text = "Ms."},
                new SelectListItem() {Value = "Miss", Text = "Miss"},
                new SelectListItem() {Value = "Dr.", Text = "Dr."},
                new SelectListItem() {Value = "Prof.", Text = "Prof."}
            };
        }

        public static List<PromotionCategory> GetCategories()
        {
            return helper.TypedContent(WellknownNodes.Promotions).Children.OfTypes(WellknownDocumentTypes.Campaign).Select(x => new PromotionCategory
            {
                CategoryId = x.Id,
                ImageCategory = x.GetMediaImageUrl(WellknownProperties.Image),
                TitleCategory = x.CoaleseField(WellknownProperties.Title),
                UrlCategory = x.Url
            }).ToList();
        }
        public static string GetPromotionCategories()
        {
            var categories = GetCategories();
            return JsonConvert.SerializeObject(categories);
        }

        public static List<Promotion> GetListPromotions()
        {
            return helper.TypedContent(WellknownNodes.Promotions).Descendants().Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.Campaign)
                 .Where(CheckValidScheduleDate)
                 .Select(MapContentToPromotion).ToList();
        }

        public static string GetPromotions()
        {
            var promotions = GetListPromotions();

            return JsonConvert.SerializeObject(promotions);
        }

        private static Promotion MapContentToPromotion(IPublishedContent content)
        {
            return new Promotion
            {
                CampaignId = content.Id,
                CategoryId = string.IsNullOrEmpty(content.CoaleseField(WellknownProperties.Category)) ? 0 : int.Parse(content.CoaleseField(WellknownProperties.Category)),
                ClaimBy = content.GetPropertyValue<DateTime>(WellknownProperties.ClaimBy).ToString("dd/MM/yyyy"),
                PurchaseDateStart = content.GetPropertyValue<DateTime>(WellknownProperties.PurchaseDateStart).ToString("dd/MM/yyyy"),
                PurchaseDateEnd = content.GetPropertyValue<DateTime>(WellknownProperties.PurchaseDateEnd).ToString("dd/MM/yyyy"),
                TitleCampaign = content.CoaleseField(WellknownProperties.Title),
                TypeCampaign = string.IsNullOrEmpty(content.CoaleseField(WellknownProperties.TypeCampaign)) ? string.Empty : content.CoaleseField(WellknownProperties.TypeCampaign),
                ImageCampaign = content.GetMediaImageUrl(WellknownProperties.ImageCampaign),
                ImageClaiming = content.GetMediaImageUrl(WellknownProperties.ImageClaiming),
                Claiming = content.CoaleseField(WellknownProperties.Claiming),
                UrlCampaign = content.GetPropertyValue<bool>(WellknownProperties.DisableDetailPage) ? content.CoaleseField(WellknownProperties.ClaimNowLink) : content.Url,
                Summary = content.CoaleseField(WellknownProperties.Summary),
                Description = content.CoaleseField(WellknownProperties.Description),
                DisableDetailPage = content.GetPropertyValue<bool>(WellknownProperties.DisableDetailPage)
            };
        }

        public static bool CheckValidScheduleDate(IPublishedContent content)
        {
            var promotionStartDate = content.GetPropertyValue<DateTime>(WellknownProperties.PromotionStartDate);
            var promotionEndDate = content.GetPropertyValue<DateTime>(WellknownProperties.PromotionEndDate);
            return (promotionStartDate == DateTime.MinValue || promotionStartDate < DateTime.Now) && (promotionEndDate == DateTime.MinValue || promotionEndDate > DateTime.Now);
        }

        private static PromotionDetail MapContentToPromotionDetail(IPublishedContent content)
        {
            var contactDetail = new ContactDetail
            {
                PhoneNumber = content.CoaleseField(WellknownProperties.ContactPhoneNumber),
                Email = content.CoaleseField(WellknownProperties.ContactEmailAddress),
                Address = content.CoaleseField(WellknownProperties.ContactAddress)
            };
            return new PromotionDetail
            {
                Promotion = MapContentToPromotion(content),
                HeroImage = content.GetMediaImageUrl(WellknownProperties.HeroImage),
                Description = content.CoaleseField(WellknownProperties.Description),
                TypeModel = string.IsNullOrEmpty(content.GetPropertyValue<string>(WellknownProperties.TypeModel)) ? "0" : content.GetPropertyValue<string>(WellknownProperties.TypeModel),
                Items = GetPromotionItems(content.Id),
                ListModels = GetPromotionModelNames(content.Id),
                CampaignId = content.Id,
                TermsAndConditions = content.CoaleseField(WellknownProperties.TermsAndConditions),
                ContactDetail = contactDetail,
                Claims = GetRelatedPromotions(content),
                Retailers = GetRetailers(content),
                CashbackAdditionalDays = content.GetPropertyValue<string>(WellknownProperties.CashbackAdditionalDays)
            };
        }

        private static List<string> GetRetailers(IPublishedContent content)
        {
            var retailerIds = content.CoaleseField(WellknownProperties.Retailers).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (retailerIds.Any())
            {
                return helper.TypedContent(retailerIds).OrderBy(x => x.CoaleseField(WellknownProperties.RetailerName)).Select(x => x.CoaleseField(WellknownProperties.RetailerName)).ToList();
            }
            return null;
        }

        private static List<Claim> GetRelatedPromotions(IPublishedContent content)
        {
            return null;

            var categoryId = content.CoaleseField(WellknownProperties.Category);
            var campaignType = content.CoaleseField(WellknownProperties.TypeCampaign);
            if (campaignType.ToLower() == "warranty")
            {
                return null;
            }

            var promotions = helper.TypedContent(WellknownNodes.Promotions)
                                    .Descendants()
                                    .Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.Campaign
                                            && x.CoaleseField(WellknownProperties.Category) == categoryId
                                            && x.CoaleseField(WellknownProperties.TypeCampaign).ToLower() == "warranty");

            if (!promotions.Any())
            {
                return null;
            }

            return promotions.Select(x => new Claim
            {
                Title = x.CoaleseField(WellknownProperties.Title),
                Image = x.GetMediaImageUrl(WellknownProperties.ImageClaiming),
                PurchaseDateStart = x.GetPropertyValue<DateTime>(WellknownProperties.PurchaseDateStart).ToString("dd/MM/yyyy"),
                PurchaseDateEnd = x.GetPropertyValue<DateTime>(WellknownProperties.PurchaseDateEnd).ToString("dd/MM/yyyy"),
                ClaimBy = x.GetPropertyValue<DateTime>(WellknownProperties.ClaimBy).ToString("dd/MM/yyyy"),
                TermsAndCondition = x.CoaleseField(WellknownProperties.TermsAndConditions)
            }).ToList();
        }

        public static List<PromotionDetail> GetListPromotionDetails(int categoryId = 0)
        {
            IEnumerable<IPublishedContent> promotionNodes = helper.TypedContent(WellknownNodes.Promotions).Descendants().Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.Campaign);

            if (categoryId != 0)
            {
                promotionNodes = promotionNodes.Where(x => x.GetPropertyValue<int>(WellknownProperties.Category) == categoryId);
            }

            return promotionNodes.Select(x => MapContentToPromotionDetail(x)).ToList();
        }

        public static string GetPromotionDetails(int categoryId = 0)
        {
            var promotionDetails = GetListPromotionDetails(0);

            return JsonConvert.SerializeObject(promotionDetails);
        }

        private static List<PromotionItem> GetPromotionItems(int promotionId)
        {
            var promotionNode = helper.TypedContent(promotionId);
            List<PromotionItem> items = promotionNode.DescendantsOrSelf("item").Select(x => new PromotionItem
            {
                Target = x.CoaleseField(WellknownProperties.Target),
                TypeModal = x.CoaleseField(WellknownProperties.TypeModal),
                ImageDetail = x.GetMediaImageUrl(WellknownProperties.Image),
                ModalTitle = x.CoaleseField(WellknownProperties.Title),
                Model = GetPromotionModels(promotionId, x.CoaleseField(WellknownProperties.Target))
            }).ToList();
            return items;
        }

        private static List<string> GetPromotionModelNames(int promotionId)
        {
            return helper.TypedContent(promotionId).DescendantsOrSelf("model").Select(x => x.CoaleseField(WellknownProperties.ModelName)).ToList();
        }

        private static List<PromotionModel> GetPromotionModels(int promotionId, string target)
        {
            if (target == "modal-models")
            {
                var promotionNode = helper.TypedContent(promotionId);
                return promotionNode.DescendantsOrSelf("model").Select(x => new PromotionModel
                {
                    ModelNumber = x.CoaleseField(WellknownProperties.ModelName),
                    CashbackE = x.CoaleseField(WellknownProperties.CashbackE),
                    CashbackEU = x.CoaleseField(WellknownProperties.CashbackEU),
                    Gift = x.CoaleseField(WellknownProperties.Gift),
                    Warranty = x.CoaleseField(WellknownProperties.Warranty)
                }).ToList();
            }

            return null;
        }

        public static string GetPromotionOptionCategories()
        {
            string res = "<option value=\"all\">--</option>";
            foreach (var node in helper.TypedContent(WellknownNodes.Promotions).Descendants()
                                                    .Where(x => x.DocumentTypeAlias == WellknownDocumentTypes.Campaign)
                                                    .OrderBy(x => x.CoaleseField(WellknownProperties.Title)))
            {
                res += string.Format("<option value=\"{0}\">{1}</option>", node.Id, node.CoaleseField(WellknownProperties.Title));
            }
            return res;
        }

        public static string GetAllRetailers()
        {
            return JsonConvert.SerializeObject(helper.TypedContent(WellknownNodes.Retailers).Children.OrderBy(x => x.CoaleseField(WellknownProperties.RetailerName)).Select(x => x.CoaleseField(WellknownProperties.RetailerName)).ToList());
        }

        public static UserClaimItemUmbraco MapClaimItemToUmbracoNode(UserClaimItem item)
        {
            var userclaimItem = new UserClaimItemUmbraco
            {
                CampaignId = item.CampaignId,
                CampaignName = item.CampaignName
            };
            if (item.Customer != null)
            {
                userclaimItem.AddressLine1 = item.Customer.Address;
                userclaimItem.AddressLine2 = item.Customer.Address2;
                userclaimItem.AgeGroup = item.Customer.AgeGroup;
                userclaimItem.Suburb = item.Customer.Suburb;
                userclaimItem.State = item.Customer.State;
                userclaimItem.DateOfBirth = item.Customer.DateOfBirth;
                userclaimItem.DateOfPurchase = item.Product.DatePurchase;
                userclaimItem.DidYouPurchaseOnlineOrInStore = item.Customer.MethodPurchase;
                userclaimItem.Email = item.Customer.Email;
                userclaimItem.FirstName = item.Customer.FirstName;
                userclaimItem.Gender = item.Customer.Gender;
                userclaimItem.HaveYouPreviouslyOwnedAPanasonicCameracamcorder = item.Customer.HaveCamera;
                userclaimItem.ModelNumber = item.Product.Model;
                userclaimItem.PhoneNumber = item.Customer.Phone;
                userclaimItem.Postcode = item.Customer.Postcode;
                userclaimItem.Surname = item.Customer.Surname;
                userclaimItem.Title = item.Customer.Title;
                userclaimItem.WhatDoYouUseYourCameraFor = item.Customer.UseCamera;
                userclaimItem.WhatGenresOfPhotographyInterestYou = item.Customer.Genres;
                userclaimItem.WhatMadeYouBuyYourPanasonicCameracamcorder = item.Customer.ReasonBuy;
                userclaimItem.WhereDidYouHearAboutYourPanasonicCameracamcorder = item.Customer.HearPanasonic;
                userclaimItem.WhichOtherManufacturersbrandsDidYouConsiderBeforeYourPanasonicPurchase = item.Customer.Brands;
                userclaimItem.WhyDidYouChooseThisRetailer = item.Customer.ChooseRetailer;
            }
            if (item.Product != null)
            {
                userclaimItem.PromotionCodes = item.Product.Code;
                userclaimItem.SerialNumber = item.Product.SerialNumber;
                userclaimItem.Subscribe = item.Product.ReceiveLastestNews == "1";
                userclaimItem.RegisterYourProduct = item.Product.AcceptRegister == "1";
                userclaimItem.Retailer = item.Product.Store;
            }
            if (item.ProofOfPurchase != null)
            {
                userclaimItem.SendProofType = item.ProofOfPurchase.Method;
                userclaimItem.UploadBarcode = item.ProofOfPurchase.BarcodeImage;
                userclaimItem.UploadReceipt = item.ProofOfPurchase.ReceiptImage;
            }
            if (item.CashBack != null)
            {
                userclaimItem.CashBackMethod = item.CashBack.Method;
                userclaimItem.PaypalEmail = item.CashBack.PayPalEmail;
            }
            return userclaimItem;
        }

        public static UserClaimItemEloqua MapClaimItemToEloquaItem(UserClaimItem item)
        {
            var userclaimItem = new UserClaimItemEloqua
            {
                CampaignName = item.CampaignName
            };
            if (item.Customer != null)
            {
                userclaimItem.AddressLine1 = item.Customer.Address;
                userclaimItem.AddressLine2 = item.Customer.Address2;
                userclaimItem.AgeGroup = item.Customer.AgeGroup;
                userclaimItem.Suburb = item.Customer.Suburb;
                userclaimItem.State = item.Customer.State;
                userclaimItem.DateOfBirth = item.Customer.DateOfBirth;
                userclaimItem.DateOfPurchase = item.Product.DatePurchase;
                userclaimItem.DidYouPurchaseOnlineOrInStore = item.Customer.MethodPurchase;
                userclaimItem.Email = item.Customer.Email;
                userclaimItem.FirstName = item.Customer.FirstName;
                userclaimItem.Gender = item.Customer.Gender;
                userclaimItem.HaveYouPreviouslyOwnedAPanasonicCameracamcorder = item.Customer.HaveCamera;
                userclaimItem.ModelNumber = item.Product.Model;
                userclaimItem.PhoneNumber = item.Customer.Phone;
                userclaimItem.Postcode = item.Customer.Postcode;
                userclaimItem.Surname = item.Customer.Surname;
                userclaimItem.Title = item.Customer.Title;
                userclaimItem.WhatDoYouUseYourCameraFor = item.Customer.UseCamera;
                userclaimItem.WhatGenresOfPhotographyInterestYou = item.Customer.Genres;
                userclaimItem.WhatMadeYouBuyYourPanasonicCameracamcorder = item.Customer.ReasonBuy;
                userclaimItem.WhereDidYouHearAboutYourPanasonicCameracamcorder = item.Customer.HearPanasonic;
                userclaimItem.WhichOtherManufacturersbrandsDidYouConsiderBeforeYourPanasonicPurchase = item.Customer.Brands;
                userclaimItem.WhyDidYouChooseThisRetailer = item.Customer.ChooseRetailer;
            }
            if (item.Product != null)
            {
                userclaimItem.PromotionCodes = item.Product.Code;
                userclaimItem.SerialNumber = item.Product.SerialNumber;
                userclaimItem.Subscribe = item.Product.ReceiveLastestNews == "1" ? "Yes" : "No";
                userclaimItem.RegisterYourProduct = item.Product.AcceptRegister == "1" ? "Yes" : "No";
                userclaimItem.Retailer = item.Product.Store;
            }
            if (item.ProofOfPurchase != null)
            {
                userclaimItem.SendProofType = item.ProofOfPurchase.Method;
                userclaimItem.UploadBarcode = item.ProofOfPurchase.BarcodeImage;
                userclaimItem.UploadReceipt = item.ProofOfPurchase.ReceiptImage;
            }
            if (item.CashBack != null)
            {
                userclaimItem.CashBackMethod = item.CashBack.Method;
                userclaimItem.PaypalEmail = item.CashBack.PayPalEmail;
            }
            return userclaimItem;
        }

        public static StandardPageModel GetStandardPageModel(IPublishedContent content)
        {
            return new StandardPageModel
            {
                Title = content.CoaleseField(WellknownProperties.Title),
                Summary = content.CoaleseField(WellknownProperties.Summary)
            };
        }

        public static AreaOfInterestModel MapContactProfileToAreaOfInterestModel(ContactProfiles contactProfiles)
        {
            var trueStr = bool.TrueString;
            if (contactProfiles != null)
            {
                return new AreaOfInterestModel()
                {
                    PhotographyAndFilm = contactProfiles.PhotographyAndFilm == trueStr,
                    HomeEntertainment = contactProfiles.HomeEntertainment == trueStr,
                    HouseholdElectronics = contactProfiles.HouseholdElectronics == trueStr,
                    Telecommunications = contactProfiles.Telecommunications == trueStr,
                    VIErATV = contactProfiles.VIErATV == trueStr,
                    AirConditioner = contactProfiles.AirConditioner == trueStr
                };
            }
            return new AreaOfInterestModel();
        }

        public static MetaTags GetPromotionMetaTags(IPublishedContent currentTypedContent)
        {
            //Page title
            var pageTitle = currentTypedContent.CoaleseField(WellknownProperties.Title);
            pageTitle = !string.IsNullOrEmpty(pageTitle) ? pageTitle : Title;
            //Meta Description
            var metaDescription = currentTypedContent.CoaleseField(WellknownProperties.MetaDescription);
            metaDescription = !string.IsNullOrEmpty(metaDescription) ? metaDescription : Description;
            //Open Graph Image Facebook
            var openGraphImage = currentTypedContent.GetMediaImageUrl(WellknownProperties.OpenGraphImageFacebook);
            openGraphImage = openGraphImage.StartsWith("//") ? string.Format("https:{0}", openGraphImage) : openGraphImage;
            openGraphImage = !string.IsNullOrEmpty(openGraphImage) ? openGraphImage : OGImage;
            //Open Graph Title Facebook
            var openGraphTitle = currentTypedContent.CoaleseField(WellknownProperties.OpenGraphTitleFacebook);
            openGraphTitle = !string.IsNullOrEmpty(openGraphTitle) ? openGraphTitle : Title;
            //Open Graph Description Facebook
            var openGraphDescription = currentTypedContent.CoaleseField(WellknownProperties.OpenGraphDescriptionFacebook);
            openGraphDescription = !string.IsNullOrEmpty(openGraphDescription) ? openGraphDescription : Description;

            return new MetaTags
            {
                PageTitle = pageTitle,
                MetaDescription = metaDescription,
                OpenGraphImage = openGraphImage,
                OpenGraphTitle = openGraphTitle,
                OpenGraphDescription = openGraphDescription
            };
        }

        public static ContactDetails MapLumixAccountModelToContactDetailEntity(LumixPromotionUserProfileModel model)
        {
            var result = new ContactDetails()
            {
                LumixPromotionId = model.Id,
                Title = model.Title,
                FirstName = model.FirstName,
                Surname = model.LastName,
                Email = model.Email,
                Address = new AddressDetails()
                {
                    AddressLine1 = model.Address1,
                    AddressLine2 = model.Address2,
                    AddressLine3 = model.SuburbOther,
                    Postcode = model.PostCode,
                    State = model.State,
                    Suburb = model.Suburb,
                    AreaCode = model.AreaCode
                },
                PhoneAH = model.Phone,
                Mobile = model.Mobile,
                PAU = "1",
                PAUMemberSource = "Lumix Promotion"
            };

            return result;
        }

        public static LumixPromotionUserProfileModel MapContactDetailEntityToLumixAccountModel(ContactDetails model)
        {
            string email = model.Email.Replace(string.Format("_{0}", model.LumixPromotionId), "");
            
            var result = new LumixPromotionUserProfileModel()
            {
                Id = model.LumixPromotionId,
                Title = model.Title,
                FirstName = model.FirstName,
                LastName = model.Surname,
                Email = email,
                Phone = model.PhoneAH,
                Mobile = model.Mobile,
                Address1 = model.Address.AddressLine1,
                Address2 = model.Address.AddressLine2,
                SuburbOther = model.Address.AddressLine3,
                AreaCode = model.Address.AreaCode,
                Suburb = model.Address.Suburb,
                State = model.Address.State,
                PostCode = model.Address.Postcode
            };
            return result;
        }
    }
}