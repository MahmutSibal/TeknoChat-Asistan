import { useEffect } from "react";
import { useLocation } from "react-router-dom";

/// After every in-app navigation, cosmetically resets the visible address bar back to "/" via
/// the raw History API — bypassing React Router's own navigate/pushState calls entirely, so its
/// internal location state (which drives which page renders and NavLink highlighting) is
/// unaffected. Only ever call this inside layouts whose routes don't need to be directly
/// linkable — never on the auth flow (login/register/verify-email/forgot-password/reset-password),
/// since those are opened from real, path-specific links (e.g. an emailed reset-password URL).
///
/// Trade-off: refreshing the page while "on" a hidden route lands back on "/", and those routes
/// can no longer be bookmarked — accepted as the cost of never showing internal paths.
export function useHideUrlBar() {
  const location = useLocation();

  useEffect(() => {
    if (window.location.pathname !== "/" || window.location.search) {
      window.history.replaceState(null, "", "/");
    }
  }, [location]);
}
