using System;
using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class TagSingleViewModel : DirigoBaseModel
    {
        public List<Blog> BlogRoll;
        public Dictionary<String, String> UserNameToDisplayName; 
        public string TheTag;

        public List<BlogsCategoriesViewModel.BlogCatExtraData> Categories;

        public TagSingleViewModel(string tag)
        {
            TheTag = tag;

            BlogRoll = Context.Blogs.Where(x => x.Tags.Any(tg => tg.BlogTagName == tag)
                && x.IsActive).OrderByDescending(x => x.Date).ToList();
            UserNameToDisplayName = Utils.UserUtils.GetUsernamesForBlogs(BlogRoll, Context);
           
            Categories = new List<BlogsCategoriesViewModel.BlogCatExtraData>();

            var cats = Context.BlogCategories.Where(x => x.IsActive).ToList();
            foreach (var cat in cats)
            {
                int count = Context.Blogs.Count(x => x.Category.CategoryId == cat.CategoryId);
                Categories.Add(new BlogsCategoriesViewModel.BlogCatExtraData() { TheCategory = cat, BlogCount = count });
            }
        }
    }
}