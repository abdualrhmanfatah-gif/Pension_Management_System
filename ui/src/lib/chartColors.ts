/**
 * Chart color palette — single source of truth for all chart colors.
 * Uses HSL values that complement the Tailwind design tokens.
 */

export const chartColors = {
  /** Primary blue — main data series */
  primary: "hsl(215, 75%, 36%)",
  /** Success green — positive values, revenue, credit */
  success: "hsl(142, 71%, 45%)",
  /** Warning amber — liabilities, warnings */
  warning: "hsl(38, 92%, 50%)",
  /** Danger red — expenses, debits, losses */
  danger: "hsl(0, 84%, 60%)",
  /** Purple — secondary series, pie chart accent */
  purple: "hsl(262, 83%, 58%)",
  /** Pink — pie chart accent */
  pink: "hsl(330, 80%, 55%)",
  /** Teal — pie chart accent */
  teal: "hsl(160, 60%, 45%)",
} as const;

/** Array of distinct colors for pie/donut charts with many segments */
export const pieChartColors = [
  chartColors.primary,
  chartColors.success,
  chartColors.warning,
  chartColors.danger,
  chartColors.purple,
  chartColors.pink,
  chartColors.teal,
] as const;
