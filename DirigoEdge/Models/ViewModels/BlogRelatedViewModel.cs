using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class BlogRelatedViewModel : DirigoBaseModel
    {
        public List<Blog> RelatedPosts;
        public Blog TheBlog;
        public int BlogRollCount;
        public int MaxBlogCount;
        public int SkipBlogs;
        public int LastBlogId;

        public Blog FeaturedBlog;
        public bool ReachedMaxBlogs;

        public BlogRelatedViewModel(string title)
        {
            // Try permalink first
            TheBlog = Context.Blogs.FirstOrDefault(x => x.PermaLink == title);

            var model = new BlogListModel(Context);
            MaxBlogCount = model.GetBlogSettings().MaxBlogsOnHomepageBeforeLoad;
            SkipBlogs = MaxBlogCount;

            // If no go then try title as a final back up
            if (TheBlog == null)
            {
                title = title.Replace(ContentGlobals.BLOGDELIMMETER, " ");
                TheBlog = Context.Blogs.FirstOrDefault(x => x.Title == title);
            }

            if (TheBlog == null || TheBlog.Tags == null)
            {
                return;
            }

           var relPosts  = Context.Blogs.Where(x => x.BlogId != TheBlog.BlogId && x.IsActive)
                .OrderByDescending(blog => blog.Date).ToList();

           relPosts.RemoveAll(posts => !posts.Tags.Intersect(TheBlog.Tags).Any() && posts.Category.CategoryId != TheBlog.Category.CategoryId);
           RelatedPosts = relPosts.Take(5).ToList();
        }
    }
}