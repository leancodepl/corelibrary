# Push notifications - FCM

Firebase Cloud Messaging (FCM) integrated into LeanCode's CoreLibrary facilitates efficient push notification delivery across various platforms, allowing developers to engage users with timely updates and alerts. FCM's integration simplifies configuration and customization, offering targeted messaging.

Configuring push notifications via Firebase Cloud Messaging (FCM) involves a few key steps for efficient implementation. Firstly, the setup requires a mechanism to capture and store unique device tokens associated with individual users. These tokens act as unique identifiers, allowing targeted message delivery to specific devices.

Upon a user's device registration or app installation, the FCM SDK generates a unique token for that device. This token needs to be captured and stored on in database, associated with the respective user ID. This ensures that when notifications are sent, they can be directed precisely to the intended devices.

Furthermore, as users uninstall or log out of your application, it's essential to remove their associated tokens to maintain an updated list of active users. This involves implementing a process to handle token deletion or deactivation from database.

Sending push notifications involves retrieving the stored tokens of intended recipients from database and utilizing FCM's functionalities to dispatch messages.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.Firebase | [![NuGet version (LeanCode.Firebase)](https://img.shields.io/nuget/vpre/LeanCode.Firebase.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Firebase/8.0.2260-preview/) | Firebase configuration |
| LeanCode.Firebase.FCM | [![NuGet version (LeanCode.Firebase.FCM)](https://img.shields.io/nuget/vpre/LeanCode.Firebase.FCM.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Firebase.FCM/8.0.2260-preview/) | Push notifications |
| LeanCode.Localization | [![NuGet version (LeanCode.Localization)](https://img.shields.io/nuget/vpre/LeanCode.Localization.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.Localization/8.0.2260-preview/) | Localization |
| Microsoft.EntityFrameworkCore | [![NuGet version (Microsoft.EntityFrameworkCore)](https://img.shields.io/nuget/v/Microsoft.EntityFrameworkCore.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/8.0.0/) | Push notification token store |

### Configuration

The following configuration demonstrates the setup of push notification token storage and the registration of an FCM client to facilitate sending push notifications to users. The localization of notification templates is achieved through the use of `.resx` resource files.

To begin, it is necessary to include the `FirebaseConfiguration` with the Google API key for sending push notifications via the FCM client. Additionally, a string localizer is added to adapt notifications according to resource files. Integration of FCM is accomplished by utilizing the `AddFCM` method, alongside the incorporation of a token store through the `AddTokenStore<T>` method.

```csharp
// `Strings` is a marker class and should reside in the same
// directory as `Strings.resx` containing localized strings.
public class Strings { }
```

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // . . .

    // Add Google API key as `cfg` agrument.
    services.AddSingleton(FirebaseConfiguration.Prepare(
        cfg: "",
        name: Guid.NewGuid().ToString()));

    // Adds string localizer for push notifications.
    services.AddStringLocalizer(
        LocalizationConfiguration.For<Strings>());

    // Adds FCM client with `Guid` as user ID and TokenStore on `CoreDbContext`.
    services.AddFCM<Guid>(fcm => fcm.AddTokenStore<CoreDbContext>());
    // . . .
}
```

> **Tip:** Further details about localization can be found [here](../../features/localization/index.md).

Following the above configuration, it is necessary to incorporate a `DbSet` for the push notification token store. This can be accomplished through `CoreDbContext`, utilizing `Microsoft.EntityFrameworkCore`.

```csharp
public class CoreDbContext : DbContext
{
    public DbSet<PushNotificationTokenEntity<Guid>> PushNotificationTokens
        => Set<PushNotificationTokenEntity<Guid>>();

    // . . .

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // . . .

        builder.ConfigurePushNotificationTokenEntity<Guid>(
            setTokenColumnMaxLength: false);

        // . . .
    }
}

```

### Adding/removing tokens from PushNotificationTokenStore

Managing tokens within the `IPushNotificationTokenStore<T>` involves adding and removing tokens as necessary. The provided code snippets demonstrate the usage of `AddUserTokenAsync` and `RemoveUserTokenAsync` methods from `LeanCode.Firebase.FCM` respectively.

```csharp
public class PushNotificationTokenStore
{
    private readonly IPushNotificationTokenStore<Guid> tokenStore;

    public PushNotificationTokenStore(
        IPushNotificationTokenStore<Guid> tokenStore)
    {
        this.tokenStore = tokenStore;
    }

    public Task AddAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken)
    {
        return tokenStore.AddUserTokenAsync(
            userId,
            token,
            cancellationToken);
    }

    public Task RemoveAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken)
    {
        return tokenStore.RemoveUserTokenAsync(
            userId,
            token,
            cancellationToken);
    }
}
```

### Sending notification

Once the `IPushNotificationTokenStore<T>` is set up and tokens are being managed, the system is ready to dispatch notifications to users. To initiate this process, message templates within the `Strings.resx` file should be defined.

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="notifications.meeting-started.title" xml:space="preserve">
    <value>Meeting has started</value>
  </data>
  <data name="notifications.meeting-started.body" xml:space="preserve">
    <value>{0}</value>
  </data>
</root>

```

To send notifications, the `FCMClient<T>` class is utilized. Following code snippet illustrates sending push notification to specified user with title, body and image.

```csharp
public class PushNotificationSender
{
    private readonly FCMClient<Guid> fcmClient;

    public PushNotificationSender(FCMClient<Guid> fcmClient)
    {
        this.fcmClient = fcmClient;
    }

    public async Task SendMeetingStartedPN(
        Guid userId,
        string content,
        Uri absoluteImageUrl)
    {
        var message = new MulticastMessage
        {
            Notification = fcmClient
                .Localize(Consts.DefaultUserCulture)
                .Title("notifications.meeting-started.title")
                .Body("notifications.meeting-started.body", content)
                .RawImageUrl(absoluteImageUrl)
                .Build(),
            Data = new Dictionary<string, string>
            {
                // Data specific to Flutter, required to open
                // a particular screen upon clicking the notification.
                ["click_action"] = "FLUTTER_NOTIFICATION_CLICK",
                ["type"] = "MeetingHasStarted",
                ["name"] = content,
            },
        };

        // If no token is found for the specified user,
        // no notification will be sent.
        await fcmClient.SendToUserAsync(
            userId,
            message,
            context.RequestAborted);
    }
}
```
