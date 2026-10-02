import { Component, type ErrorInfo, type ReactNode } from "react";

import { Button } from "@/components/ui";
import { logger } from "@/lib/logger";

interface Props {
    children: ReactNode;
    fallback?: ReactNode;
}

interface State {
    hasError: boolean;
    error: Error | null;
}

/**
 * ErrorBoundary — يلتقط أخطاء العرض غير المعالجة ويعرض واجهة بديلة
 *
 * Usage:
 *   <ErrorBoundary>
 *     <YourComponent />
 *   </ErrorBoundary>
 */
export class ErrorBoundary extends Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error: Error): State {
        return { hasError: true, error };
    }

    componentDidCatch(error: Error, _errorInfo: ErrorInfo) {
        logger.error("Uncaught error", error, { component: "ErrorBoundary" });
    }

    handleRetry = () => {
        this.setState({ hasError: false, error: null });
    };

    render() {
        if (this.state.hasError) {
            if (this.props.fallback) {
                return this.props.fallback;
            }

            return (
                <div className="flex items-center justify-center min-h-[60dvh]">
                    <div className="text-center space-y-4 w-full max-w-md mx-auto p-8">
                        <div className="w-16 h-16 mx-auto rounded-2xl bg-red-100 dark:bg-red-950/50 flex items-center justify-center">
                            <span className="text-2xl">⚠️</span>
                        </div>
                        <h2 className="text-xl font-bold text-foreground">
                            حدث خطأ غير متوقع
                        </h2>
                        <p className="text-sm text-muted-foreground">
                            نعتذر عن الإزعاج. يرجى تحديث الصفحة أو المحاولة مرة أخرى.
                        </p>
                        {import.meta.env.DEV && this.state.error && (
                            <pre className="mt-4 p-3 rounded-lg bg-muted text-xs text-end overflow-auto max-h-40 border">
                                {this.state.error.message}
                            </pre>
                        )}
                        <div className="flex gap-2 justify-center pt-2">
                            <Button onClick={this.handleRetry}>
                                إعادة المحاولة
                            </Button>
                            <Button variant="outline" onClick={() => window.location.reload()}>
                                تحديث الصفحة
                            </Button>
                        </div>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}
