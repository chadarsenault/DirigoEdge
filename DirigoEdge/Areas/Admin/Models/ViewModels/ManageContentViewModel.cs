using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Models;

namespace DirigoEdge.Areas.Admin.Models.ViewModels
{
    public class ManageContentViewModel : DirigoBaseModel
    {
        // Label Customization
        public string Heading = "Manage Content Pages";
        public string NewButtonText = "New Page +";
        public string EditContentHeading = "Edit Page";  // Passed to editcontent controller for display purposes
        public int SchemaId = -1;
        public string Sort = "";

        public List<ContentPage> Pages;
        public List<Schema> Schemata; 
        public Boolean UseTemplateSelector = true;
        public List<PageTemplate> Templates = new List<PageTemplate>();

        // Nav Id, Label
        public Dictionary<int, string> NavList;
      

        public ManageContentViewModel(string[] templateViews)
        {
            // Add any Schema Id's here that you don't want to be listed on this manage page
            var excludeSchemas = new List<int> { 1, 3 };
            Schemata = Context.Schemas.OrderBy(x => x.DisplayName).ToList();

            foreach (var view in templateViews)
            {
                var contents = System.IO.File.ReadAllText(view);
                var metadata = new Regex(@"^@\*---\r?\n\s?([\w\W]*)\r?\n\s?---\*@").Match(contents);

                if (!metadata.Success && metadata.Groups.Count < 2) continue;

                var metaRows = new Regex("\r?\n").Split(metadata.Groups[1].ToString());
                var metaDictionary = metaRows.ToDictionary(row => row.Split(':').First().Trim(), row => row.Split(new[] { ':' }, 2).Last().Trim());

                Templates.Add(new PageTemplate
                {
                    TemplatePath = "~\\" + view.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty),
                    Name = metaDictionary.ContainsKey("name") && metaDictionary["name"] != null ? metaDictionary["name"] : "",
                    ThumbnailPath = metaDictionary.ContainsKey("thumbnail") && metaDictionary["thumbnail"] != null ? metaDictionary["thumbnail"] : "",
                    ViewTemplate = metaDictionary.ContainsKey("template") && metaDictionary["template"] != null ? metaDictionary["template"] : "Blank",
                    MetaData = metaDictionary
                });
            }

            Pages = Context.ContentPages.Where(x => x.IsRevision == false && !excludeSchemas.Contains((int)x.SchemaId)).ToList();

            // Grab the formatted nav list for the category drop down
            NavList = NavigationUtils.GetNavList();
        }

        public ManageContentViewModel(int schemaId)
        {
            SchemaId = schemaId;
            Pages = Context.ContentPages.Where(x => x.IsRevision == false && x.SchemaId == schemaId).OrderBy(x => x.SortOrder).ToList();

            // Grab the formatted nav list for the category drop down
            NavList = NavigationUtils.GetNavList();

            Schemata = new List<Schema>();
        }

        public class PageTemplate
        {
            public String Name { get; set; }
            public String TemplatePath { get; set; }
            public String ViewTemplate { get; set; }
            public String ThumbnailPath { get; set; }
            public Dictionary<string, string> MetaData { get; set; }
        }
    }
}