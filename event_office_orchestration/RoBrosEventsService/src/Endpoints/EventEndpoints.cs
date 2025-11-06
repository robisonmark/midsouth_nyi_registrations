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
            
            group.MapPost("/", async (Event newEvent, IEventService EventService) =>
            {
                // Logic to create a new event
                await EventService.CreateEventAsync(newEvent);
                // This is a placeholder; actual implementation would involve calling a method on EventService
                return Results.Created($"/api/events/{newEvent.Id}", newEvent);
            });

            // GET /api/events
            group.MapGet("/", async (IEventService EventService) =>
            {
                var events = await EventService.GetAllEventsAsync();
                return Results.Ok(events);
            });

            group.MapGet("/{eventId:guid}", async (Guid eventId, IEventService EventService) =>
            {
                var events = await EventService.GetEventAsync(eventId);
                return Results.Ok(events);
            });

            group.MapGet("/{eventId:guid}/time_slots", async (Guid eventId, IEventService EventService) =>
            {
                var time_slots = await EventService.GetTimeSlotsAsync(eventId);
                return Results.Ok(time_slots);
            });

            group.MapGet("/timeslots", async (string event_name, string category, string age, IEventService EventService) =>
            {
                var time_slots = await EventService.GetTimeSlotsByCategoryAndAgeAsync(category, age);
                return Results.Ok(time_slots);
            });

        }
    }
}
