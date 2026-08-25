import { type ElementType, forwardRef } from "react";

import { cn } from "@/lib/utils";

type HeadingLevel = "h1" | "h2" | "h3" | "h4";

interface HeadingProps extends React.HTMLAttributes<HTMLHeadingElement> {
  level?: HeadingLevel;
  as?: ElementType;
}

const headingStyles: Record<HeadingLevel, string> = {
  h1: "text-2xl font-bold tracking-tight",
  h2: "text-xl font-bold tracking-tight",
  h3: "text-lg font-semibold tracking-tight",
  h4: "text-base font-medium tracking-tight",
};

const Heading = forwardRef<HTMLHeadingElement, HeadingProps>(
  ({ level = "h1", as, className, ...props }, ref) => {
    const Component = as || level;
    return (
      <Component
        ref={ref}
        className={cn(headingStyles[level], className)}
        {...props}
      />
    );
  }
);
Heading.displayName = "Heading";

type TextVariant =
  | "body"
  | "body-sm"
  | "small"
  | "description"
  | "caption"
  | "label"
  | "muted";

interface TextProps extends React.HTMLAttributes<HTMLParagraphElement> {
  variant?: TextVariant;
  as?: ElementType;
}

const textStyles: Record<TextVariant, string> = {
  body: "text-sm leading-relaxed",
  "body-sm": "text-xs leading-relaxed",
  small: "text-xs",
  description: "text-sm text-muted-foreground",
  caption: "text-xs text-muted-foreground",
  label: "text-sm font-medium",
  muted: "text-sm text-muted-foreground",
};

const Text = forwardRef<HTMLParagraphElement, TextProps>(
  ({ variant = "body", as, className, ...props }, ref) => {
    const Component = as || "p";
    return (
      <Component
        ref={ref}
        className={cn(textStyles[variant], className)}
        {...props}
      />
    );
  }
);
Text.displayName = "Text";

export { Heading, Text };
export type { HeadingLevel, HeadingProps, TextVariant, TextProps };
