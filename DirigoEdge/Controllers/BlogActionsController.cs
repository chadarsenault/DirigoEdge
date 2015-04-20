using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirigoEdgeCore.Controllers;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Models;
using DirigoEdgeCore.Utils;

namespace DirigoEdge.Controllers
{
    public class BlogActionsController : DirigoBaseController
    {
        protected BlogListModel model { get; set; }

        public BlogActionsController()
        {
            model = new BlogListModel(Context);
        }

        /// <summary>
        /// Load blog based on skip and take, and sometimes category, user and date
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="category"></param>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <returns>Blog listing partial in descending order</returns>
        public JsonResult LoadBlogs(int take, int skip, string category = "", string user = "", string date = "")
        {
            var result = new JsonResult();


            List<Blog> blogs = model.GetMoreBlogs(take, skip, category, user, date);
            string html = ContentUtils.RenderPartialViewToString("/Views/Shared/Partials/BlogArticleSinglePartial.cshtml", blogs, ControllerContext, ViewData, TempData);
            result.Data = new { html = html, skip = skip + take, buttonClass = blogs.Count() < take ? "hide" : "" };
            return result;
        }

        /// <summary>
        /// Get a list of blogs based on search term
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public JsonResult LoadBlogsByTags(string tags = "", string category = "")
        {
            var result = new JsonResult();
            List<Blog> blogs = model.GetMoreBlogsByTags(tags, category);
            string html = ContentUtils.RenderPartialViewToString("/Views/Shared/Partials/BlogArticleSinglePartial.cshtml", blogs, ControllerContext, ViewData, TempData);
            result.Data = new { html = html, skip = 0, buttonClass = "hide" };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public JsonResult LoadMorePopularBlogs(int take, int skip, string category = "")
        {
            var result = new JsonResult();
            List<Blog> blogs = model.GetMoreBlogs(take, skip, category);
            string html = ContentUtils.RenderPartialViewToString("/Views/Shared/Partials/PopularBlogSinglePartial.cshtml", blogs, ControllerContext, ViewData, TempData);
            result.Data = new { html = html, skip = skip + take, buttonClass = blogs.Count() < take ? "hide" : "" };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <param name="category"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult LoadMoreRelatedBlogs(int take, int skip, string category, int id = 0)
        {
            var result = new JsonResult();
            List<Blog> blogs = model.GetMoreRelatedBlogs(take, skip, category, id);
            string html = ContentUtils.RenderPartialViewToString("/Views/Shared/Partials/RelatedBlogSinglePartial.cshtml", blogs, ControllerContext, ViewData, TempData);
            result.Data = new {html, skip = skip + take, buttonClass = blogs.Count() < take ? "hide" : "" };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastMonth"></param>
        /// <param name="count"></param>
        /// <param name="idList"></param>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public JsonResult LoadMoreArchives(string lastMonth, int count, List<string> idList, string user = "", string date = "")
        {
            var result = new JsonResult();

            IEnumerable<string> archives = model.GetArchives(lastMonth, count, idList, user, date);

            string html = ContentUtils.RenderPartialViewToString("/Views/Shared/Partials/BlogArchiveSinglePartial.cshtml", archives, ControllerContext, ViewData, TempData);

            lastMonth = !archives.Any() ? "0" : "";
            result.Data = new { html = html, lastMonth = lastMonth };

            return result;
        }


    }
}
