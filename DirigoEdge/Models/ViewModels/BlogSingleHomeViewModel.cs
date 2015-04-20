using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class BlogSingleHomeViewModel : DirigoBaseModel
    {
        public Blog TheBlog;
        public BlogUser TheBlogUser;
        public BlogRelatedViewModel RelatedPosts;

        public bool ShowFacebookLikeButton;
        public bool ShowFacebookComments;
        public bool AllCommentsAreDisabled;
        public bool UseDisqusComments;
        public string DisqusShortName;
        public List<string> Tags;
        public List<BlogsCategoriesViewModel.BlogCatExtraData> Categories;

        public BlogAuthorViewModel BlogAuthorModel;

        public string BlogAbsoluteUrl;

        public BlogSingleHomeViewModel(string title)
        {
            // Try permalink first
            TheBlog = Context.Blogs.FirstOrDefault(x => x.PermaLink == title);

            // If no go then try title as a final back up
            if (TheBlog == null)
            {
                title = title.Replace(ContentGlobals.BLOGDELIMMETER, " ");
                TheBlog = Context.Blogs.FirstOrDefault(x => x.Title == title);

                // Go ahead and set the permalink for future reference
                TheBlog.PermaLink = ContentUtils.GetFormattedUrl(TheBlog.Title);
                Context.SaveChanges();
            }

            // Set up the Related Posts Module
            RelatedPosts = new BlogRelatedViewModel(TheBlog.Title);

            // Get User based on authorid
            TheBlogUser = Context.BlogUsers.FirstOrDefault(x => x.UserId == TheBlog.AuthorId);

            BlogAuthorModel = new BlogAuthorViewModel(TheBlog.BlogAuthor.Username);

            // Facebook Like button
            ShowFacebookLikeButton = SettingsUtils.ShowFbLikeButton();

            // Facebook Comments
            ShowFacebookComments = SettingsUtils.ShowFbComments();

            // Absolute Url for FB Like Button
            BlogAbsoluteUrl = HttpContext.Current.Request.Url.AbsoluteUri;

            // Disqus Comments
            UseDisqusComments = SettingsUtils.UseDisqusComments();
            if (UseDisqusComments)
            {
                DisqusShortName = SettingsUtils.DisqusShortName();
            }

            // Tag Listing



            Tags = new List<string>();
            foreach (var tag in TheBlog.Tags)
            {
                Tags.Add(tag.BlogTagName);
            }

            // Full Category Listing
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