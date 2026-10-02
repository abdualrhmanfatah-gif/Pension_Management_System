/**
 * Chart color palette — single source of truth for all chart colors.
 * مُشتقة من رموز التصميم (تتبع الثيم Light/Dark).
 */

export const chartColors = {
  /** Primary blue — main data series */
  primary: "hsl(var(--primary))",
  /** Success green — positive values, revenue, credit */
  success: "hsl(var(--gov-success))",
  /** Warning amber — liabilities, warnings */
  warning: "hsl(var(--gov-warning))",
  /** Danger red — expenses, debits, losses */
  danger: "hsl(var(--gov-danger))",
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
