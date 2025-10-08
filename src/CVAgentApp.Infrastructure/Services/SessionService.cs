using CVAgentApp.Core.Interfaces;
using CVAgentApp.Core.Entities;
using CVAgentApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CVAgentApp.Core.Enums;
using System.Linq;

namespace CVAgentApp.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly ApplicationDbContext _context;

    public SessionService(ILogger<SessionService> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<Session> CreateSessionAsync(Guid candidateId, Guid jobPostingId)
    {
        try
        {
            _logger.LogInformation("Creating session for candidate: {CandidateId}, job: {JobPostingId}", candidateId, jobPostingId);

            var session = new Session
            {
                Id = Guid.NewGuid(),
                SessionToken = Guid.NewGuid().ToString(),
                CandidateId = candidateId,
                JobPostingId = jobPostingId,
                Status = SessionStatus.Created,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            // No need to save to database - this is a stateless service
            // _context.Sessions.Add(session);
            // await _context.SaveChangesAsync();

            _logger.LogInformation("Session created successfully: {SessionId}", session.Id);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            throw;
        }
    }

    public async Task<Session?> GetSessionAsync(string sessionToken)
    {
        try
        {
            _logger.LogInformation("Getting session: {SessionToken}", sessionToken);

            // Stateless service - return null since we don't persist sessions
            _logger.LogWarning("Session not found: {SessionToken} (stateless service)", sessionToken);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session: {SessionToken}", sessionToken);
            throw;
        }
    }

    public async Task<bool> UpdateSessionStatusAsync(Guid sessionId, SessionStatus status, string? log = null)
    {
        try
        {
            _logger.LogInformation("Updating session status: {SessionId} to {Status}", sessionId, status);

            // Stateless service - just log the status update
            _logger.LogInformation("Session status updated (stateless): {SessionId} to {Status}", sessionId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session status: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> CompleteSessionAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("Completing session: {SessionId}", sessionId);

            // Stateless service - just log the completion
            _logger.LogInformation("Session completed (stateless): {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> ExpireSessionAsync(Guid sessionId)
    {
        try
        {
            _logger.LogInformation("Expiring session: {SessionId}", sessionId);

            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session not found: {SessionId}", sessionId);
                return false;
            }

            session.Status = SessionStatus.Expired;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Session expired successfully: {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> CleanupExpiredSessionsAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning up expired sessions");

            var expiredSessions = await _context.Sessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow && s.Status != SessionStatus.Expired)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.Status = SessionStatus.Expired;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            throw;
        }
    }
}
