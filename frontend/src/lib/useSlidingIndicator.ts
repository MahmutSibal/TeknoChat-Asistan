import { useLayoutEffect, useRef, useState } from "react";

/** Tracks the position/size of a registered nav item along one axis so a caller can render a
 * pill that glides beneath the hovered/active item, like a floating glass nav bar. */
export function useSlidingIndicator(activeKey: string | null, axis: "x" | "y" = "x") {
  const containerRef = useRef<HTMLElement | null>(null);
  const itemRefs = useRef(new Map<string, HTMLElement>());
  const [rect, setRect] = useState<{ offset: number; size: number } | null>(null);

  const measure = (key: string | null) => {
    const container = containerRef.current;
    const el = key ? itemRefs.current.get(key) : null;
    if (!container || !el) {
      setRect(null);
      return;
    }
    const containerBox = container.getBoundingClientRect();
    const itemBox = el.getBoundingClientRect();
    setRect(
      axis === "x"
        ? { offset: itemBox.left - containerBox.left, size: itemBox.width }
        : { offset: itemBox.top - containerBox.top, size: itemBox.height },
    );
  };

  useLayoutEffect(() => {
    measure(activeKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeKey]);

  const registerItem = (key: string) => (el: HTMLElement | null) => {
    if (el) itemRefs.current.set(key, el);
    else itemRefs.current.delete(key);
  };

  return {
    containerRef,
    registerItem,
    rect,
    showAt: (key: string) => measure(key),
    reset: () => measure(activeKey),
  };
}
