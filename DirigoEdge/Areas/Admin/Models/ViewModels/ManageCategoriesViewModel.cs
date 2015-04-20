using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Models;

namespace DirigoEdge.Areas.Admin.Models.ViewModels
{
    public class ManageCategoriesViewModel : DirigoBaseModel
    {

        public List<BlogCategory> BlogCategories;
        public List<BlogTotals> Totals;

        public ManageCategoriesViewModel()
        {
            BlogCategories = Context.BlogCategories.ToList();
            BookmarkTitle = "Manage Categories";
            Totals = (from bc in Context.BlogCategories
                      select
                          new BlogTotals
                          {
                              Category = bc.CategoryName,
                              TotalPosts = Context.Blogs.Count(x => x.Category == bc)
                          }).ToList();
        }
    }

    public class BlogTotals
    {
        public string Category { get; set; }
        public int TotalPosts { get; set; }
    }
}