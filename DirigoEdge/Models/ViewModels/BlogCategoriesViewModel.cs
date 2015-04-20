using System;
using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class BlogsCategoriesViewModel : DirigoBaseModel
    {
        public List<BlogCatExtraData> Categories;
        public String CurrentCategory;

        public BlogsCategoriesViewModel(string current)
        {
            CurrentCategory = current;
            Categories = new List<BlogCatExtraData>();

            var cats = Context.BlogCategories.Where(x => x.IsActive).ToList();
            foreach (var cat in cats)
            {
                int count = Context.Blogs.Count(x => x.Category.CategoryName == cat.CategoryName && x.IsActive);
                Categories.Add(new BlogCatExtraData() { TheCategory = cat, BlogCount = count });
            }
        }

        public class BlogCatExtraData
        {
            public BlogCategory TheCategory;
            public int BlogCount;
        }
    }
}