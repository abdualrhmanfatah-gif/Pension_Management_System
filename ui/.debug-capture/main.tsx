import { StrictMode, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { BulkUploadDialog } from '../src/components/domain/retirement/BulkUploadDialog';
import '../src/index.css';

const client = new QueryClient();
client.setQueryData(['file-entities', 1], [{ value: 'retiree', label: 'المتقاعد' }]);
function Harness() {
  const [open, setOpen] = useState(true);
  return <QueryClientProvider client={client}>
    <BulkUploadDialog open={open} onOpenChange={setOpen} fileId={1} docTypes={[{id: 1, name: 'بطاقة شخصية'}]} />
    <Toaster />
  </QueryClientProvider>;
}
createRoot(document.getElementById('root')!).render(<StrictMode><Harness /></StrictMode>);
