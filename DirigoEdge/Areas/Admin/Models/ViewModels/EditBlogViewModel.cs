using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using DirigoEdge.Areas.Admin.Models.DataModels;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Models;
using DirigoEdgeCore.Utils;

namespace DirigoEdge.Areas.Admin.Models.ViewModels
{
    public class EditBlogViewModel : DirigoBaseModel
    {
        public Blog ThisBlog;
        public List<BlogUser> BlogUsers;
        public List<BlogCategory> Categories;
        public List<string> UsersSelectedCategories;
        public List<BlogAdminModule> AdminModulesColumn1;
        public List<BlogAdminModule> AdminModulesColumn2;
        public int BlogId;
        public string SiteUrl;
        public List<TagMetric> TagCounts;

        private User _thisUser;
        private readonly MembershipUser _memUser;

        public EditBlogViewModel(string blogId)
        {
            var utils = new BlogUtils(Context);
            BlogId = Int32.Parse(blogId);
            _memUser = Membership.GetUser(HttpContext.Current.User.Identity.Name);
            SiteUrl = HTTPUtils.GetFullyQualifiedApplicationPath() + "blog/";

            ThisBlog = Context.Blogs.FirstOrDefault(x => x.BlogId == BlogId);

            if (ThisBlog.Category == null)
            {
                ThisBlog.Category = utils.GetUncategorizedCategory();
            }

            // Make sure we have a permalink set
            if (String.IsNullOrEmpty(ThisBlog.PermaLink))
            {
                ThisBlog.PermaLink = ContentUtils.GetFormattedUrl(ThisBlog.Title);
                Context.SaveChanges();
            }

            // Get the list of Authors for the drop down select
            BlogUsers = Context.BlogUsers.Where(x => x.IsActive).OrderBy(x => x.DisplayName).ToList();

            Categories = Context.BlogCategories.Where(x => x.IsActive).ToList();

            UsersSelectedCategories = new List<string>();

            _thisUser = Context.Users.FirstOrDefault(x => x.Username == _memUser.UserName);

            // Get and parse tags for unqiue count
             var tagList = Context.Blogs.Select(x => x.Tags).ToList();
            var tagStr = String.Join(",", tagList);
            var tags = tagStr.Split(',').Select(x => x.Trim()).ToList();

            TagCounts = new List<TagMetric>();

            TagCounts = Context.BlogTags.Select(t => new TagMetric()
            {
                Tag = t.BlogTagName,
                Count = t.Blogs.Count()
            }).ToList();

            BookmarkTitle = ThisBlog.Title;

            // Get the admin modules that will be displayed to the user in each column
            getAdminModules();
        }

        private void getAdminModules()
        {
            AdminModulesColumn1 = Context.BlogAdminModules.Where(x => x.User.Username == _thisUser.Username && x.ColumnNumber == 1).OrderBy(x => x.OrderNumber).ToList();
            AdminModulesColumn2 = Context.BlogAdminModules.Where(x => x.User.Username == _thisUser.Username && x.ColumnNumber == 2).OrderBy(x => x.OrderNumber).ToList();

            // If no settings have been saved, set some defaults for the user
            if (AdminModulesColumn1.Count == 0 && AdminModulesColumn2.Count == 0)
            {
                setDefaultModules();

                AdminModulesColumn1 = Context.BlogAdminModules.Where(x => x.User.Username == _thisUser.Username && x.ColumnNumber == 1).OrderBy(x => x.OrderNumber).ToList();
                AdminModulesColumn2 = Context.BlogAdminModules.Where(x => x.User.Username == _thisUser.Username && x.ColumnNumber == 2).OrderBy(x => x.OrderNumber).ToList();
            }
        }

        private void setDefaultModules()
        {
            var user = Context.Users.FirstOrDefault(x => x.Username == _memUser.UserName);
            var modules = DefaultAdminModules.GetDefaultAdminModules(user);

            foreach (var module in modules)
            {
                user.BlogAdminModules.Add(module);
            }

            Context.SaveChanges();
        }
    }

    public class TagMetric
    {
        public String Tag;
        public int Count;
    }
}