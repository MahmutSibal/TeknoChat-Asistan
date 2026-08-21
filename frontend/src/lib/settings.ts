const NOTIFICATIONS_KEY = "teknofest_notifications_enabled";

export function getNotificationsEnabled(): boolean {
  return localStorage.getItem(NOTIFICATIONS_KEY) !== "false";
}

export function setNotificationsEnabled(enabled: boolean) {
  localStorage.setItem(NOTIFICATIONS_KEY, String(enabled));
}
