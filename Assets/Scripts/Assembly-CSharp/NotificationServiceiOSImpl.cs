#if UNITY_IOS
using System;
using System.Collections.Generic;
using Interfaces.Notification;
using Rcs;
using UnityEngine.iOS;

public class NotificationServiceiOSImpl : INotificationService
{
	private string DebugTag = "[NotificationServiceiOSImpl] ";

	private bool m_initialized;
	
	private PushNotifications m_Notifications;
	
	private LocalNotificationPlatformIndependent CopyLocalNotificationBody(LocalNotification notification)
	{
		return new LocalNotificationPlatformIndependent
		{
			alertAction = notification.alertAction,
			alertLaunchImage = notification.alertLaunchImage,
			alertBody = notification.alertBody,
			applicationIconBadgeNumber = notification.applicationIconBadgeNumber,
			fireDate = notification.fireDate,
			hasAction = notification.hasAction,
			repeatCalendar = (CalendarIdentifierPlatformIndependent)notification.repeatCalendar,
			repeatInterval = (CalendarUnitPlatformIndependent)notification.repeatInterval,
			soundName = notification.soundName,
			timeZone = notification.timeZone,
			userInfo = notification.userInfo
		};
	}
	
	private RemoteNotificationPlatformIndependent CopyRemoteNotificationBody(RemoteNotification notification)
	{
		return new RemoteNotificationPlatformIndependent
		{
			userInfo = notification.userInfo,
			alertBody = notification.alertBody,
			applicationIconBadgeNumber = notification.applicationIconBadgeNumber,
			hasAction = notification.hasAction,
			soundName = notification.soundName
		};
	}

	private RemoteNotificationTypePlatformIndependent CopyRemoteNotificationType(NotificationType type)
	{
		return (RemoteNotificationTypePlatformIndependent)type;
	}

	private LocalNotification CopyLocalNotificationBody(LocalNotificationPlatformIndependent notification)
	{
		return new LocalNotification
		{
			alertAction = notification.alertAction,
			alertLaunchImage = notification.alertLaunchImage,
			alertBody = notification.alertBody,
			applicationIconBadgeNumber = notification.applicationIconBadgeNumber,
			fireDate = notification.fireDate,
			hasAction = notification.hasAction,
			repeatCalendar = (CalendarIdentifier)notification.repeatCalendar,
			repeatInterval = (CalendarUnit)notification.repeatInterval,
			soundName = notification.soundName,
			timeZone = notification.timeZone,
			userInfo = notification.userInfo
		};
	}

	private NotificationType CopyRemoteNotificationType(RemoteNotificationTypePlatformIndependent type)
	{
		return (NotificationType)type;
	}

	public void Init()
	{
		if (m_initialized)
			return;
		
		DebugLog.Log("[NotificationServiceiOSImpl] Trying to init push notifications, device token: " + NotificationServices.deviceToken);
		if (NotificationServices.deviceToken == null || NotificationServices.deviceToken.Length == 0)
		{
			DebugLog.Log("[NotificationServiceiOSImpl] Did not get device token. Aborting NotificationService.");
			return;
		}
		
		InitSkynestAndRegisterDevice();
		m_initialized = true;
	}

	private void InitSkynestAndRegisterDevice()
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Init skynest push notifications with device token: " + GetDeviceTokenAsString());
		m_Notifications = new PushNotifications(ContentLoader.Instance.m_BeaconConnectionMgr.Identity, GetDeviceTokenAsString());
		DebugLog.Log("[NotificationServiceiOSImpl] RegisterDevice to skynest.");
		m_Notifications.RegisterDevice(SkynestNotificationService_RegisterSuccess, SkynestNotificationService_RegisterError);
	}

	private void SkynestNotificationService_RegisterSuccess()
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Device registered, initializing; deciveToken: " + NotificationServices.deviceToken);
	}

	private void SkynestNotificationService_RegisterError(int status, string errorMsg)
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Error registering decive: " + status + ", " + errorMsg);
	}

	public void CheckForNotifications()
	{
		var newRemoteNotifications = NotificationServices.remoteNotifications;
		for (int i = 0; i < newRemoteNotifications.Length; i++)
		{
			var notification = newRemoteNotifications[i];
			var infoString = string.Empty;
			foreach (var infoKey in notification.userInfo.Keys)
			{
				infoString += infoKey.ToString() + ", ";
			}
			foreach (var infoValue in notification.userInfo.Values)
			{
				infoString += infoValue.ToString() + ", ";
			}
			
			DebugLog.Log("[NotificationServiceiOSImpl] CheckForNotifications: remoteNotifications: " + infoString, ", " + notification.alertBody);

			var dict = new Dictionary<string, string>();
			dict.Add("Type", "RemoteNotification");
			dict.Add("Description", notification.alertBody);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("NotificationWorked", dict, false);
		}
		
		var newLocalNotifications = NotificationServices.localNotifications;
		for (int i = 0; i < newLocalNotifications.Length; i++)
		{
			var notification = newLocalNotifications[i];
			var infoString = string.Empty;
			foreach (var infoKey in notification.userInfo.Keys)
			{
				infoString += infoKey.ToString() + ", ";
			}
			foreach (var infoValue in notification.userInfo.Values)
			{
				infoString += infoValue.ToString() + ", ";
			}
			
			DebugLog.Log("[NotificationServiceiOSImpl] CheckForNotifications: localNotifications: " + infoString, ", " + notification.alertBody);

			var dict = new Dictionary<string, string>();
			dict.Add("Type", "LocalNotification");
			dict.Add("Description", notification.alertBody);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("NotificationWorked", dict, false);
		}
	}

	public string GetDeviceTokenAsString()
	{
		if (NotificationServices.deviceToken == null)
			return string.Empty;

		return BitConverter.ToString(NotificationServices.deviceToken).Replace("-", string.Empty).ToLower();
	}
	
	public byte[] deviceToken
	{
		get
		{
			return NotificationServices.deviceToken;
		}
	}

	public RemoteNotificationTypePlatformIndependent enabledRemoteNotificationTypes
	{
		get
		{
			return (RemoteNotificationTypePlatformIndependent)NotificationServices.enabledNotificationTypes;
		}
	}
	
	public int localNotificationCount
	{
		get
		{
			return NotificationServices.localNotificationCount;
		}
	}
	
	public LocalNotificationPlatformIndependent[] localNotifications
	{
		get
		{
			var notifs = NotificationServices.localNotifications;
			var newNotifs = new LocalNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < NotificationServices.localNotifications.Length; i++)
			{
				newNotifs[i] = CopyLocalNotificationBody(NotificationServices.localNotifications[i]);
			}

			return newNotifs;
		}
	}
	
	public string registrationError
	{
		get
		{
			return NotificationServices.registrationError;
		}
	}
	
	public int remoteNotificationCount
	{
		get
		{
			return NotificationServices.remoteNotificationCount;
		}
	}
	
	public RemoteNotificationPlatformIndependent[] remoteNotifications
	{
		get
		{
			var notifs = NotificationServices.remoteNotifications;
			var newNotifs = new RemoteNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < NotificationServices.localNotifications.Length; i++) // it's actually like this in vanilla
			{
				newNotifs[i] = CopyRemoteNotificationBody(NotificationServices.remoteNotifications[i]);
			}

			return newNotifs;
		}
	}
	
	public LocalNotificationPlatformIndependent[] scheduledLocalNotifications
	{
		get
		{
			var notifs = NotificationServices.scheduledLocalNotifications;
			var newNotifs = new LocalNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < NotificationServices.scheduledLocalNotifications.Length; i++)
			{
				newNotifs[i] = CopyLocalNotificationBody(NotificationServices.scheduledLocalNotifications[i]);
			}

			return newNotifs;
		}
	}
	
	public void CancelAllLocalNotifications()
	{
		DebugLog.Log(DebugTag + "Cancel All Local Notifications");
		NotificationServices.CancelAllLocalNotifications();
	}
	
	public void CancelLocalNotification(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Cancel Local Notification: " + notification.alertAction);
		NotificationServices.CancelLocalNotification(CopyLocalNotificationBody(notification));
	}
	
	public void ClearLocalNotifications()
	{
		DebugLog.Log(DebugTag + "Clear Local Notifications");
		NotificationServices.ClearLocalNotifications();
	}
	
	public void ClearRemoteNotifications()
	{
		DebugLog.Log(DebugTag + "Clear Remote Notifications");
		NotificationServices.ClearRemoteNotifications();
	}

	public LocalNotificationPlatformIndependent GetLocalNotification(int index)
	{
		return CopyLocalNotificationBody(NotificationServices.GetLocalNotification(index));
	}

	public RemoteNotificationPlatformIndependent GetRemoteNotification(int index)
	{
		return CopyRemoteNotificationBody(NotificationServices.GetRemoteNotification(index));
	}

	public void PresentLocalNotificationNow(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Present Local Notification now! " + notification.alertAction);
		NotificationServices.PresentLocalNotificationNow(CopyLocalNotificationBody(notification));
	}

	public void RegisterForRemoteNotificationTypes(RemoteNotificationTypePlatformIndependent notificationTypes)
	{
		DebugLog.Log(DebugTag + "Register for Remote Notification Types: " + notificationTypes);
		NotificationServices.RegisterForNotifications((NotificationType)notificationTypes);
	}

	public void ScheduleLocalNotification(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Schedule Local Notification: " + notification.alertAction + " to fire in " + DIContainerLogic.GetTimingService().TimeLeftUntil(notification.fireDate).TotalSeconds);
		NotificationServices.ScheduleLocalNotification(CopyLocalNotificationBody(notification));
	}

	public void UnregisterForRemoteNotifications()
	{
		DebugLog.Log("Unregister Device for SkynestNotificationService");
		NotificationServices.UnregisterForRemoteNotifications();
	}

	public string GetDefaultSoundName()
	{
		return LocalNotification.defaultSoundName;
	}
}
#endif