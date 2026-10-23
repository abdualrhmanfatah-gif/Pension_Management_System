export type AgencyScope = 1 | 2 | 3;
export type AgentScopeKind = "retiree" | "allEntitled" | "specificEntitled" | "legacy";

interface ScopedAgent {
    agencyScope?: AgencyScope | null;
    entitledID?: number | null;
    entitledPersonName?: string | null;
}

export function agentScopeKind(agent: ScopedAgent): AgentScopeKind {
    if (agent.agencyScope === 1) return "retiree";
    if (agent.agencyScope === 2) return "allEntitled";
    if (agent.agencyScope === 3 || agent.entitledID) return "specificEntitled";
    return "legacy";
}

export function agentScopeLabel(agent: ScopedAgent): string {
    switch (agentScopeKind(agent)) {
        case "retiree": return "عن المتقاعد";
        case "allEntitled": return "عن جميع المستحقين";
        case "specificEntitled": return `عن المستحق: ${agent.entitledPersonName || "مستحق معين"}`;
        default: return "وكالة عامة للملف";
    }
}

export function agentScopePayload(kind: AgentScopeKind, entitledId: string) {
    return {
        agencyScope: kind === "retiree" ? 1 : kind === "allEntitled" ? 2 : kind === "specificEntitled" ? 3 : undefined,
        entitledID: kind === "specificEntitled" ? entitledId : "",
    };
}
