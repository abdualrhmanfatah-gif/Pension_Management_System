import { cn } from "@/lib/utils";

type SkeletonProps = React.HTMLAttributes<HTMLDivElement>;

export function Skeleton({ className, ...props }: SkeletonProps) {
    return (
        <div
            className={cn(
                "animate-pulse rounded-md bg-muted",
                className
            )}
            {...props}
        />
    );
}

export function TableSkeleton({ rows = 5, cols = 4 }: { rows?: number; cols?: number }) {
    return (
        <div className="w-full space-y-4">
            <div className="flex items-center justify-between">
                <Skeleton className="h-10 w-[250px]" />
                <Skeleton className="h-10 w-[100px]" />
            </div>
            <div className="rounded-md border">
                <div className="h-12 border-b bg-muted/50 px-4 flex items-center gap-4">
                    {Array.from({ length: cols }).map((_, i) => (
                        <Skeleton key={i} className="h-4 flex-1" />
                    ))}
                </div>
                {Array.from({ length: rows }).map((_, i) => (
                    <div key={i} className="h-16 px-4 flex items-center gap-4 border-b last:border-0">
                        {Array.from({ length: cols }).map((_, j) => (
                            <Skeleton key={j} className="h-4 flex-1" />
                        ))}
                    </div>
                ))}
            </div>
        </div>
    );
}

export function ProfileSkeleton() {
    return (
        <div className="rounded-xl border bg-card overflow-hidden text-center">
            <Skeleton className="h-24 w-full rounded-none" />
            <div className="-mt-12 flex justify-center mb-4">
                <Skeleton className="w-24 h-32 rounded-xl border-4 border-background" />
            </div>
            <div className="px-5 pb-6 space-y-5">
                <div className="space-y-2 flex flex-col items-center">
                    <Skeleton className="h-6 w-32" />
                    <Skeleton className="h-4 w-24" />
                </div>
                <div className="space-y-3 bg-muted/40 p-4 rounded-xl">
                    {Array.from({ length: 6 }).map((_, i) => (
                        <div key={i} className="flex justify-between">
                            <Skeleton className="h-3 w-12" />
                            <Skeleton className="h-3 w-20" />
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export function TabContentSkeleton() {
    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <Skeleton className="h-8 w-48" />
                <Skeleton className="h-10 w-32" />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Skeleton className="h-32 w-full rounded-xl" />
                <Skeleton className="h-32 w-full rounded-xl" />
            </div>
            <TableSkeleton rows={3} />
        </div>
    );
}
