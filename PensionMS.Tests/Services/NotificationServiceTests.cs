using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Notifications;
using PensionMS.Infrastructure.Services.Notifications;

namespace PensionMS.Tests.Services;

/// ضوابط خدمة الإشعارات: التجهيز دون حفظ، ونطاق القراءة، وأسماء الأدوار (N14 مؤجل حتى قرار Q2).
public class NotificationServiceTests : IDisposable
{
    private readonly List<Notification> _notifications = [];
    private readonly List<NotificationOutbox> _outbox = [];
    private readonly List<User> _users = [];
    private readonly List<NotificationAttachment> _attachments = [];
    private readonly Mock<IRepository<Notification>> _notificationRepo = new();
    private readonly Mock<IRepository<NotificationOutbox>> _outboxRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<NotificationAttachment>> _attachmentRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<INotificationClient> _client = new();
    private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hub = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IScopedPermissionResolver> _permissions = new();
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _notificationRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<Notification>(_notifications));
        _notificationRepo.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(_notifications.Add).Returns(Task.CompletedTask);
        _outboxRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<NotificationOutbox>(_outbox));
        _outboxRepo.Setup(r => r.AddAsync(It.IsAny<NotificationOutbox>()))
            .Callback<NotificationOutbox>(_outbox.Add).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<User>(_users));
        _attachmentRepo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<NotificationAttachment>(_attachments));
        _unitOfWork.SetupGet(u => u.Notifications).Returns(_notificationRepo.Object);
        _unitOfWork.SetupGet(u => u.NotificationOutbox).Returns(_outboxRepo.Object);
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepo.Object);
        _unitOfWork.SetupGet(u => u.NotificationAttachments).Returns(_attachmentRepo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        var clients = new Mock<IHubClients<INotificationClient>>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(_client.Object);
        _hub.SetupGet(h => h.Clients).Returns(clients.Object);
        _permissions.Setup(p => p.GetAllowedScopeAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(PensionMS.Core.Security.PermissionScope.All);
        _service = new NotificationService(_unitOfWork.Object, _hub.Object, _fileStorage.Object, _permissions.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public async Task FileNotification_RespectsTheRecipientsPermissionScope(int fileBranch, bool allowed)
    {
        var files = new Mock<IRepository<RetirementFile>>();
        files.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<RetirementFile>(new[]
        {
            new RetirementFile { FileID = 100, PensionBranchID = fileBranch }
        }));
        _unitOfWork.Setup(u => u.RetirementFiles).Returns(files.Object);
        _permissions.Setup(p => p.GetAllowedScopeAsync(7, "File.View.Sensitive"))
            .ReturnsAsync(PensionMS.Core.Security.PermissionScope.Restricted([2]));
        var result = await _service.SendNotificationAsync(new CreateNotificationDto
        {
            UserID = 7, Title = "ملف", RelatedTableID = 2, RelatedRecordID = 100
        });
        Assert.Equal(allowed, result.IsSuccess);
        Assert.Equal(allowed ? 1 : 0, _notifications.Count);
        Assert.Equal(allowed ? 1 : 0, _outbox.Count);
    }

    [Fact]
    public async Task EnqueueAsync_StagesNotificationAndOutboxWithoutSaving()
    {
        await _service.EnqueueAsync(new CreateNotificationDto { UserID = 99, Title = "اختبار" });

        var notification = Assert.Single(_notifications);
        Assert.Equal(99, notification.UserID);
        Assert.False(notification.IsRead);
        var outboxEntry = Assert.Single(_outbox);
        Assert.Same(notification, outboxEntry.Notification);
        Assert.Equal(OutboxStatus.Pending, outboxEntry.Status);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never,
            "المنادي يجهّز فقط؛ الحفظ ملك معاملة المنادي");
    }

    [Fact]
    public async Task EnqueueRoleAsync_StagesOnePerActiveUserOfRole()
    {
        _users.Add(new User { UserID = 1, IsActive = true, Role = new Role { RoleName = "FinanceOfficer" } });
        _users.Add(new User { UserID = 2, IsActive = false, Role = new Role { RoleName = "FinanceOfficer" } });
        _users.Add(new User { UserID = 3, IsActive = true, Role = new Role { RoleName = "PensionManager" } });

        await _service.EnqueueRoleAsync("FinanceOfficer", new CreateNotificationDto { Title = "اختبار" });

        Assert.Equal([1], _notifications.Select(n => n.UserID));
        Assert.Single(_outbox);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_OtherUsersNotification_IsRejected()
    {
        _notifications.Add(new Notification { NotificationID = 1, UserID = 99, Title = "اختبار" });

        var result = await _service.MarkAsReadAsync(1, 100);

        Assert.False(result.IsSuccess);
        Assert.False(_notifications[0].IsRead);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task PushUnreadCount_HubFailure_DoesNotThrow()
    {
        _client.Setup(c => c.UnreadCountUpdated(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("خطأ اختبار"));

        await _service.PushUnreadCountAsync(99, 25);

        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never,
            "N15: دفع العدد لا يكتب Outbox ولا يحفظ");
    }

    // ── NF1.3: ملكية المرفق ─────────────────────────────────

    private void SeedAttachment(int attachmentId, long notificationId, int recipientUserId, bool deleted = false)
    {
        var notification = new Notification { NotificationID = notificationId, UserID = recipientUserId, Title = "إشعار" };
        _notifications.Add(notification);
        _attachments.Add(new NotificationAttachment
        {
            AttachmentID = attachmentId,
            NotificationID = notificationId,
            Notification = notification,
            FileName = "file.pdf",
            FilePath = "uploads/notifications/file.pdf",
            MimeType = "application/pdf",
            IsDeleted = deleted
        });
    }

    [Fact]
    public async Task DownloadAttachment_Recipient_GetsFile()
    {
        SeedAttachment(1, 10, recipientUserId: 5);
        _fileStorage.Setup(f => f.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(new MemoryStream([1, 2, 3]));

        var result = await _service.DownloadAttachmentAsync(10, 1, userId: 5);

        Assert.True(result.IsSuccess);
        Assert.Equal("file.pdf", result.Data.FileName);
        Assert.Equal("application/pdf", result.Data.ContentType);
    }

    [Fact]
    public async Task DownloadAttachment_OtherUser_IsRejected()
    {
        SeedAttachment(1, 10, recipientUserId: 5);

        var result = await _service.DownloadAttachmentAsync(10, 1, userId: 99);

        Assert.False(result.IsSuccess, "مستخدم آخر برابط معروف لا يحصل على الملف");
        _fileStorage.Verify(f => f.GetFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DownloadAttachment_Admin_IsAllowed()
    {
        SeedAttachment(1, 10, recipientUserId: 5);
        _fileStorage.Setup(f => f.GetFileAsync(It.IsAny<string>()))
            .ReturnsAsync(new MemoryStream([1]));

        var result = await _service.DownloadAttachmentAsync(10, 1, userId: 99, isAdmin: true);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DownloadAttachment_MismatchedIds_IsRejected()
    {
        // مرفق تابع لإشعار 10 — يُطلب عبر إشعار 99
        SeedAttachment(1, 10, recipientUserId: 5);

        var result = await _service.DownloadAttachmentAsync(99, 1, userId: 5);

        Assert.False(result.IsSuccess, "تطابق ID الإشعار والمرفق مفروض");
    }

    [Fact]
    public async Task DownloadAttachment_DeletedAttachment_IsRejected()
    {
        SeedAttachment(1, 10, recipientUserId: 5, deleted: true);

        var result = await _service.DownloadAttachmentAsync(10, 1, userId: 5);

        Assert.False(result.IsSuccess, "المرفق المحذوف لا يُرسل");
    }
}
