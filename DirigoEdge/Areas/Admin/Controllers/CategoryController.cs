using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirigoEdge.Areas.Admin.Models;
using DirigoEdge.Controllers;
using DirigoEdgeCore.Controllers;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;
using WebGrease.Css.Extensions;

namespace DirigoEdge.Areas.Admin.Controllers
{
    public class CategoryController : DirigoBaseAdminController
    {
        private BlogUtils utils;

        public CategoryController()
        {
            utils = new BlogUtils(Context);
        }

        [HttpPost]
        [PermissionsFilter(Permissions = "Can Edit Blog Categories")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult AddCategory(string name)
        {
            var result = new JsonResult()
            {
                Data = new
                {
                    success = false,
                    message = "There was an error processing your request."
                }
            };

            var success = 0;
            int theId = 0;

            if (!String.IsNullOrEmpty(name))
            {
                var newCategory = new BlogCategory
                {
                    CategoryName = name,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true,
                    CategoryNameFormatted = ContentUtils.FormatCategory(name)
                };

                Context.BlogCategories.Add(newCategory);
                success = Context.SaveChanges();
                theId = newCategory.CategoryId;
            }
            if (success > 0)
            {
                result.Data = new
                {
                    success = true,
                    message = "Category added successfully.",
                    id = theId
                };
            }
            return result;
        }

        [HttpPost]
        [PermissionsFilter(Permissions = "Can Edit Blog Categories")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteCategory(string id)
        {
            var result = new JsonResult()
            {
                Data = new
                {
                    success = false,
                    message = "There was an error processing your request."
                }
            };

            var success = 0;

            if (!String.IsNullOrEmpty(id))
            {
                int catId = Int32.Parse(id);

                var cat = Context.BlogCategories.FirstOrDefault(x => x.CategoryId == catId);
                var uncat = utils.GetUncategorizedCategory();
                Context.SaveChanges();

                // did we find a category
                if (cat != null)
                {
                    // find all posts with this category and change to Uncategorized
                    foreach (var x in Context.Blogs.Where(x => x.Category.CategoryName == cat.CategoryName))
                    {
                        x.Category = uncat;
                    }

                    Context.BlogCategories.Remove(cat);
                    success = Context.SaveChanges();
                }
            }

            if (success > 0)
            {
                result.Data = new
                {
                    success = true,
                    message = "Category removed successfully. Blog posts changed: " + (success - 1)
                };
            }
            return result;
        }
    }
}
