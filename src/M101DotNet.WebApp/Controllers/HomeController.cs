using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest

            var recentPosts = await blogContext.Posts.Find(new BsonDocument()).Sort(Builders<Post>.Sort.Descending(p => p.CreatedAtUtc)).ToListAsync();
            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // Insert the post into the posts collection
            var Id = ObjectId.GenerateNewId(System.DateTime.Now);
            await blogContext.Posts.InsertOneAsync(new Post { Id = Id, Author = this.User.Identity.Name, Content = model.Content, Title = model.Title, Tags = model.Tags.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) });

            return RedirectToAction("Post", new { id = Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier
            var result = await blogContext.Posts.Find(Builders<Post>.Filter.Eq("_id", ObjectId.Parse( id))).ToListAsync();
            var post = result[0];

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.

            var posts = await blogContext.Posts.Find(Builders<Post>.Filter.In("Tags", new[] { tag })).ToListAsync();


            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"
            var result = await blogContext.Posts.Find(Builders<Post>.Filter.Eq("_id", ObjectId.Parse(model.PostId))).ToListAsync();
            var commentsData = result[0].Comments ==null ? new List<Comment>() : result[0].Comments.ToList();
            commentsData.Add(new Comment { Author = this.User.Identity.Name, Content = model.Content, CreatedAtUtc = System.DateTime.Now });

            await blogContext.Posts.FindOneAndUpdateAsync(Builders<Post>.Filter.Eq(p => p.Id, ObjectId.Parse(model.PostId)), Builders<Post>.Update.Set(p => p.Comments, commentsData.ToArray()));

            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}