/**
 * Smooth scroll utility functions
 */

/**
 * Smooth scroll to an element with header offset
 * @param elementRef - React ref object or DOM element
 * @param offset - Header height offset in pixels (default: 80)
 */
export const smoothScrollToElement = (
  elementRef: React.RefObject<HTMLElement> | HTMLElement | null,
  offset: number = 80
) => {
  let element: HTMLElement | null = null;
  
  if (elementRef && 'current' in elementRef) {
    element = elementRef.current;
  } else if (elementRef instanceof HTMLElement) {
    element = elementRef;
  }
  
  if (!element) return;

  const elementPosition = element.getBoundingClientRect().top;
  const offsetPosition = elementPosition + window.pageYOffset - offset;

  window.scrollTo({
    top: offsetPosition,
    behavior: 'smooth'
  });
};

/**
 * Smooth scroll to top of page
 */
export const scrollToTop = () => {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  });
};

/**
 * Get header height based on viewport size
 * @returns Header height in pixels
 */
export const getHeaderHeight = (): number => {
  // Adjust header height based on screen size if needed
  if (window.innerWidth < 768) {
    return 60; // Mobile header height
  }
  return 80; // Desktop header height
};
