using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RoBrosEventsService.Models;
using RoBrosEventsService.Services;

namespace RoBrosEventsService.Endpoints
{
    public static class ReservationEndpoints
    {
        public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/reservations");
            
            // POST /api/reservations/{slotId}
            group.MapPost("/{slotId:guid}", async (
                Guid slotId,
                ReservationRequest request,
                IEventService eventService) =>
            {
                var reservation = new SlotReservation
                {
                    SlotId = slotId,
                    ParticipantId = request.ParticipantId,
                    ReservedName = request.ReservedName,
                    ReservedContact = request.ReservedContact,
                    // Status, CreatedAt, and Id will use their defaults
                };
                await eventService.CreateReservationAsync(reservation);
                return Results.Ok();
            });

            group.MapGet("/{reservationId:guid}", async (Guid reservationId, IEventService eventService) =>
            {
                // Implementation for getting a reservation by ID can be added here
                var reservation = await eventService.GetReservationAsync(reservationId);
                return Results.Ok(reservation);
            });
        }
    }

    public record ReservationRequest(
        Guid ParticipantId,
        string ReservedName,
        string ReservedContact
    );
}
