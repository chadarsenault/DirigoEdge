using System.Linq;
using System.Web;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdgeCore.Models.ViewModels
{
    public class BlogAuthorViewModel : DirigoBaseModel
    {

        public Blog TheBlog;
        public BlogUser TheBlogUser;
        public string BlogUsername;

        public bool ShowFacebookLikeButton;
        public bool ShowFacebookComments;
        public bool AllCommentsAreDisabled;

        public string BlogAbsoluteUrl;

        public BlogAuthorViewModel(string username)
        {

            // Get back to the original name before url conversion
            BlogUsername = username.Replace(ContentGlobals.BLOGDELIMMETER, " ");

            TheBlog = Context.Blogs.FirstOrDefault(x => x.BlogAuthor.Username == BlogUsername);

            // Get User based on authorid
            TheBlogUser = Context.BlogUsers.FirstOrDefault(x => x.Username == BlogUsername);

            // Facebook Like button
            ShowFacebookLikeButton = SettingsUtils.ShowFbLikeButton();

            // Facebook Comments
            ShowFacebookComments = SettingsUtils.ShowFbComments();

            // Absolute Url for FB Like Button
            BlogAbsoluteUrl = HttpContext.Current.Request.Url.AbsoluteUri;
        }
    }
}