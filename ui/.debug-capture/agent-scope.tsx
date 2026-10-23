import React from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import api from '../src/api/axios';
import { AgentDialog } from '../src/components/domain/retirement/AgentDialog';
import { useAuthStore } from '../src/stores/authStore';
import '../src/index.css';

const params = new URLSearchParams(location.search);
if (params.has('dark')) document.documentElement.classList.add('dark');
useAuthStore.setState({ isHydrated: true, isAuthenticated: true, permissionsLoaded: true,
    user: { id: 7, fullName: 'مستخدم الفحص', username: 'test', role: 'test' },
    permissions: params.has('noPermission') ? [] : ['EntitledPerson.View', 'Agent.Add', 'Agent.Edit'] });
api.defaults.adapter = async config => {
    if (params.has('error') && config.url?.includes('entitled-persons')) throw new Error('test-load-failure');
    let data: unknown = [];
    if (config.url?.includes('entitled-persons')) data = params.has('empty') ? [] : [
        { entitledID: 11, fullName: 'أحمد محمد علي عبدالله', nationalID: '1234567890', isEligible: true },
        { entitledID: 12, fullName: 'فاطمة محمد أحمد علي', nationalID: '1234567891', isEligible: true },
        { entitledID: 13, fullName: 'محمد عبدالله حسن أحمد', isDeceased: true },
    ];
    if (config.method === 'post' || config.method === 'put') {
        const payload = Object.fromEntries((config.data as FormData).entries());
        sessionStorage.setItem('agent-scope-test-payload', JSON.stringify(payload));
        data = { agentID: 9, ...payload };
    }
    return { data: { isSuccess: true, data }, status: 200, statusText: 'OK', headers: {}, config };
};
const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
const initialData = params.has('edit') ? {
    agentID: 9, fullName: 'وكيل مسجل سابقًا', nationalID: '1234567890',
    agencyNumber: '123', agencyDate: '2026-10-01T00:00:00', agencyScope: 3 as const,
    entitledID: 12, entitledPersonName: 'فاطمة محمد أحمد علي',
} : undefined;
createRoot(document.getElementById('root')!).render(
    <QueryClientProvider client={client}><AgentDialog open fileId={1} fileNumber="F-001"
        mode={initialData ? 'edit' : 'create'} initialData={initialData} onClose={() => {}} /></QueryClientProvider>
);
