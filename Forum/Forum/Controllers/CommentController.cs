using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Data;
using Forum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forum.Controllers
{
    public class CommentController : Controller
    {
        private readonly ForumDbContext context;

        public CommentController(ForumDbContext context)
        {
            this.context = context;
        }

        // GET: Topic/Details/{topicId}/Comment/Create
        [Authorize]
        [HttpGet]
        [Route("/Topic/Details/{id}/Comment/Create")]
        public IActionResult Create(int id)
        {
            return View();
        }

        //POST: Topic/Details/id/Comment/Create
        [Authorize]
        [HttpPost]
        [Route("/Topic/Details/{TopicId}/Comment/Create")]
        public IActionResult Create (Comment comment)
        {
            if (ModelState.IsValid)
            {
                // Set CreateDate and LastUpdateDate
                comment.CreatedDate = DateTime.Now;
                comment.LastUpdatedDate = DateTime.Now;

                string authorId = context
                    .Users
                    .Where(u => u.UserName == User.Identity.Name)
                    .SingleOrDefault()
                    .Id;

                comment.AuthorId = authorId;

                Topic topic = context.Topics.Find(comment.TopicId);
                topic.LastUpdatedDate = DateTime.Now;

                context.Comments.Add(comment);
                context.SaveChanges();

                return Redirect($"/Topic/Details/{comment.TopicId}");
            }

            return View(comment);
        }

        //GET: /Topic/Details/{TopicID}/Comment/Edit/{id}
        [Authorize]
        [HttpGet]
        [Route("/Topic/Details/{TopicId}/Comment/Edit/{id}")]
        public IActionResult Edit(int? topicId, int? id)
        {
            if (id == null)
            {
                return RedirectPermanent($"/Topic/Details/{ topicId}");

            }

            Comment comment = context
                .Comments
                .Include(c => c.Author)
                .Include(c => c.Topic)
                .ThenInclude(t => t.Author)
                .SingleOrDefault(c => c.CommentId == id);

            if (comment == null)
            {
                return RedirectPermanent($"/Topic/Details/{topicId}");
            }

            return View(comment);
        }

        //POST: /Topic/Details/{topicId}/Comment/Edit/{id}
        [Route("/Topic/Details/{TopicId}/Comment/Edit/{id}")]
        public IActionResult Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                Comment commentToEdit = context
                    .Comments
                    .Include(c => c.Author)
                    .SingleOrDefault(c => c.CommentId.Equals(comment.TopicId));

                if (commentToEdit == null)
                {
                    return RedirectPermanent($"/Topic/Details/{comment.TopicId}");
                }

                commentToEdit.Description = comment.Description;
                commentToEdit.LastUpdatedDate = DateTime.Now;

                Topic topic = context.Topics.Find(comment.TopicId);
                topic.LastUpdatedDate = DateTime.Now;

                context.SaveChanges();

                return RedirectPermanent($"/Topic/Details/{comment.TopicId}");
            }
            return View(comment);
        }

        //GET: /Topic/Details/{TopicID}/Comment/Delete/{id}
        [Authorize]
        [HttpGet]
        [Route("/Topic/Details/{TopicId}/Comment/Delete/{id}")]
        public IActionResult Delete(int topicId, int? id)
        {
            if (id == null)
            {
                return RedirectPermanent($"/Topic/Details/{topicId}");
            }

            var comment = context
                .Comments
                .Include(c => c.Author)
                .Include(c => c.Topic)
                .ThenInclude(t => t.Author)
                .SingleOrDefault(c => c.CommentId == id);

            if (comment == null)
            {
                return RedirectPermanent("/Topic/Details/{topicId}");
            }

            return View(comment);
        }

        //POST: /Topic/Details/{TopicId}/Comment/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Topic/Details/{TopicId}/Comment/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var comment = context
                .Comments
                .Find(id);
            if (comment != null)
            {
                Topic topic = context.Topics.Find(comment.TopicId);
                topic.LastUpdatedDate = DateTime.Now;

                context.Comments.Remove(comment);
                context.SaveChanges();
            }
            return RedirectPermanent("/Topic/Details/{comment.TopicId}");
        }
    }
}