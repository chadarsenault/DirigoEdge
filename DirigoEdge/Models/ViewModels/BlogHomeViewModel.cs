using System;
using System.Collections.Generic;
using System.Linq;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Models;
using DirigoEdgeCore.Models.ViewModels;
using DirigoEdgeCore.Utils;

namespace DirigoEdge.Models.ViewModels
{
	public class BlogHomeViewModel : CacheableObject
	{
		public List<Blog> BlogRoll;
		public List<Blog> AllBlogs;
		public BlogsCategoriesViewModel BlogCats;

		public int BlogRollCount;
		public int MaxBlogCount;
		public int LastBlogId;
		public int SkipBlogs;

		public Blog FeaturedBlog;
		public bool ReachedMaxBlogs;

		public string CurrentMonth;

		public string BlogTitle;

		public BlogHomeViewModel(object key)
			: base(key)
		{

		}

		public override void Load()
		{
			var model = new BlogListModel(Context);
			MaxBlogCount = model.GetBlogSettings().MaxBlogsOnHomepageBeforeLoad;
			
			SkipBlogs = MaxBlogCount;
			BlogTitle = model.GetBlogSettings().BlogTitle;
			FeaturedBlog = Context.Blogs.FirstOrDefault(x => x.IsFeatured);
			AllBlogs = Context.Blogs.Where(x => x.IsActive).ToList();
			BlogCats = new BlogsCategoriesViewModel("");

			DateTime startDate;
			if (!String.IsNullOrEmpty(Key) && DateTime.TryParse(Key, out startDate))
			{
				CurrentMonth = startDate.ToString("MM/yyyy");

				BlogRoll =
					Context.Blogs.Where(
						x => x.IsActive
						     && (x.Date.Month == startDate.Month)
						     && (x.Date.Year == startDate.Year)
						)
						.OrderByDescending(x => x.Date)
						.Take(MaxBlogCount)
						.ToList();
			}
			else
			{
				CurrentMonth = "";
				BlogRoll = AllBlogs.Where(x => x.IsActive)
						.OrderByDescending(x => x.Date)
						.Take(MaxBlogCount)
						.ToList();
			}
		}
	}
}