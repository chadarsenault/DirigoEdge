using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class CategorySingleViewModel : DirigoBaseModel
    {
        public BlogCategory TheCategory;
        public List<Blog> AllBlogsInCategory;

        public List<Blog> BlogRoll;
        public List<Blog> BlogsByCat;
        public int BlogRollCount = 10;
        public int MaxBlogCount = 10;
        public int LastBlogId = 0;
        public bool ReachedMaxBlogs;
        public int SkipBlogs = 0;

        public List<string> ImageList;

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        private HttpServerUtilityBase _server;

        public string BlogTitle;

        public CategorySingleViewModel(string category, HttpServerUtilityBase server)
        {
            _server = server;

            category = formatCategoryString(category);


            AllBlogsInCategory = Context.Blogs.Where(x => x.Category.CategoryNameFormatted == category && x.IsActive)
                        .OrderByDescending(blog => blog.Date)
                        .ToList();

            BlogRoll = AllBlogsInCategory
                .Take(MaxBlogCount)
                .ToList();


            TheCategory = Context.BlogCategories.FirstOrDefault(x => x.CategoryNameFormatted == category);
            var model = new BlogListModel(Context);
            MaxBlogCount = model.GetBlogSettings().MaxBlogsOnHomepageBeforeLoad;
            SkipBlogs = MaxBlogCount;
            BlogTitle = model.GetBlogSettings().BlogTitle;

            BlogsByCat = AllBlogsInCategory
                        .Take(MaxBlogCount)
                        .ToList();
        }

        private string formatCategoryString(string category)
        {
            category = category.Replace(ContentGlobals.BLOGDELIMMETER, " ");

            // E-Commerce should not have it's dash removed
            if (category.ToLower() != "e-commerce")
            {
                category = category.Replace(ContentGlobals.BLOGDELIMMETER, " ");
            }

            return category;
        }
    }

}