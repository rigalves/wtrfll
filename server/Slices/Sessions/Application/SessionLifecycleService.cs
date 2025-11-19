using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Wtrfll.Server.Slices.Sessions.Domain;
using Wtrfll.Server.Infrastructure.Data;

namespace Wtrfll.Server.Slices.Sessions.Application;

public sealed class SessionLifecycleService
{
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    private const int ShortCodeLength = 6;

    private readonly AppDbContext _dbContext;

    public SessionLifecycleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SessionCreatedDto> CreateSessionAsync(CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        var desiredName = request.Name?.Trim();
        var shortCode = await GenerateUniqueShortCodeAsync(cancellationToken);
        var fallbackName = string.IsNullOrWhiteSpace(desiredName) ? $"Session {shortCode}" : desiredName!;
        var scheduledAtUtc = NormalizeToUtc(request.ScheduledAt);

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ShortCode = shortCode,
            ControllerJoinCode = GenerateJoinToken(),
            DisplayJoinCode = GenerateJoinToken(),
            CreatedAt = DateTime.UtcNow,
            Status = SessionStatus.Pending,
            Name = fallbackName,
            ScheduledAt = scheduledAtUtc,
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SessionCreatedDto
        {
            Id = session.Id,
            ShortCode = session.ShortCode,
            ControllerJoinToken = session.ControllerJoinCode,
            DisplayJoinToken = session.DisplayJoinCode,
            CreatedAt = session.CreatedAt,
            Name = session.Name,
            ScheduledAt = session.ScheduledAt,
        };
    }

    public async Task<JoinSessionOperationResult> JoinSessionAsync(
        Guid sessionId,
        SessionParticipantRole role,
        string joinToken,
        CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return JoinSessionOperationResult.NotFound();
        }

        var expectedToken = role == SessionParticipantRole.Controller
            ? session.ControllerJoinCode
            : session.DisplayJoinCode;

        if (!string.Equals(expectedToken, joinToken, StringComparison.Ordinal))
        {
            return JoinSessionOperationResult.InvalidToken();
        }

        var controllerAlreadyJoined = session.Participants.Any(participant => participant.Role == SessionParticipantRole.Controller);
        if (role == SessionParticipantRole.Controller && controllerAlreadyJoined)
        {
            var lockedPayload = new JoinSessionResultDto
            {
                Ok = false,
                ControllerLocked = true,
                Message = "Controller already joined this session.",
                SessionId = session.Id,
                ShortCode = session.ShortCode,
                Role = role,
                Name = session.Name,
                ScheduledAt = session.ScheduledAt,
            };
            return JoinSessionOperationResult.ControllerLocked(lockedPayload);
        }

        var participant = new SessionParticipant
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Role = role,
            JoinedAt = DateTime.UtcNow,
        };
        _dbContext.SessionParticipants.Add(participant);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return JoinSessionOperationResult.Success(new JoinSessionResultDto
        {
            Ok = true,
            ControllerLocked = false,
            Message = null,
            SessionId = session.Id,
            ShortCode = session.ShortCode,
            Role = role,
            Name = session.Name,
            ScheduledAt = session.ScheduledAt,
        });
    }

    public async Task<bool> ValidateJoinTokenAsync(
        Guid sessionId,
        SessionParticipantRole role,
        string joinToken,
        CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null)
        {
            return false;
        }

        var expectedToken = role == SessionParticipantRole.Controller
            ? session.ControllerJoinCode
            : session.DisplayJoinCode;

        return string.Equals(expectedToken, joinToken, StringComparison.Ordinal);
    }

    private async Task<string> GenerateUniqueShortCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var candidate = GenerateShortCode();
            var exists = await _dbContext.Sessions.AnyAsync(s => s.ShortCode == candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }
    }

    private static string GenerateShortCode()
    {
        Span<char> buffer = stackalloc char[ShortCodeLength];
        Span<byte> randomBytes = stackalloc byte[ShortCodeLength];
        RandomNumberGenerator.Fill(randomBytes);

        for (var i = 0; i < ShortCodeLength; i++)
        {
            buffer[i] = CodeAlphabet[randomBytes[i] % CodeAlphabet.Length];
        }

        return new string(buffer);
    }

    private static string GenerateJoinToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }

    private static DateTime? NormalizeToUtc(DateTime? input)
    {
        if (input is null)
        {
            return null;
        }

        return input.Value.Kind switch
        {
            DateTimeKind.Local => input.Value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(input.Value, DateTimeKind.Utc),
            _ => input,
        };
    }
}

public sealed class JoinSessionOperationResult
{
    private JoinSessionOperationResult(JoinSessionOperationStatus status, JoinSessionResultDto? payload = null)
    {
        Status = status;
        Payload = payload;
    }

    public JoinSessionOperationStatus Status { get; }
    public JoinSessionResultDto? Payload { get; }

    public static JoinSessionOperationResult Success(JoinSessionResultDto payload) =>
        new(JoinSessionOperationStatus.Success, payload);

    public static JoinSessionOperationResult InvalidToken() =>
        new(JoinSessionOperationStatus.InvalidToken);

    public static JoinSessionOperationResult ControllerLocked(JoinSessionResultDto payload) =>
        new(JoinSessionOperationStatus.ControllerLocked, payload);

    public static JoinSessionOperationResult NotFound() =>
        new(JoinSessionOperationStatus.NotFound);
}

public enum JoinSessionOperationStatus
{
    Success,
    InvalidToken,
    ControllerLocked,
    NotFound,
}


