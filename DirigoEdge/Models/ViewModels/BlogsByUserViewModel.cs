using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class BlogsByUserViewModel : DirigoBaseModel
    {
        public Blog TheBlog;
        public List<Blog> AllBlogs;
        public List<Blog> BlogsByUser;
        public BlogUser TheBlogUser;
        public int BlogRollCount;
        public int MaxBlogCount;
        public int LastBlogId;
        public bool ReachedMaxBlogs;
        public string BlogUsername;
        public int SkipBlogs;

        public string BlogTitle;

        public BlogsByUserViewModel(string username)
        {
            // Get back to the original name before url conversion
            BlogUsername = username.Replace(ContentGlobals.BLOGDELIMMETER, " ");

            // Get User based on authorid
            TheBlogUser = Context.BlogUsers.FirstOrDefault(x => x.Username == BlogUsername);
            
            var model = new BlogListModel(Context);
            MaxBlogCount = model.GetBlogSettings().MaxBlogsOnHomepageBeforeLoad;
            SkipBlogs = MaxBlogCount;
            BlogTitle = model.GetBlogSettings().BlogTitle;

            AllBlogs = Context.Blogs.Where(x => x.BlogAuthor.Username == BlogUsername && x.IsActive).ToList();

            BlogsByUser = AllBlogs
                        .OrderByDescending(blog => blog.Date)
                        .Take(MaxBlogCount)
                        .ToList();

            // Try permalink first
            TheBlog = BlogsByUser.FirstOrDefault(x => x.BlogAuthor.Username == BlogUsername);

        }
    }
}