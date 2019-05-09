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
    public class TopicController : Controller
    {
        private readonly ForumDbContext context;

        public TopicController (ForumDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //GET: Topic/Details/id
        public IActionResult Details (int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Topic topic = context.Topics
                .Include(t => t.Author)
                .Include(t => t.Category)
                .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
                .Where(t => t.Id == id)
                .SingleOrDefault();

            if (topic == null)
            {
                return RedirectToAction("Index", "Home");
            }

            
            return View(topic);
        }

        //GET: Topic/Create
        [Authorize]
        public IActionResult Create()
        {
            var categoryNames = context.Categories.Select(c => c.Name).ToList();

            ViewData["CategoryNames"] = categoryNames;
            return View();
        }

        // POST: Topic/Create

        [HttpPost]
        [Authorize]
        public IActionResult Create (string categoryName, Topic topic)
        {
            if (ModelState.IsValid)
            {
                              // Insert topic in DB:

                // - Set CreatedDate and LastUpdadetDate:
                topic.CreatedDate = DateTime.Now;
                topic.LastUpdatedDate = DateTime.Now;

                // - Get userId:
                string authorId = context.Users
                .Where(u => u.UserName == User.Identity.Name)
                .First()
                .Id;

                // - Set topic authorId:
                topic.AuthorId = authorId;

                if (!context.Categories.Any(c => c.Name == categoryName))
                {
                    return View(topic);
                }

                int categoryId = context.Categories.SingleOrDefault(c => c.Name == categoryName).Id;

                topic.CategoryId = categoryId;

                // - Save topic:
                context.Topics.Add(topic);
                context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            return View(topic);
        }

        // Get: Topic/Delete/id
        [HttpGet]
        [Authorize]
        public IActionResult Delete (int? id)
        {
            if (id == null)
            {
                RedirectToAction("Index", "Home");
            }

            var topic = context.Topics
                .Include(t => t.Author)
                .FirstOrDefault(t => t.Id == id);

            if (topic == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!topic.IsAuthor(User.Identity.Name))
            {
                return Forbid();
            }

            return View(topic);
        }

        // POST: Topic/Delete/id
        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            // get topic:
            Topic topic = context.Topics
                .Include(t => t.Author)
                .FirstOrDefault(t => t.Id == id);

            // check if topic exists:
            if (topic != null)
            {
                // delete topic:
                context.Topics.Remove(topic);
                context.SaveChanges();
            }

            // redirect to Index page:
            return RedirectToAction("Index", "Home");
        }

        //GET: Topic/Edti/id:
        [HttpGet]
        public IActionResult Edit (int? id)
        {
            // checking if id is null:
            if (id == null)
            {
                RedirectToAction("Index", "Home");
            }

            // getting topic from DB:
            Topic topic = context.Topics
                .Include(t => t.Author)
                .Include(t => t.Category)
                .Where(t => t.Id == id)
                .SingleOrDefault();

            // checking if the topic exists:
            if (topic == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!topic.IsAuthor(User.Identity.Name))
            {
                return Forbid();
            }

            var categoryNames = context.Categories.Select(c => c.Name).ToList();

            ViewData["CategoryNames"] = categoryNames;

            // passing the model to the view:
            return View(topic);
        }

        //POST: Topic/Edit/id
        [HttpPost]
        [Authorize]
        public IActionResult Edit (string categoryName, Topic topic)
        {
            if (ModelState.IsValid)
            {
                Topic topicToEdit = context.Topics
                    .Include(t => t.Author)
                    .SingleOrDefault(t => t.Id.Equals(topic.Id));

                if (topicToEdit == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                topicToEdit.Title = topic.Title;
                topicToEdit.Description = topic.Description;

                int categoryId = context.Categories.SingleOrDefault(c => c.Name == categoryName).Id;
                topicToEdit.CategoryId = categoryId;

                topicToEdit.LastUpdatedDate = DateTime.Now;

                context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(topic);
        }

    }
}