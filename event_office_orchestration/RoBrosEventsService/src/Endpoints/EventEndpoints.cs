using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoBrosEventsService.Models;
using RoBrosEventsService.Services;

namespace RoBrosEventsService.Endpoints
{
    public static class EventEndpoints
    {
        public static void MapEventEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/events");

            group.MapPost("/", async (Event newEvent, IEventService eventService) =>
            {
                // Logic to create a new event
                await eventService.CreateEventAsync(newEvent);
                // This is a placeholder; actual implementation would involve calling a method on EventService
                return Results.Created($"/api/events/{newEvent.Id}", newEvent);
            });

            // GET /api/events?level=junior
            group.MapGet("/", async ([FromQuery] string? level, IEventService eventService) =>
            {
                var events = await eventService.GetAllEventsAsync(level);
                return Results.Ok(events);
            });

            group.MapGet("/{eventId:guid}", async (Guid eventId, IEventService eventService) =>
            {
                var events = await eventService.GetEventAsync(eventId);
                return Results.Ok(events);
            });

            group.MapGet("/{eventId:guid}/time_slots", async (Guid eventId, IEventService eventService) =>
            {
                var time_slots = await eventService.GetTimeSlotsAsync(eventId);
                return Results.Ok(time_slots);
            });

            group.MapGet("/timeslots", async (string event_name, string category, string age, IEventService eventService) =>
            {
                var time_slots = await eventService.GetTimeSlotsByCategoryAndAgeAsync(category, age);
                return Results.Ok(time_slots);
            });

            // GET /api/events/age_groups
            group.MapGet("/age_groups", async (IEventService eventService) =>
            {
                var ageGroups = await eventService.GetAllAgeGroupsAsync();
                return Results.Ok(ageGroups);
            });

            // GET /api/events/{eventId}/age_groups
            group.MapGet("/{eventId:guid}/age_groups", async (Guid eventId, IEventService eventService) =>
            {
                var ageGroups = await eventService.GetAgeGroupsByEventAsync(eventId);
                return Results.Ok(ageGroups);
            });
        }
    }
}