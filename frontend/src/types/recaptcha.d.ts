interface RecaptchaRenderConfig {
  sitekey: string;
  callback: (token: string) => void;
  "expired-callback"?: () => void;
}

interface Window {
  grecaptcha?: {
    render: (container: HTMLElement, config: RecaptchaRenderConfig) => number;
    reset: (widgetId?: number) => void;
  };
}
