﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirigoEdge.Areas.Admin.Models;
using DirigoEdge.Areas.Admin.Models.ViewModels;
using DirigoEdge.Controllers;
using DirigoEdgeCore.Controllers;
using DirigoEdgeCore.Data.Entities;
using DirigoEdgeCore.Utils;

namespace DirigoEdge.Areas.Admin.Controllers
{
    public class EventsController : DirigoBaseAdminController
    {
        [PermissionsFilter(Permissions = "Can Edit Events")]
        public ActionResult EditEvent(string id)
        {
            var model = new EditEventViewModel(id);
            return View(model);
        }

        [PermissionsFilter(Permissions = "Can Edit Events")]
        public ActionResult ManageEvents()
        {
            var model = new ManageEventsViewModel();
            return View(model);
        }

        [PermissionsFilter(Permissions = "Can Edit Events")]
        public ActionResult AddEvent()
        {
            string eventId = String.Empty;

            // Create a new event to be passed to the edit event action
            var thisEvent = new Event() { IsActive = false, Title = "New Event", DateCreated = DateTime.UtcNow, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7) };

            Context.Events.Add(thisEvent);
            Context.SaveChanges();

            // Update the event title / permalink with the new id we now have
            eventId = thisEvent.EventId.ToString();

            thisEvent.Title = thisEvent.Title + " " + eventId;
            Context.SaveChanges();

            return RedirectToAction("EditEvent", "Events", new { id = eventId });
        }

        [HttpPost]
        [PermissionsFilter(Permissions = "Can Edit Events")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult ModifyEvent(Event entity)
        {
            JsonResult result = new JsonResult();

            if (!String.IsNullOrEmpty(entity.Title))
            {
                Event editedEvent = Context.Events.FirstOrDefault(x => x.EventId == entity.EventId);
                if (editedEvent != null)
                {
                    editedEvent.HtmlContent = entity.HtmlContent;
                    editedEvent.FeaturedImageUrl = ContentUtils.ScrubInput(entity.FeaturedImageUrl);
                    editedEvent.IsActive = entity.IsActive;
                    editedEvent.IsFeatured = entity.IsFeatured;
                    editedEvent.Title = ContentUtils.ScrubInput(entity.Title);
                    editedEvent.PermaLink = ContentUtils.ScrubInput(entity.PermaLink);
                    editedEvent.MainCategory = ContentUtils.ScrubInput(entity.MainCategory);
                    editedEvent.EventCategoryId = entity.EventCategoryId;
                    editedEvent.ShortDesc = entity.ShortDesc;
                    editedEvent.StartDate = entity.StartDate;
                    editedEvent.EndDate = entity.EndDate;

                    Context.SaveChanges();

                    result.Data = new { id = entity.EventId };
                }
            }

            return result;
        }

        [HttpPost]
        [PermissionsFilter(Permissions = "Can Edit Events")]
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DeleteEvent(string id)
        {
            var result = new JsonResult()
            {
                Data = new { success = false, message = "There was an error processing your request." }
            };

            if (String.IsNullOrEmpty(id))
            {
                return result;
            }

            int eventId = Int32.Parse(id);

            var thisEvent = Context.Events.FirstOrDefault(x => x.EventId == eventId);

            Context.Events.Remove(thisEvent);
            var success = Context.SaveChanges();

            if (success > 0)
            {
                result.Data = new { success = true, message = "The event has been successfully deleted." };
            }

            return result;
        }
    }

    public class EventEntity
    {
        public string Title { get; set; }
        public string HtmlContent { get; set; }
    }

    public class EventAdminModules
    {
        public List<EventAdminModule> EventAdminModulesColumn1 { get; set; }
        public List<EventAdminModule> EventAdminModulesColumn2 { get; set; }
    }
}