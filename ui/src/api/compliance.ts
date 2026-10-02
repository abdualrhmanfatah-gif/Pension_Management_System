import api from "./axios";

export interface ComplianceDashboard {
    totalOpenRiskEvents: number;
    criticalRiskEvents: number;
    highRiskEvents: number;
    pendingOverrides: number;
    recentRiskEvents: any[];
    pendingOverrideReviews: any[];
    branchRiskSummary: any[];
}

export const complianceApi = {
    getDashboard: () =>
        api.get<ComplianceDashboard>("/compliance/dashboard").then(r => r.data),

    verifyChain: () =>
        api.get("/compliance/audit/verify-chain").then(r => r.data),

    resolveRisk: (id: number) =>
        api.post(`/compliance/risk-events/${id}/resolve`, { notes: "تمت المراجعة" }),
};
