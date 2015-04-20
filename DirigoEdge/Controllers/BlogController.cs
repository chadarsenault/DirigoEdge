using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using DirigoEdge.Models.ViewModels;
using DirigoEdgeCore.Controllers;
using DirigoEdgeCore.Models.DataModels;
using DirigoEdgeCore.Models.ViewModels;
using DirigoEdgeCore.Utils;

namespace DirigoEdge.Controllers
{
    public class BlogController : DirigoBaseController
    {
        /// <summary>
        /// If no title or category, could be just listing page.
        /// If no title, but category is set, probably a category listing page
        /// If title and category are set, individual blog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="category"></param>
        /// <param name="date"></param>
        /// <returns>View</returns>
        public ActionResult Index(string title, string category, string date)
        {
            // Blog Listing Homepage
            if (String.IsNullOrEmpty(title) && String.IsNullOrEmpty(category))
            {
                var model = new BlogHomeViewModel(date);
                return View("~/Views/Home/Blog.cshtml", model);
            }
            // Category
            
            if (String.IsNullOrEmpty(title))
            {
                
                // Category
                var cats = Context.BlogCategories.ToList().Select(x => ContentUtils.FormatCategory(x.CategoryName));

                if (cats.Contains(category))
                {
                    var model = new CategorySingleViewModel(category, Server);
                    return View("~/Views/Blog/CategoriesSingle.cshtml", model);
                }

                // Not a blog category or tags page
                HttpContext.Response.StatusCode = 404;
                return View("~/Views/Home/Error404.cshtml");
            }

            // Tag
            if (category == "tags" && !string.IsNullOrEmpty(title))
            {
                var model = new TagSingleViewModel(title);
                return View("~/Views/Blog/TagSingle.cshtml", model);
            }

            // Blog User
            if (category == "user" && !string.IsNullOrEmpty(title))
            {
                var model = new BlogsByUserViewModel(title);
                return View("~/Views/Blog/BlogsByUser.cshtml", model);
            }

            // Category is set and we are trying to view an individual blog
            var blog = Context.Blogs.FirstOrDefault(x => x.PermaLink == title);
            if (blog != null)
            {
                var theModel = new BlogSingleHomeViewModel(title);
                return View("~/Views/Home/BlogSingle.cshtml", theModel);
            }

            // Not a blog category or a blog
            HttpContext.Response.StatusCode = 404;
            return View("~/Views/Home/Error404.cshtml");
        }

        public new ActionResult User(string username)
        {
            var model = new BlogsByUserViewModel(username);

            return View("~/Views/Blog/BlogsByUser.cshtml", model);
        }

        public ActionResult Categories(string category)
        {
            // Blog Listing Homepage
            if (String.IsNullOrEmpty(category))
            {
                var model = new CategoryHomeViewModel();

                return View("~/Views/Blog/CategoriesHome.cshtml", model);
            }
            // Individual Blog
            else
            {
                var model = new CategorySingleViewModel(category, Server);

                return View("~/Views/Blog/CategoriesSingle.cshtml", model);
            }
        }

        public ActionResult NewPosts()
        {
            var blog = Context.Blogs.FirstOrDefault(x => x.IsActive == true);
            string blogUrl = "http://" + HttpContext.Request.Url.Host + "/blog/";

            var postItems = Context.Blogs.Where(p => p.IsActive == true).OrderByDescending(p => p.Date).Take(25).ToList()
                .Select(p => new SyndicationItem(p.Title, p.HtmlContent, new Uri(blogUrl + p.Title)));


            var feed = new SyndicationFeed(blog.Title, blog.Title, new Uri(blogUrl + blog.Title), postItems)
            {
                Language = "en-US",
                Title = new TextSyndicationContent(blog.Title)
            };

            return new FeedResult(new Rss20FeedFormatter(feed));
        }

        public ActionResult Tags(string tag)
        {
            // Don't index blog tag pages
            ViewBag.Robots = "NOINDEX, NOFOLLOW";

            // Blog Listing Homepage
            if (String.IsNullOrEmpty(tag))
            {
                tag = "";
            }

            var model = new TagSingleViewModel(tag);

            return View("~/Views/Blog/TagSingle.cshtml", model);
        }
    }
}
