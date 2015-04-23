﻿using System;
using System.Linq;
using System.ServiceModel.Security;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Web.UI;
using DirigoEdge.Business;
using DirigoEdgeCore.Controllers;
using DirigoEdgeCore.Models.DataModels;

namespace DirigoEdge.Controllers
{
    public class BlogController : DirigoBaseController
    {
        private String BaseUrl
        {
            get
            {
                var generatedBaseUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/"; // Fallback if site settings isn't in place
                return SettingsUtils.GetSiteBaseUrl() ?? generatedBaseUrl;
            }
        }

        private BlogLoader _loader;
        private BlogLoader Loader
        {
            get { return _loader ?? (_loader = new BlogLoader(Context)); }
        }

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
            ViewBag.OGType = "article";

            // Blog Listing Homepage
            if (String.IsNullOrEmpty(title) && String.IsNullOrEmpty(category))
            {
                return GetBlogHome(date);
            }
            // Category

            if (String.IsNullOrEmpty(title))
            {
                return GetBlogByCategory(category);
            }

            // Tag
            if (category == "tags" && !string.IsNullOrEmpty(title))
            {
                return GetBlogsByTag(title);
            }

            // Blog User
            if (category == "user" && !string.IsNullOrEmpty(title))
            {
                return GetBlogsByUser(title);
            }

            // Category is set and we are trying to view an individual blog
            if (Context.Blogs.Any(x => x.PermaLink == title))
            {
                return GetSingleBlogByTitle(title);
            }

            // Not a blog category or a blog
            return NotFound;
        }

        private ActionResult GetBlogHome(string date)
        {
            var model = Loader.LoadBlogHome(date);
            ViewBag.OGTitle = model.BlogTitle;
            ViewBag.OGUrl = BaseUrl + "blog";

            return View("~/Views/Home/Blog.cshtml", model);
        }

        private ActionResult NotFound
        {
            get
            {
                HttpContext.Response.StatusCode = 404;
                return View("~/Views/Home/Error404.cshtml");
            }
        }

        private ActionResult GetSingleBlogByTitle(string title)
        {
            var theModel = Loader.LoadSingleBlog(title);

            ViewBag.OGTitle = theModel.TheBlog.Title;
            ViewBag.OGImage = theModel.TheBlog.OGImage;
            ViewBag.MetaDesc = theModel.TheBlog.MetaDescription;
            ViewBag.OGUrl = theModel.BlogAbsoluteUrl;
            ViewBag.OGType = !String.IsNullOrEmpty(theModel.TheBlog.OGType)
                ? theModel.TheBlog.OGType
                : "article";


            return View("~/Views/Home/BlogSingle.cshtml", theModel);
        }

        private ActionResult GetBlogsByUser(string username)
        {
            var model = Loader.LoadBlogsByUser(username);
            ViewBag.OGTitle = "Blogs by : " + username;
            ViewBag.OGUrl = BaseUrl + "blog/user/" + username ;

            return View("~/Views/Blog/BlogsByUser.cshtml", model);
        }

        private ActionResult GetBlogsByTag(string tag)
        {
            var model = Loader.LoadBlogsByTag(tag);
            ViewBag.OGUrl = BaseUrl + "blog/" + tag;
            ViewBag.OGTitle = tag;
            ViewBag.Robots = "NOINDEX, NOFOLLOW";
            return View("~/Views/Blog/TagSingle.cshtml", model);
        }

        private ActionResult GetBlogByCategory(string category)
        {
            if (!Context.BlogCategories.Any(cat => cat.CategoryNameFormatted == category))
            {
                return NotFound;
            }

            ViewBag.OGTitle = category;
            ViewBag.Robots = "NOINDEX, NOFOLLOW";
            ViewBag.OGUrl = BaseUrl + "blog/" + category;


            var model = Loader.LoadBlogsByCategory(category);
            return View("~/Views/Blog/CategoriesSingle.cshtml", model);
        }
        
        public ActionResult NewPosts()
        {
            var blog = Context.Blogs.FirstOrDefault(x => x.IsActive);

            if (HttpContext.Request.Url == null || blog == null)
            {
                return NotFound;
            }

            var blogUrl = "http://" + HttpContext.Request.Url.Host + "/blog/";
            var postItems = Context.Blogs.Where(p => p.IsActive).OrderByDescending(p => p.Date).Take(25).ToList()
                .Select(p => new SyndicationItem(p.Title, p.HtmlContent, new Uri(blogUrl + p.Title)));


            var feed = new SyndicationFeed(blog.Title, blog.Title, new Uri(blogUrl + blog.Title), postItems)
            {
                Language = "en-US",
                Title = new TextSyndicationContent(blog.Title)
            };

            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}
